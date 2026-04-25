namespace WordSpinAlpha.Core
{
    public static class DevTestPolicy
    {
        // Development/manual-test policy only.
        // Energy system is not removed; only the level-entry energy gate is bypassed during testing.
        // This does not change save/progress/session/telemetry behavior.
        // Market/final builds must keep the energy system active.
        // To re-enable entry-energy checks, set DisableEntryEnergyForManualTesting to false.
        // Release builds always return bypass disabled.
        public const bool DisableEntryEnergyForManualTesting = true;

        // Development/manual-test policy only for HubPreview level locks.
        // Keep false for market/final flow.
        // This is separate from energy bypass policy.
        // This does not mutate save/progress/session data; it only relaxes HubPreview lock checks in testing.
        // For locked-node -> active-session alignment tests, keep this false.
        public const bool UnlockAllHubLevelsForManualTesting = false;

        // Development/manual-test policy only for HubPreview highest-unlocked checks.
        // This does not write save/progress data.
        // It can be used for locked-node and active-session alignment tests.
        // Keep this ineffective in market/final builds.
        // Set to 0 to return to normal progress-based behavior.
        public const int ForceHubHighestUnlockedLevelForManualTesting = 4;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        public static bool IsEnergyBypassEnabled()
        {
            return DisableEntryEnergyForManualTesting;
        }

        public static bool IsHubLevelUnlockBypassEnabled()
        {
            return UnlockAllHubLevelsForManualTesting;
        }

        public static bool IsHubHighestUnlockedOverrideEnabled(out int forcedHighestUnlockedLevel)
        {
            forcedHighestUnlockedLevel = ForceHubHighestUnlockedLevelForManualTesting;
            return forcedHighestUnlockedLevel > 0;
        }
#else
        public static bool IsEnergyBypassEnabled()
        {
            return false;
        }

        public static bool IsHubLevelUnlockBypassEnabled()
        {
            return false;
        }

        public static bool IsHubHighestUnlockedOverrideEnabled(out int forcedHighestUnlockedLevel)
        {
            forcedHighestUnlockedLevel = 0;
            return false;
        }
#endif
    }
}
