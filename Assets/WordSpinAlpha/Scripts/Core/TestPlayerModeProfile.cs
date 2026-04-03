using System;
using UnityEngine;

namespace WordSpinAlpha.Core
{
    public enum TestPlayerMode
    {
        Default = 0,
        FreePlayer = 1,
        PremiumPlayer = 2
    }

    [CreateAssetMenu(fileName = "TestPlayerModeProfile", menuName = "WordSpin Alpha/Test Player Mode Profile")]
    public class TestPlayerModeProfile : ScriptableObject
    {
        public const string DefaultResourcePath = "Configs/TestPlayerModeProfile";

        [Serializable]
        public class ModeTuning
        {
            public bool overrideMembership;
            public bool premiumMembershipActive;
            public bool noAdsOwned;
            public bool overrideEnergyRules;
            public int maxEnergy = 5;
            public int refillMinutes = 10;
            public bool bypassEntryEnergy;
            public bool overrideQuestionHearts;
            public int questionHearts = GameConstants.DefaultQuestionHearts;
            public bool requireRewardedContinue;
            public int rewardedAdSeconds = 30;
        }

        [SerializeField] private TestPlayerMode activeMode = TestPlayerMode.Default;
        [SerializeField, HideInInspector] private TestPlayerMode lastAppliedMode = TestPlayerMode.Default;
        [SerializeField, HideInInspector] private string defaultModeSaveSnapshotJson = string.Empty;
        [SerializeField, HideInInspector] private string freeModeSaveSnapshotJson = string.Empty;
        [SerializeField, HideInInspector] private string premiumModeSaveSnapshotJson = string.Empty;
        [SerializeField] private ModeTuning freePlayer = new ModeTuning
        {
            overrideMembership = true,
            premiumMembershipActive = false,
            noAdsOwned = false,
            overrideEnergyRules = true,
            maxEnergy = 5,
            refillMinutes = 10,
            bypassEntryEnergy = false,
            overrideQuestionHearts = true,
            questionHearts = GameConstants.DefaultQuestionHearts,
            requireRewardedContinue = true,
            rewardedAdSeconds = 30
        };
        [SerializeField] private ModeTuning premiumPlayer = new ModeTuning
        {
            overrideMembership = true,
            premiumMembershipActive = true,
            noAdsOwned = true,
            overrideEnergyRules = true,
            maxEnergy = GameConstants.DefaultMaxEnergy,
            refillMinutes = GameConstants.DefaultEnergyRefillMinutes,
            bypassEntryEnergy = true,
            overrideQuestionHearts = true,
            questionHearts = 999,
            requireRewardedContinue = false,
            rewardedAdSeconds = 0
        };

        public TestPlayerMode ActiveMode
        {
            get => activeMode;
            set => activeMode = value;
        }

        public TestPlayerMode LastAppliedMode
        {
            get => lastAppliedMode;
            set => lastAppliedMode = value;
        }

        public ModeTuning FreePlayer => freePlayer;
        public ModeTuning PremiumPlayer => premiumPlayer;

        public void ResetToDefaults()
        {
            activeMode = TestPlayerMode.Default;
            freePlayer = new ModeTuning
            {
                overrideMembership = true,
                premiumMembershipActive = false,
                noAdsOwned = false,
                overrideEnergyRules = true,
                maxEnergy = 5,
                refillMinutes = 10,
                bypassEntryEnergy = false,
                overrideQuestionHearts = true,
                questionHearts = GameConstants.DefaultQuestionHearts,
                requireRewardedContinue = true,
                rewardedAdSeconds = 30
            };
            premiumPlayer = new ModeTuning
            {
                overrideMembership = true,
                premiumMembershipActive = true,
                noAdsOwned = true,
                overrideEnergyRules = true,
                maxEnergy = GameConstants.DefaultMaxEnergy,
                refillMinutes = GameConstants.DefaultEnergyRefillMinutes,
                bypassEntryEnergy = true,
                overrideQuestionHearts = true,
                questionHearts = 999,
                requireRewardedContinue = false,
                rewardedAdSeconds = 0
            };
            lastAppliedMode = TestPlayerMode.Default;
            defaultModeSaveSnapshotJson = string.Empty;
            freeModeSaveSnapshotJson = string.Empty;
            premiumModeSaveSnapshotJson = string.Empty;
        }

