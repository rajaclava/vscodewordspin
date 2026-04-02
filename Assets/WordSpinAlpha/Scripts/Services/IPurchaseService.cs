namespace WordSpinAlpha.Services
{
    public enum PurchaseProductType
    {
        Theme,
        HintPack,
        EnergyPack,
        NoAds,
        PremiumMembership
    }

    public struct PurchaseResult
    {
        public bool success;
        public string productId;
        public string message;
    }

    public interface IPurchaseService
    {
        PurchaseResult Purchase(string productId, PurchaseProductType productType);
    }
}
