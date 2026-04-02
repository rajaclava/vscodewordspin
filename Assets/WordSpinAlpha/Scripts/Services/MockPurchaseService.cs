using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Services
{
    public class MockPurchaseService : Singleton<MockPurchaseService>, IPurchaseService
    {
        public PurchaseResult Purchase(string productId, PurchaseProductType productType)
        {
            PurchaseResult result = new PurchaseResult
            {
                success = true,
                productId = productId,
                message = $"Mock purchase completed: {productId}"
            };

            switch (productType)
            {
                case PurchaseProductType.Theme:
                    EconomyManager.Instance?.UnlockTheme(productId);
                    break;
                case PurchaseProductType.HintPack:
                    EconomyManager.Instance?.GrantHints(3);
                    break;
                case PurchaseProductType.EnergyPack:
                    EnergyManager.Instance?.GrantEnergy(3);
                    break;
                case PurchaseProductType.NoAds:
                    EconomyManager.Instance?.SetNoAdsOwned(true);
                    break;
                case PurchaseProductType.PremiumMembership:
                    EconomyManager.Instance?.SetPremiumMembership(true);
                    break;
            }

            GameEvents.RaiseMetric("mockPurchase", $"{{\"productId\":\"{productId}\",\"type\":\"{productType}\"}}");
            Debug.Log($"[MockPurchaseService] {result.message}");
            return result;
        }
    }
}
