using WordSpinAlpha.Content;

namespace WordSpinAlpha.Services
{
    public interface IContentProvider
    {
        LevelCatalog LoadLevels();
        QuestionCatalog LoadQuestions();
        ThemeCatalog LoadThemes();
        InfoCardCatalog LoadInfoCards();
        CampaignCatalog LoadCampaigns();
        DifficultyCatalog LoadDifficultyProfiles();
        DifficultyTierCatalog LoadDifficultyTiers();
        RhythmCatalog LoadRhythmProfiles();
        ShapeLayoutCatalog LoadShapeLayouts();
        EnergyConfigDefinition LoadEnergyConfig();
        KeyboardConfigDefinition LoadKeyboardConfig();
        StoreCatalogDefinition LoadStoreCatalog();
        MembershipProfileDefinition LoadMembershipProfile();
    }
}
