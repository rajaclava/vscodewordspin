using UnityEngine;
using UnityEngine.UI;

namespace WordSpinAlpha.Presentation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class HubPreviewTabEditStrip : MonoBehaviour
    {
        private const string BackgroundObjectName = "Background";
        private const string PreviewSizeReferenceName = "Reference_Hidden";
        private const string EditPreviewBackgroundName = "EditPreviewBackground";
        private const string PreviewChromeSuffix = "_PreviewChrome";

        [SerializeField] private RectTransform previewSizeReference;
        [SerializeField] private Image sourceBackground;
        [SerializeField, Min(0f)] private float previewGap = 180f;

        private LevelHubPreviewController controller;

        private void OnEnable()
        {
            ApplyLayout();
        }

        private void OnValidate()
        {
            ApplyLayout();
        }

        private void Update()
        {
            if (Application.isPlaying)
            {
                return;
            }

            ApplyLayout();
        }

        private void ApplyLayout()
        {
            if (!gameObject.scene.IsValid())
            {
                return;
            }

            controller ??= GetComponent<LevelHubPreviewController>();
            RectTransform missionsRoot = ResolvePageRoot(controller != null ? controller.missionsPlaceholder : null);
            RectTransform profileRoot = ResolvePageRoot(controller != null ? controller.profilePlaceholder : null);
            RectTransform storeRoot = ResolvePageRoot(controller != null ? controller.storePlaceholder : null);

            Vector2 previewSize = ResolvePreviewSize();
            float previewStep = previewSize.x + previewGap;
            bool isEditPreview = !Application.isPlaying;
            Sprite backgroundSprite = ResolveBackgroundSprite();

            ApplyPagePreview(missionsRoot, isEditPreview, backgroundSprite, -1f * previewStep, previewSize);
            ApplyPagePreview(profileRoot, isEditPreview, backgroundSprite, -2f * previewStep, previewSize);
            ApplyPagePreview(storeRoot, isEditPreview, backgroundSprite, 1f * previewStep, previewSize);
        }

        private void ApplyPagePreview(RectTransform pageRoot, bool isEditPreview, Sprite backgroundSprite, float anchoredX, Vector2 previewSize)
        {
            if (pageRoot == null)
            {
                return;
            }

            CleanupLegacyChildPreviewBackground(pageRoot);

            if (isEditPreview && !pageRoot.gameObject.activeSelf)
            {
                pageRoot.gameObject.SetActive(true);
            }

            pageRoot.anchorMin = new Vector2(0.5f, 0.5f);
            pageRoot.anchorMax = new Vector2(0.5f, 0.5f);
            pageRoot.pivot = new Vector2(0.5f, 0.5f);
            pageRoot.sizeDelta = previewSize;
            pageRoot.anchoredPosition = isEditPreview
                ? new Vector2(anchoredX, 0f)
                : Vector2.zero;

            Image previewBackground = GetOrCreatePreviewBackgroundChrome(pageRoot, backgroundSprite, anchoredX, previewSize);
            if (previewBackground != null)
            {
                previewBackground.enabled = isEditPreview && backgroundSprite != null;
                previewBackground.raycastTarget = false;
            }
        }

        private void CleanupLegacyChildPreviewBackground(RectTransform pageRoot)
        {
            if (pageRoot == null)
            {
                return;
            }

            Transform existing = pageRoot.Find(EditPreviewBackgroundName);
            if (existing == null)
            {
                return;
            }

            Image legacyImage = existing.GetComponent<Image>();
            if (legacyImage != null)
            {
                legacyImage.raycastTarget = false;
                legacyImage.enabled = false;
            }

            if (existing.gameObject.activeSelf)
            {
                existing.gameObject.SetActive(false);
            }
        }

        private Image GetOrCreatePreviewBackgroundChrome(RectTransform pageRoot, Sprite backgroundSprite, float anchoredX, Vector2 previewSize)
        {
            if (pageRoot == null)
            {
                return null;
            }

            string chromeName = pageRoot.name + PreviewChromeSuffix;
            Transform existing = transform.Find(chromeName);
            Image image = existing != null ? existing.GetComponent<Image>() : null;
            if (image == null)
            {
                GameObject backgroundObject = new GameObject(chromeName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                RectTransform rect = backgroundObject.GetComponent<RectTransform>();
                rect.SetParent(transform, false);
                image = backgroundObject.GetComponent<Image>();
                image.raycastTarget = false;
            }

            RectTransform chromeRect = image.rectTransform;
            chromeRect.anchorMin = new Vector2(0.5f, 0.5f);
            chromeRect.anchorMax = new Vector2(0.5f, 0.5f);
            chromeRect.pivot = new Vector2(0.5f, 0.5f);
            chromeRect.sizeDelta = previewSize;
            chromeRect.anchoredPosition = new Vector2(anchoredX, 0f);

            image.sprite = backgroundSprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = false;

            int rootIndex = pageRoot.GetSiblingIndex();
            image.transform.SetSiblingIndex(Mathf.Max(0, rootIndex));
            pageRoot.SetSiblingIndex(Mathf.Min(transform.childCount - 1, image.transform.GetSiblingIndex() + 1));
            return image;
        }

        private Vector2 ResolvePreviewSize()
        {
            if (previewSizeReference == null)
            {
                previewSizeReference = FindDirectChildRect(PreviewSizeReferenceName);
            }

            if (previewSizeReference != null)
            {
                Rect rect = previewSizeReference.rect;
                if (rect.width > 0f && rect.height > 0f)
                {
                    return rect.size;
                }
            }

            return new Vector2(864f, 1536f);
        }

        private Sprite ResolveBackgroundSprite()
        {
            if (sourceBackground == null)
            {
                RectTransform backgroundRect = FindDirectChildRect(BackgroundObjectName);
                if (backgroundRect != null)
                {
                    sourceBackground = backgroundRect.GetComponent<Image>();
                }
            }

            return sourceBackground != null ? sourceBackground.sprite : null;
        }

        private RectTransform ResolvePageRoot(GameObject rootObject)
        {
            return rootObject != null ? rootObject.GetComponent<RectTransform>() : null;
        }

        private RectTransform FindDirectChildRect(string childName)
        {
            if (string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            Transform directChild = transform.Find(childName);
            return directChild as RectTransform;
        }
    }
}
