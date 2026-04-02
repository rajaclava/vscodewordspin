using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Services
{
    public class TelemetryService : Singleton<TelemetryService>
    {
        private const float TelemetryWriteThrottleSeconds = 0.35f;

        private readonly TelemetryQueueData _queue = new TelemetryQueueData();
        private readonly Dictionary<int, LevelTelemetrySummary> _levelSummaries = new Dictionary<int, LevelTelemetrySummary>();
        private LevelContext? _currentLevel;
        private QuestionContext? _currentQuestion;
        private float _questionStartedAt;
        private float _lastHitAt;
        private int _currentCombo;
        private bool _queueSavePending;
        private bool _snapshotSavePending;
        private float _nextWriteAllowedAt;

        private string QueuePath => Path.Combine(Application.persistentDataPath, GameConstants.TelemetryQueueFileName);
        private string SnapshotPath => Path.Combine(Application.persistentDataPath, GameConstants.TelemetrySnapshotFileName);

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            EnsureSessionId();
            LoadQueue();
        }

        private void OnEnable()
        {
            GameEvents.LevelStarted += HandleLevelStarted;
            GameEvents.QuestionStarted += HandleQuestionStarted;
            GameEvents.HitEvaluated += HandleHitEvaluated;
            GameEvents.QuestionFailed += HandleQuestionFailed;
            GameEvents.QuestionCompleted += HandleQuestionCompleted;
            GameEvents.LevelCompleted += HandleLevelCompleted;
            GameEvents.MetricEventRaised += HandleMetricRaised;
        }

        private void Update()
        {
            FlushPendingPersistence(false);
        }

        private void OnDisable()
        {
            GameEvents.LevelStarted -= HandleLevelStarted;
            GameEvents.QuestionStarted -= HandleQuestionStarted;
            GameEvents.HitEvaluated -= HandleHitEvaluated;
            GameEvents.QuestionFailed -= HandleQuestionFailed;
            GameEvents.QuestionCompleted -= HandleQuestionCompleted;
            GameEvents.LevelCompleted -= HandleLevelCompleted;
            GameEvents.MetricEventRaised -= HandleMetricRaised;
            FlushPendingPersistence(true);
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                FlushPendingPersistence(true);
            }
        }

        private void OnApplicationQuit()
        {
            FlushPendingPersistence(true);
        }

        private void HandleLevelStarted(LevelContext context)
        {
            _currentLevel = context;
            _currentCombo = 0;
            GetOrCreateLevelSummary(context).starts++;
            TrackEvent("levelStart", string.Empty, "{}", 0, 0);
        }

        private void HandleQuestionStarted(QuestionContext context, string answerWord)
        {
            _currentQuestion = context;
            _questionStartedAt = Time.unscaledTime;
            TrackEvent("questionStart", string.Empty, "{}", _currentCombo, _currentCombo);
        }

        private void HandleHitEvaluated(HitData hit)
        {
            string resultType = hit.resultType.ToString();
            int comboBefore = _currentCombo;
            LevelTelemetrySummary summary = GetOrCreateLevelSummary(_currentLevel);

            switch (hit.resultType)
            {
                case HitResultType.Perfect:
                    summary.perfectHits++;
                    _currentCombo++;
                    summary.highestPerfectCombo = Mathf.Max(summary.highestPerfectCombo, _currentCombo);
                    break;
                case HitResultType.Tolerated:
                    summary.goodHits++;
                    _currentCombo = 0;
                    break;
                case HitResultType.NearMiss:
                    summary.nearMisses++;
                    _currentCombo = 0;
                    break;
                case HitResultType.Miss:
                    summary.misses++;
                    _currentCombo = 0;
                    break;
                case HitResultType.WrongSlot:
                    summary.wrongSlots++;
                    _currentCombo = 0;
                    break;
                case HitResultType.WrongLetter:
                    summary.wrongLetters++;
                    _currentCombo = 0;
                    break;
            }

            TrackEvent("hit", resultType, $"{{\"slotIndex\":{hit.slotIndex},\"expectedSlotIndex\":{hit.expectedSlotIndex}}}", comboBefore, _currentCombo);
            _lastHitAt = Time.unscaledTime;
            MarkSnapshotDirty();
        }

        private void HandleQuestionFailed()
        {
            float elapsed = _questionStartedAt > 0f ? Time.unscaledTime - _questionStartedAt : 0f;
            LevelTelemetrySummary summary = GetOrCreateLevelSummary(_currentLevel);
            summary.totalQuestionTime += elapsed;
            summary.questionTimeSamples++;
            TrackEvent("questionFail", string.Empty, "{}", _currentCombo, 0);
            _currentCombo = 0;
            MarkSnapshotDirty();
        }

        private void HandleQuestionCompleted(QuestionContext context)
        {
            float elapsed = _questionStartedAt > 0f ? Time.unscaledTime - _questionStartedAt : 0f;
            LevelTelemetrySummary summary = GetOrCreateLevelSummary(_currentLevel);
            summary.totalQuestionTime += elapsed;
            summary.questionTimeSamples++;
            TrackEvent("questionComplete", string.Empty, "{}", _currentCombo, _currentCombo);
            MarkSnapshotDirty();
        }

        private void HandleLevelCompleted(LevelContext context)
        {
            GetOrCreateLevelSummary(context).completes++;
            TrackEvent("levelComplete", string.Empty, "{}", _currentCombo, _currentCombo);
            MarkSnapshotDirty();
        }

        private void HandleMetricRaised(string eventName, string payload)
        {
            LevelTelemetrySummary summary = GetOrCreateLevelSummary(_currentLevel);
            if (eventName == "retryLevel")
            {
                summary.retries++;
            }
            else if (eventName == "continueLevel")
            {
                summary.continues++;
            }

            TrackEvent(eventName, string.Empty, payload, _currentCombo, _currentCombo);
            MarkSnapshotDirty();
        }

        private void TrackEvent(string eventType, string resultType, string payload, int comboBefore, int comboAfter)
        {
            int hearts = QuestionLifeManager.Instance != null ? QuestionLifeManager.Instance.CurrentHearts : 0;
            int energy = EnergyManager.Instance != null ? EnergyManager.Instance.CurrentEnergy : 0;

            TelemetryEventRecord record = new TelemetryEventRecord
            {
                timestampUtc = DateTime.UtcNow.ToString("O"),
                sessionId = SaveManager.Instance != null ? SaveManager.Instance.Data.telemetry.currentSessionId : Guid.NewGuid().ToString("N"),
                playerId = SystemInfo.deviceUniqueIdentifier,
                levelId = _currentLevel.HasValue ? _currentLevel.Value.levelId : 0,
                questionId = _currentQuestion.HasValue ? _currentQuestion.Value.questionId : string.Empty,
                themeId = _currentLevel.HasValue ? _currentLevel.Value.themeId : string.Empty,
                difficultyProfileId = _currentLevel.HasValue ? _currentLevel.Value.difficultyProfileId : string.Empty,
                rhythmProfileId = _currentLevel.HasValue ? _currentLevel.Value.rhythmProfileId : string.Empty,
                eventType = eventType,
                resultType = resultType,
                attemptIndex = _currentQuestion.HasValue ? _currentQuestion.Value.questionIndex : 0,
                heartBefore = hearts,
                heartAfter = hearts,
                energyBefore = energy,
                energyAfter = energy,
                comboBefore = comboBefore,
                comboAfter = comboAfter,
                timeSinceQuestionStart = _questionStartedAt > 0f ? Time.unscaledTime - _questionStartedAt : 0f,
                timeSincePreviousHit = _lastHitAt > 0f ? Time.unscaledTime - _lastHitAt : 0f,
                payload = payload ?? "{}"
            };

            _queue.events.Add(record);
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.telemetry.pendingTelemetryEventCount = _queue.events.Count;
                SaveManager.Instance.Save();
            }

            MarkQueueDirty();
        }

        private void EnsureSessionId()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(SaveManager.Instance.Data.telemetry.currentSessionId))
            {
                SaveManager.Instance.Data.telemetry.currentSessionId = Guid.NewGuid().ToString("N");
                SaveManager.Instance.Save();
            }
        }

        private void LoadQueue()
        {
            if (!File.Exists(QueuePath))
            {
                return;
            }

            try
            {
                string json = File.ReadAllText(QueuePath);
                TelemetryQueueData loaded = JsonUtility.FromJson<TelemetryQueueData>(json);
                if (loaded != null && loaded.events != null)
                {
                    _queue.events.AddRange(loaded.events);
                }
            }
            catch
            {
                _queue.events.Clear();
            }
        }

        private void SaveQueue()
        {
            File.WriteAllText(QueuePath, JsonUtility.ToJson(_queue, true));
        }

        private void SaveSnapshot()
        {
            TelemetrySnapshotData snapshot = new TelemetrySnapshotData
            {
                generatedAtUtc = DateTime.UtcNow.ToString("O"),
                manifestVersion = SaveManager.Instance != null ? SaveManager.Instance.Data.remoteContent.activeManifestVersion : "local-only",
                pendingEventCount = _queue.events.Count
            };

            foreach (LevelTelemetrySummary summary in _levelSummaries.Values)
            {
                summary.recommendation = BuildRecommendation(summary);
                snapshot.levelSummaries.Add(summary);
            }

            File.WriteAllText(SnapshotPath, JsonUtility.ToJson(snapshot, true));
        }

        private void MarkQueueDirty()
        {
            _queueSavePending = true;
            SchedulePersistence();
        }

        private void MarkSnapshotDirty()
        {
            _snapshotSavePending = true;
            SchedulePersistence();
        }

        private void SchedulePersistence()
        {
            if (_nextWriteAllowedAt <= 0f)
            {
                _nextWriteAllowedAt = Time.unscaledTime + TelemetryWriteThrottleSeconds;
            }
        }

        private void FlushPendingPersistence(bool force)
        {
            if (!_queueSavePending && !_snapshotSavePending)
            {
                return;
            }

            if (!force && Time.unscaledTime < _nextWriteAllowedAt)
            {
                return;
            }

            if (_queueSavePending)
            {
                SaveQueue();
                _queueSavePending = false;
            }

            if (_snapshotSavePending)
            {
                SaveSnapshot();
                _snapshotSavePending = false;
            }

            _nextWriteAllowedAt = Time.unscaledTime + TelemetryWriteThrottleSeconds;
        }

        private LevelTelemetrySummary GetOrCreateLevelSummary(LevelContext? context)
        {
            if (!context.HasValue)
            {
                return GetOrCreateLevelSummary(0, string.Empty, string.Empty, string.Empty);
            }

            return GetOrCreateLevelSummary(context.Value.levelId, context.Value.themeId, context.Value.difficultyProfileId, context.Value.rhythmProfileId);
        }

        private LevelTelemetrySummary GetOrCreateLevelSummary(LevelContext context)
        {
            return GetOrCreateLevelSummary(context.levelId, context.themeId, context.difficultyProfileId, context.rhythmProfileId);
        }

        private LevelTelemetrySummary GetOrCreateLevelSummary(int levelId, string themeId, string difficultyProfileId, string rhythmProfileId)
        {
            if (_levelSummaries.TryGetValue(levelId, out LevelTelemetrySummary existing))
            {
                return existing;
            }

            LevelTelemetrySummary created = new LevelTelemetrySummary
            {
                levelId = levelId,
                themeId = themeId,
                difficultyProfileId = difficultyProfileId,
                rhythmProfileId = rhythmProfileId
            };

            _levelSummaries[levelId] = created;
            return created;
        }

        private static string BuildRecommendation(LevelTelemetrySummary summary)
        {
            if (summary.levelId <= 0)
            {
                return "Telemetry toplaniyor.";
            }

            if (summary.wrongSlots > summary.perfectHits + summary.goodHits)
            {
                return "Wrong Slot baskin. Magnet radius veya hedef akis suresini artir.";
            }

            if (summary.misses > summary.perfectHits + summary.goodHits)
            {
                return "Miss orani yuksek. Bekleme suresini kisalt veya ritim akisina yardim ekle.";
            }

            if (summary.completes == 0 && summary.starts >= 3)
            {
                return "Tamamlama yok. Zorluk ve pacing birlikte dusurulmeli.";
            }

            if (summary.continues > summary.retries)
            {
                return "Continue kullanimi yuksek. Fail egrisi agresif olabilir.";
            }

            return "Seviye dengesi kabul edilebilir gorunuyor.";
        }
    }
}