        public void EnsureDefaults()
        {
            if (freePlayer == null)
            {
                freePlayer = new ModeTuning
                {
                    overrideMembership = true,
                    premiumMembershipActive = false,
                    noAdsOwned = false,
                    overrideEnergyRules = true,
                    maxEnergy = 5,
                    refillMinutes = 10,
                    bypassEntryEnergy = false,
                    overrideQuestionHearts = true,
                    questionHearts = GameConstants.DefaultQuestionHearts,
                    requireRewardedContinue = true,
                    rewardedAdSeconds = 30
                };
            }

            if (premiumPlayer == null)
            {
                premiumPlayer = new ModeTuning
                {
                    overrideMembership = true,
                    premiumMembershipActive = true,
                    noAdsOwned = true,
                    overrideEnergyRules = true,
                    maxEnergy = GameConstants.DefaultMaxEnergy,
                    refillMinutes = GameConstants.DefaultEnergyRefillMinutes,
                    bypassEntryEnergy = true,
                    overrideQuestionHearts = true,
                    questionHearts = 999,
                    requireRewardedContinue = false,
                    rewardedAdSeconds = 0
                };
            }

            freePlayer.maxEnergy = Mathf.Max(1, freePlayer.maxEnergy <= 0 ? 5 : freePlayer.maxEnergy);
            freePlayer.refillMinutes = Mathf.Max(1, freePlayer.refillMinutes <= 0 ? 10 : freePlayer.refillMinutes);
            freePlayer.questionHearts = Mathf.Max(1, freePlayer.questionHearts <= 0 ? GameConstants.DefaultQuestionHearts : freePlayer.questionHearts);
            freePlayer.rewardedAdSeconds = Mathf.Max(1, freePlayer.rewardedAdSeconds <= 0 ? 30 : freePlayer.rewardedAdSeconds);

            premiumPlayer.maxEnergy = Mathf.Max(1, premiumPlayer.maxEnergy <= 0 ? GameConstants.DefaultMaxEnergy : premiumPlayer.maxEnergy);
            premiumPlayer.refillMinutes = Mathf.Max(1, premiumPlayer.refillMinutes <= 0 ? GameConstants.DefaultEnergyRefillMinutes : premiumPlayer.refillMinutes);
            premiumPlayer.questionHearts = Mathf.Max(1, premiumPlayer.questionHearts <= 0 ? 999 : premiumPlayer.questionHearts);
            premiumPlayer.rewardedAdSeconds = Mathf.Max(0, premiumPlayer.rewardedAdSeconds);
        }

        public ModeTuning ResolveActiveMode()
        {
            return ResolveMode(activeMode);
        }

        public ModeTuning ResolveMode(TestPlayerMode mode)
        {
            EnsureDefaults();
            switch (mode)
            {
                case TestPlayerMode.FreePlayer:
                    return freePlayer;
                case TestPlayerMode.PremiumPlayer:
                    return premiumPlayer;
                default:
                    return null;
            }
        }

        public string GetSaveSnapshotJson(TestPlayerMode mode)
        {
            switch (mode)
            {
                case TestPlayerMode.FreePlayer:
                    return freeModeSaveSnapshotJson;
                case TestPlayerMode.PremiumPlayer:
                    return premiumModeSaveSnapshotJson;
                default:
                    return defaultModeSaveSnapshotJson;
            }
        }

        public void SetSaveSnapshotJson(TestPlayerMode mode, string json)
        {
            switch (mode)
            {
                case TestPlayerMode.FreePlayer:
                    freeModeSaveSnapshotJson = json ?? string.Empty;
                    break;
                case TestPlayerMode.PremiumPlayer:
                    premiumModeSaveSnapshotJson = json ?? string.Empty;
                    break;
                default:
                    defaultModeSaveSnapshotJson = json ?? string.Empty;
                    break;
            }
        }
    }
}
