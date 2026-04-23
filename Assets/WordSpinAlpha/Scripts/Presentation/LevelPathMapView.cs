using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordSpinAlpha.Content;

namespace WordSpinAlpha.Presentation
{
    public class LevelPathMapView : MonoBehaviour
    {
        private static Sprite s_defaultSprite;
        private static Sprite s_roundSprite;

        private enum MapVisualStyle
        {
            Road,
            NeonHub
        }

        [Header("Scene Refs")]
        [SerializeField] private ScrollRect scrollRect;
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;
        [SerializeField] private Button levelButtonTemplate;
        [SerializeField] private MapVisualStyle visualStyle = MapVisualStyle.Road;

        [Header("Path Layout")]
        [SerializeField] private float topPadding = 280f;
        [SerializeField] private float bottomPadding = 340f;
        [SerializeField] private float verticalSpacing = 232f;
        [SerializeField] private float pathAmplitude = 176f;
        [SerializeField] private float pathFrequency = 0.76f;
        [SerializeField, Range(0.1f, 0.9f)] private float focusViewportNormalized = 0.30f;
        [SerializeField] private Vector2 nodeSize = new Vector2(308f, 152f);
        [SerializeField] private float minNodeScale = 0.62f;
        [SerializeField] private float maxNodeScale = 1.14f;
        [SerializeField] private float roadOuterThickness = 122f;
        [SerializeField] private float roadInnerThickness = 86f;
        [SerializeField] private float roadLaneThickness = 10f;

        [Header("Palette")]
        [SerializeField] private Color viewportBaseColor = new Color(0.80f, 0.92f, 0.98f, 0.98f);
        [SerializeField] private Color topGlowColor = new Color(1f, 1f, 1f, 0.28f);
        [SerializeField] private Color seaColor = new Color(0.38f, 0.72f, 0.92f, 0.18f);
        [SerializeField] private Color cliffLeftColor = new Color(1f, 0.93f, 0.82f, 0.96f);
        [SerializeField] private Color cliffRightColor = new Color(1f, 0.83f, 0.63f, 0.94f);
        [SerializeField] private Color roadShadowColor = new Color(0f, 0f, 0f, 0.14f);
        [SerializeField] private Color roadOuterColor = new Color(0.96f, 0.90f, 0.82f, 0.98f);
        [SerializeField] private Color roadInnerColor = new Color(0.88f, 0.72f, 0.57f, 0.98f);
        [SerializeField] private Color roadLaneColor = new Color(1f, 0.96f, 0.90f, 0.52f);
        [SerializeField] private Color completedNodeColor = new Color(0.33f, 0.66f, 0.42f, 1f);
        [SerializeField] private Color activeNodeColor = new Color(0.93f, 0.39f, 0.32f, 1f);
        [SerializeField] private Color futureNodeColor = new Color(0.30f, 0.51f, 0.71f, 1f);
        [SerializeField] private Color activeGlowColor = new Color(1f, 0.44f, 0.30f, 0.30f);
        [SerializeField] private Color completedGlowColor = new Color(0.36f, 0.84f, 0.48f, 0.22f);
        [SerializeField] private Color futureGlowColor = new Color(0.42f, 0.64f, 0.92f, 0.18f);
        [SerializeField] private Color accentColor = new Color(1f, 0.95f, 0.84f, 1f);
        [SerializeField] private Color nodeNumberColor = Color.white;
        [SerializeField] private Color nodeCaptionColor = new Color(1f, 1f, 1f, 0.94f);

        private readonly List<NodeRefs> _nodes = new List<NodeRefs>();
        private readonly List<SegmentRefs> _segments = new List<SegmentRefs>();
        private readonly List<RectTransform> _decorations = new List<RectTransform>();

        private RectTransform _decorLayer;
        private RectTransform _roadLayer;
        private RectTransform _nodeLayer;
        private RectTransform _viewportBackdrop;

        private LevelDefinition[] _levels = Array.Empty<LevelDefinition>();
        private Action<int> _onLevelSelected;
        private string _levelCaption = "Level";
        private int _focusLevelId = 1;
        private bool _pendingCenter;
        private bool _structureReady;

        private void LateUpdate()
        {
            if (!ResolveSceneRefs() || !isActiveAndEnabled)
            {
                return;
            }

            if (_pendingCenter && gameObject.activeInHierarchy)
            {
                Canvas.ForceUpdateCanvases();
                CenterOnLevel(_focusLevelId, true);
                _pendingCenter = false;
            }

            if (_nodes.Count > 0)
            {
                RefreshViewportDrivenVisuals();
            }
        }

        public void BindScene(ScrollRect targetScrollRect, RectTransform targetContent, Button template)
        {
            scrollRect = targetScrollRect;
            content = targetContent;
            levelButtonTemplate = template;
            viewport = scrollRect != null ? scrollRect.viewport : null;
            PrepareStructure();
        }

