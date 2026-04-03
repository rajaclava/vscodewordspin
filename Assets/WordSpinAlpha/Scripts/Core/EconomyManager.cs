using System.Collections.Generic;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Core
{
    public class EconomyManager : Singleton<EconomyManager>
    {
        private const bool ForceDisablePremiumForValidation = false;

        private StoreCatalogDefinition _storeCatalog;
        private MembershipProfileDefinition _membershipProfile;
        private EconomyBalanceProfile _economyBalanceProfile;
        private TestPlayerMode _loadedEconomyProfileMode = (TestPlayerMode)(-1);

        public bool PremiumMembershipActive
        {
            get
            {
                if (ForceDisablePremiumForValidation)
                {
                    return false;
                }

                if (TestPlayerModeManager.Instance != null &&
                    TestPlayerModeManager.Instance.TryGetPremiumMembershipOverride(out bool overrideValue))
                {
                    return overrideValue;
                }

                return SaveManager.Instance != null && SaveManager.Instance.Data.membership.premiumMembershipActive;
            }
        }

        public bool NoAdsOwned
        {
            get
            {
                if (TestPlayerModeManager.Instance != null &&
                    TestPlayerModeManager.Instance.TryGetNoAdsOverride(out bool overrideValue))
                {
                    return overrideValue;
                }

                return SaveManager.Instance != null && SaveManager.Instance.Data.membership.noAdsOwned;
            }
        }
        public int SoftCurrency => SaveManager.Instance != null ? SaveManager.Instance.Data.economy.softCurrency : 0;
        public int Hints => SaveManager.Instance != null ? SaveManager.Instance.Data.economy.hints : 0;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            if (ContentService.Instance != null)
            {
                _storeCatalog = ContentService.Instance.LoadStoreCatalog();
                _membershipProfile = ContentService.Instance.LoadMembershipProfile();
            }

            InitializeDefaultEconomy();
            GameEvents.RaiseSoftCurrencyChanged(SoftCurrency, 0);
        }

        private void InitializeDefaultEconomy()
        {
            if (SaveManager.Instance == null || ContentService.Instance == null)
            {
                return;
            }

            EnergyConfigDefinition config = ContentService.Instance.LoadEnergyConfig();
            if (config == null)
            {
                return;
            }

            if (!SaveManager.Instance.Data.economy.startingHintsGranted)
            {
                SaveManager.Instance.Data.economy.hints = Mathf.Max(SaveManager.Instance.Data.economy.hints, Mathf.Max(0, config.startingHints));
                SaveManager.Instance.Data.economy.startingHintsGranted = true;
            }

            if (!SaveManager.Instance.Data.economy.startingSoftCurrencyGranted)
            {
                SaveManager.Instance.Data.economy.softCurrency = Mathf.Max(SaveManager.Instance.Data.economy.softCurrency, Mathf.Max(0, config.startingSoftCurrency));
                SaveManager.Instance.Data.economy.startingSoftCurrencyGranted = true;
            }

            SaveManager.Instance.Save();
        }

        public void GrantHints(int amount)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.economy.hints += Mathf.Max(0, amount);
            SaveManager.Instance.Save();
        }

        public bool TrySpendHint()
        {
            if (SaveManager.Instance == null || SaveManager.Instance.Data.economy.hints <= 0)
            {
                return false;
            }

            SaveManager.Instance.Data.economy.hints--;
            SaveManager.Instance.Save();
            GameEvents.RaiseMetric("hintUsed", "{}");
            return true;
        }

        public bool TryUnlockThemeWithSoftCurrency(string themeId)
        {
            if (_storeCatalog == null || SaveManager.Instance == null)
            {
                return false;
            }

            foreach (ThemePriceDefinition price in _storeCatalog.themes ?? new ThemePriceDefinition[0])
            {
                if (price.themeId != themeId)
                {
                    continue;
                }

                int resolvedPrice = ResolveThemeSoftCurrencyPrice(themeId, price.softCurrencyPrice);
                if (SaveManager.Instance.Data.economy.softCurrency < resolvedPrice)
                {
                    return false;
                }

                SaveManager.Instance.Data.economy.softCurrency -= resolvedPrice;
                UnlockTheme(themeId);
                SaveManager.Instance.Save();
                GameEvents.RaiseSoftCurrencyChanged(SoftCurrency, -resolvedPrice);
                return true;
            }

            return false;
        }

        public void UnlockTheme(string themeId)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            List<string> unlockedThemes = SaveManager.Instance.Data.themes.unlockedThemes;
            if (!unlockedThemes.Contains(themeId))
            {
                unlockedThemes.Add(themeId);
            }

            SaveManager.Instance.Save();
            GameEvents.RaiseThemeUnlocked(themeId);
        }

        public bool IsThemeUnlocked(string themeId)
        {
            if (string.IsNullOrEmpty(themeId))
            {
                return false;
            }

            if (themeId == GameConstants.DefaultThemeId)
            {
                return true;
            }

            if (PremiumMembershipActive && _membershipProfile != null && _membershipProfile.unlockFutureThemes)
            {
                return true;
            }

            return SaveManager.Instance != null && SaveManager.Instance.Data.themes.unlockedThemes.Contains(themeId);
        }

        public void SetActiveTheme(string themeId)
        {
            if (SaveManager.Instance == null || !IsThemeUnlocked(themeId))
            {
                return;
            }

            SaveManager.Instance.Data.themes.activeThemeId = themeId;
            SaveManager.Instance.Save();
        }

        public void SetPremiumMembership(bool value)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.membership.premiumMembershipActive = value;
            SaveManager.Instance.Save();
            GameEvents.RaiseMembershipChanged(PremiumMembershipActive);
        }

        public void SetNoAdsOwned(bool value)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.membership.noAdsOwned = value;
            SaveManager.Instance.Save();
            GameEvents.RaiseMembershipChanged(PremiumMembershipActive);
        }

        public void GrantSoftCurrency(int amount, string metricReason = null)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            int delta = Mathf.Max(0, amount);
            if (delta <= 0)
            {
                return;
            }

            SaveManager.Instance.Data.economy.softCurrency += delta;
            SaveManager.Instance.Save();
            GameEvents.RaiseSoftCurrencyChanged(SoftCurrency, delta);

            if (!string.IsNullOrWhiteSpace(metricReason))
            {
                GameEvents.RaiseMetric("softCurrencyGranted", $"{{\"reason\":\"{metricReason}\",\"amount\":{delta}}}");
            }
        }

        public void RefreshEconomyProfile()
        {
            _economyBalanceProfile = null;
            _loadedEconomyProfileMode = (TestPlayerMode)(-1);
            EnsureEconomyBalanceProfile();
        }

        private int ResolveThemeSoftCurrencyPrice(string themeId, int fallbackPrice)
        {
            EnsureEconomyBalanceProfile();
            return _economyBalanceProfile != null
                ? _economyBalanceProfile.ResolveThemeSoftCurrencyPrice(themeId, fallbackPrice)
                : fallbackPrice;
        }

        private void EnsureEconomyBalanceProfile()
        {
            TestPlayerMode desiredMode = TestPlayerModeManager.Instance != null ? TestPlayerModeManager.Instance.AppliedMode : TestPlayerMode.Default;
            if (_economyBalanceProfile != null && _loadedEconomyProfileMode == desiredMode)
            {
                _economyBalanceProfile.EnsureDefaults();
                return;
            }

            _economyBalanceProfile = Resources.Load<EconomyBalanceProfile>(EconomyBalanceProfile.GetResourcePathForMode(desiredMode));
            if (_economyBalanceProfile == null && desiredMode != TestPlayerMode.Default)
            {
                _economyBalanceProfile = Resources.Load<EconomyBalanceProfile>(EconomyBalanceProfile.GetResourcePathForMode(TestPlayerMode.Default));
            }
            if (_economyBalanceProfile != null)
            {
                _loadedEconomyProfileMode = desiredMode;
                _economyBalanceProfile.EnsureDefaults();
            }
        }
    }
}
