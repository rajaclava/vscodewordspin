using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    [DisallowMultipleComponent]
    public sealed class MainMenuPreviewPressEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerClickHandler
    {
        [SerializeField] private RectTransform[] scaleTargets;
        [SerializeField] private Graphic[] brightnessTargets;
        [SerializeField] private float pressedScale = 0.92f;
        [SerializeField] private float releaseScale = 1.05f;
        [SerializeField] private float normalScale = 1f;
        [SerializeField] private float pressDuration = 0.07f;
        [SerializeField] private float releaseDuration = 0.10f;
        [SerializeField] private float settleDuration = 0.12f;
        [SerializeField] private float pressedBrightness = 0.82f;
        [SerializeField] private float releaseBrightness = 1.18f;
        [SerializeField] private float mainMenuNavigationDelay = 0.035f;
        [SerializeField] private float mainMenuPressFeedbackDuration = 0.045f;
        [SerializeField] private float mainMenuReleaseFeedbackDuration = 0.12f;

        private Vector3[] baseScales;
        private Color[] baseColors;
        private Coroutine effectRoutine;
        private bool cacheReady;
        private bool immediateNavigationTriggered;

        public void Configure(RectTransform[] targetTransforms, Graphic[] targetGraphics)
        {
            scaleTargets = targetTransforms;
            brightnessTargets = targetGraphics;
            cacheReady = false;
            CacheBaseState();
        }

        private void Awake()
        {
            CacheBaseState();
        }

        private void OnEnable()
        {
            CacheBaseState();
            PrepareInstantMainMenuPlayButton();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (TryTriggerMainMenuImmediateNavigation())
            {
                return;
            }

            PlayPress();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (immediateNavigationTriggered)
            {
                return;
            }

            PlayRelease();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (immediateNavigationTriggered)
            {
                return;
            }

            PlayRelease();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // Preview-only visual feedback. Real navigation is not bound here.
        }

        private bool TryTriggerMainMenuImmediateNavigation()
        {
            if (immediateNavigationTriggered || !Application.isPlaying)
            {
                return false;
            }

            if (!IsRuntimeMainMenuPlayButton())
            {
                return false;
            }

            MainMenuPresenter presenter = GetComponentInParent<MainMenuPresenter>();
            if (presenter == null)
            {
                return false;
            }

            immediateNavigationTriggered = true;
            StopEffectRoutine();

            Image image = GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = false;
            }

            Button button = GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }

            effectRoutine = StartCoroutine(PlayMainMenuPressAndNavigate(presenter));
            return true;
        }

        private IEnumerator PlayMainMenuPressAndNavigate(MainMenuPresenter presenter)
        {
            float pressFeedback = Mathf.Clamp(mainMenuPressFeedbackDuration, 0.025f, 0.07f);
            float releaseFeedback = Mathf.Clamp(mainMenuReleaseFeedbackDuration, 0.06f, 0.16f);
            float navigationDelay = Mathf.Clamp(mainMenuNavigationDelay, 0.01f, releaseFeedback);

            yield return AnimateScaleAndBrightness(pressedScale, pressedBrightness, pressFeedback);

            StartCoroutine(AnimateScaleAndBrightness(normalScale, 1f, releaseFeedback));
            float elapsed = 0f;
            while (elapsed < navigationDelay)
            {
                elapsed += Time.unscaledDeltaTime;
                yield return null;
            }

            if (presenter != null)
            {
                presenter.StartCurrentProgressLevel();
            }

            effectRoutine = null;
        }

        private void PrepareInstantMainMenuPlayButton()
        {
            if (!Application.isPlaying || !IsRuntimeMainMenuPlayButton())
            {
                return;
            }

            Button button = GetComponent<Button>();
            if (button == null)
            {
                return;
            }

            button.transition = Selectable.Transition.None;
            button.onClick.RemoveAllListeners();
        }

        private bool IsRuntimeMainMenuPlayButton()
        {
            return string.Equals(SceneManager.GetActiveScene().name, GameConstants.SceneMainMenu, System.StringComparison.Ordinal) &&
                   string.Equals(gameObject.name, "PlayButton_Hitbox", System.StringComparison.Ordinal);
        }

        private void PlayPress()
        {
            StopEffectRoutine();
            effectRoutine = StartCoroutine(AnimateScaleAndBrightness(pressedScale, pressedBrightness, pressDuration));
        }

        private void PlayRelease()
        {
            StopEffectRoutine();
            effectRoutine = StartCoroutine(ReleaseBounce());
        }

        private IEnumerator ReleaseBounce()
        {
            yield return AnimateScaleAndBrightness(releaseScale, releaseBrightness, releaseDuration);
            yield return AnimateScaleAndBrightness(normalScale, 1f, settleDuration);
            effectRoutine = null;
        }

        private IEnumerator AnimateScaleAndBrightness(float targetScale, float targetBrightness, float duration)
        {
            CacheBaseState();

            Vector3[] startScales = CaptureScales();
            Color[] startColors = CaptureColors();
            float elapsed = 0f;
            float safeDuration = Mathf.Max(0.001f, duration);

            while (elapsed < safeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / safeDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f);
                ApplyState(startScales, startColors, targetScale, targetBrightness, eased);
                yield return null;
            }

            ApplyState(startScales, startColors, targetScale, targetBrightness, 1f);
        }

        private void ApplyState(Vector3[] startScales, Color[] startColors, float targetScale, float targetBrightness, float t)
        {
            if (scaleTargets != null)
            {
                for (int i = 0; i < scaleTargets.Length; i++)
                {
                    RectTransform target = scaleTargets[i];
                    if (target == null || baseScales == null || i >= baseScales.Length || startScales == null || i >= startScales.Length)
                    {
                        continue;
                    }

                    target.localScale = Vector3.Lerp(startScales[i], baseScales[i] * targetScale, t);
                }
            }

            if (brightnessTargets != null)
            {
                for (int i = 0; i < brightnessTargets.Length; i++)
                {
                    Graphic target = brightnessTargets[i];
                    if (target == null || baseColors == null || i >= baseColors.Length || startColors == null || i >= startColors.Length)
                    {
                        continue;
                    }

                    Color destination = MultiplyRgb(baseColors[i], targetBrightness);
                    target.color = Color.Lerp(startColors[i], destination, t);
                }
            }
        }

        private Vector3[] CaptureScales()
        {
            if (scaleTargets == null)
            {
                return null;
            }

            Vector3[] values = new Vector3[scaleTargets.Length];
            for (int i = 0; i < scaleTargets.Length; i++)
            {
                values[i] = scaleTargets[i] != null ? scaleTargets[i].localScale : Vector3.one;
            }

            return values;
        }

        private Color[] CaptureColors()
        {
            if (brightnessTargets == null)
            {
                return null;
            }

            Color[] values = new Color[brightnessTargets.Length];
            for (int i = 0; i < brightnessTargets.Length; i++)
            {
                values[i] = brightnessTargets[i] != null ? brightnessTargets[i].color : Color.white;
            }

            return values;
        }

        private void CacheBaseState()
        {
            if (cacheReady)
            {
                return;
            }

            if (scaleTargets == null || scaleTargets.Length == 0)
            {
                scaleTargets = new[] { transform as RectTransform };
            }

            baseScales = new Vector3[scaleTargets.Length];
            for (int i = 0; i < scaleTargets.Length; i++)
            {
                baseScales[i] = scaleTargets[i] != null ? scaleTargets[i].localScale : Vector3.one;
            }

            if (brightnessTargets != null)
            {
                baseColors = new Color[brightnessTargets.Length];
                for (int i = 0; i < brightnessTargets.Length; i++)
                {
                    baseColors[i] = brightnessTargets[i] != null ? brightnessTargets[i].color : Color.white;
                }
            }

            cacheReady = true;
        }

        private static Color MultiplyRgb(Color color, float multiplier)
        {
            return new Color(
                Mathf.Clamp01(color.r * multiplier),
                Mathf.Clamp01(color.g * multiplier),
                Mathf.Clamp01(color.b * multiplier),
                color.a);
        }

        private void StopEffectRoutine()
        {
            if (effectRoutine == null)
            {
                return;
            }

            StopCoroutine(effectRoutine);
            effectRoutine = null;
        }

        private void RestoreBaseState()
        {
            if (!cacheReady)
            {
                return;
            }

            if (scaleTargets != null && baseScales != null)
            {
                for (int i = 0; i < scaleTargets.Length && i < baseScales.Length; i++)
                {
                    if (scaleTargets[i] != null)
                    {
                        scaleTargets[i].localScale = baseScales[i];
                    }
                }
            }

            if (brightnessTargets != null && baseColors != null)
            {
                for (int i = 0; i < brightnessTargets.Length && i < baseColors.Length; i++)
                {
                    if (brightnessTargets[i] != null)
                    {
                        brightnessTargets[i].color = baseColors[i];
                    }
                }
            }
        }

        private void OnDisable()
        {
            StopEffectRoutine();
            RestoreBaseState();
        }
    }
}
