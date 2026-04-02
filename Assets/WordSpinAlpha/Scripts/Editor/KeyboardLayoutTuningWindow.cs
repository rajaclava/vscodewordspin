using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    public class KeyboardLayoutTuningWindow : EditorWindow
    {
        private const string AssetPath = "Assets/WordSpinAlpha/Resources/Configs/KeyboardLayoutTuningProfile.asset";
        private const string KeyboardConfigPath = "Assets/WordSpinAlpha/Resources/Configs/keyboard_config.json";

        private static readonly string[] TabLabels = { "Turkce", "Ingilizce", "Ispanyolca", "Almanca" };
        private static readonly string[] TabCodes = { "tr", "en", "es", "de" };
        private static readonly string[] PreviewDeviceLabels =
        {
            "9:16 Referans",
            "9:19.5 Uzun Telefon",
            "9:20 Uzun Android",
            "9:21 Cok Uzun Android"
        };

        private static readonly Vector2[] PreviewDeviceResolutions =
        {
            new Vector2(1080f, 1920f),
            new Vector2(1080f, 2340f),
            new Vector2(1080f, 2400f),
            new Vector2(1080f, 2520f)
        };

        private KeyboardLayoutTuningProfile _profile;
        private Vector2 _scroll;
        private int _selectedTab;
        private int _selectedPreviewDevice;
        private int _copyTargetTab;
        private KeyboardConfigData _keyboardConfig;

        [MenuItem("Tools/WordSpin Alpha/Klavye Yerlesim Editoru")]
        public static void Open()
        {
            GetWindow<KeyboardLayoutTuningWindow>("Klavye Editoru");
        }

        [InitializeOnLoadMethod]
        private static void EnsureProfileOnLoad()
        {
            EditorApplication.delayCall += () =>
            {
                if (EditorApplication.isCompiling)
                {
                    return;
                }

                EnsureProfileAsset();
            };
        }

        private void OnEnable()
        {
            _profile = EnsureProfileAsset();
            _keyboardConfig = LoadKeyboardConfig();
        }

        private void OnGUI()
        {
            _profile = EnsureProfileAsset();
            if (_profile == null)
            {
                EditorGUILayout.HelpBox("Klavye tuning profili olusturulamadi.", MessageType.Error);
                return;
            }

            EditorGUILayout.LabelField("Klavye Yerlesim Editoru", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu editor her dil icin klavye dock yerlesimi, tus boyutu, bosluk ve yazi ayarlarini ayri ayri duzenler. Yaptigin degisiklik o dildeki tum harflere uygulanir.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Profil Assetini Sec", GUILayout.Height(26f)))
                {
                    Selection.activeObject = _profile;
                    EditorGUIUtility.PingObject(_profile);
                }

                if (GUILayout.Button("Aktif Gameplay Sahnesine Uygula", GUILayout.Height(26f)))
                {
                    ApplyToOpenGameplayScene();
                }

                if (GUILayout.Button("Tum Dilleri Varsayilana Dondur", GUILayout.Height(26f)))
                {
                    Undo.RecordObject(_profile, "Klavye Profilini Sifirla");
                    _profile.ResetToDefaults();
                    EditorUtility.SetDirty(_profile);
                    AssetDatabase.SaveAssets();
                }
            }

            EditorGUILayout.Space(6f);
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField("Bu dili su dile kopyala", GUILayout.Width(150f));
                _copyTargetTab = EditorGUILayout.Popup(_copyTargetTab, TabLabels);
                using (new EditorGUI.DisabledScope(_copyTargetTab == _selectedTab))
                {
                    if (GUILayout.Button("Aynisini Aktar", GUILayout.Height(24f), GUILayout.Width(140f)))
                    {
                        CopyLanguageTuning(TabCodes[_selectedTab], TabCodes[_copyTargetTab]);
                    }
                }
            }

            EditorGUILayout.Space(8f);
            _selectedTab = GUILayout.Toolbar(_selectedTab, TabLabels);
            EditorGUILayout.Space(8f);

            KeyboardLayoutTuningProfile.LanguageTuning tuning = _profile.GetLanguageTuning(TabCodes[_selectedTab]);
            if (tuning == null)
            {
                EditorGUILayout.HelpBox("Secilen dil icin tuning bulunamadi.", MessageType.Warning);
                return;
            }

            EditorGUI.BeginChangeCheck();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawLayoutSection(tuning);
            EditorGUILayout.Space(10f);
            DrawButtonSection(tuning);
            EditorGUILayout.Space(10f);
            DrawLabelSection(tuning);
            EditorGUILayout.Space(14f);
            DrawPreviewSection(tuning, TabCodes[_selectedTab]);

            EditorGUILayout.EndScrollView();

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(_profile, "Klavye Yerlesimi Degisti");
                EditorUtility.SetDirty(_profile);
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawPreviewSection(KeyboardLayoutTuningProfile.LanguageTuning tuning, string languageCode)
        {
            EditorGUILayout.LabelField("Onizleme", EditorStyles.boldLabel);
            _selectedPreviewDevice = EditorGUILayout.Popup("Goruntu Orani", _selectedPreviewDevice, PreviewDeviceLabels);

            Rect previewRect = GUILayoutUtility.GetRect(position.width - 32f, 620f, GUILayout.ExpandWidth(true));
            DrawKeyboardPreview(previewRect, tuning, languageCode);
        }

        private static void DrawLayoutSection(KeyboardLayoutTuningProfile.LanguageTuning tuning)
        {
            EditorGUILayout.LabelField("Ekran Yerlesimi", EditorStyles.boldLabel);
            tuning.bottomBarAnchoredPosition = EditorGUILayout.Vector2Field("Alt Bar Konumu", tuning.bottomBarAnchoredPosition);
            tuning.bottomBarSize = EditorGUILayout.Vector2Field("Alt Bar Boyutu", tuning.bottomBarSize);
            tuning.keyboardSkinFrameAnchoredPosition = EditorGUILayout.Vector2Field("Klavye Cerceve Konumu", tuning.keyboardSkinFrameAnchoredPosition);
            tuning.keyboardSkinFrameSize = EditorGUILayout.Vector2Field("Klavye Cerceve Boyutu", tuning.keyboardSkinFrameSize);
            tuning.keyboardGridAnchoredPosition = EditorGUILayout.Vector2Field("Harf Grid Konumu", tuning.keyboardGridAnchoredPosition);
            tuning.keyboardGridSize = EditorGUILayout.Vector2Field("Harf Grid Boyutu", tuning.keyboardGridSize);
            tuning.menuButtonAnchors = EditorGUILayout.Vector2Field("Menu Buton Anchor", tuning.menuButtonAnchors);
            tuning.storeButtonAnchors = EditorGUILayout.Vector2Field("Store Buton Anchor", tuning.storeButtonAnchors);
            tuning.navButtonSize = EditorGUILayout.Vector2Field("Alt Buton Boyutu", tuning.navButtonSize);
            tuning.navButtonTopOffset = EditorGUILayout.FloatField("Alt Buton Ust Ofset", tuning.navButtonTopOffset);
            tuning.swipeHintAnchors = EditorGUILayout.Vector2Field("Swipe Hint Anchor", tuning.swipeHintAnchors);
        }

        private static void DrawButtonSection(KeyboardLayoutTuningProfile.LanguageTuning tuning)
        {
            EditorGUILayout.LabelField("Tus Boyutu ve Bosluk", EditorStyles.boldLabel);
            tuning.horizontalPadding = EditorGUILayout.FloatField("Yatay Ic Bosluk", tuning.horizontalPadding);
            tuning.topPadding = EditorGUILayout.FloatField("Ust Ic Bosluk", tuning.topPadding);
            tuning.bottomPadding = EditorGUILayout.FloatField("Alt Ic Bosluk", tuning.bottomPadding);
            tuning.columnSpacing = EditorGUILayout.FloatField("Harfler Arasi Yatay Bosluk", tuning.columnSpacing);
            tuning.rowSpacing = EditorGUILayout.FloatField("Satirlar Arasi Bosluk", tuning.rowSpacing);
            tuning.keyboardCellSize = EditorGUILayout.Vector2Field("Referans Hucre Boyutu", tuning.keyboardCellSize);
            tuning.keyboardSpacing = EditorGUILayout.Vector2Field("Grid Referans Bosluk", tuning.keyboardSpacing);
            tuning.maxButtonWidth = EditorGUILayout.FloatField("Maksimum Tus Genisligi", tuning.maxButtonWidth);
            tuning.maxButtonHeight = EditorGUILayout.FloatField("Maksimum Tus Yuksekligi", tuning.maxButtonHeight);
            tuning.minButtonWidth = EditorGUILayout.FloatField("Minimum Tus Genisligi", tuning.minButtonWidth);
            tuning.minButtonHeight = EditorGUILayout.FloatField("Minimum Tus Yuksekligi", tuning.minButtonHeight);
            tuning.buttonAspectRatio = EditorGUILayout.FloatField("Tus Orani (Genislik/Yukseklik)", tuning.buttonAspectRatio);
        }

        private static void DrawLabelSection(KeyboardLayoutTuningProfile.LanguageTuning tuning)
        {
            EditorGUILayout.LabelField("Yazi Ayarlari", EditorStyles.boldLabel);
            tuning.minLabelFontSize = EditorGUILayout.FloatField("Minimum Harf Punto", tuning.minLabelFontSize);
            tuning.maxLabelFontSize = EditorGUILayout.FloatField("Maksimum Harf Punto", tuning.maxLabelFontSize);
        }

        private void CopyLanguageTuning(string sourceLanguageCode, string targetLanguageCode)
        {
            KeyboardLayoutTuningProfile.LanguageTuning source = _profile.GetLanguageTuning(sourceLanguageCode);
            KeyboardLayoutTuningProfile.LanguageTuning target = _profile.GetLanguageTuning(targetLanguageCode);
            if (source == null || target == null)
            {
                return;
            }

            Undo.RecordObject(_profile, "Dil Klavye Ayarlarini Kopyala");

            target.bottomBarAnchoredPosition = source.bottomBarAnchoredPosition;
            target.bottomBarSize = source.bottomBarSize;
            target.keyboardSkinFrameAnchoredPosition = source.keyboardSkinFrameAnchoredPosition;
            target.keyboardSkinFrameSize = source.keyboardSkinFrameSize;
            target.keyboardGridAnchoredPosition = source.keyboardGridAnchoredPosition;
            target.keyboardGridSize = source.keyboardGridSize;
            target.keyboardCellSize = source.keyboardCellSize;
            target.keyboardSpacing = source.keyboardSpacing;
            target.menuButtonAnchors = source.menuButtonAnchors;
            target.storeButtonAnchors = source.storeButtonAnchors;
            target.navButtonSize = source.navButtonSize;
            target.navButtonTopOffset = source.navButtonTopOffset;
            target.swipeHintAnchors = source.swipeHintAnchors;
            target.horizontalPadding = source.horizontalPadding;
            target.topPadding = source.topPadding;
            target.bottomPadding = source.bottomPadding;
            target.columnSpacing = source.columnSpacing;
            target.rowSpacing = source.rowSpacing;
            target.maxButtonWidth = source.maxButtonWidth;
            target.maxButtonHeight = source.maxButtonHeight;
            target.minButtonWidth = source.minButtonWidth;
            target.minButtonHeight = source.minButtonHeight;
            target.buttonAspectRatio = source.buttonAspectRatio;
            target.minLabelFontSize = source.minLabelFontSize;
            target.maxLabelFontSize = source.maxLabelFontSize;

            EditorUtility.SetDirty(_profile);
            AssetDatabase.SaveAssets();
            Repaint();
        }

        private static void ApplyToOpenGameplayScene()
        {
            bool appliedAny = false;
            GameplaySceneTuner[] tuners = UnityEngine.Object.FindObjectsOfType<GameplaySceneTuner>(true);
            for (int i = 0; i < tuners.Length; i++)
            {
                GameplaySceneTuner tuner = tuners[i];
                if (tuner == null)
                {
                    continue;
                }

                tuner.ApplyTuning();
                EditorUtility.SetDirty(tuner);
                if (!Application.isPlaying)
                {
                    EditorSceneManager.MarkSceneDirty(tuner.gameObject.scene);
                }

                appliedAny = true;
            }

            if (Application.isPlaying)
            {
                KeyboardPresenter[] keyboards = UnityEngine.Object.FindObjectsOfType<KeyboardPresenter>(true);
                for (int i = 0; i < keyboards.Length; i++)
                {
                    keyboards[i].Build();
                    appliedAny = true;
                }
            }

            if (appliedAny)
            {
                AssetDatabase.SaveAssets();
            }
        }

        private void DrawKeyboardPreview(Rect rect, KeyboardLayoutTuningProfile.LanguageTuning tuning, string languageCode)
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            EditorGUI.DrawRect(rect, new Color(0.11f, 0.11f, 0.12f, 1f));

            Vector2 resolution = PreviewDeviceResolutions[Mathf.Clamp(_selectedPreviewDevice, 0, PreviewDeviceResolutions.Length - 1)];
            float deviceAspect = resolution.y / resolution.x;
            float deviceHeight = Mathf.Min(rect.height - 16f, (rect.width - 16f) * deviceAspect);
            float deviceWidth = deviceHeight / deviceAspect;

            Rect deviceRect = new Rect(
                rect.x + (rect.width - deviceWidth) * 0.5f,
                rect.y + 8f,
                deviceWidth,
                deviceHeight);

            EditorGUI.DrawRect(deviceRect, Color.black);
            Rect screenRect = new Rect(deviceRect.x + 10f, deviceRect.y + 16f, deviceRect.width - 20f, deviceRect.height - 32f);
            EditorGUI.DrawRect(screenRect, new Color(0.05f, 0.08f, 0.10f, 1f));

            float topInset = 18f;
            float sideInset = 12f;
            float bottomInset = 18f;
            if (_selectedPreviewDevice > 0)
            {
                topInset = 26f;
                sideInset = 14f;
                bottomInset = 20f;
            }

            Rect safeRect = new Rect(
                screenRect.x + sideInset,
                screenRect.y + topInset,
                screenRect.width - sideInset * 2f,
                screenRect.height - topInset - bottomInset);
            EditorGUI.DrawRect(safeRect, new Color(1f, 0.92f, 0.15f, 0.10f));
            DrawOutline(safeRect, new Color(1f, 0.92f, 0.15f, 0.9f), 2f);

            Rect keyboardFrameRect = ResolvePreviewRect(safeRect, tuning.keyboardSkinFrameAnchoredPosition, tuning.keyboardSkinFrameSize, tuning.bottomBarAnchoredPosition, tuning.bottomBarSize);
            Rect keyboardGridRect = ResolvePreviewRect(safeRect, tuning.keyboardGridAnchoredPosition, tuning.keyboardGridSize, tuning.bottomBarAnchoredPosition, tuning.bottomBarSize);

            EditorGUI.DrawRect(keyboardFrameRect, new Color(0.38f, 0.69f, 0.90f, 0.88f));
            EditorGUI.DrawRect(new Rect(keyboardFrameRect.x, keyboardFrameRect.y, keyboardFrameRect.width, 5f), new Color(0.48f, 0.82f, 1f, 0.95f));

            string[][] rows = ResolveRows(languageCode);
            if (rows == null || rows.Length == 0)
            {
                return;
            }

            float previewScaleX = keyboardGridRect.width / Mathf.Max(1f, tuning.keyboardGridSize.x);
            float previewScaleY = keyboardGridRect.height / Mathf.Max(1f, tuning.keyboardGridSize.y);
            float buttonWidth = Mathf.Min(tuning.maxButtonWidth, (tuning.keyboardGridSize.x - (tuning.horizontalPadding * 2f) - ((MaxRowLength(rows) - 1) * tuning.columnSpacing)) / MaxRowLength(rows));
            buttonWidth = Mathf.Max(tuning.minButtonWidth, buttonWidth);
            float buttonHeight = buttonWidth / Mathf.Max(0.1f, tuning.buttonAspectRatio);
            buttonHeight = Mathf.Clamp(buttonHeight, tuning.minButtonHeight, tuning.maxButtonHeight);

            int rowCount = rows.Count(r => r != null && r.Length > 0);
            float blockHeight = (rowCount * buttonHeight) + ((rowCount - 1) * tuning.rowSpacing);
            float topY = (blockHeight * 0.5f) - (buttonHeight * 0.5f);

            int drawnRow = 0;
            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                string[] row = rows[rowIndex];
                if (row == null || row.Length == 0)
                {
                    continue;
                }

                float totalWidth = (row.Length * buttonWidth) + ((row.Length - 1) * tuning.columnSpacing);
                float startX = -(totalWidth * 0.5f) + (buttonWidth * 0.5f);
                float rowCenterY = topY - drawnRow * (buttonHeight + tuning.rowSpacing);

                for (int i = 0; i < row.Length; i++)
                {
                    float localCenterX = startX + i * (buttonWidth + tuning.columnSpacing);
                    Rect keyRect = new Rect(
                        keyboardGridRect.center.x + (localCenterX * previewScaleX) - (buttonWidth * previewScaleX * 0.5f),
                        keyboardGridRect.center.y - (rowCenterY * previewScaleY) - (buttonHeight * previewScaleY * 0.5f),
                        buttonWidth * previewScaleX,
                        buttonHeight * previewScaleY);

                    EditorGUI.DrawRect(keyRect, new Color(0.24f, 0.27f, 0.34f, 1f));
                    DrawOutline(keyRect, new Color(0.43f, 0.48f, 0.58f, 0.65f), 1f);
                    GUIStyle style = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontSize = Mathf.Clamp(Mathf.RoundToInt(keyRect.height * 0.28f), 10, 20),
                        normal = { textColor = Color.white }
                    };
                    GUI.Label(keyRect, row[i], style);
                }

                drawnRow++;
            }

            GUIStyle caption = new GUIStyle(EditorStyles.miniBoldLabel)
            {
                normal = { textColor = Color.white }
            };
            GUI.Label(new Rect(rect.x + 8f, rect.y + 8f, rect.width - 16f, 20f), $"Onizleme: {TabLabels[_selectedTab]} / {PreviewDeviceLabels[_selectedPreviewDevice]}", caption);
        }

        private static Rect ResolvePreviewRect(Rect safeRect, Vector2 localAnchoredPosition, Vector2 localSize, Vector2 bottomBarPosition, Vector2 bottomBarSize)
        {
            float scaleX = safeRect.width / 1080f;
            float scaleY = safeRect.height / 1920f;

            Vector2 bottomSize = new Vector2(bottomBarSize.x * scaleX, bottomBarSize.y * scaleY);
            Vector2 bottomCenter = new Vector2(
                safeRect.center.x + (bottomBarPosition.x * scaleX),
                safeRect.yMax + (bottomBarPosition.y * scaleY) - (bottomSize.y * 0.5f));

            Vector2 size = new Vector2(localSize.x * scaleX, localSize.y * scaleY);
            Vector2 center = new Vector2(
                bottomCenter.x + (localAnchoredPosition.x * scaleX),
                bottomCenter.y + (localAnchoredPosition.y * scaleY));

            return new Rect(center.x - size.x * 0.5f, center.y - size.y * 0.5f, size.x, size.y);
        }

        private string[][] ResolveRows(string languageCode)
        {
            if (_keyboardConfig == null || _keyboardConfig.layouts == null)
            {
                return Array.Empty<string[]>();
            }

            KeyboardLayoutData layout = _keyboardConfig.layouts.FirstOrDefault(l => l.languageCode == languageCode);
            if (layout == null || layout.keys == null)
            {
                return Array.Empty<string[]>();
            }

            int[] rowLengths = ResolveRowLengths(languageCode, layout.keys.Length);
            List<string[]> rows = new List<string[]>();
            int offset = 0;
            for (int i = 0; i < rowLengths.Length; i++)
            {
                int count = Mathf.Min(rowLengths[i], layout.keys.Length - offset);
                string[] row = new string[count];
                if (count > 0)
                {
                    Array.Copy(layout.keys, offset, row, 0, count);
                }

                rows.Add(row);
                offset += count;
            }

            return rows.ToArray();
        }

        private static int[] ResolveRowLengths(string languageCode, int keyCount)
        {
            switch (languageCode)
            {
                case "tr":
                    return new[] { 12, 11, 9 };
                case "es":
                    return new[] { 10, 10, 7 };
                case "de":
                    return new[] { 11, 11, 7 };
                case "en":
                default:
                    return keyCount <= 26 ? new[] { 10, 9, 7 } : new[] { 10, 10, Mathf.Max(0, keyCount - 20) };
            }
        }

        private static int MaxRowLength(string[][] rows)
        {
            int max = 1;
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] != null && rows[i].Length > max)
                {
                    max = rows[i].Length;
                }
            }

            return max;
        }

        private static void DrawOutline(Rect rect, Color color, float thickness)
        {
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.yMax - thickness, rect.width, thickness), color);
            EditorGUI.DrawRect(new Rect(rect.x, rect.y, thickness, rect.height), color);
            EditorGUI.DrawRect(new Rect(rect.xMax - thickness, rect.y, thickness, rect.height), color);
        }

        private static KeyboardConfigData LoadKeyboardConfig()
        {
            TextAsset asset = AssetDatabase.LoadAssetAtPath<TextAsset>(KeyboardConfigPath);
            return asset != null ? JsonUtility.FromJson<KeyboardConfigData>(asset.text) : null;
        }

        private static KeyboardLayoutTuningProfile EnsureProfileAsset()
        {
            KeyboardLayoutTuningProfile profile = AssetDatabase.LoadAssetAtPath<KeyboardLayoutTuningProfile>(AssetPath);
            if (profile != null)
            {
                int beforeCount = profile.Languages.Count;
                profile.EnsureDefaults();
                if (profile.Languages.Count != beforeCount)
                {
                    EditorUtility.SetDirty(profile);
                    AssetDatabase.SaveAssets();
                }

                return profile;
            }

            EnsureFolder("Assets/WordSpinAlpha/Resources");
            EnsureFolder("Assets/WordSpinAlpha/Resources/Configs");

            profile = CreateInstance<KeyboardLayoutTuningProfile>();
            profile.ResetToDefaults();
            AssetDatabase.CreateAsset(profile, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return profile;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }

        [Serializable]
        private class KeyboardConfigData
        {
            public KeyboardLayoutData[] layouts;
        }

        [Serializable]
        private class KeyboardLayoutData
        {
            public string languageCode;
            public string[] keys;
        }
    }
}
