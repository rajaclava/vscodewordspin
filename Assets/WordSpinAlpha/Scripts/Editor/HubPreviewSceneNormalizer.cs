using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WordSpinAlpha.Core;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    [InitializeOnLoad]
    public static class HubPreviewSceneNormalizer
    {
        private const string HubPreviewScenePath = "Assets/WordSpinAlpha/Scenes/" + GameConstants.SceneHubPreview + ".unity";

        private static readonly Vector2 CanvasReference = new Vector2(864f, 1536f);
        private static readonly Vector2 BackgroundCoverSize = new Vector2(1024f, 1536f);
        private static readonly Vector2 LogoPosition = new Vector2(0f, 540f);
        private static readonly Vector2 LogoSize = new Vector2(640f, 260f);
        private static readonly Vector2 LanguagePosition = new Vector2(0f, 380f);
        private static readonly Vector2 LanguageSize = new Vector2(440f, 70f);
        private static readonly string[] LanguageHighlightLabels = { "TR", "EN", "DE", "ES" };
        private static readonly float[] LanguageHighlightXPositions = { -165f, -55f, 55f, 165f };
        private static readonly Vector2 RotatorPosition = new Vector2(0f, 80f);
        private static readonly Vector2 RotatorSize = new Vector2(520f, 400f);
        private static readonly Vector2 PlayButtonPosition = new Vector2(0f, -430f);
        private static readonly Vector2 PlayButtonSize = new Vector2(520f, 170f);
        private static readonly Vector2 PlayButtonTitlePosition = new Vector2(0f, -416f);
        private static readonly Vector2 PlayButtonTitleSize = new Vector2(380f, 50f);
        private static readonly Vector2 PlayButtonSubtitlePosition = new Vector2(0f, -448f);
        private static readonly Vector2 PlayButtonSubtitleSize = new Vector2(360f, 30f);
        private static readonly Vector2 StartLevelPosition = new Vector2(0f, -550f);
        private static readonly Vector2 StartLevelSize = new Vector2(420f, 32f);

        private const float PlayButtonTitleFontSize = 36f;
        private const float PlayButtonSubtitleFontSize = 19f;
        private const float StartLevelFontSize = 20f;
        private const float PreviewRootRotationZ = 0f;

        static HubPreviewSceneNormalizer()
        {
            EditorApplication.delayCall += NormalizeIfHubPreviewIsOpen;
            EditorSceneManager.sceneOpened -= OnSceneOpened;
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredEditMode)
            {
                EditorApplication.delayCall += NormalizeIfHubPreviewIsOpen;
            }
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (scene.path != HubPreviewScenePath)
            {
                return;
            }

            EditorApplication.delayCall += NormalizeIfHubPreviewIsOpen;
        }

        [MenuItem("Tools/WordSpin Alpha/Normalize Open Hub Preview")]
        public static void NormalizeIfHubPreviewIsOpen()
        {
            return; // PHASE 1: Disable sandbox normalization
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.path != HubPreviewScenePath)
            {
                return;
            }

            bool changed = false;
            Canvas canvas = FindObjectByName<Canvas>("HubPreviewCanvas");
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                changed |= canvasRect != null ? NormalizeCanvasRect(canvasRect) : NormalizeTransform(canvas.transform);
                CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
                if (scaler != null)
                {
                    if ((scaler.referenceResolution - CanvasReference).sqrMagnitude >= 0.001f)
                    {
                        scaler.referenceResolution = CanvasReference;
                        changed = true;
                    }

                    if (!Mathf.Approximately(scaler.matchWidthOrHeight, 1f))
                    {
                        scaler.matchWidthOrHeight = 1f;
                        changed = true;
                    }
                }
            }

            RectTransform root = FindObjectByName<RectTransform>("MainMenuPngPreviewRoot");
            if (root != null)
            {
                changed |= NormalizeRect(root, Vector2.zero, CanvasReference, false);
                changed |= SetRotation(root, PreviewRootRotationZ);
                changed |= ForceUprightSubtree(root);
                changed |= EnsureInteractiveCanvasGroup(root);
                changed |= NormalizeLayer("Background", Vector2.zero, BackgroundCoverSize);
                changed |= NormalizeLayer("Logo", LogoPosition, LogoSize);
                changed |= NormalizeLayer("LanguageSelect", LanguagePosition, LanguageSize);
                changed |= EnsureLanguageHighlights(root);
                changed |= RemoveRootNodeVisualOrphans(activeScene);
                changed |= NormalizeLayer("Rotator", RotatorPosition, RotatorSize);
                changed |= NormalizeLayer("PlayButton", PlayButtonPosition, PlayButtonSize);
                changed |= EnsurePreviewButton(root);
                changed |= NormalizeLayer("PlayButton_Title", PlayButtonTitlePosition, PlayButtonTitleSize);
                changed |= NormalizeText("PlayButton_Title", PlayButtonTitleFontSize);
                changed |= NormalizeLayer("PlayButton_Subtitle", PlayButtonSubtitlePosition, PlayButtonSubtitleSize);
                changed |= NormalizeText("PlayButton_Subtitle", PlayButtonSubtitleFontSize);
                changed |= NormalizeLayer("StartLevel_Label", StartLevelPosition, StartLevelSize);
                changed |= NormalizeText("StartLevel_Label", StartLevelFontSize);
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
                EditorSceneManager.SaveScene(activeScene);
                Debug.Log("[WordSpinAlpha] HubPreview scene normalized and saved: rotation/scale, language highlight, and main menu layer layout reset.");
            }
        }

        private static bool NormalizeLayer(string name, Vector2 position, Vector2 size)
        {
            RectTransform rect = FindObjectByName<RectTransform>(name);
            return rect != null && NormalizeRect(rect, position, size, true);
        }

        private static bool NormalizeText(string name, float fontSize)
        {
            TextMeshProUGUI text = FindObjectByName<TextMeshProUGUI>(name);
            if (text == null || Mathf.Approximately(text.fontSize, fontSize))
            {
                return false;
            }

            text.fontSize = fontSize;
            return true;
        }

        private static bool EnsureLanguageHighlights(RectTransform root)
        {
            bool changed = false;
            CanvasGroup[] highlights = new CanvasGroup[LanguageHighlightLabels.Length];

            for (int i = 0; i < LanguageHighlightLabels.Length; i++)
            {
                string label = LanguageHighlightLabels[i];
                RectTransform highlightRect = FindDirectChildRect(root, $"LanguageHighlight_{label}");
                if (highlightRect == null)
                {
                    GameObject highlightObject = new GameObject($"LanguageHighlight_{label}", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                    highlightRect = highlightObject.GetComponent<RectTransform>();
                    highlightRect.SetParent(root, false);
                    changed = true;
                }

                changed |= NormalizeRect(highlightRect, new Vector2(LanguageHighlightXPositions[i], LanguagePosition.y), new Vector2(98f, 54f), true);

                CanvasGroup highlightGroup = highlightRect.GetComponent<CanvasGroup>();
                if (highlightGroup == null)
                {
                    highlightGroup = highlightRect.gameObject.AddComponent<CanvasGroup>();
                    changed = true;
                }

                if (highlightGroup.interactable)
                {
                    highlightGroup.interactable = false;
                    changed = true;
                }

                if (highlightGroup.blocksRaycasts)
                {
                    highlightGroup.blocksRaycasts = false;
                    changed = true;
                }

                Image glow = highlightRect.GetComponent<Image>();
                if (glow == null)
                {
                    glow = highlightRect.gameObject.AddComponent<Image>();
                    changed = true;
                }

                changed |= ConfigurePreviewImage(glow, new Color(1f, 0.72f, 0.18f, 0.26f), Image.Type.Sliced, false);
                changed |= EnsureColorChild(highlightRect, $"LanguageHighlight_{label}_Rim", Vector2.zero, new Vector2(88f, 42f), new Color(1f, 0.91f, 0.46f, 0.34f));
                changed |= EnsureColorChild(highlightRect, $"LanguageHighlight_{label}_Underline", new Vector2(0f, -22f), new Vector2(58f, 5f), new Color(1f, 0.78f, 0.20f, 0.92f));
                changed |= EnsureColorChild(highlightRect, $"LanguageHighlight_{label}_TopShine", new Vector2(0f, 17f), new Vector2(62f, 4f), new Color(1f, 0.96f, 0.70f, 0.50f));
                highlights[i] = highlightGroup;
            }

            MainMenuLanguagePreviewHighlighter highlighter = root.GetComponent<MainMenuLanguagePreviewHighlighter>();
            if (highlighter == null)
            {
                highlighter = root.gameObject.AddComponent<MainMenuLanguagePreviewHighlighter>();
                changed = true;
            }

            changed |= SetCanvasGroupArray(highlighter, "languageHighlights", highlights);
            changed |= EnsureLanguageHitboxes(root, highlighter);
            highlighter.Refresh();
            return changed;
        }

        private static bool RemoveRootNodeVisualOrphans(Scene scene)
        {
            bool changed = false;
            GameObject[] rootObjects = scene.GetRootGameObjects();

            for (int i = 0; i < rootObjects.Length; i++)
            {
                GameObject rootObject = rootObjects[i];
                if (rootObject == null || rootObject.name != "NodeVisual")
                {
                    continue;
                }

                Object.DestroyImmediate(rootObject);
                changed = true;
            }

            return changed;
        }

        private static bool EnsureInteractiveCanvasGroup(RectTransform root)
        {
            bool changed = false;
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            if (group == null)
            {
                group = root.gameObject.AddComponent<CanvasGroup>();
                changed = true;
            }

            if (!group.interactable)
            {
                group.interactable = true;
                changed = true;
            }

            if (!group.blocksRaycasts)
            {
                group.blocksRaycasts = true;
                changed = true;
            }

            if (!Mathf.Approximately(group.alpha, 1f))
            {
                group.alpha = 1f;
                changed = true;
            }

            return changed;
        }

        private static bool EnsureLanguageHitboxes(RectTransform root, MainMenuLanguagePreviewHighlighter highlighter)
        {
            bool changed = false;
            string[] codes = { "tr", "en", "de", "es" };

            for (int i = 0; i < LanguageHighlightLabels.Length; i++)
            {
                string label = LanguageHighlightLabels[i];
                RectTransform hitboxRect = FindDirectChildRect(root, $"LanguageButton_{label}");
                if (hitboxRect == null)
                {
                    GameObject hitboxObject = new GameObject($"LanguageButton_{label}", typeof(RectTransform), typeof(Image), typeof(ButtonPressEffect), typeof(MainMenuPreviewLanguageButton));
                    hitboxRect = hitboxObject.GetComponent<RectTransform>();
                    hitboxRect.SetParent(root, false);
                    changed = true;
                }

                changed |= NormalizeRect(hitboxRect, new Vector2(LanguageHighlightXPositions[i], LanguagePosition.y), new Vector2(104f, 62f), true);

                Image image = hitboxRect.GetComponent<Image>();
                if (image == null)
                {
                    image = hitboxRect.gameObject.AddComponent<Image>();
                    changed = true;
                }

                changed |= ConfigurePreviewImage(image, new Color(1f, 1f, 1f, 0f), Image.Type.Simple, true);

                if (hitboxRect.GetComponent<ButtonPressEffect>() == null)
                {
                    hitboxRect.gameObject.AddComponent<ButtonPressEffect>();
                    changed = true;
                }

                MainMenuPreviewLanguageButton languageButton = hitboxRect.GetComponent<MainMenuPreviewLanguageButton>();
                if (languageButton == null)
                {
                    languageButton = hitboxRect.gameObject.AddComponent<MainMenuPreviewLanguageButton>();
                    changed = true;
                }

                languageButton.Configure(highlighter, codes[i]);
            }

            return changed;
        }

        private static bool EnsurePreviewButton(RectTransform root)
        {
            RectTransform rect = FindDirectChildRect(root, "PlayButton");
            if (rect == null)
            {
                return false;
            }

            bool changed = false;
            Image image = rect.GetComponent<Image>();
            if (image != null && !image.raycastTarget)
            {
                image.raycastTarget = true;
                changed = true;
            }

            Button button = rect.GetComponent<Button>();
            if (button == null)
            {
                button = rect.gameObject.AddComponent<Button>();
                changed = true;
            }

            if (button != null && button.targetGraphic != image)
            {
                button.targetGraphic = image;
                changed = true;
            }

            if (rect.GetComponent<ButtonPressEffect>() == null)
            {
                rect.gameObject.AddComponent<ButtonPressEffect>();
                changed = true;
            }

            RectTransform title = FindDirectChildRect(root, "PlayButton_Title");
            RectTransform subtitle = FindDirectChildRect(root, "PlayButton_Subtitle");
            TextMeshProUGUI titleGraphic = title != null ? title.GetComponent<TextMeshProUGUI>() : null;
            TextMeshProUGUI subtitleGraphic = subtitle != null ? subtitle.GetComponent<TextMeshProUGUI>() : null;

            RectTransform hitboxRect = FindDirectChildRect(root, "PlayButton_Hitbox");
            if (hitboxRect == null)
            {
                GameObject hitboxObject = new GameObject("PlayButton_Hitbox", typeof(RectTransform), typeof(Image), typeof(MainMenuPreviewPressEffect));
                hitboxRect = hitboxObject.GetComponent<RectTransform>();
                hitboxRect.SetParent(root, false);
                changed = true;
            }

            hitboxRect.SetAsLastSibling();
            changed |= NormalizeRect(hitboxRect, PlayButtonPosition, PlayButtonSize, true);

            Image hitboxImage = hitboxRect.GetComponent<Image>();
            if (hitboxImage == null)
            {
                hitboxImage = hitboxRect.gameObject.AddComponent<Image>();
                changed = true;
            }

            changed |= ConfigurePreviewImage(hitboxImage, new Color(1f, 1f, 1f, 0f), Image.Type.Simple, true);

            MainMenuPreviewPressEffect pressEffect = hitboxRect.GetComponent<MainMenuPreviewPressEffect>();
            if (pressEffect == null)
            {
                pressEffect = hitboxRect.gameObject.AddComponent<MainMenuPreviewPressEffect>();
                changed = true;
            }

            RectTransform[] scaleTargets = title != null && subtitle != null
                ? new[] { rect, title, subtitle }
                : new[] { rect };
            Graphic[] brightnessTargets = titleGraphic != null && subtitleGraphic != null
                ? new Graphic[] { image, titleGraphic, subtitleGraphic }
                : new Graphic[] { image };
            pressEffect.Configure(scaleTargets, brightnessTargets);
            return true;
        }

        private static bool EnsureColorChild(RectTransform parent, string name, Vector2 position, Vector2 size, Color color)
        {
            bool changed = false;
            RectTransform rect = FindDirectChildRect(parent, name);
            if (rect == null)
            {
                GameObject child = new GameObject(name, typeof(RectTransform), typeof(Image));
                rect = child.GetComponent<RectTransform>();
                rect.SetParent(parent, false);
                changed = true;
            }

            changed |= NormalizeRect(rect, position, size, true);

            Image image = rect.GetComponent<Image>();
            if (image == null)
            {
                image = rect.gameObject.AddComponent<Image>();
                changed = true;
            }

            changed |= ConfigurePreviewImage(image, color, Image.Type.Sliced, false);
            return changed;
        }

        private static bool ConfigurePreviewImage(Image image, Color color, Image.Type imageType, bool raycastTarget)
        {
            bool changed = false;
            Sprite builtin = AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
            if (image.sprite != builtin)
            {
                image.sprite = builtin;
                changed = true;
            }

            if (image.type != imageType)
            {
                image.type = imageType;
                changed = true;
            }

            if (image.color != color)
            {
                image.color = color;
                changed = true;
            }

            if (image.raycastTarget != raycastTarget)
            {
                image.raycastTarget = raycastTarget;
                changed = true;
            }

            return changed;
        }

        private static bool SetCanvasGroupArray(UnityEngine.Object target, string field, CanvasGroup[] values)
        {
            bool changed = false;
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(field);
            if (property == null || !property.isArray)
            {
                return false;
            }

            if (property.arraySize != values.Length)
            {
                property.arraySize = values.Length;
                changed = true;
            }

            for (int i = 0; i < values.Length; i++)
            {
                SerializedProperty element = property.GetArrayElementAtIndex(i);
                if (element.objectReferenceValue != values[i])
                {
                    element.objectReferenceValue = values[i];
                    changed = true;
                }
            }

            if (changed)
            {
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static bool NormalizeRect(RectTransform rect, Vector2 position, Vector2 size, bool centerAnchors)
        {
            bool changed = NormalizeTransform(rect);
            if (centerAnchors)
            {
                if ((rect.anchorMin - new Vector2(0.5f, 0.5f)).sqrMagnitude >= 0.001f)
                {
                    rect.anchorMin = new Vector2(0.5f, 0.5f);
                    changed = true;
                }

                if ((rect.anchorMax - new Vector2(0.5f, 0.5f)).sqrMagnitude >= 0.001f)
                {
                    rect.anchorMax = new Vector2(0.5f, 0.5f);
                    changed = true;
                }
            }

            if ((rect.pivot - new Vector2(0.5f, 0.5f)).sqrMagnitude >= 0.001f)
            {
                rect.pivot = new Vector2(0.5f, 0.5f);
                changed = true;
            }

            if ((rect.anchoredPosition - position).sqrMagnitude >= 0.001f)
            {
                rect.anchoredPosition = position;
                changed = true;
            }

            if ((rect.sizeDelta - size).sqrMagnitude >= 0.001f)
            {
                rect.sizeDelta = size;
                changed = true;
            }

            return changed;
        }

        private static RectTransform FindDirectChildRect(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child as RectTransform;
                }
            }

            return null;
        }

        private static bool NormalizeCanvasRect(RectTransform rect)
        {
            bool changed = NormalizeTransform(rect);
            if ((rect.anchorMin - Vector2.zero).sqrMagnitude >= 0.001f)
            {
                rect.anchorMin = Vector2.zero;
                changed = true;
            }

            if ((rect.anchorMax - Vector2.one).sqrMagnitude >= 0.001f)
            {
                rect.anchorMax = Vector2.one;
                changed = true;
            }

            if ((rect.pivot - new Vector2(0.5f, 0.5f)).sqrMagnitude >= 0.001f)
            {
                rect.pivot = new Vector2(0.5f, 0.5f);
                changed = true;
            }

            if (rect.anchoredPosition.sqrMagnitude >= 0.001f)
            {
                rect.anchoredPosition = Vector2.zero;
                changed = true;
            }

            if (rect.sizeDelta.sqrMagnitude >= 0.001f)
            {
                rect.sizeDelta = Vector2.zero;
                changed = true;
            }

            return changed;
        }

        private static bool SetRotation(Transform transform, float rotationZ)
        {
            Quaternion expected = Quaternion.Euler(0f, 0f, rotationZ);
            if (Quaternion.Angle(transform.localRotation, expected) <= 0.01f)
            {
                return false;
            }

            transform.localRotation = expected;
            return true;
        }

        private static bool ForceUprightSubtree(Transform parent)
        {
            bool changed = false;
            foreach (Transform child in parent)
            {
                changed |= NormalizeTransform(child);
                changed |= ForceUprightSubtree(child);
            }

            return changed;
        }

        private static bool NormalizeTransform(Transform transform)
        {
            bool changed = false;
            if (transform.localRotation != Quaternion.identity)
            {
                transform.localRotation = Quaternion.identity;
                changed = true;
            }

            if (transform.localScale != Vector3.one)
            {
                transform.localScale = Vector3.one;
                changed = true;
            }

            return changed;
        }

        private static T FindObjectByName<T>(string objectName) where T : Component
        {
            T[] objects = UnityEngine.Object.FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (T item in objects)
            {
                if (item.name == objectName)
                {
                    return item;
                }
            }

            return null;
        }
    }
}
