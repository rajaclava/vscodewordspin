using System;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Presentation
{
    public class MainMenuPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI energyLabel;
        [SerializeField] private TextMeshProUGUI hintLabel;
        [SerializeField] private GameObject levelSelectRoot;
        [SerializeField] private Transform levelButtonContainer;
        [SerializeField] private Button levelButtonTemplate;
        [SerializeField] private TMP_InputField levelJumpInput;
        [SerializeField] private TextMeshProUGUI levelSelectSummaryLabel;
        [SerializeField] private TextMeshProUGUI languageLabel;

        private LevelDefinition[] _cachedLevels = Array.Empty<LevelDefinition>();
        private GameObject _languageChangeDialogRoot;
        private TextMeshProUGUI _languageChangeTitleLabel;
        private TextMeshProUGUI _languageChangeBodyLabel;
        private TextMeshProUGUI _languageChangeConfirmLabel;
        private TextMeshProUGUI _languageChangeCancelLabel;
        private string _pendingLanguageCode;

        private void OnEnable()
        {
            GameEvents.EntryEnergyChanged += HandleEnergyChanged;
            GameEvents.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            GameEvents.EntryEnergyChanged -= HandleEnergyChanged;
            GameEvents.LanguageChanged -= HandleLanguageChanged;
        }

        private void Start()
        {
            EnsureLanguageChangeDialog();
            RefreshLanguageLabel();
            RefreshLocalizedTexts();
            RefreshHints();

            if (EnergyManager.Instance != null)
            {
                HandleEnergyChanged(EnergyManager.Instance.CurrentEnergy, EnergyManager.Instance.MaxEnergy);
            }

            BuildLevelButtons();
            if (levelSelectRoot != null)
            {
                levelSelectRoot.SetActive(false);
            }
        }

        public void StartCurrentProgressLevel()
        {
            SceneNavigator.Instance?.OpenGameplayForProgress();
        }

        public void OpenStore()
        {
            SceneNavigator.Instance?.OpenStore();
        }

        public void SetLanguageTR() => PromptLanguageChange(GameConstants.DefaultLanguageCode);
        public void SetLanguageEN() => PromptLanguageChange("en");
        public void SetLanguageES() => PromptLanguageChange("es");
        public void SetLanguageDE() => PromptLanguageChange("de");

        public void OpenLevelSelect()
        {
            BuildLevelButtons();
            if (levelSelectRoot != null)
            {
                levelSelectRoot.SetActive(true);
            }
        }

        public void CloseLevelSelect()
        {
            if (levelSelectRoot != null)
            {
                levelSelectRoot.SetActive(false);
            }
        }

        public void OpenTypedLevel()
        {
            if (levelJumpInput == null || string.IsNullOrWhiteSpace(levelJumpInput.text))
            {
                return;
            }

            if (!int.TryParse(levelJumpInput.text.Trim(), out int levelId))
            {
                UpdateLevelSelectSummary(GetLocalized("invalid_level"));
                return;
            }

            OpenSpecificLevel(levelId);
        }

        public void RefreshHints()
        {
            if (hintLabel != null)
            {
                hintLabel.text = EconomyManager.Instance != null
                    ? $"{GetLocalized("hints")}: {EconomyManager.Instance.Hints}"
                    : $"{GetLocalized("hints")}: 0";
            }
        }

        private void HandleEnergyChanged(int current, int max)
        {
            if (energyLabel != null)
            {
                energyLabel.text = $"{GetLocalized("energy")}: {current}/{max}";
            }
        }

        private void BuildLevelButtons()
        {
            if (levelButtonContainer == null || levelButtonTemplate == null)
            {
                return;
            }

            LevelDefinition[] levels = ContentService.Instance != null
                ? (ContentService.Instance.LoadLevels().levels ?? Array.Empty<LevelDefinition>()).OrderBy(level => level.levelId).ToArray()
                : Array.Empty<LevelDefinition>();

            if (levels.Length == 0)
            {
                UpdateLevelSelectSummary(GetLocalized("no_levels"));
                return;
            }

            bool requiresRebuild = _cachedLevels.Length != levels.Length;
            if (!requiresRebuild)
            {
                for (int i = 0; i < levels.Length; i++)
                {
                    if (_cachedLevels[i].levelId != levels[i].levelId)
                    {
                        requiresRebuild = true;
                        break;
                    }
                }
            }

            _cachedLevels = levels;
            UpdateLevelSelectSummary(BuildSummaryText(levels.Length));

            if (!requiresRebuild)
            {
                return;
            }

            foreach (Transform child in levelButtonContainer)
            {
                if (child != levelButtonTemplate.transform)
                {
                    Destroy(child.gameObject);
                }
            }

            levelButtonTemplate.gameObject.SetActive(false);
            for (int i = 0; i < levels.Length; i++)
            {
                LevelDefinition level = levels[i];
                Button levelButton = Instantiate(levelButtonTemplate, levelButtonContainer);
                levelButton.gameObject.name = $"LevelButton_{level.levelId}";
                levelButton.gameObject.SetActive(true);
                levelButton.onClick.RemoveAllListeners();
                int capturedLevelId = level.levelId;
                levelButton.onClick.AddListener(() => OpenSpecificLevel(capturedLevelId));

                TextMeshProUGUI label = levelButton.GetComponentInChildren<TextMeshProUGUI>(true);
                if (label != null)
                {
                    label.text = $"{GetLocalized("level")} {capturedLevelId}";
                }
            }
        }

        private void OpenSpecificLevel(int levelId)
        {
            if (_cachedLevels.Length == 0)
            {
                BuildLevelButtons();
            }

            if (!_cachedLevels.Any(level => level.levelId == levelId))
            {
                UpdateLevelSelectSummary($"{GetLocalized("level")} {levelId} {GetLocalized("not_found_suffix")}");
                return;
            }

            CloseLevelSelect();
            SceneNavigator.Instance?.OpenGameplayLevel(levelId, false);
        }

        private string BuildSummaryText(int totalLevelCount)
        {
            int highestUnlocked = SaveManager.Instance != null ? SaveManager.Instance.Data.progress.highestUnlockedLevel : 1;
            return $"{GetLocalized("total")} {totalLevelCount} {GetLocalized("level")} • {GetLocalized("progress")}: {highestUnlocked}";
        }

        private void UpdateLevelSelectSummary(string text)
        {
            if (levelSelectSummaryLabel != null)
            {
                levelSelectSummaryLabel.text = text;
            }
        }

        private void SetLanguage(string languageCode)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            string normalized = GameConstants.NormalizeLanguageCode(languageCode);
            SaveManager.Instance.Data.languageCode = normalized;
            SaveManager.Instance.Save();
            ContentService.Instance?.RefreshLanguageContext();
            InputManager.Instance?.RefreshKeyboardConfig();
            RefreshLanguageLabel();
            RefreshLocalizedTexts();
            RefreshHints();
            if (EnergyManager.Instance != null)
            {
                HandleEnergyChanged(EnergyManager.Instance.CurrentEnergy, EnergyManager.Instance.MaxEnergy);
            }

            BuildLevelButtons();
            GameEvents.RaiseLanguageChanged(normalized);
        }

        private void RefreshLanguageLabel()
        {
            if (languageLabel == null)
            {
                return;
            }

            string normalized = SaveManager.Instance != null
                ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode).ToUpperInvariant()
                : GameConstants.DefaultLanguageCode.ToUpperInvariant();
            languageLabel.text = $"Dil: {normalized}";
        }

        private void HandleLanguageChanged(string _)
        {
            RefreshLanguageLabel();
            RefreshLocalizedTexts();
            RefreshLanguageChangeDialogTexts();
            RefreshHints();
            if (EnergyManager.Instance != null)
            {
                HandleEnergyChanged(EnergyManager.Instance.CurrentEnergy, EnergyManager.Instance.MaxEnergy);
            }

            BuildLevelButtons();
        }

        private void RefreshLocalizedTexts()
        {
            SetText("PlayButton/Label", GetLocalized("play"));
            SetText("LevelsButton/Label", GetLocalized("levels"));
            SetText("StoreButton/Label", GetLocalized("store"));
            SetText("LevelSelectOverlay/LevelSelectTitle", GetLocalized("level_select"));
            SetText("LevelSelectOverlay/CloseLevelsButton/Label", GetLocalized("close"));
            SetText("LevelSelectOverlay/JumpButton/Label", GetLocalized("go"));
            SetInputPlaceholder("LevelSelectOverlay/LevelJumpInput", GetLocalized("level_number"));
        }

        private void PromptLanguageChange(string languageCode)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            string normalized = GameConstants.NormalizeLanguageCode(languageCode);
            string current = GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode);
            if (string.Equals(normalized, current, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            _pendingLanguageCode = normalized;
            CloseLevelSelect();
            EnsureLanguageChangeDialog();
            RefreshLanguageChangeDialogTexts();
            if (_languageChangeDialogRoot != null)
            {
                _languageChangeDialogRoot.SetActive(true);
            }
        }

        private void ConfirmLanguageChange()
        {
            if (SaveManager.Instance == null || string.IsNullOrWhiteSpace(_pendingLanguageCode))
            {
                HideLanguageChangeDialog();
                return;
            }

            string normalized = GameConstants.NormalizeLanguageCode(_pendingLanguageCode);
            SaveManager.Instance.Data.languageCode = normalized;
            SaveManager.Instance.Data.session = new SessionSnapshot
            {
                languageCode = normalized
            };
            SaveManager.Instance.Save();

            ContentService.Instance?.RefreshLanguageContext();
            InputManager.Instance?.RefreshKeyboardConfig();
            GameEvents.RaiseLanguageChanged(normalized);

            _pendingLanguageCode = null;
            SceneManager.LoadScene(GameConstants.SceneMainMenu);
        }

        private void CancelLanguageChange()
        {
            _pendingLanguageCode = null;
            HideLanguageChangeDialog();
        }

        private void HideLanguageChangeDialog()
        {
            if (_languageChangeDialogRoot != null)
            {
                _languageChangeDialogRoot.SetActive(false);
            }
        }

        private void EnsureLanguageChangeDialog()
        {
            if (_languageChangeDialogRoot != null)
            {
                return;
            }

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            Transform parent = parentCanvas != null ? parentCanvas.transform : transform;

            _languageChangeDialogRoot = new GameObject("LanguageChangeDialog", typeof(RectTransform), typeof(Image));
            _languageChangeDialogRoot.transform.SetParent(parent, false);
            RectTransform rootRect = (RectTransform)_languageChangeDialogRoot.transform;
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            Image rootImage = _languageChangeDialogRoot.GetComponent<Image>();
            rootImage.color = new Color(0.02f, 0.04f, 0.08f, 0.78f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(_languageChangeDialogRoot.transform, false);
            RectTransform panelRect = (RectTransform)panel.transform;
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(580f, 280f);
            panelRect.anchoredPosition = Vector2.zero;
            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.1f, 0.15f, 0.22f, 0.96f);

            TMP_FontAsset font = languageLabel != null ? languageLabel.font : TMP_Settings.defaultFontAsset;
            _languageChangeTitleLabel = CreateDialogLabel("Title", panel.transform, font, 28f, FontStyles.Bold, TextAlignmentOptions.Center, new Vector2(0f, 82f), new Vector2(500f, 40f));
            _languageChangeBodyLabel = CreateDialogLabel("Body", panel.transform, font, 21f, FontStyles.Normal, TextAlignmentOptions.Midline, new Vector2(0f, 8f), new Vector2(500f, 92f));
            _languageChangeBodyLabel.enableWordWrapping = true;

            Button confirmButton = CreateDialogButton("ConfirmButton", panel.transform, font, new Vector2(-112f, -90f), new Vector2(190f, 56f), new Color(0.22f, 0.44f, 0.72f, 1f), out _languageChangeConfirmLabel);
            confirmButton.onClick.AddListener(ConfirmLanguageChange);
            Button cancelButton = CreateDialogButton("CancelButton", panel.transform, font, new Vector2(112f, -90f), new Vector2(190f, 56f), new Color(0.28f, 0.23f, 0.23f, 1f), out _languageChangeCancelLabel);
            cancelButton.onClick.AddListener(CancelLanguageChange);

            _languageChangeDialogRoot.SetActive(false);
            RefreshLanguageChangeDialogTexts();
        }

        private void RefreshLanguageChangeDialogTexts()
        {
            if (_languageChangeTitleLabel != null)
            {
                _languageChangeTitleLabel.text = GetLocalized("language_warning_title");
            }

            if (_languageChangeBodyLabel != null)
            {
                _languageChangeBodyLabel.text = GetLocalized("language_warning_body");
            }

            if (_languageChangeConfirmLabel != null)
            {
                _languageChangeConfirmLabel.text = GetLocalized("restart");
            }

            if (_languageChangeCancelLabel != null)
            {
                _languageChangeCancelLabel.text = GetLocalized("cancel");
            }
        }

        private static TextMeshProUGUI CreateDialogLabel(string name, Transform parent, TMP_FontAsset font, float fontSize, FontStyles style, TextAlignmentOptions alignment, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject labelObject = new GameObject(name, typeof(RectTransform));
            labelObject.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)labelObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            TextMeshProUGUI label = labelObject.AddComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.alignment = alignment;
            label.color = new Color(0.93f, 0.95f, 0.98f, 1f);
            return label;
        }

        private static Button CreateDialogButton(string name, Transform parent, TMP_FontAsset font, Vector2 anchoredPosition, Vector2 size, Color backgroundColor, out TextMeshProUGUI label)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            RectTransform rect = (RectTransform)buttonObject.transform;
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = anchoredPosition;

            Image image = buttonObject.GetComponent<Image>();
            image.color = backgroundColor;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = backgroundColor;
            colors.highlightedColor = backgroundColor * 1.08f;
            colors.pressedColor = backgroundColor * 0.92f;
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            label = CreateDialogLabel("Label", buttonObject.transform, font, 24f, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, size);
            return button;
        }

        private void SetText(string path, string value)
        {
            Transform child = transform.Find(path);
            if (child == null)
            {
                return;
            }

            TextMeshProUGUI label = child.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = value;
            }
        }

        private void SetInputPlaceholder(string path, string value)
        {
            Transform child = transform.Find(path + "/Placeholder");
            if (child == null)
            {
                return;
            }

            TextMeshProUGUI label = child.GetComponent<TextMeshProUGUI>();
            if (label != null)
            {
                label.text = value;
            }
        }

        private static string GetLocalized(string key)
        {
            string language = SaveManager.Instance != null ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode) : GameConstants.DefaultLanguageCode;
            switch (language)
            {
                case "en":
                    return key switch
                    {
                        "play" => "Play",
                        "levels" => "Levels",
                        "store" => "Store",
                        "level_select" => "Level Select",
                        "close" => "Close",
                        "go" => "Go",
                        "cancel" => "Cancel",
                        "restart" => "Restart",
                        "language_warning_title" => "Change language?",
                        "language_warning_body" => "Changing the language will restart the game. Your progress will be kept.",
                        "level_number" => "Level no",
                        "energy" => "Energy",
                        "hints" => "Hints",
                        "invalid_level" => "Invalid level number",
                        "no_levels" => "No level data found",
                        "level" => "Level",
                        "total" => "Total",
                        "progress" => "Progress",
                        "not_found_suffix" => "not found",
                        _ => key
                    };
                case "es":
                    return key switch
                    {
                        "play" => "Jugar",
                        "levels" => "Niveles",
                        "store" => "Tienda",
                        "level_select" => "Seleccion de nivel",
                        "close" => "Cerrar",
                        "go" => "Ir",
                        "cancel" => "Cancelar",
                        "restart" => "Reiniciar",
                        "language_warning_title" => "Cambiar idioma?",
                        "language_warning_body" => "Cambiar el idioma reiniciara el juego. Tu progreso se conserva.",
                        "level_number" => "Nivel",
                        "energy" => "Energia",
                        "hints" => "Pistas",
                        "invalid_level" => "Numero de nivel invalido",
                        "no_levels" => "No hay datos de nivel",
                        "level" => "Nivel",
                        "total" => "Total",
                        "progress" => "Progreso",
                        "not_found_suffix" => "no encontrado",
                        _ => key
                    };
                case "de":
                    return key switch
                    {
                        "play" => "Spielen",
                        "levels" => "Level",
                        "store" => "Shop",
                        "level_select" => "Levelwahl",
                        "close" => "Schliessen",
                        "go" => "Los",
                        "cancel" => "Abbrechen",
                        "restart" => "Neustart",
                        "language_warning_title" => "Sprache wechseln?",
                        "language_warning_body" => "Beim Sprachwechsel startet das Spiel neu. Dein Fortschritt bleibt erhalten.",
                        "level_number" => "Levelnr",
                        "energy" => "Energie",
                        "hints" => "Tipps",
                        "invalid_level" => "Ungueltige Levelnummer",
                        "no_levels" => "Keine Leveldaten gefunden",
                        "level" => "Level",
                        "total" => "Gesamt",
                        "progress" => "Fortschritt",
                        "not_found_suffix" => "nicht gefunden",
                        _ => key
                    };
                default:
                    return key switch
                    {
                        "play" => "Oyna",
                        "levels" => "Leveller",
                        "store" => "Magaza",
                        "level_select" => "Level Secimi",
                        "close" => "Kapat",
                        "go" => "Git",
                        "cancel" => "Vazgec",
                        "restart" => "Yeniden Baslat",
                        "language_warning_title" => "Dil degissin mi?",
                        "language_warning_body" => "Dil degisince oyun yeniden baslar. Ilerlemen sifirlanmaz.",
                        "level_number" => "Level no",
                        "energy" => "Enerji",
                        "hints" => "Ipucu",
                        "invalid_level" => "Gecersiz level numarasi",
                        "no_levels" => "Level verisi bulunamadi",
                        "level" => "Level",
                        "total" => "Toplam",
                        "progress" => "Acik ilerleme",
                        "not_found_suffix" => "bulunamadi",
                        _ => key
                    };
            }
        }
    }
}
