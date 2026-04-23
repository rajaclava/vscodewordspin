using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace WordSpinAlpha.Presentation
{
    /// <summary>
    /// Bir UI butonuna basıldığında scale + parlaklık animasyonu uygular.
    /// DOTween gerektirmez; saf Unity Coroutine ile çalışır.
    /// PlayButton gibi Image-only nesnelere eklenebilir.
    /// </summary>
    [DisallowMultipleComponent]
    public class ButtonPressEffect : MonoBehaviour,
        IPointerDownHandler,
        IPointerUpHandler,
        IPointerClickHandler
    {
        [Header("Scale Ayarları")]
        [SerializeField] private float pressedScale   = 0.88f;   // Basılı tutulunca ulaşılan ölçek
        [SerializeField] private float releaseScale   = 1.05f;   // Bırakınca geçici zıplama ölçeği
        [SerializeField] private float normalScale    = 1.00f;   // Dinlenme ölçeği
        [SerializeField] private float pressSpeed     = 14f;     // Basma hızı (lerp faktörü)
        [SerializeField] private float releaseSpeed   = 10f;     // Bırakma zıplama hızı
        [SerializeField] private float settleSpeed    = 6f;      // Dinlenmeye dönme hızı

        [Header("Parlaklık Ayarları")]
        [SerializeField] private bool  useBrightness  = true;
        [SerializeField] private float pressedBrightness  = 0.72f;  // Basılıyken renk koyulaşma
        [SerializeField] private float releaseBrightness  = 1.18f;  // Bırakınca parlaklık patlaması
        [SerializeField] private float brightnessSpeed     = 10f;

        // ----------------------------------------------------------------
        private Vector3   _baseScale;
        private float     _targetScale;
        private float     _currentScale;

        private UnityEngine.UI.Graphic _graphic;
        private float     _targetBrightness;
        private float     _currentBrightness;
        private bool      _pressing;

        // Coroutine referansları (çakışmayı önlemek için)
        private Coroutine _scaleCoroutine;
        private Coroutine _releaseBounceCoroutine;

        // ----------------------------------------------------------------
        private void Awake()
        {
            _baseScale         = transform.localScale;
            _targetScale       = 1f;
            _currentScale      = 1f;
            _targetBrightness  = 1f;
            _currentBrightness = 1f;

            _graphic = GetComponent<UnityEngine.UI.Graphic>();
        }

        // ----------------------------------------------------------------
        public void OnPointerDown(PointerEventData eventData)
        {
            _pressing = true;
            StopAllEffectCoroutines();
            _scaleCoroutine = StartCoroutine(LerpScaleTo(pressedScale, pressSpeed, false));
            if (useBrightness)
                StartCoroutine(LerpBrightnessTo(pressedBrightness, brightnessSpeed));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            _pressing = false;
            StopAllEffectCoroutines();
            _releaseBounceCoroutine = StartCoroutine(ReleaseBounce());
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Animasyonu tetiklemek için OnPointerUp yeterli.
            // Bu metod Unity EventSystem gerekliliği için boş kalabilir.
        }

        // ----------------------------------------------------------------
        private IEnumerator LerpScaleTo(float target, float speed, bool isSettle)
        {
            float start = _currentScale;
            float elapsed = 0f;

            while (true)
            {
                elapsed += Time.unscaledDeltaTime * speed;
                _currentScale = Mathf.Lerp(start, target, elapsed);
                ApplyScale(_currentScale);

                if (elapsed >= 1f) break;
                yield return null;
            }

            _currentScale = target;
            ApplyScale(_currentScale);
        }

        private IEnumerator ReleaseBounce()
        {
            // 1. Hızla zıplama ölçeğine ulaş
            yield return StartCoroutine(LerpScaleTo(releaseScale, releaseSpeed, false));

            // 2. Eş zamanlı parlama
            if (useBrightness)
                StartCoroutine(LerpBrightnessTo(releaseBrightness, brightnessSpeed));

            // 3. Kısa bekleme
            yield return new WaitForSecondsRealtime(0.06f);

            // 4. Normal ölçeğe yerleş
            yield return StartCoroutine(LerpScaleTo(normalScale, settleSpeed, true));

            // 5. Parlaklığı normale döndür
            if (useBrightness)
                StartCoroutine(LerpBrightnessTo(1f, brightnessSpeed * 0.6f));
        }

        private IEnumerator LerpBrightnessTo(float targetB, float speed)
        {
            float start   = _currentBrightness;
            float elapsed = 0f;

            while (true)
            {
                elapsed += Time.unscaledDeltaTime * speed;
                _currentBrightness = Mathf.Lerp(start, targetB, elapsed);
                ApplyBrightness(_currentBrightness);

                if (elapsed >= 1f) break;
                yield return null;
            }

            _currentBrightness = targetB;
            ApplyBrightness(_currentBrightness);
        }

        // ----------------------------------------------------------------
        private void ApplyScale(float t)
        {
            transform.localScale = _baseScale * t;
        }

        private void ApplyBrightness(float b)
        {
            if (_graphic == null) return;
            Color c = _graphic.color;
            // Orijinal rengi koru, sadece RGB parlaklığını ölçekle
            _graphic.color = new Color(
                Mathf.Clamp01(c.r * b / Mathf.Max(_currentBrightness, 0.001f)),
                Mathf.Clamp01(c.g * b / Mathf.Max(_currentBrightness, 0.001f)),
                Mathf.Clamp01(c.b * b / Mathf.Max(_currentBrightness, 0.001f)),
                c.a);
        }

        private void StopAllEffectCoroutines()
        {
            if (_scaleCoroutine != null)
            {
                StopCoroutine(_scaleCoroutine);
                _scaleCoroutine = null;
            }
            if (_releaseBounceCoroutine != null)
            {
                StopCoroutine(_releaseBounceCoroutine);
                _releaseBounceCoroutine = null;
            }
        }

        private void OnDisable()
        {
            // Nesne devre dışıyken scale'i sıfırla
            _pressing = false;
            StopAllEffectCoroutines();
            transform.localScale = _baseScale;
            if (_graphic != null && useBrightness)
            {
                ApplyBrightness(1f);
            }
        }
    }
}
