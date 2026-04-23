using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace WordSpinAlpha.Editor
{
    [CreateAssetMenu(fileName = "HubPreviewLayoutTuningProfile", menuName = "WordSpin Alpha/Hub Preview Layout Tuning Profile")]
    public sealed class HubPreviewLayoutTuningProfile : ScriptableObject
    {
        public const string DefaultAssetPath = "Assets/WordSpinAlpha/Generated/EditorData/HubPreviewLayoutTuningProfile.asset";
        public const string AlttasElementId = "Alttas";
        public const string MainMenuRotatorElementId = "MainMenuRotator";
        public const string BottomPageNavElementId = "HubPreviewBottomPageNav";
        public const string HubPreviewOynaButtonElementId = "HubPreviewOynaButton";
        private static readonly Vector2 BottomPageNavAnchoredPosition = new Vector2(0f, -450f);
        private static readonly Vector2 BottomPageNavMaxSize = new Vector2(710f, 143f);
        private static readonly Vector2 HubPreviewOynaButtonAnchoredPosition = new Vector2(0f, -605f);
        private static readonly Vector2 HubPreviewOynaButtonSize = new Vector2(540f, 145f);
        private const string BottomPageNavSpritePath = "Assets/WordSpinAlpha/Art/UI/LevelHubPreview/Source/levelsecimhub_sayfanavigasyonbar_boscerceve(1).png";
        private static readonly Vector2 HubPreviewCanvasReferenceSize = new Vector2(864f, 1536f);
        private const string MainMenuRotatorSpritePath = "Assets/WordSpinAlpha/Art/UI/MainMenu/Cropped/rotator_crop.png";

        [Serializable]
        public class LayoutElementTuningValue
        {
            public Vector2 anchoredPosition = new Vector2(0f, -355f);
            public float width = 490f;
            public float height = 420f;
            public bool preserveAspect = true;

            public void CopyFrom(LayoutElementTuningValue source)
            {
                if (source == null)
                {
                    return;
                }

                anchoredPosition = source.anchoredPosition;
                width = source.width;
                height = source.height;
                preserveAspect = source.preserveAspect;
            }
        }

        [Serializable]
        public sealed class LayoutElementTuning : LayoutElementTuningValue
        {
            public string elementId = AlttasElementId;
            [SerializeField, HideInInspector] private LayoutElementTuningValue baselineValue = new LayoutElementTuningValue();
            [SerializeField, HideInInspector] private bool hasBaselineValue;

            public LayoutElementTuningValue CurrentValue => this;

            public LayoutElementTuningValue BaselineValue
            {
                get
                {
                    if (baselineValue == null)
                    {
                        baselineValue = new LayoutElementTuningValue();
                    }

                    return baselineValue;
                }
            }

            public bool HasBaselineValue => hasBaselineValue;

            public bool TryCopyBaselineToCurrent()
            {
                if (!hasBaselineValue || baselineValue == null)
                {
                    return false;
                }

                CopyFrom(baselineValue);
                return true;
            }

            public void CopyCurrentToBaseline()
            {
                BaselineValue.CopyFrom(this);
                hasBaselineValue = true;
            }

            public Vector2 SizeDelta
            {
                get { return new Vector2(width, height); }
                set
                {
                    width = Mathf.Max(1f, value.x);
                    height = Mathf.Max(1f, value.y);
                }
            }

            private bool IsAlttas()
            {
                return NormalizeElementId(elementId) == NormalizeElementId(AlttasElementId);
            }

            private bool IsMainMenuRotator()
            {
                return NormalizeElementId(elementId) == NormalizeElementId(MainMenuRotatorElementId);
            }

            private bool IsBottomPageNav()
            {
                return NormalizeElementId(elementId) == NormalizeElementId(BottomPageNavElementId);
            }

            public Vector2 GetSafeSizeDelta(Image image = null)
            {
                float safeWidth = Mathf.Max(1f, width);
                if (IsMainMenuRotator())
                {
                    float aspectRatio = GetMainMenuRotatorAspectRatio(image);
                    float normalizedHeight = Mathf.Max(1f, Mathf.Round(safeWidth / aspectRatio));
                    return new Vector2(safeWidth, normalizedHeight);
                }

                float defaultHeight = Mathf.Max(1f, height);
                return new Vector2(safeWidth, defaultHeight);
            }

            public Vector2 GetSafeAnchoredPosition(Image image = null)
            {
                Vector2 safeSizeDelta = GetSafeSizeDelta(image);
                return GetSafeAnchoredPosition(safeSizeDelta);
            }

            public Vector2 GetSafeAnchoredPosition(Vector2 safeSizeDelta)
            {
                if (IsAlttas())
                {
                    return ClampAnchoredPositionToHubPreviewBounds(anchoredPosition, safeSizeDelta);
                }

                return anchoredPosition;
            }

            public bool GetSafePreserveAspect()
            {
                return IsMainMenuRotator() || IsBottomPageNav() ? true : preserveAspect;
            }

            public void ApplyTo(RectTransform rectTransform, Image image = null)
            {
                if (rectTransform == null)
                {
                    return;
                }

                Vector2 safeSizeDelta = GetSafeSizeDelta(image);
                rectTransform.anchoredPosition = GetSafeAnchoredPosition(safeSizeDelta);
                rectTransform.sizeDelta = safeSizeDelta;

                if (image != null)
                {
                    image.preserveAspect = GetSafePreserveAspect();
                }
            }

            public void CaptureFrom(RectTransform rectTransform, Image image = null)
            {
                if (rectTransform == null)
                {
                    return;
                }

                anchoredPosition = rectTransform.anchoredPosition;
                width = Mathf.Max(1f, rectTransform.sizeDelta.x);
                height = Mathf.Max(1f, rectTransform.sizeDelta.y);

                if (image != null)
                {
                    preserveAspect = image.preserveAspect;
                }

                NormalizeInPlace(image);
            }

            public bool NormalizeInPlace(Image image = null)
            {
                bool changed = false;

                float safeWidth = Mathf.Max(1f, width);
                if (!Mathf.Approximately(width, safeWidth))
                {
                    width = safeWidth;
                    changed = true;
                }

                float safeHeight = Mathf.Max(1f, height);
                if (!Mathf.Approximately(height, safeHeight))
                {
                    height = safeHeight;
                    changed = true;
                }

                if (IsMainMenuRotator())
                {
                    float aspectRatio = GetMainMenuRotatorAspectRatio(image);
                    float normalizedHeight = Mathf.Max(1f, Mathf.Round(safeWidth / aspectRatio));
                    if (!Mathf.Approximately(height, normalizedHeight))
                    {
                        height = normalizedHeight;
                        changed = true;
                    }

                    if (!preserveAspect)
                    {
                        preserveAspect = true;
                        changed = true;
                    }
                }

                if (IsBottomPageNav() && !preserveAspect)
                {
                    preserveAspect = true;
                    changed = true;
                }

                if (IsAlttas())
                {
                    Vector2 clampedPosition = ClampAnchoredPositionToHubPreviewBounds(anchoredPosition, new Vector2(width, height));
                    if ((clampedPosition - anchoredPosition).sqrMagnitude > 0.0001f)
                    {
                        anchoredPosition = clampedPosition;
                        changed = true;
                    }
                }

                if (!hasBaselineValue)
                {
                    BaselineValue.CopyFrom(this);
                    hasBaselineValue = true;
                    changed = true;
                }

                return changed;
            }

            private static float GetMainMenuRotatorAspectRatio(Image image = null)
            {
                if (image != null && image.sprite != null && image.sprite.rect.height > 0.001f)
                {
                    return Mathf.Max(0.0001f, image.sprite.rect.width / image.sprite.rect.height);
                }

                Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(MainMenuRotatorSpritePath);
                if (sprite != null && sprite.rect.height > 0.001f)
                {
                    return Mathf.Max(0.0001f, sprite.rect.width / sprite.rect.height);
                }

                return 4f / 3f;
            }

            private static Vector2 ClampAnchoredPositionToHubPreviewBounds(Vector2 anchoredPosition, Vector2 sizeDelta)
            {
                float safeHalfWidth = Mathf.Max(0f, HubPreviewCanvasReferenceSize.x * 0.5f - Mathf.Max(1f, sizeDelta.x) * 0.5f);
                float safeHalfHeight = Mathf.Max(0f, HubPreviewCanvasReferenceSize.y * 0.5f - Mathf.Max(1f, sizeDelta.y) * 0.5f);

                return new Vector2(
                    Mathf.Clamp(anchoredPosition.x, -safeHalfWidth, safeHalfWidth),
                    Mathf.Clamp(anchoredPosition.y, -safeHalfHeight, safeHalfHeight));
            }
        }

        [SerializeField] private List<LayoutElementTuning> layoutElements = new List<LayoutElementTuning>();

        public IReadOnlyList<LayoutElementTuning> LayoutElements => layoutElements;

        public static HubPreviewLayoutTuningProfile Load()
        {
            return AssetDatabase.LoadAssetAtPath<HubPreviewLayoutTuningProfile>(DefaultAssetPath);
        }

        public static LayoutElementTuning Resolve(string elementId)
        {
            HubPreviewLayoutTuningProfile profile = Load();
            if (profile != null)
            {
                LayoutElementTuning tuning = profile.GetElement(elementId);
                if (tuning != null)
                {
                    return tuning;
                }
            }

            return CreateDefaultElement(elementId);
        }

        public static LayoutElementTuning ResolveAlttas()
        {
            return Resolve(AlttasElementId);
        }

        public static LayoutElementTuning ResolveMainMenuRotator()
        {
            return Resolve(MainMenuRotatorElementId);
        }

        public static LayoutElementTuning ResolveBottomPageNav()
        {
            return Resolve(BottomPageNavElementId);
        }

        public static LayoutElementTuning ResolveOynaButton()
        {
            return Resolve(HubPreviewOynaButtonElementId);
        }

        public LayoutElementTuning GetElement(string elementId)
        {
            if (layoutElements == null)
            {
                return null;
            }

            string normalizedElementId = NormalizeElementId(elementId);
            for (int i = 0; i < layoutElements.Count; i++)
            {
                LayoutElementTuning element = layoutElements[i];
                if (element != null && NormalizeElementId(element.elementId) == normalizedElementId)
                {
                    return element;
                }
            }

            return null;
        }

        public LayoutElementTuning GetOrCreateElement(string elementId)
        {
            LayoutElementTuning existing = GetElement(elementId);
            if (existing != null)
            {
                return existing;
            }

            if (layoutElements == null)
            {
                layoutElements = new List<LayoutElementTuning>();
            }

            LayoutElementTuning created = CreateDefaultElement(elementId);
            layoutElements.Add(created);
            return created;
        }

        public void EnsureDefaults()
        {
            GetOrCreateElement(AlttasElementId);
            GetOrCreateElement(MainMenuRotatorElementId);
            GetOrCreateElement(BottomPageNavElementId);
            GetOrCreateElement(HubPreviewOynaButtonElementId);
        }

        public bool NormalizeLayoutElements()
        {
            if (layoutElements == null)
            {
                return false;
            }

            bool changed = false;
            for (int i = 0; i < layoutElements.Count; i++)
            {
                LayoutElementTuning element = layoutElements[i];
                if (element != null)
                {
                    changed |= element.NormalizeInPlace();
                }
            }

            return changed;
        }

        public void ResetToDefaults()
        {
            LayoutElementTuning alttas = ResetElementToDefault(AlttasElementId);
            LayoutElementTuning mainMenuRotator = ResetElementToDefault(MainMenuRotatorElementId);
            LayoutElementTuning bottomPageNav = ResetElementToDefault(BottomPageNavElementId);
            LayoutElementTuning oynaButton = ResetElementToDefault(HubPreviewOynaButtonElementId);
            layoutElements = new List<LayoutElementTuning>
            {
                alttas,
                mainMenuRotator,
                bottomPageNav,
                oynaButton
            };
        }

        public LayoutElementTuning ResetElementToDefault(string elementId)
        {
            string canonicalElementId = GetCanonicalElementId(elementId);
            LayoutElementTuning existing = GetElement(elementId);
            if (existing != null)
            {
                existing.elementId = canonicalElementId;
                if (!existing.TryCopyBaselineToCurrent())
                {
                    LayoutElementTuning fallbackElement = CreateDefaultElement(elementId);
                    existing.CopyFrom(fallbackElement);
                }

                return existing;
            }

            if (layoutElements == null)
            {
                layoutElements = new List<LayoutElementTuning>();
            }

            LayoutElementTuning defaultElement = CreateDefaultElement(elementId);
            layoutElements.Add(defaultElement);
            return defaultElement;
        }

        private static LayoutElementTuning CreateDefaultElement(string elementId)
        {
            string normalizedElementId = NormalizeElementId(elementId);
            if (normalizedElementId == NormalizeElementId(MainMenuRotatorElementId))
            {
                return new LayoutElementTuning
                {
                    elementId = MainMenuRotatorElementId,
                    anchoredPosition = new Vector2(0f, 80f),
                    width = 520f,
                    height = 390f,
                    preserveAspect = true
                };
            }

            if (normalizedElementId == NormalizeElementId(BottomPageNavElementId))
            {
                Vector2 defaultSize = GetDefaultBottomPageNavSize();
                return new LayoutElementTuning
                {
                    elementId = BottomPageNavElementId,
                    anchoredPosition = BottomPageNavAnchoredPosition,
                    width = defaultSize.x,
                    height = defaultSize.y,
                    preserveAspect = true
                };
            }

            if (normalizedElementId == NormalizeElementId(HubPreviewOynaButtonElementId))
            {
                return new LayoutElementTuning
                {
                    elementId = HubPreviewOynaButtonElementId,
                    anchoredPosition = HubPreviewOynaButtonAnchoredPosition,
                    width = HubPreviewOynaButtonSize.x,
                    height = HubPreviewOynaButtonSize.y,
                    preserveAspect = false
                };
            }

            return new LayoutElementTuning
            {
                elementId = string.IsNullOrWhiteSpace(elementId) ? AlttasElementId : elementId,
                anchoredPosition = new Vector2(0f, -355f),
                width = 490f,
                height = 420f,
                preserveAspect = true
            };
        }

        private static string NormalizeElementId(string elementId)
        {
            return string.IsNullOrWhiteSpace(elementId) ? string.Empty : elementId.Trim().ToLowerInvariant();
        }

        private static string GetCanonicalElementId(string elementId)
        {
            string normalizedElementId = NormalizeElementId(elementId);
            if (normalizedElementId == NormalizeElementId(MainMenuRotatorElementId))
            {
                return MainMenuRotatorElementId;
            }

            if (normalizedElementId == NormalizeElementId(BottomPageNavElementId))
            {
                return BottomPageNavElementId;
            }

            if (normalizedElementId == NormalizeElementId(HubPreviewOynaButtonElementId))
            {
                return HubPreviewOynaButtonElementId;
            }

            return string.IsNullOrWhiteSpace(elementId) ? AlttasElementId : elementId;
        }

        private static Vector2 GetDefaultBottomPageNavSize()
        {
            Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(BottomPageNavSpritePath);
            Vector2 frameSpriteSize = frameSprite != null ? frameSprite.rect.size : BottomPageNavMaxSize;
            return FitWithin(frameSpriteSize, BottomPageNavMaxSize);
        }

        private static Vector2 FitWithin(Vector2 size, Vector2 maxSize)
        {
            if (size.x <= 0f || size.y <= 0f)
            {
                return maxSize;
            }

            float scale = Mathf.Min(Mathf.Min(maxSize.x / size.x, maxSize.y / size.y), 1f);
            return new Vector2(size.x * scale, size.y * scale);
        }

        private void OnValidate()
        {
            EnsureDefaults();
            if (NormalizeLayoutElements())
            {
                EditorUtility.SetDirty(this);
            }
        }
    }

}
