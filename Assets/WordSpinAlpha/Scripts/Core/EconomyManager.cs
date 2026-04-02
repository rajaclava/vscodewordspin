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

        public bool PremiumMembershipActive => !ForceDisablePremiumForValidation && SaveManager.Instance != null && SaveManager.Instance.Data.membership.premiumMembershipActive;
        public bool NoAdsOwned => SaveManager.Instance != null && SaveManager.Instance.Data.membership.noAdsOwned;
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
        }

        private void InitializeDefaultEconomy()
        {
            if (SaveManager.Instance == null || ContentService.Instance == null)
            {
                return;
            }

            EnergyConfigDefinition config = ContentService.Instance.LoadEnergyConfig();
            SaveManager.Instance.Data.economy.hints = Mathf.Max(SaveManager.Instance.Data.economy.hints, config.startingHints);
            SaveManager.Instance.Data.economy.softCurrency = Mathf.Max(SaveManager.Instance.Data.economy.softCurrency, config.startingSoftCurrency);
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

                if (SaveManager.Instance.Data.economy.softCurrency < price.softCurrencyPrice)
                {
                    return false;
                }

                SaveManager.Instance.Data.economy.softCurrency -= price.softCurrencyPrice;
                UnlockTheme(themeId);
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
            GameEvents.RaiseMembershipChanged(value);
        }

        public void SetNoAdsOwned(bool value)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.membership.noAdsOwned = value;
            SaveManager.Instance.Save();
        }
    }
}
