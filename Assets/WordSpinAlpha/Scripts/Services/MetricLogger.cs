using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Services
{
    public class MetricLogger : Singleton<MetricLogger>
    {
        private void OnEnable()
        {
            GameEvents.MetricEventRaised += HandleMetricRaised;
        }

        private void OnDisable()
        {
            GameEvents.MetricEventRaised -= HandleMetricRaised;
        }

        private void HandleMetricRaised(string eventName, string payload)
        {
            Debug.Log($"[Metric] {eventName} :: {payload}");
        }
    }
}
