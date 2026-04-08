using TMPro;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class MembershipPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI benefitsLabel;

        private void OnEnable()
        {
            GameEvents.MembershipChanged += HandleMembershipChanged;
            GameEvents.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            GameEvents.MembershipChanged -= HandleMembershipChanged;
            GameEvents.LanguageChanged -= HandleLanguageChanged;
        }

        private void Start()
        {
            HandleMembershipChanged(EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive);
        }

        private void HandleMembershipChanged(bool active)
        {
            if (benefitsLabel == null)
            {
                return;
            }

            benefitsLabel.text = active
                ? GetLocalized("benefits_active")
                : GetLocalized("benefits_inactive");
        }

        private void HandleLanguageChanged(string _)
        {
            HandleMembershipChanged(EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive);
        }

        public void RefreshForEditor()
        {
            HandleMembershipChanged(EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive);
        }

        private static string GetLocalized(string key)
        {
            string language = SaveManager.Instance != null ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode) : GameConstants.DefaultLanguageCode;
            switch (language)
            {
                case "en":
                    return key == "benefits_active"
                        ? "Premium active: no ads, unlimited energy, future themes unlocked."
                        : "Premium inactive: ads enabled, entry energy active, themes bought separately.";
                case "es":
                    return key == "benefits_active"
                        ? "Premium activo: sin anuncios, energia ilimitada y futuros temas abiertos."
                        : "Premium inactivo: anuncios activos, energia limitada y temas por separado.";
                case "de":
                    return key == "benefits_active"
                        ? "Premium aktiv: keine Werbung, unbegrenzte Energie und kunftige Themes frei."
                        : "Premium inaktiv: Werbung aktiv, Energie begrenzt und Themes separat.";
                default:
                    return key == "benefits_active"
                        ? "Premium aktif: reklamsiz, sinirsiz enerji ve gelecek temalar acik."
                        : "Premium pasif: reklam aktif, giris enerjisi sinirli ve temalar ayridir.";
            }
        }
    }
}
