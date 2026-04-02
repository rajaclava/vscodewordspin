using UnityEngine;
using UnityEngine.UI;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    [ExecuteAlways]
    public class GameplaySceneTuner : MonoBehaviour
    {
        [Header("Scene References")]
        [SerializeField] private KeyboardLayoutTuningProfile keyboardLayoutProfile;
        [SerializeField] private Camera gameplayCamera;
        [SerializeField] private Transform rotatorRoot;
        [SerializeField] private Transform rotatorVisualRoot;
        [SerializeField] private TargetRotator targetRotator;
        [SerializeField] private Transform[] slotAnchors;
        [SerializeField] private Transform launcherBody;
        [SerializeField] private Transform pinSpawnPoint;
        [SerializeField] private SpriteRenderer flightLane;
        [SerializeField] private PinLauncher pinLauncher;
        [SerializeField] private RectTransform topBar;
        [SerializeField] private RectTransform questionPanel;
        [SerializeField] private RectTransform bottomBar;
        [SerializeField] private RectTransform keyboardSkinFrame;
        [SerializeField] private RectTransform keyboardGrid;
        [SerializeField] private RectTransform menuButton;
        [SerializeField] private RectTransform storeButton;
        [SerializeField] private RectTransform swipeHint;
        [SerializeField] private GridLayoutGroup keyboardGridLayout;
        [SerializeField] private SpriteRenderer leftAura;
        [SerializeField] private SpriteRenderer rightAura;
        [SerializeField] private SpriteRenderer leftPillar;
        [SerializeField] private SpriteRenderer rightPillar;

        [Header("Camera")]
        [SerializeField] private float cameraSize = 5.4f;
        [SerializeField] private float referencePortraitAspect = 0.5625f;

        [Header("Rotator")]
        [SerializeField] private Vector2 rotatorPosition = new Vector2(0f, 1.08f);
        [SerializeField] private float rotatorScale = 1f;
        [SerializeField] private float anchorRadius = 1.02f;
        [SerializeField] private float rotatorSpeed = 45f;
        [SerializeField] private bool clockwise = true;

        [Header("Launcher")]
        [SerializeField] private Vector2 launcherPosition = new Vector2(0f, -2.85f);
        [SerializeField] private Vector2 launcherScale = new Vector2(1.8f, 0.40f);
        [SerializeField] private Vector2 pinSpawnLocalOffset = new Vector2(0f, 0.38f);
        [SerializeField] private float pinScale = 1.18f;
        [SerializeField] private float pinLoadTweenDuration = 0.30f;

        [Header("Flight Lane")]
        [SerializeField] private Vector2 flightLanePosition = new Vector2(0f, -0.55f);
        [SerializeField] private Vector2 flightLaneScale = new Vector2(0.08f, 5.0f);

        [Header("UI Layout")]
        [SerializeField] private Vector2 topBarAnchoredPosition = Vector2.zero;
        [SerializeField] private Vector2 topBarSize = new Vector2(980f, 80f);
        [SerializeField] private Vector2 questionPanelAnchoredPosition = new Vector2(0f, 132f);
        [SerializeField] private Vector2 questionPanelSize = new Vector2(900f, 220f);
        [SerializeField] private Vector2 bottomBarAnchoredPosition = new Vector2(0f, -18f);
        [SerializeField] private Vector2 bottomBarSize = new Vector2(1080f, 362f);
        [SerializeField] private Vector2 keyboardSkinFrameAnchoredPosition = new Vector2(0f, -8f);
        [SerializeField] private Vector2 keyboardSkinFrameSize = new Vector2(1000f, 320f);
        [SerializeField] private Vector2 keyboardGridAnchoredPosition = new Vector2(0f, -2f);
        [SerializeField] private Vector2 keyboardGridSize = new Vector2(964f, 292f);
        [SerializeField] private Vector2 keyboardCellSize = new Vector2(72f, 82f);
        [SerializeField] private Vector2 keyboardSpacing = new Vector2(2f, 6f);
        [SerializeField] private Vector2 menuButtonAnchors = new Vector2(0.14f, 1f);
        [SerializeField] private Vector2 storeButtonAnchors = new Vector2(0.86f, 1f);
        [SerializeField] private Vector2 navButtonSize = new Vector2(156f, 52f);
        [SerializeField] private float navButtonTopOffset = 34f;
        [SerializeField] private Vector2 swipeHintAnchors = new Vector2(0.5f, 1.24f);
        [SerializeField] private bool hideSideDecor = true;
        [SerializeField] private bool useMobileLayoutPreset = true;

        [SerializeField] private bool autoApplyInEditMode = true;
        private KeyboardLayoutTuningProfile _loadedKeyboardLayoutProfile;

        private void OnEnable()
        {
            ResolveReferences();
            if (Application.isPlaying)
            {
                GameEvents.LanguageChanged += HandleLanguageChanged;
            }
            if (Application.isPlaying)
            {
                ApplyTuning();
            }
            else
            {
                ApplyTuning();
            }
        }

        private void Start()
        {
            if (Application.isPlaying)
            {
                ApplyTuning();
            }
        }

        private void OnDisable()
        {
            if (Application.isPlaying)
            {
                GameEvents.LanguageChanged -= HandleLanguageChanged;
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
            if (!autoApplyInEditMode || Application.isPlaying)
            {
                return;
            }

            ApplyTuning();
        }

        public void ApplyTuning()
        {
            ResolveReferences();
            ApplyMobilePreset();
            ApplyKeyboardProfileOverride();

            if (gameplayCamera != null)
            {
                float resolvedCameraSize = Mathf.Max(1f, cameraSize);
                float currentAspect = (float)Screen.width / Mathf.Max(1f, Screen.height);
                if (currentAspect > 0f && currentAspect < referencePortraitAspect)
                {
                    resolvedCameraSize *= referencePortraitAspect / currentAspect;
                }

                gameplayCamera.orthographicSize = resolvedCameraSize;
            }

            if (rotatorRoot != null)
            {
                rotatorRoot.position = new Vector3(rotatorPosition.x, rotatorPosition.y, rotatorRoot.position.z);
            }

            if (rotatorVisualRoot != null)
            {
                rotatorVisualRoot.localScale = Vector3.one * Mathf.Max(0.1f, rotatorScale);
            }

            if (slotAnchors != null && slotAnchors.Length > 0)
            {
                for (int i = 0; i < slotAnchors.Length; i++)
                {
                    Transform anchor = slotAnchors[i];
                    if (anchor == null)
                    {
                        continue;
                    }

                    float angle = i * Mathf.PI * 2f / slotAnchors.Length;
                    anchor.localPosition = new Vector3(Mathf.Sin(angle) * anchorRadius, Mathf.Cos(angle) * anchorRadius, 0f);
                    anchor.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Rad2Deg * angle);
                }
            }

            if (targetRotator != null)
            {
                targetRotator.ApplyLevelSettings(rotatorSpeed, clockwise);
            }

            if (launcherBody != null)
            {
                launcherBody.position = new Vector3(launcherPosition.x, launcherPosition.y, launcherBody.position.z);
                launcherBody.localScale = new Vector3(launcherScale.x, launcherScale.y, 1f);
            }

            if (pinSpawnPoint != null)
            {
                pinSpawnPoint.localPosition = new Vector3(pinSpawnLocalOffset.x, pinSpawnLocalOffset.y, pinSpawnPoint.localPosition.z);
            }

            if (pinLauncher != null)
            {
                pinLauncher.ApplyTuning(pinLoadTweenDuration, pinScale);
            }

            if (flightLane != null)
            {
                flightLane.transform.position = new Vector3(flightLanePosition.x, flightLanePosition.y, flightLane.transform.position.z);
                flightLane.transform.localScale = new Vector3(flightLaneScale.x, flightLaneScale.y, 1f);
            }

            ApplyRect(topBar, topBarAnchoredPosition, topBarSize);
            ApplyRect(questionPanel, questionPanelAnchoredPosition, questionPanelSize);
            ApplyRect(bottomBar, bottomBarAnchoredPosition, bottomBarSize);
            ApplyRect(keyboardSkinFrame, keyboardSkinFrameAnchoredPosition, keyboardSkinFrameSize);
            ApplyRect(keyboardGrid, keyboardGridAnchoredPosition, keyboardGridSize);
            ApplyBottomButton(menuButton, bottomBar, menuButtonAnchors, navButtonSize, navButtonTopOffset);
            ApplyBottomButton(storeButton, bottomBar, storeButtonAnchors, navButtonSize, navButtonTopOffset);
            ApplyAnchoredElement(swipeHint, bottomBar, swipeHintAnchors);

            if (keyboardGridLayout != null)
            {
                keyboardGridLayout.cellSize = keyboardCellSize;
                keyboardGridLayout.spacing = keyboardSpacing;
            }

            if (hideSideDecor)
            {
                SetRendererEnabled(leftAura, false);
                SetRendererEnabled(rightAura, false);
                SetRendererEnabled(leftPillar, false);
                SetRendererEnabled(rightPillar, false);
            }
        }

        public void CaptureFromScene()
        {
            ResolveReferences();

            if (gameplayCamera != null)
            {
                cameraSize = gameplayCamera.orthographicSize;
            }

            if (rotatorRoot != null)
            {
                rotatorPosition = new Vector2(rotatorRoot.position.x, rotatorRoot.position.y);
            }

            if (rotatorVisualRoot != null)
            {
                rotatorScale = rotatorVisualRoot.localScale.x;
            }

            if (slotAnchors != null && slotAnchors.Length > 0)
            {
                anchorRadius = slotAnchors[0] != null ? slotAnchors[0].localPosition.magnitude : anchorRadius;
            }

            if (targetRotator != null)
            {
                rotatorSpeed = targetRotator.RotationSpeed;
                clockwise = targetRotator.Clockwise;
            }

            if (launcherBody != null)
            {
                launcherPosition = new Vector2(launcherBody.position.x, launcherBody.position.y);
                launcherScale = new Vector2(launcherBody.localScale.x, launcherBody.localScale.y);
            }

            if (pinSpawnPoint != null)
            {
                pinSpawnLocalOffset = new Vector2(pinSpawnPoint.localPosition.x, pinSpawnPoint.localPosition.y);
            }

            if (pinLauncher != null)
            {
                pinLoadTweenDuration = pinLauncher.LoadTweenDuration;
                pinScale = pinLauncher.PinVisualScale;
            }

            if (flightLane != null)
            {
                flightLanePosition = new Vector2(flightLane.transform.position.x, flightLane.transform.position.y);
                flightLaneScale = new Vector2(flightLane.transform.localScale.x, flightLane.transform.localScale.y);
            }

            CaptureRect(topBar, ref topBarAnchoredPosition, ref topBarSize);
            CaptureRect(questionPanel, ref questionPanelAnchoredPosition, ref questionPanelSize);
            CaptureRect(bottomBar, ref bottomBarAnchoredPosition, ref bottomBarSize);
            CaptureRect(keyboardSkinFrame, ref keyboardSkinFrameAnchoredPosition, ref keyboardSkinFrameSize);
            CaptureRect(keyboardGrid, ref keyboardGridAnchoredPosition, ref keyboardGridSize);
            if (menuButton != null)
            {
                navButtonSize = menuButton.sizeDelta;
            }

            if (keyboardGridLayout != null)
            {
                keyboardCellSize = keyboardGridLayout.cellSize;
                keyboardSpacing = keyboardGridLayout.spacing;
            }
        }

        public void ResetToDefaults()
        {
            cameraSize = 5.4f;
            rotatorPosition = new Vector2(0f, 1.08f);
            rotatorScale = 1f;
            anchorRadius = 1.02f;
            rotatorSpeed = 45f;
            clockwise = true;
            launcherPosition = new Vector2(0f, -2.85f);
            launcherScale = new Vector2(1.8f, 0.40f);
            pinSpawnLocalOffset = new Vector2(0f, 0.38f);
            pinScale = 1.18f;
            pinLoadTweenDuration = 0.30f;
            flightLanePosition = new Vector2(0f, -0.55f);
            flightLaneScale = new Vector2(0.08f, 5.0f);
            topBarAnchoredPosition = Vector2.zero;
            topBarSize = new Vector2(980f, 80f);
            questionPanelAnchoredPosition = new Vector2(0f, 132f);
            questionPanelSize = new Vector2(900f, 220f);
            bottomBarAnchoredPosition = new Vector2(0f, -18f);
            bottomBarSize = new Vector2(1080f, 362f);
            keyboardSkinFrameAnchoredPosition = new Vector2(0f, -8f);
            keyboardSkinFrameSize = new Vector2(1000f, 320f);
            keyboardGridAnchoredPosition = new Vector2(0f, -2f);
            keyboardGridSize = new Vector2(964f, 292f);
            keyboardCellSize = new Vector2(72f, 82f);
            keyboardSpacing = new Vector2(2f, 6f);
            menuButtonAnchors = new Vector2(0.14f, 1f);
            storeButtonAnchors = new Vector2(0.86f, 1f);
            navButtonSize = new Vector2(156f, 52f);
            navButtonTopOffset = 34f;
            swipeHintAnchors = new Vector2(0.5f, 1.24f);
            hideSideDecor = true;
            ApplyTuning();
        }

        private void ApplyMobilePreset()
        {
            if (!useMobileLayoutPreset)
            {
                return;
            }

            rotatorPosition = new Vector2(0f, 1.16f);
            questionPanelAnchoredPosition = new Vector2(0f, 146f);
            bottomBarAnchoredPosition = new Vector2(0f, -18f);
            bottomBarSize = new Vector2(1080f, 362f);
            keyboardSkinFrameAnchoredPosition = new Vector2(0f, -8f);
            keyboardSkinFrameSize = new Vector2(1000f, 320f);
            keyboardGridAnchoredPosition = new Vector2(0f, -2f);
            keyboardGridSize = new Vector2(964f, 292f);
            keyboardCellSize = new Vector2(72f, 82f);
            keyboardSpacing = new Vector2(2f, 6f);
            menuButtonAnchors = new Vector2(0.16f, 1f);
            storeButtonAnchors = new Vector2(0.84f, 1f);
            navButtonSize = new Vector2(156f, 52f);
            navButtonTopOffset = 34f;
            swipeHintAnchors = new Vector2(0.5f, 1.24f);
            hideSideDecor = true;
        }

        private void ApplyKeyboardProfileOverride()
        {
            KeyboardLayoutTuningProfile profile = ResolveKeyboardLayoutProfile();
            if (profile == null)
            {
                return;
            }

            KeyboardLayoutTuningProfile.LanguageTuning tuning = profile.GetLanguageTuning(ResolveLanguageCode());
            if (tuning == null)
            {
                return;
            }

            bottomBarAnchoredPosition = tuning.bottomBarAnchoredPosition;
            bottomBarSize = tuning.bottomBarSize;
            keyboardSkinFrameAnchoredPosition = tuning.keyboardSkinFrameAnchoredPosition;
            keyboardSkinFrameSize = tuning.keyboardSkinFrameSize;
            keyboardGridAnchoredPosition = tuning.keyboardGridAnchoredPosition;
            keyboardGridSize = tuning.keyboardGridSize;
            keyboardCellSize = tuning.keyboardCellSize;
            keyboardSpacing = tuning.keyboardSpacing;
            menuButtonAnchors = tuning.menuButtonAnchors;
            storeButtonAnchors = tuning.storeButtonAnchors;
            navButtonSize = tuning.navButtonSize;
            navButtonTopOffset = tuning.navButtonTopOffset;
            swipeHintAnchors = tuning.swipeHintAnchors;
        }

        private KeyboardLayoutTuningProfile ResolveKeyboardLayoutProfile()
        {
            if (keyboardLayoutProfile != null)
            {
                return keyboardLayoutProfile;
            }

            if (_loadedKeyboardLayoutProfile == null)
            {
                _loadedKeyboardLayoutProfile = Resources.Load<KeyboardLayoutTuningProfile>(KeyboardLayoutTuningProfile.DefaultResourcePath);
            }

            return _loadedKeyboardLayoutProfile;
        }

        private static string ResolveLanguageCode()
        {
            return SaveManager.Instance != null
                ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode)
                : GameConstants.DefaultLanguageCode;
        }

        private void HandleLanguageChanged(string _)
        {
            ApplyTuning();
        }

        private void ResolveReferences()
        {
            if (rotatorRoot != null && rotatorVisualRoot == null)
            {
                Transform child = rotatorRoot.Find("RotatorVisual");
                if (child != null)
                {
                    rotatorVisualRoot = child;
                }
            }

            if (keyboardGrid != null && keyboardGridLayout == null)
            {
                keyboardGridLayout = keyboardGrid.GetComponent<GridLayoutGroup>();
            }

            if (keyboardSkinFrame == null)
            {
                GameObject found = GameObject.Find("KeyboardSkinFrame");
                if (found != null)
                {
                    keyboardSkinFrame = found.GetComponent<RectTransform>();
                }
            }

            if (menuButton == null)
            {
                GameObject found = GameObject.Find("MenuOpen");
                if (found != null)
                {
                    menuButton = found.GetComponent<RectTransform>();
                }
            }

            if (storeButton == null)
            {
                GameObject found = GameObject.Find("StoreOpen");
                if (found != null)
                {
                    storeButton = found.GetComponent<RectTransform>();
                }
            }

            if (swipeHint == null)
            {
                GameObject found = GameObject.Find("SwipeHint");
                if (found != null)
                {
                    swipeHint = found.GetComponent<RectTransform>();
                }
            }

            if (leftAura == null)
            {
                leftAura = FindSpriteRenderer("LeftAura");
            }

            if (rightAura == null)
            {
                rightAura = FindSpriteRenderer("RightAura");
            }

            if (leftPillar == null)
            {
                leftPillar = FindSpriteRenderer("LeftPillar");
            }

            if (rightPillar == null)
            {
                rightPillar = FindSpriteRenderer("RightPillar");
            }

            if (launcherBody != null && pinSpawnPoint == null)
            {
                Transform child = launcherBody.Find("PinSpawnPoint");
                if (child != null)
                {
                    pinSpawnPoint = child;
                }
            }
        }

        private static void ApplyRect(RectTransform rect, Vector2 anchoredPosition, Vector2 size)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        private static void CaptureRect(RectTransform rect, ref Vector2 anchoredPosition, ref Vector2 size)
        {
            if (rect == null)
            {
                return;
            }

            anchoredPosition = rect.anchoredPosition;
            size = rect.sizeDelta;
        }

        private static void ApplyBottomButton(RectTransform buttonRect, RectTransform parent, Vector2 anchors, Vector2 size, float topOffset)
        {
            if (buttonRect == null || parent == null)
            {
                return;
            }

            if (buttonRect.parent != parent)
            {
                buttonRect.SetParent(parent, false);
            }

            buttonRect.anchorMin = anchors;
            buttonRect.anchorMax = anchors;
            buttonRect.anchoredPosition = new Vector2(0f, topOffset);
            buttonRect.sizeDelta = size;
            buttonRect.localScale = Vector3.one;
        }

        private static void ApplyAnchoredElement(RectTransform rect, RectTransform parent, Vector2 anchors)
        {
            if (rect == null || parent == null)
            {
                return;
            }

            if (rect.parent != parent)
            {
                rect.SetParent(parent, false);
            }

            rect.anchorMin = anchors;
            rect.anchorMax = anchors;
            rect.anchoredPosition = Vector2.zero;
            rect.localScale = Vector3.one;
        }

        private static void SetRendererEnabled(SpriteRenderer renderer, bool enabled)
        {
            if (renderer != null)
            {
                renderer.enabled = enabled;
            }
        }

        private static SpriteRenderer FindSpriteRenderer(string name)
        {
            GameObject found = GameObject.Find(name);
            return found != null ? found.GetComponent<SpriteRenderer>() : null;
        }
    }
}
