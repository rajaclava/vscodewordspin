using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class PinBreakFragment : MonoBehaviour
    {
        private Vector3 _velocity;
        private float _angularVelocity;
        private float _lifetime;
        private float _elapsed;
        private SpriteRenderer _spriteRenderer;
        private TextMesh _textMesh;
        private Color _baseColor;
        private Vector3 _baseScale = Vector3.one;

        public void Initialize(Vector3 velocity, float angularVelocity, float lifetime, float scaleBoost = 1f)
        {
            _velocity = velocity;
            _angularVelocity = angularVelocity;
            _lifetime = Mathf.Max(0.16f, lifetime);
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _textMesh = GetComponent<TextMesh>();
            _baseScale = transform.localScale * Mathf.Max(1f, scaleBoost);
            transform.localScale = _baseScale;

            if (_spriteRenderer != null)
            {
                _baseColor = _spriteRenderer.color;
            }
            else if (_textMesh != null)
            {
                _baseColor = _textMesh.color;
            }
        }

        private void Update()
        {
            _elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(_elapsed / _lifetime);

            transform.position += _velocity * Time.deltaTime;
            _velocity += Vector3.down * (1.8f * Time.deltaTime);
            transform.Rotate(0f, 0f, _angularVelocity * Time.deltaTime);
            transform.localScale = Vector3.Lerp(_baseScale, _baseScale * 0.82f, t);

            Color faded = _baseColor;
            faded.a = Mathf.Lerp(_baseColor.a, 0f, t);

            if (_spriteRenderer != null)
            {
                _spriteRenderer.color = faded;
            }

            if (_textMesh != null)
            {
                _textMesh.color = faded;
            }

            if (_elapsed >= _lifetime)
            {
                Destroy(gameObject);
            }
        }
    }
}
