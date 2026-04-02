using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordSpinAlpha.Core;
using System.Collections.Generic;
using System;

namespace WordSpinAlpha.Presentation
{
    public class KeyboardPresenter : MonoBehaviour
    {
        [SerializeField] private KeyboardLayoutTuningProfile tuningProfile;
        [SerializeField] private Transform container;
        [SerializeField] private Button keyPrefab;
        [SerializeField] private float horizontalPadding = 20f;
        [SerializeField] private float topPadding = 8f;
        [SerializeField] private float bottomPadding = 8f;
        [SerializeField] private float columnSpacing = 2f;
        [SerializeField] private float rowSpacing = 6f;
        [SerializeField] private float maxButtonWidth = 72f;
        [SerializeField] private float maxButtonHeight = 86f;
        [SerializeField] private float minButtonWidth = 48f;
        [SerializeField] private float minButtonHeight = 42f;
        [SerializeField] private float buttonAspectRatio = 0.88f;
        [SerializeField] private float minLabelFontSize = 18f;
        [SerializeField] private float maxLabelFontSize = 26f;
        [SerializeField] private float germanHorizontalPaddingBoost = 24f;
        [SerializeField] private float germanMaxButtonWidth = 66f;

        private readonly List<Button> _builtButtons = new List<Button>();
        private Button _consumedButton;
        private Vector2 _lastContainerSize = Vector2.zero;
        private KeyboardLayoutTuningProfile _loadedProfile;

        private void Start()
        {
            Build();
        }

        private void OnEnable()
        {
            GameEvents.PinReleased += RestoreConsumedKey;
            GameEvents.QuestionFailed += RestoreConsumedKey;
            GameEvents.LevelCompleted += HandleLevelCompleted;
            GameEvents.LanguageChanged += HandleLanguageChanged;
            Build();
        }

        private void OnDisable()
        {
            GameEvents.PinReleased -= RestoreConsumedKey;
            GameEvents.QuestionFailed -= RestoreConsumedKey;
            GameEvents.LevelCompleted -= HandleLevelCompleted;
            GameEvents.LanguageChanged -= HandleLanguageChanged;
        }

        private void Update()
        {
            RectTransform rootRect = container as RectTransform;
            if (rootRect == null)
            {
                return;
            }

            Vector2 currentSize = rootRect.rect.size;
            if (currentSize.x <= 0f || currentSize.y <= 0f)
            {
                return;
            }

            if (Vector2.Distance(currentSize, _lastContainerSize) > 0.5f)
            {
                Build();
            }
        }

        public void Build()
        {
            if (container == null || keyPrefab == null || InputManager.Instance == null)
            {
                return;
            }

            foreach (Transform child in container)
            {
                Destroy(child.gameObject);
            }

            _builtButtons.Clear();
            _consumedButton = null;

            string languageCode = ResolveLanguageCode();
            string[] keys = InputManager.Instance.GetLayout(languageCode);
            BuildRows(languageCode, keys);

            RectTransform rootRect = container as RectTransform;
            if (rootRect != null)
            {
                _lastContainerSize = rootRect.rect.size;
            }
        }

        private void HandleKeyPressed(Button button, char pressed)
        {
            if (_consumedButton != null)
            {
                return;
            }

            _consumedButton = button;
            SetButtonVisual(button, false);
            InputManager.Instance.ProcessLetterButton(pressed, ResolveButtonScreenPosition(button));
        }

        private Vector3 ResolveButtonScreenPosition(Button button)
        {
            if (button == null)
            {
                return Vector3.zero;
            }

            RectTransform rect = button.transform as RectTransform;
            if (rect == null)
            {
                return button.transform.position;
            }

            Canvas canvas = button.GetComponentInParent<Canvas>();
            Camera eventCamera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? canvas.worldCamera
                : null;

            return RectTransformUtility.WorldToScreenPoint(eventCamera, rect.TransformPoint(rect.rect.center));
        }

        private void RestoreConsumedKey()
        {
            if (_consumedButton == null)
            {
                return;
            }

            SetButtonVisual(_consumedButton, true);
            _consumedButton = null;
        }

        private void HandleLevelCompleted(LevelContext context)
        {
            RestoreConsumedKey();
        }

        private void HandleLanguageChanged(string _)
        {
            RestoreConsumedKey();
            Build();
        }

        private static string ResolveLanguageCode()
        {
            return SaveManager.Instance != null
                ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode)
                : GameConstants.DefaultLanguageCode;
        }

