using System;
using System.Collections.Generic;
using UnityEngine;

namespace WordSpinAlpha.Core
{
    [CreateAssetMenu(fileName = "EconomyBalanceProfile", menuName = "WordSpin Alpha/Economy Balance Profile")]
    public class EconomyBalanceProfile : ScriptableObject
    {
        public const string DefaultResourcePath = "Configs/EconomyBalanceProfile";
        public const string FreeSandboxResourcePath = "Configs/EconomyBalanceProfile_Free";
        public const string PremiumSandboxResourcePath = "Configs/EconomyBalanceProfile_Premium";

        [Serializable]
        public class LevelCoinOverride
        {
            public int levelId;
            public int firstClearCoins = 20;
            public int replayCoins;
        }

        [Serializable]
        public class ThemeOfferTuning
        {
            public string themeId = GameConstants.PremiumMythologyThemeId;
            public int softCurrencyPriceOverride = 1200;
            public bool allowCoinPurchase = true;
            public bool allowPremiumMembership = true;
            public bool allowDirectPurchase = true;
            public string iapTierId = "theme_small";
        }

        [Serializable]
        public class RegionalPricePreview
        {
            public string regionCode = "TR";
            public string currencyCode = "TRY";
            public string membershipPrice = "0";
            public string themePackPrice = "0";
            public string notes = string.Empty;
        }

        [Header("Coin Odul Kurallari")]
        [SerializeField] private bool awardCoinsOnlyOnFirstCompletion = true;
        [SerializeField] private int defaultFirstClearCoins = 24;
        [SerializeField] private int defaultReplayCoins;
        [SerializeField] private float premiumMembershipCoinMultiplier = 1f;

        [Header("Yildiz Kurallari")]
        [SerializeField] private int maxStars = 3;
        [SerializeField] private int zeroMistakeStars = 3;
        [SerializeField] private int oneMistakeStars = 2;
        [SerializeField] private int twoOrMoreMistakesStars = 1;
        [SerializeField] private int maxStarsAfterContinue = 1;

        [Header("Coin Carpanlari")]
        [SerializeField] private float threeStarCoinMultiplier = 1f;
        [SerializeField] private float twoStarCoinMultiplier = 0.70f;
        [SerializeField] private float oneStarCoinMultiplier = 0.45f;

        [Header("Reklam Hook")]
        [SerializeField] private bool enableAdCatchupHook = true;
        [SerializeField] private bool adCatchupOnlyOnFirstClear = true;
        [SerializeField] private int adCatchupTargetStars = 3;
        [SerializeField] private float adCatchupBonusMultiplier = 1f;

        [Header("HUD ve Store Hook")]
        [SerializeField] private bool showGameplayCoinHook = true;
        [SerializeField] private bool showUpcomingThemesTeaser = true;

        [Header("Dogrudan Ayarlanabilir Listeler")]
        [SerializeField] private List<LevelCoinOverride> levelCoinOverrides = new List<LevelCoinOverride>();
        [SerializeField] private List<ThemeOfferTuning> themeOffers = new List<ThemeOfferTuning>();
        [SerializeField] private List<RegionalPricePreview> regionalPricePreviews = new List<RegionalPricePreview>();

        public bool AwardCoinsOnlyOnFirstCompletion => awardCoinsOnlyOnFirstCompletion;
        public int DefaultFirstClearCoins => defaultFirstClearCoins;
        public int DefaultReplayCoins => defaultReplayCoins;
        public bool ShowGameplayCoinHook => showGameplayCoinHook;
        public bool ShowUpcomingThemesTeaser => showUpcomingThemesTeaser;
        public IReadOnlyList<LevelCoinOverride> LevelCoinOverrides => levelCoinOverrides;
        public IReadOnlyList<ThemeOfferTuning> ThemeOffers => themeOffers;
        public IReadOnlyList<RegionalPricePreview> RegionalPricePreviews => regionalPricePreviews;

        public static string GetResourcePathForMode(TestPlayerMode mode)
        {
            switch (mode)
            {
                case TestPlayerMode.FreePlayer:
                    return FreeSandboxResourcePath;
                case TestPlayerMode.PremiumPlayer:
                    return PremiumSandboxResourcePath;
                default:
                    return DefaultResourcePath;
            }
        }

