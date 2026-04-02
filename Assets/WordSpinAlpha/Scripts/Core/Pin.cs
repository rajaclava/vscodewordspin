using UnityEngine;
using WordSpinAlpha.Content;

namespace WordSpinAlpha.Core
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Pin : MonoBehaviour
    {
        [SerializeField] private float speed = 9.5f;
        [SerializeField] private float tipOffset = 0.18f;
        [SerializeField] private float maxTravelDistance = 7.5f;
        [SerializeField] private TextMesh letterLabel;
        [SerializeField] private SpriteRenderer ringRenderer;
        [SerializeField] private SpriteRenderer coreRenderer;
        [SerializeField] private SpriteRenderer sheenRenderer;
        [SerializeField] private SpriteRenderer shaftRenderer;

        private Rigidbody2D _rigidbody;
        private Collider2D _collider;
        private SlotManager _slotManager;
        private Vector3 _direction = Vector3.up;
        private Vector3 _launchOrigin;
        private bool _isLoaded;
        private bool _isLaunched;
        private bool _isStuck;
        private Transform _stickTarget;
        private Vector3 _stickLocalPosition;
        private float _stickLocalRotationZ;
        private Vector3 _previousTipWorldPosition;

        public char CarryingLetter { get; private set; }
        public bool IsLoaded => _isLoaded;
        public float TravelSpeed => speed;
        public Vector3 FlightDirection => _direction;
        public Vector3 TipWorldPosition => transform.position + transform.up * tipOffset;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
            _collider = GetComponent<Collider2D>();
            _rigidbody.gravityScale = 0f;
            _rigidbody.bodyType = RigidbodyType2D.Kinematic;
            _slotManager = FindObjectOfType<SlotManager>();
            ResolveVisualReferences();
            EnsureRuntimeVisualStructure();
            tipOffset = Mathf.Max(0.40f, tipOffset);
        }

        private void Update()
        {
            if (!_isLaunched || _isStuck)
            {
                return;
            }

            Vector3 segmentStart = _previousTipWorldPosition;
            transform.position += _direction * (speed * Time.deltaTime);
            Vector3 segmentEnd = TipWorldPosition;
            _previousTipWorldPosition = segmentEnd;

            if (TryResolveImpactAlongPath(segmentStart, segmentEnd))
            {
                return;
            }

            if (Vector3.Distance(_launchOrigin, transform.position) >= maxTravelDistance)
            {
                _isLaunched = false;
                GameManager.Instance?.HandlePinFlightMiss(this);
            }
        }

        private void LateUpdate()
        {
            if (_isStuck && _stickTarget != null)
            {
                transform.position = _stickTarget.TransformPoint(_stickLocalPosition);
                transform.rotation = Quaternion.Euler(0f, 0f, _stickTarget.eulerAngles.z + _stickLocalRotationZ);
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!_isLaunched || _isStuck)
            {
                return;
            }

            Slot slot = other.GetComponent<Slot>();
            if (slot == null)
            {
                return;
            }

            if (GameManager.Instance != null)
            {
                HitData preview = GameManager.Instance.PreviewPinHit(this, slot, TipWorldPosition);
                if (preview.resultType == HitResultType.NearMiss)
                {
                    return;
                }
            }

            _isLaunched = false;
            GameManager.Instance?.ResolvePinHit(this, slot, TipWorldPosition);
        }

        public void Load(char letter)
        {
            CarryingLetter = char.ToUpperInvariant(letter);
            _isLoaded = true;
            if (letterLabel != null)
            {
                letterLabel.text = CarryingLetter.ToString();
            }
        }

        public bool Fire(Vector3 direction)
        {
            if (!_isLoaded || _isLaunched)
            {
                return false;
            }

            _direction = direction.normalized;
            _launchOrigin = transform.position;
            _isLaunched = true;
            transform.rotation = Quaternion.Euler(0f, 0f, DirectionToRotationZ(_direction));
            _previousTipWorldPosition = TipWorldPosition;
            return true;
        }

        public void StickTipTo(Transform target, Vector3 tipWorldPoint)
        {
            Vector3 tipDirection = _direction.sqrMagnitude > 0.0001f ? _direction.normalized : transform.up;
            StickTipTo(target, tipWorldPoint, tipDirection);
        }

        public void StickTipTo(Transform target, Vector3 tipWorldPoint, Vector3 tipDirection)
        {
            _isStuck = true;
            _isLaunched = false;
            _collider.enabled = false;
            _rigidbody.simulated = false;
            _stickTarget = target;
            _direction = tipDirection.sqrMagnitude > 0.0001f ? tipDirection.normalized : Vector3.up;
            Vector3 rootWorldPosition = tipWorldPoint - (tipDirection * tipOffset);
            float worldRotationZ = DirectionToRotationZ(tipDirection);
            transform.SetParent(target, true);
            _stickLocalPosition = target.InverseTransformPoint(rootWorldPosition);
            _stickLocalRotationZ = worldRotationZ - target.eulerAngles.z;
            transform.position = rootWorldPosition;
            transform.rotation = Quaternion.Euler(0f, 0f, worldRotationZ);
        }

        public void RestoreStuckPose(Transform target, Vector3 localPosition, float localRotationZ)
        {
            _isStuck = true;
            _isLaunched = false;
            _collider.enabled = false;
            _rigidbody.simulated = false;
            _stickTarget = target;
            _stickLocalPosition = localPosition;
            _stickLocalRotationZ = localRotationZ;
            transform.SetParent(target, false);
            transform.localPosition = localPosition;
            transform.localRotation = Quaternion.Euler(0f, 0f, localRotationZ);
        }

        public void PlayWrongSlotBreak(Vector3 impactPoint)
        {
            SpawnSpriteFragment("BreakRing", ringRenderer, impactPoint, new Vector3(-0.55f, 0.95f, 0f), -220f, 0.28f);
            SpawnSpriteFragment("BreakCore", coreRenderer, impactPoint, new Vector3(0f, 1.10f, 0f), 180f, 0.26f);
            SpawnSpriteFragment("BreakShaft", shaftRenderer, impactPoint, new Vector3(0.42f, 0.72f, 0f), 260f, 0.30f);
            SpawnTextFragment("BreakLetter", impactPoint, new Vector3(-0.24f, 0.82f, 0f), -140f, 0.32f);
            Destroy(gameObject);
        }

        public void ApplyThemeSkin(ThemePackDefinition theme)
        {
            if (theme == null)
            {
                return;
            }

            ResolveVisualReferences();

            if (ColorUtility.TryParseHtmlString(theme.uiPrimaryHex, out Color ringColor) && ringRenderer != null)
            {
                ringRenderer.color = ringColor;
            }

            if (ColorUtility.TryParseHtmlString(theme.uiAccentHex, out Color accentColor))
            {
                if (coreRenderer != null)
                {
                    coreRenderer.color = accentColor;
                }

                if (sheenRenderer != null)
                {
                    Color sheenColor = Color.Lerp(accentColor, Color.white, 0.6f);
                    sheenColor.a = 0.55f;
                    sheenRenderer.color = sheenColor;
                }
            }

            if (!string.IsNullOrWhiteSpace(theme.pinResourcePath))
            {
                Sprite loaded = Resources.Load<Sprite>(theme.pinResourcePath);
                if (loaded != null && coreRenderer != null)
                {
                    coreRenderer.sprite = loaded;
                }
            }
        }

        private void ResolveVisualReferences()
        {
            if (ringRenderer == null)
            {
                Transform child = transform.Find("OuterRing");
                if (child != null)
                {
                    ringRenderer = child.GetComponent<SpriteRenderer>();
                }
            }

            if (coreRenderer == null)
            {
                Transform child = transform.Find("Core");
                if (child != null)
                {
                    coreRenderer = child.GetComponent<SpriteRenderer>();
                }
            }

            if (sheenRenderer == null)
            {
                Transform child = transform.Find("Sheen");
                if (child != null)
                {
                    sheenRenderer = child.GetComponent<SpriteRenderer>();
                }
            }

            if (shaftRenderer == null)
            {
                Transform child = transform.Find("Shaft");
                if (child != null)
                {
                    shaftRenderer = child.GetComponent<SpriteRenderer>();
                }
            }
        }

        private static float DirectionToRotationZ(Vector3 direction)
        {
            Vector3 normalized = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector3.up;
            return Mathf.Atan2(normalized.y, normalized.x) * Mathf.Rad2Deg - 90f;
        }

        private void EnsureRuntimeVisualStructure()
        {
            if (shaftRenderer == null)
            {
                GameObject shaft = new GameObject("Shaft");
                shaft.transform.SetParent(transform, false);
                shaft.transform.localPosition = new Vector3(0f, -0.38f, 0f);
                shaft.transform.localRotation = Quaternion.identity;
                shaft.transform.localScale = new Vector3(0.11f, 0.74f, 1f);
                shaftRenderer = shaft.AddComponent<SpriteRenderer>();
                shaftRenderer.sortingOrder = 9;
            }

            if (shaftRenderer != null)
            {
                shaftRenderer.sprite = coreRenderer != null ? coreRenderer.sprite : null;
                shaftRenderer.color = ringRenderer != null ? ringRenderer.color : new Color(0.24f, 0.18f, 0.12f, 0.96f);
                shaftRenderer.transform.localPosition = new Vector3(0f, -0.38f, 0f);
                shaftRenderer.transform.localScale = new Vector3(0.11f, 0.74f, 1f);
            }

            if (letterLabel != null)
            {
                letterLabel.transform.localPosition = new Vector3(0f, -0.80f, -0.1f);
                letterLabel.characterSize = 0.10f;
                letterLabel.fontSize = 52;
            }

            if (sheenRenderer != null)
            {
                sheenRenderer.transform.localPosition = new Vector3(-0.02f, 0.04f, 0f);
            }

            if (ringRenderer != null)
            {
                ringRenderer.transform.localScale = new Vector3(0.28f, 0.28f, 1f);
            }

            if (coreRenderer != null)
            {
                coreRenderer.transform.localScale = new Vector3(0.20f, 0.20f, 1f);
            }

            if (sheenRenderer != null)
            {
                sheenRenderer.transform.localScale = new Vector3(0.08f, 0.08f, 1f);
            }
        }

        private bool TryResolveImpactAlongPath(Vector3 segmentStart, Vector3 segmentEnd)
        {
            if (_slotManager == null)
            {
                return false;
            }

            float distance = Vector3.Distance(segmentStart, segmentEnd);
            int samples = Mathf.Clamp(Mathf.CeilToInt(distance / 0.05f), 1, 12);
            bool sawNearMiss = false;

            for (int i = 1; i <= samples; i++)
            {
                Vector3 samplePoint = Vector3.Lerp(segmentStart, segmentEnd, i / (float)samples);
                if (!_slotManager.TryFindPlaqueCandidate(samplePoint, out Slot plaqueSlot))
                {
                    continue;
                }

                HitData preview = GameManager.Instance != null
                    ? GameManager.Instance.PreviewPinHit(this, plaqueSlot, samplePoint)
                    : new HitData { resultType = HitResultType.Miss };

                if (preview.resultType == HitResultType.NearMiss)
                {
                    sawNearMiss = true;
                    continue;
                }

                _isLaunched = false;
                GameManager.Instance?.ResolvePinHit(this, plaqueSlot, samplePoint);
                return true;
            }

            return sawNearMiss;
        }

        private void SpawnSpriteFragment(string name, SpriteRenderer source, Vector3 origin, Vector3 velocity, float angularVelocity, float lifetime)
        {
            if (source == null || source.sprite == null)
            {
                return;
            }

            GameObject fragment = new GameObject(name);
            fragment.transform.position = source.bounds.center;
            fragment.transform.rotation = source.transform.rotation;
            fragment.transform.localScale = source.transform.lossyScale;
            SpriteRenderer renderer = fragment.AddComponent<SpriteRenderer>();
            renderer.sprite = source.sprite;
            renderer.color = source.color;
            renderer.sortingOrder = source.sortingOrder + 2;
            PinBreakFragment breakFragment = fragment.AddComponent<PinBreakFragment>();
            Vector3 launch = (source.bounds.center - origin).normalized;
            if (launch.sqrMagnitude < 0.001f)
            {
                launch = velocity.normalized;
            }

            breakFragment.Initialize((launch * 1.10f) + velocity, angularVelocity, lifetime + 0.16f, 1.45f);
        }

        private void SpawnTextFragment(string name, Vector3 origin, Vector3 velocity, float angularVelocity, float lifetime)
        {
            if (letterLabel == null)
            {
                return;
            }

            GameObject fragment = new GameObject(name, typeof(TextMesh));
            fragment.transform.position = letterLabel.transform.position;
            fragment.transform.rotation = letterLabel.transform.rotation;
            fragment.transform.localScale = letterLabel.transform.lossyScale;
            TextMesh text = fragment.GetComponent<TextMesh>();
            text.text = CarryingLetter.ToString();
            text.characterSize = letterLabel.characterSize;
            text.fontSize = letterLabel.fontSize;
            text.anchor = letterLabel.anchor;
            text.alignment = letterLabel.alignment;
            text.font = letterLabel.font;
            text.color = letterLabel.color;
            MeshRenderer meshRenderer = fragment.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 20;
            }

            PinBreakFragment breakFragment = fragment.AddComponent<PinBreakFragment>();
            Vector3 launch = (fragment.transform.position - origin).normalized;
            if (launch.sqrMagnitude < 0.001f)
            {
                launch = velocity.normalized;
            }

            breakFragment.Initialize((launch * 0.90f) + velocity, angularVelocity, lifetime + 0.18f, 1.35f);
        }
    }
}
