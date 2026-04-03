using WordSpinAlpha.Core;

namespace WordSpinAlpha.Services
{
    public class StorePricingManager : Singleton<StorePricingManager>, IStorePricingProvider
    {
        protected override bool PersistAcrossScenes => true;

        public StorePriceQuote GetMembershipQuote(string languageCode)
        {
            if (PreviewStorePricingProvider.Instance != null)
            {
                return PreviewStorePricingProvider.Instance.GetMembershipQuote(languageCode);
            }

            return default;
        }

        public StorePriceQuote GetThemeQuote(string themeId, string languageCode)
        {
            if (PreviewStorePricingProvider.Instance != null)
            {
                return PreviewStorePricingProvider.Instance.GetThemeQuote(themeId, languageCode);
            }

            return default;
        }
    }
}
