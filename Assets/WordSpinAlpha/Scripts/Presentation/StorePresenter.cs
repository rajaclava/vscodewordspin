using TMPro;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Presentation
{
    public class StorePresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI themeStatusLabel;
        [SerializeField] private TextMeshProUGUI membershipStatusLabel;
        [SerializeField] private TextMeshProUGUI coinStatusLabel;
        [SerializeField] private TextMeshProUGUI comingSoonLabel;
        [SerializeField] private TextMeshProUGUI themePriceLabel;
        [SerializeField] private TextMeshProUGUI membershipPriceLabel;

        private StoreCatalogDefinition _catalog;

        private void Awake()
        {
            EnsureCatalog();
            EnsureRuntimeLabels();
        }

        private void OnEnable()
        {
            GameEvents.LanguageChanged += HandleLanguageChanged;
            GameEvents.SoftCurrencyChanged += HandleSoftCurrencyChanged;
            GameEvents.MembershipChanged += HandleMembershipChanged;
        }

        private void OnDisable()
        {
            GameEvents.LanguageChanged -= HandleLanguageChanged;
            GameEvents.SoftCurrencyChanged -= HandleSoftCurrencyChanged;
            GameEvents.MembershipChanged -= HandleMembershipChanged;
        }

        private void Start()
        {
            Refresh();
        }

        public void BuyMythologyTheme()
        {
            if (EconomyManager.Instance != null && EconomyManager.Instance.IsThemeUnlocked(GameConstants.PremiumMythologyThemeId))
            {
                EconomyManager.Instance.SetActiveTheme(GameConstants.PremiumMythologyThemeId);
                Refresh();
                return;
            }

            if (EconomyManager.Instance != null && EconomyManager.Instance.TryUnlockThemeWithSoftCurrency(GameConstants.PremiumMythologyThemeId))
            {
                EconomyManager.Instance.SetActiveTheme(GameConstants.PremiumMythologyThemeId);
                Refresh();
                return;
            }

            Refresh();
        }

        public void BuyHints()
        {
            EnsureCatalog();
            if (_catalog != null && _catalog.hintPacks != null && _catalog.hintPacks.Length > 0)
            {
                MockPurchaseService.Instance.Purchase(_catalog.hintPacks[0].productId, PurchaseProductType.HintPack);
            }

            Refresh();
        }

        public void BuyEnergy()
        {
            EnsureCatalog();
            if (_catalog != null && _catalog.energyPacks != null && _catalog.energyPacks.Length > 0)
            {
                MockPurchaseService.Instance.Purchase(_catalog.energyPacks[0].productId, PurchaseProductType.EnergyPack);
            }

            Refresh();
        }

        public void BuyMembership()
        {
            EnsureCatalog();
            if (_catalog == null)
            {
                return;
            }

            MockPurchaseService.Instance.Purchase(_catalog.premiumMembershipProductId, PurchaseProductType.PremiumMembership);
            Refresh();
        }

        public void BuyNoAds()
        {
            EnsureCatalog();
            if (_catalog == null)
            {
                return;
            }

            MockPurchaseService.Instance.Purchase(_catalog.noAdsProductId, PurchaseProductType.NoAds);
            Refresh();
        }

        public void BackToMenu()
        {
            SceneNavigator.Instance?.ReturnFromStore();
        }

        private void Refresh()
        {
            RefreshLocalizedTexts();

            if (themeStatusLabel != null)
            {
                bool unlocked = EconomyManager.Instance != null && EconomyManager.Instance.IsThemeUnlocked(GameConstants.PremiumMythologyThemeId);
                int themeCoinPrice = LevelEconomyManager.Instance != null
                    ? LevelEconomyManager.Instance.ResolveThemeSoftCurrencyPrice(GameConstants.PremiumMythologyThemeId, 1200)
                    : 1200;
                themeStatusLabel.text = unlocked
                    ? GetLocalized("theme_unlocked")
                    : $"{GetLocalized("theme_locked")} ({themeCoinPrice} {GetLocalized("coin_short")})";
            }

            if (themePriceLabel != null)
            {
                StorePriceQuote themeQuote = ResolveThemeQuote();
                themePriceLabel.text = themeQuote.available
                    ? $"{GetLocalized("theme_price")}: {themeQuote.formattedPrice}"
                    : GetLocalized("theme_price_unavailable");
            }

            if (membershipStatusLabel != null)
            {
                bool premium = EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive;
                membershipStatusLabel.text = premium ? GetLocalized("membership_active") : GetLocalized("membership_inactive");
            }

            if (membershipPriceLabel != null)
            {
                StorePriceQuote membershipQuote = ResolveMembershipQuote();
                membershipPriceLabel.text = membershipQuote.available
                    ? $"{GetLocalized("membership_price")}: {membershipQuote.formattedPrice}"
                    : GetLocalized("membership_price_unavailable");
            }

            if (coinStatusLabel != null)
            {
                int softCurrency = EconomyManager.Instance != null ? EconomyManager.Instance.SoftCurrency : 0;
                coinStatusLabel.text = $"{GetLocalized("coin_balance")}: {softCurrency}";
            }

            if (comingSoonLabel != null)
            {
                EconomyBalanceProfile balanceProfile = LevelEconomyManager.Instance != null ? LevelEconomyManager.Instance.Profile : null;
                comingSoonLabel.gameObject.SetActive(balanceProfile == null || balanceProfile.ShowUpcomingThemesTeaser);
                comingSoonLabel.text = $"{GetLocalized("coming_soon")}\n{GetLocalized("pricing_note")}";
            }
        }

        private void EnsureCatalog()
        {
            if (_catalog == null && ContentService.Instance != null)
            {
                _catalog = ContentService.Instance.LoadStoreCatalog();
            }
        }

        private void HandleLanguageChanged(string _)
        {
            Refresh();
        }

        private void HandleSoftCurrencyChanged(int _, int __)
        {
            Refresh();
        }

        private void HandleMembershipChanged(bool _)
        {
            Refresh();
        }

        private void RefreshLocalizedTexts()
        {
            SetText("Title", GetLocalized("title"));
            SetText("ThemeButton/Label", GetLocalized("unlock_theme"));
            SetText("HintsButton/Label", GetLocalized("buy_hints"));
            SetText("EnergyButton/Label", GetLocalized("buy_energy"));
            SetText("PremiumButton/Label", GetLocalized("membership_button"));
            SetText("BackButton/Label", GetLocalized("back"));
        }

        private void EnsureRuntimeLabels()
        {
            TMP_FontAsset font = themeStatusLabel != null ? themeStatusLabel.font : TMP_Settings.defaultFontAsset;
            if (coinStatusLabel == null)
            {
                Transform existing = transform.Find("CoinStatus");
                if (existing != null)
                {
                    coinStatusLabel = existing.GetComponent<TextMeshProUGUI>();
                }
            }

            if (comingSoonLabel == null)
            {
                Transform existing = transform.Find("ComingSoon");
                if (existing != null)
                {
                    comingSoonLabel = existing.GetComponent<TextMeshProUGUI>();
                }
            }

            if (themePriceLabel == null)
            {
                Transform existing = transform.Find("ThemePrice");
                if (existing != null)
                {
                    themePriceLabel = existing.GetComponent<TextMeshProUGUI>();
                }
            }

            if (membershipPriceLabel == null)
            {
                Transform existing = transform.Find("MembershipPrice");
                if (existing != null)
                {
                    membershipPriceLabel = existing.GetComponent<TextMeshProUGUI>();
                }
            }

            if (coinStatusLabel == null)
            {
                coinStatusLabel = CreateRuntimeLabel("CoinStatus", font, new Vector2(0.5f, 0.58f), new Vector2(760f, 42f), 24f, new Color(0.92f, 0.90f, 0.86f));
            }

            if (comingSoonLabel == null)
            {
                comingSoonLabel = CreateRuntimeLabel("ComingSoon", font, new Vector2(0.5f, 0.52f), new Vector2(820f, 52f), 22f, new Color(0.96f, 0.78f, 0.52f));
            }

            if (themePriceLabel == null)
            {
                themePriceLabel = CreateRuntimeLabel("ThemePrice", font, new Vector2(0.5f, 0.74f), new Vector2(760f, 38f), 22f, new Color(0.88f, 0.83f, 0.75f));
            }

            if (membershipPriceLabel == null)
            {
                membershipPriceLabel = CreateRuntimeLabel("MembershipPrice", font, new Vector2(0.5f, 0.34f), new Vector2(760f, 38f), 22f, new Color(0.88f, 0.83f, 0.75f));
            }
        }

        private TextMeshProUGUI CreateRuntimeLabel(string name, TMP_FontAsset font, Vector2 anchor, Vector2 size, float fontSize, Color color)
        {
            GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(transform, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = TextAlignmentOptions.Center;
            label.enableWordWrapping = true;
            label.color = color;
            return label;
        }

        private void SetText(string path, string value)
        {
            Transform child = transform.Find(path);
            if (child == null)
            {
                return;
            }

            TextMeshProUGUI label = child.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = value;
            }
        }

        private static StorePriceQuote ResolveMembershipQuote()
        {
            string language = SaveManager.Instance != null ? SaveManager.Instance.Data.languageCode : GameConstants.DefaultLanguageCode;
            if (StorePricingManager.Instance != null)
            {
                return StorePricingManager.Instance.GetMembershipQuote(language);
            }

            return default;
        }

        private static StorePriceQuote ResolveThemeQuote()
        {
            string language = SaveManager.Instance != null ? SaveManager.Instance.Data.languageCode : GameConstants.DefaultLanguageCode;
            if (StorePricingManager.Instance != null)
            {
                return StorePricingManager.Instance.GetThemeQuote(GameConstants.PremiumMythologyThemeId, language);
            }

            return default;
        }

        private static string GetLocalized(string key)
        {
            string language = SaveManager.Instance != null ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode) : GameConstants.DefaultLanguageCode;
            switch (language)
            {
                case "en":
                    return key switch
                    {
                        "title" => "Theme Store",
                        "unlock_theme" => "Unlock Theme",
                        "buy_hints" => "Buy Hints",
                        "buy_energy" => "Buy Energy",
                        "membership_button" => "Membership",
                        "back" => "Back",
                        "coin_balance" => "Store Coins",
                        "coin_short" => "coins",
                        "theme_price" => "Theme Pack",
                        "theme_price_unavailable" => "Theme Pack: price preview unavailable",
                        "membership_price" => "Membership",
                        "membership_price_unavailable" => "Membership: price preview unavailable",
                        "pricing_note" => "Preview prices follow language defaults for testing. Final release will use Play Store regional storefront pricing.",
                        "coming_soon" => "Premium theme packs are coming soon. Save your coins or unlock them faster with membership.",
                        "theme_unlocked" => "Mythology Theme: Unlocked",
                        "theme_locked" => "Mythology Theme: Locked",
                        "membership_active" => "Membership: Active",
                        "membership_inactive" => "Membership: Inactive",
                        _ => key
                    };
                case "es":
                    return key switch
                    {
                        "title" => "Tienda",
                        "unlock_theme" => "Desbloquear tema",
                        "buy_hints" => "Comprar pistas",
                        "buy_energy" => "Comprar energia",
                        "membership_button" => "Membresia",
                        "back" => "Volver",
                        "coin_balance" => "Monedas de tienda",
                        "coin_short" => "mon",
                        "theme_price" => "Paquete tematico",
                        "theme_price_unavailable" => "Paquete tematico: precio no disponible",
                        "membership_price" => "Membresia",
                        "membership_price_unavailable" => "Membresia: precio no disponible",
                        "pricing_note" => "Los precios de prueba siguen el idioma para test. La version final usara precios regionales de Play Store.",
                        "coming_soon" => "Los paquetes de temas premium llegaran pronto. Guarda monedas o desbloquealos mas rapido con membresia.",
                        "theme_unlocked" => "Tema mitologico: desbloqueado",
                        "theme_locked" => "Tema mitologico: bloqueado",
                        "membership_active" => "Membresia: activa",
                        "membership_inactive" => "Membresia: inactiva",
                        _ => key
                    };
                case "de":
                    return key switch
                    {
                        "title" => "Shop",
                        "unlock_theme" => "Theme freischalten",
                        "buy_hints" => "Tipps kaufen",
                        "buy_energy" => "Energie kaufen",
                        "membership_button" => "Mitgliedschaft",
                        "back" => "Zuruck",
                        "coin_balance" => "Shop-Muenzen",
                        "coin_short" => "Mz",
                        "theme_price" => "Theme-Paket",
                        "theme_price_unavailable" => "Theme-Paket: keine Preisvorschau",
                        "membership_price" => "Mitgliedschaft",
                        "membership_price_unavailable" => "Mitgliedschaft: keine Preisvorschau",
                        "pricing_note" => "Testpreise folgen der Sprachzuordnung. Im finalen Release kommt der Preis aus dem regionalen Play-Store-Storefront.",
                        "coming_soon" => "Premium-Theme-Pakete kommen bald. Spare Muenzen oder schalte sie schneller mit Mitgliedschaft frei.",
                        "theme_unlocked" => "Mythologie-Theme: Frei",
                        "theme_locked" => "Mythologie-Theme: Gesperrt",
                        "membership_active" => "Mitgliedschaft: Aktiv",
                        "membership_inactive" => "Mitgliedschaft: Inaktiv",
                        _ => key
                    };
                default:
                    return key switch
                    {
                        "title" => "Tema Magazasi",
                        "unlock_theme" => "Tema Ac",
                        "buy_hints" => "Ipucu Al",
                        "buy_energy" => "Enerji Al",
                        "membership_button" => "Uyelik",
                        "back" => "Geri",
                        "coin_balance" => "Magaza Coini",
                        "coin_short" => "coin",
                        "theme_price" => "Tema Paketi",
                        "theme_price_unavailable" => "Tema Paketi: fiyat onizlemesi yok",
                        "membership_price" => "Premium Uyelik",
                        "membership_price_unavailable" => "Premium Uyelik: fiyat onizlemesi yok",
                        "pricing_note" => "Test fiyatlari simdilik dil varsayimina gore gosterilir. Nihai surumde fiyat Play Store bolgesel storefront verisinden gelecek.",
                        "coming_soon" => "Premium tema paketleri cok yakinda. Coin biriktir veya uyelikle daha hizli eris.",
                        "theme_unlocked" => "Mitoloji Temasi: Acik",
                        "theme_locked" => "Mitoloji Temasi: Kilitli",
                        "membership_active" => "Uyelik: Aktif",
                        "membership_inactive" => "Uyelik: Pasif",
                        _ => key
                    };
            }
        }
    }
}
