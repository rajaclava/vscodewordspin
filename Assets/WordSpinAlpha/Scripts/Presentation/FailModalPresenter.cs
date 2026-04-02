using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class FailModalPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI titleLabel;
        [SerializeField] private TextMeshProUGUI bodyLabel;
        [SerializeField] private TextMeshProUGUI energyLabel;
        [SerializeField] private Button continueButton;
        [SerializeField] private TextMeshProUGUI continueButtonLabel;
        [SerializeField] private Button retryButton;
        [SerializeField] private TextMeshProUGUI retryButtonLabel;
        [SerializeField] private Button premiumButton;
        [SerializeField] private TextMeshProUGUI premiumButtonLabel;

        private bool _buttonsHooked;

        private void Awake()
        {
            EnsureRuntimeUi();
            HideImmediate();
        }

        private void OnEnable()
        {
            GameEvents.FailModalRequested += HandleFailModalRequested;
            GameEvents.LevelStarted += HandleLevelStarted;
            GameEvents.MembershipChanged += HandleMembershipChanged;
            GameEvents.EntryEnergyChanged += HandleEntryEnergyChanged;
            HookButtons();
        }

        private void OnDisable()
        {
            GameEvents.FailModalRequested -= HandleFailModalRequested;
            GameEvents.LevelStarted -= HandleLevelStarted;
            GameEvents.MembershipChanged -= HandleMembershipChanged;
            GameEvents.EntryEnergyChanged -= HandleEntryEnergyChanged;
        }

        public void Continue()
        {
            bool premium = EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive;
            if (GameManager.Instance != null && GameManager.Instance.ContinueAfterFailure(premium))
            {
                HideImmediate();
            }
        }

        public void Retry()
        {
            if (GameManager.Instance != null && GameManager.Instance.RetryCurrentLevel())
            {
                HideImmediate();
                return;
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = "Tekrar dene icin giris enerjisi gerekli. Enerji bittiginde premium enerji bypass'i veya magaza paketi gerekir.";
            }
        }

        public void OpenPremiumStore()
        {
            HideImmediate();
            SceneNavigator.Instance?.OpenStore();
        }

        private void HandleFailModalRequested(FailModalContext context)
        {
            EnsureRuntimeUi();
            RefreshText();
            if (root != null)
            {
                root.SetActive(true);
            }
        }

        private void HandleLevelStarted(LevelContext context)
        {
            HideImmediate();
        }

        private void HandleMembershipChanged(bool active)
        {
            if (root != null && root.activeSelf)
            {
                RefreshText();
            }
        }

        private void HandleEntryEnergyChanged(int current, int max)
        {
            if (root != null && root.activeSelf)
            {
                RefreshText();
            }
        }

        private void RefreshText()
        {
            bool premium = EconomyManager.Instance != null && EconomyManager.Instance.PremiumMembershipActive;
            int currentEnergy = EnergyManager.Instance != null ? EnergyManager.Instance.CurrentEnergy : 0;
            int maxEnergy = EnergyManager.Instance != null ? EnergyManager.Instance.MaxEnergy : 0;
            bool canRetry = premium || currentEnergy > 0;

            if (titleLabel != null)
            {
                titleLabel.text = "Hata Hakki Bitti";
            }

            if (bodyLabel != null)
            {
                bodyLabel.text = premium
                    ? "Premium aktif. Reklam izlemeden 1 can ile kaldigin yerden devam edebilirsin."
                    : "Reklam izleyerek 1 can ile kaldigin yerden devam et. Premium uyelik alirsan bunu reklamsiz ve sinirsiz kullanirsin.";
            }

            if (energyLabel != null)
            {
                energyLabel.text = premium
                    ? "Enerji: Premium limitsiz giris"
                    : $"Enerji: {currentEnergy}/{maxEnergy}  |  Tekrar dene maliyeti: 1";
                energyLabel.color = canRetry ? new Color(1f, 0.83f, 0.63f) : new Color(0.96f, 0.44f, 0.34f);
            }

            if (continueButtonLabel != null)
            {
                continueButtonLabel.text = premium ? "Devam Et" : "Reklam Izle ve Devam Et";
            }

            if (retryButtonLabel != null)
            {
                retryButtonLabel.text = canRetry ? "Tekrar Dene" : "Enerji Yetersiz";
            }

            if (premiumButtonLabel != null)
            {
                premiumButtonLabel.text = premium ? "Magazayi Ac" : "Premium Al";
            }

            if (retryButton != null)
            {
                retryButton.interactable = canRetry;
            }
        }

        private void HideImmediate()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        private void HookButtons()
        {
            if (_buttonsHooked)
            {
                return;
            }

            EnsureRuntimeUi();
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(Continue);
            }

            if (retryButton != null)
            {
                retryButton.onClick.AddListener(Retry);
            }

            if (premiumButton != null)
            {
                premiumButton.onClick.AddListener(OpenPremiumStore);
            }

            _buttonsHooked = true;
        }

        private void EnsureRuntimeUi()
        {
            if (root != null && titleLabel != null && bodyLabel != null && energyLabel != null && continueButton != null && retryButton != null && premiumButton != null)
            {
                BindButtonLabels();
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                canvas = FindObjectOfType<Canvas>();
            }

            if (canvas == null)
            {
                return;
            }

            if (root == null)
            {
                root = CreatePanel("FailModal", canvas.transform, new Vector2(760f, 380f), new Color(0.10f, 0.09f, 0.12f, 0.96f));
            }

            RectTransform rootRect = root.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.anchorMin = new Vector2(0.5f, 0.5f);
                rootRect.anchorMax = new Vector2(0.5f, 0.5f);
                rootRect.anchoredPosition = new Vector2(0f, 20f);
            }

            Image blocker = root.GetComponent<Image>();
            if (blocker != null)
            {
                blocker.raycastTarget = true;
            }

            if (titleLabel == null)
            {
                titleLabel = CreateLabel("Title", root.transform, new Vector2(0f, 118f), new Vector2(620f, 56f), 34f, new Color(1f, 0.85f, 0.60f));
            }

            if (bodyLabel == null)
            {
                bodyLabel = CreateLabel("Body", root.transform, new Vector2(0f, 44f), new Vector2(620f, 120f), 24f, Color.white);
                bodyLabel.enableWordWrapping = true;
            }

            if (energyLabel == null)
            {
                energyLabel = CreateLabel("Energy", root.transform, new Vector2(0f, -18f), new Vector2(620f, 36f), 22f, new Color(1f, 0.83f, 0.63f));
            }

            if (continueButton == null)
            {
                continueButton = CreateButton("ContinueButton", root.transform, new Vector2(0f, -64f), new Vector2(420f, 62f), new Color(0.84f, 0.43f, 0.16f));
            }

            if (retryButton == null)
            {
                retryButton = CreateButton("RetryButton", root.transform, new Vector2(-138f, -148f), new Vector2(240f, 56f), new Color(0.23f, 0.24f, 0.30f));
            }

            if (premiumButton == null)
            {
                premiumButton = CreateButton("PremiumButton", root.transform, new Vector2(138f, -148f), new Vector2(240f, 56f), new Color(0.42f, 0.27f, 0.16f));
            }

            BindButtonLabels();
        }

        private void BindButtonLabels()
        {
            if (continueButtonLabel == null && continueButton != null)
            {
                continueButtonLabel = continueButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (retryButtonLabel == null && retryButton != null)
            {
                retryButtonLabel = retryButton.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (premiumButtonLabel == null && premiumButton != null)
            {
                premiumButtonLabel = premiumButton.GetComponentInChildren<TextMeshProUGUI>();
            }
        }

        private static GameObject CreatePanel(string name, Transform parent, Vector2 size, Color color)
        {
            GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(parent, false);
            RectTransform rect = panel.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            Image image = panel.GetComponent<Image>();
            image.color = color;
            return panel;
        }

        private static TextMeshProUGUI CreateLabel(string name, Transform parent, Vector2 position, Vector2 size, float fontSize, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            return label;
        }

        private static Button CreateButton(string name, Transform parent, Vector2 position, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
            Image image = go.GetComponent<Image>();
            image.color = color;
            Button button = go.GetComponent<Button>();
            button.targetGraphic = image;
            TextMeshProUGUI label = CreateLabel("Label", go.transform, Vector2.zero, new Vector2(size.x - 24f, size.y - 12f), 24f, Color.white);
            label.alignment = TextAlignmentOptions.Center;
            return button;
        }
    }
}
