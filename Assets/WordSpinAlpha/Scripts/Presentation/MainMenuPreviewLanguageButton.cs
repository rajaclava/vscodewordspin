using UnityEngine;
using UnityEngine.EventSystems;

namespace WordSpinAlpha.Presentation
{
    [DisallowMultipleComponent]
    public sealed class MainMenuPreviewLanguageButton : MonoBehaviour, IPointerClickHandler
    {
        [SerializeField] private MainMenuLanguagePreviewHighlighter highlighter;
        [SerializeField] private string languageCode = "tr";

        public void Configure(MainMenuLanguagePreviewHighlighter targetHighlighter, string targetLanguageCode)
        {
            highlighter = targetHighlighter;
            languageCode = targetLanguageCode;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (highlighter == null)
            {
                highlighter = GetComponentInParent<MainMenuLanguagePreviewHighlighter>();
            }

            highlighter?.SelectLanguage(languageCode);
        }
    }
}