        public void ResetToDefaults()
        {
            awardCoinsOnlyOnFirstCompletion = true;
            defaultFirstClearCoins = 24;
            defaultReplayCoins = 0;
            premiumMembershipCoinMultiplier = 1f;
            maxStars = 3;
            zeroMistakeStars = 3;
            oneMistakeStars = 2;
            twoOrMoreMistakesStars = 1;
            maxStarsAfterContinue = 1;
            threeStarCoinMultiplier = 1f;
            twoStarCoinMultiplier = 0.70f;
            oneStarCoinMultiplier = 0.45f;
            enableAdCatchupHook = true;
            adCatchupOnlyOnFirstClear = true;
            adCatchupTargetStars = 3;
            adCatchupBonusMultiplier = 1f;
            showGameplayCoinHook = true;
            showUpcomingThemesTeaser = true;

            levelCoinOverrides = new List<LevelCoinOverride>();
            themeOffers = new List<ThemeOfferTuning>
            {
                new ThemeOfferTuning()
            };
            regionalPricePreviews = new List<RegionalPricePreview>
            {
                new RegionalPricePreview { regionCode = "TR", currencyCode = "TRY", membershipPrice = "89.99", themePackPrice = "44.99", notes = "Taslak tier. Store fiyatlari daha sonra kesinlesecek." },
                new RegionalPricePreview { regionCode = "DE", currencyCode = "EUR", membershipPrice = "7.99", themePackPrice = "3.99", notes = "Taslak tier. Dil degil storefront bolgesi esas alinacak." },
                new RegionalPricePreview { regionCode = "ES", currencyCode = "EUR", membershipPrice = "7.99", themePackPrice = "3.99", notes = "Taslak tier. Nihai fiyat market cikisina yakin belirlenecek." },
                new RegionalPricePreview { regionCode = "US", currencyCode = "USD", membershipPrice = "7.99", themePackPrice = "3.99", notes = "Taslak tier. Nihai fiyat store eksperimenti sonrasi ayarlanacak." }
            };
        }

        public void EnsureDefaults()
        {
            EnsureCollections();

            bool hasDefaultThemeOffer = false;
            for (int i = 0; i < themeOffers.Count; i++)
            {
                ThemeOfferTuning offer = themeOffers[i];
                if (offer != null && string.Equals(offer.themeId, GameConstants.PremiumMythologyThemeId, StringComparison.OrdinalIgnoreCase))
                {
                    hasDefaultThemeOffer = true;
                    break;
                }
            }

            if (!hasDefaultThemeOffer)
            {
                themeOffers.Add(new ThemeOfferTuning
                {
                    themeId = GameConstants.PremiumMythologyThemeId,
                    softCurrencyPriceOverride = 1200,
                    iapTierId = "theme_small"
                });
            }
        }

        public void EnsureLevelEntries(IEnumerable<int> levelIds)
        {
            EnsureDefaults();
            if (levelIds == null)
            {
                return;
            }

            foreach (int levelId in levelIds)
            {
                if (levelId <= 0 || GetLevelCoinOverride(levelId) != null)
                {
                    continue;
                }

                levelCoinOverrides.Add(new LevelCoinOverride
                {
                    levelId = levelId,
                    firstClearCoins = defaultFirstClearCoins,
                    replayCoins = defaultReplayCoins
                });
            }

            levelCoinOverrides.Sort((a, b) => a.levelId.CompareTo(b.levelId));
        }

        public int ResolveStars(int mistakeCount, bool continueUsed)
        {
            int stars = mistakeCount <= 0
                ? zeroMistakeStars
                : mistakeCount == 1
                    ? oneMistakeStars
                    : twoOrMoreMistakesStars;

            if (continueUsed)
            {
                stars = Mathf.Min(stars, Mathf.Max(1, maxStarsAfterContinue));
            }

            return Mathf.Clamp(stars, 1, Mathf.Max(1, maxStars));
        }

