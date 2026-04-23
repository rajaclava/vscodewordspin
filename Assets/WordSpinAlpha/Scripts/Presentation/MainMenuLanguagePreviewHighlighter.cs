using System;
using System.IO;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    [ExecuteAlways]
    public sealed class MainMenuLanguagePreviewHighlighter : MonoBehaviour
    {
        [SerializeField] private CanvasGroup[] languageHighlights;
        [SerializeField] private float inactiveAlpha = 0f;
        [SerializeField] private float minimumSelectedAlpha = 0.62f;
        [SerializeField] private float maximumSelectedAlpha = 0.98f;
        [SerializeField] private float pulseSpeed = 2.2f;

        private static readonly string[] LanguageCodes = { "tr", "en", "de", "es" };
        private int activeIndex = -1;

        private void OnEnable()
        {
            if (Application.isPlaying)
            {
                GameEvents.LanguageChanged += HandleLanguageChanged;
            }

            Refresh();
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                GameEvents.LanguageChanged -= HandleLanguageChanged;
            }
        }

        private void Update()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            if (activeIndex < 0 || languageHighlights == null || activeIndex >= languageHighlights.Length)
            {
                return;
            }

            CanvasGroup selected = languageHighlights[activeIndex];
            if (selected == null)
            {
                return;
            }

            float pulse = (Mathf.Sin(Time.unscaledTime * pulseSpeed) + 1f) * 0.5f;
            selected.alpha = Mathf.Lerp(minimumSelectedAlpha, maximumSelectedAlpha, pulse);
        }

        public void Refresh()
        {
            SetActiveLanguage(ResolveLanguageCode());
        }

        public void SelectTR() => SelectLanguage("tr");

        public void SelectEN() => SelectLanguage("en");

        public void SelectDE() => SelectLanguage("de");

        public void SelectES() => SelectLanguage("es");

        public void SelectLanguage(string languageCode)
        {
            string normalized = GameConstants.NormalizeLanguageCode(languageCode);
            PersistLanguage(normalized);
            SetActiveLanguage(normalized);

            if (Application.isPlaying)
            {
                GameEvents.RaiseLanguageChanged(normalized);
            }
        }

        private void HandleLanguageChanged(string languageCode)
        {
            SetActiveLanguage(languageCode);
        }

        private void SetActiveLanguage(string languageCode)
        {
            string normalized = GameConstants.NormalizeLanguageCode(languageCode);
            activeIndex = ResolveIndex(normalized);

            if (languageHighlights == null)
            {
                return;
            }

            for (int i = 0; i < languageHighlights.Length; i++)
            {
                CanvasGroup highlight = languageHighlights[i];
                if (highlight == null)
                {
                    continue;
                }

                bool selected = i == activeIndex;
                highlight.alpha = selected ? maximumSelectedAlpha : inactiveAlpha;
                highlight.gameObject.SetActive(selected);
            }
        }

        private static int ResolveIndex(string languageCode)
        {
            for (int i = 0; i < LanguageCodes.Length; i++)
            {
                if (string.Equals(LanguageCodes[i], languageCode, StringComparison.OrdinalIgnoreCase))
                {
                    return i;
                }
            }

            return 0;
        }

        private static string ResolveLanguageCode()
        {
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                return SaveManager.Instance.Data.languageCode;
            }

            try
            {
                string savePath = Path.Combine(Application.persistentDataPath, GameConstants.SaveFileName);
                if (!File.Exists(savePath))
                {
                    return GameConstants.DefaultLanguageCode;
                }

                PlayerSaveData data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(savePath));
                return data != null ? data.languageCode : GameConstants.DefaultLanguageCode;
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[MainMenuLanguagePreviewHighlighter] Dil kaydi okunamadi, varsayilan dil kullaniliyor. {exception.Message}");
                return GameConstants.DefaultLanguageCode;
            }
        }

        private static void PersistLanguage(string languageCode)
        {
            string normalized = GameConstants.NormalizeLanguageCode(languageCode);
            if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            {
                SaveManager.Instance.Data.languageCode = normalized;
                SaveManager.Instance.Data.progress.EnsureLanguageProgressMigrated(normalized);
                SaveManager.Instance.Save();
                return;
            }

            try
            {
                string savePath = Path.Combine(Application.persistentDataPath, GameConstants.SaveFileName);
                PlayerSaveData data = null;
                if (File.Exists(savePath))
                {
                    data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(savePath));
                }

                data ??= new PlayerSaveData();
                data.languageCode = normalized;
                data.progress.EnsureLanguageProgressMigrated(normalized);
                File.WriteAllText(savePath, JsonUtility.ToJson(data, true));
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"[MainMenuLanguagePreviewHighlighter] Dil kaydi yazilamadi. {exception.Message}");
            }
        }
    }
}
