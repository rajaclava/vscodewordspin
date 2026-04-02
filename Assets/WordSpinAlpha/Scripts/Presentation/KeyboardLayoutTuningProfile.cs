using System;
using System.Collections.Generic;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    [CreateAssetMenu(fileName = "KeyboardLayoutTuningProfile", menuName = "WordSpin Alpha/Keyboard Layout Tuning Profile")]
    public class KeyboardLayoutTuningProfile : ScriptableObject
    {
        public const string DefaultResourcePath = "Configs/KeyboardLayoutTuningProfile";

        [Serializable]
        public class LanguageTuning
        {
            public string languageCode = GameConstants.DefaultLanguageCode;

            [Header("Klavye Dock Yerlesimi")]
            public Vector2 bottomBarAnchoredPosition = new Vector2(0f, -18f);
            public Vector2 bottomBarSize = new Vector2(1080f, 362f);
            public Vector2 keyboardSkinFrameAnchoredPosition = new Vector2(0f, -8f);
            public Vector2 keyboardSkinFrameSize = new Vector2(1000f, 320f);
            public Vector2 keyboardGridAnchoredPosition = new Vector2(0f, -2f);
            public Vector2 keyboardGridSize = new Vector2(964f, 292f);
            public Vector2 keyboardCellSize = new Vector2(72f, 82f);
            public Vector2 keyboardSpacing = new Vector2(2f, 6f);
            public Vector2 menuButtonAnchors = new Vector2(0.16f, 1f);
            public Vector2 storeButtonAnchors = new Vector2(0.84f, 1f);
            public Vector2 navButtonSize = new Vector2(156f, 52f);
            public float navButtonTopOffset = 34f;
            public Vector2 swipeHintAnchors = new Vector2(0.5f, 1.24f);

            [Header("Tus Yerlesimi")]
            public float horizontalPadding = 20f;
            public float topPadding = 8f;
            public float bottomPadding = 8f;
            public float columnSpacing = 2f;
            public float rowSpacing = 6f;

            [Header("Tus Boyutlari")]
            public float maxButtonWidth = 72f;
            public float maxButtonHeight = 86f;
            public float minButtonWidth = 48f;
            public float minButtonHeight = 42f;
            public float buttonAspectRatio = 0.88f;

            [Header("Yazi")]
            public float minLabelFontSize = 18f;
            public float maxLabelFontSize = 26f;
        }

        [SerializeField] private List<LanguageTuning> languages = new List<LanguageTuning>();

        public IReadOnlyList<LanguageTuning> Languages => languages;

        public LanguageTuning GetLanguageTuning(string languageCode)
        {
            string normalized = GameConstants.NormalizeLanguageCode(languageCode);
            for (int i = 0; i < languages.Count; i++)
            {
                LanguageTuning tuning = languages[i];
                if (tuning != null && GameConstants.NormalizeLanguageCode(tuning.languageCode) == normalized)
                {
                    return tuning;
                }
            }

            return null;
        }

        public void EnsureDefaults()
        {
            EnsureLanguageEntry("tr", 20f, 72f);
            EnsureLanguageEntry("en", 20f, 72f);
            EnsureLanguageEntry("es", 20f, 72f);
            EnsureLanguageEntry("de", 44f, 66f);
        }

        public void ResetToDefaults()
        {
            languages = new List<LanguageTuning>();
            EnsureDefaults();
        }

        private void EnsureLanguageEntry(string languageCode, float horizontalPaddingOverride, float maxButtonWidthOverride)
        {
            LanguageTuning existing = GetLanguageTuning(languageCode);
            if (existing != null)
            {
                existing.languageCode = languageCode;
                return;
            }

            LanguageTuning entry = new LanguageTuning
            {
                languageCode = languageCode,
                horizontalPadding = horizontalPaddingOverride,
                maxButtonWidth = maxButtonWidthOverride
            };

            languages.Add(entry);
        }
    }
}
