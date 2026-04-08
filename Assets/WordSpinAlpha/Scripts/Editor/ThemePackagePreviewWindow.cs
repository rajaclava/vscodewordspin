using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    public class ThemePackagePreviewWindow : EditorWindow
    {
        private Vector2 _scroll;
        private ThemeCatalog _catalog;
        private readonly List<bool> _foldouts = new List<bool>();
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Tema Paketi Onizleme")]
        public static void Open()
        {
            GetWindow<ThemePackagePreviewWindow>("Tema Onizleme");
        }

        private void OnEnable()
        {
            Reload();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Tema Paketi Onizleme", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Tema kaynak yollarini, renk swatch'larini ve play sirasinda tema preview uygulamasini kontrol eder. Tema metadata duzenleme icin Tema ve Magaza Config penceresi kullanilir.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Yeniden Yukle", GUILayout.Height(28f)))
                {
                    Reload();
                }

                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    if (GUILayout.Button("Aktif Temayi Yenile", GUILayout.Height(28f)))
                    {
                        ThemeRuntimeController themeRuntime = Object.FindObjectOfType<ThemeRuntimeController>(true);
                        if (themeRuntime != null)
                        {
                            themeRuntime.RefreshForEditor();
                        }
                    }
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            if (_catalog == null || _catalog.themes == null || _catalog.themes.Length == 0)
            {
                EditorGUILayout.HelpBox("Tema katalogu bos.", MessageType.Warning);
            }
            else
            {
                for (int i = 0; i < _catalog.themes.Length; i++)
                {
                    ThemePackDefinition theme = _catalog.themes[i];
                    EditorGUILayout.BeginVertical("box");
                    _foldouts[i] = EditorGUILayout.Foldout(_foldouts[i], $"{theme.displayName} | {theme.themeId}", true);
                    if (_foldouts[i])
                    {
                        DrawTheme(theme);
                    }
                    EditorGUILayout.EndVertical();
                    EditorGUILayout.Space(4f);
                }
            }
            EditorGUILayout.EndScrollView();
        }

        private void Reload()
        {
            _catalog = WordSpinAlphaRuntimeConfigRepository.LoadThemes();
            if (_catalog == null || _catalog.themes == null)
            {
                _catalog = new ThemeCatalog { themes = System.Array.Empty<ThemePackDefinition>() };
            }

            while (_foldouts.Count < _catalog.themes.Length)
            {
                _foldouts.Add(false);
            }

            while (_foldouts.Count > _catalog.themes.Length)
            {
                _foldouts.RemoveAt(_foldouts.Count - 1);
            }
        }

        private void TryAutoRefresh()
        {
            if (!WordSpinAlphaEditorSyncUtility.ConsumeChanges(WordSpinAlphaEditorSyncKind.RuntimeConfig, ref _syncStamp))
            {
                return;
            }

            Reload();
            Repaint();
        }

        private void DrawTheme(ThemePackDefinition theme)
        {
            EditorGUILayout.LabelField("Kategori", string.IsNullOrWhiteSpace(theme.themeCategory) ? "-" : theme.themeCategory);
            EditorGUILayout.LabelField("Mechanical Identity", string.IsNullOrWhiteSpace(theme.mechanicalIdentity) ? "-" : theme.mechanicalIdentity);
            EditorGUILayout.LabelField("Premium Paket", theme.isPremium ? "Evet" : "Hayir");
            DrawColorSwatch("UI Primary", theme.uiPrimaryHex);
            DrawColorSwatch("UI Accent", theme.uiAccentHex);
            DrawColorSwatch("UI Background", theme.uiBackgroundHex);
            EditorGUILayout.Space(4f);

            DrawResourceStatus("Arka Plan", theme.backgroundResourcePath, typeof(Sprite));
            DrawResourceStatus("Rotator", theme.rotatorResourcePath, typeof(Sprite));
            DrawResourceStatus("Pin", theme.pinResourcePath, typeof(Sprite));
            DrawResourceStatus("Klavye Skin", theme.keyboardSkinResourcePath, typeof(Object));
            DrawResourceStatus("Launch VFX", theme.launchVfxResourcePath, typeof(Object));
            DrawResourceStatus("Impact VFX", theme.impactVfxResourcePath, typeof(Object));
            DrawResourceStatus("Complete VFX", theme.completeVfxResourcePath, typeof(Object));
            DrawResourceStatus("BGM", theme.bgmResourcePath, typeof(AudioClip));
            DrawResourceStatus("Hit SFX", theme.hitSfxResourcePath, typeof(AudioClip));
            DrawResourceStatus("Miss SFX", theme.missSfxResourcePath, typeof(AudioClip));
            DrawResourceStatus("Completion SFX", theme.completionSfxResourcePath, typeof(AudioClip));

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                if (GUILayout.Button("Playde Bu Temayi Uygula"))
                {
                    if (EconomyManager.Instance != null)
                    {
                        EconomyManager.Instance.SetActiveTheme(theme.themeId);
                    }

                    ThemeRuntimeController themeRuntime = Object.FindObjectOfType<ThemeRuntimeController>(true);
                    if (themeRuntime != null)
                    {
                        themeRuntime.RefreshForEditor();
                    }

                    WordSpinAlphaEditorRuntimeRefreshUtility.RefreshUiPresentation();
                }
            }
        }

        private static void DrawColorSwatch(string label, string hex)
        {
            Color preview = Color.black;
            bool valid = TryParseHex(hex, out preview);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(label, GUILayout.Width(120f));
                EditorGUI.DrawRect(GUILayoutUtility.GetRect(56f, 18f), valid ? preview : new Color(0.45f, 0.12f, 0.12f));
                EditorGUILayout.LabelField(valid ? hex : "Gecersiz HEX");
            }
        }

        private static void DrawResourceStatus(string label, string resourcePath, System.Type resourceType)
        {
            bool missingPath = string.IsNullOrWhiteSpace(resourcePath);
            Object loaded = missingPath ? null : Resources.Load(resourcePath, resourceType);
            Color color = missingPath
                ? new Color(0.82f, 0.64f, 0.24f)
                : loaded != null
                    ? new Color(0.24f, 0.72f, 0.38f)
                    : new Color(0.84f, 0.28f, 0.22f);

            using (new EditorGUILayout.HorizontalScope())
            {
                Rect rect = GUILayoutUtility.GetRect(14f, 14f, GUILayout.Width(14f));
                EditorGUI.DrawRect(rect, color);
                EditorGUILayout.LabelField(label, GUILayout.Width(120f));
                EditorGUILayout.LabelField(missingPath ? "(bos)" : resourcePath);
            }
        }

        private static bool TryParseHex(string hex, out Color color)
        {
            color = Color.white;
            if (string.IsNullOrWhiteSpace(hex))
            {
                return false;
            }

            string normalized = hex.StartsWith("#", System.StringComparison.Ordinal) ? hex : $"#{hex}";
            return ColorUtility.TryParseHtmlString(normalized, out color);
        }
    }
}