        public void ApplyNeonHubTheme()
        {
            visualStyle = MapVisualStyle.NeonHub;
            topPadding = 200f;
            bottomPadding = 300f;
            verticalSpacing = 244f;
            pathAmplitude = 102f;
            pathFrequency = 0.88f;
            focusViewportNormalized = 0.38f;
            nodeSize = new Vector2(118f, 118f);
            minNodeScale = 0.76f;
            maxNodeScale = 1.18f;
            roadOuterThickness = 18f;
            roadInnerThickness = 10f;
            roadLaneThickness = 5f;

            viewportBaseColor = new Color(0.05f, 0.17f, 0.23f, 0.98f);
            topGlowColor = new Color(0.26f, 0.48f, 0.90f, 0.16f);
            seaColor = new Color(0.24f, 0.78f, 1f, 0.07f);
            cliffLeftColor = new Color(0.18f, 0.48f, 0.62f, 0.10f);
            cliffRightColor = new Color(0.14f, 0.34f, 0.48f, 0.10f);
            roadShadowColor = new Color(0.26f, 0.12f, 0.48f, 0.18f);
            roadOuterColor = new Color(0.28f, 0.18f, 0.58f, 0.30f);
            roadInnerColor = new Color(0.50f, 0.34f, 0.94f, 0.52f);
            roadLaneColor = new Color(0.92f, 0.50f, 0.84f, 0.42f);
            completedNodeColor = new Color(0.39f, 0.25f, 0.84f, 1f);
            activeNodeColor = new Color(1f, 0.50f, 0.09f, 1f);
            futureNodeColor = new Color(0.12f, 0.37f, 0.50f, 1f);
            activeGlowColor = new Color(1f, 0.62f, 0.16f, 0.44f);
            completedGlowColor = new Color(0.45f, 0.30f, 0.98f, 0.24f);
            futureGlowColor = new Color(0.18f, 0.72f, 0.92f, 0.18f);
            accentColor = new Color(0.76f, 0.70f, 1f, 1f);
            nodeNumberColor = Color.white;
            nodeCaptionColor = new Color(0.80f, 0.90f, 0.96f, 0.92f);
        }

        public void Build(LevelDefinition[] levels, int focusLevelId, string levelCaption, Action<int> onLevelSelected)
        {
            _levels = levels ?? Array.Empty<LevelDefinition>();
            _focusLevelId = Mathf.Max(1, focusLevelId);
            _levelCaption = string.IsNullOrWhiteSpace(levelCaption) ? "Level" : levelCaption;
            _onLevelSelected = onLevelSelected;

            if (_levels.Length > 0)
            {
                int minLevelId = _levels[0] != null ? _levels[0].levelId : 1;
                int maxLevelId = _levels[_levels.Length - 1] != null ? _levels[_levels.Length - 1].levelId : minLevelId;
                _focusLevelId = Mathf.Clamp(_focusLevelId, Mathf.Max(1, minLevelId), Mathf.Max(minLevelId, maxLevelId));
            }

            if (!PrepareStructure())
            {
                return;
            }

            EnsureNodeCount(_levels.Length);
            EnsureSegmentCount(Mathf.Max(0, _levels.Length - 1));
            RebuildDecorations();
            LayoutMap();
            RefreshViewportDrivenVisuals();
            _pendingCenter = true;
        }

        public void CenterOnLevel(int levelId, bool immediate)
        {
            if (!ResolveSceneRefs() || scrollRect == null || viewport == null || content == null || _levels.Length == 0)
            {
                return;
            }

            int index = Array.FindIndex(_levels, level => level != null && level.levelId == levelId);
            if (index < 0)
            {
                index = Mathf.Clamp(_levels.Length - 1, 0, _levels.Length - 1);
            }

            float viewportHeight = Mathf.Max(1f, viewport.rect.height);
            float contentHeight = Mathf.Max(viewportHeight, content.sizeDelta.y);
            float distanceFromTop = GetDistanceFromTop(index);
            float desiredFocusFromTop = viewportHeight * (1f - focusViewportNormalized);
            float desiredScroll = Mathf.Clamp(distanceFromTop - desiredFocusFromTop, 0f, Mathf.Max(0f, contentHeight - viewportHeight));

            Vector2 anchored = content.anchoredPosition;
            anchored.y = desiredScroll;
            content.anchoredPosition = anchored;

            if (!immediate)
            {
                Canvas.ForceUpdateCanvases();
            }
        }

