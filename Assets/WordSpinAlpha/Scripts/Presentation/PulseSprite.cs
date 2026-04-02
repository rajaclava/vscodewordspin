using UnityEngine;

namespace WordSpinAlpha.Presentation
{
    public class PulseSprite : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private float speed = 1.6f;
        [SerializeField] private float scaleAmplitude = 0.06f;
        [SerializeField] private float alphaAmplitude = 0.12f;

        private Vector3 _baseScale;
        private Color _baseColor;
        private bool _initialized;

        private void Awake()
        {
            EnsureInitialized();
        }

        public void SetBaseColor(Color color)
        {
            EnsureInitialized();
            _baseColor = color;
            if (target != null)
            {
                target.color = color;
            }
        }

        public void SetPulse(float pulseSpeed, float pulseScaleAmplitude, float pulseAlphaAmplitude)
        {
            EnsureInitialized();
            speed = Mathf.Max(0.05f, pulseSpeed);
            scaleAmplitude = Mathf.Max(0f, pulseScaleAmplitude);
            alphaAmplitude = Mathf.Max(0f, pulseAlphaAmplitude);
        }

        private void Update()
        {
            EnsureInitialized();
            float wave = (Mathf.Sin(Time.time * speed) + 1f) * 0.5f;
            float scaleMultiplier = 1f + (wave * scaleAmplitude);
            scaleMultiplier = Mathf.Clamp(scaleMultiplier, 0.5f, 2f);
            transform.localScale = _baseScale * scaleMultiplier;

            if (target != null)
            {
                Color color = _baseColor;
                color.a = Mathf.Clamp01(_baseColor.a + (wave - 0.5f) * alphaAmplitude);
                target.color = color;
            }
        }

        private void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            if (target == null)
            {
                target = GetComponent<SpriteRenderer>();
            }

            _baseScale = transform.localScale;
            if (_baseScale == Vector3.zero)
            {
                _baseScale = Vector3.one;
            }

            if (target != null)
            {
                _baseColor = target.color;
            }

            _initialized = true;
        }
    }
}
