namespace WordSpinAlpha.Core
{
    public static class GameConstants
    {
        public const string DefaultLanguageCode = "tr";
        public const string SaveFileName = "wordspin_alpha_save.json";
        public const string SceneBoot = "Boot";
        public const string SceneMainMenu = "MainMenu";
        public const string SceneHub = "Hub";
        public const string SceneHubPreview = "HubPreview";
        public const string SceneGameplay = "Gameplay";
        public const string SceneStore = "Store";

        public const string ResourceLevels = "Content/levels";
        public const string ResourceQuestions = "Content/questions";
        public const string ResourceThemes = "Content/themes";
        public const string ResourceInfoCards = "Content/info_cards";
        public const string ResourceCampaigns = "Content/campaigns";
        public const string ResourceDifficulties = "Content/difficulty_profiles";
        public const string ResourceDifficultyTiers = "Content/difficulty_tiers";
        public const string ResourceRhythms = "Content/rhythm_profiles";
        public const string ResourceShapeLayouts = "Content/shape_layouts";
        public const string ResourceLocalizedContentRoot = "Content/Locales";

        public const string ResourceEnergyConfig = "Configs/energy_config";
        public const string ResourceKeyboardConfig = "Configs/keyboard_config";
        public const string ResourceStoreCatalog = "Configs/store_catalog";
        public const string ResourceMembershipProfile = "Configs/membership_profile";

        public const string RemoteContentDirectory = "wordspin_remote";
        public const string RemoteManifestFileName = "manifest.json";
        public const string TelemetryQueueFileName = "wordspin_telemetry_queue.json";
        public const string TelemetrySnapshotFileName = "wordspin_telemetry_snapshot.json";

        public const string DefaultCampaignId = "alpha_main";
        public const string DefaultThemeId = "base_default";
        public const string PremiumMythologyThemeId = "myth_hades";

        public const int DefaultQuestionHearts = 3;
        public const int DefaultMaxEnergy = 5;
        public const int DefaultEnergyRefillMinutes = 30;

        public static string NormalizeLanguageCode(string languageCode)
        {
            switch ((languageCode ?? DefaultLanguageCode).Trim().ToLowerInvariant())
            {
                case "en":
                case "es":
                case "de":
                    return languageCode.Trim().ToLowerInvariant();
                default:
                    return DefaultLanguageCode;
            }
        }

        public static string BuildLocalizedResourcePath(string languageCode, string fileNameWithoutExtension)
        {
            return $"{ResourceLocalizedContentRoot}/{NormalizeLanguageCode(languageCode)}/{fileNameWithoutExtension}";
        }

        public static string BuildLocalizedRemoteRelativePath(string languageCode, string fileName)
        {
            return $"Locales/{NormalizeLanguageCode(languageCode)}/{fileName}";
        }
    }
}
