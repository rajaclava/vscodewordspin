using System.Collections.Generic;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class RotatorPlaquePresenter : MonoBehaviour
    {
        [SerializeField] private Transform rotatorRoot;
        [SerializeField] private Transform rotatorVisualRoot;
        [SerializeField] private Transform anchorRoot;

        private readonly List<PlaqueVisual> _plaques = new List<PlaqueVisual>();
        private Transform _generatedDiskRoot;
        private Sprite _squareSprite;
        private Sprite _roundSprite;

        private void Awake()
        {
            ResolveReferences();
            RebuildVisuals();
            RestoreActiveState();
        }

        public void RebuildLayout()
        {
            ResolveReferences();
            RebuildVisuals();
            RestoreActiveState();
        }

        private void LateUpdate()
        {
            SyncToAnchors();
        }

        private void OnEnable()
        {
            GameEvents.TargetSlotUpdated += HandleTargetSlotUpdated;
            GameEvents.HitEvaluated += HandleHitEvaluated;
            GameEvents.QuestionFailed += HandleQuestionFailed;
            GameEvents.LevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            GameEvents.TargetSlotUpdated -= HandleTargetSlotUpdated;
            GameEvents.HitEvaluated -= HandleHitEvaluated;
            GameEvents.QuestionFailed -= HandleQuestionFailed;
            GameEvents.LevelCompleted -= HandleLevelCompleted;
        }

        private void HandleTargetSlotUpdated(int slotIndex, int answerIndex, char letter)
        {
            for (int i = 0; i < _plaques.Count; i++)
            {
                _plaques[i].SetActive(i == slotIndex);
            }
        }

        private void HandleHitEvaluated(HitData hit)
        {
            if (hit.expectedSlotIndex >= 0 && hit.expectedSlotIndex < _plaques.Count)
            {
                if (hit.resultType == HitResultType.Perfect)
                {
                    _plaques[hit.expectedSlotIndex].Flash(_plaques[hit.expectedSlotIndex].perfectFlashColor, 1.15f);
                }
                else if (hit.resultType == HitResultType.Tolerated)
                {
                    _plaques[hit.expectedSlotIndex].Flash(_plaques[hit.expectedSlotIndex].toleratedFlashColor, 1.10f);
                }
                else if (hit.resultType == HitResultType.NearMiss)
                {
                    _plaques[hit.expectedSlotIndex].Flash(_plaques[hit.expectedSlotIndex].failFlashColor, 1.05f);
                }
            }

            if (hit.resultType == HitResultType.WrongSlot && hit.slotIndex >= 0 && hit.slotIndex < _plaques.Count)
            {
                _plaques[hit.slotIndex].Flash(_plaques[hit.slotIndex].failFlashColor, 1.08f);
            }
        }

        private void HandleQuestionFailed()
        {
            ApplyIdleState();
        }

        private void HandleLevelCompleted(LevelContext context)
        {
            ApplyIdleState();
        }

        private void ResolveReferences()
        {
            if (rotatorRoot == null)
            {
                TargetRotator rotator = FindObjectOfType<TargetRotator>();
                if (rotator != null)
                {
                    rotatorRoot = rotator.transform;
                }
            }

            if (rotatorRoot != null && rotatorVisualRoot == null)
            {
                Transform visual = rotatorRoot.Find("RotatorVisual");
                if (visual != null)
                {
                    rotatorVisualRoot = visual;
                }
            }

            if (rotatorRoot != null && anchorRoot == null)
            {
                Transform anchors = rotatorRoot.Find("AnchorRoot");
                if (anchors != null)
                {
                    anchorRoot = anchors;
                }
            }
        }

        private void RebuildVisuals()
        {
            ClearVisuals();

            if (rotatorVisualRoot == null || anchorRoot == null)
            {
                return;
            }

            CacheSourceSprites();
            float anchorRadius = GetAnchorRadius();
            BuildGeneratedDisk(anchorRadius);
            BuildPlaques();
        }

        private void CacheSourceSprites()
        {
            if (_squareSprite == null || _roundSprite == null)
            {
                foreach (Transform anchor in anchorRoot)
                {
                    if (anchor == null)
                    {
                        continue;
                    }

                    if (_squareSprite == null)
                    {
                        SpriteRenderer glyph = FindRenderer(anchor, "RuneGlyph");
                        if (glyph != null)
                        {
                            _squareSprite = glyph.sprite;
                        }
                    }

                    if (_roundSprite == null)
                    {
                        SpriteRenderer core = FindRenderer(anchor, "PlaqueCore");
                        if (core != null)
                        {
                            _roundSprite = core.sprite;
                        }
                    }
                }
            }

            if (_squareSprite == null)
            {
                _squareSprite = _roundSprite;
            }

            if (_roundSprite == null)
            {
                _roundSprite = _squareSprite;
            }
        }

        private float GetAnchorRadius()
        {
            float radius = 1.0f;
            foreach (Transform anchor in anchorRoot)
            {
                if (anchor == null)
                {
                    continue;
                }

                radius = Mathf.Max(radius, anchor.localPosition.magnitude);
            }

            return radius;
        }

        private void BuildGeneratedDisk(float anchorRadius)
        {
            ShapeLayoutDefinition currentLayout = ResolveCurrentShapeLayout();
            if (TryBuildPrefabVisual(currentLayout))
            {
                return;
            }

            bool adaptivePlaqueLayout = ShapeLayoutGeometry.UsesAdaptivePlaqueVisuals(currentLayout);

            _generatedDiskRoot = new GameObject("GeneratedRotatorDisk").transform;
            _generatedDiskRoot.SetParent(rotatorVisualRoot, false);
            _generatedDiskRoot.localPosition = Vector3.zero;
            _generatedDiskRoot.localRotation = Quaternion.identity;
            _generatedDiskRoot.localScale = Vector3.one;

            Vector2 layoutScale = ResolveLayoutScale(currentLayout);
            float actionRingBias = ResolveRadiusBias(currentLayout, "action");
            float plaqueBandBias = ResolveRadiusBias(currentLayout, "plaque");
            float gapBias = ResolveRadiusBias(currentLayout, "gap");
            float centerBias = ResolveRadiusBias(currentLayout, "center");
            float outerRadius = anchorRadius + (adaptivePlaqueLayout ? 0.72f : 0.86f);
            float actionRingOuterRadius = anchorRadius + (adaptivePlaqueLayout ? 0.48f : 0.60f) + actionRingBias;
            float plaqueBandRadius = anchorRadius + (adaptivePlaqueLayout ? 0.18f : 0.34f) + plaqueBandBias;
            float gapOuterRadius = Mathf.Max(0.85f, anchorRadius - 0.02f + gapBias);
            float gapInnerRadius = Mathf.Max(0.66f, anchorRadius - 0.34f + gapBias);
            float centerOuterRadius = Mathf.Max(0.58f, anchorRadius - 0.48f + centerBias);
            float centerInnerRadius = Mathf.Max(0.42f, anchorRadius - 0.66f + centerBias);
            Color actionGlowColor = adaptivePlaqueLayout ? new Color(1f, 0.56f, 0.18f, 0.06f) : new Color(1f, 0.56f, 0.18f, 0.10f);
            Color plaqueBandColor = adaptivePlaqueLayout ? new Color(0.55f, 0.34f, 0.19f, 0.16f) : new Color(0.55f, 0.34f, 0.19f, 0.32f);

            CreateDiskLayer("ActionShadow", _generatedDiskRoot, _roundSprite, layoutScale * (outerRadius * 2.44f), new Color(0f, 0f, 0f, 0.40f), 30, new Vector3(0f, -0.06f, 0f));
            CreateDiskLayer("ActionGlow", _generatedDiskRoot, _roundSprite, layoutScale * (outerRadius * 2.36f), actionGlowColor, 31, Vector3.zero);
            CreateDiskLayer("ActionOuterRim", _generatedDiskRoot, _roundSprite, layoutScale * (outerRadius * 2.24f), new Color(0.15f, 0.09f, 0.07f, 1f), 32, Vector3.zero);
            CreateDiskLayer("ActionRing", _generatedDiskRoot, _roundSprite, layoutScale * (actionRingOuterRadius * 2.22f), new Color(0.30f, 0.18f, 0.14f, 0.98f), 33, Vector3.zero);
            CreateDiskLayer("PlaqueBand", _generatedDiskRoot, _roundSprite, layoutScale * (plaqueBandRadius * 2.28f), plaqueBandColor, 34, Vector3.zero);
            CreateDiskLayer("GapRim", _generatedDiskRoot, _roundSprite, layoutScale * (gapOuterRadius * 2.10f), new Color(0.20f, 0.13f, 0.10f, 0.98f), 35, Vector3.zero);
            CreateDiskLayer("GapCutout", _generatedDiskRoot, _roundSprite, layoutScale * (gapInnerRadius * 2.04f), new Color(0.10f, 0.08f, 0.08f, 0.98f), 36, Vector3.zero);
            CreateDiskLayer("CenterShadow", _generatedDiskRoot, _roundSprite, layoutScale * (centerOuterRadius * 2.18f), new Color(0f, 0f, 0f, 0.24f), 37, new Vector3(0f, -0.02f, 0f));
            CreateDiskLayer("CenterOuter", _generatedDiskRoot, _roundSprite, layoutScale * (centerOuterRadius * 2.06f), new Color(0.25f, 0.16f, 0.12f, 1f), 38, Vector3.zero);
            CreateDiskLayer("CenterFace", _generatedDiskRoot, _roundSprite, layoutScale * (centerInnerRadius * 2.02f), new Color(0.14f, 0.10f, 0.09f, 0.98f), 39, Vector3.zero);
            CreateDiskLayer("CenterInset", _generatedDiskRoot, _roundSprite, layoutScale * (centerInnerRadius * 1.18f), new Color(0.72f, 0.44f, 0.22f, 0.18f), 40, Vector3.zero);
            AddLayoutAccents(currentLayout, anchorRadius, layoutScale);
        }

        private void BuildPlaques()
        {
            ShapeLayoutDefinition currentLayout = ResolveCurrentShapeLayout();
            ShapePointDefinition[] resolvedPoints = ShapeLayoutGeometry.ResolvePoints(currentLayout);
            foreach (Transform anchor in anchorRoot)
            {
                Slot slot = anchor != null ? anchor.GetComponent<Slot>() : null;
                if (slot == null || !slot.gameObject.activeInHierarchy)
                {
                    continue;
                }

                GameObject plaqueRoot = new GameObject(anchor.name + "_Plaque");
                plaqueRoot.transform.SetParent(_generatedDiskRoot != null ? _generatedDiskRoot : rotatorVisualRoot, false);
                plaqueRoot.transform.localPosition = anchor.localPosition;
                plaqueRoot.transform.localRotation = anchor.localRotation;
                plaqueRoot.transform.localScale = Vector3.one;

                ShapePlaqueVisualLayoutInfo visualLayout = ShapeLayoutGeometry.ResolvePlaqueVisualLayout(
                    currentLayout,
                    resolvedPoints,
                    slot.SlotIndex,
                    resolvedPoints != null ? resolvedPoints.Length : 0);
                ApplyLiveSlotVisualOverride(currentLayout, slot, ref visualLayout);
                Vector2 plaqueSize = visualLayout.plaqueSize;
                Vector2 innerSize = visualLayout.innerSize;
                Vector2 runeSize = visualLayout.runeSize;
                Vector2 seatSize = visualLayout.seatSize;

                PlaqueVisual plaque = new PlaqueVisual
                {
                    anchor = anchor,
                    root = plaqueRoot.transform,
                    outwardOffset = visualLayout.outwardOffset,
                    localRotationOffsetDegrees = visualLayout.localRotationDegrees,
                    shadow = CreatePlaqueLayer("Shadow", plaqueRoot.transform, seatSize + new Vector2(0.06f, 0.06f), new Color(0f, 0f, 0f, 0.24f), 41, new Vector3(0f, -0.03f, 0f)),
                    seat = CreatePlaqueLayer("Seat", plaqueRoot.transform, seatSize, new Color(0.12f, 0.08f, 0.06f, 0.96f), 42, new Vector3(0f, -0.02f, 0f)),
                    frame = CreatePlaqueLayer("Frame", plaqueRoot.transform, plaqueSize, new Color(0.17f, 0.11f, 0.09f, 0.98f), 43, Vector3.zero),
                    face = CreatePlaqueLayer("Face", plaqueRoot.transform, innerSize, new Color(0.58f, 0.37f, 0.21f, 0.88f), 44, Vector3.zero),
                    rune = CreatePlaqueLayer("Rune", plaqueRoot.transform, runeSize, new Color(0.18f, 0.10f, 0.04f, 0.55f), 45, Vector3.zero),
                    glow = CreatePlaqueLayer("Glow", plaqueRoot.transform, visualLayout.glowSize, new Color(1f, 0.60f, 0.22f, 0.58f), 46, Vector3.zero)
                };

                plaque.baseFrameColor = plaque.frame != null ? plaque.frame.color : Color.white;
                plaque.baseFaceColor = plaque.face != null ? plaque.face.color : Color.white;
                plaque.baseGlowColor = plaque.glow != null ? plaque.glow.color : Color.white;
                plaque.inactiveFrameColor = Color.Lerp(slot.InactiveColor, new Color(0.14f, 0.09f, 0.07f, slot.InactiveColor.a), 0.42f);
                plaque.inactiveFaceColor = slot.InactiveColor;
                plaque.inactiveSeatColor = Color.Lerp(slot.InactiveColor, new Color(0.10f, 0.07f, 0.05f, slot.InactiveColor.a), 0.65f);
                plaque.activeFrameColor = slot.ActiveColor;
                plaque.activeFaceColor = Color.Lerp(slot.ActiveColor, Color.white, 0.22f);
                plaque.activeSeatColor = Color.Lerp(slot.ActiveColor, new Color(0.14f, 0.09f, 0.07f, 1f), 0.45f);
                plaque.activeGlowColor = WithAlpha(slot.ActiveColor, Mathf.Clamp01(Mathf.Max(0.20f, slot.ActiveColor.a)));
                plaque.activeScaleMultiplier = Mathf.Max(1f, slot.ActiveScaleMultiplier);
                plaque.perfectFlashColor = slot.PerfectFeedbackColor;
                plaque.toleratedFlashColor = slot.ToleratedFeedbackColor;
                plaque.failFlashColor = slot.FailFeedbackColor;

                if (plaque.glow != null)
                {
                    plaque.glow.enabled = false;
                }

                _plaques.Add(plaque);
                ApplyPlaqueAnchorTransform(plaque);
            }
        }

        private void SyncToAnchors()
        {
            for (int i = 0; i < _plaques.Count; i++)
            {
                PlaqueVisual plaque = _plaques[i];
                if (plaque.anchor == null || plaque.root == null)
                {
                    continue;
                }

                ApplyPlaqueAnchorTransform(plaque);
                plaque.Tick();
            }
        }

        private static void ApplyPlaqueAnchorTransform(PlaqueVisual plaque)
        {
            if (plaque.anchor == null || plaque.root == null)
            {
                return;
            }

            Vector3 outwardOffset = plaque.anchor.localRotation * (Vector3.up * plaque.outwardOffset);
            plaque.root.localPosition = plaque.anchor.localPosition + outwardOffset;
            plaque.root.localRotation = plaque.anchor.localRotation * Quaternion.Euler(0f, 0f, plaque.localRotationOffsetDegrees);
        }

        private void ApplyIdleState()
        {
            for (int i = 0; i < _plaques.Count; i++)
            {
                _plaques[i].SetActive(false);
            }
        }

        private void RestoreActiveState()
        {
            SlotManager manager = FindObjectOfType<SlotManager>();
            if (manager == null || manager.CurrentTargetSlot < 0)
            {
                ApplyIdleState();
                return;
            }

            for (int i = 0; i < _plaques.Count; i++)
            {
                _plaques[i].SetActive(i == manager.CurrentTargetSlot);
            }
        }

        private void ClearVisuals()
        {
            if (_generatedDiskRoot != null)
            {
                Destroy(_generatedDiskRoot.gameObject);
                _generatedDiskRoot = null;
            }

            for (int i = 0; i < _plaques.Count; i++)
            {
                if (_plaques[i].root != null)
                {
                    Destroy(_plaques[i].root.gameObject);
                }
            }

            _plaques.Clear();
        }

        private SpriteRenderer CreateDiskLayer(string name, Transform parent, Sprite sprite, Vector2 scale, Color color, int sortingOrder, Vector3 localPosition)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = new Vector3(scale.x, scale.y, 1f);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private SpriteRenderer CreatePlaqueLayer(string name, Transform parent, Vector2 size, Color color, int sortingOrder, Vector3 localPosition)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.identity;
            go.transform.localScale = Vector3.one;

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = _squareSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
            return renderer;
        }

        private static SpriteRenderer FindRenderer(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            return child != null ? child.GetComponent<SpriteRenderer>() : null;
        }

        private ShapeLayoutDefinition ResolveCurrentShapeLayout()
        {
            LevelFlowController flow = FindObjectOfType<LevelFlowController>();
            return flow != null ? flow.CurrentShapeLayout : null;
        }

        private bool TryBuildPrefabVisual(ShapeLayoutDefinition layout)
        {
            if (layout == null || string.IsNullOrWhiteSpace(layout.visualPrefabResourcePath))
            {
                return false;
            }

            GameObject prefab = Resources.Load<GameObject>(layout.visualPrefabResourcePath);
            if (prefab == null)
            {
                return false;
            }

            GameObject instance = Instantiate(prefab, rotatorVisualRoot);
            instance.name = "GeneratedRotatorDisk";
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localRotation = Quaternion.identity;
            instance.transform.localScale = Vector3.one;
            _generatedDiskRoot = instance.transform;
            return true;
        }

        private static Vector2 ResolveLayoutScale(ShapeLayoutDefinition layout)
        {
            if (layout == null)
            {
                return Vector2.one;
            }

            string layoutId = layout.shapeLayoutId ?? string.Empty;
            switch (layoutId)
            {
                case "shield_guard":
                    return new Vector2(1.06f, 1.10f);
                case "crown_arc":
                    return new Vector2(1.02f, 1.12f);
                case "square_lock":
                    return new Vector2(1.10f, 0.98f);
                case "hex_bloom":
                    return new Vector2(1.07f, 1.03f);
            }

            switch ((layout.shapeFamily ?? "circle").ToLowerInvariant())
            {
                case "oval":
                case "ellipse":
                    return new Vector2(1.12f, 0.92f);
                case "diamond":
                    return new Vector2(1.06f, 1.00f);
                case "hex":
                    return new Vector2(1.04f, 1.02f);
                case "square":
                    return new Vector2(1.08f, 0.96f);
                default:
                    return Vector2.one;
            }
        }

        private float ResolveRadiusBias(ShapeLayoutDefinition layout, string layer)
        {
            string layoutId = layout != null ? layout.shapeLayoutId : string.Empty;
            switch (layoutId)
            {
                case "shield_guard":
                    return layer == "action" ? 0.10f : layer == "plaque" ? 0.06f : layer == "center" ? -0.02f : 0f;
                case "crown_arc":
                    return layer == "action" ? 0.08f : layer == "plaque" ? 0.04f : 0f;
                case "square_lock":
                    return layer == "plaque" ? 0.08f : layer == "gap" ? -0.02f : 0f;
                case "hex_bloom":
                    return layer == "plaque" ? 0.10f : layer == "action" ? 0.04f : 0f;
                default:
                    return 0f;
            }
        }

        private void AddLayoutAccents(ShapeLayoutDefinition layout, float anchorRadius, Vector2 layoutScale)
        {
            if (_generatedDiskRoot == null || layout == null)
            {
                return;
            }

            switch (layout.shapeLayoutId)
            {
                case "square_lock":
                    AddSquareLockAccents(anchorRadius, layoutScale);
                    break;
                case "hex_bloom":
                    AddHexBloomAccents(anchorRadius, layoutScale);
                    break;
                case "shield_guard":
                    AddShieldGuardAccent(anchorRadius, layoutScale);
                    break;
                case "crown_arc":
                    AddCrownArcAccents(anchorRadius, layoutScale);
                    break;
            }
        }

        private static void ApplyLiveSlotVisualOverride(ShapeLayoutDefinition layout, Slot slot, ref ShapePlaqueVisualLayoutInfo info)
        {
            if (slot == null)
            {
                return;
            }

            Vector2 basePlaqueSize = new Vector2(
                Mathf.Max(0.14f, layout != null ? layout.plaqueWidth : 0.30f),
                Mathf.Max(0.10f, layout != null ? layout.plaqueHeight : 0.18f)) * 1.55f;
            Vector2 livePlaqueSize = slot.PlaqueSize * 1.55f;
            Vector2 scale = new Vector2(
                Mathf.Clamp(livePlaqueSize.x / Mathf.Max(0.0001f, basePlaqueSize.x), 0.55f, 2.4f),
                Mathf.Clamp(livePlaqueSize.y / Mathf.Max(0.0001f, basePlaqueSize.y), 0.55f, 2.4f));

            info.plaqueSize = ScaleVector(info.plaqueSize, scale, 0.10f, 0.08f);
            info.innerSize = ScaleVector(info.innerSize, scale, 0.08f, 0.06f);
            info.runeSize = ScaleVector(info.runeSize, scale, 0.05f, 0.07f);
            info.seatSize = ScaleVector(info.seatSize, scale, 0.10f, 0.08f);
            info.glowSize = ScaleVector(info.glowSize, scale, 0.10f, 0.08f);
        }

        private static Vector2 ScaleVector(Vector2 source, Vector2 scale, float minX, float minY)
        {
            return new Vector2(
                Mathf.Max(minX, source.x * scale.x),
                Mathf.Max(minY, source.y * scale.y));
        }

        private static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        private void AddSquareLockAccents(float anchorRadius, Vector2 layoutScale)
        {
            Vector3[] corners =
            {
                new Vector3(0.78f, 0.78f, 0f),
                new Vector3(-0.78f, 0.78f, 0f),
                new Vector3(0.78f, -0.78f, 0f),
                new Vector3(-0.78f, -0.78f, 0f)
            };

            for (int i = 0; i < corners.Length; i++)
            {
                CreateAccentPlate(
                    "LockCorner" + i,
                    corners[i],
                    new Vector2(0.28f, 0.16f),
                    new Color(0.42f, 0.25f, 0.15f, 0.82f),
                    34,
                    45f,
                    layoutScale);
            }
        }

        private void AddHexBloomAccents(float anchorRadius, Vector2 layoutScale)
        {
            for (int i = 0; i < 6; i++)
            {
                float angle = (i / 6f) * Mathf.PI * 2f;
                Vector3 local = new Vector3(Mathf.Sin(angle) * (anchorRadius + 0.18f), Mathf.Cos(angle) * (anchorRadius + 0.18f), 0f);
                CreateAccentPlate(
                    "HexBloomNode" + i,
                    local,
                    new Vector2(0.18f, 0.12f),
                    new Color(0.72f, 0.44f, 0.22f, 0.46f),
                    35,
                    -angle * Mathf.Rad2Deg,
                    layoutScale);
            }
        }

        private void AddShieldGuardAccent(float anchorRadius, Vector2 layoutScale)
        {
            CreateAccentPlate(
                "ShieldGuardPlate",
                new Vector3(0f, -(anchorRadius * 0.42f), 0f),
                new Vector2(anchorRadius * 1.05f, 0.26f),
                new Color(0.34f, 0.20f, 0.12f, 0.88f),
                34,
                0f,
                layoutScale);
        }

        private void AddCrownArcAccents(float anchorRadius, Vector2 layoutScale)
        {
            float y = anchorRadius * 0.78f;
            CreateAccentPlate("CrownTop", new Vector3(0f, y + 0.06f, 0f), new Vector2(0.24f, 0.18f), new Color(0.74f, 0.46f, 0.23f, 0.70f), 35, 0f, layoutScale);
            CreateAccentPlate("CrownLeft", new Vector3(-0.34f, y - 0.02f, 0f), new Vector2(0.20f, 0.14f), new Color(0.60f, 0.36f, 0.19f, 0.58f), 35, 18f, layoutScale);
            CreateAccentPlate("CrownRight", new Vector3(0.34f, y - 0.02f, 0f), new Vector2(0.20f, 0.14f), new Color(0.60f, 0.36f, 0.19f, 0.58f), 35, -18f, layoutScale);
        }

        private void CreateAccentPlate(string name, Vector3 localPosition, Vector2 size, Color color, int sortingOrder, float rotationZ, Vector2 layoutScale)
        {
            GameObject go = new GameObject(name);
            go.transform.SetParent(_generatedDiskRoot, false);
            go.transform.localPosition = new Vector3(localPosition.x * layoutScale.x, localPosition.y * layoutScale.y, localPosition.z);
            go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            go.transform.localScale = Vector3.one;

            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = _squareSprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            renderer.drawMode = SpriteDrawMode.Sliced;
            renderer.size = size;
        }

        private sealed class PlaqueVisual
        {
            public Transform anchor;
            public Transform root;
            public float outwardOffset;
            public float localRotationOffsetDegrees;
            public SpriteRenderer shadow;
            public SpriteRenderer seat;
            public SpriteRenderer frame;
            public SpriteRenderer face;
            public SpriteRenderer rune;
            public SpriteRenderer glow;
            public Color baseFrameColor;
            public Color baseFaceColor;
            public Color baseGlowColor;
            public Color inactiveFrameColor;
            public Color inactiveFaceColor;
            public Color inactiveSeatColor;
            public Color activeFrameColor;
            public Color activeFaceColor;
            public Color activeSeatColor;
            public Color activeGlowColor;
            public float activeScaleMultiplier = 1.04f;
            public Color perfectFlashColor;
            public Color toleratedFlashColor;
            public Color failFlashColor;

            private bool _active;
            private float _flashEndsAt;
            private Color _flashColor;
            private float _flashScale = 1f;

            public void SetActive(bool active)
            {
                _active = active;
                if (_flashEndsAt > Time.time)
                {
                    return;
                }

                ApplyBaseVisuals();
            }

            public void Flash(Color color, float scaleMultiplier)
            {
                _flashColor = color;
                _flashScale = scaleMultiplier;
                _flashEndsAt = Time.time + 0.18f;
                ApplyFlashVisuals();
            }

            public void Tick()
            {
                if (_flashEndsAt > 0f && Time.time >= _flashEndsAt)
                {
                    _flashEndsAt = 0f;
                    ApplyBaseVisuals();
                }
            }

            private void ApplyBaseVisuals()
            {
                if (frame != null)
                {
                    frame.color = _active ? activeFrameColor : inactiveFrameColor;
                }

                if (seat != null)
                {
                    seat.color = _active ? activeSeatColor : inactiveSeatColor;
                }

                if (face != null)
                {
                    face.color = _active ? activeFaceColor : inactiveFaceColor;
                }

                if (glow != null)
                {
                    glow.enabled = _active;
                    if (_active)
                    {
                        glow.color = activeGlowColor;
                    }
                }

                if (root != null)
                {
                    root.localScale = _active ? Vector3.one * activeScaleMultiplier : Vector3.one;
                }
            }

            private void ApplyFlashVisuals()
            {
                if (frame != null)
                {
                    frame.color = _flashColor;
                }

                if (seat != null)
                {
                    seat.color = Color.Lerp(_flashColor, new Color(0.14f, 0.09f, 0.07f, 1f), 0.45f);
                }

                if (face != null)
                {
                    face.color = _flashColor;
                }

                if (glow != null)
                {
                    glow.enabled = true;
                    glow.color = _flashColor;
                }

                if (root != null)
                {
                    root.localScale = Vector3.one * _flashScale;
                }
            }
        }
    }
}
