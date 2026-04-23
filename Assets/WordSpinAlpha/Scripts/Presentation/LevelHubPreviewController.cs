using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class LevelHubPreviewController : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [System.Serializable]
        public struct RailPoint
        {
            public Vector2 position;
            [Range(0.05f, 2f)] public float scale;
            public float rotation;
            [Range(0f, 1f)] public float alpha;

            public RailPoint(Vector2 position, float scale, float rotation = 0f, float alpha = 1f)
            {
                this.position = position;
                this.scale = scale;
                this.rotation = rotation;
                this.alpha = alpha;
            }
        }

        private const int DefaultTotalLevels = 25;

        private static readonly RailPoint[] DefaultRailPoints =
        {
            new RailPoint(new Vector2(0f, -300f), 1.00f),
            new RailPoint(new Vector2(100f, -115f), 0.72f),
            new RailPoint(new Vector2(145f, 60f), 0.58f),
            new RailPoint(new Vector2(20f, 230f), 0.46f),
            new RailPoint(new Vector2(-80f, 375f), 0.36f),
            new RailPoint(new Vector2(-115f, 490f), 0.28f),
            new RailPoint(new Vector2(-85f, 585f), 0.22f)
        };

        private static readonly Color HiddenNodeHostColor = new Color(1f, 1f, 1f, 0f);

        [SerializeField] internal RectTransform[] levelNodes;
        [SerializeField] internal TextMeshProUGUI[] levelNumberLabels;
        [SerializeField] internal TextMeshProUGUI oynaSubtitleLabel;
        [SerializeField] private RailPoint[] railPoints = CloneDefaultRailPoints();
        [SerializeField, Min(1)] private int totalLevels = DefaultTotalLevels;
        [SerializeField, Min(20f)] private float dragPixelsPerLevel = 160f;
        [SerializeField, Min(1f)] private float snapSharpness = 12f;

        private float scrollOffset;
        private float dragStartY;
        private float scrollAtDragStart;
        private Coroutine snapRoutine;

        private int ActiveLevel => Mathf.Clamp(Mathf.RoundToInt(scrollOffset) + 1, 1, Mathf.Max(1, totalLevels));
        public int RailPointCount => railPoints != null ? railPoints.Length : 0;

        private void Start()
        {
            EnsureRuntimeDefaults();

            if (!Application.isPlaying)
            {
                Refresh();
                return;
            }

            for (int i = 0; levelNodes != null && i < levelNodes.Length; i++)
            {
                if (levelNodes[i] == null)
                {
                    continue;
                }

                Button button = levelNodes[i].GetComponent<Button>();
                if (button == null)
                {
                    continue;
                }

                int captured = i;
                button.onClick.AddListener(() => OnNodeTapped(captured));
            }

            Refresh();
        }

        private void OnValidate()
        {
            EnsureRuntimeDefaults();
            Refresh();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (snapRoutine != null)
            {
                StopCoroutine(snapRoutine);
                snapRoutine = null;
            }

            dragStartY = eventData.position.y;
            scrollAtDragStart = scrollOffset;
        }

        public void OnDrag(PointerEventData eventData)
        {
            float delta = (eventData.position.y - dragStartY) / Mathf.Max(20f, dragPixelsPerLevel);
            scrollOffset = Mathf.Clamp(scrollAtDragStart - delta, 0f, Mathf.Max(0f, totalLevels - 1f));
            Refresh();
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (Application.isPlaying)
            {
                snapRoutine = StartCoroutine(SnapRoutine());
            }
            else
            {
                scrollOffset = Mathf.Clamp(Mathf.Round(scrollOffset), 0f, Mathf.Max(0f, totalLevels - 1f));
                Refresh();
            }
        }

        public void OnOynaPressed()
        {
            int selectedLevel = ActiveLevel;
            int highestUnlockedLevel = GetHighestUnlockedLevel();
            if (selectedLevel > highestUnlockedLevel)
            {
                Debug.LogWarning($"[HubPreview] Kilitli seviye baslatilamaz. Secili={selectedLevel}, Acik={highestUnlockedLevel}");
                return;
            }

            if (SceneNavigator.Instance == null)
            {
                Debug.LogWarning("[HubPreview] SceneNavigator bulunamadi. SceneBootstrap aktif olmali.");
                return;
            }

            bool started = CanResumeSelectedLevel()
                ? SceneNavigator.Instance.OpenGameplayForProgress()
                : SceneNavigator.Instance.OpenGameplayLevel(selectedLevel, true);

            if (!started)
            {
                Debug.LogWarning($"[HubPreview] Seviye {selectedLevel} baslatilamadi. Can veya session durumu kontrol edilmeli.");
            }
        }

        public void EditorRefresh()
        {
            EnsureRuntimeDefaults();
            scrollOffset = Mathf.Clamp(scrollOffset, 0f, Mathf.Max(0f, totalLevels - 1f));
            Refresh();
        }

        public RailPoint GetRailPoint(int index)
        {
            EnsureRailPoints();
            return railPoints[Mathf.Clamp(index, 0, railPoints.Length - 1)];
        }

        public Vector2 SampleRailPosition(float t)
        {
            return EvalPosition(t);
        }

        public void SetRailPoint(int index, RailPoint point)
        {
            EnsureRailPoints();
            if (index < 0 || index >= railPoints.Length)
            {
                return;
            }

            railPoints[index] = point;
            Refresh();
        }

        public void ResetRailToDefault()
        {
            railPoints = CloneDefaultRailPoints();
            Refresh();
        }

        private void OnNodeTapped(int poolIndex)
        {
            int level = Mathf.FloorToInt(scrollOffset) + poolIndex + 1;
            if (level < 1 || level > totalLevels)
            {
                return;
            }

            if (snapRoutine != null)
            {
                StopCoroutine(snapRoutine);
                snapRoutine = null;
            }

            snapRoutine = StartCoroutine(LerpScrollTo(level - 1f));
        }

        private static int GetHighestUnlockedLevel()
        {
            if (SaveManager.Instance == null)
            {
                return 1;
            }

            return SaveManager.Instance.Data.progress.GetHighestUnlockedLevel(CurrentLanguageCode());
        }

        private bool CanResumeSelectedLevel()
        {
            if (SaveManager.Instance == null)
            {
                return false;
            }

            SessionSnapshot session = SaveManager.Instance.Data.session;
            return session != null &&
                   session.hasActiveSession &&
                   session.levelId == ActiveLevel &&
                   string.Equals(
                       GameConstants.NormalizeLanguageCode(session.languageCode),
                       CurrentLanguageCode(),
                       System.StringComparison.OrdinalIgnoreCase);
        }

        private static string CurrentLanguageCode()
        {
            return SaveManager.Instance != null
                ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode)
                : GameConstants.DefaultLanguageCode;
        }

        private void Refresh()
        {
            EnsureRuntimeDefaults();
            if (levelNodes == null || levelNodes.Length == 0)
            {
                return;
            }

            int baseLevelIndex = Mathf.FloorToInt(scrollOffset);
            float fraction = scrollOffset - baseLevelIndex;
            int poolSize = Mathf.Min(levelNodes.Length, Mathf.Max(1, RailPointCount));

            for (int i = 0; i < levelNodes.Length; i++)
            {
                RectTransform node = levelNodes[i];
                if (node == null)
                {
                    continue;
                }

                if (i >= poolSize)
                {
                    node.gameObject.SetActive(false);
                    continue;
                }

                float slotT = i - fraction;
                int levelNumber = baseLevelIndex + i + 1;
                bool visible = levelNumber >= 1 && levelNumber <= totalLevels && slotT >= -0.6f && slotT < poolSize - 0.4f;

                node.gameObject.SetActive(visible);
                if (!visible)
                {
                    continue;
                }

                node.anchoredPosition = EvalPosition(slotT);
                float scale = EvalScale(slotT);
                node.localScale = new Vector3(scale, scale, 1f);
                node.localRotation = Quaternion.Euler(0f, 0f, EvalRotation(slotT));
                TextMeshProUGUI label = levelNumberLabels != null && i < levelNumberLabels.Length ? levelNumberLabels[i] : null;
                SetNodeAlpha(node, label, EvalAlpha(slotT));

                if (label != null)
                {
                    label.text = levelNumber.ToString();
                }
            }

            if (oynaSubtitleLabel != null)
            {
                oynaSubtitleLabel.text = $"Seviye {ActiveLevel}'den basla";
            }
        }

        private IEnumerator SnapRoutine()
        {
            float target = Mathf.Clamp(Mathf.Round(scrollOffset), 0f, Mathf.Max(0f, totalLevels - 1f));
            yield return LerpScrollTo(target);
        }

        private IEnumerator LerpScrollTo(float target)
        {
            target = Mathf.Clamp(target, 0f, Mathf.Max(0f, totalLevels - 1f));
            while (Mathf.Abs(scrollOffset - target) > 0.004f)
            {
                scrollOffset = Mathf.Lerp(scrollOffset, target, Time.deltaTime * snapSharpness);
                Refresh();
                yield return null;
            }

            scrollOffset = target;
            Refresh();
            snapRoutine = null;
        }

        private Vector2 EvalPosition(float t)
        {
            EnsureRailPoints();
            int length = railPoints.Length;
            t = Mathf.Clamp(t, 0f, length - 1f);
            int low = Mathf.FloorToInt(t);
            int high = Mathf.Min(low + 1, length - 1);

            Vector2 p0 = railPoints[Mathf.Max(0, low - 1)].position;
            Vector2 p1 = railPoints[low].position;
            Vector2 p2 = railPoints[high].position;
            Vector2 p3 = railPoints[Mathf.Min(length - 1, high + 1)].position;

            float u = t - low;
            float u2 = u * u;
            float u3 = u2 * u;

            return 0.5f * ((2f * p1) + (-p0 + p2) * u + (2f * p0 - 5f * p1 + 4f * p2 - p3) * u2 + (-p0 + 3f * p1 - 3f * p2 + p3) * u3);
        }

        private float EvalScale(float t)
        {
            return EvalFloat(t, point => point.scale, 1f);
        }

        private float EvalRotation(float t)
        {
            return EvalFloat(t, point => point.rotation, 0f);
        }

        private float EvalAlpha(float t)
        {
            return EvalFloat(t, point => point.alpha, 1f);
        }

        private float EvalFloat(float t, System.Func<RailPoint, float> selector, float fallback)
        {
            EnsureRailPoints();
            int length = railPoints.Length;
            if (length == 0)
            {
                return fallback;
            }

            t = Mathf.Clamp(t, 0f, length - 1f);
            int low = Mathf.FloorToInt(t);
            int high = Mathf.Min(low + 1, length - 1);
            return Mathf.Lerp(selector(railPoints[low]), selector(railPoints[high]), t - low);
        }

        private void EnsureRailPoints()
        {
            if (railPoints != null && railPoints.Length >= 2)
            {
                return;
            }

            railPoints = CloneDefaultRailPoints();
        }

        private void EnsureRuntimeDefaults()
        {
            EnsureRailPoints();

            if (totalLevels < 1)
            {
                totalLevels = DefaultTotalLevels;
            }

            if (dragPixelsPerLevel < 20f)
            {
                dragPixelsPerLevel = 160f;
            }

            if (snapSharpness < 1f)
            {
                snapSharpness = 12f;
            }

        }

        private static RailPoint[] CloneDefaultRailPoints()
        {
            var clone = new RailPoint[DefaultRailPoints.Length];
            for (int i = 0; i < DefaultRailPoints.Length; i++)
            {
                clone[i] = DefaultRailPoints[i];
            }

            return clone;
        }

        private static void SetNodeAlpha(RectTransform node, TextMeshProUGUI label, float alpha)
        {
            if (node == null)
            {
                return;
            }

            Image hostImage = node.GetComponent<Image>();
            if (hostImage != null)
            {
                if (hostImage.sprite != null)
                {
                    hostImage.sprite = null;
                }

                if (hostImage.color != HiddenNodeHostColor)
                {
                    hostImage.color = HiddenNodeHostColor;
                }

                if (!hostImage.raycastTarget)
                {
                    hostImage.raycastTarget = true;
                }
            }

            Button button = node.GetComponent<Button>();
            SetGraphicAlpha(button != null ? button.targetGraphic : null, alpha);
            SetGraphicAlpha(label, alpha);
        }

        private static void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
            {
                return;
            }

            Color color = graphic.color;
            if (!Mathf.Approximately(color.a, alpha))
            {
                color.a = alpha;
                graphic.color = color;
            }
        }
    }
}

            if (dragPixelsPerLevel < 20f)
            {
                dragPixelsPerLevel = 160f;
            }

            if (snapSharpness < 1f)
            {
                snapSharpness = 12f;
            }

        }

        private static RailPoint[] CloneDefaultRailPoints()
        {
            var clone = new RailPoint[DefaultRailPoints.Length];
            for (int i = 0; i < DefaultRailPoints.Length; i++)
            {
                clone[i] = DefaultRailPoints[i];
            }

            return clone;
        }

        private static void SetNodeAlpha(RectTransform node, TextMeshProUGUI label, float alpha)
        {
            if (node == null)
            {
                return;
            }

            Image hostImage = node.GetComponent<Image>();
            if (hostImage != null)
            {
                if (hostImage.sprite != null)
                {
                    hostImage.sprite = null;
                }

                if (hostImage.color != HiddenNodeHostColor)
                {
                    hostImage.color = HiddenNodeHostColor;
                }

                if (!hostImage.raycastTarget)
                {
                    hostImage.raycastTarget = true;
                }
            }

            Button button = node.GetComponent<Button>();
            SetGraphicAlpha(button != null ? button.targetGraphic : null, alpha);
            SetGraphicAlpha(label, alpha);
        }

        private static void SetGraphicAlpha(Graphic graphic, float alpha)
        {
            if (graphic == null)
            {
                return;
            }

            Color color = graphic.color;
            if (!Mathf.Approximately(color.a, alpha))
            {
                color.a = alpha;
                graphic.color = color;
            }
        }

        public void OpenJourneyTab() => SwitchTab(HubPreviewTab.Journey);
        public void OpenMissionsTab() => SwitchTab(HubPreviewTab.Missions);
        public void OpenProfileTab() => SwitchTab(HubPreviewTab.Profile);
        public void OpenStoreTab() => SwitchTab(HubPreviewTab.Store);

        private void SwitchTab(HubPreviewTab target)
        {
            if (activeTab == target) return;
            activeTab = target;
            EnsureTabVisibility(activeTab);
        }

        private void EnsureTabVisibility(HubPreviewTab tab)
        {
            bool isJourney = tab == HubPreviewTab.Journey;
            
            if (alttasRoot != null) alttasRoot.SetActive(isJourney);
            if (pathContainerRoot != null) pathContainerRoot.SetActive(isJourney);
            if (oynaBgRoot != null) oynaBgRoot.SetActive(isJourney);

            if (missionsPlaceholder != null) missionsPlaceholder.SetActive(tab == HubPreviewTab.Missions);
            if (profilePlaceholder != null) profilePlaceholder.SetActive(tab == HubPreviewTab.Profile);
            if (storePlaceholder != null) storePlaceholder.SetActive(tab == HubPreviewTab.Store);
        }

        private void LoadCachedLevels()
        {
            if (ContentService.Instance != null && Application.isPlaying)
            {
                cachedLevels = (ContentService.Instance.LoadLevels().levels ?? Array.Empty<LevelDefinition>()).OrderBy(level => level.levelId).ToArray();
                if (cachedLevels.Length > 0)
                {
                    totalLevels = cachedLevels.Length;
                }
            }
        }

        private void EnsureResumePrompt()
        {
            if (resumePromptRoot != null)
            {
                WireResumePromptButtons();
                resumePromptRoot.SetActive(false);
                return;
            }

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            Transform parent = parentCanvas != null ? parentCanvas.transform : transform;

            resumePromptRoot = new GameObject("ResumePromptOverlay", typeof(RectTransform), typeof(Image));
            resumePromptRoot.transform.SetParent(parent, false);
            resumePromptRoot.transform.SetAsLastSibling();
            RectTransform rootRect = resumePromptRoot.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            Image rootImage = resumePromptRoot.GetComponent<Image>();
            rootImage.color = new Color(0.03f, 0.06f, 0.10f, 0.78f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(resumePromptRoot.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(700f, 420f);
            panelRect.anchoredPosition = Vector2.zero;
            Image panelImage = panel.GetComponent<Image>();
            panelImage.type = Image.Type.Simple;
            panelImage.color = new Color(0.16f, 0.28f, 0.44f, 0.98f);

            resumePromptTitleLabel = CreateLabel("Title", panel.transform, new Vector2(0.5f, 0.76f), new Vector2(560f, 54f), 34f, FontStyles.Bold, Color.white);
            resumePromptBodyLabel = CreateLabel("Body", panel.transform, new Vector2(0.5f, 0.56f), new Vector2(580f, 120f), 24f, FontStyles.Normal, new Color(0.94f, 0.97f, 1f));
            resumePromptBodyLabel.enableWordWrapping = true;
            resumePromptTitleLabel.text = "Kayitli Ilerleme Bulundu";
            resumePromptBodyLabel.text = "Bu seviyede kayitli ilerlemen var. Devam et dersen can harcanmaz. Bastan baslatirsan yeni giris maliyeti uygulanir.";

            resumePromptContinueButton = CreateButton("ContinueButton", panel.transform, new Vector2(0.5f, 0.28f), new Vector2(360f, 68f), new Color(0.26f, 0.55f, 0.89f, 1f), "Devam Et");
            resumePromptRestartButton = CreateButton("RestartButton", panel.transform, new Vector2(0.5f, 0.14f), new Vector2(360f, 68f), new Color(0.84f, 0.43f, 0.22f, 1f), "Bastan Basla");
            resumePromptCancelButton = CreateButton("CancelButton", panel.transform, new Vector2(0.5f, 0.06f), new Vector2(280f, 54f), new Color(0.18f, 0.24f, 0.31f, 1f), "Vazgec");

            WireResumePromptButtons();
            resumePromptRoot.SetActive(false);
        }

        private void WireResumePromptButtons()
        {
            if (resumePromptContinueButton != null)
            {
                resumePromptContinueButton.onClick.RemoveAllListeners();
                resumePromptContinueButton.onClick.AddListener(ContinueSelectedLevel);
            }

            if (resumePromptRestartButton != null)
            {
                resumePromptRestartButton.onClick.RemoveAllListeners();
                resumePromptRestartButton.onClick.AddListener(RestartSelectedLevel);
            }

            if (resumePromptCancelButton != null)
            {
                resumePromptCancelButton.onClick.RemoveAllListeners();
                resumePromptCancelButton.onClick.AddListener(CancelResumePrompt);
            }
        }

        private void ShowResumePrompt()
        {
            EnsureResumePrompt();
            if (resumePromptRoot != null) resumePromptRoot.SetActive(true);
        }

        private void HideResumePrompt()
        {
            if (resumePromptRoot != null) resumePromptRoot.SetActive(false);
        }

        public void ContinueSelectedLevel()
        {
            HideResumePrompt();
            if (CanResumeSelectedLevel())
            {
                SceneNavigator.Instance?.OpenGameplayForProgress();
                return;
            }

            if (SceneNavigator.Instance == null || !SceneNavigator.Instance.OpenGameplayLevel(ActiveLevel, true))
            {
                Debug.LogWarning("[HubPreview] Kayitli ilerleme bulunamadi. Seviye acilamadi.");
            }
        }

        public void RestartSelectedLevel()
        {
            HideResumePrompt();
            if (SceneNavigator.Instance == null || !SceneNavigator.Instance.OpenGameplayLevel(ActiveLevel, true))
            {
                Debug.LogWarning("[HubPreview] Bastan baslatmak icin yeterli can yok.");
            }
        }

        public void CancelResumePrompt()
        {
            HideResumePrompt();
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, Vector2 anchor, Vector2 size, float fontSize, FontStyles style, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 anchor, Vector2 size, Color color, string text)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.type = Image.Type.Simple;
            image.color = color;
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;

            GameObject labelGo = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform labelRect = labelGo.GetComponent<RectTransform>();
            labelRect.SetParent(rect, false);
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            TextMeshProUGUI label = labelGo.GetComponent<TextMeshProUGUI>();
            label.fontSize = 24f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.text = text;

            return button;
        }
    }
}
