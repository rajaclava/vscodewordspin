using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Services
{
    public class LocalContentProvider : Singleton<LocalContentProvider>, IContentProvider
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

        public LevelCatalog LoadLevels() => _levels ?? (_levels = LoadLocalizedJson<LevelCatalog>("levels", GameConstants.ResourceLevels, HasLevels));
        public QuestionCatalog LoadQuestions() => _questions ?? (_questions = LoadLocalizedJson<QuestionCatalog>("questions", GameConstants.ResourceQuestions, HasQuestions));
        public ThemeCatalog LoadThemes() => _themes ?? (_themes = LoadJson<ThemeCatalog>(GameConstants.ResourceThemes));
        public InfoCardCatalog LoadInfoCards() => _infoCards ?? (_infoCards = LoadLocalizedJson<InfoCardCatalog>("info_cards", GameConstants.ResourceInfoCards, HasInfoCards));
        public CampaignCatalog LoadCampaigns() => _campaigns ?? (_campaigns = LoadJson<CampaignCatalog>(GameConstants.ResourceCampaigns));
        public DifficultyCatalog LoadDifficultyProfiles() => _difficulties ?? (_difficulties = LoadJson<DifficultyCatalog>(GameConstants.ResourceDifficulties));
        public DifficultyTierCatalog LoadDifficultyTiers() => _difficultyTiers ?? (_difficultyTiers = LoadJson<DifficultyTierCatalog>(GameConstants.ResourceDifficultyTiers));
        public RhythmCatalog LoadRhythmProfiles() => _rhythms ?? (_rhythms = LoadJson<RhythmCatalog>(GameConstants.ResourceRhythms));
        public ShapeLayoutCatalog LoadShapeLayouts() => _shapeLayouts ?? (_shapeLayouts = LoadJson<ShapeLayoutCatalog>(GameConstants.ResourceShapeLayouts));
        public EnergyConfigDefinition LoadEnergyConfig() => _energyConfig ?? (_energyConfig = LoadJson<EnergyConfigDefinition>(GameConstants.ResourceEnergyConfig));
        public KeyboardConfigDefinition LoadKeyboardConfig() => _keyboardConfig ?? (_keyboardConfig = LoadJson<KeyboardConfigDefinition>(GameConstants.ResourceKeyboardConfig));
        public StoreCatalogDefinition LoadStoreCatalog() => _storeCatalog ?? (_storeCatalog = LoadJson<StoreCatalogDefinition>(GameConstants.ResourceStoreCatalog));
        public MembershipProfileDefinition LoadMembershipProfile() => _membershipProfile ?? (_membershipProfile = LoadJson<MembershipProfileDefinition>(GameConstants.ResourceMembershipProfile));

        public void RefreshLocalization()
        {
            _levels = null;
            _questions = null;
            _infoCards = null;
        }

        private static T LoadJson<T>(string resourcePath) where T : class, new()
        {
            TextAsset asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                Debug.LogWarning($"[LocalContentProvider] Missing resource: {resourcePath}");
                return new T();
            }

            T parsed = JsonUtility.FromJson<T>(asset.text);
            if (parsed == null)
            {
                Debug.LogWarning($"[LocalContentProvider] Could not parse resource: {resourcePath}");
                return new T();
            }

            return parsed;
        }

        private static T LoadLocalizedJson<T>(string localizedFileNameWithoutExtension, string fallbackResourcePath, System.Func<T, bool> hasContent) where T : class, new()
        {
            string languageCode = SaveManager.Instance != null ? SaveManager.Instance.Data.languageCode : GameConstants.DefaultLanguageCode;
            T localized = LoadJson<T>(GameConstants.BuildLocalizedResourcePath(languageCode, localizedFileNameWithoutExtension));
            if (hasContent(localized))
            {
                return localized;
            }

            return LoadJson<T>(fallbackResourcePath);
        }

        private static bool HasLevels(LevelCatalog catalog) => catalog != null && catalog.levels != null && catalog.levels.Length > 0;
        private static bool HasQuestions(QuestionCatalog catalog) => catalog != null && catalog.questions != null && catalog.questions.Length > 0;
        private static bool HasInfoCards(InfoCardCatalog catalog) => catalog != null && catalog.cards != null && catalog.cards.Length > 0;
    }
}
