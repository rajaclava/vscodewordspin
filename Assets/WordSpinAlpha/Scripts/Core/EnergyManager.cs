using System;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Core
{
    public class EnergyManager : Singleton<EnergyManager>
    {
        private EnergyConfigDefinition _config;

        public int MaxEnergy => Mathf.Max(1, _config != null ? _config.maxEnergy : GameConstants.DefaultMaxEnergy);
        public int CurrentEnergy => SaveManager.Instance != null ? SaveManager.Instance.Data.energy.currentEnergy : MaxEnergy;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            if (ContentService.Instance != null)
            {
                _config = ContentService.Instance.LoadEnergyConfig();
            }

            EnsureInitialized();
            RefillFromElapsedTime();
        }

        private void EnsureInitialized()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            if (SaveManager.Instance.Data.energy.currentEnergy <= 0)
            {
                SaveManager.Instance.Data.energy.currentEnergy = MaxEnergy;
            }

            if (SaveManager.Instance.Data.energy.lastRefillUtcTicks <= 0)
            {
                SaveManager.Instance.Data.energy.lastRefillUtcTicks = DateTime.UtcNow.Ticks;
            }

            SaveManager.Instance.Save();
            GameEvents.RaiseEntryEnergyChanged(CurrentEnergy, MaxEnergy);
        }

        public void RefillFromElapsedTime()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            if (EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive && _config != null && _config.bypassForPremiumMembership)
            {
                SaveManager.Instance.Data.energy.currentEnergy = MaxEnergy;
                SaveManager.Instance.Save();
                GameEvents.RaiseEntryEnergyChanged(CurrentEnergy, MaxEnergy);
                return;
            }

            DateTime lastRefill = new DateTime(SaveManager.Instance.Data.energy.lastRefillUtcTicks, DateTimeKind.Utc);
            TimeSpan elapsed = DateTime.UtcNow - lastRefill;
            int refillMinutes = Mathf.Max(1, _config != null ? _config.refillMinutes : GameConstants.DefaultEnergyRefillMinutes);
            int refillCount = Mathf.FloorToInt((float)elapsed.TotalMinutes / refillMinutes);
            if (refillCount <= 0)
            {
                return;
            }

            SaveManager.Instance.Data.energy.currentEnergy = Mathf.Min(MaxEnergy, SaveManager.Instance.Data.energy.currentEnergy + refillCount);
            SaveManager.Instance.Data.energy.lastRefillUtcTicks = DateTime.UtcNow.Ticks;
            SaveManager.Instance.Save();
            GameEvents.RaiseEntryEnergyChanged(CurrentEnergy, MaxEnergy);
        }

        public bool TryConsumeEntryEnergy()
        {
            RefillFromElapsedTime();

            if (EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive && _config != null && _config.bypassForPremiumMembership)
            {
                GameEvents.RaiseEntryEnergyChanged(MaxEnergy, MaxEnergy);
                return true;
            }

            if (SaveManager.Instance == null || SaveManager.Instance.Data.energy.currentEnergy <= 0)
            {
                return false;
            }

            SaveManager.Instance.Data.energy.currentEnergy--;
            SaveManager.Instance.Data.energy.lastRefillUtcTicks = DateTime.UtcNow.Ticks;
            SaveManager.Instance.Save();
            GameEvents.RaiseEntryEnergyChanged(CurrentEnergy, MaxEnergy);
            GameEvents.RaiseMetric("energySpent", "{}");
            return true;
        }

        public void GrantEnergy(int amount)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.energy.currentEnergy = Mathf.Min(MaxEnergy, SaveManager.Instance.Data.energy.currentEnergy + Mathf.Max(0, amount));
            SaveManager.Instance.Data.energy.lastRefillUtcTicks = DateTime.UtcNow.Ticks;
            SaveManager.Instance.Save();
            GameEvents.RaiseEntryEnergyChanged(CurrentEnergy, MaxEnergy);
        }
    }
}
