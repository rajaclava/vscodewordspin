using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class TestPlayerModeManager : Singleton<TestPlayerModeManager>
    {
        [SerializeField] private TestPlayerModeProfile profile;

        public TestPlayerModeProfile Profile
        {
            get
            {
                EnsureProfile();
                return profile;
            }
        }

        public TestPlayerMode ActiveMode => Profile != null ? Profile.ActiveMode : TestPlayerMode.Default;
        public TestPlayerMode AppliedMode => Profile != null ? Profile.LastAppliedMode : TestPlayerMode.Default;

        protected override bool PersistAcrossScenes => true;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            EnsureProfile();
        }

        public void RefreshProfileCache()
        {
            profile = Resources.Load<TestPlayerModeProfile>(TestPlayerModeProfile.DefaultResourcePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<TestPlayerModeProfile>();
                profile.ResetToDefaults();
            }

            profile.EnsureDefaults();
        }

        public bool TryGetPremiumMembershipOverride(out bool value)
        {
            TestPlayerModeProfile.ModeTuning tuning = Profile != null ? Profile.ResolveMode(AppliedMode) : null;
            if (tuning != null && tuning.overrideMembership)
            {
                value = tuning.premiumMembershipActive;
                return true;
            }

            value = false;
            return false;
        }

        public bool TryGetNoAdsOverride(out bool value)
        {
            TestPlayerModeProfile.ModeTuning tuning = Profile != null ? Profile.ResolveMode(AppliedMode) : null;
            if (tuning != null && tuning.overrideMembership)
            {
                value = tuning.noAdsOwned;
                return true;
            }

            value = false;
            return false;
        }

        public bool TryGetEnergyOverride(out int maxEnergy, out int refillMinutes, out bool bypassEntryEnergy)
        {
            TestPlayerModeProfile.ModeTuning tuning = Profile != null ? Profile.ResolveMode(AppliedMode) : null;
            if (tuning != null && tuning.overrideEnergyRules)
            {
                maxEnergy = Mathf.Max(1, tuning.maxEnergy);
                refillMinutes = Mathf.Max(1, tuning.refillMinutes);
                bypassEntryEnergy = tuning.bypassEntryEnergy;
                return true;
            }

            maxEnergy = 0;
            refillMinutes = 0;
            bypassEntryEnergy = false;
            return false;
        }

        public bool TryGetQuestionHeartsOverride(out int hearts)
        {
            TestPlayerModeProfile.ModeTuning tuning = Profile != null ? Profile.ResolveMode(AppliedMode) : null;
            if (tuning != null && tuning.overrideQuestionHearts)
            {
                hearts = Mathf.Max(1, tuning.questionHearts);
                return true;
            }

            hearts = 0;
            return false;
        }

        public bool RequiresRewardedContinue()
        {
            TestPlayerModeProfile.ModeTuning tuning = Profile != null ? Profile.ResolveMode(AppliedMode) : null;
            if (tuning == null)
            {
                return false;
            }

            if (tuning.overrideMembership && tuning.premiumMembershipActive)
            {
                return false;
            }

            return tuning.requireRewardedContinue;
        }

        public int RewardedContinueSeconds()
        {
            TestPlayerModeProfile.ModeTuning tuning = Profile != null ? Profile.ResolveMode(AppliedMode) : null;
            return tuning != null ? Mathf.Max(1, tuning.rewardedAdSeconds) : 30;
        }

        public void ApplyActiveModeToSaveState()
        {
            if (SaveManager.Instance == null || Profile == null)
            {
                return;
            }

            TestPlayerMode previousMode = Profile.LastAppliedMode;
            TestPlayerMode targetMode = ActiveMode;

            CaptureCurrentModeSnapshot(previousMode);
            RestoreModeSnapshot(targetMode);
            ApplyModeAdjustments(targetMode);

            Profile.LastAppliedMode = targetMode;
            SaveManager.Instance.Save();
            LevelEconomyManager.Instance?.RefreshProfileCache();
            EconomyManager.Instance?.RefreshEconomyProfile();
            QuestionLifeManager.Instance?.RefreshForTesting(true);
            BroadcastRuntimeState();
        }

        public void CaptureCurrentModeSnapshot()
        {
            CaptureCurrentModeSnapshot(Profile != null ? Profile.LastAppliedMode : TestPlayerMode.Default);
        }

        private void ApplyModeAdjustments(TestPlayerMode mode)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            switch (mode)
            {
                case TestPlayerMode.FreePlayer:
                    SaveManager.Instance.Data.membership.premiumMembershipActive = false;
                    SaveManager.Instance.Data.membership.noAdsOwned = false;
                    SaveManager.Instance.Data.themes.unlockedThemes.Clear();
                    SaveManager.Instance.Data.themes.unlockedThemes.Add(GameConstants.DefaultThemeId);
                    SaveManager.Instance.Data.themes.activeThemeId = GameConstants.DefaultThemeId;
                    SaveManager.Instance.Data.energy.currentEnergy = ResolveTargetEnergyForMode();
                    SaveManager.Instance.Data.energy.lastRefillUtcTicks = System.DateTime.UtcNow.Ticks;
                    break;

                case TestPlayerMode.PremiumPlayer:
                    SaveManager.Instance.Data.membership.premiumMembershipActive = true;
                    SaveManager.Instance.Data.membership.noAdsOwned = true;
                    SaveManager.Instance.Data.energy.currentEnergy = ResolveTargetEnergyForMode();
                    SaveManager.Instance.Data.energy.lastRefillUtcTicks = System.DateTime.UtcNow.Ticks;
                    break;

                default:
                    break;
            }
        }

        public void ResetPendingScoreAndSession()
        {
            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.Data.session.pendingFailResolution = false;
                SaveManager.Instance.Data.session.pendingInfoCard = false;
                SaveManager.Instance.Data.session.pendingInfoCardId = string.Empty;
                SaveManager.Instance.Data.session.pendingQuestionAdvanceAfterInfoCard = false;
                SaveManager.Instance.Data.session.pendingLevelCompleteAfterInfoCard = false;
                SaveManager.Instance.Data.session.pendingLevelResult = false;
                SaveManager.Instance.Data.session.pendingResultLevelId = 0;
                SaveManager.Instance.Data.session.pendingResultTotalScore = 0;
                SaveManager.Instance.Data.session.pendingResultHitScore = 0;
                SaveManager.Instance.Data.session.pendingResultClearScore = 0;
                SaveManager.Instance.Data.session.pendingResultBestMultiplier = 0f;
                SaveManager.Instance.Data.session.pendingResultStars = 0;
                SaveManager.Instance.Data.session.pendingResultCoinReward = 0;
                SaveManager.Instance.Data.session.pendingResultAdBonusCoins = 0;
                SaveManager.Instance.Data.session.pendingResultAdBonusEligible = false;
                SaveManager.Instance.Data.session.currentScoreTotal = 0;
                SaveManager.Instance.Data.session.currentHitScore = 0;
                SaveManager.Instance.Data.session.currentPerfectStreak = 0;
                SaveManager.Instance.Data.session.currentMultiplier = 1f;
                SaveManager.Instance.Data.session.currentBestMultiplier = 1f;
                SaveManager.Instance.Data.session.currentMistakeCount = 0;
                SaveManager.Instance.Data.session.currentPerfectHits = 0;
                SaveManager.Instance.Data.session.currentGoodHits = 0;
                SaveManager.Instance.Data.session.currentLevelElapsedSeconds = 0f;
                SaveManager.Instance.Data.session.currentTargetShownElapsedSeconds = -1f;
                SaveManager.Instance.Data.session.currentLastSuccessfulHitElapsedSeconds = -1f;
                SaveManager.Instance.Data.session.usedContinueInCurrentLevel = false;
                SaveManager.Instance.Data.session.questionHeartsRemaining = 0;
                SaveManager.Instance.Data.session.hasActiveSession = false;
                SaveManager.Instance.Save();
            }

            ScoreManager.Instance?.ResetForTesting();
            CaptureCurrentModeSnapshot();
            BroadcastRuntimeState();
        }

        public void ResetSoftCurrency()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.economy.softCurrency = 0;
            SaveManager.Instance.Data.economy.startingSoftCurrencyGranted = true;
            SaveManager.Instance.Save();
            CaptureCurrentModeSnapshot();
            BroadcastRuntimeState();
        }

        public void ResetThemeOwnership()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.themes.unlockedThemes.Clear();
            SaveManager.Instance.Data.themes.unlockedThemes.Add(GameConstants.DefaultThemeId);
            SaveManager.Instance.Data.themes.activeThemeId = GameConstants.DefaultThemeId;
            SaveManager.Instance.Save();
            CaptureCurrentModeSnapshot();
            BroadcastRuntimeState();
        }

        public void ResetMembershipFlags()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.membership.noAdsOwned = false;
            SaveManager.Instance.Data.membership.premiumMembershipActive = false;
            SaveManager.Instance.Save();
            CaptureCurrentModeSnapshot();
            BroadcastRuntimeState();
        }

        public void FillEnergyForCurrentMode()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.energy.currentEnergy = ResolveTargetEnergyForMode();
            SaveManager.Instance.Data.energy.lastRefillUtcTicks = System.DateTime.UtcNow.Ticks;
            SaveManager.Instance.Save();
            CaptureCurrentModeSnapshot();
            BroadcastRuntimeState();
        }

        public void BroadcastRuntimeState()
        {
            GameEvents.RaiseMembershipChanged(EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive);
            GameEvents.RaiseEntryEnergyChanged(EnergyManager.Instance != null ? EnergyManager.Instance.CurrentEnergy : 0, EnergyManager.Instance != null ? EnergyManager.Instance.MaxEnergy : 0);
            GameEvents.RaiseQuestionHeartsChanged(QuestionLifeManager.Instance != null ? QuestionLifeManager.Instance.CurrentHearts : 0);
            GameEvents.RaiseSoftCurrencyChanged(EconomyManager.Instance != null ? EconomyManager.Instance.SoftCurrency : 0, 0);
        }

        private int ResolveTargetEnergyForMode()
        {
            if (TryGetEnergyOverride(out int maxEnergy, out _, out bool bypassEntryEnergy))
            {
                return bypassEntryEnergy ? maxEnergy : maxEnergy;
            }

            return EnergyManager.Instance != null ? EnergyManager.Instance.MaxEnergy : GameConstants.DefaultMaxEnergy;
        }

        private void CaptureCurrentModeSnapshot(TestPlayerMode mode)
        {
            if (SaveManager.Instance == null || Profile == null)
            {
                return;
            }

            string json = JsonUtility.ToJson(SaveManager.Instance.Data);
            Profile.SetSaveSnapshotJson(mode, json);
        }

        private void RestoreModeSnapshot(TestPlayerMode mode)
        {
            if (SaveManager.Instance == null || Profile == null)
            {
                return;
            }

            string snapshotJson = Profile.GetSaveSnapshotJson(mode);
            if (string.IsNullOrWhiteSpace(snapshotJson))
            {
                if (mode == TestPlayerMode.Default)
                {
                    return;
                }

                snapshotJson = Profile.GetSaveSnapshotJson(TestPlayerMode.Default);
            }

            if (string.IsNullOrWhiteSpace(snapshotJson))
            {
                return;
            }

            PlayerSaveData restored = JsonUtility.FromJson<PlayerSaveData>(snapshotJson);
            if (restored != null)
            {
                SaveManager.Instance.ReplaceData(restored);
            }
        }

        private void EnsureProfile()
        {
            if (profile != null)
            {
                profile.EnsureDefaults();
                return;
            }

            profile = Resources.Load<TestPlayerModeProfile>(TestPlayerModeProfile.DefaultResourcePath);
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<TestPlayerModeProfile>();
                profile.ResetToDefaults();
            }

            profile.EnsureDefaults();
        }
    }
}
