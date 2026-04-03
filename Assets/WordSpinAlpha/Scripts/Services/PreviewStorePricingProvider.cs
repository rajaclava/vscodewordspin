using System;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Services
{
    public class PreviewStorePricingProvider : Singleton<PreviewStorePricingProvider>, IStorePricingProvider
    {
        private StoreCatalogDefinition _catalog;

        protected override bool PersistAcrossScenes => true;

        public StorePriceQuote GetMembershipQuote(string languageCode)
        {
            EnsureCatalog();
            EconomyBalanceProfile profile = LevelEconomyManager.Instance != null ? LevelEconomyManager.Instance.Profile : null;
            EconomyBalanceProfile.RegionalPricePreview preview = ResolvePreview(profile, languageCode);

            return new StorePriceQuote
            {
                available = preview != null,
                productId = _catalog != null ? _catalog.premiumMembershipProductId : string.Empty,
                formattedPrice = preview != null ? $"{preview.membershipPrice} {preview.currencyCode}" : string.Empty,
                currencyCode = preview != null ? preview.currencyCode : string.Empty,
                regionCode = preview != null ? preview.regionCode : string.Empty,
                source = StorePriceSource.Preview,
                note = preview != null ? preview.notes : "Preview fiyat bulunamadi."
            };
        }

        public StorePriceQuote GetThemeQuote(string themeId, string languageCode)
        {
            EnsureCatalog();
            EconomyBalanceProfile profile = LevelEconomyManager.Instance != null ? LevelEconomyManager.Instance.Profile : null;
            EconomyBalanceProfile.RegionalPricePreview preview = ResolvePreview(profile, languageCode);
            string productId = ResolveThemeProductId(themeId);

            return new StorePriceQuote
            {
                available = preview != null,
                productId = productId,
                formattedPrice = preview != null ? $"{preview.themePackPrice} {preview.currencyCode}" : string.Empty,
                currencyCode = preview != null ? preview.currencyCode : string.Empty,
                regionCode = preview != null ? preview.regionCode : string.Empty,
                source = StorePriceSource.Preview,
                note = preview != null ? preview.notes : "Preview fiyat bulunamadi."
            };
        }

        private void EnsureCatalog()
        {
            if (_catalog == null && ContentService.Instance != null)
            {
                _catalog = ContentService.Instance.LoadStoreCatalog();
            }
        }

        private static EconomyBalanceProfile.RegionalPricePreview ResolvePreview(EconomyBalanceProfile profile, string languageCode)
        {
            if (profile == null || profile.RegionalPricePreviews == null)
            {
                return null;
            }

            string targetRegion = ResolvePreviewRegionForLanguage(languageCode);
            for (int i = 0; i < profile.RegionalPricePreviews.Count; i++)
            {
                EconomyBalanceProfile.RegionalPricePreview preview = profile.RegionalPricePreviews[i];
                if (preview != null && string.Equals(preview.regionCode, targetRegion, StringComparison.OrdinalIgnoreCase))
                {
                    return preview;
                }
            }

            for (int i = 0; i < profile.RegionalPricePreviews.Count; i++)
            {
                if (profile.RegionalPricePreviews[i] != null)
                {
                    return profile.RegionalPricePreviews[i];
                }
            }

            return null;
        }

        private string ResolveThemeProductId(string themeId)
        {
            if (_catalog == null || _catalog.themes == null)
            {
                return string.Empty;
            }

            for (int i = 0; i < _catalog.themes.Length; i++)
            {
                ThemePriceDefinition theme = _catalog.themes[i];
                if (theme != null && string.Equals(theme.themeId, themeId, StringComparison.OrdinalIgnoreCase))
                {
                    return theme.iapProductId ?? string.Empty;
                }
            }

            return string.Empty;
        }

        private static string ResolvePreviewRegionForLanguage(string languageCode)
        {
            switch (GameConstants.NormalizeLanguageCode(languageCode))
            {
                case "en":
                    return "US";
                case "es":
                    return "ES";
                case "de":
                    return "DE";
                default:
                    return "TR";
            }
        }
    }
}