        private bool PrepareStructure()
        {
            if (!ResolveSceneRefs())
            {
                return false;
            }

            if (_structureReady)
            {
                return true;
            }

            if (content.TryGetComponent(out GridLayoutGroup grid))
            {
                Destroy(grid);
            }

            if (content.TryGetComponent(out ContentSizeFitter fitter))
            {
                Destroy(fitter);
            }

            scrollRect.horizontal = false;
            scrollRect.vertical = true;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;
            scrollRect.scrollSensitivity = 34f;
            scrollRect.inertia = true;
            scrollRect.decelerationRate = 0.12f;

            RectTransform scrollRootRect = scrollRect.GetComponent<RectTransform>();
            if (scrollRootRect != null)
            {
                scrollRootRect.sizeDelta = new Vector2(900f, 1020f);
            }

            if (viewport != null)
            {
                viewport.sizeDelta = new Vector2(876f, 996f);
            }

            RectTransform contentRect = content;
            contentRect.anchorMin = new Vector2(0.5f, 1f);
            contentRect.anchorMax = new Vector2(0.5f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.localScale = Vector3.one;

            EnsureViewportBackdrop();
            EnsureLayers();
            StyleTemplate();

            _structureReady = true;
            return true;
        }

        private bool ResolveSceneRefs()
        {
            if (scrollRect == null)
            {
                scrollRect = GetComponent<ScrollRect>();
            }

            if (scrollRect == null && content != null)
            {
                scrollRect = content.GetComponentInParent<ScrollRect>(true);
            }

            if (viewport == null && scrollRect != null)
            {
                viewport = scrollRect.viewport;
            }

            if (content == null && scrollRect != null)
            {
                content = scrollRect.content;
            }

            return scrollRect != null && viewport != null && content != null && levelButtonTemplate != null;
        }

        private void EnsureViewportBackdrop()
        {
            Image rootImage = scrollRect.GetComponent<Image>();
            if (rootImage != null)
            {
                rootImage.color = viewportBaseColor;
                rootImage.type = Image.Type.Sliced;
            }

            Transform existing = viewport.Find("MapBackdrop");
            if (existing == null)
            {
                GameObject backdrop = new GameObject("MapBackdrop", typeof(RectTransform));
                _viewportBackdrop = backdrop.GetComponent<RectTransform>();
                _viewportBackdrop.SetParent(viewport, false);
                _viewportBackdrop.anchorMin = Vector2.zero;
                _viewportBackdrop.anchorMax = Vector2.one;
                _viewportBackdrop.offsetMin = Vector2.zero;
                _viewportBackdrop.offsetMax = Vector2.zero;

                if (visualStyle == MapVisualStyle.NeonHub)
                {
                    CreateDecor("NebulaTop", _viewportBackdrop, new Vector2(0.5f, 1f), new Vector2(1280f, 940f), new Vector2(0f, -160f), GetRoundSprite(), topGlowColor);
                    CreateDecor("NebulaLeft", _viewportBackdrop, new Vector2(0f, 0.42f), new Vector2(340f, 880f), new Vector2(-60f, 0f), GetRoundSprite(), cliffLeftColor);
                    CreateDecor("NebulaRight", _viewportBackdrop, new Vector2(1f, 0.62f), new Vector2(280f, 760f), new Vector2(54f, 0f), GetRoundSprite(), cliffRightColor);
                    CreateDecor("AmbientPulseA", _viewportBackdrop, new Vector2(0.18f, 0.26f), new Vector2(220f, 220f), Vector2.zero, GetRoundSprite(), seaColor);
                    CreateDecor("AmbientPulseB", _viewportBackdrop, new Vector2(0.86f, 0.20f), new Vector2(180f, 180f), Vector2.zero, GetRoundSprite(), new Color(1f, 0.58f, 0.22f, 0.12f));
                    CreateDecor("BottomVignette", _viewportBackdrop, new Vector2(0.5f, 0f), new Vector2(1320f, 360f), new Vector2(0f, 88f), GetRoundSprite(), new Color(0f, 0f, 0f, 0.28f));
                }
                else
                {
                    CreateDecor("TopGlow", _viewportBackdrop, new Vector2(0.5f, 1f), new Vector2(1260f, 860f), new Vector2(0f, -120f), GetRoundSprite(), topGlowColor);
                    CreateDecor("SeaTint", _viewportBackdrop, new Vector2(0.5f, 0.15f), new Vector2(1080f, 360f), new Vector2(0f, 0f), GetRoundSprite(), seaColor);
                    CreateDecor("CliffLeft", _viewportBackdrop, new Vector2(0f, 0.22f), new Vector2(340f, 760f), new Vector2(-70f, 0f), GetRoundSprite(), cliffLeftColor);
                    CreateDecor("CliffRight", _viewportBackdrop, new Vector2(1f, 0.50f), new Vector2(280f, 980f), new Vector2(86f, 0f), GetRoundSprite(), cliffRightColor);
                    CreateDecor("CliffLeftInset", _viewportBackdrop, new Vector2(0f, 0.58f), new Vector2(210f, 400f), new Vector2(40f, 0f), GetRoundSprite(), new Color(1f, 1f, 1f, 0.28f));
                    CreateDecor("CliffRightInset", _viewportBackdrop, new Vector2(1f, 0.18f), new Vector2(180f, 320f), new Vector2(-26f, 0f), GetRoundSprite(), new Color(1f, 0.95f, 0.88f, 0.36f));
                    CreateDecor("BottomMist", _viewportBackdrop, new Vector2(0.5f, 0f), new Vector2(1120f, 260f), new Vector2(0f, 90f), GetRoundSprite(), new Color(1f, 1f, 1f, 0.14f));
                }
            }
            else
            {
                _viewportBackdrop = existing as RectTransform;
            }

            if (_viewportBackdrop != null)
            {
                _viewportBackdrop.SetSiblingIndex(0);
            }

            content.SetAsLastSibling();
        }

        private void EnsureLayers()
        {
            _decorLayer = EnsureLayer("MapDecorLayer", 0);
            _roadLayer = EnsureLayer("MapRoadLayer", 1);
            _nodeLayer = EnsureLayer("MapNodeLayer", 2);

            if (levelButtonTemplate.transform.parent != _nodeLayer)
            {
                levelButtonTemplate.transform.SetParent(_nodeLayer, false);
            }
        }

        private RectTransform EnsureLayer(string name, int siblingIndex)
        {
            Transform existing = content.Find(name);
            RectTransform layer;
            if (existing == null)
            {
                GameObject go = new GameObject(name, typeof(RectTransform));
                layer = go.GetComponent<RectTransform>();
                layer.SetParent(content, false);
                layer.anchorMin = new Vector2(0.5f, 1f);
                layer.anchorMax = new Vector2(0.5f, 1f);
                layer.pivot = new Vector2(0.5f, 1f);
                layer.anchoredPosition = Vector2.zero;
                layer.sizeDelta = Vector2.zero;
                layer.localScale = Vector3.one;
            }
            else
            {
                layer = (RectTransform)existing;
            }

            layer.SetSiblingIndex(siblingIndex);
            return layer;
        }

        private void StyleTemplate()
        {
            RectTransform templateRect = levelButtonTemplate.GetComponent<RectTransform>();
            templateRect.anchorMin = new Vector2(0.5f, 0.5f);
            templateRect.anchorMax = new Vector2(0.5f, 0.5f);
            templateRect.pivot = new Vector2(0.5f, 0.5f);
            templateRect.sizeDelta = nodeSize;
            templateRect.anchoredPosition = Vector2.zero;

            Image plate = levelButtonTemplate.GetComponent<Image>();
            if (plate != null)
            {
                plate.sprite = visualStyle == MapVisualStyle.NeonHub ? GetRoundSprite() : GetDefaultSprite();
                plate.type = visualStyle == MapVisualStyle.NeonHub ? Image.Type.Simple : Image.Type.Sliced;
                plate.color = futureNodeColor;
            }

            TextMeshProUGUI label = levelButtonTemplate.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                RectTransform labelRect = label.rectTransform;
                labelRect.anchorMin = visualStyle == MapVisualStyle.NeonHub ? new Vector2(0.5f, 0.52f) : new Vector2(0.5f, 0.54f);
                labelRect.anchorMax = visualStyle == MapVisualStyle.NeonHub ? new Vector2(0.5f, 0.52f) : new Vector2(0.5f, 0.54f);
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.sizeDelta = visualStyle == MapVisualStyle.NeonHub ? new Vector2(nodeSize.x - 30f, 80f) : new Vector2(nodeSize.x - 50f, 62f);
                labelRect.anchoredPosition = visualStyle == MapVisualStyle.NeonHub ? Vector2.zero : new Vector2(0f, -4f);
                label.fontSize = visualStyle == MapVisualStyle.NeonHub ? 62f : 54f;
                label.fontStyle = FontStyles.Bold;
                label.alignment = TextAlignmentOptions.Center;
                label.color = nodeNumberColor;
            }

            levelButtonTemplate.gameObject.SetActive(false);
        }