        private void BuildRows(string languageCode, string[] keys)
        {
            RectTransform rootRect = container as RectTransform;
            if (rootRect == null)
            {
                return;
            }

            GridLayoutGroup grid = container.GetComponent<GridLayoutGroup>();
            if (grid != null)
            {
                grid.enabled = false;
            }

            string[][] rows = SplitRows(languageCode, keys);
            KeyboardLayoutTuningProfile.LanguageTuning tuning = ResolveLanguageTuning(languageCode);
            float buttonWidth;
            float buttonHeight;
            ResolveResponsiveButtonSize(rootRect, rows, tuning, languageCode, out buttonWidth, out buttonHeight);

            int rowCount = 0;
            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] != null && rows[i].Length > 0)
                {
                    rowCount++;
                }
            }

            if (rowCount == 0)
            {
                return;
            }

            float resolvedColumnSpacing = tuning != null ? tuning.columnSpacing : columnSpacing;
            float resolvedRowSpacing = tuning != null ? tuning.rowSpacing : rowSpacing;
            float resolvedMinLabelFontSize = tuning != null ? tuning.minLabelFontSize : minLabelFontSize;
            float resolvedMaxLabelFontSize = tuning != null ? tuning.maxLabelFontSize : maxLabelFontSize;

            float blockHeight = (rowCount * buttonHeight) + ((rowCount - 1) * resolvedRowSpacing);
            float topY = (blockHeight * 0.5f) - (buttonHeight * 0.5f);

            for (int rowIndex = 0; rowIndex < rows.Length; rowIndex++)
            {
                string[] rowKeys = rows[rowIndex];
                if (rowKeys == null || rowKeys.Length == 0)
                {
                    continue;
                }

                float totalWidth = (rowKeys.Length * buttonWidth) + ((rowKeys.Length - 1) * resolvedColumnSpacing);
                float startX = -(totalWidth * 0.5f) + (buttonWidth * 0.5f);
                float y = topY - rowIndex * (buttonHeight + resolvedRowSpacing);

                for (int keyIndex = 0; keyIndex < rowKeys.Length; keyIndex++)
                {
                    string key = rowKeys[keyIndex];
                    if (string.IsNullOrEmpty(key))
                    {
                        continue;
                    }

                    Button button = Instantiate(keyPrefab, container);
                    button.name = $"Key_{key}";

                    RectTransform buttonRect = button.transform as RectTransform;
                    if (buttonRect != null)
                    {
                        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
                        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
                        buttonRect.pivot = new Vector2(0.5f, 0.5f);
                        buttonRect.anchoredPosition = new Vector2(startX + keyIndex * (buttonWidth + resolvedColumnSpacing), y);
                        buttonRect.sizeDelta = new Vector2(buttonWidth, buttonHeight);
                    }

                    TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (label != null)
                    {
                        label.text = key;
                        label.enableAutoSizing = true;
                        label.fontSizeMin = resolvedMinLabelFontSize;
                        label.fontSizeMax = Mathf.Clamp(buttonHeight * 0.46f, resolvedMinLabelFontSize, resolvedMaxLabelFontSize);
                        RectTransform labelRect = label.rectTransform;
                        if (labelRect != null)
                        {
                            labelRect.sizeDelta = new Vector2(Mathf.Max(18f, buttonWidth - 10f), Mathf.Max(18f, buttonHeight - 8f));
                        }
                    }

                    char pressed = char.ToUpperInvariant(key[0]);
                    button.onClick.AddListener(() => HandleKeyPressed(button, pressed));
                    _builtButtons.Add(button);
                }
            }
        }

        private void ResolveResponsiveButtonSize(RectTransform rootRect, string[][] rows, KeyboardLayoutTuningProfile.LanguageTuning tuning, string languageCode, out float buttonWidth, out float buttonHeight)
        {
            int maxRowLength = 0;
            int activeRowCount = 0;

            for (int i = 0; i < rows.Length; i++)
            {
                if (rows[i] == null || rows[i].Length == 0)
                {
                    continue;
                }

                activeRowCount++;
                if (rows[i].Length > maxRowLength)
                {
                    maxRowLength = rows[i].Length;
                }
            }

            maxRowLength = Mathf.Max(1, maxRowLength);
            activeRowCount = Mathf.Max(1, activeRowCount);

            float resolvedHorizontalPadding = horizontalPadding;
            float resolvedTopPadding = topPadding;
            float resolvedBottomPadding = bottomPadding;
            float resolvedColumnSpacing = columnSpacing;
            float resolvedRowSpacing = rowSpacing;
            float resolvedMaxButtonWidth = maxButtonWidth;
            float resolvedMaxButtonHeight = maxButtonHeight;
            float resolvedMinButtonWidth = minButtonWidth;
            float resolvedMinButtonHeight = minButtonHeight;
            float resolvedAspectRatio = buttonAspectRatio;

            if (tuning != null)
            {
                resolvedHorizontalPadding = tuning.horizontalPadding;
                resolvedTopPadding = tuning.topPadding;
                resolvedBottomPadding = tuning.bottomPadding;
                resolvedColumnSpacing = tuning.columnSpacing;
                resolvedRowSpacing = tuning.rowSpacing;
                resolvedMaxButtonWidth = tuning.maxButtonWidth;
                resolvedMaxButtonHeight = tuning.maxButtonHeight;
                resolvedMinButtonWidth = tuning.minButtonWidth;
                resolvedMinButtonHeight = tuning.minButtonHeight;
                resolvedAspectRatio = tuning.buttonAspectRatio;
            }
            else if (GameConstants.NormalizeLanguageCode(languageCode) == "de")
            {
                resolvedHorizontalPadding += germanHorizontalPaddingBoost;
                resolvedMaxButtonWidth = Mathf.Min(resolvedMaxButtonWidth, germanMaxButtonWidth);
            }

            float availableWidth = Mathf.Max(1f, rootRect.rect.width - resolvedHorizontalPadding - resolvedHorizontalPadding);
            float availableHeight = Mathf.Max(1f, rootRect.rect.height - resolvedTopPadding - resolvedBottomPadding);
            float widthByWidth = (availableWidth - ((maxRowLength - 1) * resolvedColumnSpacing)) / maxRowLength;
            float maxHeightByRows = (availableHeight - ((activeRowCount - 1) * resolvedRowSpacing)) / activeRowCount;
            float widthByHeight = maxHeightByRows * Mathf.Max(0.1f, resolvedAspectRatio);

            buttonWidth = Mathf.Min(resolvedMaxButtonWidth, widthByWidth, widthByHeight);
            buttonWidth = Mathf.Max(resolvedMinButtonWidth, buttonWidth);

            buttonHeight = buttonWidth / Mathf.Max(0.1f, resolvedAspectRatio);
            if (buttonHeight > maxHeightByRows)
            {
                buttonHeight = maxHeightByRows;
                buttonWidth = buttonHeight * Mathf.Max(0.1f, resolvedAspectRatio);
            }

            buttonHeight = Mathf.Clamp(buttonHeight, resolvedMinButtonHeight, resolvedMaxButtonHeight);
            buttonWidth = Mathf.Clamp(buttonWidth, resolvedMinButtonWidth, resolvedMaxButtonWidth);
        }

        private KeyboardLayoutTuningProfile.LanguageTuning ResolveLanguageTuning(string languageCode)
        {
            KeyboardLayoutTuningProfile profile = ResolveProfile();
            return profile != null ? profile.GetLanguageTuning(languageCode) : null;
        }

        private KeyboardLayoutTuningProfile ResolveProfile()
        {
            if (tuningProfile != null)
            {
                return tuningProfile;
            }

            if (_loadedProfile == null)
            {
                _loadedProfile = Resources.Load<KeyboardLayoutTuningProfile>(KeyboardLayoutTuningProfile.DefaultResourcePath);
            }

            return _loadedProfile;
        }

        private static string[][] SplitRows(string languageCode, string[] keys)
        {
            int[] rowLengths = GetRowLengths(languageCode, keys != null ? keys.Length : 0);
            List<string[]> rows = new List<string[]>(rowLengths.Length);
            int offset = 0;

            for (int i = 0; i < rowLengths.Length; i++)
            {
                int remaining = Mathf.Max(0, (keys != null ? keys.Length : 0) - offset);
                int count = Mathf.Min(rowLengths[i], remaining);
                string[] row = new string[count];
                if (count > 0)
                {
                    Array.Copy(keys, offset, row, 0, count);
                }

                rows.Add(row);
                offset += count;
            }

            return rows.ToArray();
        }

        private static int[] GetRowLengths(string languageCode, int keyCount)
        {
            switch (GameConstants.NormalizeLanguageCode(languageCode))
            {
                case "tr":
                    return new[] { 12, 11, 9 };
                case "es":
                    return new[] { 10, 10, 7 };
                case "de":
                    return new[] { 11, 11, 7 };
                case "en":
                default:
                    if (keyCount <= 26)
                    {
                        return new[] { 10, 9, 7 };
                    }

                    return new[] { 10, 10, Mathf.Max(0, keyCount - 20) };
            }
        }

        private static void SetButtonVisual(Button button, bool visible)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                Color color = image.color;
                color.a = visible ? 0.96f : 0.18f;
                image.color = color;
            }

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = visible ? button.name.Replace("Key_", string.Empty) : string.Empty;
            }
        }
    }
}
