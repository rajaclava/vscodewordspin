using System.IO;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Services
{
    public class RemoteContentProvider : Singleton<RemoteContentProvider>, IContentProvider
    {
        private LevelCatalog _levels;
        private QuestionCatalog _questions;
        private ThemeCatalog _themes;
        private InfoCardCatalog _infoCards;
        private CampaignCatalog _campaigns;
        private DifficultyCatalog _difficulties;
        private DifficultyTierCatalog _difficultyTiers;
        private RhythmCatalog _rhythms;
        private ShapeLayoutCatalog _shapeLayouts;
        private EnergyConfigDefinition _energyConfig;
        private KeyboardConfigDefinition _keyboardConfig;
        private StoreCatalogDefinition _storeCatalog;
        private MembershipProfileDefinition _membershipProfile;
        private RemoteContentManifestDefinition _manifest;

        public RemoteContentManifestDefinition LoadManifest() => _manifest ?? (_manifest = LoadRemoteJson<RemoteContentManifestDefinition>(GameConstants.RemoteManifestFileName));

        public LevelCatalog LoadLevels() => _levels ?? (_levels = LoadLocalizedRemoteJson<LevelCatalog>("levels.json", HasLevels));
        public QuestionCatalog LoadQuestions() => _questions ?? (_questions = LoadLocalizedRemoteJson<QuestionCatalog>("questions.json", HasQuestions));
        public ThemeCatalog LoadThemes() => _themes ?? (_themes = LoadRemoteJson<ThemeCatalog>("themes.json"));
        public InfoCardCatalog LoadInfoCards() => _infoCards ?? (_infoCards = LoadLocalizedRemoteJson<InfoCardCatalog>("info_cards.json", HasInfoCards));
        public CampaignCatalog LoadCampaigns() => _campaigns ?? (_campaigns = LoadRemoteJson<CampaignCatalog>("campaigns.json"));
        public DifficultyCatalog LoadDifficultyProfiles() => _difficulties ?? (_difficulties = LoadRemoteJson<DifficultyCatalog>("difficulty_profiles.json"));
        public DifficultyTierCatalog LoadDifficultyTiers() => _difficultyTiers ?? (_difficultyTiers = LoadRemoteJson<DifficultyTierCatalog>("difficulty_tiers.json"));
        public RhythmCatalog LoadRhythmProfiles() => _rhythms ?? (_rhythms = LoadRemoteJson<RhythmCatalog>("rhythm_profiles.json"));
        public ShapeLayoutCatalog LoadShapeLayouts() => _shapeLayouts ?? (_shapeLayouts = LoadRemoteJson<ShapeLayoutCatalog>("shape_layouts.json"));
        public EnergyConfigDefinition LoadEnergyConfig() => _energyConfig ?? (_energyConfig = LoadRemoteJson<EnergyConfigDefinition>("energy_config.json"));
        public KeyboardConfigDefinition LoadKeyboardConfig() => _keyboardConfig ?? (_keyboardConfig = LoadRemoteJson<KeyboardConfigDefinition>("keyboard_config.json"));
        public StoreCatalogDefinition LoadStoreCatalog() => _storeCatalog ?? (_storeCatalog = LoadRemoteJson<StoreCatalogDefinition>("store_catalog.json"));
        public MembershipProfileDefinition LoadMembershipProfile() => _membershipProfile ?? (_membershipProfile = LoadRemoteJson<MembershipProfileDefinition>("membership_profile.json"));

        public void RefreshLocalization()
        {
            _levels = null;
            _questions = null;
            _infoCards = null;
        }

        public void Refresh()
        {
            _levels = null;
            _questions = null;
            _themes = null;
            _infoCards = null;
            _campaigns = null;
            _difficulties = null;
            _difficultyTiers = null;
            _rhythms = null;
            _shapeLayouts = null;
            _energyConfig = null;
            _keyboardConfig = null;
            _storeCatalog = null;
            _membershipProfile = null;
            _manifest = null;
        }

        private static T LoadRemoteJson<T>(string fileName) where T : class, new()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, GameConstants.RemoteContentDirectory, fileName);
            if (!File.Exists(fullPath))
            {
                return new T();
            }

            try
            {
                string json = File.ReadAllText(fullPath);
                T parsed = JsonUtility.FromJson<T>(json);
                return parsed ?? new T();
            }
            catch
            {
                return new T();
            }
        }

        private static T LoadLocalizedRemoteJson<T>(string fileName, System.Func<T, bool> hasContent) where T : class, new()
        {
            string languageCode = SaveManager.Instance != null ? SaveManager.Instance.Data.languageCode : GameConstants.DefaultLanguageCode;
            T localized = LoadRemoteJson<T>(GameConstants.BuildLocalizedRemoteRelativePath(languageCode, fileName));
            if (hasContent(localized))
            {
                return localized;
            }

            return LoadRemoteJson<T>(fileName);
        }

        private static bool HasLevels(LevelCatalog catalog) => catalog != null && catalog.levels != null && catalog.levels.Length > 0;
        private static bool HasQuestions(QuestionCatalog catalog) => catalog != null && catalog.questions != null && catalog.questions.Length > 0;
        private static bool HasInfoCards(InfoCardCatalog catalog) => catalog != null && catalog.cards != null && catalog.cards.Length > 0;
    }
}
