using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Presentation
{
    public enum HubPreviewTab
    {
        Journey,
        Missions,
        Profile,
        Store
    }

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
        [SerializeField] internal GameObject alttasRoot;
        [SerializeField] internal GameObject pathContainerRoot;
        [SerializeField] internal GameObject oynaBgRoot;
        [SerializeField] internal GameObject missionsPlaceholder;
        [SerializeField] internal GameObject profilePlaceholder;
        [SerializeField] internal GameObject storePlaceholder;
        [SerializeField] private TextMeshProUGUI topBarEnergyLabel;
        [SerializeField] private TextMeshProUGUI topBarHintLabel;
        [SerializeField] private TextMeshProUGUI topBarCoinLabel;
        [SerializeField] private TextMeshProUGUI topBarLanguageLabel;
        [SerializeField] private RailPoint[] railPoints = CloneDefaultRailPoints();
        [SerializeField, Min(1)] private int totalLevels = DefaultTotalLevels;
        [SerializeField, Min(20f)] private float dragPixelsPerLevel = 160f;
        [SerializeField, Min(1f)] private float snapSharpness = 12f;

        private float scrollOffset;
        private float dragStartY;
        private float scrollAtDragStart;
        private Coroutine snapRoutine;
        private LevelDefinition[] cachedLevels = Array.Empty<LevelDefinition>();
        private GameObject resumePromptRoot;
        private Button resumePromptContinueButton;
        private Button resumePromptRestartButton;
        private Button resumePromptCancelButton;
        private HubPreviewTab activeTab = HubPreviewTab.Journey;

        private static int pendingScrollToLevel = -1;

        private int SelectedLevelId
        {
            get
            {
                int catalogIndex = Mathf.Clamp(Mathf.RoundToInt(scrollOffset), 0, Mathf.Max(0, totalLevels - 1));
                return ResolveLevelIdAtCatalogIndex(catalogIndex);
            }
        }

        public int RailPointCount => railPoints != null ? railPoints.Length : 0;

        private LevelDefinition ResolveLevelAtIndex(int index)
        {
            if (cachedLevels == null || index < 0 || index >= cachedLevels.Length)
                return null;
            return cachedLevels[index];
        }

        private int ResolveLevelIdAtCatalogIndex(int catalogIndex)
        {
            if (cachedLevels != null && cachedLevels.Length > 0)
            {
                catalogIndex = Mathf.Clamp(catalogIndex, 0, cachedLevels.Length - 1);
                return cachedLevels[catalogIndex].levelId;
            }
            return catalogIndex + 1;
        }

        private int ResolveVisibleLevelIdAtPoolIndex(int poolIndex)
        {
            int catalogIndex = Mathf.FloorToInt(scrollOffset) + poolIndex;
            return ResolveLevelIdAtCatalogIndex(catalogIndex);
        }

        private int ResolveSelectedLevelId()
        {
            return SelectedLevelId;
        }

        private int FindCatalogIndexForLevelId(int levelId)
        {
            if (cachedLevels != null && cachedLevels.Length > 0)
            {
                for (int i = 0; i < cachedLevels.Length; i++)
                {
                    if (cachedLevels[i].levelId == levelId)
                        return i;
                }
                int nearest = 0;
                int minDist = Mathf.Abs(cachedLevels[0].levelId - levelId);
                for (int i = 1; i < cachedLevels.Length; i++)
                {
                    int dist = Mathf.Abs(cachedLevels[i].levelId - levelId);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        nearest = i;
                    }
                }
                return nearest;
            }
            return Mathf.Max(0, levelId - 1);
        }

        private void OnEnable()
        {
            GameEvents.EntryEnergyChanged += HandleTopBarEnergyChanged;
            GameEvents.SoftCurrencyChanged += HandleTopBarCurrencyChanged;
            GameEvents.LanguageChanged += HandleTopBarLanguageChanged;
        }

        private void OnDisable()
        {
            GameEvents.EntryEnergyChanged -= HandleTopBarEnergyChanged;
            GameEvents.SoftCurrencyChanged -= HandleTopBarCurrencyChanged;
            GameEvents.LanguageChanged -= HandleTopBarLanguageChanged;
        }

        private void HandleTopBarEnergyChanged(int _, int __) => RefreshTopBarMetrics();
        private void HandleTopBarCurrencyChanged(int _, int __) => RefreshTopBarMetrics();
        private void HandleTopBarLanguageChanged(string _) => RefreshTopBarMetrics();

        private void Start()
        {
            EnsureRuntimeDefaults();
            LoadCachedLevels();
            EnsureResumePrompt();

            if (!Application.isPlaying)
            {
                Refresh();
                return;
            }

            // PHASE 2: Do not overwrite rail points with node positions in Play mode
            // CaptureRailPointsFromNodes();

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

            RestoreScrollFromGameplay();
            Refresh();
            RefreshTopBarMetrics();
        }

        private void RestoreScrollFromGameplay()
        {
            if (pendingScrollToLevel < 1)
            {
                return;
            }

            int targetLevelId = pendingScrollToLevel;
            pendingScrollToLevel = -1;
            int catalogIndex = FindCatalogIndexForLevelId(targetLevelId);
            scrollOffset = Mathf.Clamp(catalogIndex, 0f, Mathf.Max(0f, totalLevels - 1f));
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
            int selectedLevel = SelectedLevelId;
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

            if (CanResumeSelectedLevel())
            {
                ShowResumePrompt();
                return;
            }

            bool started = SceneNavigator.Instance.OpenGameplayLevel(selectedLevel, true);

            if (started)
            {
                SetReturnScene();
                SetReturnLevel(selectedLevel);
            }

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
            int catalogIndex = Mathf.FloorToInt(scrollOffset) + poolIndex;
            if (catalogIndex < 0 || catalogIndex >= totalLevels)
            {
                return;
            }

            if (snapRoutine != null)
            {
                StopCoroutine(snapRoutine);
                snapRoutine = null;
            }

            snapRoutine = StartCoroutine(LerpScrollTo(catalogIndex));
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
                   session.levelId == SelectedLevelId &&
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
                int catalogIndex = baseLevelIndex + i;
                int levelId = ResolveLevelIdAtCatalogIndex(catalogIndex);
                bool visible = catalogIndex >= 0 && catalogIndex < totalLevels && slotT >= -0.6f && slotT < poolSize - 0.4f;

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
                float lockDim = 1f;
                SetNodeAlpha(node, label, EvalAlpha(slotT) * lockDim);

                if (label != null)
                {
                    label.text = levelId.ToString();
                }
            }

            if (oynaSubtitleLabel != null)
            {
                int selectedId = SelectedLevelId;
                if (Application.isPlaying)
                {
                    int highestForSubtitle = GetHighestUnlockedLevel();
                    if (selectedId > highestForSubtitle)
                    {
                        oynaSubtitleLabel.text = "Kilitli";
                    }
                    else if (CanResumeSelectedLevel())
                    {
                        oynaSubtitleLabel.text = "Devam Et / Bastan";
                    }
                    else
                    {
                        oynaSubtitleLabel.text = $"Seviye {selectedId}'den basla";
                    }
                }
                else
                {
                    oynaSubtitleLabel.text = $"Seviye {selectedId}'den basla";
                }
            }
        }

        private void RefreshTopBarMetrics()
        {
            if (!Application.isPlaying) return;

            if (topBarEnergyLabel != null && EnergyManager.Instance != null)
            {
                topBarEnergyLabel.text =
                    $"{EnergyManager.Instance.CurrentEnergy}/{EnergyManager.Instance.MaxEnergy}";
            }

            if (topBarHintLabel != null && EconomyManager.Instance != null)
            {
                topBarHintLabel.text = EconomyManager.Instance.Hints.ToString();
            }

            if (topBarCoinLabel != null && EconomyManager.Instance != null)
            {
                topBarCoinLabel.text = EconomyManager.Instance.SoftCurrency.ToString();
            }

            if (topBarLanguageLabel != null)
            {
                topBarLanguageLabel.text = CurrentLanguageCode().ToUpperInvariant();
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

        private void CaptureRailPointsFromNodes()
        {
            if (railPoints == null || levelNodes == null)
            {
                return;
            }

            int count = Mathf.Min(railPoints.Length, levelNodes.Length);
            for (int i = 0; i < count; i++)
            {
                RectTransform node = levelNodes[i];
                if (node == null)
                {
                    continue;
                }

                railPoints[i].position = node.anchoredPosition;
            }
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

        private void LoadCachedLevels()
        {
            if (!Application.isPlaying || ContentService.Instance == null)
            {
                return;
            }

            LevelCatalog catalog = ContentService.Instance.LoadLevels();
            LevelDefinition[] levels = catalog != null ? catalog.levels : null;
            if (levels != null && levels.Length > 0)
            {
                cachedLevels = levels.OrderBy(l => l.levelId).ToArray();
                totalLevels = cachedLevels.Length;
            }
        }

        private void EnsureResumePrompt()
        {
            if (resumePromptRoot != null)
            {
                resumePromptRoot.SetActive(false);
                return;
            }

            if (!Application.isPlaying)
            {
                return;
            }

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            Transform promptParent = parentCanvas != null ? parentCanvas.transform : transform;

            resumePromptRoot = new GameObject("ResumePromptOverlay", typeof(RectTransform), typeof(Image));
            resumePromptRoot.transform.SetParent(promptParent, false);
            resumePromptRoot.transform.SetAsLastSibling();
            RectTransform overlayRect = resumePromptRoot.GetComponent<RectTransform>();
            overlayRect.anchorMin = Vector2.zero;
            overlayRect.anchorMax = Vector2.one;
            overlayRect.offsetMin = Vector2.zero;
            overlayRect.offsetMax = Vector2.zero;
            Image overlayImage = resumePromptRoot.GetComponent<Image>();
            overlayImage.color = new Color(0.03f, 0.06f, 0.10f, 0.78f);

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

            PromptLabel("Title", panel.transform, new Vector2(0f, 100f), new Vector2(560f, 54f), 34f, FontStyles.Bold, Color.white, "Kayitli Ilerleme Bulundu");
            TextMeshProUGUI body = PromptLabel("Body", panel.transform, new Vector2(0f, 30f), new Vector2(580f, 120f), 24f, FontStyles.Normal, new Color(0.94f, 0.97f, 1f),
                "Bu seviyede kayitli ilerlemen var.\nDevam et dersen can harcanmaz.\nBastan baslatirsan yeni giris maliyeti uygulanir.");
            body.enableWordWrapping = true;

            resumePromptContinueButton = PromptButton("ContinueButton", panel.transform, new Vector2(0f, -60f), new Vector2(360f, 68f), new Color(0.26f, 0.55f, 0.89f, 1f), "Devam Et");
            resumePromptRestartButton = PromptButton("RestartButton", panel.transform, new Vector2(0f, -140f), new Vector2(360f, 68f), new Color(0.84f, 0.43f, 0.22f, 1f), "Bastan Basla");
            resumePromptCancelButton = PromptButton("CancelButton", panel.transform, new Vector2(0f, -200f), new Vector2(280f, 54f), new Color(0.18f, 0.24f, 0.31f, 1f), "Vazgec");

            resumePromptContinueButton.onClick.AddListener(OnResumeContinue);
            resumePromptRestartButton.onClick.AddListener(OnResumeRestart);
            resumePromptCancelButton.onClick.AddListener(OnResumeCancel);

            resumePromptRoot.SetActive(false);
        }

        private void ShowResumePrompt()
        {
            EnsureResumePrompt();
            if (resumePromptRoot != null)
            {
                resumePromptRoot.SetActive(true);
            }
        }

        private void HideResumePrompt()
        {
            if (resumePromptRoot != null)
            {
                resumePromptRoot.SetActive(false);
            }
        }

        private void OnResumeContinue()
        {
            HideResumePrompt();
            if (SceneNavigator.Instance != null && CanResumeSelectedLevel())
            {
                SetReturnScene();
                SetReturnLevel(SelectedLevelId);
                SceneNavigator.Instance.OpenGameplayForProgress();
            }
        }

        private void OnResumeRestart()
        {
            HideResumePrompt();
            if (SceneNavigator.Instance != null)
            {
                SetReturnScene();
                SetReturnLevel(SelectedLevelId);
                SceneNavigator.Instance.OpenGameplayLevel(SelectedLevelId, true);
            }
        }

        private void OnResumeCancel()
        {
            HideResumePrompt();
        }

        private static TextMeshProUGUI PromptLabel(string name, Transform parent, Vector2 pos, Vector2 size, float fontSize, FontStyles style, Color color, string text)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = TextAlignmentOptions.Center;
            label.color = color;
            label.raycastTarget = false;
            return label;
        }

        private static Button PromptButton(string name, Transform parent, Vector2 pos, Vector2 size, Color bgColor, string text)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = pos;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.type = Image.Type.Simple;
            image.color = bgColor;
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
            label.text = text;
            label.fontSize = 24f;
            label.fontStyle = FontStyles.Bold;
            label.alignment = TextAlignmentOptions.Center;
            label.color = Color.white;
            label.raycastTarget = false;

            return button;
        }

        public void OpenJourneyTab() { SwitchTab(HubPreviewTab.Journey); }
        public void OpenMissionsTab() { SwitchTab(HubPreviewTab.Missions); }
        public void OpenProfileTab() { SwitchTab(HubPreviewTab.Profile); }
        public void OpenStoreTab() { SwitchTab(HubPreviewTab.Store); }

        private void SwitchTab(HubPreviewTab target)
        {
            if (activeTab == target)
            {
                return;
            }

            activeTab = target;
            EnsureTabVisibility(activeTab);
        }

        private void EnsureTabVisibility(HubPreviewTab tab)
        {
            bool isJourney = tab == HubPreviewTab.Journey;

            if (alttasRoot != null) alttasRoot.SetActive(isJourney);
            if (pathContainerRoot != null) pathContainerRoot.SetActive(isJourney);
            if (oynaBgRoot != null) oynaBgRoot.SetActive(isJourney);

            if (missionsPlaceholder != null)
            {
                missionsPlaceholder.SetActive(tab == HubPreviewTab.Missions);
                if (tab == HubPreviewTab.Missions) EnsureBackButton(missionsPlaceholder);
            }
            if (profilePlaceholder != null)
            {
                profilePlaceholder.SetActive(tab == HubPreviewTab.Profile);
                if (tab == HubPreviewTab.Profile) EnsureBackButton(profilePlaceholder);
            }
            if (storePlaceholder != null)
            {
                storePlaceholder.SetActive(tab == HubPreviewTab.Store);
                if (tab == HubPreviewTab.Store) EnsureBackButton(storePlaceholder);
            }
        }

        private void EnsureBackButton(GameObject placeholder)
        {
            if (placeholder == null) return;

            Transform existing = placeholder.transform.Find("BackButton");
            if (existing != null) return;

            Button backBtn = PromptButton("BackButton", placeholder.transform,
                new Vector2(0f, -220f), new Vector2(280f, 64f),
                new Color(0.26f, 0.55f, 0.89f, 1f), "< Geri");
            backBtn.onClick.AddListener(() => SwitchTab(HubPreviewTab.Journey));
        }

        private static void SetReturnScene()
        {
            if (SceneNavigator.Instance == null) return;
            SceneNavigator.Instance.SetReturnSceneOverride(GameConstants.SceneHubPreview);
        }

        private static void SetReturnLevel(int levelId)
        {
            pendingScrollToLevel = levelId;
        }
    }
}
