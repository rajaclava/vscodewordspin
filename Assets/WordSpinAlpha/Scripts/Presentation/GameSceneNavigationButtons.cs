using TMPro;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class GameSceneNavigationButtons : MonoBehaviour
    {
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
            RefreshLocalizedTexts();
        }

        public void OpenMainMenu()
        {
            SessionManager.Instance?.TakeSnapshot();
            SceneNavigator.Instance?.OpenMainMenu();
        }

        public void OpenStore()
        {
            SceneNavigator.Instance?.OpenStore();
        }

        private void HandleLanguageChanged(string _)
        {
            RefreshLocalizedTexts();
        }

        private void RefreshLocalizedTexts()
        {
            SetButtonLabel("MenuOpen", GetLocalized("menu"));
            SetButtonLabel("StoreOpen", GetLocalized("store"));
            SetStandaloneLabel("SwipeHint", GetLocalized("swipe_hint"));
        }

        private static void SetButtonLabel(string objectName, string value)
        {
            GameObject root = GameObject.Find(objectName);
            if (root == null)
            {
                return;
            }

            TextMeshProUGUI label = root.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = value;
            }
        }

        private static void SetStandaloneLabel(string objectName, string value)
        {
            GameObject root = GameObject.Find(objectName);
            if (root == null)
            {
                return;
            }

            TextMeshProUGUI label = root.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = value;
            }
        }

        private static string GetLocalized(string key)
        {
            string language = SaveManager.Instance != null
                ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode)
                : GameConstants.DefaultLanguageCode;

            switch (language)
            {
                case "en":
                    return key switch
                    {
                        "menu" => "Menu",
                        "store" => "Store",
                        "swipe_hint" => "Tap a letter, then swipe up",
                        _ => key
                    };
                case "es":
                    return key switch
                    {
                        "menu" => "Menu",
                        "store" => "Tienda",
                        "swipe_hint" => "Toca una letra y desliza hacia arriba",
                        _ => key
                    };
                case "de":
                    return key switch
                    {
                        "menu" => "Menu",
                        "store" => "Shop",
                        "swipe_hint" => "Tippe einen Buchstaben und wische nach oben",
                        _ => key
                    };
                default:
                    return key switch
                    {
                        "menu" => "Menu",
                        "store" => "Magaza",
                        "swipe_hint" => "Bir harfe dokun, sonra yukari kaydir",
                        _ => key
                    };
            }
        }
    }
}