        private void EnsureNodeCount(int count)
        {
            while (_nodes.Count < count)
            {
                _nodes.Add(CreateNode(_nodes.Count));
            }

            for (int i = 0; i < _nodes.Count; i++)
            {
                _nodes[i].root.gameObject.SetActive(i < count);
            }
        }

        private NodeRefs CreateNode(int index)
        {
            GameObject root = new GameObject($"MapNode_{index + 1}", typeof(RectTransform));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(_nodeLayer, false);
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = visualStyle == MapVisualStyle.NeonHub
                ? new Vector2(nodeSize.x + 86f, nodeSize.y + 96f)
                : new Vector2(nodeSize.x + 120f, nodeSize.y + 80f);

            Image shadow = CreateDecor(
                "Shadow",
                rootRect,
                new Vector2(0.5f, 0.5f),
                visualStyle == MapVisualStyle.NeonHub ? new Vector2(nodeSize.x + 18f, nodeSize.y + 18f) : new Vector2(nodeSize.x + 26f, nodeSize.y + 26f),
                visualStyle == MapVisualStyle.NeonHub ? new Vector2(0f, -10f) : new Vector2(0f, -20f),
                GetRoundSprite(),
                new Color(0f, 0f, 0f, visualStyle == MapVisualStyle.NeonHub ? 0.32f : 0.20f));
            Image glow = CreateDecor(
                "Glow",
                rootRect,
                new Vector2(0.5f, 0.5f),
                visualStyle == MapVisualStyle.NeonHub ? new Vector2(nodeSize.x + 86f, nodeSize.y + 86f) : new Vector2(nodeSize.x + 120f, nodeSize.y + 80f),
                new Vector2(0f, -6f),
                GetRoundSprite(),
                futureGlowColor);

            Button button = Instantiate(levelButtonTemplate, rootRect);
            button.gameObject.SetActive(true);
            button.name = "Button";
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = nodeSize;
            buttonRect.anchoredPosition = Vector2.zero;
            Image plate = button.GetComponent<Image>();
            plate.sprite = visualStyle == MapVisualStyle.NeonHub ? GetRoundSprite() : GetDefaultSprite();
            plate.type = visualStyle == MapVisualStyle.NeonHub ? Image.Type.Simple : Image.Type.Sliced;
            Shadow uiShadow = button.GetComponent<Shadow>();
            if (uiShadow == null)
            {
                uiShadow = button.gameObject.AddComponent<Shadow>();
            }
            uiShadow.effectColor = visualStyle == MapVisualStyle.NeonHub
                ? new Color(0f, 0f, 0f, 0.34f)
                : new Color(0.18f, 0.07f, 0.04f, 0.28f);
            uiShadow.effectDistance = visualStyle == MapVisualStyle.NeonHub ? new Vector2(0f, -5f) : new Vector2(0f, -8f);

            Outline outline = button.GetComponent<Outline>();
            if (outline == null)
            {
                outline = button.gameObject.AddComponent<Outline>();
            }
            outline.effectColor = visualStyle == MapVisualStyle.NeonHub
                ? new Color(1f, 1f, 1f, 0.16f)
                : new Color(1f, 1f, 1f, 0.14f);
            outline.effectDistance = visualStyle == MapVisualStyle.NeonHub ? new Vector2(2f, -2f) : new Vector2(2f, -2f);

            TextMeshProUGUI numberLabel = button.GetComponentInChildren<TextMeshProUGUI>(true);
            TextMeshProUGUI captionLabel = EnsureText(
                buttonRect,
                "Caption",
                visualStyle == MapVisualStyle.NeonHub ? new Vector2(0.5f, 0.84f) : new Vector2(0.5f, 0.78f),
                new Vector2(nodeSize.x - 48f, 34f),
                visualStyle == MapVisualStyle.NeonHub ? 18f : 23f,
                FontStyles.Bold,
                nodeCaptionColor);
            TextMeshProUGUI statusLabel = EnsureText(
                buttonRect,
                "Status",
                visualStyle == MapVisualStyle.NeonHub ? new Vector2(0.5f, 0.14f) : new Vector2(0.5f, 0.18f),
                new Vector2(nodeSize.x - 60f, 28f),
                visualStyle == MapVisualStyle.NeonHub ? 0.1f : 18f,
                FontStyles.Normal,
                new Color(1f, 1f, 1f, 0.88f));
            Image accent = CreateDecor(
                "Accent",
                buttonRect,
                new Vector2(0.5f, 0.90f),
                visualStyle == MapVisualStyle.NeonHub ? new Vector2(88f, 10f) : new Vector2(126f, 14f),
                Vector2.zero,
                GetDefaultSprite(),
                accentColor);
            Image badge = CreateDecor(
                "Badge",
                buttonRect,
                new Vector2(0.84f, 0.82f),
                visualStyle == MapVisualStyle.NeonHub ? new Vector2(30f, 30f) : new Vector2(38f, 38f),
                Vector2.zero,
                GetRoundSprite(),
                new Color(1f, 1f, 1f, 0.24f));

            return new NodeRefs
            {
                root = rootRect,
                shadow = shadow,
                glow = glow,
                button = button,
                plate = plate,
                numberLabel = numberLabel,
                captionLabel = captionLabel,
                statusLabel = statusLabel,
                accent = accent,
                badge = badge
            };
        }

