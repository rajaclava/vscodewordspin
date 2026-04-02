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

            if (root != null)
            {
                root.SetActive(true);
            }

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
        }
    }
}
