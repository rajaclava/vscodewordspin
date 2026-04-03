using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Presentation;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Core
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private LevelFlowController levelFlow;
        [SerializeField] private HitEvaluator hitEvaluator;
        [SerializeField] private SessionManager sessionManager;

        private bool _pendingQuestionAdvanceAfterInfoCard;
        private bool _pendingLevelCompleteAfterInfoCard;
        private bool _awaitingFailResolution;
        private bool _usedContinueInCurrentLevel;
        private string _pendingInfoCardId;

        public bool IsAwaitingFailResolution => _awaitingFailResolution;
        public int CurrentLevelId => levelFlow != null && levelFlow.CurrentLevel != null ? levelFlow.CurrentLevel.levelId : 0;

        protected override bool PersistAcrossScenes => false;

        private void Start()
        {
            ResolveSceneReferences();
            EnsureRuntimePresenters();

            if (levelFlow == null || hitEvaluator == null)
            {
                Debug.LogWarning("[GameManager] Missing level flow or hit evaluator in scene.");
                return;
            }

            if (SceneNavigator.Instance != null && SceneNavigator.Instance.TryConsumePendingGameplayRequest(out int pendingLevelId, out bool resumeSavedSession))
            {
                if (resumeSavedSession && CanRestoreActiveSession())
                {
                    levelFlow.RestoreSession(SaveManager.Instance.Data.session);
                    RestoreContinuationStateFromSession();
                    EnterPendingFailResolutionStateIfNeeded();
                    RestorePendingCompletionUi();
                    return;
                }

                sessionManager?.ClearSnapshot();
                if (!StartLevel(ResolvePlayableLevelId(pendingLevelId), false))
                {
                    StartLevel(ResolvePlayableLevelId(1), false);
                }
                return;
            }

            if (CanRestoreActiveSession())
            {
                levelFlow.RestoreSession(SaveManager.Instance.Data.session);
                RestoreContinuationStateFromSession();
                EnterPendingFailResolutionStateIfNeeded();
                RestorePendingCompletionUi();
                return;
            }

            int progressLevelId = SaveManager.Instance != null ? SaveManager.Instance.Data.progress.highestUnlockedLevel : 1;
            StartLevel(ResolvePlayableLevelId(progressLevelId), true);
        }

        private void OnEnable()
        {
            GameEvents.QuestionFailed += HandleQuestionFailed;
            GameEvents.InfoCardClosed += HandleInfoCardClosed;
        }

        private void OnDisable()
        {
            GameEvents.QuestionFailed -= HandleQuestionFailed;
            GameEvents.InfoCardClosed -= HandleInfoCardClosed;
        }

        private void ResolveSceneReferences()
        {
            if (levelFlow == null)
            {
                levelFlow = FindObjectOfType<LevelFlowController>();
            }

            if (hitEvaluator == null)
            {
                hitEvaluator = FindObjectOfType<HitEvaluator>();
            }

            if (sessionManager == null)
            {
                sessionManager = SessionManager.Instance;
            }
        }

        private static void EnsureRuntimePresenters()
        {
            if (Object.FindObjectOfType<FailModalPresenter>() != null)
            {
            }
            else
            {
                Canvas canvas = Object.FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    GameObject failModalHost = new GameObject("RuntimeFailModalPresenter");
                    failModalHost.transform.SetParent(canvas.transform, false);
                    failModalHost.AddComponent<FailModalPresenter>();
                }
            }

            if (Object.FindObjectOfType<RotatorPlaquePresenter>() == null)
            {
                TargetRotator rotator = Object.FindObjectOfType<TargetRotator>();
                if (rotator != null)
                {
                    GameObject plaqueHost = new GameObject("RuntimeRotatorPlaquePresenter");
                    plaqueHost.transform.SetParent(rotator.transform, false);
                    plaqueHost.AddComponent<RotatorPlaquePresenter>();
                }
            }

            if (Object.FindObjectOfType<ImpactFeedbackController>() != null)
            {
                return;
            }

            Camera camera = Object.FindObjectOfType<Camera>();
            if (camera != null)
            {
                GameObject feedbackHost = new GameObject("RuntimeImpactFeedbackController");
                feedbackHost.AddComponent<ImpactFeedbackController>();
            }
        }

        public bool StartLevel(int levelId, bool consumeEntryEnergy)
        {
            if (consumeEntryEnergy && !EnergyManager.Instance.TryConsumeEntryEnergy())
            {
                GameEvents.RaiseMetric("energyBlockedLevelStart", $"{{\"levelId\":{levelId}}}");
                return false;
            }

            bool loaded = levelFlow.LoadLevel(levelId);
            if (!loaded)
            {
                return false;
            }

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.progress.highestUnlockedLevel = Mathf.Max(SaveManager.Instance.Data.progress.highestUnlockedLevel, levelId);
                ClearPendingCompletionState(SaveManager.Instance.Data.session);
                SaveManager.Instance.Save();
            }

            _awaitingFailResolution = false;
            _usedContinueInCurrentLevel = false;
            SetGameplayInputEnabled(true);
            sessionManager?.TakeSnapshot();

            return true;
        }

        public void ResolvePinHit(Pin pin, Slot slot, Vector3 pinTipWorldPoint)
        {
            HitData hit = hitEvaluator.EvaluateImpact(slot, pin.CarryingLetter, pinTipWorldPoint, levelFlow.CurrentDifficultyProfile, levelFlow.CurrentDifficultyTier);
            GameEvents.RaiseHitEvaluated(hit);
            RaiseImpactEvent(hit, pinTipWorldPoint, pin.TravelSpeed);

            switch (hit.resultType)
            {
                case HitResultType.Perfect:
                case HitResultType.Tolerated:
                    levelFlow.RegisterImpactOutcome(hit.resultType);
                    pin.StickTipTo(slot.transform, pinTipWorldPoint);
                    levelFlow.RecordSuccessfulHit(slot, pinTipWorldPoint, pin.transform.localPosition, pin.transform.localEulerAngles.z);
                    GameEvents.RaiseMetric(
                        hit.resultType == HitResultType.Perfect ? "perfectHit" : "toleratedHit",
                        $"{{\"levelId\":{levelFlow.CurrentLevel.levelId},\"slotIndex\":{slot.SlotIndex},\"precision\":{hit.precisionScore.ToString(System.Globalization.CultureInfo.InvariantCulture)}}}");
                    bool wordCompleted = levelFlow.RevealNextLetter();
                    if (wordCompleted)
                    {
                        HandleQuestionCompleted();
                    }
                    break;

                case HitResultType.NearMiss:
                case HitResultType.Miss:
                case HitResultType.WrongLetter:
                case HitResultType.WrongSlot:
                    levelFlow.RegisterImpactOutcome(hit.resultType);
                    RegisterQuestionError(hit);
                    if (hit.resultType == HitResultType.WrongSlot)
                    {
                        pin.PlayWrongSlotBreak(pinTipWorldPoint);
                    }
                    else
                    {
                        Destroy(pin.gameObject);
                    }
                    break;
            }
        }

        public HitData PreviewPinHit(Pin pin, Slot slot, Vector3 pinTipWorldPoint)
        {
            if (pin == null || slot == null || hitEvaluator == null || levelFlow == null)
            {
                return new HitData { resultType = HitResultType.Miss };
            }

            return hitEvaluator.EvaluateImpact(slot, pin.CarryingLetter, pinTipWorldPoint, levelFlow.CurrentDifficultyProfile, levelFlow.CurrentDifficultyTier);
        }

        public void HandlePinFlightMiss(Pin pin)
        {
            if (pin == null || levelFlow == null || levelFlow.CurrentLevel == null)
            {
                return;
            }

            SlotManager slotManager = FindObjectOfType<SlotManager>();

            HitData hit = new HitData
            {
                resultType = HitResultType.Miss,
                enteredLetter = pin.CarryingLetter,
                expectedSlotIndex = slotManager != null ? slotManager.CurrentTargetSlot : -1
            };

            GameEvents.RaiseHitEvaluated(hit);
            RaiseImpactEvent(hit, pin.TipWorldPosition, pin.TravelSpeed);
            levelFlow?.RegisterImpactOutcome(hit.resultType);
            RegisterQuestionError(hit);
            Destroy(pin.gameObject);
        }

        public void HandleWrongLetterInput(char wrongLetter)
        {
            HitData hit = new HitData
            {
                resultType = HitResultType.WrongLetter,
                enteredLetter = wrongLetter
            };

            GameEvents.RaiseHitEvaluated(hit);
            RaiseImpactEvent(hit, Vector3.zero, 0f);
            levelFlow?.RegisterImpactOutcome(hit.resultType);
            RegisterQuestionError(hit);
        }

        private static void RaiseImpactEvent(HitData hit, Vector3 impactWorldPos, float impactSpeed)
        {
            GameEvents.RaiseImpactOccurred(new ImpactEventData
            {
                impactType = hit.resultType,
                impactWorldPos = impactWorldPos,
                impactSpeed = impactSpeed,
                accuracy = Mathf.Clamp01(hit.precisionScore),
                correctTarget = hit.slotIndex >= 0 && hit.slotIndex == hit.expectedSlotIndex,
                correctLetter = char.ToUpperInvariant(hit.enteredLetter) == char.ToUpperInvariant(hit.expectedLetter),
                combo = 0,
                slotIndex = hit.slotIndex,
                expectedSlotIndex = hit.expectedSlotIndex
            });
        }

        private void RegisterQuestionError(HitData hit)
        {
            sessionManager?.TakeSnapshot();
            bool stillAlive = QuestionLifeManager.Instance.ConsumeHeart();
            string payload = $"{{\"type\":\"{hit.resultType}\",\"levelId\":{levelFlow.CurrentLevel.levelId}}}";

            switch (hit.resultType)
            {
                case HitResultType.WrongLetter:
                    GameEvents.RaiseMetric("wrongLetter", payload);
                    break;
                case HitResultType.WrongSlot:
                    GameEvents.RaiseMetric("wrongSlot", payload);
                    break;
                case HitResultType.NearMiss:
                    GameEvents.RaiseMetric("nearMiss", payload);
                    break;
                default:
                    GameEvents.RaiseMetric("questionMistake", payload);
                    break;
            }

            if (!stillAlive)
            {
                GameEvents.RaiseMetric("questionFail", payload);
            }
        }

        private void HandleQuestionCompleted()
        {
            PinLauncher.Instance?.ClearLoadedPin();
            GameEvents.RaiseQuestionCompleted(BuildQuestionContext());
            bool hasInfoCard = levelFlow.CurrentQuestion != null && !string.IsNullOrEmpty(levelFlow.CurrentQuestion.infoCardId);
            bool hasAnotherQuestion = levelFlow.CurrentLevel != null &&
                                      levelFlow.CurrentLevel.questionIds != null &&
                                      levelFlow.CurrentQuestionIndex + 1 < levelFlow.CurrentLevel.questionIds.Length;

            if (hasInfoCard)
            {
                _pendingInfoCardId = levelFlow.CurrentQuestion.infoCardId;
                _pendingQuestionAdvanceAfterInfoCard = hasAnotherQuestion;
                _pendingLevelCompleteAfterInfoCard = !hasAnotherQuestion;
                PersistPendingCompletionState();
                GameEvents.RaiseInfoCardRequested(levelFlow.CurrentQuestion.infoCardId);
                return;
            }

            ContinueAfterQuestionCompletion(hasAnotherQuestion);
        }

        private void ContinueAfterQuestionCompletion(bool hasAnotherQuestion)
        {
            _pendingInfoCardId = string.Empty;
            _pendingQuestionAdvanceAfterInfoCard = false;
            _pendingLevelCompleteAfterInfoCard = false;
            PersistPendingCompletionState();

            if (hasAnotherQuestion && levelFlow.AdvanceQuestion())
            {
                return;
            }

            LevelContext context = BuildLevelContext();

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.progress.lastCompletedLevel = context.levelId;
                SaveManager.Instance.Data.progress.highestUnlockedLevel = Mathf.Max(SaveManager.Instance.Data.progress.highestUnlockedLevel, context.levelId + 1);
                SaveManager.Instance.Save();
            }

            GameEvents.RaiseLevelCompleted(context);
            GameEvents.RaiseMetric("levelComplete", $"{{\"levelId\":{context.levelId}}}");
        }

        private void HandleQuestionFailed()
        {
            PinLauncher.Instance?.ClearLoadedPin();
            EnterFailResolutionState(true);
        }

        private void HandleInfoCardClosed()
        {
            if (_pendingQuestionAdvanceAfterInfoCard)
            {
                ContinueAfterQuestionCompletion(true);
                return;
            }

            if (_pendingLevelCompleteAfterInfoCard)
            {
                ContinueAfterQuestionCompletion(false);
            }
        }

        public void PopulateSessionSnapshot(SessionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            snapshot.pendingInfoCard = !string.IsNullOrEmpty(_pendingInfoCardId);
            snapshot.pendingInfoCardId = _pendingInfoCardId ?? string.Empty;
            snapshot.pendingQuestionAdvanceAfterInfoCard = _pendingQuestionAdvanceAfterInfoCard;
            snapshot.pendingLevelCompleteAfterInfoCard = _pendingLevelCompleteAfterInfoCard;
            snapshot.usedContinueInCurrentLevel = _usedContinueInCurrentLevel;
        }

        private LevelContext BuildLevelContext()
        {
            return new LevelContext
            {
                levelId = levelFlow.CurrentLevel.levelId,
                campaignId = levelFlow.CurrentLevel.campaignId,
                themeId = levelFlow.CurrentLevel.themeId,
                difficultyProfileId = levelFlow.CurrentLevel.difficultyProfileId,
                rhythmProfileId = levelFlow.CurrentRhythmProfile != null ? levelFlow.CurrentRhythmProfile.rhythmProfileId : levelFlow.CurrentLevel.difficultyProfileId,
                languageCode = levelFlow.LanguageCode,
                questionIndex = levelFlow.CurrentQuestionIndex,
                totalQuestions = levelFlow.CurrentLevel.questionIds.Length,
                obstacleBudget = levelFlow.CurrentDifficultyProfile != null ? levelFlow.CurrentDifficultyProfile.obstacleBudget : 0,
                dopamineSpike = levelFlow.CurrentDifficultyProfile != null && levelFlow.CurrentDifficultyProfile.dopamineSpike,
                breathLevel = levelFlow.CurrentDifficultyProfile != null && levelFlow.CurrentDifficultyProfile.breathLevel
            };
        }

        private QuestionContext BuildQuestionContext()
        {
            return new QuestionContext
            {
                questionId = levelFlow.CurrentQuestion != null ? levelFlow.CurrentQuestion.questionId : string.Empty,
                levelId = levelFlow.CurrentLevel != null ? levelFlow.CurrentLevel.levelId : 0,
                questionIndex = levelFlow.CurrentQuestionIndex,
                languageCode = levelFlow.LanguageCode,
                totalLetters = levelFlow.CurrentQuestion != null ? levelFlow.CurrentQuestion.Letters(levelFlow.LanguageCode).Length : 0
            };
        }

        public bool ContinueAfterFailure(bool usedPremiumContinue)
        {
            if (!_awaitingFailResolution)
            {
                return false;
            }

            _awaitingFailResolution = false;
            _usedContinueInCurrentLevel = true;
            QuestionLifeManager.Instance?.Restore(1);
            levelFlow?.RefreshCurrentTarget();
            SetPendingFailResolutionFlag(false, 1);
            sessionManager?.TakeSnapshot();
            SetGameplayInputEnabled(true);

            string payload = $"{{\"levelId\":{CurrentLevelId},\"questionIndex\":{(levelFlow != null ? levelFlow.CurrentQuestionIndex : 0)},\"premium\":{usedPremiumContinue.ToString().ToLowerInvariant()}}}";
            GameEvents.RaiseLevelContinueUsed(usedPremiumContinue);
            GameEvents.RaiseMetric("continueLevel", payload);
            GameEvents.RaiseMetric(usedPremiumContinue ? "continueLevelPremium" : "continueLevelRewardedAd", payload);
            return true;
        }

        public bool RetryCurrentLevel()
        {
            if (!_awaitingFailResolution || levelFlow == null || levelFlow.CurrentLevel == null || SceneNavigator.Instance == null)
            {
                return false;
            }

            int levelId = levelFlow.CurrentLevel.levelId;
            string payload = $"{{\"levelId\":{levelId},\"questionIndex\":{levelFlow.CurrentQuestionIndex}}}";
            if (!SceneNavigator.Instance.OpenGameplayLevel(levelId, true))
            {
                return false;
            }

            _awaitingFailResolution = false;
            SetGameplayInputEnabled(true);
            sessionManager?.ClearSnapshot();
            GameEvents.RaiseMetric("retryLevel", payload);
            return true;
        }

        private void EnterPendingFailResolutionStateIfNeeded()
        {
            if (SaveManager.Instance == null || !SaveManager.Instance.Data.session.pendingFailResolution)
            {
                _awaitingFailResolution = false;
                SetGameplayInputEnabled(true);
                return;
            }

            EnterFailResolutionState(false);
        }

        private void EnterFailResolutionState(bool persistPendingFlag)
        {
            _awaitingFailResolution = true;
            SetGameplayInputEnabled(false);

            if (persistPendingFlag)
            {
                SetPendingFailResolutionFlag(true, 0);
            }

            GameEvents.RaiseFailModalRequested(new FailModalContext
            {
                levelId = CurrentLevelId,
                questionIndex = levelFlow != null ? levelFlow.CurrentQuestionIndex : 0,
                restoreHeartsOnContinue = 1,
                premiumContinueAvailable = EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive
            });
        }

        private static void SetGameplayInputEnabled(bool enabled)
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.enabled = enabled;
            }
        }

        private void SetPendingFailResolutionFlag(bool pending, int hearts)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.session.hasActiveSession = true;
            SaveManager.Instance.Data.session.pendingFailResolution = pending;
            SaveManager.Instance.Data.session.questionHeartsRemaining = hearts;
            SaveManager.Instance.Save();
        }

        private void RestorePendingCompletionUi()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SessionSnapshot snapshot = SaveManager.Instance.Data.session;
            _pendingInfoCardId = snapshot.pendingInfoCardId ?? string.Empty;
            _pendingQuestionAdvanceAfterInfoCard = snapshot.pendingQuestionAdvanceAfterInfoCard;
            _pendingLevelCompleteAfterInfoCard = snapshot.pendingLevelCompleteAfterInfoCard;

            if (snapshot.pendingInfoCard && !string.IsNullOrEmpty(_pendingInfoCardId))
            {
                GameEvents.RaiseInfoCardRequested(_pendingInfoCardId);
                return;
            }

            ResultPresenter resultPresenter = FindObjectOfType<ResultPresenter>();
            resultPresenter?.RestorePendingResultFromSave();
        }

        private void PersistPendingCompletionState()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            PopulateSessionSnapshot(SaveManager.Instance.Data.session);
            SaveManager.Instance.Save();
        }

        private static void ClearPendingCompletionState(SessionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            snapshot.pendingInfoCard = false;
            snapshot.pendingInfoCardId = string.Empty;
            snapshot.pendingQuestionAdvanceAfterInfoCard = false;
            snapshot.pendingLevelCompleteAfterInfoCard = false;
            snapshot.pendingLevelResult = false;
            snapshot.pendingResultLevelId = 0;
            snapshot.pendingResultTotalScore = 0;
            snapshot.pendingResultHitScore = 0;
            snapshot.pendingResultClearScore = 0;
            snapshot.pendingResultBestMultiplier = 0f;
            snapshot.pendingResultStars = 0;
            snapshot.pendingResultCoinReward = 0;
            snapshot.pendingResultAdBonusCoins = 0;
            snapshot.pendingResultAdBonusEligible = false;
            snapshot.usedContinueInCurrentLevel = false;
        }

        private void RestoreContinuationStateFromSession()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            _usedContinueInCurrentLevel = SaveManager.Instance.Data.session.usedContinueInCurrentLevel;
            if (_usedContinueInCurrentLevel)
            {
                GameEvents.RaiseLevelContinueUsed(false);
            }
        }

        private static bool CanRestoreActiveSession()
        {
            if (SaveManager.Instance == null || !SaveManager.Instance.Data.session.hasActiveSession)
            {
                return false;
            }

            SessionSnapshot session = SaveManager.Instance.Data.session;
            if (session.levelId <= 0)
            {
                return false;
            }

            string currentLanguage = GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode);
            string sessionLanguage = GameConstants.NormalizeLanguageCode(session.languageCode);
            return string.Equals(currentLanguage, sessionLanguage, System.StringComparison.OrdinalIgnoreCase);
        }

        private static int ResolvePlayableLevelId(int requestedLevelId)
        {
            int normalizedRequestedLevelId = Mathf.Max(1, requestedLevelId);
            LevelCatalog levelCatalog = ContentService.Instance?.LoadLevels();
            LevelDefinition[] levels = levelCatalog != null ? levelCatalog.levels : null;
            if (levels == null || levels.Length == 0)
            {
                return normalizedRequestedLevelId;
            }

            int smallestLevelId = int.MaxValue;
            int bestLevelId = 0;
            for (int i = 0; i < levels.Length; i++)
            {
                LevelDefinition level = levels[i];
                if (level == null || level.levelId <= 0)
                {
                    continue;
                }

                if (level.levelId < smallestLevelId)
                {
                    smallestLevelId = level.levelId;
                }

                if (level.levelId <= normalizedRequestedLevelId && level.levelId > bestLevelId)
                {
                    bestLevelId = level.levelId;
                }
            }

            if (bestLevelId > 0)
            {
                return bestLevelId;
            }

            return smallestLevelId != int.MaxValue ? smallestLevelId : normalizedRequestedLevelId;
        }
    }
}