        private void EnsureSegmentCount(int count)
        {
            while (_segments.Count < count)
            {
                _segments.Add(CreateSegment(_segments.Count));
            }

            for (int i = 0; i < _segments.Count; i++)
            {
                _segments[i].root.gameObject.SetActive(i < count);
            }
        }

        private SegmentRefs CreateSegment(int index)
        {
            GameObject root = new GameObject($"RoadSegment_{index + 1}", typeof(RectTransform));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(_roadLayer, false);
            rootRect.anchorMin = new Vector2(0.5f, 1f);
            rootRect.anchorMax = new Vector2(0.5f, 1f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);

            Image shadow = CreateDecor("Shadow", rootRect, new Vector2(0.5f, 0.5f), new Vector2(100f, roadOuterThickness + 20f), new Vector2(0f, -14f), GetDefaultSprite(), roadShadowColor);
            Image outer = CreateDecor("Outer", rootRect, new Vector2(0.5f, 0.5f), new Vector2(100f, roadOuterThickness), Vector2.zero, GetDefaultSprite(), roadOuterColor);
            Image inner = CreateDecor("Inner", rootRect, new Vector2(0.5f, 0.5f), new Vector2(100f, roadInnerThickness), Vector2.zero, GetDefaultSprite(), roadInnerColor);
            Image lane = CreateDecor("Lane", rootRect, new Vector2(0.5f, 0.5f), new Vector2(100f, roadLaneThickness), Vector2.zero, GetDefaultSprite(), roadLaneColor);

            return new SegmentRefs
            {
                root = rootRect,
                shadow = shadow,
                outer = outer,
                inner = inner,
                lane = lane
            };
        }

