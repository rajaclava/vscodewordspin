using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class Slot : MonoBehaviour
    {
        [SerializeField] private int slotIndex;
        [SerializeField] private SpriteRenderer glowRenderer;
        [SerializeField] private SpriteRenderer bodyRenderer;
        [SerializeField] private bool anchorOnlyMode = true;
        [SerializeField] private Vector2 plaqueSize = new Vector2(0.30f, 0.18f);
        [SerializeField] private Vector2 perfectZoneSize = new Vector2(0.14f, 0.08f);
        [SerializeField] private float nearMissPadding = 0.08f;
        [SerializeField] private float hitBandDepth = 0.11f;
        [SerializeField] private float hitBandInset = 0.02f;
        [SerializeField] private Color inactiveColor = new Color(0.88f, 0.88f, 0.94f, 0.78f);
        [SerializeField] private Color activeColor = new Color(1f, 0.72f, 0.28f, 1f);
        [SerializeField] private float activeScaleMultiplier = 1.08f;
        [SerializeField] private Color perfectFeedbackColor = new Color(1f, 0.93f, 0.55f, 1f);
        [SerializeField] private Color toleratedFeedbackColor = new Color(1f, 0.74f, 0.28f, 1f);
        [SerializeField] private Color failFeedbackColor = new Color(0.96f, 0.35f, 0.27f, 1f);
        [SerializeField] private float feedbackDuration = 0.16f;

        private Vector3 _baseScale;
        private float _feedbackEndsAt;
        private Color _feedbackColor;
        private float _feedbackScaleMultiplier = 1f;
        private SpriteRenderer[] _anchorVisualRenderers = new SpriteRenderer[0];

        public int SlotIndex => slotIndex;
        public char TargetLetter { get; private set; }
        public bool IsActiveTarget { get; private set; }
        public Vector2 PlaqueSize => plaqueSize;
        public Vector2 PerfectZoneSize => perfectZoneSize;
        public float NearMissPadding => nearMissPadding;

        private void Awake()
        {
            if (bodyRenderer == null)
            {
                bodyRenderer = GetComponent<SpriteRenderer>();
            }

            _baseScale = transform.localScale;
            ResolveAnchorVisuals();
            ApplyVisualState(false);
        }

        private void OnEnable()
        {
            GameEvents.HitEvaluated += HandleHitEvaluated;
            GameEvents.QuestionFailed += HandleQuestionReset;
            GameEvents.LevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            GameEvents.HitEvaluated -= HandleHitEvaluated;
            GameEvents.QuestionFailed -= HandleQuestionReset;
            GameEvents.LevelCompleted -= HandleLevelCompleted;
        }

        private void Update()
        {
            if (_feedbackEndsAt > 0f && Time.time >= _feedbackEndsAt)
            {
                _feedbackEndsAt = 0f;
                ApplyVisualState(IsActiveTarget);
            }
        }

        public void Configure(int index, char targetLetter)
        {
            slotIndex = index;
            TargetLetter = char.ToUpperInvariant(targetLetter);
            Deactivate();
        }

        public void ApplyShapeLayout(Vector2 newPlaqueSize, float perfectWidthScale, float perfectHeightScale, float newNearMissPadding)
        {
            plaqueSize = newPlaqueSize;
            perfectZoneSize = new Vector2(
                Mathf.Max(0.04f, plaqueSize.x * Mathf.Clamp(perfectWidthScale, 0.2f, 0.95f)),
                Mathf.Max(0.03f, plaqueSize.y * Mathf.Clamp(perfectHeightScale, 0.2f, 0.95f)));
            nearMissPadding = Mathf.Max(0.01f, newNearMissPadding);

            CircleCollider2D circle = GetComponent<CircleCollider2D>();
            if (circle != null)
            {
                circle.radius = Mathf.Max(0.09f, Mathf.Max(plaqueSize.x, plaqueSize.y) * 0.42f);
            }
        }

        public void ClearAttachedPins()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (child == null)
                {
                    continue;
                }

                if (child.GetComponent<Pin>() != null)
                {
                    Destroy(child.gameObject);
                }
            }
        }

        public void Activate(char targetLetter)
        {
            TargetLetter = char.ToUpperInvariant(targetLetter);
            IsActiveTarget = true;
            ApplyVisualState(true);
        }

        public void Deactivate()
        {
            IsActiveTarget = false;
            ApplyVisualState(false);
        }

        public Vector3 GetSnapWorldPoint()
        {
            return transform.position;
        }

        public Vector3 GetDefaultPinnedTipWorldPoint()
        {
            Vector2 outward = GetOutwardDirection();
            float edgeDepth = GetInnerEdgeDepth(1f);
            float bandCenterDepth = edgeDepth - (hitBandDepth * 0.45f);
            return transform.position + (Vector3)(outward * bandCenterDepth);
        }

        public Vector3 GetOutwardWorldDirection()
        {
            return GetOutwardDirection();
        }

        public Vector2 GetPlaqueLocalPoint(Vector3 worldPoint)
        {
            Vector3 local = transform.InverseTransformPoint(worldPoint);
            return new Vector2(local.x, local.y);
        }

        public bool IsInsidePerfectZone(Vector3 worldPoint, float scale = 1f)
        {
            Vector2 radialPoint = GetRadialHitPoint(worldPoint);
            if (!IsInsideHitBand(radialPoint, scale))
            {
                return false;
            }

            return Mathf.Abs(radialPoint.x) <= (perfectZoneSize.x * scale) * 0.5f;
        }

        public bool IsInsideMagnetZone(Vector3 worldPoint, float scale = 1f)
        {
            Vector2 radialPoint = GetRadialHitPoint(worldPoint);
            if (!IsInsideHitBand(radialPoint, scale))
            {
                return false;
            }

            return Mathf.Abs(radialPoint.x) <= (plaqueSize.x * scale) * 0.5f;
        }

        public bool IsInsideNearMissZone(Vector3 worldPoint, float scale = 1f)
        {
            Vector2 radialPoint = GetRadialHitPoint(worldPoint);
            float halfWidth = ((plaqueSize.x * scale) * 0.5f) + nearMissPadding;
            float edgeDepth = GetInnerEdgeDepth(scale);
            float minDepth = edgeDepth - (hitBandDepth * scale) - nearMissPadding;
            float maxDepth = edgeDepth + nearMissPadding;
            return Mathf.Abs(radialPoint.x) <= halfWidth && radialPoint.y >= minDepth && radialPoint.y <= maxDepth;
        }

        private void ApplyVisualState(bool active)
        {
            if (_baseScale == Vector3.zero)
            {
                _baseScale = transform.localScale;
            }

            if (_feedbackEndsAt > Time.time)
            {
                transform.localScale = _baseScale * _feedbackScaleMultiplier;

                if (bodyRenderer != null)
                {
                    bodyRenderer.color = _feedbackColor;
                }

                if (glowRenderer != null)
                {
                    glowRenderer.enabled = true;
                    glowRenderer.color = _feedbackColor;
                }

                ApplyAnchorVisualVisibility(active);

                return;
            }

            transform.localScale = active ? _baseScale * activeScaleMultiplier : _baseScale;

            if (bodyRenderer != null)
            {
                bodyRenderer.color = active ? activeColor : inactiveColor;
            }

            if (glowRenderer != null)
            {
                glowRenderer.enabled = active;
                glowRenderer.color = activeColor;
            }

            ApplyAnchorVisualVisibility(active);
        }

        private void HandleHitEvaluated(HitData hit)
        {
            if (hit.slotIndex != slotIndex && hit.expectedSlotIndex != slotIndex)
            {
                return;
            }

            switch (hit.resultType)
            {
                case HitResultType.Perfect:
                    StartFeedback(perfectFeedbackColor, 1.18f);
                    break;
                case HitResultType.Tolerated:
                    StartFeedback(toleratedFeedbackColor, 1.12f);
                    break;
                case HitResultType.NearMiss:
                case HitResultType.Miss:
                case HitResultType.WrongLetter:
                case HitResultType.WrongSlot:
                    StartFeedback(failFeedbackColor, 1.10f);
                    break;
            }
        }

        private void HandleQuestionReset()
        {
            _feedbackEndsAt = 0f;
            ApplyVisualState(IsActiveTarget);
        }

        private void HandleLevelCompleted(LevelContext context)
        {
            _feedbackEndsAt = 0f;
            ApplyVisualState(false);
        }

        private void StartFeedback(Color color, float scaleMultiplier)
        {
            _feedbackColor = color;
            _feedbackScaleMultiplier = scaleMultiplier;
            _feedbackEndsAt = Time.time + feedbackDuration;
            ApplyVisualState(IsActiveTarget);
        }

        private void ResolveAnchorVisuals()
        {
            _anchorVisualRenderers = new[]
            {
                FindRenderer("PlaqueShadow"),
                FindRenderer("PlaqueRing"),
                FindRenderer("PlaqueCore"),
                FindRenderer("RuneGlyph"),
                glowRenderer,
                bodyRenderer
            };
        }

        private void ApplyAnchorVisualVisibility(bool active)
        {
            if (!anchorOnlyMode)
            {
                return;
            }

            if (_anchorVisualRenderers == null || _anchorVisualRenderers.Length == 0)
            {
                ResolveAnchorVisuals();
            }

            for (int i = 0; i < _anchorVisualRenderers.Length; i++)
            {
                SpriteRenderer renderer = _anchorVisualRenderers[i];
                if (renderer == null)
                {
                    continue;
                }

                renderer.enabled = false;
            }

            if (glowRenderer != null)
            {
                glowRenderer.enabled = false;
            }
        }

        private SpriteRenderer FindRenderer(string childName)
        {
            Transform child = transform.Find(childName);
            return child != null ? child.GetComponent<SpriteRenderer>() : null;
        }

        private Vector2 GetRadialHitPoint(Vector3 worldPoint)
        {
            Vector2 offset = worldPoint - transform.position;
            Vector2 outward = GetOutwardDirection();

            Vector2 tangent = new Vector2(-outward.y, outward.x);
            float lateral = Vector2.Dot(offset, tangent);
            float depth = Vector2.Dot(offset, outward);
            return new Vector2(lateral, depth);
        }

        private bool IsInsideHitBand(Vector2 radialPoint, float scale)
        {
            float halfWidth = (plaqueSize.x * scale) * 0.5f;
            float edgeDepth = GetInnerEdgeDepth(scale);
            float minDepth = edgeDepth - (hitBandDepth * scale);
            float maxDepth = edgeDepth + 0.025f;
            return Mathf.Abs(radialPoint.x) <= halfWidth && radialPoint.y >= minDepth && radialPoint.y <= maxDepth;
        }

        private float GetInnerEdgeDepth(float scale)
        {
            return ((plaqueSize.y * scale) * 0.5f) - hitBandInset;
        }

        private Vector2 GetOutwardDirection()
        {
            Transform rotator = transform.parent != null ? transform.parent.parent : null;
            Vector2 outward = rotator != null
                ? ((Vector2)transform.position - (Vector2)rotator.position).normalized
                : (Vector2)transform.up;

            if (outward.sqrMagnitude < 0.0001f)
            {
                outward = transform.up;
            }

            return outward;
        }
    }
}
