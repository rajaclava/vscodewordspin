using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class LevelEconomyManager : Singleton<LevelEconomyManager>
    {
        [SerializeField] private EconomyBalanceProfile profile;

        private int _activeLevelId;
        private bool _continueUsedInLevel;
        private TestPlayerMode _loadedProfileMode = (TestPlayerMode)(-1);

        public EconomyBalanceProfile Profile
        {
            get
            {
                EnsureProfile();
                return profile;
            }
        }

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

        private void OnEnable()
        {
            GameEvents.LevelStarted += HandleLevelStarted;
            GameEvents.LevelContinueUsed += HandleLevelContinueUsed;
            GameEvents.LevelScoreFinalized += HandleLevelScoreFinalized;
        }

        private void OnDisable()
        {
            GameEvents.LevelStarted -= HandleLevelStarted;
            GameEvents.LevelContinueUsed -= HandleLevelContinueUsed;
            GameEvents.LevelScoreFinalized -= HandleLevelScoreFinalized;
        }

        public int ResolveThemeSoftCurrencyPrice(string themeId, int fallbackPrice)
        {
            return Profile != null ? Profile.ResolveThemeSoftCurrencyPrice(themeId, fallbackPrice) : fallbackPrice;
        }

        public void RefreshProfileCache()
        {
            _loadedProfileMode = (TestPlayerMode)(-1);
            EnsureProfile();
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<EconomyBalanceProfile>();
                profile.ResetToDefaults();
            }
            profile.EnsureDefaults();
        }

        private void HandleLevelStarted(LevelContext context)
        {
            _activeLevelId = context.levelId;
            _continueUsedInLevel = false;
        }

        private void HandleLevelContinueUsed(bool _)
        {
            _continueUsedInLevel = true;
        }

        private void HandleLevelScoreFinalized(LevelScoreSummaryData summary)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            EnsureProfile();
            if (profile == null)
            {
                return;
            }

            int levelId = summary.levelId > 0 ? summary.levelId : _activeLevelId;
            if (levelId <= 0)
            {
                return;
            }

            LevelRewardProgressState progress = GetOrCreateLevelRewardProgress(levelId);
            bool firstClear = !progress.firstClearCoinsGranted;
            bool eligibleForCoinReward = firstClear || !profile.AwardCoinsOnlyOnFirstCompletion;
            int baseCoins = eligibleForCoinReward ? profile.ResolveBaseCoinReward(levelId, firstClear) : 0;
            int starsEarned = profile.ResolveStars(summary.mistakeCount, _continueUsedInLevel);
            int coinReward = profile.ResolveCoinReward(baseCoins, starsEarned, EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive);
            int adBonusCoins = profile.ResolveAdCatchupBonus(baseCoins, starsEarned, firstClear, EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive);

            progress.bestStars = Mathf.Max(progress.bestStars, starsEarned);
            progress.completionCount++;
            progress.firstClearCoinsGranted |= firstClear;
            progress.totalCoinsEarned += coinReward;
            SaveManager.Instance.Save();

            if (coinReward > 0)
            {
                EconomyManager.Instance?.GrantSoftCurrency(coinReward, $"levelReward:{levelId}");
            }

            GameEvents.RaiseLevelEconomyFinalized(new LevelEconomySummaryData
            {
                levelId = levelId,
                starsEarned = starsEarned,
                baseCoinReward = baseCoins,
                coinReward = coinReward,
                adBonusCoins = adBonusCoins,
                adBonusEligible = adBonusCoins > 0,
                firstClear = firstClear,
                continueUsed = _continueUsedInLevel,
                completionCount = progress.completionCount
            });
        }

        private static LevelRewardProgressState GetOrCreateLevelRewardProgress(int levelId)
        {
            if (SaveManager.Instance == null)
            {
                return null;
            }

            if (SaveManager.Instance.Data.progress.levelRewards == null)
            {
                SaveManager.Instance.Data.progress.levelRewards = new System.Collections.Generic.List<LevelRewardProgressState>();
            }

            for (int i = 0; i < SaveManager.Instance.Data.progress.levelRewards.Count; i++)
            {
                LevelRewardProgressState entry = SaveManager.Instance.Data.progress.levelRewards[i];
                if (entry != null && entry.levelId == levelId)
                {
                    return entry;
                }
            }

            LevelRewardProgressState created = new LevelRewardProgressState { levelId = levelId };
            SaveManager.Instance.Data.progress.levelRewards.Add(created);
            return created;
        }

        private void EnsureProfile()
        {
            TestPlayerMode desiredMode = TestPlayerModeManager.Instance != null ? TestPlayerModeManager.Instance.AppliedMode : TestPlayerMode.Default;
            if (profile != null && _loadedProfileMode == desiredMode)
            {
                profile.EnsureDefaults();
                return;
            }

            profile = Resources.Load<EconomyBalanceProfile>(EconomyBalanceProfile.GetResourcePathForMode(desiredMode));
            if (profile == null && desiredMode != TestPlayerMode.Default)
            {
                profile = Resources.Load<EconomyBalanceProfile>(EconomyBalanceProfile.GetResourcePathForMode(TestPlayerMode.Default));
            }
            if (profile == null)
            {
                profile = ScriptableObject.CreateInstance<EconomyBalanceProfile>();
                profile.ResetToDefaults();
            }

            _loadedProfileMode = desiredMode;
            profile.EnsureDefaults();
        }
    }
}