        private void RebuildDecorations()
        {
            for (int i = 0; i < _decorations.Count; i++)
            {
                if (_decorations[i] != null)
                {
                    Destroy(_decorations[i].gameObject);
                }
            }

            _decorations.Clear();

            if (_levels.Length <= 0)
            {
                return;
            }

            if (visualStyle == MapVisualStyle.NeonHub)
            {
                int pulses = Mathf.Max(3, Mathf.CeilToInt(_levels.Length / 6f));
                for (int i = 0; i < pulses; i++)
                {
                    float t = pulses == 1 ? 0f : i / (float)(pulses - 1);
                    float y = -(topPadding + t * Mathf.Max(0f, (_levels.Length - 1) * verticalSpacing));
                    float x = ((i & 1) == 0 ? -1f : 1f) * (pathAmplitude + 146f + (14f * (i % 2)));
                    _decorations.Add(CreateContentDecoration($"GlowOrb_{i}", new Vector2(x, y - 24f), new Vector2(120f, 120f), new Color(0.55f, 0.35f, 1f, 0.16f)));
                    _decorations.Add(CreateContentDecoration($"Spark_{i}", new Vector2(x * 0.72f, y + 58f), new Vector2(52f, 52f), new Color(1f, 0.68f, 0.28f, 0.14f)));
                }

                return;
            }

            int steps = Mathf.Max(4, Mathf.CeilToInt(_levels.Length / 5f));
            for (int i = 0; i < steps; i++)
            {
                float t = steps == 1 ? 0f : i / (float)(steps - 1);
                float y = -(topPadding + t * Mathf.Max(0f, (_levels.Length - 1) * verticalSpacing));
                float x = ((i & 1) == 0 ? -1f : 1f) * (pathAmplitude + 188f + (18f * (i % 3)));
                Color primary = ((i & 1) == 0)
                    ? new Color(1f, 0.97f, 0.88f, 0.80f)
                    : new Color(1f, 0.83f, 0.66f, 0.68f);
                Color secondary = ((i & 1) == 0)
                    ? new Color(0.60f, 0.80f, 0.30f, 0.44f)
                    : new Color(0.40f, 0.72f, 0.92f, 0.34f);

                _decorations.Add(CreateContentDecoration($"Decoration_{i}_A", new Vector2(x, y), new Vector2(178f, 178f), primary));
                _decorations.Add(CreateContentDecoration($"Decoration_{i}_B", new Vector2(x + (((i & 1) == 0) ? 52f : -52f), y - 72f), new Vector2(78f, 78f), secondary));
            }
        }

        private RectTransform CreateContentDecoration(string name, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            Image image = CreateDecor(name, _decorLayer, new Vector2(0.5f, 1f), size, anchoredPosition, GetRoundSprite(), color);
            return image.rectTransform;
        }

        private void LayoutMap()
        {
            float viewportWidth = viewport != null && viewport.rect.width > 10f ? viewport.rect.width : 840f;
            float contentHeight = Mathf.Max(viewport.rect.height + 20f, topPadding + bottomPadding + Mathf.Max(0, _levels.Length - 1) * verticalSpacing);
            content.sizeDelta = new Vector2(viewportWidth, contentHeight);
            _decorLayer.sizeDelta = content.sizeDelta;
            _roadLayer.sizeDelta = content.sizeDelta;
            _nodeLayer.sizeDelta = content.sizeDelta;

            for (int i = 0; i < _levels.Length; i++)
            {
                NodeRefs node = _nodes[i];
                LevelDefinition level = _levels[i];
                Vector2 point = GetPathPoint(i);
                node.root.anchoredPosition = point;
                node.levelId = level != null ? level.levelId : i + 1;
                node.button.onClick.RemoveAllListeners();
                int capturedLevelId = node.levelId;
                node.button.onClick.AddListener(() => _onLevelSelected?.Invoke(capturedLevelId));

                if (node.numberLabel != null)
                {
                    node.numberLabel.text = capturedLevelId.ToString();
                }

                if (node.captionLabel != null)
                {
                    node.captionLabel.text = visualStyle == MapVisualStyle.NeonHub ? string.Empty : _levelCaption.ToUpperInvariant();
                }

                if (node.statusLabel != null)
                {
                    node.statusLabel.text = BuildNodeStatus(capturedLevelId);
                }

                if (visualStyle == MapVisualStyle.NeonHub)
                {
                    float tilt = capturedLevelId == _focusLevelId ? 0f : ((i & 1) == 0 ? -4f : 4f);
                    node.button.transform.localRotation = Quaternion.Euler(0f, 0f, tilt);
                }
                else
                {
                    node.button.transform.localRotation = Quaternion.identity;
                }

                ApplyNodeState(node, capturedLevelId);
            }

            for (int i = 0; i < _segments.Count; i++)
            {
                Vector2 start = GetPathPoint(i);
                Vector2 end = GetPathPoint(i + 1);
                LayoutSegment(_segments[i], start, end);
            }
        }

