using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class GameplayPausePresenter : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI bodyLabel;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button menuButton;

        private KeyboardPresenter _keyboardPresenter;

        private void Awake()
        {
            EnsureLayout();
            RefreshTexts();
            HideImmediate();
        }

        private void Start()
        {
            _keyboardPresenter = FindObjectOfType<KeyboardPresenter>(true);
        }

        private void OnEnable()
        {
            GameEvents.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
            GameEvents.LanguageChanged -= HandleLanguageChanged;
        }

        public void OpenPause()
        {
            if (root == null || !CanOpenPause())
            {
                return;
            }

            root.SetActive(true);
            Time.timeScale = 0f;
            InputManager.Instance?.SetGameplayInputActive(false);
            _keyboardPresenter?.Build();
        }

        public void ResumeGameplay()
        {
            HideImmediate();
            Time.timeScale = 1f;
            InputManager.Instance?.SetGameplayInputActive(true);
            _keyboardPresenter?.Build();
        }

        public void ReturnToHub()
        {
            SessionManager.Instance?.TakeSnapshot();
            SaveManager.Instance?.FlushNow();
            HideImmediate();
            Time.timeScale = 1f;
            InputManager.Instance?.SetGameplayInputActive(true);
            SceneNavigator.Instance?.OpenMainMenu();
        }

        public static GameplayPausePresenter EnsureInScene()
        {
            GameplayPausePresenter existing = FindObjectOfType<GameplayPausePresenter>(true);
            if (existing != null)
            {
                return existing;
            }

            Canvas canvas = FindObjectOfType<Canvas>(true);
            if (canvas == null)
            {
                return null;
            }

            GameObject host = new GameObject("RuntimeGameplayPausePresenter");
            host.transform.SetParent(canvas.transform, false);
            return host.AddComponent<GameplayPausePresenter>();
        }

        private bool CanOpenPause()
        {
            return InputManager.Instance == null || InputManager.Instance.CanAcceptGameplayInput;
        }

        private void EnsureLayout()
        {
            if (root != null)
            {
                WireButtons();
                return;
            }

            Canvas parentCanvas = GetComponentInParent<Canvas>();
            Transform parent = parentCanvas != null ? parentCanvas.transform : transform;

            root = new GameObject("PauseOverlay", typeof(RectTransform), typeof(Image));
            root.transform.SetParent(parent, false);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.offsetMin = Vector2.zero;
            rootRect.offsetMax = Vector2.zero;
            Image rootImage = root.GetComponent<Image>();
            rootImage.color = new Color(0.03f, 0.04f, 0.08f, 0.78f);

            GameObject panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            RectTransform panelRect = panel.GetComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(560f, 320f);
            panelRect.anchoredPosition = Vector2.zero;
            Image panelImage = panel.GetComponent<Image>();
            panelImage.color = new Color(0.10f, 0.15f, 0.22f, 0.97f);

            GameObject accent = new GameObject("Accent", typeof(RectTransform), typeof(Image));
            RectTransform accentRect = accent.GetComponent<RectTransform>();
            accentRect.SetParent(panel.transform, false);
            accentRect.anchorMin = new Vector2(0.5f, 0.88f);
            accentRect.anchorMax = new Vector2(0.5f, 0.88f);
            accentRect.pivot = new Vector2(0.5f, 0.5f);
            accentRect.sizeDelta = new Vector2(340f, 62f);
            Image accentImage = accent.GetComponent<Image>();
            accentImage.color = new Color(0.95f, 0.56f, 0.26f, 0.96f);

            titleLabel = CreateLabel("Title", panel.transform, new Vector2(0.5f, 0.72f), new Vector2(420f, 42f), 34f, FontStyles.Bold);
            bodyLabel = CreateLabel("Body", panel.transform, new Vector2(0.5f, 0.54f), new Vector2(460f, 96f), 22f, FontStyles.Normal);
            bodyLabel.enableWordWrapping = true;
            bodyLabel.color = new Color(0.92f, 0.97f, 1f, 1f);
            continueButton = CreateButton("ContinueButton", panel.transform, new Vector2(0.5f, 0.30f), new Vector2(300f, 64f), new Color(0.26f, 0.55f, 0.89f, 1f), "Devam Et");
            menuButton = CreateButton("MenuButton", panel.transform, new Vector2(0.5f, 0.10f), new Vector2(300f, 64f), new Color(0.36f, 0.22f, 0.18f, 1f), "Ana Menu");
            WireButtons();
        }

        private void WireButtons()
        {
            if (continueButton != null)
            {
                continueButton.onClick.RemoveAllListeners();
                continueButton.onClick.AddListener(ResumeGameplay);
            }

            if (menuButton != null)
            {
                menuButton.onClick.RemoveAllListeners();
                menuButton.onClick.AddListener(ReturnToHub);
            }
        }

        private void HideImmediate()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void HandleLanguageChanged(string _)
        {
            RefreshTexts();
        }

        private void RefreshTexts()
        {
            if (titleLabel != null)
            {
                titleLabel.text = GetLocalized("pause_title");
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = GetLocalized("pause_body");
            }

            SetButtonLabel(continueButton, GetLocalized("continue"));
            SetButtonLabel(menuButton, GetLocalized("menu"));
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, Vector2 anchor, Vector2 size, float fontSize, FontStyles style)
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
            label.color = Color.white;
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
            image.color = color;
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            TextMeshProUGUI label = CreateLabel("Label", go.transform, new Vector2(0.5f, 0.5f), new Vector2(size.x - 20f, 36f), 28f, FontStyles.Bold);
            label.text = text;
            return button;
        }

        private static void SetButtonLabel(Button button, string text)
        {
            if (button == null)
            {
                return;
            }

            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = text;
            }
        }

        private static string GetLocalized(string key)
        {
            string language = SaveManager.Instance != null
                ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode)
                : GameConstants.DefaultLanguageCode;

            return language switch
            {
                "en" => key switch
                {
                    "pause_title" => "Pause",
                    "pause_body" => "Your progress is kept. Continue now or return to the main hub.",
                    "continue" => "Continue",
                    "menu" => "Main Hub",
                    _ => key
                },
                "es" => key switch
                {
                    "pause_title" => "Pausa",
                    "pause_body" => "Tu progreso se guarda. Puedes continuar o volver al centro.",
                    "continue" => "Continuar",
                    "menu" => "Centro",
                    _ => key
                },
                "de" => key switch
                {
                    "pause_title" => "Pause",
                    "pause_body" => "Dein Fortschritt bleibt erhalten. Du kannst fortsetzen oder in den Hub gehen.",
                    "continue" => "Weiter",
                    "menu" => "Hub",
                    _ => key
                },
                _ => key switch
                {
                    "pause_title" => "Duraklatildi",
                    "pause_body" => "Ilerlemen kaydedilir. Oyuna devam edebilir veya ana merkeze donebilirsin.",
                    "continue" => "Devam Et",
                    "menu" => "Ana Merkez",
                    _ => key
                }
            };
        }
    }
}
