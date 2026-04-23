using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Presentation
{
    public class HubPresenter : MonoBehaviour
    {
        private static Sprite s_defaultSprite;
        private static Sprite s_roundSprite;

        private enum HubTab
        {
            Journey,
            Missions,
            Profile,
            Store
        }

        [Header("Top Bar")]
        [SerializeField] private TextMeshProUGUI energyLabel;
        [SerializeField] private TextMeshProUGUI hintLabel;
        [SerializeField] private TextMeshProUGUI coinLabel;
        [SerializeField] private TextMeshProUGUI languageLabel;
        [SerializeField] private TextMeshProUGUI headerTitleLabel;
        [SerializeField] private TextMeshProUGUI headerSubtitleLabel;

        [Header("Pages")]
        [SerializeField] private RectTransform pageViewport;
        [SerializeField] private RectTransform journeyPage;
        [SerializeField] private RectTransform missionsPage;
        [SerializeField] private RectTransform profilePage;
        [SerializeField] private RectTransform storePage;

        [Header("Journey")]
        [SerializeField] private LevelPathMapView levelPathMapView;
        [SerializeField] private Transform levelButtonContainer;
        [SerializeField] private Button levelButtonTemplate;
        [SerializeField] private TextMeshProUGUI mapTitleLabel;
        [SerializeField] private TextMeshProUGUI mapSummaryLabel;
        [SerializeField] private TextMeshProUGUI selectedLevelTitleLabel;
        [SerializeField] private TextMeshProUGUI selectedLevelBodyLabel;
        [SerializeField] private Button playSelectedLevelButton;

        [Header("Text Blocks")]
        [SerializeField] private TextMeshProUGUI missionsTitleLabel;
        [SerializeField] private TextMeshProUGUI missionsSummaryLabel;
        [SerializeField] private TextMeshProUGUI profileTitleLabel;
        [SerializeField] private TextMeshProUGUI profileSummaryLabel;
        [SerializeField] private TextMeshProUGUI storeTitleLabel;
        [SerializeField] private TextMeshProUGUI storeSummaryLabel;
        [SerializeField] private TextMeshProUGUI quickToastLabel;

        [Header("Resume Prompt")]
        [SerializeField] private GameObject resumePromptRoot;
        [SerializeField] private TextMeshProUGUI resumePromptTitleLabel;
        [SerializeField] private TextMeshProUGUI resumePromptBodyLabel;
        [SerializeField] private Button resumePromptContinueButton;
        [SerializeField] private Button resumePromptRestartButton;
        [SerializeField] private Button resumePromptCancelButton;

        [Header("Buttons")]
        [SerializeField] private Button navJourneyButton;
        [SerializeField] private Button navMissionsButton;
        [SerializeField] private Button navProfileButton;
        [SerializeField] private Button navStoreButton;
        [SerializeField] private Button quickGiftButton;
        [SerializeField] private Button quickMissionButton;
        [SerializeField] private Button quickStoreButton;
        [SerializeField] private Button storeThemeButton;
        [SerializeField] private Button storeHintsButton;
        [SerializeField] private Button storeEnergyButton;
        [SerializeField] private Button storeMembershipButton;

        [Header("Theme")]
        [SerializeField] private bool compactTopBarStyle;
        [SerializeField] private string headerTitleText = "Macera Merkezi";
        [SerializeField] private string headerSubtitleText = "Alt menuden gecis yap, seviyeni sec ve devam et.";
        [SerializeField] private string levelCaptionText = "Level";
        [SerializeField] private Color activeNavColor = new Color(0.39f, 0.69f, 0.96f, 1f);
        [SerializeField] private Color inactiveNavColor = new Color(0.16f, 0.22f, 0.31f, 0.96f);
        [SerializeField] private Color activeNavTextColor = Color.white;
        [SerializeField] private Color inactiveNavTextColor = new Color(0.88f, 0.93f, 1f, 0.92f);

        private LevelDefinition[] _cachedLevels = Array.Empty<LevelDefinition>();
        private int _selectedLevelId;
        private HubTab _activeTab = HubTab.Journey;
        private Coroutine _transitionRoutine;

        private void Awake()
        {
            WireButtons();
            EnsureResumePrompt();
            EnsurePageState(HubTab.Journey, true);
        }

        private void OnEnable()
        {
            GameEvents.EntryEnergyChanged += HandleEnergyChanged;
            GameEvents.LanguageChanged += HandleLanguageChanged;
            GameEvents.SoftCurrencyChanged += HandleSoftCurrencyChanged;
            GameEvents.MembershipChanged += HandleMembershipChanged;
            GameEvents.ThemeUnlocked += HandleThemeUnlocked;
        }

        private void OnDisable()
        {
            GameEvents.EntryEnergyChanged -= HandleEnergyChanged;
            GameEvents.LanguageChanged -= HandleLanguageChanged;
            GameEvents.SoftCurrencyChanged -= HandleSoftCurrencyChanged;
            GameEvents.MembershipChanged -= HandleMembershipChanged;
            GameEvents.ThemeUnlocked -= HandleThemeUnlocked;
        }

        private void Start()
        {
            RefreshAll();
        }

        public void OpenJourneyTab() => SwitchTab(HubTab.Journey);
        public void OpenMissionsTab() => SwitchTab(HubTab.Missions);
        public void OpenProfileTab() => SwitchTab(HubTab.Profile);
        public void OpenStoreTab() => SwitchTab(HubTab.Store);

        public void StartSelectedLevel()
        {
            int progressLevel = GetProgressLevelId();
            if (_selectedLevelId > progressLevel)
            {
                SetToast("Bu seviyeyi acmak icin once onceki seviyeleri tamamla.");
                return;
            }

            if (CanResumeSelectedLevel())
            {
                ShowResumePrompt();
                return;
            }

            if (SceneNavigator.Instance == null || !SceneNavigator.Instance.OpenGameplayLevel(_selectedLevelId, true))
            {
                SetToast("Seviye acilamadi. Can durumunu kontrol et.");
            }
        }

        public void SelectLevel(int levelId)
        {
            _selectedLevelId = Mathf.Max(1, levelId);
            HideResumePrompt();
            BuildLevelMap();
            RefreshJourneySummary();
        }

        public void ContinueSelectedLevel()
        {
            HideResumePrompt();
            if (CanResumeSelectedLevel())
            {
                SceneNavigator.Instance?.OpenGameplayForProgress();
                return;
            }

            if (SceneNavigator.Instance == null || !SceneNavigator.Instance.OpenGameplayLevel(_selectedLevelId, true))
            {
                SetToast("Kayitli ilerleme bulunamadi. Seviye acilamadi.");
            }
        }

        public void RestartSelectedLevel()
        {
            HideResumePrompt();
            if (SceneNavigator.Instance == null || !SceneNavigator.Instance.OpenGameplayLevel(_selectedLevelId, true))
            {
                SetToast("Bastan baslatmak icin yeterli can yok.");
            }
        }

        public void CancelResumePrompt()
        {
            HideResumePrompt();
        }

        public void BuyTheme()
        {
            if (EconomyManager.Instance != null && EconomyManager.Instance.IsThemeUnlocked(GameConstants.PremiumMythologyThemeId))
            {
                EconomyManager.Instance.SetActiveTheme(GameConstants.PremiumMythologyThemeId);
            }
            else
            {
                EconomyManager.Instance?.TryUnlockThemeWithSoftCurrency(GameConstants.PremiumMythologyThemeId);
            }

            RefreshStoreSummary();
            RefreshProfileSummary();
            SetToast("Tema islemi uygulandi.");
        }

        public void BuyHints()
        {
            StoreCatalogDefinition catalog = ContentService.Instance != null ? ContentService.Instance.LoadStoreCatalog() : null;
            string productId = catalog != null && catalog.hintPacks != null && catalog.hintPacks.Length > 0 ? catalog.hintPacks[0].productId : "hint_pack_preview";
            MockPurchaseService.Instance?.Purchase(productId, PurchaseProductType.HintPack);
            RefreshAll();
            SetToast("Ipucular verildi.");
        }

        public void BuyEnergy()
        {
            StoreCatalogDefinition catalog = ContentService.Instance != null ? ContentService.Instance.LoadStoreCatalog() : null;
            string productId = catalog != null && catalog.energyPacks != null && catalog.energyPacks.Length > 0 ? catalog.energyPacks[0].productId : "energy_pack_preview";
            MockPurchaseService.Instance?.Purchase(productId, PurchaseProductType.EnergyPack);
            RefreshAll();
            SetToast("Can verildi.");
        }

        public void BuyMembership()
        {
            StoreCatalogDefinition catalog = ContentService.Instance != null ? ContentService.Instance.LoadStoreCatalog() : null;
            string productId = catalog != null ? catalog.premiumMembershipProductId : "membership_preview";
            MockPurchaseService.Instance?.Purchase(productId, PurchaseProductType.PremiumMembership);
            RefreshAll();
            SetToast("Premium aktif edildi.");
        }

        public void OpenGiftOffer()
        {
            SwitchTab(HubTab.Missions);
            SetToast("Hediye paneli on izleme olarak acildi.");
        }

        public void OpenMissionOffer()
        {
            SwitchTab(HubTab.Missions);
            SetToast("Gorev paneli acildi.");
        }

        public void OpenStoreOffer()
        {
            SwitchTab(HubTab.Store);
            SetToast("Magaza paneli acildi.");
        }

        public void RefreshForEditor()
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            RefreshMetrics();
            RefreshHeader();
            RefreshLocalizedTexts();
            BuildLevelMap();
            RefreshJourneySummary();
            RefreshMissionsSummary();
            RefreshProfileSummary();
            RefreshStoreSummary();
            EnsurePageState(_activeTab, true);
        }

        private void BuildLevelMap()
        {
            if (levelPathMapView == null || levelButtonContainer == null || levelButtonTemplate == null)
            {
                return;
            }

            _cachedLevels = ContentService.Instance != null
                ? (ContentService.Instance.LoadLevels().levels ?? Array.Empty<LevelDefinition>()).OrderBy(level => level.levelId).ToArray()
                : Array.Empty<LevelDefinition>();

            if (_cachedLevels.Length <= 0)
            {
                return;
            }

            if (_selectedLevelId <= 0 || !_cachedLevels.Any(level => level != null && level.levelId == _selectedLevelId))
            {
                _selectedLevelId = GetSuggestedSelectedLevelId();
            }

            _selectedLevelId = Mathf.Clamp(_selectedLevelId, _cachedLevels[0].levelId, _cachedLevels[_cachedLevels.Length - 1].levelId);
            levelPathMapView.Build(_cachedLevels, _selectedLevelId, levelCaptionText, SelectLevel);
            levelPathMapView.CenterOnLevel(_selectedLevelId, true);
        }

        private void RefreshMetrics()
        {
            string languageCode = CurrentLanguageCode().ToUpperInvariant();
            if (energyLabel != null)
            {
                int currentEnergy = EnergyManager.Instance != null ? EnergyManager.Instance.CurrentEnergy : 0;
                int maxEnergy = EnergyManager.Instance != null ? EnergyManager.Instance.MaxEnergy : GameConstants.DefaultMaxEnergy;
                energyLabel.text = compactTopBarStyle ? $"{currentEnergy}/{maxEnergy}" : $"Can {currentEnergy}/{maxEnergy}";
            }

            if (hintLabel != null)
            {
                int hints = EconomyManager.Instance != null ? EconomyManager.Instance.Hints : 0;
                hintLabel.text = compactTopBarStyle ? hints.ToString() : $"Ipucu {hints}";
            }

            if (coinLabel != null)
            {
                coinLabel.text = $"{(EconomyManager.Instance != null ? EconomyManager.Instance.SoftCurrency : 0)}";
            }

            if (languageLabel != null)
            {
                languageLabel.text = languageCode;
            }
        }

        private void RefreshHeader()
        {
            if (headerTitleLabel != null)
            {
                headerTitleLabel.text = headerTitleText;
            }

            if (headerSubtitleLabel != null)
            {
                headerSubtitleLabel.text = headerSubtitleText;
            }
        }

        private void RefreshLocalizedTexts()
        {
            SetButtonLabel(navJourneyButton, "Yolculuk");
            SetButtonLabel(navMissionsButton, "Gorevler");
            SetButtonLabel(navProfileButton, "Profil");
            SetButtonLabel(navStoreButton, "Magaza");
            SetButtonLabel(quickGiftButton, "Hediye");
            SetButtonLabel(quickMissionButton, "Etkinlik");
            SetButtonLabel(quickStoreButton, "Teklif");
            SetButtonLabel(storeThemeButton, "Tema Paketi");
            SetButtonLabel(storeHintsButton, "Ipucu x3");
            SetButtonLabel(storeEnergyButton, "Can +3");
            SetButtonLabel(storeMembershipButton, "Premium");
            if (resumePromptTitleLabel != null)
            {
                resumePromptTitleLabel.text = "Kayitli Ilerleme Bulundu";
            }

            if (resumePromptBodyLabel != null)
            {
                resumePromptBodyLabel.text = "Bu seviyede kayitli ilerlemen var. Devam et dersen can harcanmaz. Bastan baslatirsan yeni giris maliyeti uygulanir.";
            }

            SetButtonLabel(resumePromptContinueButton, "Devam Et");
            SetButtonLabel(resumePromptRestartButton, "Bastan Basla");
            SetButtonLabel(resumePromptCancelButton, "Vazgec");
        }

        private void RefreshJourneySummary()
        {
            if (mapTitleLabel != null)
            {
                mapTitleLabel.text = "Yol Haritasi";
            }

            if (mapSummaryLabel != null)
            {
                mapSummaryLabel.text = $"Ilerleme: {GetProgressLevelId()}/{Mathf.Max(1, _cachedLevels.Length)}";
            }

            if (selectedLevelTitleLabel != null)
            {
                selectedLevelTitleLabel.text = $"{levelCaptionText} {_selectedLevelId}";
            }

            if (selectedLevelBodyLabel != null)
            {
                int progressLevel = GetProgressLevelId();
                selectedLevelBodyLabel.text = CanResumeSelectedLevel()
                    ? "Kayitli ilerleme var. Devam etmek ucretsiz, bastan baslatmak maliyetlidir."
                    : _selectedLevelId == progressLevel
                    ? "Aktif seviyen hazir."
                    : _selectedLevelId < progressLevel
                        ? "Istersen bu seviyeyi tekrar oynayabilirsin."
                        : "Bu seviyeyi acmak icin ilerlemelisin.";
            }

            string playLabel;
            if (compactTopBarStyle)
            {
                playLabel = CanResumeSelectedLevel() ? "DEVAM ET / BASTAN" : "DEVAM ET";
            }
            else
            {
                playLabel = CanResumeSelectedLevel() ? $"Devam / Bastan Basla {_selectedLevelId}" : $"Seviyeyi Oyna {_selectedLevelId}";
            }

            SetButtonLabel(playSelectedLevelButton, playLabel);
        }

        private void RefreshMissionsSummary()
        {
            if (missionsTitleLabel != null)
            {
                missionsTitleLabel.text = "Gorevler ve Etkinlikler";
            }

            if (missionsSummaryLabel != null)
            {
                int progress = GetProgressLevelId();
                int nextMilestone = Mathf.Max(progress, ((progress / 5) + 1) * 5);
                missionsSummaryLabel.text =
                    $"Gunluk hedef: Level {progress} tamamla\n" +
                    $"Haftalik hedef: {nextMilestone}. levele ulas\n" +
                    "Bu alan gecici gorev kartlari icin hazirlandi ve sonra genisletilebilir.";
            }
        }

        private void RefreshProfileSummary()
        {
            if (profileTitleLabel != null)
            {
                profileTitleLabel.text = "Oyuncu Profili";
            }

            if (profileSummaryLabel != null)
            {
                int progress = GetProgressLevelId();
                int lastCompleted = SaveManager.Instance != null ? SaveManager.Instance.Data.progress.GetLastCompletedLevel(CurrentLanguageCode()) : 0;
                string activeTheme = SaveManager.Instance != null ? SaveManager.Instance.Data.themes.activeThemeId : GameConstants.DefaultThemeId;
                bool premium = EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive;
                profileSummaryLabel.text =
                    $"Dil: {CurrentLanguageCode().ToUpperInvariant()}\n" +
                    $"Ilerleme: {progress}\n" +
                    $"Son tamamlanan: {lastCompleted}\n" +
                    $"Tema: {activeTheme}\n" +
                    $"Uyelik: {(premium ? "Aktif" : "Pasif")}";
            }
        }

        private void RefreshStoreSummary()
        {
            if (storeTitleLabel != null)
            {
                storeTitleLabel.text = "Teklifler ve Magaza";
            }

            if (storeSummaryLabel != null)
            {
                bool themeUnlocked = EconomyManager.Instance != null && EconomyManager.Instance.IsThemeUnlocked(GameConstants.PremiumMythologyThemeId);
                StorePriceQuote membershipQuote = StorePricingManager.Instance != null ? StorePricingManager.Instance.GetMembershipQuote(CurrentLanguageCode()) : default;
                StorePriceQuote themeQuote = StorePricingManager.Instance != null ? StorePricingManager.Instance.GetThemeQuote(GameConstants.PremiumMythologyThemeId, CurrentLanguageCode()) : default;
                string membershipText = membershipQuote.available ? membershipQuote.formattedPrice : "on izleme fiyati";
                string themeText = themeQuote.available ? themeQuote.formattedPrice : "on izleme fiyati";
                storeSummaryLabel.text =
                    "Gecici magaza kartlari hazir. Canli config ile sonra genisletilebilir.\n" +
                    $"Tema paketi: {(themeUnlocked ? "Acik" : themeText)}\n" +
                    $"Premium uyelik: {membershipText}";
            }
        }

        private void WireButtons()
        {
            BindButton(playSelectedLevelButton, StartSelectedLevel);
            BindButton(navJourneyButton, OpenJourneyTab);
            BindButton(navMissionsButton, OpenMissionsTab);
            BindButton(navProfileButton, OpenProfileTab);
            BindButton(navStoreButton, OpenStoreTab);
            BindButton(quickGiftButton, OpenGiftOffer);
            BindButton(quickMissionButton, OpenMissionOffer);
            BindButton(quickStoreButton, OpenStoreOffer);
            BindButton(storeThemeButton, BuyTheme);
            BindButton(storeHintsButton, BuyHints);
            BindButton(storeEnergyButton, BuyEnergy);
            BindButton(storeMembershipButton, BuyMembership);
        }

        private void BindButton(Button button, Action action)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());
        }

        private void SwitchTab(HubTab target)
        {
            if (_activeTab == target)
            {
                EnsurePageState(target, true);
                return;
            }

            if (_transitionRoutine != null)
            {
                StopCoroutine(_transitionRoutine);
            }

            _transitionRoutine = StartCoroutine(AnimateTabTransition(_activeTab, target));
            _activeTab = target;
            RefreshNavState();
        }

        private IEnumerator AnimateTabTransition(HubTab from, HubTab to)
        {
            RectTransform fromPage = GetPage(from);
            RectTransform toPage = GetPage(to);
            if (fromPage == null || toPage == null || pageViewport == null)
            {
                EnsurePageState(to, true);
                yield break;
            }

            float width = Mathf.Max(100f, pageViewport.rect.width);
            int direction = (int)to > (int)from ? 1 : -1;
            CanvasGroup fromGroup = EnsureCanvasGroup(fromPage);
            CanvasGroup toGroup = EnsureCanvasGroup(toPage);

            toPage.gameObject.SetActive(true);
            toPage.anchoredPosition = new Vector2(width * direction, 0f);
            toGroup.alpha = 0.72f;

            float elapsed = 0f;
            const float duration = 0.24f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float eased = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
                fromPage.anchoredPosition = Vector2.LerpUnclamped(Vector2.zero, new Vector2(-width * direction, 0f), eased);
                toPage.anchoredPosition = Vector2.LerpUnclamped(new Vector2(width * direction, 0f), Vector2.zero, eased);
                fromGroup.alpha = Mathf.Lerp(1f, 0.66f, eased);
                toGroup.alpha = Mathf.Lerp(0.72f, 1f, eased);
                yield return null;
            }

            fromPage.gameObject.SetActive(false);
            fromPage.anchoredPosition = Vector2.zero;
            fromGroup.alpha = 1f;
            toPage.anchoredPosition = Vector2.zero;
            toGroup.alpha = 1f;
            _transitionRoutine = null;
        }

        private void EnsurePageState(HubTab active, bool immediate)
        {
            SetPageActive(journeyPage, active == HubTab.Journey);
            SetPageActive(missionsPage, active == HubTab.Missions);
            SetPageActive(profilePage, active == HubTab.Profile);
            SetPageActive(storePage, active == HubTab.Store);
            _activeTab = active;
            RefreshNavState();
            if (immediate && active == HubTab.Journey)
            {
                levelPathMapView?.CenterOnLevel(_selectedLevelId, true);
            }
        }

        private void RefreshNavState()
        {
            SetNavState(navJourneyButton, _activeTab == HubTab.Journey);
            SetNavState(navMissionsButton, _activeTab == HubTab.Missions);
            SetNavState(navProfileButton, _activeTab == HubTab.Profile);
            SetNavState(navStoreButton, _activeTab == HubTab.Store);
        }

        private void SetNavState(Button button, bool active)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = active ? activeNavColor : inactiveNavColor;
            }

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.color = active ? activeNavTextColor : inactiveNavTextColor;
                label.fontStyle = active ? FontStyles.Bold : FontStyles.Normal;
            }
        }

        private static void SetPageActive(RectTransform page, bool active)
        {
            if (page == null)
            {
                return;
            }

            page.gameObject.SetActive(active);
            page.anchoredPosition = Vector2.zero;
            EnsureCanvasGroup(page).alpha = 1f;
        }

        private static CanvasGroup EnsureCanvasGroup(RectTransform page)
        {
            CanvasGroup group = page.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = page.gameObject.AddComponent<CanvasGroup>();
            }

            return group;
        }

        private RectTransform GetPage(HubTab tab)
        {
            return tab switch
            {
                HubTab.Journey => journeyPage,
                HubTab.Missions => missionsPage,
                HubTab.Profile => profilePage,
                HubTab.Store => storePage,
                _ => journeyPage
            };
        }

        private void HandleEnergyChanged(int _, int __) => RefreshMetrics();
        private void HandleSoftCurrencyChanged(int _, int __)
        {
            RefreshMetrics();
            RefreshStoreSummary();
            RefreshProfileSummary();
        }

        private void HandleMembershipChanged(bool _)
        {
            RefreshAll();
        }

        private void HandleThemeUnlocked(string _)
        {
            RefreshStoreSummary();
            RefreshProfileSummary();
        }

        private void HandleLanguageChanged(string _)
        {
            RefreshAll();
        }

        private void SetToast(string message)
        {
            if (quickToastLabel != null)
            {
                quickToastLabel.text = message;
            }
        }

        private void EnsureResumePrompt()
        {
            if (resumePromptRoot != null)
            {
                WireResumePromptButtons();
                resumePromptRoot.SetActive(false);
                return;
            }

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            Transform parent = parentCanvas != null ? parentCanvas.transform : transform;

            resumePromptRoot = new GameObject("ResumePromptOverlay", typeof(RectTransform), typeof(Image));
            resumePromptRoot.transform.SetParent(parent, false);
            RectTransform rootRect = resumePromptRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            Image rootImage = resumePromptRoot.GetComponent<Image>();
            rootImage.color = new Color(0.03f, 0.06f, 0.10f, 0.78f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(resumePromptRoot.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700f, 420f);
            panelRect.anchoredPosition = Vector2.zero;
            Image panelImage = panel.GetComponent<Image>();
            panelImage.sprite = GetDefaultSprite();
            panelImage.type = Image.Type.Sliced;
            panelImage.color = new Color(0.16f, 0.28f, 0.44f, 0.98f);

            Image accent = CreatePanelImage("Accent", panel.transform, new Vector2(0.5f, 0.90f), new Vector2(520f, 84f), new Color(0.96f, 0.55f, 0.28f, 0.96f), true);
            accent.color = new Color(0.96f, 0.55f, 0.28f, 0.96f);

            resumePromptTitleLabel = CreateLabel("Title", panel.transform, new Vector2(0.5f, 0.76f), new Vector2(560f, 54f), 34f, FontStyles.Bold, Color.white);
            resumePromptBodyLabel = CreateLabel("Body", panel.transform, new Vector2(0.5f, 0.56f), new Vector2(580f, 120f), 24f, FontStyles.Normal, new Color(0.94f, 0.97f, 1f));
            resumePromptBodyLabel.enableWordWrapping = true;

            resumePromptContinueButton = CreateButton("ContinueButton", panel.transform, new Vector2(0.5f, 0.28f), new Vector2(360f, 68f), new Color(0.26f, 0.55f, 0.89f, 1f), "Devam Et");
            resumePromptRestartButton = CreateButton("RestartButton", panel.transform, new Vector2(0.5f, 0.14f), new Vector2(360f, 68f), new Color(0.84f, 0.43f, 0.22f, 1f), "Bastan Basla");
            resumePromptCancelButton = CreateButton("CancelButton", panel.transform, new Vector2(0.5f, 0.06f), new Vector2(280f, 54f), new Color(0.18f, 0.24f, 0.31f, 1f), "Vazgec");

            WireResumePromptButtons();
            resumePromptRoot.SetActive(false);
        }

        private void WireResumePromptButtons()
        {
            BindButton(resumePromptContinueButton, ContinueSelectedLevel);
            BindButton(resumePromptRestartButton, RestartSelectedLevel);
            BindButton(resumePromptCancelButton, CancelResumePrompt);
        }

        private void ShowResumePrompt()
        {
            EnsureResumePrompt();
            if (resumePromptRoot != null)
            {
                resumePromptRoot.SetActive(true);
            }
        }

        private void HideResumePrompt()
        {
            if (resumePromptRoot != null)
            {
                resumePromptRoot.SetActive(false);
            }
        }

        private bool CanResumeSelectedLevel()
        {
            SessionSnapshot session = SaveManager.Instance != null ? SaveManager.Instance.Data.session : null;
            return session != null &&
                   session.hasActiveSession &&
                   session.levelId == _selectedLevelId &&
                   string.Equals(GameConstants.NormalizeLanguageCode(session.languageCode), CurrentLanguageCode(), StringComparison.OrdinalIgnoreCase);
        }

        private int GetProgressLevelId()
        {
            if (SaveManager.Instance == null)
            {
                return 1;
            }

            return SaveManager.Instance.Data.progress.GetHighestUnlockedLevel(CurrentLanguageCode());
        }

        private static string CurrentLanguageCode()
        {
            return SaveManager.Instance != null
                ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode)
                : GameConstants.DefaultLanguageCode;
        }

        private int GetSuggestedSelectedLevelId()
        {
            SessionSnapshot session = SaveManager.Instance != null ? SaveManager.Instance.Data.session : null;
            if (session != null &&
                session.hasActiveSession &&
                session.levelId > 0 &&
                string.Equals(GameConstants.NormalizeLanguageCode(session.languageCode), CurrentLanguageCode(), StringComparison.OrdinalIgnoreCase))
            {
                return session.levelId;
            }

            return GetProgressLevelId();
        }

        private static void SetButtonLabel(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = text;
            }
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, Vector2 anchor, Vector2 size, float fontSize, FontStyles style, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchor, Vector2 size, Color color, string text)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.sprite = GetDefaultSprite();
            image.type = Image.Type.Sliced;
            image.color = color;
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            TextMeshProUGUI label = CreateLabel("Label", go.transform, new Vector2(0.5f, 0.5f), new Vector2(size.x - 24f, size.y - 12f), 28f, FontStyles.Bold, Color.white);
            label.text = text;
            return button;
        }

        private static Image CreatePanelImage(string name, Transform parent, Vector2 anchor, Vector2 size, Color color, bool round)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.sprite = round ? GetRoundSprite() : GetDefaultSprite();
            image.type = Image.Type.Simple;
            image.color = color;
            return image;
        }

        private static Sprite GetDefaultSprite()
        {
            if (s_defaultSprite != null)
            {
                return s_defaultSprite;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            Color32[] pixels = { Color.white, Color.white, Color.white, Color.white };
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            s_defaultSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            return s_defaultSprite;
        }

        private static Sprite GetRoundSprite()
        {
            if (s_roundSprite != null)
            {
                return s_roundSprite;
            }

            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = size * 0.48f;
            Color32[] pixels = new Color32[size * size];
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    pixels[y * size + x] = distance <= radius ? (Color32)Color.white : new Color32(255, 255, 255, 0);
                }
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            s_roundSprite = Sprite.Create(texture, new Rect(0f, 0f, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
            return s_roundSprite;
        }
    }
}
