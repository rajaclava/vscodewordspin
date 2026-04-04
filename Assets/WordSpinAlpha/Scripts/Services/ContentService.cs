using System;
using System.Collections.Generic;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Services
{
    public class ContentService : Singleton<ContentService>, IContentProvider
    {
        [SerializeField] private LocalContentProvider localProvider;
        [SerializeField] private RemoteContentProvider remoteProvider;

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
        private bool _forceLocalEditorContent;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            ResolveProviders();
            RefreshRemoteOverrides();
        }

        public void RefreshRemoteOverrides()
        {
            ResolveProviders();
            remoteProvider?.Refresh();
            _forceLocalEditorContent = false;
            ClearCaches();

            if (SaveManager.Instance == null || remoteProvider == null)
            {
                return;
            }

            RemoteContentManifestDefinition manifest = remoteProvider.LoadManifest();
            SaveManager.Instance.Data.remoteContent.remoteContentEnabled = manifest != null && manifest.remoteContentEnabled;
            SaveManager.Instance.Data.remoteContent.activeManifestVersion = manifest != null && !string.IsNullOrWhiteSpace(manifest.manifestVersion)
                ? manifest.manifestVersion
                : "local-only";
            SaveManager.Instance.Data.remoteContent.lastRemoteRefreshUtcTicks = DateTime.UtcNow.Ticks;
            SaveManager.Instance.Save();
        }

        public void RefreshLanguageContext()
        {
            ResolveProviders();
            localProvider?.RefreshLocalization();
            remoteProvider?.RefreshLocalization();
            ClearCaches();
        }

        public void RefreshEditorContent()
        {
            ResolveProviders();
            _forceLocalEditorContent = true;
            localProvider?.RefreshAll();
            remoteProvider?.Refresh();
            ClearCaches();
        }

        public LevelCatalog LoadLevels() => _levels ?? (_levels = MergeLevels(localProvider?.LoadLevels(), ShouldUseRemote() ? remoteProvider?.LoadLevels() : null));
        public QuestionCatalog LoadQuestions() => _questions ?? (_questions = MergeQuestions(localProvider?.LoadQuestions(), ShouldUseRemote() ? remoteProvider?.LoadQuestions() : null));
        public ThemeCatalog LoadThemes() => _themes ?? (_themes = MergeThemes(localProvider?.LoadThemes(), ShouldUseRemote() ? remoteProvider?.LoadThemes() : null));
        public InfoCardCatalog LoadInfoCards() => _infoCards ?? (_infoCards = MergeInfoCards(localProvider?.LoadInfoCards(), ShouldUseRemote() ? remoteProvider?.LoadInfoCards() : null));
        public CampaignCatalog LoadCampaigns() => _campaigns ?? (_campaigns = MergeCampaigns(localProvider?.LoadCampaigns(), ShouldUseRemote() ? remoteProvider?.LoadCampaigns() : null));
        public DifficultyCatalog LoadDifficultyProfiles() => _difficulties ?? (_difficulties = MergeDifficulties(localProvider?.LoadDifficultyProfiles(), ShouldUseRemote() ? remoteProvider?.LoadDifficultyProfiles() : null));
        public DifficultyTierCatalog LoadDifficultyTiers() => _difficultyTiers ?? (_difficultyTiers = MergeDifficultyTiers(localProvider?.LoadDifficultyTiers(), ShouldUseRemote() ? remoteProvider?.LoadDifficultyTiers() : null));
        public RhythmCatalog LoadRhythmProfiles() => _rhythms ?? (_rhythms = MergeRhythms(localProvider?.LoadRhythmProfiles(), ShouldUseRemote() ? remoteProvider?.LoadRhythmProfiles() : null));
        public ShapeLayoutCatalog LoadShapeLayouts() => _shapeLayouts ?? (_shapeLayouts = MergeShapeLayouts(localProvider?.LoadShapeLayouts(), ShouldUseRemote() ? remoteProvider?.LoadShapeLayouts() : null));
        public EnergyConfigDefinition LoadEnergyConfig() => _energyConfig ?? (_energyConfig = PreferRemote(localProvider?.LoadEnergyConfig(), ShouldUseRemote() ? remoteProvider?.LoadEnergyConfig() : null, HasMeaningfulEnergyConfig));
        public KeyboardConfigDefinition LoadKeyboardConfig() => _keyboardConfig ?? (_keyboardConfig = PreferRemote(localProvider?.LoadKeyboardConfig(), ShouldUseRemote() ? remoteProvider?.LoadKeyboardConfig() : null, c => c != null && c.layouts != null && c.layouts.Length > 0));
        public StoreCatalogDefinition LoadStoreCatalog() => _storeCatalog ?? (_storeCatalog = PreferRemote(localProvider?.LoadStoreCatalog(), ShouldUseRemote() ? remoteProvider?.LoadStoreCatalog() : null, c => c != null && ((c.themes != null && c.themes.Length > 0) || !string.IsNullOrWhiteSpace(c.premiumMembershipProductId))));
        public MembershipProfileDefinition LoadMembershipProfile() => _membershipProfile ?? (_membershipProfile = PreferRemote(localProvider?.LoadMembershipProfile(), ShouldUseRemote() ? remoteProvider?.LoadMembershipProfile() : null, c => c != null && (c.removeAds || c.unlimitedEntryEnergy || c.unlockFutureThemes)));

        private bool ShouldUseRemote()
        {
            if (_forceLocalEditorContent)
            {
                return false;
            }

            return SaveManager.Instance == null || SaveManager.Instance.Data.remoteContent.remoteContentEnabled;
        }

        private void ResolveProviders()
        {
            if (localProvider == null)
            {
                localProvider = LocalContentProvider.Instance ?? FindObjectOfType<LocalContentProvider>();
            }

            if (remoteProvider == null)
            {
                remoteProvider = RemoteContentProvider.Instance ?? FindObjectOfType<RemoteContentProvider>();
            }
        }

        private void ClearCaches()
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
        }

        private static T PreferRemote<T>(T local, T remote, Func<T, bool> hasRemoteContent) where T : class, new()
        {
            if (remote != null && hasRemoteContent(remote))
            {
                return remote;
            }

            return local ?? new T();
        }

        private static bool HasMeaningfulEnergyConfig(EnergyConfigDefinition config)
        {
            return config != null && (config.maxEnergy > 0 || config.refillMinutes > 0 || config.startingHints > 0 || config.startingSoftCurrency > 0 || config.bypassForPremiumMembership);
        }

        private static LevelCatalog MergeLevels(LevelCatalog local, LevelCatalog remote)
        {
            return new LevelCatalog { levels = MergeById(local?.levels, remote?.levels, item => item.levelId) };
        }

        private static QuestionCatalog MergeQuestions(QuestionCatalog local, QuestionCatalog remote)
        {
            return new QuestionCatalog { questions = MergeById(local?.questions, remote?.questions, item => item.questionId) };
        }

        private static ThemeCatalog MergeThemes(ThemeCatalog local, ThemeCatalog remote)
        {
            return new ThemeCatalog { themes = MergeById(local?.themes, remote?.themes, item => item.themeId) };
        }

        private static InfoCardCatalog MergeInfoCards(InfoCardCatalog local, InfoCardCatalog remote)
        {
            return new InfoCardCatalog { cards = MergeById(local?.cards, remote?.cards, item => item.infoCardId) };
        }

        private static CampaignCatalog MergeCampaigns(CampaignCatalog local, CampaignCatalog remote)
        {
            return new CampaignCatalog { campaigns = MergeById(local?.campaigns, remote?.campaigns, item => item.campaignId) };
        }

        private static DifficultyCatalog MergeDifficulties(DifficultyCatalog local, DifficultyCatalog remote)
        {
            return new DifficultyCatalog { profiles = MergeById(local?.profiles, remote?.profiles, item => item.difficultyProfileId) };
        }

        private static RhythmCatalog MergeRhythms(RhythmCatalog local, RhythmCatalog remote)
        {
            return new RhythmCatalog { profiles = MergeById(local?.profiles, remote?.profiles, item => item.rhythmProfileId) };
        }

        private static DifficultyTierCatalog MergeDifficultyTiers(DifficultyTierCatalog local, DifficultyTierCatalog remote)
        {
            return new DifficultyTierCatalog { tiers = MergeById(local?.tiers, remote?.tiers, item => item.difficultyTierId) };
        }

        private static ShapeLayoutCatalog MergeShapeLayouts(ShapeLayoutCatalog local, ShapeLayoutCatalog remote)
        {
            return new ShapeLayoutCatalog { layouts = MergeById(local?.layouts, remote?.layouts, item => item.shapeLayoutId) };
        }

        private static TItem[] MergeById<TItem, TKey>(TItem[] localItems, TItem[] remoteItems, Func<TItem, TKey> keySelector) where TItem : class
        {
            Dictionary<TKey, TItem> merged = new Dictionary<TKey, TItem>();

            foreach (TItem item in localItems ?? Array.Empty<TItem>())
            {
                if (item == null)
                {
                    continue;
                }

                merged[keySelector(item)] = item;
            }

            foreach (TItem item in remoteItems ?? Array.Empty<TItem>())
            {
                if (item == null)
                {
                    continue;
                }

                merged[keySelector(item)] = item;
            }

            TItem[] result = new TItem[merged.Count];
            int index = 0;
            foreach (TItem item in merged.Values)
            {
                result[index++] = item;
            }

            return result;
        }
    }
}
