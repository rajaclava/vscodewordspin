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
    }

    [Serializable]
    public class ProgressState
    {
        public string activeCampaignId = GameConstants.DefaultCampaignId;
        public int highestUnlockedLevel = 1;
        public int lastCompletedLevel = 0;
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
