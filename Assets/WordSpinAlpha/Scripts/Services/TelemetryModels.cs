using System;
using System.Collections.Generic;

namespace WordSpinAlpha.Services
{
    [Serializable]
    public class TelemetryEventRecord
    {
        public string timestampUtc;
        public string sessionId;
        public string playerId;
        public int levelId;
        public string questionId;
        public string themeId;
        public string difficultyProfileId;
        public string rhythmProfileId;
        public string eventType;
        public string resultType;
        public int attemptIndex;
        public int heartBefore;
        public int heartAfter;
        public int energyBefore;
        public int energyAfter;
        public int comboBefore;
        public int comboAfter;
        public float timeSinceQuestionStart;
        public float timeSincePreviousHit;
        public string payload;
    }

    [Serializable]
    public class TelemetryQueueData
    {
        public List<TelemetryEventRecord> events = new List<TelemetryEventRecord>();
    }

    [Serializable]
    public class LevelTelemetrySummary
    {
        public int levelId;
        public string themeId;
        public string difficultyProfileId;
        public string rhythmProfileId;
        public int starts;
        public int completes;
        public int perfectHits;
        public int goodHits;
        public int nearMisses;
        public int misses;
        public int wrongSlots;
        public int wrongLetters;
        public int retries;
        public int continues;
        public float totalQuestionTime;
        public int questionTimeSamples;
        public int highestPerfectCombo;
        public string recommendation;
    }

    [Serializable]
    public class TelemetrySnapshotData
    {
        public string generatedAtUtc;
        public string manifestVersion;
        public int pendingEventCount;
        public List<LevelTelemetrySummary> levelSummaries = new List<LevelTelemetrySummary>();
    }
}
