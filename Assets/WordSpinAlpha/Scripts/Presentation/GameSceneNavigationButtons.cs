using TMPro;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class GameSceneNavigationButtons : MonoBehaviour
    {
        private GameplayPausePresenter _pausePresenter;

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
            _pausePresenter = GameplayPausePresenter.EnsureInScene();
            RefreshLocalizedTexts();
        }

        public void OpenMainMenu()
        {
            if (_pausePresenter == null)
            {
                _pausePresenter = GameplayPausePresenter.EnsureInScene();
            }

            if (_pausePresenter != null)
            {
                _pausePresenter.OpenPause();
                return;
            }

            SessionManager.Instance?.TakeSnapshot();
            SceneNavigator.Instance?.OpenMainMenu();
        }

        public void OpenStore()
        {
            if (InputManager.Instance != null && !InputManager.Instance.CanAcceptGameplayInput)
            {
                return;
            }

            SceneNavigator.Instance?.OpenStore();
        }

        private void HandleLanguageChanged(string _)
        {
            RefreshLocalizedTexts();
        }

        private void RefreshLocalizedTexts()
        {
            SetButtonLabel("MenuOpen", GetLocalized("pause"));
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
                        "pause" => "Pause",
                        "store" => "Store",
                        "swipe_hint" => "Tap a letter, then swipe up",
                        _ => key
                    };
                case "es":
                    return key switch
                    {
                        "pause" => "Pausa",
                        "store" => "Tienda",
                        "swipe_hint" => "Toca una letra y desliza hacia arriba",
                        _ => key
                    };
                case "de":
                    return key switch
                    {
                        "pause" => "Pause",
                        "store" => "Shop",
                        "swipe_hint" => "Tippe einen Buchstaben und wische nach oben",
                        _ => key
                    };
                default:
                    return key switch
                    {
                        "pause" => "Duraklat",
                        "store" => "Magaza",
                        "swipe_hint" => "Bir harfe dokun, sonra yukari kaydir",
                        _ => key
                    };
            }
        }
    }
}