        private Vector2 GetPathPoint(int levelIndex)
        {
            int visualIndexFromTop = (_levels.Length - 1) - levelIndex;
            float y = -(topPadding + (visualIndexFromTop * verticalSpacing));
            float wave = Mathf.Sin(levelIndex * pathFrequency) + (0.34f * Mathf.Sin((levelIndex * 0.46f) + 0.8f));
            float x = wave * pathAmplitude;
            return new Vector2(x, y);
        }

        private void LayoutSegment(SegmentRefs segment, Vector2 start, Vector2 end)
        {
            Vector2 midpoint = (start + end) * 0.5f;
            Vector2 delta = end - start;
            float length = Mathf.Max(40f, delta.magnitude + 18f);
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            segment.root.anchoredPosition = midpoint;
            segment.root.localRotation = Quaternion.Euler(0f, 0f, angle);
            segment.root.sizeDelta = new Vector2(length, roadOuterThickness + 24f);

            if (visualStyle == MapVisualStyle.NeonHub)
            {
                SetSize(segment.shadow.rectTransform, length + 20f, roadOuterThickness + 20f);
                SetSize(segment.outer.rectTransform, length, roadOuterThickness);
                SetSize(segment.inner.rectTransform, length - 8f, roadInnerThickness);
                SetSize(segment.lane.rectTransform, length - 40f, roadLaneThickness);
            }
            else
            {
                SetSize(segment.shadow.rectTransform, length + 12f, roadOuterThickness + 24f);
                SetSize(segment.outer.rectTransform, length, roadOuterThickness);
                SetSize(segment.inner.rectTransform, length - 8f, roadInnerThickness);
                SetSize(segment.lane.rectTransform, length - 28f, roadLaneThickness);
            }
        }

        private void RefreshViewportDrivenVisuals()
        {
            if (viewport == null)
            {
                return;
            }

            float viewportHalfHeight = Mathf.Max(1f, viewport.rect.height * 0.5f);
            for (int i = 0; i < _levels.Length; i++)
            {
                NodeRefs node = _nodes[i];
                if (node.root == null || !node.root.gameObject.activeSelf)
                {
                    continue;
                }

                Vector3 nodeWorld = node.root.TransformPoint(Vector3.zero);
                Vector3 viewportLocal = viewport.InverseTransformPoint(nodeWorld);
                float proximity = 1f - Mathf.Clamp01(Mathf.Abs(viewportLocal.y) / (viewportHalfHeight * 1.06f));
                float scale = Mathf.Lerp(minNodeScale, maxNodeScale, proximity);
                if (node.levelId == _focusLevelId)
                {
                    scale *= 1.06f;
                }

                node.root.localScale = Vector3.one * scale;

                float alpha = Mathf.Lerp(0.55f, 1f, Mathf.Clamp01(proximity + 0.18f));
                SetAlpha(node.glow, alpha * node.baseGlowAlpha);
                SetAlpha(node.shadow, Mathf.Lerp(0.16f, 0.26f, proximity));
                SetAlpha(node.plate, Mathf.Lerp(0.82f, 1f, proximity));
                SetTextAlpha(node.captionLabel, visualStyle == MapVisualStyle.NeonHub ? 0f : Mathf.Lerp(0.58f, 0.95f, proximity));
                SetTextAlpha(node.statusLabel, visualStyle == MapVisualStyle.NeonHub ? 0f : Mathf.Lerp(0.46f, 0.90f, proximity));
                SetTextAlpha(node.numberLabel, Mathf.Lerp(0.70f, 1f, proximity));
                SetAlpha(node.badge, Mathf.Lerp(0.18f, 0.34f, proximity));
            }
        }

