using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    public class WordSpinAlphaContentEditorWindow : EditorWindow
    {
        private enum EditorTab
        {
            Leveller = 0,
            Sekiller = 1,
            Dogrulama = 2
        }

        private static readonly string[] TabLabels = { "Leveller", "Sekil Kutuphanesi", "Dogrulama" };
        private static readonly string[] LanguageLabels = { "Turkce", "Ingilizce", "Ispanyolca", "Almanca" };
        private static readonly string[] ShapeFamilies = { "circle", "oval", "diamond", "square", "hex", "star", "heart", "custom" };

        private WordSpinContentEditorDocument _document;
        private List<ContentEditorValidationIssue> _issues = new List<ContentEditorValidationIssue>();
        private Vector2 _scroll;
        private EditorTab _selectedTab;
        private string _levelSearch = string.Empty;
        private readonly Dictionary<int, bool> _levelFoldouts = new Dictionary<int, bool>();
        private readonly Dictionary<string, bool> _questionFoldouts = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _languageFoldouts = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> _shapeFoldouts = new Dictionary<string, bool>();
        private string _newShapeName = "Yeni Sekil";
        private string _newShapeId = "shape_001";
        private int _newShapeSlotCount = 9;
        private int _newShapeFamilyIndex;
        private int _applyShapeStartLevel = 1;
        private int _applyShapeEndLevel = 1;
        private string _applyShapeLayoutId = string.Empty;
        private string _dragShapeId = string.Empty;
        private int _dragPointIndex = -1;
        private string _rotateShapeId = string.Empty;
        private int _rotatePointIndex = -1;
        private bool _showShapeGuideLines = true;
        private bool _independentManualPreview;

        [MenuItem("Tools/WordSpin Alpha/Icerik ve Level Editoru")]
        public static void Open()
        {
            GetWindow<WordSpinAlphaContentEditorWindow>("Icerik Editoru");
        }

        private void OnEnable()
        {
            ReloadDocument();
        }

        private void OnGUI()
        {
            if (_document == null)
            {
                ReloadDocument();
            }

            if (_document == null)
            {
                EditorGUILayout.HelpBox("Icerik dokumani yuklenemedi.", MessageType.Error);
                return;
            }

            DrawHeader();
            EditorGUILayout.Space(8f);
            _selectedTab = (EditorTab)GUILayout.Toolbar((int)_selectedTab, TabLabels);
            EditorGUILayout.Space(10f);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            switch (_selectedTab)
            {
                case EditorTab.Leveller:
                    DrawLevelsTab();
                    break;
                case EditorTab.Sekiller:
                    DrawShapesTab();
                    break;
                case EditorTab.Dogrulama:
                    DrawValidationTab();
                    break;
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("WordSpin Alpha Icerik ve Level Editoru", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu pencere 4 dil soru/cevap, bilgi karti, level gameplay ayari ve shape kutuphanesini tek yerden yonetir. Dil panelleri ve level satirlari varsayilan olarak kapali gelir.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Yeniden Yukle", GUILayout.Height(28f)))
                {
                    ReloadDocument();
                }

                if (GUILayout.Button("Dogrula", GUILayout.Height(28f)))
                {
                    _issues = WordSpinAlphaContentEditorRepository.Validate(_document);
                    _selectedTab = EditorTab.Dogrulama;
                }

                string saveLabel = EditorApplication.isPlaying ? "Kaydet ve Canli Uygula" : "Tumunu Kaydet";
                if (GUILayout.Button(saveLabel, GUILayout.Height(28f)))
                {
                    SaveDocument();
                }
            }
        }

        private void DrawLevelsTab()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                _levelSearch = EditorGUILayout.TextField("Level Ara", _levelSearch);
                if (GUILayout.Button("Yeni Level", GUILayout.Width(140f), GUILayout.Height(24f)))
                {
                    AddNewLevel();
                }
            }

            EditorGUILayout.Space(8f);
            IEnumerable<LevelContentEditorEntry> levels = _document.levels.OrderBy(level => level.levelId);
            if (!string.IsNullOrWhiteSpace(_levelSearch))
            {
                string search = _levelSearch.Trim().ToLowerInvariant();
                levels = levels.Where(level =>
                    $"seviye {level.levelId}".Contains(search)
                    || (level.shapeLayoutId ?? string.Empty).ToLowerInvariant().Contains(search)
                    || (level.difficultyProfileId ?? string.Empty).ToLowerInvariant().Contains(search)
                    || (level.questions != null && level.questions.Any(question =>
                        (question.questionId ?? string.Empty).ToLowerInvariant().Contains(search)
                        || (question.questionText.tr ?? string.Empty).ToLowerInvariant().Contains(search)
                        || (question.questionText.en ?? string.Empty).ToLowerInvariant().Contains(search)
                        || (question.questionText.es ?? string.Empty).ToLowerInvariant().Contains(search)
                        || (question.questionText.de ?? string.Empty).ToLowerInvariant().Contains(search))));
            }

            foreach (LevelContentEditorEntry level in levels.ToList())
            {
                DrawLevelEntry(level);
            }
        }

        private void DrawShapesTab()
        {
            EditorGUILayout.LabelField("Yeni Sekil", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                _newShapeName = EditorGUILayout.TextField("Gorunen Ad", _newShapeName);
                _newShapeId = EditorGUILayout.TextField("Shape Id", _newShapeId);
                _newShapeFamilyIndex = EditorGUILayout.Popup("Sekil Ailesi", _newShapeFamilyIndex, ShapeFamilies);
                _newShapeSlotCount = EditorGUILayout.IntField("Kutu Sayisi", _newShapeSlotCount);
                if (GUILayout.Button("Yeni Sekli Kutuphaneye Ekle", GUILayout.Height(26f)))
                {
                    AddNewShape();
                }
            }

            EditorGUILayout.Space(10f);
            EditorGUILayout.LabelField("Level Araligina Shape Ata", EditorStyles.boldLabel);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                _applyShapeStartLevel = EditorGUILayout.IntField("Baslangic Level", _applyShapeStartLevel);
                _applyShapeEndLevel = EditorGUILayout.IntField("Bitis Level", _applyShapeEndLevel);
                _applyShapeLayoutId = DrawStringPopup("Sekil", _applyShapeLayoutId, GetShapeIdOptions());
                if (GUILayout.Button("Level Araligina Uygula", GUILayout.Height(26f)))
                {
                    ApplyShapeToLevels();
                }
            }

            EditorGUILayout.Space(10f);
            foreach (ShapeLayoutDefinition shape in _document.shapeLayouts
                .OrderBy(layout => layout.displayName ?? layout.shapeLayoutId)
                .ToList())
            {
                DrawShapeEntry(shape);
            }
        }

        private void DrawValidationTab()
        {
            if (_issues == null || _issues.Count == 0)
            {
                EditorGUILayout.HelpBox("Kayitli dogrulama hatasi yok. 'Dogrula' butonu ile yeniden tarayabilirsin.", MessageType.Info);
                return;
            }

            EditorGUILayout.LabelField($"Toplam Issue: {_issues.Count}", EditorStyles.boldLabel);
            foreach (ContentEditorValidationIssue issue in _issues)
            {
                MessageType type = string.Equals(issue.severity, "Hata", StringComparison.OrdinalIgnoreCase) ? MessageType.Error : MessageType.Warning;
                EditorGUILayout.HelpBox($"{issue.area}\n{issue.message}", type);
            }
        }

        private void DrawLevelEntry(LevelContentEditorEntry level)
        {
            if (level == null)
            {
                return;
            }

            int languageCount = CountFilledLanguages(level);
            bool open = GetFoldoutState(_levelFoldouts, level.levelId);
            string header = $"Seviye {level.levelId} | {level.shapeLayoutId} | {level.difficultyProfileId} | {languageCount}/4 dil dolu";

            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool nextOpen = EditorGUILayout.Foldout(open, header, true);
                _levelFoldouts[level.levelId] = nextOpen;
                if (!nextOpen)
                {
                    return;
                }

                EditorGUILayout.Space(4f);
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Leveli Kopyala", GUILayout.Width(120f)))
                    {
                        DuplicateLevel(level);
                        return;
                    }

                    if (GUILayout.Button("Leveli Sil", GUILayout.Width(100f)))
                    {
                        DeleteLevel(level);
                        return;
                    }
                }

                EditorGUILayout.Space(4f);
                level.levelId = EditorGUILayout.IntField("Level Id", level.levelId);
                level.campaignId = DrawStringPopup("Campaign", level.campaignId, _document.campaignIds);
                level.themeId = DrawStringPopup("Theme", level.themeId, _document.themeIds);
                level.difficultyProfileId = DrawStringPopup("Difficulty Profile", level.difficultyProfileId, _document.difficultyProfileIds);
                level.difficultyTierId = DrawStringPopup("Difficulty Tier", level.difficultyTierId, _document.difficultyTierIds);
                level.rhythmProfileId = DrawStringPopup("Rhythm Profile", level.rhythmProfileId, _document.rhythmProfileIds);
                level.shapeLayoutId = DrawStringPopup("Shape Layout", level.shapeLayoutId, GetShapeIdOptions());
                ShapeLayoutDefinition currentShape = FindShape(level.shapeLayoutId);
                if (currentShape != null)
                {
                    level.shape = currentShape.shapeFamily;
                }

                level.rotationSpeed = EditorGUILayout.FloatField("Rotation Speed", level.rotationSpeed);
                level.clockwise = EditorGUILayout.Toggle("Clockwise", level.clockwise);
                level.randomSlots = EditorGUILayout.Toggle("Random Slots", level.randomSlots);
                level.skipAllowed = EditorGUILayout.Toggle("Skip Allowed", level.skipAllowed);

                DrawObstacleSection(level);

                EditorGUILayout.Space(6f);
                EditorGUILayout.LabelField("Soru Bloglari", EditorStyles.boldLabel);
                for (int i = 0; i < level.questions.Count; i++)
                {
                    DrawQuestionEntry(level, level.questions[i], i);
                }

                if (GUILayout.Button("Yeni Soru Blogu Ekle", GUILayout.Height(24f)))
                {
                    AddQuestionToLevel(level);
                }
            }
        }

        private void DrawObstacleSection(LevelContentEditorEntry level)
        {
            EditorGUILayout.LabelField("Obstacle Ayarlari", EditorStyles.miniBoldLabel);
            if (level.obstacles == null)
            {
                level.obstacles = new List<ObstacleDefinition>();
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Obstacle Ekle", GUILayout.Width(120f)))
                {
                    level.obstacles.Add(new ObstacleDefinition { obstacleType = "spike", angleOffset = 0f, severity = 1f });
                }

                using (new EditorGUI.DisabledScope(level.obstacles.Count == 0))
                {
                    if (GUILayout.Button("Son Obstacle Sil", GUILayout.Width(120f)))
                    {
                        level.obstacles.RemoveAt(level.obstacles.Count - 1);
                    }
                }
            }

            for (int i = 0; i < level.obstacles.Count; i++)
            {
                ObstacleDefinition obstacle = level.obstacles[i];
                using (new EditorGUILayout.VerticalScope("box"))
                {
                    obstacle.obstacleType = EditorGUILayout.TextField("Obstacle Type", obstacle.obstacleType);
                    obstacle.angleOffset = EditorGUILayout.FloatField("Angle Offset", obstacle.angleOffset);
                    obstacle.severity = EditorGUILayout.FloatField("Severity", obstacle.severity);
                }
            }
        }

        private void DrawQuestionEntry(LevelContentEditorEntry level, LevelQuestionEditorEntry question, int index)
        {
            string key = $"{level.levelId}:{question.questionId}:{index}";
            bool open = GetFoldoutState(_questionFoldouts, key);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool nextOpen = EditorGUILayout.Foldout(open, $"Soru {index + 1} | {question.questionId} | {question.infoCardId}", true);
                _questionFoldouts[key] = nextOpen;
                if (!nextOpen)
                {
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Bu Blogu Sil", GUILayout.Width(120f)))
                    {
                        level.questions.Remove(question);
                        GUIUtility.ExitGUI();
                    }
                }

                question.questionId = EditorGUILayout.TextField("Question Id", question.questionId);
                question.infoCardId = EditorGUILayout.TextField("InfoCard Id", question.infoCardId);
                question.packId = EditorGUILayout.TextField("Pack Id", question.packId);
                question.themeId = DrawStringPopup("Question Theme", question.themeId, _document.themeIds);
                question.difficultyTier = DrawStringPopup("Question Difficulty", question.difficultyTier, _document.questionDifficultyBandIds);
                question.imageResourcePath = EditorGUILayout.TextField("Info Card Image", question.imageResourcePath);
                EditorGUILayout.HelpBox(
                    $"TR Soru: {ClipText(question.questionText.tr, 120)}\n" +
                    $"TR Cevap: {ClipText(question.answerText.tr, 60)}\n" +
                    $"TR Bilgi Baslik: {ClipText(question.infoTitle.tr, 80)}\n" +
                    $"TR Bilgi Govde: {ClipText(question.infoBody.tr, 120)}",
                    MessageType.None);

                for (int i = 0; i < Lang.All.Length; i++)
                {
                    DrawLanguageBlock(key, Lang.All[i], LanguageLabels[i], question);
                }
            }
        }

        private void DrawLanguageBlock(string questionKey, string languageCode, string label, LevelQuestionEditorEntry question)
        {
            string foldoutKey = $"{questionKey}:{languageCode}";
            bool open = GetFoldoutState(_languageFoldouts, foldoutKey);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool nextOpen = EditorGUILayout.Foldout(open, label, true);
                _languageFoldouts[foldoutKey] = nextOpen;
                if (!nextOpen)
                {
                    return;
                }

                question.questionText.Set(languageCode, DrawTextArea("Soru", question.questionText.Get(languageCode), 56f));
                question.answerText.Set(languageCode, EditorGUILayout.TextField("Cevap", question.answerText.Get(languageCode)));
                question.infoTitle.Set(languageCode, EditorGUILayout.TextField("Bilgi Karti Baslik", question.infoTitle.Get(languageCode)));
                question.infoBody.Set(languageCode, DrawTextArea("Bilgi Karti Govde", question.infoBody.Get(languageCode), 82f));
            }
        }

        private void DrawShapeEntry(ShapeLayoutDefinition shape)
        {
            if (shape == null)
            {
                return;
            }

            bool open = GetFoldoutState(_shapeFoldouts, shape.shapeLayoutId);
            using (new EditorGUILayout.VerticalScope("box"))
            {
                bool nextOpen = EditorGUILayout.Foldout(open, $"{shape.displayName} | {shape.shapeLayoutId} | {shape.slotCount} kutu", true);
                _shapeFoldouts[shape.shapeLayoutId] = nextOpen;
                if (!nextOpen)
                {
                    return;
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Varsayilan Sekle Don", GUILayout.Width(160f)))
                    {
                        WordSpinAlphaContentEditorRepository.ResetShapeToDefault(shape);
                    }

                    using (new EditorGUI.DisabledScope(string.IsNullOrWhiteSpace(shape.editorReferenceImagePath)))
                    {
                        if (GUILayout.Button("Referans Gorselden Uret", GUILayout.Width(180f)))
                        {
                            WordSpinAlphaContentEditorRepository.RegenerateShapeFromReferenceImage(shape);
                        }
                    }

                    if (GUILayout.Button("Sekli Sil", GUILayout.Width(90f)))
                    {
                        DeleteShape(shape);
                        return;
                    }
                }

                shape.shapeLayoutId = EditorGUILayout.TextField("Shape Id", shape.shapeLayoutId);
                shape.displayName = EditorGUILayout.TextField("Gorunen Ad", shape.displayName);
                shape.shapeFamily = DrawStringPopup("Sekil Ailesi", shape.shapeFamily, ShapeFamilies);
                int previousSlotCount = Mathf.Max(1, shape.slotCount);
                int nextSlotCount = Mathf.Max(1, EditorGUILayout.IntField("Kutu Sayisi", shape.slotCount));
                if (nextSlotCount != previousSlotCount)
                {
                    shape.slotCount = nextSlotCount;
                    WordSpinAlphaContentEditorRepository.ApplySlotCountChange(shape, previousSlotCount);
                }

                shape.radiusX = EditorGUILayout.FloatField("Radius X", shape.radiusX);
                shape.radiusY = EditorGUILayout.FloatField("Radius Y", shape.radiusY);
                shape.rotationOffsetDegrees = EditorGUILayout.FloatField("Rotation Offset", shape.rotationOffsetDegrees);
                shape.plaqueWidth = EditorGUILayout.FloatField("Plaque Width", shape.plaqueWidth);
                shape.plaqueHeight = EditorGUILayout.FloatField("Plaque Height", shape.plaqueHeight);
                shape.perfectWidthScale = EditorGUILayout.FloatField("Perfect Width Scale", shape.perfectWidthScale);
                shape.perfectHeightScale = EditorGUILayout.FloatField("Perfect Height Scale", shape.perfectHeightScale);
                shape.nearMissPadding = EditorGUILayout.FloatField("Near Miss Padding", shape.nearMissPadding);
                shape.useTangentialRotation = EditorGUILayout.Toggle("Tangential Rotation", shape.useTangentialRotation);
                shape.gameplayAutoFit = EditorGUILayout.Toggle("Otomatik Gameplay Fit", shape.gameplayAutoFit);
                shape.visualPrefabResourcePath = EditorGUILayout.TextField("Visual Prefab Path", shape.visualPrefabResourcePath);

                EditorGUILayout.Space(4f);
                EditorGUILayout.LabelField("Plaque Gorsel Uyum", EditorStyles.miniBoldLabel);
                shape.adaptivePlaqueVisuals = EditorGUILayout.Toggle("Adaptif Gorsel Plaque", shape.adaptivePlaqueVisuals);
                shape.plaqueVisualPadding = EditorGUILayout.FloatField("Komsu Bosluk Payi", shape.plaqueVisualPadding);
                shape.plaqueVisualMinWidthScale = EditorGUILayout.FloatField("Min Genislik Carpan", shape.plaqueVisualMinWidthScale);
                shape.plaqueVisualMaxWidthScale = EditorGUILayout.FloatField("Maks Genislik Carpan", shape.plaqueVisualMaxWidthScale);
                shape.plaqueVisualMinHeightScale = EditorGUILayout.FloatField("Min Yukseklik Carpan", shape.plaqueVisualMinHeightScale);
                shape.plaqueVisualMaxHeightScale = EditorGUILayout.FloatField("Maks Yukseklik Carpan", shape.plaqueVisualMaxHeightScale);
                shape.plaqueVisualOutwardOffset = EditorGUILayout.FloatField("Disa Itme", shape.plaqueVisualOutwardOffset);
                shape.plaqueVisualContourFollow = EditorGUILayout.Slider("Konturu Takip", shape.plaqueVisualContourFollow, 0f, 1f);

                Texture2D currentReference = WordSpinAlphaContentEditorRepository.LoadReferenceTexture(shape);
                Texture2D nextReference = (Texture2D)EditorGUILayout.ObjectField("Referans Gorsel", currentReference, typeof(Texture2D), false);
                if (nextReference != currentReference)
                {
                    WordSpinAlphaContentEditorRepository.SetReferenceTexture(shape, nextReference);
                    if (nextReference != null)
                    {
                        WordSpinAlphaContentEditorRepository.RegenerateShapeFromReferenceImage(shape);
                    }
                }

                if (!string.IsNullOrWhiteSpace(shape.editorReferenceImagePath))
                {
                    EditorGUILayout.LabelField("Referans Yol", shape.editorReferenceImagePath, EditorStyles.miniLabel);
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    _independentManualPreview = EditorGUILayout.ToggleLeft("Bagimsiz Manuel Duzenleme", _independentManualPreview, GUILayout.Width(190f));
                    _showShapeGuideLines = EditorGUILayout.ToggleLeft("Klavuz Cizgileri Goster", _showShapeGuideLines, GUILayout.Width(170f));
                }

                EditorGUILayout.HelpBox("Turuncu nokta: plaque merkezini tasir. Mavi nokta: plaque acisini dondurur. 'Otomatik Gameplay Fit' kapaliyken manuel pozisyonlar aynen korunur.", MessageType.None);

                Rect previewRect = GUILayoutUtility.GetRect(position.width - 48f, 260f, GUILayout.ExpandWidth(true));
                DrawShapePreview(shape, previewRect);
            }
        }

        private void DrawShapePreview(ShapeLayoutDefinition shape, Rect rect)
        {
            EditorGUI.DrawRect(rect, new Color(0.10f, 0.11f, 0.13f, 1f));
            Texture2D referenceTexture = WordSpinAlphaContentEditorRepository.LoadReferenceTexture(shape);
            if (referenceTexture != null)
            {
                Rect textureRect = GetAspectFitRect(rect, referenceTexture.width, referenceTexture.height);
                Color previousGuiColor = GUI.color;
                GUI.color = new Color(1f, 1f, 1f, 0.18f);
                GUI.DrawTexture(textureRect, referenceTexture, ScaleMode.ScaleToFit, true);
                GUI.color = previousGuiColor;
            }

            ShapePointDefinition[] points = WordSpinAlphaContentEditorRepository.GeneratePreviewPoints(shape);
            if (points == null || points.Length == 0)
            {
                return;
            }

            float extent = 1.2f;
            for (int i = 0; i < points.Length; i++)
            {
                extent = Mathf.Max(extent, Mathf.Abs(points[i].x), Mathf.Abs(points[i].y));
            }

            Vector2 center = rect.center;
            float scale = Mathf.Min(rect.width, rect.height) / (extent * 2.6f);
            DrawShapePreviewPlaques(shape, rect, points, center, scale);
            if (_showShapeGuideLines)
            {
                Handles.BeginGUI();
                Color previousColor = Handles.color;
                Handles.color = new Color(0.95f, 0.70f, 0.36f, 1f);

                for (int i = 0; i < points.Length; i++)
                {
                    Vector2 a = ToPreviewPoint(points[i], center, scale);
                    Vector2 b = ToPreviewPoint(points[(i + 1) % points.Length], center, scale);
                    Handles.DrawLine(a, b);
                }

                Handles.color = previousColor;
                Handles.EndGUI();
            }

            Event currentEvent = Event.current;
            for (int i = 0; i < points.Length; i++)
            {
                Vector2 pointPosition = ToPreviewPoint(points[i], center, scale);
                Rect handleRect = new Rect(pointPosition - new Vector2(8f, 8f), new Vector2(16f, 16f));
                EditorGUI.DrawRect(handleRect, new Color(1f, 0.55f, 0.16f, 1f));

                ShapePlaqueVisualLayoutInfo visualLayout = ResolvePreviewPlaqueVisualLayout(shape, points, i);
                Vector2 worldPoint = new Vector2(points[i].x, points[i].y);
                Vector2 outward = worldPoint.sqrMagnitude > 0.0001f ? worldPoint.normalized : Vector2.up;
                Vector2 adjustedCenter = worldPoint + (outward * visualLayout.outwardOffset);
                float baseRotation = shape.useTangentialRotation
                    ? Mathf.Atan2(-outward.x, outward.y) * Mathf.Rad2Deg
                    : Mathf.Atan2(outward.y, outward.x) * Mathf.Rad2Deg - 90f;
                float finalRotation = baseRotation + visualLayout.localRotationDegrees;
                Vector2 rotationHandleOffset = RotatePreviewVector(new Vector2(0f, -(Mathf.Max(visualLayout.seatSize.x, visualLayout.seatSize.y) * scale * 0.60f)), finalRotation);
                Vector2 plaqueCenterPosition = ToPreviewPoint(new ShapePointDefinition { x = adjustedCenter.x, y = adjustedCenter.y }, center, scale);
                Rect rotationHandleRect = new Rect(plaqueCenterPosition + rotationHandleOffset - new Vector2(6f, 6f), new Vector2(12f, 12f));
                EditorGUI.DrawRect(rotationHandleRect, new Color(0.38f, 0.82f, 1f, 1f));

                if (currentEvent.type == EventType.MouseDown && handleRect.Contains(currentEvent.mousePosition))
                {
                    WordSpinAlphaContentEditorRepository.EnsureEditableCustomPoints(shape);
                    _dragShapeId = shape.shapeLayoutId;
                    _dragPointIndex = i;
                    _rotateShapeId = string.Empty;
                    _rotatePointIndex = -1;
                    currentEvent.Use();
                    Repaint();
                }
                else if (currentEvent.type == EventType.MouseDown && rotationHandleRect.Contains(currentEvent.mousePosition))
                {
                    WordSpinAlphaContentEditorRepository.EnsureEditablePlaqueAngles(shape);
                    _rotateShapeId = shape.shapeLayoutId;
                    _rotatePointIndex = i;
                    _dragShapeId = string.Empty;
                    _dragPointIndex = -1;
                    currentEvent.Use();
                    Repaint();
                }
            }

            if (_dragPointIndex >= 0 && _dragShapeId == shape.shapeLayoutId)
            {
                if (currentEvent.type == EventType.MouseDrag)
                {
                    ShapePointDefinition[] editablePoints = shape.customPoints ?? WordSpinAlphaContentEditorRepository.EnsureEditableCustomPoints(shape);
                    if (_dragPointIndex < 0 || _dragPointIndex >= editablePoints.Length)
                    {
                        _dragPointIndex = -1;
                        _dragShapeId = string.Empty;
                        currentEvent.Use();
                        return;
                    }

                    Vector2 worldPoint = FromPreviewPoint(currentEvent.mousePosition, center, scale);
                    editablePoints[_dragPointIndex].x = worldPoint.x;
                    editablePoints[_dragPointIndex].y = worldPoint.y;
                    currentEvent.Use();
                    Repaint();
                }
                else if (currentEvent.type == EventType.MouseUp)
                {
                    _dragPointIndex = -1;
                    _dragShapeId = string.Empty;
                    currentEvent.Use();
                }
            }

            if (_rotatePointIndex >= 0 && _rotateShapeId == shape.shapeLayoutId)
            {
                if (currentEvent.type == EventType.MouseDrag)
                {
                    float[] angleOffsets = shape.plaqueVisualAngleOffsets ?? WordSpinAlphaContentEditorRepository.EnsureEditablePlaqueAngles(shape);
                    if (_rotatePointIndex < 0 || _rotatePointIndex >= angleOffsets.Length || _rotatePointIndex >= points.Length)
                    {
                        _rotatePointIndex = -1;
                        _rotateShapeId = string.Empty;
                        currentEvent.Use();
                        return;
                    }

                    Vector2 pointPosition = ToPreviewPoint(points[_rotatePointIndex], center, scale);
                    Vector2 dragVector = currentEvent.mousePosition - pointPosition;
                    if (dragVector.sqrMagnitude > 1f)
                    {
                        Vector2 worldPoint = new Vector2(points[_rotatePointIndex].x, points[_rotatePointIndex].y);
                        Vector2 outward = worldPoint.sqrMagnitude > 0.0001f ? worldPoint.normalized : Vector2.up;
                        ShapePlaqueVisualLayoutInfo visualLayout = ResolvePreviewPlaqueVisualLayout(shape, points, _rotatePointIndex);
                        Vector2 adjustedCenter = worldPoint + (outward * visualLayout.outwardOffset);
                        float baseRotation = shape.useTangentialRotation
                            ? Mathf.Atan2(-outward.x, outward.y) * Mathf.Rad2Deg
                            : Mathf.Atan2(outward.y, outward.x) * Mathf.Rad2Deg - 90f;
                        float autoRotation = visualLayout.localRotationDegrees
                            - ShapeLayoutGeometry.ResolveManualPlaqueAngleOffset(shape, _rotatePointIndex, points.Length);
                        Vector2 plaqueCenterPosition = ToPreviewPoint(new ShapePointDefinition { x = adjustedCenter.x, y = adjustedCenter.y }, center, scale);
                        Vector2 rotateVector = currentEvent.mousePosition - plaqueCenterPosition;
                        float desiredFinalRotation = Mathf.Atan2(rotateVector.y, rotateVector.x) * Mathf.Rad2Deg - 90f;
                        angleOffsets[_rotatePointIndex] = Mathf.DeltaAngle(baseRotation + autoRotation, desiredFinalRotation);
                    }

                    currentEvent.Use();
                    Repaint();
                }
                else if (currentEvent.type == EventType.MouseUp)
                {
                    _rotatePointIndex = -1;
                    _rotateShapeId = string.Empty;
                    currentEvent.Use();
                }
            }
        }

        private void DrawShapePreviewPlaques(ShapeLayoutDefinition shape, Rect rect, ShapePointDefinition[] points, Vector2 center, float scale)
        {
            if (shape == null || points == null || points.Length == 0)
            {
                return;
            }

            Handles.BeginGUI();
            for (int i = 0; i < points.Length; i++)
            {
                ShapePlaqueVisualLayoutInfo visualLayout = ResolvePreviewPlaqueVisualLayout(shape, points, i);
                Vector2 point = new Vector2(points[i].x, points[i].y);
                Vector2 outward = point.sqrMagnitude > 0.0001f ? point.normalized : Vector2.up;
                Vector2 adjustedCenter = point + (outward * visualLayout.outwardOffset);
                float baseRotation = shape.useTangentialRotation
                    ? Mathf.Atan2(-outward.x, outward.y) * Mathf.Rad2Deg
                    : Mathf.Atan2(outward.y, outward.x) * Mathf.Rad2Deg - 90f;
                float finalRotation = baseRotation + visualLayout.localRotationDegrees;

                DrawPreviewPlaquePolygon(
                    ToPreviewPoint(new ShapePointDefinition { x = adjustedCenter.x, y = adjustedCenter.y }, center, scale),
                    visualLayout.seatSize * scale,
                    finalRotation,
                    new Color(0.16f, 0.11f, 0.08f, 0.55f),
                    new Color(0.28f, 0.18f, 0.12f, 0.85f));
                DrawPreviewPlaquePolygon(
                    ToPreviewPoint(new ShapePointDefinition { x = adjustedCenter.x, y = adjustedCenter.y }, center, scale),
                    visualLayout.plaqueSize * scale,
                    finalRotation,
                    new Color(0.68f, 0.43f, 0.23f, 0.55f),
                    new Color(0.95f, 0.70f, 0.36f, 0.92f));
            }

            Handles.EndGUI();
        }

        private ShapePlaqueVisualLayoutInfo ResolvePreviewPlaqueVisualLayout(ShapeLayoutDefinition shape, ShapePointDefinition[] points, int index)
        {
            if (!_independentManualPreview)
            {
                return ShapeLayoutGeometry.ResolvePlaqueVisualLayout(shape, points, index, points != null ? points.Length : 0);
            }

            Vector2 basePlaqueSize = new Vector2(
                Mathf.Max(0.14f, shape != null ? shape.plaqueWidth : 0.30f),
                Mathf.Max(0.10f, shape != null ? shape.plaqueHeight : 0.18f)) * 1.55f;
            return new ShapePlaqueVisualLayoutInfo
            {
                plaqueSize = basePlaqueSize,
                innerSize = basePlaqueSize * new Vector2(0.78f, 0.78f),
                runeSize = new Vector2(basePlaqueSize.x * 0.14f, basePlaqueSize.y * 0.48f),
                seatSize = basePlaqueSize + new Vector2(0.16f, 0.14f),
                glowSize = basePlaqueSize + new Vector2(0.10f, 0.10f),
                outwardOffset = 0f,
                localRotationDegrees = ShapeLayoutGeometry.ResolveManualPlaqueAngleOffset(shape, index, points != null ? points.Length : 0)
            };
        }

        private static void DrawPreviewPlaquePolygon(Vector2 center, Vector2 size, float rotationDegrees, Color fillColor, Color outlineColor)
        {
            Vector2 half = size * 0.5f;
            float radians = rotationDegrees * Mathf.Deg2Rad;
            Vector2 right = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));
            Vector2 up = new Vector2(-Mathf.Sin(radians), Mathf.Cos(radians));

            Vector3[] corners =
            {
                center + (-right * half.x) + (up * half.y),
                center + (right * half.x) + (up * half.y),
                center + (right * half.x) + (-up * half.y),
                center + (-right * half.x) + (-up * half.y)
            };

            Handles.DrawSolidRectangleWithOutline(corners, fillColor, outlineColor);
        }

        private void ReloadDocument()
        {
            _document = WordSpinAlphaContentEditorRepository.LoadDocument();
            _issues = WordSpinAlphaContentEditorRepository.Validate(_document);
            _levelFoldouts.Clear();
            _questionFoldouts.Clear();
            _languageFoldouts.Clear();
            _shapeFoldouts.Clear();
            if (_document.shapeLayouts.Count == 0)
            {
                _applyShapeLayoutId = string.Empty;
            }
            else if (string.IsNullOrWhiteSpace(_applyShapeLayoutId) ||
                     !_document.shapeLayouts.Any(shape => string.Equals(shape.shapeLayoutId, _applyShapeLayoutId, StringComparison.OrdinalIgnoreCase)))
            {
                _applyShapeLayoutId = _document.shapeLayouts[0].shapeLayoutId;
            }
        }

        private void SaveDocument()
        {
            _issues = WordSpinAlphaContentEditorRepository.Validate(_document);
            bool hasBlockingError = _issues.Any(issue => string.Equals(issue.severity, "Hata", StringComparison.OrdinalIgnoreCase));
            if (hasBlockingError)
            {
                _selectedTab = EditorTab.Dogrulama;
                EditorUtility.DisplayDialog("WordSpin Alpha", "Kaydetmeden once dogrulama hatalarini duzelt.", "OK");
                return;
            }

            WordSpinAlphaContentEditorRepository.SaveDocument(_document);
            ApplyRuntimeChangesIfNeeded();
            if (EditorApplication.isPlaying)
            {
                ShowNotification(new GUIContent("Kaydedildi ve canli uygulandi."));
            }
            else
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", "Icerik ve shape kataloglari kaydedildi.", "OK");
            }
        }

        private void AddNewLevel()
        {
            int nextLevelId = WordSpinAlphaContentEditorRepository.GenerateNextLevelId(_document);
            LevelContentEditorEntry level = new LevelContentEditorEntry
            {
                levelId = nextLevelId,
                campaignId = _document.campaignIds.FirstOrDefault() ?? "alpha_main",
                themeId = _document.themeIds.FirstOrDefault() ?? "base_default",
                difficultyProfileId = _document.difficultyProfileIds.FirstOrDefault() ?? "hook",
                difficultyTierId = _document.difficultyTierIds.FirstOrDefault() ?? "intro_perfect",
                rhythmProfileId = _document.rhythmProfileIds.FirstOrDefault() ?? "hook",
                shapeLayoutId = _document.shapeLayouts.FirstOrDefault() != null ? _document.shapeLayouts[0].shapeLayoutId : "circle_classic",
                rotationSpeed = 40f + ((nextLevelId - 1) * 2f),
                clockwise = nextLevelId % 2 == 1
            };

            ShapeLayoutDefinition shape = FindShape(level.shapeLayoutId);
            level.shape = shape != null ? shape.shapeFamily : "circle";
            AddQuestionToLevel(level);
            _document.levels.Add(level);
            _document.levels = _document.levels.OrderBy(entry => entry.levelId).ToList();
            _levelFoldouts[level.levelId] = true;
        }

        private void DuplicateLevel(LevelContentEditorEntry source)
        {
            int nextLevelId = WordSpinAlphaContentEditorRepository.GenerateNextLevelId(_document);
            Func<string> nextQuestionId = CreateIdAllocator("q", level => level.questions, question => question.questionId);
            Func<string> nextInfoCardId = CreateIdAllocator("c", level => level.questions, question => question.infoCardId);
            LevelContentEditorEntry clone = source.CloneWithNewIds(
                nextLevelId,
                nextQuestionId,
                nextInfoCardId);

            _document.levels.Add(clone);
            _document.levels = _document.levels.OrderBy(level => level.levelId).ToList();
            _levelFoldouts[clone.levelId] = true;
        }

        private void DeleteLevel(LevelContentEditorEntry level)
        {
            if (!EditorUtility.DisplayDialog("WordSpin Alpha", $"Seviye {level.levelId} silinsin mi?", "Sil", "Vazgec"))
            {
                return;
            }

            _document.levels.Remove(level);
        }

        private void AddQuestionToLevel(LevelContentEditorEntry level)
        {
            string nextQuestionId = WordSpinAlphaContentEditorRepository.GenerateNextQuestionId(_document);
            string nextInfoCardId = WordSpinAlphaContentEditorRepository.GenerateNextInfoCardId(_document);
            LevelQuestionEditorEntry question = new LevelQuestionEditorEntry
            {
                questionId = nextQuestionId,
                infoCardId = nextInfoCardId,
                themeId = level.themeId,
                difficultyTier = _document.questionDifficultyBandIds.FirstOrDefault() ?? level.difficultyProfileId,
                packId = "alpha25_pack",
                imageResourcePath = $"Cards/Base/{nextInfoCardId}"
            };
            level.questions.Add(question);
            int index = level.questions.Count - 1;
            string questionKey = $"{level.levelId}:{question.questionId}:{index}";
            _questionFoldouts[questionKey] = true;
            _languageFoldouts[$"{questionKey}:{Lang.TR}"] = true;
        }

        private void AddNewShape()
        {
            string shapeId = string.IsNullOrWhiteSpace(_newShapeId) ? $"shape_{_document.shapeLayouts.Count + 1:000}" : _newShapeId.Trim();
            if (_document.shapeLayouts.Any(shape => string.Equals(shape.shapeLayoutId, shapeId, StringComparison.OrdinalIgnoreCase)))
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", "Bu shape id zaten kullanimda.", "OK");
                return;
            }

            ShapeLayoutDefinition shape = WordSpinAlphaContentEditorRepository.CreateNewShapeLayout(
                shapeId,
                string.IsNullOrWhiteSpace(_newShapeName) ? shapeId : _newShapeName.Trim(),
                ShapeFamilies[Mathf.Clamp(_newShapeFamilyIndex, 0, ShapeFamilies.Length - 1)],
                _newShapeSlotCount);

            _document.shapeLayouts.Add(shape);
            _document.shapeLayouts = _document.shapeLayouts
                .OrderBy(layout => layout.displayName ?? layout.shapeLayoutId)
                .ToList();
            _shapeFoldouts[shape.shapeLayoutId] = true;
            _applyShapeLayoutId = shape.shapeLayoutId;
            _newShapeId = $"shape_{_document.shapeLayouts.Count + 1:000}";
            _newShapeName = "Yeni Sekil";
        }

        private void DeleteShape(ShapeLayoutDefinition shape)
        {
            if (!EditorUtility.DisplayDialog("WordSpin Alpha", $"{shape.displayName} silinsin mi?", "Sil", "Vazgec"))
            {
                return;
            }

            _document.shapeLayouts.Remove(shape);
            _shapeFoldouts.Remove(shape.shapeLayoutId);
            if (string.Equals(_applyShapeLayoutId, shape.shapeLayoutId, StringComparison.OrdinalIgnoreCase))
            {
                _applyShapeLayoutId = _document.shapeLayouts.FirstOrDefault() != null
                    ? _document.shapeLayouts[0].shapeLayoutId
                    : string.Empty;
            }
        }

        private void ApplyShapeToLevels()
        {
            int start = Mathf.Min(_applyShapeStartLevel, _applyShapeEndLevel);
            int end = Mathf.Max(_applyShapeStartLevel, _applyShapeEndLevel);
            ShapeLayoutDefinition shape = FindShape(_applyShapeLayoutId);
            if (shape == null)
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", "Uygulanacak shape bulunamadi.", "OK");
                return;
            }

            for (int i = 0; i < _document.levels.Count; i++)
            {
                LevelContentEditorEntry level = _document.levels[i];
                if (level.levelId < start || level.levelId > end)
                {
                    continue;
                }

                level.shapeLayoutId = shape.shapeLayoutId;
                level.shape = shape.shapeFamily;
            }
        }

        private ShapeLayoutDefinition FindShape(string shapeLayoutId)
        {
            return _document.shapeLayouts.FirstOrDefault(shape => string.Equals(shape.shapeLayoutId, shapeLayoutId, StringComparison.OrdinalIgnoreCase));
        }

        private Func<string> CreateIdAllocator(string prefix, Func<LevelContentEditorEntry, IEnumerable<LevelQuestionEditorEntry>> selector, Func<LevelQuestionEditorEntry, string> valueSelector)
        {
            HashSet<string> usedIds = new HashSet<string>(
                (_document.levels ?? new List<LevelContentEditorEntry>())
                    .SelectMany(level => selector(level) ?? Enumerable.Empty<LevelQuestionEditorEntry>())
                    .Select(valueSelector)
                    .Where(id => !string.IsNullOrWhiteSpace(id)),
                StringComparer.OrdinalIgnoreCase);

            int max = 0;
            foreach (string usedId in usedIds)
            {
                if (!usedId.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string numeric = usedId.Substring(prefix.Length);
                if (int.TryParse(numeric, out int parsed))
                {
                    max = Mathf.Max(max, parsed);
                }
            }

            return () =>
            {
                max++;
                string next = $"{prefix}{max:000}";
                while (usedIds.Contains(next))
                {
                    max++;
                    next = $"{prefix}{max:000}";
                }

                usedIds.Add(next);
                return next;
            };
        }

        private string[] GetShapeIdOptions()
        {
            return _document.shapeLayouts.Select(shape => shape.shapeLayoutId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().OrderBy(id => id).ToArray();
        }

        private static bool GetFoldoutState<TKey>(Dictionary<TKey, bool> dictionary, TKey key)
        {
            return dictionary.TryGetValue(key, out bool value) && value;
        }

        private static string DrawTextArea(string label, string value, float minHeight)
        {
            EditorGUILayout.LabelField(label);
            return EditorGUILayout.TextArea(value ?? string.Empty, GUILayout.MinHeight(minHeight));
        }

        private static Vector2 ToPreviewPoint(ShapePointDefinition point, Vector2 center, float scale)
        {
            return center + new Vector2(point.x, -point.y) * scale;
        }

        private static Vector2 FromPreviewPoint(Vector2 previewPoint, Vector2 center, float scale)
        {
            Vector2 relative = (previewPoint - center) / Mathf.Max(0.0001f, scale);
            return new Vector2(relative.x, -relative.y);
        }

        private static Rect GetAspectFitRect(Rect bounds, float sourceWidth, float sourceHeight)
        {
            if (sourceWidth <= 0f || sourceHeight <= 0f)
            {
                return bounds;
            }

            float sourceAspect = sourceWidth / sourceHeight;
            float boundsAspect = bounds.width / Mathf.Max(1f, bounds.height);
            if (sourceAspect > boundsAspect)
            {
                float height = bounds.width / sourceAspect;
                return new Rect(bounds.x, bounds.y + ((bounds.height - height) * 0.5f), bounds.width, height);
            }

            float width = bounds.height * sourceAspect;
            return new Rect(bounds.x + ((bounds.width - width) * 0.5f), bounds.y, width, bounds.height);
        }

        private static Vector2 RotatePreviewVector(Vector2 vector, float degrees)
        {
            float radians = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(radians);
            float sin = Mathf.Sin(radians);
            return new Vector2((vector.x * cos) - (vector.y * sin), (vector.x * sin) + (vector.y * cos));
        }

        private static int CountFilledLanguages(LevelContentEditorEntry level)
        {
            HashSet<string> filled = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (LevelQuestionEditorEntry question in level.questions ?? new List<LevelQuestionEditorEntry>())
            {
                foreach (string languageCode in Lang.All)
                {
                    if (!string.IsNullOrWhiteSpace(question.questionText.Get(languageCode)))
                    {
                        filled.Add(languageCode);
                    }
                }
            }

            return filled.Count;
        }

        private static string ClipText(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "-";
            }

            string normalized = value.Replace('\n', ' ').Replace('\r', ' ').Trim();
            if (normalized.Length <= maxLength)
            {
                return normalized;
            }

            return normalized.Substring(0, Mathf.Max(0, maxLength - 3)) + "...";
        }

        private static void ApplyRuntimeChangesIfNeeded()
        {
            if (!EditorApplication.isPlaying)
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ReloadCurrentSessionForEditorContent();
                return;
            }

            MainMenuPresenter mainMenuPresenter = FindObjectOfType<MainMenuPresenter>();
            if (mainMenuPresenter != null)
            {
                mainMenuPresenter.RefreshEditorContent();
            }
        }

        private string DrawStringPopup(string label, string currentValue, IEnumerable<string> options)
        {
            List<string> values = (options ?? Enumerable.Empty<string>())
                .Where(option => !string.IsNullOrWhiteSpace(option))
                .Distinct()
                .OrderBy(option => option)
                .ToList();

            if (!string.IsNullOrWhiteSpace(currentValue) && !values.Contains(currentValue))
            {
                values.Insert(0, currentValue);
            }

            if (values.Count == 0)
            {
                return EditorGUILayout.TextField(label, currentValue);
            }

            int index = Mathf.Max(0, values.IndexOf(currentValue));
            int nextIndex = EditorGUILayout.Popup(label, index, values.ToArray());
            return values[Mathf.Clamp(nextIndex, 0, values.Count - 1)];
        }
    }
}
