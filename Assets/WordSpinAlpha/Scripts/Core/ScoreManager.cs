using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class ScoreManager : Singleton<ScoreManager>
    {
        [SerializeField] private ScoreTuningProfile profile;

        private int _totalScore;
        private int _hitScore;
        private int _perfectStreak;
        private float _currentMultiplier = 1f;
        private float _bestMultiplier = 1f;
        private int _mistakeCount;
        private int _perfectHits;
        private int _goodHits;
        private int _activeLevelId;
        private float _levelStartedAt;
        private float _targetShownAt = -1f;
        private float _lastSuccessfulHitAt;

        public int TotalScore => _totalScore;
        public float CurrentMultiplier => _currentMultiplier;
        public int PerfectStreak => _perfectStreak;

        protected override bool PersistAcrossScenes => true;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            if (profile == null)
            {
                profile = Resources.Load<ScoreTuningProfile>("Configs/ScoreTuningProfile");
            }
        }

        private void OnEnable()
        {
            GameEvents.LevelStarted += HandleLevelStarted;
            GameEvents.TargetSlotUpdated += HandleTargetSlotUpdated;
            GameEvents.HitEvaluated += HandleHitEvaluated;
            GameEvents.LevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            GameEvents.LevelStarted -= HandleLevelStarted;
            GameEvents.TargetSlotUpdated -= HandleTargetSlotUpdated;
            GameEvents.HitEvaluated -= HandleHitEvaluated;
            GameEvents.LevelCompleted -= HandleLevelCompleted;
        }

        private void HandleLevelStarted(LevelContext context)
        {
            if (context.questionIndex != 0 && context.levelId == _activeLevelId)
            {
                _targetShownAt = Time.unscaledTime;
                return;
            }

            _activeLevelId = context.levelId;
            _totalScore = 0;
            _hitScore = 0;
            _perfectStreak = 0;
            _currentMultiplier = 1f;
            _bestMultiplier = 1f;
            _mistakeCount = 0;
            _perfectHits = 0;
            _goodHits = 0;
            _levelStartedAt = Time.unscaledTime;
            _targetShownAt = Time.unscaledTime;
            _lastSuccessfulHitAt = Time.unscaledTime;
            PublishScoreChanged(0, 0, 0, 0, HitResultType.Tolerated);
        }

        private void HandleTargetSlotUpdated(int slotIndex, int answerIndex, char letter)
        {
            _targetShownAt = Time.unscaledTime;
        }

        private void HandleHitEvaluated(HitData hit)
        {
            ScoreTuningProfile tuning = profile;
            if (tuning == null)
            {
                return;
            }

            int basePoints = 0;
            int reactionBonus = 0;
            int award = 0;

            switch (hit.resultType)
            {
                case HitResultType.Perfect:
                    _perfectHits++;
                    _perfectStreak++;
                    _currentMultiplier = tuning.ResolvePerfectMultiplier(_perfectStreak);
                    _bestMultiplier = Mathf.Max(_bestMultiplier, _currentMultiplier);
                    basePoints = tuning.PerfectBasePoints;
                    reactionBonus = tuning.ResolveReactionBonus(Time.unscaledTime - Mathf.Max(0f, _targetShownAt));
                    award = Mathf.RoundToInt(basePoints * _currentMultiplier) + reactionBonus;
                    ApplyAward(award, true);
                    _lastSuccessfulHitAt = Time.unscaledTime;
                    break;

                case HitResultType.Tolerated:
                    _goodHits++;
                    float goodMultiplier = tuning.GoodUsesCurrentMultiplier ? Mathf.Max(1f, _currentMultiplier) : 1f;
                    basePoints = tuning.GoodBasePoints;
                    reactionBonus = tuning.ResolveReactionBonus(Time.unscaledTime - Mathf.Max(0f, _targetShownAt));
                    award = Mathf.RoundToInt(basePoints * goodMultiplier) + reactionBonus;
                    ApplyAward(award, true);
                    _lastSuccessfulHitAt = Time.unscaledTime;
                    if (tuning.GoodResetsPerfectChain)
                    {
                        ResetPerfectChain();
                    }
                    break;

                case HitResultType.NearMiss:
                case HitResultType.WrongSlot:
                case HitResultType.WrongLetter:
                case HitResultType.Miss:
                    _mistakeCount++;
                    ResetPerfectChain();
                    break;
            }

            PublishScoreChanged(award, basePoints, reactionBonus, 0, hit.resultType);
        }

        private void HandleLevelCompleted(LevelContext context)
        {
            ScoreTuningProfile tuning = profile;
            if (tuning == null)
            {
                return;
            }

            float clearReferenceTime = _lastSuccessfulHitAt > _levelStartedAt ? _lastSuccessfulHitAt : Time.unscaledTime;
            float elapsedSeconds = Mathf.Max(0f, clearReferenceTime - _levelStartedAt);
            int clearBonus = tuning.LevelClearBonus;
            int noMistakeBonus = _mistakeCount == 0 ? tuning.NoMistakeBonus : 0;
            int allPerfectBonus = _perfectHits > 0 && _goodHits == 0 && _mistakeCount == 0 ? tuning.AllPerfectBonus : 0;
            int timeBonus = tuning.ResolveClearTimeBonus(elapsedSeconds) + tuning.ResolveRemainingTimeBonus(elapsedSeconds, 0f);
            int totalClearBonus = clearBonus + noMistakeBonus + allPerfectBonus + timeBonus;

            ApplyAward(totalClearBonus, false);
            PublishScoreChanged(totalClearBonus, 0, 0, totalClearBonus, HitResultType.Tolerated);
            GameEvents.RaiseLevelScoreFinalized(new LevelScoreSummaryData
            {
                levelId = context.levelId,
                totalScore = _totalScore,
                hitScore = _hitScore,
                clearBonus = clearBonus,
                noMistakeBonus = noMistakeBonus,
                allPerfectBonus = allPerfectBonus,
                timeBonus = timeBonus,
                bestMultiplier = _bestMultiplier,
                perfectHits = _perfectHits,
                goodHits = _goodHits,
                mistakeCount = _mistakeCount,
                elapsedSeconds = elapsedSeconds
            });
        }

        private void ApplyAward(int award, bool countAsHitScore)
        {
            if (award <= 0)
            {
                return;
            }

            _totalScore += award;
            if (countAsHitScore)
            {
                _hitScore += award;
            }
        }

        private void ResetPerfectChain()
        {
            _perfectStreak = 0;
            _currentMultiplier = 1f;
        }

        private void PublishScoreChanged(int award, int basePoints, int reactionBonus, int clearBonus, HitResultType hitType)
        {
            GameEvents.RaiseScoreChanged(new ScoreStateData
            {
                totalScore = _totalScore,
                lastAward = award,
                basePoints = basePoints,
                reactionBonus = reactionBonus,
                clearBonus = clearBonus,
                multiplier = _currentMultiplier,
                perfectStreak = _perfectStreak,
                lastHitType = hitType
            });
        }

        public void ResetForTesting()
        {
            _totalScore = 0;
            _hitScore = 0;
            _perfectStreak = 0;
            _currentMultiplier = 1f;
            _bestMultiplier = 1f;
            _mistakeCount = 0;
            _perfectHits = 0;
            _goodHits = 0;
            PublishScoreChanged(0, 0, 0, 0, HitResultType.Tolerated);
        }

        public void PopulateSessionSnapshot(SessionSnapshot snapshot)
        {
            if (snapshot == null)
            {
                return;
            }

            snapshot.currentScoreTotal = _totalScore;
            snapshot.currentHitScore = _hitScore;
            snapshot.currentPerfectStreak = _perfectStreak;
            snapshot.currentMultiplier = _currentMultiplier;
            snapshot.currentBestMultiplier = _bestMultiplier;
            snapshot.currentMistakeCount = _mistakeCount;
            snapshot.currentPerfectHits = _perfectHits;
            snapshot.currentGoodHits = _goodHits;
            snapshot.currentLevelElapsedSeconds = Mathf.Max(0f, Time.unscaledTime - _levelStartedAt);
            snapshot.currentTargetShownElapsedSeconds = _targetShownAt >= 0f
                ? Mathf.Max(0f, Time.unscaledTime - _targetShownAt)
                : -1f;
            snapshot.currentLastSuccessfulHitElapsedSeconds = _lastSuccessfulHitAt > 0f
                ? Mathf.Max(0f, Time.unscaledTime - _lastSuccessfulHitAt)
                : -1f;
        }

        public void RestoreSession(SessionSnapshot snapshot)
        {
            if (snapshot == null || !snapshot.hasActiveSession)
            {
                return;
            }

            _activeLevelId = snapshot.levelId;
            _totalScore = Mathf.Max(0, snapshot.currentScoreTotal);
            _hitScore = Mathf.Max(0, snapshot.currentHitScore);
            _perfectStreak = Mathf.Max(0, snapshot.currentPerfectStreak);
            _currentMultiplier = Mathf.Max(1f, snapshot.currentMultiplier);
            _bestMultiplier = Mathf.Max(_currentMultiplier, snapshot.currentBestMultiplier);
            _mistakeCount = Mathf.Max(0, snapshot.currentMistakeCount);
            _perfectHits = Mathf.Max(0, snapshot.currentPerfectHits);
            _goodHits = Mathf.Max(0, snapshot.currentGoodHits);
            _levelStartedAt = Time.unscaledTime - Mathf.Max(0f, snapshot.currentLevelElapsedSeconds);
            _targetShownAt = snapshot.currentTargetShownElapsedSeconds >= 0f
                ? Time.unscaledTime - snapshot.currentTargetShownElapsedSeconds
                : Time.unscaledTime;
            _lastSuccessfulHitAt = snapshot.currentLastSuccessfulHitElapsedSeconds >= 0f
                ? Time.unscaledTime - snapshot.currentLastSuccessfulHitElapsedSeconds
                : _levelStartedAt;

            PublishScoreChanged(0, 0, 0, 0, HitResultType.Tolerated);
        }
    }
}