        private void ApplyNodeState(NodeRefs node, int levelId)
        {
            bool isActive = levelId == _focusLevelId;
            bool isCompleted = levelId < _focusLevelId;

            Color plateColor = isActive ? activeNodeColor : isCompleted ? completedNodeColor : futureNodeColor;
            Color glowColor = isActive ? activeGlowColor : isCompleted ? completedGlowColor : futureGlowColor;

            node.plate.color = plateColor;
            node.glow.color = glowColor;
            node.baseGlowAlpha = glowColor.a;
            node.shadow.color = new Color(0f, 0f, 0f, isActive ? 0.24f : 0.18f);
            node.accent.color = new Color(accentColor.r, accentColor.g, accentColor.b, isActive ? 1f : 0.84f);
            node.badge.color = new Color(1f, 1f, 1f, isActive ? 0.30f : 0.18f);

            if (visualStyle == MapVisualStyle.NeonHub)
            {
                node.plate.color = isActive ? activeNodeColor : isCompleted ? completedNodeColor : futureNodeColor;
                node.glow.color = isActive ? activeGlowColor : isCompleted ? completedGlowColor : futureGlowColor;
                node.baseGlowAlpha = node.glow.color.a;
                node.accent.color = new Color(accentColor.r, accentColor.g, accentColor.b, isActive ? 1f : 0f);
                node.badge.color = new Color(0.06f, 0.08f, 0.18f, levelId > _focusLevelId ? 0.84f : 0f);
                if (node.statusLabel != null)
                {
                    node.statusLabel.text = string.Empty;
                }
            }

            if (node.statusLabel != null)
            {
                node.statusLabel.color = isActive
                    ? new Color(1f, 0.96f, 0.92f, 0.92f)
                    : isCompleted
                        ? new Color(0.90f, 1f, 0.92f, 0.86f)
                        : new Color(0.92f, 0.96f, 1f, 0.82f);
            }
        }

        private string BuildNodeStatus(int levelId)
        {
            if (visualStyle == MapVisualStyle.NeonHub)
            {
                return string.Empty;
            }

            if (levelId == _focusLevelId)
            {
                return "AKTIF";
            }

            if (levelId < _focusLevelId)
            {
                return "GECILDI";
            }

            return "SEC";
        }

        private float GetDistanceFromTop(int levelIndex)
        {
            return topPadding + ((_levels.Length - 1 - levelIndex) * verticalSpacing);
        }

        private static Image CreateDecor(string name, RectTransform parent, Vector2 anchor, Vector2 size, Vector2 position, Sprite sprite, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            rect.localScale = Vector3.one;

            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.type = sprite != null && sprite.border.sqrMagnitude > 0f ? Image.Type.Sliced : Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static TextMeshProUGUI EnsureText(RectTransform parent, string name, Vector2 anchor, Vector2 size, float fontSize, FontStyles fontStyle, Color color)
        {
            Transform existing = parent.Find(name);
            TextMeshProUGUI label;
            if (existing == null)
            {
                GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
                RectTransform rect = go.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
                rect.anchorMin = anchor;
                rect.anchorMax = anchor;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.sizeDelta = size;
                rect.anchoredPosition = Vector2.zero;
                label = go.GetComponent<TextMeshProUGUI>();
            }
            else
            {
                label = existing.GetComponent<TextMeshProUGUI>();
            }

            label.fontSize = fontSize;
            label.fontStyle = fontStyle;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.enableWordWrapping = false;
            label.raycastTarget = false;
            return label;
        }

        private static void SetSize(RectTransform rect, float width, float height)
        {
            if (rect == null)
            {
                return;
            }

            rect.sizeDelta = new Vector2(width, height);
        }

        private static void SetAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
            {
                return;
            }

            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }

        private static void SetTextAlpha(TextMeshProUGUI label, float alpha)
        {
            if (label == null)
            {
                return;
            }

            Color color = label.color;
            color.a = alpha;
            label.color = color;
        }

        private static Sprite GetDefaultSprite()
        {
            if (s_defaultSprite != null)
            {
                return s_defaultSprite;
            }

            Texture2D texture = Texture2D.whiteTexture;
            s_defaultSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(0f, 0f, 0f, 0f));
            s_defaultSprite.name = "LevelPathMap_DefaultSprite";
            return s_defaultSprite;
        }

        private static Sprite GetRoundSprite()
        {
            if (s_roundSprite != null)
            {
                return s_roundSprite;
            }

            const int size = 64;
            Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            texture.name = "LevelPathMap_RoundTexture";
            texture.wrapMode = TextureWrapMode.Clamp;
            texture.filterMode = FilterMode.Bilinear;

            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            float radius = (size - 2) * 0.5f;
            Color clear = new Color(1f, 1f, 1f, 0f);
            Color fill = Color.white;

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    texture.SetPixel(x, y, distance <= radius ? fill : clear);
                }
            }

            texture.Apply(false, true);

            s_roundSprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.Tight);
            s_roundSprite.name = "LevelPathMap_RoundSprite";
            return s_roundSprite;
        }

        [Serializable]
        private sealed class NodeRefs
        {
            public RectTransform root;
            public Image shadow;
            public Image glow;
            public Button button;
            public Image plate;
            public TextMeshProUGUI numberLabel;
            public TextMeshProUGUI captionLabel;
            public TextMeshProUGUI statusLabel;
            public Image accent;
            public Image badge;
            public float baseGlowAlpha;
            public int levelId;
        }

        [Serializable]
        private sealed class SegmentRefs
        {
            public RectTransform root;
            public Image shadow;
            public Image outer;
            public Image inner;
            public Image lane;
        }
    }
}
