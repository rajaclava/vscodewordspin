using UnityEngine;

namespace WordSpinAlpha.Services
{
    public class TelemetryPolicyProfile : ScriptableObject
    {
        public const string DefaultResourcePath = "Configs/TelemetryPolicyProfile";

        [SerializeField] private bool telemetryEnabled = true;
        [SerializeField] private float writeThrottleSeconds = 0.35f;
        [SerializeField] private int maxQueuedEvents = 4000;
        [SerializeField] private int maxSnapshotLevelSummaries = 120;
        [SerializeField] private bool trimQueueOnLoad = true;
        [SerializeField] private bool savePendingEventCountToSaveData = false;
        [SerializeField] private bool flushOnApplicationPause = true;

        public bool TelemetryEnabled => telemetryEnabled;
        public float WriteThrottleSeconds => Mathf.Max(0.05f, writeThrottleSeconds);
        public int MaxQueuedEvents => Mathf.Max(100, maxQueuedEvents);
        public int MaxSnapshotLevelSummaries => Mathf.Max(8, maxSnapshotLevelSummaries);
        public bool TrimQueueOnLoad => trimQueueOnLoad;
        public bool SavePendingEventCountToSaveData => savePendingEventCountToSaveData;
        public bool FlushOnApplicationPause => flushOnApplicationPause;

        public void ClampToSafeDefaults()
        {
            writeThrottleSeconds = Mathf.Max(0.05f, writeThrottleSeconds);
            maxQueuedEvents = Mathf.Max(100, maxQueuedEvents);
            maxSnapshotLevelSummaries = Mathf.Max(8, maxSnapshotLevelSummaries);
        }
    }
}
