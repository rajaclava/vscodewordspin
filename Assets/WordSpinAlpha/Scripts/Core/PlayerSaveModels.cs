using System;
using System.Collections.Generic;
using UnityEngine;

namespace WordSpinAlpha.Core
{
    [Serializable]
    public class SessionSnapshot
    {
        public bool hasActiveSession;
        public bool pendingFailResolution;
        public bool pendingInfoCard;
        public string pendingInfoCardId;
        public bool pendingQuestionAdvanceAfterInfoCard;
        public bool pendingLevelCompleteAfterInfoCard;
        public bool pendingLevelResult;
        public bool usedContinueInCurrentLevel;
        public int pendingResultLevelId;
        public int pendingResultTotalScore;
        public int pendingResultHitScore;
        public int pendingResultClearScore;
        public float pendingResultBestMultiplier;
        public int pendingResultStars;
        public int pendingResultCoinReward;
        public int pendingResultAdBonusCoins;
        public bool pendingResultAdBonusEligible;
        public int currentScoreTotal;
        public int currentHitScore;
        public int currentPerfectStreak;
        public float currentMultiplier = 1f;
        public float currentBestMultiplier = 1f;
        public int currentMistakeCount;
        public int currentPerfectHits;
        public int currentGoodHits;
        public float currentLevelElapsedSeconds;
        public float currentTargetShownElapsedSeconds = -1f;
        public float currentLastSuccessfulHitElapsedSeconds = -1f;
        public int levelId;
        public int questionIndex;
        public int revealedLetters;
        public int currentTargetSlotIndex = -1;
        public int questionHeartsRemaining;
        public string campaignId;
        public string themeId;
        public string languageCode;
        public List<int> revealedSlotIndices = new List<int>();
        public List<Vector2> revealedTipLocalPoints = new List<Vector2>();
        public List<Vector2> revealedPinLocalPositions = new List<Vector2>();
        public List<float> revealedPinLocalRotations = new List<float>();
    }

    [Serializable]
    public class EnergyState
    {
        public int currentEnergy = GameConstants.DefaultMaxEnergy;
        public long lastRefillUtcTicks;
    }

    [Serializable]
    public class MembershipState
    {
        public bool noAdsOwned;
        public bool premiumMembershipActive;
    }

    [Serializable]
    public class ThemeOwnershipState
    {
        public List<string> unlockedThemes = new List<string> { GameConstants.DefaultThemeId };
        public string activeThemeId = GameConstants.DefaultThemeId;
    }

    [Serializable]
    public class EconomyState
    {
        public int hints;
        public int softCurrency;
        public bool startingHintsGranted;
        public bool startingSoftCurrencyGranted;
    }

    [Serializable]
    public class ProgressState
    {
        public string activeCampaignId = GameConstants.DefaultCampaignId;
        public int highestUnlockedLevel = 1;
        public int lastCompletedLevel = 0;
        public List<LevelRewardProgressState> levelRewards = new List<LevelRewardProgressState>();
        public List<LanguageProgressState> localizedProgress = new List<LanguageProgressState>();

        public void EnsureLanguageProgressMigrated(string currentLanguageCode)
        {
            if (localizedProgress == null)
            {
                localizedProgress = new List<LanguageProgressState>();
            }

            if (localizedProgress.Count > 0)
            {
                return;
            }

            localizedProgress.Add(new LanguageProgressState
            {
                languageCode = GameConstants.NormalizeLanguageCode(currentLanguageCode),
                activeCampaignId = string.IsNullOrWhiteSpace(activeCampaignId) ? GameConstants.DefaultCampaignId : activeCampaignId,
                highestUnlockedLevel = Mathf.Max(1, highestUnlockedLevel),
                lastCompletedLevel = Mathf.Max(0, lastCompletedLevel)
            });
        }

        public LanguageProgressState GetOrCreateLanguageProgress(string languageCode)
        {
            string normalized = GameConstants.NormalizeLanguageCode(languageCode);
            EnsureLanguageProgressMigrated(normalized);

            for (int i = 0; i < localizedProgress.Count; i++)
            {
                LanguageProgressState entry = localizedProgress[i];
                if (entry != null && GameConstants.NormalizeLanguageCode(entry.languageCode) == normalized)
                {
                    entry.languageCode = normalized;
                    if (string.IsNullOrWhiteSpace(entry.activeCampaignId))
                    {
                        entry.activeCampaignId = GameConstants.DefaultCampaignId;
                    }

                    entry.highestUnlockedLevel = Mathf.Max(1, entry.highestUnlockedLevel);
                    entry.lastCompletedLevel = Mathf.Max(0, entry.lastCompletedLevel);
                    return entry;
                }
            }

            LanguageProgressState created = new LanguageProgressState
            {
                languageCode = normalized,
                activeCampaignId = GameConstants.DefaultCampaignId,
                highestUnlockedLevel = 1,
                lastCompletedLevel = 0
            };
            localizedProgress.Add(created);
            return created;
        }

        public int GetHighestUnlockedLevel(string languageCode)
        {
            return Mathf.Max(1, GetOrCreateLanguageProgress(languageCode).highestUnlockedLevel);
        }

        public int GetLastCompletedLevel(string languageCode)
        {
            return Mathf.Max(0, GetOrCreateLanguageProgress(languageCode).lastCompletedLevel);
        }

        public void SetHighestUnlockedLevel(string languageCode, int levelId)
        {
            GetOrCreateLanguageProgress(languageCode).highestUnlockedLevel = Mathf.Max(1, levelId);
        }

        public void SetLastCompletedLevel(string languageCode, int levelId)
        {
            GetOrCreateLanguageProgress(languageCode).lastCompletedLevel = Mathf.Max(0, levelId);
        }

        public void SetActiveCampaignId(string languageCode, string campaignId)
        {
            GetOrCreateLanguageProgress(languageCode).activeCampaignId = string.IsNullOrWhiteSpace(campaignId)
                ? GameConstants.DefaultCampaignId
                : campaignId;
        }
    }

    [Serializable]
    public class LanguageProgressState
    {
        public string languageCode = GameConstants.DefaultLanguageCode;
        public string activeCampaignId = GameConstants.DefaultCampaignId;
        public int highestUnlockedLevel = 1;
        public int lastCompletedLevel = 0;
    }

    [Serializable]
    public class LevelRewardProgressState
    {
        public int levelId;
        public int bestStars;
        public int completionCount;
        public bool firstClearCoinsGranted;
        public int totalCoinsEarned;
    }

    [Serializable]
    public class MetricsState
    {
        public int perfectHits;
        public int toleratedHits;
        public int nearMisses;
        public int wrongLetters;
        public int wrongSlots;
        public int completedLevels;
    }

    [Serializable]
    public class RemoteContentState
    {
        public bool remoteContentEnabled = true;
        public string activeManifestVersion = "local-only";
        public long lastRemoteRefreshUtcTicks;
    }

    [Serializable]
    public class TelemetryState
    {
        public string currentSessionId = string.Empty;
        public long lastTelemetryFlushUtcTicks;
        public int pendingTelemetryEventCount;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public string languageCode = "tr";
        public SessionSnapshot session = new SessionSnapshot();
        public EnergyState energy = new EnergyState();
        public MembershipState membership = new MembershipState();
        public ThemeOwnershipState themes = new ThemeOwnershipState();
        public EconomyState economy = new EconomyState();
        public ProgressState progress = new ProgressState();
        public MetricsState metrics = new MetricsState();
        public RemoteContentState remoteContent = new RemoteContentState();
        public TelemetryState telemetry = new TelemetryState();
    }
}