        public int ResolveBaseCoinReward(int levelId, bool firstClear)
        {
            LevelCoinOverride overrideData = GetLevelCoinOverride(levelId);
            if (overrideData != null)
            {
                return Mathf.Max(0, firstClear ? overrideData.firstClearCoins : overrideData.replayCoins);
            }

            return Mathf.Max(0, firstClear ? defaultFirstClearCoins : defaultReplayCoins);
        }

        public int ResolveCoinReward(int baseCoins, int starsEarned, bool premiumMembershipActive)
        {
            float starMultiplier = ResolveStarCoinMultiplier(starsEarned);
            float premiumMultiplier = premiumMembershipActive ? Mathf.Max(0.1f, premiumMembershipCoinMultiplier) : 1f;
            return Mathf.Max(0, Mathf.RoundToInt(Mathf.Max(0, baseCoins) * starMultiplier * premiumMultiplier));
        }

        public int ResolveAdCatchupBonus(int baseCoins, int starsEarned, bool firstClear, bool premiumMembershipActive)
        {
            if (!enableAdCatchupHook)
            {
                return 0;
            }

            if (adCatchupOnlyOnFirstClear && !firstClear)
            {
                return 0;
            }

            if (starsEarned >= Mathf.Clamp(adCatchupTargetStars, 1, maxStars))
            {
                return 0;
            }

            int currentReward = ResolveCoinReward(baseCoins, starsEarned, premiumMembershipActive);
            int catchupTargetReward = ResolveCoinReward(baseCoins, adCatchupTargetStars, premiumMembershipActive);
            int delta = Mathf.Max(0, catchupTargetReward - currentReward);
            return Mathf.Max(0, Mathf.RoundToInt(delta * Mathf.Max(0f, adCatchupBonusMultiplier)));
        }

        public int ResolveThemeSoftCurrencyPrice(string themeId, int fallbackPrice)
        {
            ThemeOfferTuning offer = GetThemeOffer(themeId);
            if (offer == null || !offer.allowCoinPurchase)
            {
                return fallbackPrice;
            }

            return Mathf.Max(0, offer.softCurrencyPriceOverride);
        }

        public ThemeOfferTuning GetThemeOffer(string themeId)
        {
            EnsureCollections();
            for (int i = 0; i < themeOffers.Count; i++)
            {
                ThemeOfferTuning offer = themeOffers[i];
                if (offer != null && string.Equals(offer.themeId, themeId, StringComparison.OrdinalIgnoreCase))
                {
                    return offer;
                }
            }

            return null;
        }

        private float ResolveStarCoinMultiplier(int starsEarned)
        {
            switch (Mathf.Clamp(starsEarned, 1, maxStars))
            {
                case 3:
                    return Mathf.Max(0f, threeStarCoinMultiplier);
                case 2:
                    return Mathf.Max(0f, twoStarCoinMultiplier);
                default:
                    return Mathf.Max(0f, oneStarCoinMultiplier);
            }
        }

        private LevelCoinOverride GetLevelCoinOverride(int levelId)
        {
            EnsureCollections();
            for (int i = 0; i < levelCoinOverrides.Count; i++)
            {
                LevelCoinOverride entry = levelCoinOverrides[i];
                if (entry != null && entry.levelId == levelId)
                {
                    return entry;
                }
            }

            return null;
        }

        private void EnsureThemeOffer(string themeId, int softCurrencyPrice, string iapTierId)
        {
            EnsureCollections();
            if (GetThemeOffer(themeId) != null)
            {
                return;
            }

            themeOffers.Add(new ThemeOfferTuning
            {
                themeId = themeId,
                softCurrencyPriceOverride = softCurrencyPrice,
                iapTierId = iapTierId
            });
        }

        private void EnsureCollections()
        {
            if (levelCoinOverrides == null)
            {
                levelCoinOverrides = new List<LevelCoinOverride>();
            }

            if (themeOffers == null)
            {
                themeOffers = new List<ThemeOfferTuning>();
            }

            if (regionalPricePreviews == null)
            {
                regionalPricePreviews = new List<RegionalPricePreview>();
            }
        }
    }
}
