using UnityEngine;
using WordSpinAlpha.Presentation;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Core
{
    public class SceneBootstrap : MonoBehaviour
    {
        private void Awake()
        {
            EnsureSingleton<SaveManager>("SaveManager");
            EnsureSingleton<LocalContentProvider>("LocalContentProvider");
            EnsureSingleton<RemoteContentProvider>("RemoteContentProvider");
            EnsureSingleton<ContentService>("ContentService");
            EnsureSingleton<MetricLogger>("MetricLogger");
            EnsureSingleton<TelemetryService>("TelemetryService");
            EnsureSingleton<TestPlayerModeManager>("TestPlayerModeManager");
            EnsureSingleton<EconomyManager>("EconomyManager");
            EnsureSingleton<EnergyManager>("EnergyManager");
            EnsureSingleton<QuestionLifeManager>("QuestionLifeManager");
            EnsureSingleton<InputManager>("InputManager");
            EnsureSingleton<MockPurchaseService>("MockPurchaseService");
            EnsureSingleton<PreviewStorePricingProvider>("PreviewStorePricingProvider");
            EnsureSingleton<StorePricingManager>("StorePricingManager");
            EnsureSingleton<SceneNavigator>("SceneNavigator");
            EnsureSingleton<GlobalMusicManager>("GlobalMusicManager");
            EnsureSingleton<StatsManager>("StatsManager");
            EnsureSingleton<ScoreManager>("ScoreManager");
            EnsureSingleton<LevelEconomyManager>("LevelEconomyManager");
            EnsureSingleton<MobileRuntimeController>("MobileRuntimeController");
            EnsureSingleton<DebugRewardedAdPresenter>("DebugRewardedAdPresenter");
        }

        private static void EnsureSingleton<T>(string objectName) where T : Component
        {
            if (Object.FindObjectOfType<T>() != null)
            {
                return;
            }

            GameObject singletonObject = new GameObject(objectName);
            singletonObject.AddComponent<T>();
        }
    }
}
