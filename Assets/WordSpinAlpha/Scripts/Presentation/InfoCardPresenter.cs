using System;
using TMPro;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Services;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class InfoCardPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI bodyLabel;

        private InfoCardCatalog _catalog;
        private string _currentInfoCardId = string.Empty;

        private void Awake()
        {
            EnsureCatalog();
        }

        private void OnEnable()
        {
            GameEvents.InfoCardRequested += ShowCard;
            GameEvents.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            GameEvents.InfoCardRequested -= ShowCard;
            GameEvents.LanguageChanged -= HandleLanguageChanged;
        }

        public void ShowCard(string infoCardId)
        {
            EnsureCatalog();
            _currentInfoCardId = infoCardId ?? string.Empty;

            if (root != null)
            {
                root.SetActive(true);
            }

            RefreshLocalizedTexts();

            if (_catalog == null || _catalog.cards == null)
            {
                return;
            }

            foreach (InfoCardDefinition card in _catalog.cards)
            {
                if (card.infoCardId != infoCardId)
                {
                    continue;
                }

                string language = SaveManager.Instance != null ? SaveManager.Instance.Data.languageCode : Lang.TR;
                if (titleLabel != null)
                {
                    titleLabel.text = card.title != null ? card.title.Get(language) : string.Empty;
                }

                if (bodyLabel != null)
                {
                    bodyLabel.text = card.body != null ? card.body.Get(language) : string.Empty;
                }

                return;
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }

            GameEvents.RaiseInfoCardClosed();
        }

        public void RefreshContentCache()
        {
            _catalog = null;
            if (root != null && root.activeSelf && !string.IsNullOrWhiteSpace(_currentInfoCardId))
            {
                ShowCard(_currentInfoCardId);
                return;
            }

            RefreshLocalizedTexts();
        }

        public void RefreshForEditor()
        {
            EnsureCatalog();
            if (root != null && root.activeSelf && !string.IsNullOrWhiteSpace(_currentInfoCardId))
            {
                ShowCard(_currentInfoCardId);
                return;
            }

            RefreshLocalizedTexts();
        }

        private void EnsureCatalog()
        {
            if (_catalog == null && ContentService.Instance != null)
            {
                _catalog = ContentService.Instance.LoadInfoCards();
            }
        }

        private void HandleLanguageChanged(string _)
        {
            _catalog = null;
            RefreshLocalizedTexts();
        }

        private void RefreshLocalizedTexts()
        {
            SetButtonLabel("InfoClose", GetLocalized("continue"));
        }

        private void SetButtonLabel(string objectName, string value)
        {
            if (root == null)
            {
                return;
            }

            Transform target = root.transform.Find(objectName);
            if (target == null)
            {
                return;
            }

            TextMeshProUGUI label = target.GetComponentInChildren<TextMeshProUGUI>(true);
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
                        "continue" => "Continue",
                        _ => key
                    };
                case "es":
                    return key switch
                    {
                        "continue" => "Continuar",
                        _ => key
                    };
                case "de":
                    return key switch
                    {
                        "continue" => "Weiter",
                        _ => key
                    };
                default:
                    return key switch
                    {
                        "continue" => "Devam Et",
                        _ => key
                    };
            }
        }
    }
}
