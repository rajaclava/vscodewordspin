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

        private StoreCatalogDefinition _catalog;

        private void Awake()
        {
            EnsureCatalog();
        }

        private void OnEnable()
        {
            GameEvents.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            GameEvents.LanguageChanged -= HandleLanguageChanged;
        }

        private void Start()
        {
            Refresh();
        }

        public void BuyMythologyTheme()
        {
            MockPurchaseService.Instance.Purchase(GameConstants.PremiumMythologyThemeId, PurchaseProductType.Theme);
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
                themeStatusLabel.text = unlocked ? GetLocalized("theme_unlocked") : GetLocalized("theme_locked");
            }

            if (membershipStatusLabel != null)
            {
                bool premium = EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive;
                membershipStatusLabel.text = premium ? GetLocalized("membership_active") : GetLocalized("membership_inactive");
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

        private void RefreshLocalizedTexts()
        {
            SetText("Title", GetLocalized("title"));
            SetText("ThemeButton/Label", GetLocalized("unlock_theme"));
            SetText("HintsButton/Label", GetLocalized("buy_hints"));
            SetText("EnergyButton/Label", GetLocalized("buy_energy"));
            SetText("PremiumButton/Label", GetLocalized("membership_button"));
            SetText("BackButton/Label", GetLocalized("back"));
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
