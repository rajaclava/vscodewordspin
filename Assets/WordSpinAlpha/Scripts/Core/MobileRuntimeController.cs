using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WordSpinAlpha.Core
{
    [DefaultExecutionOrder(-1000)]
    public class MobileRuntimeController : MonoBehaviour
    {
        [SerializeField] private float extraHorizontalSafeMargin = 18f;
        [SerializeField] private float extraBottomSafeMargin = 12f;
        [SerializeField] private float extraTopSafeMargin = 8f;

        private readonly System.Collections.Generic.Dictionary<int, Vector2> _baseAnchoredPositions = new System.Collections.Generic.Dictionary<int, Vector2>();
        private readonly System.Collections.Generic.Dictionary<int, Vector2> _baseSizes = new System.Collections.Generic.Dictionary<int, Vector2>();
        private Rect _lastSafeArea;
        private Vector2Int _lastScreenSize;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            ApplyGlobalMobileSettings();
            ApplyCurrentSceneLayout();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }

        private void LateUpdate()
        {
            Rect safeArea = Screen.safeArea;
            Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
            if (safeArea != _lastSafeArea || screenSize != _lastScreenSize)
            {
                ApplyCurrentSceneLayout();
            }
        }

        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            _baseAnchoredPositions.Clear();
            _baseSizes.Clear();
            ApplyCurrentSceneLayout();
        }

        private void ApplyCurrentSceneLayout()
        {
            ApplyGlobalMobileSettings();
            _lastSafeArea = Screen.safeArea;
            _lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas == null || !canvas.isRootCanvas)
                {
                    continue;
                }

                ApplyCanvasScaling(canvas);
                ApplySafeAreaOffsets(canvas);
            }
        }

        private static void ApplyGlobalMobileSettings()
        {
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToPortrait = true;
            Screen.autorotateToPortraitUpsideDown = false;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
            Screen.fullScreen = true;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        private static void ApplyCanvasScaling(Canvas canvas)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler == null)
            {
                return;
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 1f;
        }

        private void ApplySafeAreaOffsets(Canvas canvas)
        {
            RectTransform canvasRect = canvas.transform as RectTransform;
            if (canvasRect == null || Screen.width <= 0 || Screen.height <= 0)
            {
                return;
            }

            Rect safeArea = Screen.safeArea;
            float scaleX = canvasRect.rect.width / Screen.width;
            float scaleY = canvasRect.rect.height / Screen.height;
            float leftInset = safeArea.xMin * scaleX + extraHorizontalSafeMargin;
            float rightInset = (Screen.width - safeArea.xMax) * scaleX + extraHorizontalSafeMargin;
            float topInset = (Screen.height - safeArea.yMax) * scaleY + extraTopSafeMargin;
            float bottomInset = safeArea.yMin * scaleY + extraBottomSafeMargin;

            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                RectTransform child = canvas.transform.GetChild(i) as RectTransform;
                if (child == null)
                {
                    continue;
                }

                int key = child.GetInstanceID();
                if (!_baseAnchoredPositions.ContainsKey(key))
                {
                    _baseAnchoredPositions[key] = child.anchoredPosition;
                }
                if (!_baseSizes.ContainsKey(key))
                {
                    _baseSizes[key] = child.sizeDelta;
                }

                Vector2 anchoredPosition = _baseAnchoredPositions[key];
                Vector2 sizeDelta = _baseSizes[key];
                Vector2 min = child.anchorMin;
                Vector2 max = child.anchorMax;

                if (min == Vector2.zero && max == Vector2.one)
                {
                    continue;
                }

                if (min.y >= 0.72f && max.y >= 0.72f)
                {
                    anchoredPosition.y -= topInset;
                }
                else if (min.y <= 0.24f && max.y <= 0.24f)
                {
                    anchoredPosition.y += bottomInset;
                }

                if (min.x <= 0.18f && max.x <= 0.18f)
                {
                    anchoredPosition.x += leftInset;
                }
                else if (min.x >= 0.82f && max.x >= 0.82f)
                {
                    anchoredPosition.x -= rightInset;
                }

                bool isHorizontallyFixed = Mathf.Abs(max.x - min.x) < 0.001f;
                if (isHorizontallyFixed && sizeDelta.x > 0f)
                {
                    float horizontalInset = leftInset + rightInset;
                    sizeDelta.x = Mathf.Max(0f, _baseSizes[key].x - horizontalInset);
                }

                bool isVerticallyBottomDocked = min.y <= 0.24f && max.y <= 0.24f;
                if (isVerticallyBottomDocked && sizeDelta.y > 0f)
                {
                    sizeDelta.y = Mathf.Max(0f, _baseSizes[key].y - bottomInset);
                }

                child.anchoredPosition = anchoredPosition;
                child.sizeDelta = sizeDelta;
            }
        }
    }
}
