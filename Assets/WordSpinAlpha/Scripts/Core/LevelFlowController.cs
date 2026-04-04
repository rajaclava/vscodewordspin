using System;
using System.Collections.Generic;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Presentation;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Core
{
    public class LevelFlowController : MonoBehaviour
    {
        [SerializeField] private SlotManager slotManager;
        [SerializeField] private TargetRotator targetRotator;
        [SerializeField] private InputBuffer inputBuffer;

        private readonly Dictionary<int, LevelDefinition> _levelsById = new Dictionary<int, LevelDefinition>();
        private readonly Dictionary<string, QuestionDefinition> _questionsById = new Dictionary<string, QuestionDefinition>();
        private readonly Dictionary<string, DifficultyProfileDefinition> _difficultyById = new Dictionary<string, DifficultyProfileDefinition>();
        private readonly Dictionary<string, DifficultyTierDefinition> _difficultyTierById = new Dictionary<string, DifficultyTierDefinition>();
        private readonly Dictionary<string, RhythmProfileDefinition> _rhythmById = new Dictionary<string, RhythmProfileDefinition>();
        private readonly Dictionary<string, ShapeLayoutDefinition> _shapeLayoutById = new Dictionary<string, ShapeLayoutDefinition>();
        private readonly List<int> _revealedSlotHistory = new List<int>();
        private readonly List<Vector2> _revealedTipLocalHistory = new List<Vector2>();
        private readonly List<Vector2> _revealedPinLocalPositionHistory = new List<Vector2>();
        private readonly List<float> _revealedPinLocalRotationHistory = new List<float>();
        private readonly List<int> _revealOrder = new List<int>();
        private readonly List<int> _slotSequence = new List<int>();
        private readonly Dictionary<int, int> _answerIndexToSlotIndex = new Dictionary<int, int>();
        private int _currentTargetSlotIndex = -1;
        private HitResultType _lastImpactType = HitResultType.Tolerated;
        private bool _isPlayerArmed;
        private float _perfectChainWindowUntil;
        private int _perfectMomentumLevel;
        private float _persistentFlowSpeedMultiplier = 1f;

        public LevelDefinition CurrentLevel { get; private set; }
        public QuestionDefinition CurrentQuestion { get; private set; }
        public DifficultyProfileDefinition CurrentDifficultyProfile { get; private set; }
        public DifficultyTierDefinition CurrentDifficultyTier { get; private set; }
        public RhythmProfileDefinition CurrentRhythmProfile { get; private set; }
        public ShapeLayoutDefinition CurrentShapeLayout { get; private set; }
        public int CurrentQuestionIndex { get; private set; }
        public int CurrentLetterIndex { get; private set; }
        public int CurrentTargetSlotIndex => _currentTargetSlotIndex;
        public IReadOnlyList<int> RevealedSlotHistory => _revealedSlotHistory;
        public IReadOnlyList<Vector2> RevealedTipLocalHistory => _revealedTipLocalHistory;
        public IReadOnlyList<Vector2> RevealedPinLocalPositionHistory => _revealedPinLocalPositionHistory;
        public IReadOnlyList<float> RevealedPinLocalRotationHistory => _revealedPinLocalRotationHistory;
        public string LanguageCode => SaveManager.Instance != null ? SaveManager.Instance.Data.languageCode : Lang.TR;
        public bool CurrentLevelUsesRandomSlots => CurrentLevel != null && (CurrentLevel.randomSlots || (CurrentDifficultyProfile != null && CurrentDifficultyProfile.enableRandomSlots));

        private void Awake()
        {
            BuildCaches();
        }

        private void OnEnable()
        {
            GameEvents.PinLoaded += HandlePinLoaded;
            GameEvents.PinReleased += HandlePinReleased;
        }

        private void OnDisable()
        {
            GameEvents.PinLoaded -= HandlePinLoaded;
            GameEvents.PinReleased -= HandlePinReleased;
        }

        private void BuildCaches()
        {
            if (ContentService.Instance == null)
            {
                return;
            }

            _levelsById.Clear();
            _questionsById.Clear();
            _difficultyById.Clear();
            _difficultyTierById.Clear();
            _rhythmById.Clear();
            _shapeLayoutById.Clear();

            foreach (LevelDefinition level in ContentService.Instance.LoadLevels().levels ?? Array.Empty<LevelDefinition>())
            {
                _levelsById[level.levelId] = level;
            }

            foreach (QuestionDefinition question in ContentService.Instance.LoadQuestions().questions ?? Array.Empty<QuestionDefinition>())
            {
                _questionsById[question.questionId] = question;
            }

            foreach (DifficultyProfileDefinition profile in ContentService.Instance.LoadDifficultyProfiles().profiles ?? Array.Empty<DifficultyProfileDefinition>())
            {
                _difficultyById[profile.difficultyProfileId] = profile;
            }

            foreach (DifficultyTierDefinition tier in ContentService.Instance.LoadDifficultyTiers().tiers ?? Array.Empty<DifficultyTierDefinition>())
            {
                _difficultyTierById[tier.difficultyTierId] = tier;
            }

            foreach (RhythmProfileDefinition profile in ContentService.Instance.LoadRhythmProfiles().profiles ?? Array.Empty<RhythmProfileDefinition>())
            {
                _rhythmById[profile.rhythmProfileId] = profile;
            }

            foreach (ShapeLayoutDefinition layout in ContentService.Instance.LoadShapeLayouts().layouts ?? Array.Empty<ShapeLayoutDefinition>())
            {
                _shapeLayoutById[layout.shapeLayoutId] = layout;
            }
        }

        public void RefreshContentCaches()
        {
            BuildCaches();
        }

        public bool LoadLevel(int levelId)
        {
            if (_levelsById.Count == 0 || _questionsById.Count == 0 || _difficultyById.Count == 0 || _difficultyTierById.Count == 0 || _rhythmById.Count == 0 || _shapeLayoutById.Count == 0)
            {
                BuildCaches();
            }

            if (!_levelsById.TryGetValue(levelId, out LevelDefinition level))
            {
                Debug.LogWarning($"[LevelFlowController] Level not found: {levelId}");
                return false;
            }

            CurrentLevel = level;
            CurrentQuestionIndex = 0;
            CurrentLetterIndex = 0;
            CurrentDifficultyProfile = _difficultyById.TryGetValue(level.difficultyProfileId, out DifficultyProfileDefinition profile)
                ? profile
                : null;
            string difficultyTierId = !string.IsNullOrWhiteSpace(level.difficultyTierId) ? level.difficultyTierId : ResolveDefaultDifficultyTierId(level.levelId);
            CurrentDifficultyTier = _difficultyTierById.TryGetValue(difficultyTierId, out DifficultyTierDefinition tier)
                ? tier
                : null;
            string rhythmProfileId = !string.IsNullOrWhiteSpace(level.rhythmProfileId) ? level.rhythmProfileId : level.difficultyProfileId;
            CurrentRhythmProfile = _rhythmById.TryGetValue(rhythmProfileId, out RhythmProfileDefinition rhythmProfile)
                ? rhythmProfile
                : null;
            string shapeLayoutId = !string.IsNullOrWhiteSpace(level.shapeLayoutId) ? level.shapeLayoutId : ResolveDefaultShapeLayoutId(level);
            CurrentShapeLayout = _shapeLayoutById.TryGetValue(shapeLayoutId, out ShapeLayoutDefinition shapeLayout)
                ? shapeLayout
                : null;

            float speed = level.rotationSpeed * (CurrentDifficultyProfile != null ? CurrentDifficultyProfile.rotationSpeedMultiplier : 1f);
            if (CurrentDifficultyTier != null)
            {
                speed *= Mathf.Clamp(CurrentDifficultyTier.rotationSpeedScale, 0.7f, 1.5f);
            }

            targetRotator.ApplyLevelSettings(speed, level.clockwise);
            QuestionLifeManager.Instance?.ResetQuestionHearts();
            return LoadCurrentQuestion();
        }

        public bool LoadCurrentQuestion()
        {
            if (CurrentLevel == null || CurrentLevel.questionIds == null || CurrentQuestionIndex >= CurrentLevel.questionIds.Length)
            {
                return false;
            }

            if (!_questionsById.TryGetValue(CurrentLevel.questionIds[CurrentQuestionIndex], out QuestionDefinition question))
            {
                return false;
            }

            CurrentQuestion = question;
            CurrentLetterIndex = 0;
            _currentTargetSlotIndex = -1;
            _lastImpactType = HitResultType.Tolerated;
            _isPlayerArmed = false;
            _perfectChainWindowUntil = 0f;
            _perfectMomentumLevel = 0;
            _persistentFlowSpeedMultiplier = 1f;
            _revealedSlotHistory.Clear();
            _revealedTipLocalHistory.Clear();
            _revealedPinLocalPositionHistory.Clear();
            _revealedPinLocalRotationHistory.Clear();

            char[] letters = question.Letters(LanguageCode);
            if (CurrentShapeLayout != null && slotManager != null)
            {
                slotManager.ApplyShapeLayout(CurrentShapeLayout);
                RotatorPlaquePresenter presenter = FindObjectOfType<RotatorPlaquePresenter>();
                if (presenter != null)
                {
                    presenter.RebuildLayout();
                }
            }

            BuildTargetLayout(letters.Length);
            slotManager.ConfigureSlots(BuildPlaqueLetters(letters));

            LevelContext levelContext = new LevelContext
            {
                levelId = CurrentLevel.levelId,
                campaignId = CurrentLevel.campaignId,
                themeId = CurrentLevel.themeId,
                difficultyProfileId = CurrentLevel.difficultyProfileId,
                rhythmProfileId = CurrentRhythmProfile != null ? CurrentRhythmProfile.rhythmProfileId : CurrentLevel.difficultyProfileId,
                languageCode = LanguageCode,
                questionIndex = CurrentQuestionIndex,
                totalQuestions = CurrentLevel.questionIds.Length,
                obstacleBudget = CurrentDifficultyProfile != null ? CurrentDifficultyProfile.obstacleBudget : 0,
                dopamineSpike = CurrentDifficultyProfile != null && CurrentDifficultyProfile.dopamineSpike,
                breathLevel = CurrentDifficultyProfile != null && CurrentDifficultyProfile.breathLevel
            };

            QuestionContext questionContext = new QuestionContext
            {
                questionId = question.questionId,
                levelId = CurrentLevel.levelId,
                questionIndex = CurrentQuestionIndex,
                languageCode = LanguageCode,
                totalLetters = letters.Length
            };

            GameEvents.RaiseLevelStarted(levelContext);
            GameEvents.RaiseQuestionStarted(questionContext, question.GetAnswer(LanguageCode));
            RefreshCurrentTarget();
            BroadcastRhythmFlowState(0f);
            GameEvents.RaiseMetric(
                "levelStart",
                $"{{\"levelId\":{CurrentLevel.levelId},\"themeId\":\"{CurrentLevel.themeId}\",\"difficulty\":\"{CurrentLevel.difficultyProfileId}\",\"obstacleBudget\":{levelContext.obstacleBudget},\"dopamineSpike\":{levelContext.dopamineSpike.ToString().ToLowerInvariant()},\"breathLevel\":{levelContext.breathLevel.ToString().ToLowerInvariant()}}}");
            return true;
        }

        public bool RevealNextLetter()
        {
            if (CurrentQuestion == null)
            {
                return false;
            }

            char[] letters = CurrentQuestion.Letters(LanguageCode);
            if (CurrentLetterIndex >= letters.Length)
            {
                return false;
            }

            int answerIndex = GetCurrentAnswerIndex();
            char revealed = letters[answerIndex];
            GameEvents.RaiseLetterRevealed(answerIndex, revealed);
            if (_currentTargetSlotIndex >= 0)
            {
                _revealedSlotHistory.Add(_currentTargetSlotIndex);
            }
            CurrentLetterIndex++;

            if (CurrentLetterIndex >= letters.Length)
            {
                return true;
            }

            _currentTargetSlotIndex = -1;
            RefreshCurrentTarget();
            return false;
        }

        public bool AdvanceQuestion()
        {
            CurrentQuestionIndex++;
            if (CurrentLevel == null || CurrentQuestionIndex >= CurrentLevel.questionIds.Length)
            {
                return false;
            }

            QuestionLifeManager.Instance?.ResetQuestionHearts();
            return LoadCurrentQuestion();
        }

        public void RestartCurrentQuestion()
        {
            QuestionLifeManager.Instance?.ResetQuestionHearts();
            LoadCurrentQuestion();
        }

        public void RecordSuccessfulHit(Slot slot, Vector3 pinTipWorldPoint, Vector3 pinLocalPosition, float pinLocalRotationZ)
        {
            if (slot == null)
            {
                return;
            }

            _revealedTipLocalHistory.Add(slot.GetPlaqueLocalPoint(pinTipWorldPoint));
            _revealedPinLocalPositionHistory.Add(new Vector2(pinLocalPosition.x, pinLocalPosition.y));
            _revealedPinLocalRotationHistory.Add(pinLocalRotationZ);
        }

        public void RestoreSession(SessionSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.hasActiveSession)
            {
                return;
            }

            if (!LoadLevel(snapshot.levelId))
            {
                return;
            }

            CurrentQuestionIndex = Mathf.Clamp(snapshot.questionIndex, 0, CurrentLevel.questionIds.Length - 1);
            LoadCurrentQuestion();
            QuestionLifeManager.Instance?.Restore(snapshot.questionHeartsRemaining);

            char[] letters = CurrentQuestion.Letters(LanguageCode);
            _revealedSlotHistory.Clear();
            if (snapshot.revealedSlotIndices != null && snapshot.revealedSlotIndices.Count > 0)
            {
                _revealedSlotHistory.AddRange(snapshot.revealedSlotIndices);
            }

            _revealedTipLocalHistory.Clear();
            if (snapshot.revealedTipLocalPoints != null && snapshot.revealedTipLocalPoints.Count > 0)
            {
                _revealedTipLocalHistory.AddRange(snapshot.revealedTipLocalPoints);
            }

            _revealedPinLocalPositionHistory.Clear();
            if (snapshot.revealedPinLocalPositions != null && snapshot.revealedPinLocalPositions.Count > 0)
            {
                _revealedPinLocalPositionHistory.AddRange(snapshot.revealedPinLocalPositions);
            }

            _revealedPinLocalRotationHistory.Clear();
            if (snapshot.revealedPinLocalRotations != null && snapshot.revealedPinLocalRotations.Count > 0)
            {
                _revealedPinLocalRotationHistory.AddRange(snapshot.revealedPinLocalRotations);
            }

            for (int i = 0; i < Mathf.Clamp(snapshot.revealedLetters, 0, letters.Length); i++)
            {
                int restoredAnswerIndex = i < _revealOrder.Count ? _revealOrder[i] : i;
                GameEvents.RaiseLetterRevealed(restoredAnswerIndex, letters[restoredAnswerIndex]);
                int revealedSlotIndex = i < _revealedSlotHistory.Count ? _revealedSlotHistory[i] : i;
                if (slotManager.TryGetSlot(revealedSlotIndex, out Slot revealedSlot))
                {
                    Vector2 restoredLocalPosition = i < _revealedPinLocalPositionHistory.Count
                        ? _revealedPinLocalPositionHistory[i]
                        : (Vector2)revealedSlot.transform.InverseTransformPoint(revealedSlot.GetDefaultPinnedTipWorldPoint());
                    float restoredLocalRotation = i < _revealedPinLocalRotationHistory.Count
                        ? _revealedPinLocalRotationHistory[i]
                        : 0f;
                    PinLauncher.Instance?.RestorePinnedPin(letters[restoredAnswerIndex], revealedSlot, restoredLocalPosition, restoredLocalRotation);
                }
            }

            CurrentLetterIndex = Mathf.Clamp(snapshot.revealedLetters, 0, letters.Length);
            if (CurrentLetterIndex < letters.Length)
            {
                int nextAnswerIndex = GetCurrentAnswerIndex();
                _currentTargetSlotIndex = snapshot.currentTargetSlotIndex >= 0
                    ? snapshot.currentTargetSlotIndex
                    : ResolveTargetSlotIndex(nextAnswerIndex);
                RefreshCurrentTarget();
            }
        }

        public void RefreshCurrentTarget()
        {
            if (CurrentQuestion == null || slotManager == null || inputBuffer == null)
            {
                return;
            }

            char[] letters = CurrentQuestion.Letters(LanguageCode);
            if (letters == null || letters.Length == 0 || CurrentLetterIndex < 0 || CurrentLetterIndex >= letters.Length)
            {
                return;
            }

            int answerIndex = GetCurrentAnswerIndex();
            int slotIndex = ResolveTargetSlotIndex(answerIndex);
            _currentTargetSlotIndex = slotIndex;
            char targetLetter = letters[answerIndex];
            slotManager.SetTargetSlot(slotIndex, targetLetter);
            inputBuffer.SetExpectedLetter(targetLetter);
            ApplyRhythmPacing(slotIndex);
            GameEvents.RaiseTargetSlotUpdated(slotIndex, answerIndex, targetLetter);
        }

        public void RegisterImpactOutcome(HitResultType resultType)
        {
            _lastImpactType = resultType;
            if (resultType == HitResultType.Perfect)
            {
                _perfectMomentumLevel = Mathf.Max(1, _perfectMomentumLevel);
                _persistentFlowSpeedMultiplier = Mathf.Max(_persistentFlowSpeedMultiplier, _perfectMomentumLevel >= 2 ? 1.62f : 1.42f);
                float chainWindow = CurrentRhythmProfile != null ? Mathf.Max(0.35f, CurrentRhythmProfile.targetWindowLeadTime + 0.24f) : 0.42f;
                _perfectChainWindowUntil = Time.time + chainWindow + 0.55f;
            }
            else if (resultType == HitResultType.Tolerated)
            {
                _perfectMomentumLevel = 0;
                _perfectChainWindowUntil = 0f;
                _persistentFlowSpeedMultiplier = 1f;
                targetRotator?.ClearRhythmAssist();
            }
            else
            {
                _perfectMomentumLevel = 0;
                _perfectChainWindowUntil = 0f;
                _persistentFlowSpeedMultiplier = 1f;
                targetRotator?.ClearRhythmAssist();
            }

            BroadcastRhythmFlowState(CurrentRhythmProfile != null ? Mathf.Clamp01(CurrentRhythmProfile.easyFlowAssist) : 0.5f);
        }

        private int ResolveTargetSlotIndex(int answerIndex)
        {
            if (_answerIndexToSlotIndex.TryGetValue(answerIndex, out int slotIndex))
            {
                return slotIndex;
            }

            return Mathf.Clamp(answerIndex, 0, Mathf.Max(0, slotManager.SlotCount - 1));
        }

        private int GetCurrentAnswerIndex()
        {
            if (_revealOrder.Count == 0)
            {
                return CurrentLetterIndex;
            }

            int index = Mathf.Clamp(CurrentLetterIndex, 0, _revealOrder.Count - 1);
            return _revealOrder[index];
        }

        private void BuildTargetLayout(int letterCount)
        {
            _revealOrder.Clear();
            _slotSequence.Clear();
            _answerIndexToSlotIndex.Clear();

            for (int i = 0; i < letterCount; i++)
            {
                _revealOrder.Add(i);
            }

            bool profileAllowsRandom = CurrentDifficultyProfile != null && CurrentDifficultyProfile.enableRandomSlots;
            bool lengthAllowsRandom = CurrentDifficultyProfile == null || CurrentDifficultyProfile.maxQuestionLength <= 0 || letterCount <= CurrentDifficultyProfile.maxQuestionLength;
            bool useRandomReveal = (CurrentLevel.randomSlots || profileAllowsRandom) && lengthAllowsRandom;

            int seed = BuildQuestionSeed();
            if (useRandomReveal)
            {
                if (_revealOrder.Count > 1)
                {
                    // Alpha behavior: keep the first letter as the first target, randomize the rest.
                    List<int> tail = _revealOrder.GetRange(1, _revealOrder.Count - 1);
                    ShuffleList(tail, seed);
                    _revealOrder.Clear();
                    _revealOrder.Add(0);
                    _revealOrder.AddRange(tail);
                }
            }

            int availableSlotCount = Mathf.Max(letterCount, slotManager != null ? slotManager.SlotCount : letterCount);
            _slotSequence.AddRange(BuildFlowSlotSequence(letterCount, availableSlotCount, seed + 17));
            for (int i = 0; i < Mathf.Min(_revealOrder.Count, _slotSequence.Count); i++)
            {
                _answerIndexToSlotIndex[_revealOrder[i]] = _slotSequence[i];
            }
        }

        private char[] BuildPlaqueLetters(char[] answerLetters)
        {
            char[] plaqueLetters = new char[Mathf.Max(answerLetters.Length, slotManager != null ? slotManager.SlotCount : answerLetters.Length)];
            for (int i = 0; i < plaqueLetters.Length; i++)
            {
                plaqueLetters[i] = '\0';
            }

            for (int answerIndex = 0; answerIndex < answerLetters.Length; answerIndex++)
            {
                int slotIndex = ResolveTargetSlotIndex(answerIndex);
                if (slotIndex >= 0 && slotIndex < plaqueLetters.Length)
                {
                    plaqueLetters[slotIndex] = answerLetters[answerIndex];
                }
            }

            return plaqueLetters;
        }

        private IEnumerable<int> BuildFlowSlotSequence(int revealCount, int availableSlotCount, int seed)
        {
            List<int> remaining = BuildDistributedSlotSubset(revealCount, availableSlotCount);
            if (remaining.Count == 0)
            {
                return Array.Empty<int>();
            }

            List<int> ordered = new List<int>(remaining.Count);
            System.Random random = new System.Random(seed);
            int current = remaining[random.Next(0, remaining.Count)];

            while (remaining.Count > 0)
            {
                if (!remaining.Contains(current))
                {
                    current = remaining[0];
                }

                ordered.Add(current);
                remaining.Remove(current);

                if (remaining.Count == 0)
                {
                    break;
                }

                int preferredJump = random.NextDouble() < 0.55 ? 2 : 3;
                int bestIndex = 0;
                int bestScore = int.MaxValue;
                for (int i = 0; i < remaining.Count; i++)
                {
                    int candidate = remaining[i];
                    int distance = CircularDistance(current, candidate, availableSlotCount);
                    int score = Mathf.Abs(distance - preferredJump);
                    if (score < bestScore)
                    {
                        bestScore = score;
                        bestIndex = i;
                    }
                }

                current = remaining[bestIndex];
            }

            return ordered;
        }

        private static List<int> BuildDistributedSlotSubset(int revealCount, int availableSlotCount)
        {
            int desired = Mathf.Clamp(revealCount, 0, Mathf.Max(0, availableSlotCount));
            List<int> result = new List<int>(desired);
            if (desired <= 0 || availableSlotCount <= 0)
            {
                return result;
            }

            if (desired >= availableSlotCount)
            {
                for (int i = 0; i < availableSlotCount; i++)
                {
                    result.Add(i);
                }

                return result;
            }

            float step = availableSlotCount / (float)desired;
            for (int i = 0; i < desired; i++)
            {
                int candidate = Mathf.RoundToInt((i * step) + (step * 0.5f)) % availableSlotCount;
                if (!TryAddUniqueSlot(candidate, availableSlotCount, result))
                {
                    break;
                }
            }

            result.Sort();
            return result;
        }

        private static bool TryAddUniqueSlot(int candidate, int availableSlotCount, List<int> result)
        {
            if (availableSlotCount <= 0)
            {
                return false;
            }

            candidate = Mathf.Clamp(candidate, 0, availableSlotCount - 1);
            if (!result.Contains(candidate))
            {
                result.Add(candidate);
                return true;
            }

            for (int offset = 1; offset < availableSlotCount; offset++)
            {
                int right = (candidate + offset) % availableSlotCount;
                if (!result.Contains(right))
                {
                    result.Add(right);
                    return true;
                }

                int left = candidate - offset;
                while (left < 0)
                {
                    left += availableSlotCount;
                }

                if (!result.Contains(left))
                {
                    result.Add(left);
                    return true;
                }
            }

            return false;
        }

        private void ApplyRhythmPacing(int slotIndex)
        {
            if (targetRotator == null || slotManager == null || CurrentLevel == null)
            {
                return;
            }

            float difficultySpeed = CurrentLevel.rotationSpeed * (CurrentDifficultyProfile != null ? CurrentDifficultyProfile.rotationSpeedMultiplier : 1f);
            if (CurrentDifficultyTier != null)
            {
                difficultySpeed *= Mathf.Clamp(CurrentDifficultyTier.rotationSpeedScale, 0.7f, 1.5f);
            }
            float rhythmBase = CurrentRhythmProfile != null && CurrentRhythmProfile.baseRotationSpeed > 0f ? CurrentRhythmProfile.baseRotationSpeed : difficultySpeed;
            float baseSpeed = Mathf.Max(18f, (difficultySpeed + rhythmBase) * 0.5f);

            bool currentDirection = targetRotator.Clockwise;
            float currentWait = slotManager.EstimateArrivalTime(slotIndex, baseSpeed, currentDirection);
            float flippedWait = slotManager.EstimateArrivalTime(slotIndex, baseSpeed, !currentDirection);

            float maxWait = GetMaxWaitSeconds();
            bool preferFlip = ShouldPreferDirectionFlip() && flippedWait < currentWait;
            bool useClockwise = currentDirection;

            if (preferFlip)
            {
                useClockwise = !currentDirection;
            }
            else if (Mathf.Min(currentWait, flippedWait) > maxWait)
            {
                useClockwise = flippedWait < currentWait ? !currentDirection : currentDirection;
            }

            float chosenWait = slotManager.EstimateArrivalTime(slotIndex, baseSpeed, useClockwise);
            float speedMultiplier = 1f;
            if (chosenWait > maxWait)
            {
                speedMultiplier = Mathf.Clamp(chosenWait / maxWait, 1f, 1.45f);
            }

            speedMultiplier = Mathf.Max(speedMultiplier, _persistentFlowSpeedMultiplier);

            if (_isPlayerArmed)
            {
                float armedAssistScale = CurrentDifficultyTier != null ? Mathf.Clamp(CurrentDifficultyTier.armedAssistScale, 0.8f, 1.6f) : 1f;
                speedMultiplier = Mathf.Max(speedMultiplier, (1.08f + Mathf.Clamp01(CurrentRhythmProfile != null ? CurrentRhythmProfile.easyFlowAssist : 0.65f) * 0.16f) * armedAssistScale);
            }

            if (IsPerfectChainWindowActive() && CurrentRhythmProfile != null)
            {
                float perfectChainScale = CurrentDifficultyTier != null ? Mathf.Clamp(CurrentDifficultyTier.perfectChainAssistScale, 0.8f, 1.8f) : 1f;
                speedMultiplier = Mathf.Max(speedMultiplier, (1.12f + Mathf.Clamp01(CurrentRhythmProfile.easyFlowAssist) * 0.18f) * perfectChainScale);
            }

            if (_perfectMomentumLevel > 0)
            {
                float momentumBoost = _perfectMomentumLevel >= 2 ? 1.78f : 1.52f;
                speedMultiplier = Mathf.Max(speedMultiplier, momentumBoost);
            }

            float assistDuration = CurrentRhythmProfile != null ? Mathf.Max(0.18f, CurrentRhythmProfile.postHitRetargetDelay + 0.18f) : 0.22f;
            if (_perfectMomentumLevel > 0)
            {
                assistDuration = Mathf.Max(assistDuration, _perfectMomentumLevel >= 2 ? 1.05f : 0.78f);
            }

            targetRotator.ApplyLevelSettings(baseSpeed, useClockwise);
            targetRotator.ApplyRhythmAssist(useClockwise, speedMultiplier, assistDuration);
            BroadcastRhythmFlowState(Mathf.Clamp01((speedMultiplier - 1f) / 0.35f));
        }

        private int BuildQuestionSeed()
        {
            unchecked
            {
                int hash = 17;
                hash = (hash * 31) + (CurrentLevel != null ? CurrentLevel.levelId : 0);
                hash = (hash * 31) + CurrentQuestionIndex;
                hash = (hash * 31) + (CurrentQuestion != null && !string.IsNullOrEmpty(CurrentQuestion.questionId) ? CurrentQuestion.questionId.GetHashCode() : 0);
                return hash;
            }
        }

        private static void ShuffleList(List<int> list, int seed)
        {
            System.Random random = new System.Random(seed);
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int tmp = list[i];
                list[i] = list[j];
                list[j] = tmp;
            }
        }

        private static int CircularDistance(int from, int to, int count)
        {
            int raw = Mathf.Abs(to - from);
            return Mathf.Min(raw, count - raw);
        }

        private float GetMaxWaitSeconds()
        {
            float assist = CurrentRhythmProfile != null ? Mathf.Clamp01(CurrentRhythmProfile.easyFlowAssist) : 0.65f;
            float wait = Mathf.Lerp(1.45f, 0.75f, assist);
            if (CurrentDifficultyTier != null)
            {
                wait *= Mathf.Clamp(CurrentDifficultyTier.waitCapScale, 0.55f, 1.6f);
            }

            if (_isPlayerArmed)
            {
                wait *= 0.72f;
            }

            if (IsPerfectChainWindowActive())
            {
                wait *= 0.82f;
            }

            return wait;
        }

        private bool ShouldPreferDirectionFlip()
        {
            if (CurrentRhythmProfile == null)
            {
                return false;
            }

            return IsPerfectChainWindowActive() && CurrentRhythmProfile.easyFlowAssist >= 0.7f;
        }

        private bool IsPerfectChainWindowActive()
        {
            return _perfectChainWindowUntil > 0f && Time.time <= _perfectChainWindowUntil;
        }

        private void HandlePinLoaded(char loadedLetter)
        {
            _isPlayerArmed = true;
            if (!IsPerfectChainWindowActive())
            {
                _perfectMomentumLevel = 0;
            }
            else if (_perfectMomentumLevel > 0)
            {
                _perfectMomentumLevel = Mathf.Min(2, _perfectMomentumLevel + 1);
                _persistentFlowSpeedMultiplier = Mathf.Max(_persistentFlowSpeedMultiplier, _perfectMomentumLevel >= 2 ? 1.72f : 1.50f);
            }
            if (_currentTargetSlotIndex >= 0)
            {
                ApplyRhythmPacing(_currentTargetSlotIndex);
            }
        }

        private void HandlePinReleased()
        {
            _isPlayerArmed = false;
            BroadcastRhythmFlowState(0f);
        }

        private void BroadcastRhythmFlowState(float flowIntensity)
        {
            GameEvents.RaiseRhythmFlowStateChanged(new RhythmFlowStateData
            {
                flowIntensity = Mathf.Clamp01(Mathf.Max(flowIntensity, (_persistentFlowSpeedMultiplier - 1f) / 0.35f)),
                perfectMomentumLevel = _perfectMomentumLevel,
                isPlayerArmed = _isPlayerArmed,
                isPerfectChainWindowActive = IsPerfectChainWindowActive()
            });
        }

        private static string ResolveDefaultDifficultyTierId(int levelId)
        {
            if (levelId <= 10)
            {
                return "intro_perfect";
            }

            if (levelId <= 25)
            {
                return "early_flow";
            }

            if (levelId <= 80)
            {
                return "mid_pressure";
            }

            if (levelId <= 160)
            {
                return "late_variation";
            }

            return "expert_drive";
        }

        private static string ResolveDefaultShapeLayoutId(LevelDefinition level)
        {
            if (level == null || string.IsNullOrWhiteSpace(level.shape))
            {
                return "circle_classic";
            }

            switch (level.shape.ToLowerInvariant())
            {
                case "triangle":
                    return "diamond_drive";
                case "square":
                    return "oval_flow";
                default:
                    return "circle_classic";
            }
        }
    }
}
