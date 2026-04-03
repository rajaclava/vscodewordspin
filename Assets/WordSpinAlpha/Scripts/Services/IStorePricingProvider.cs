namespace WordSpinAlpha.Services
{
    public enum StorePriceSource
    {
        Preview,
        PlayStore
    }

    public struct StorePriceQuote
    {
        public bool available;
        public string productId;
        public string formattedPrice;
        public string currencyCode;
        public string regionCode;
        public StorePriceSource source;
        public string note;
    }

    public interface IStorePricingProvider
    {
        StorePriceQuote GetMembershipQuote(string languageCode);
        StorePriceQuote GetThemeQuote(string themeId, string languageCode);
    }
}
