using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Presentation
{
    public class GameplayHudPresenter : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI questionLabel;
        [SerializeField] private TextMeshProUGUI answerLabel;
        [SerializeField] private TextMeshProUGUI heartsLabel;
        [SerializeField] private TextMeshProUGUI targetHintLabel;
        [SerializeField] private TextMeshProUGUI debugAnswerLabel;
        [SerializeField] private TextMeshProUGUI scoreLabel;
        [SerializeField] private TextMeshProUGUI multiplierLabel;
        [SerializeField] private Button coinHookButton;
        [SerializeField] private TextMeshProUGUI coinHookTitleLabel;
        [SerializeField] private TextMeshProUGUI coinHookValueLabel;
        [SerializeField] private float targetPulseRefreshInterval = 0.05f;

        private char[] _revealedChars = new char[0];
        private int _currentTargetAnswerIndex = -1;
        private char _currentTargetLetter;
        private string _defaultHintText = string.Empty;
        private Color _defaultHintColor = Color.white;
        private float _feedbackExpiresAt;
        private Vector3 _hintBaseScale = Vector3.one;
        private int _lastPulseStep = -1;
        private readonly StringBuilder _answerBuilder = new StringBuilder(32);
        private Vector3 _coinHookBaseScale = Vector3.one;
        private float _coinHookPulseUntil;
        private int _currentSoftCurrency;
        private int _currentLevelId;
        private int _lastScoreTotal;
        private bool _hasScoreState;

        private void Awake()
        {
            EnsureScoreUi();
            EnsureCoinHookUi();
            if (targetHintLabel != null)
            {
                _hintBaseScale = targetHintLabel.rectTransform.localScale;
                _defaultHintColor = targetHintLabel.color;
                targetHintLabel.text = string.Empty;
                targetHintLabel.gameObject.SetActive(false);
            }

            _currentSoftCurrency = EconomyManager.Instance != null ? EconomyManager.Instance.SoftCurrency : 0;
            RefreshCoinHook();
        }

        private void Update()
        {
            if (targetHintLabel != null && _feedbackExpiresAt > 0f && Time.time >= _feedbackExpiresAt)
            {
                _feedbackExpiresAt = 0f;
                targetHintLabel.text = _defaultHintText;
                targetHintLabel.color = _defaultHintColor;
                targetHintLabel.rectTransform.localScale = _hintBaseScale;
                targetHintLabel.gameObject.SetActive(!string.IsNullOrWhiteSpace(_defaultHintText));
            }

            if (targetHintLabel != null && _currentTargetAnswerIndex >= 0)
            {
                int pulseStep = Mathf.FloorToInt(Time.time / Mathf.Max(0.01f, targetPulseRefreshInterval));
                if (pulseStep != _lastPulseStep)
                {
                    _lastPulseStep = pulseStep;
                    RefreshAnswerLabel();
                }
            }

            if (coinHookButton != null)
            {
                float pulse = _coinHookPulseUntil > Time.unscaledTime
                    ? 1f + (Mathf.Sin(Time.unscaledTime * 10f) * 0.06f)
                    : 1f + (Mathf.Sin(Time.unscaledTime * 2.2f) * 0.015f);
                coinHookButton.transform.localScale = _coinHookBaseScale * pulse;
            }
        }

        private void OnEnable()
        {
            GameEvents.LevelStarted += HandleLevelStarted;
            GameEvents.QuestionStarted += HandleQuestionStarted;
            GameEvents.LetterRevealed += HandleLetterRevealed;
            GameEvents.TargetSlotUpdated += HandleTargetSlotUpdated;
            GameEvents.HitEvaluated += HandleHitEvaluated;
            GameEvents.ScoreChanged += HandleScoreChanged;
            GameEvents.SoftCurrencyChanged += HandleSoftCurrencyChanged;
            GameEvents.QuestionHeartsChanged += HandleHeartsChanged;
            GameEvents.QuestionFailed += HandleQuestionFailed;
            GameEvents.LevelCompleted += HandleLevelCompleted;
            GameEvents.LanguageChanged += HandleLanguageChanged;
            SyncSoftCurrencyFromSource();
        }

        private void OnDisable()
        {
            GameEvents.LevelStarted -= HandleLevelStarted;
            GameEvents.QuestionStarted -= HandleQuestionStarted;
            GameEvents.LetterRevealed -= HandleLetterRevealed;
            GameEvents.TargetSlotUpdated -= HandleTargetSlotUpdated;
            GameEvents.HitEvaluated -= HandleHitEvaluated;
            GameEvents.ScoreChanged -= HandleScoreChanged;
            GameEvents.SoftCurrencyChanged -= HandleSoftCurrencyChanged;
            GameEvents.QuestionHeartsChanged -= HandleHeartsChanged;
            GameEvents.QuestionFailed -= HandleQuestionFailed;
            GameEvents.LevelCompleted -= HandleLevelCompleted;
            GameEvents.LanguageChanged -= HandleLanguageChanged;
        }

        private void HandleLevelStarted(LevelContext context)
        {
            _currentLevelId = context.levelId;
            SyncSoftCurrencyFromSource();
            RefreshCoinHook();
        }

        private void HandleQuestionStarted(QuestionContext context, string answerWord)
        {
            if (questionLabel != null && LevelFlowControllerFinder.TryGetCurrentQuestionText(out string questionText))
            {
                questionLabel.text = questionText;
            }

            int letterCount = context.totalLetters;
            _revealedChars = new char[letterCount];
            for (int i = 0; i < _revealedChars.Length; i++)
            {
                _revealedChars[i] = '_';
            }

            _currentTargetAnswerIndex = -1;
            _currentTargetLetter = '\0';
            _lastPulseStep = -1;
            _defaultHintText = string.Empty;
            if (context.questionIndex == 0 && scoreLabel != null)
            {
                scoreLabel.text = $"{GetLocalized("score")}: 0";
            }
            if (context.questionIndex == 0 && multiplierLabel != null)
            {
                multiplierLabel.text = "x1.0";
            }
            if (debugAnswerLabel != null)
            {
                debugAnswerLabel.text = $"Test Cevap: {answerWord}";
            }
            if (targetHintLabel != null)
            {
                targetHintLabel.text = string.Empty;
                targetHintLabel.gameObject.SetActive(false);
            }
            RefreshAnswerLabel();
        }

        private void HandleLetterRevealed(int index, char letter)
        {
            if (_revealedChars == null || index < 0 || index >= _revealedChars.Length)
            {
                return;
            }

            _revealedChars[index] = letter;
            _lastPulseStep = -1;
            RefreshAnswerLabel();
        }

        private void HandleTargetSlotUpdated(int slotIndex, int answerIndex, char letter)
        {
            _currentTargetAnswerIndex = answerIndex;
            _currentTargetLetter = letter;
            _lastPulseStep = -1;
            RefreshAnswerLabel();
        }

        private void HandleHeartsChanged(int hearts)
        {
            if (heartsLabel != null)
            {
                heartsLabel.text = $"{GetLocalized("hearts")}: {hearts}";
            }
        }

        private void HandleScoreChanged(ScoreStateData state)
        {
            _lastScoreTotal = state.totalScore;
            _hasScoreState = true;
            if (scoreLabel != null)
            {
                scoreLabel.text = $"{GetLocalized("score")}: {state.totalScore}";
            }

            if (multiplierLabel != null)
            {
                multiplierLabel.text = $"x{state.multiplier:0.0}";
                multiplierLabel.color = state.multiplier >= 3f
                    ? new Color(1f, 0.86f, 0.56f)
                    : state.multiplier >= 2f
                        ? new Color(1f, 0.76f, 0.40f)
                        : state.multiplier > 1f
                            ? new Color(0.92f, 0.68f, 0.34f)
                            : new Color(0.80f, 0.82f, 0.88f);
            }
        }

        private void HandleSoftCurrencyChanged(int current, int delta)
        {
            _currentSoftCurrency = current;
            if (delta > 0)
            {
                _coinHookPulseUntil = Time.unscaledTime + 0.9f;
            }

            RefreshCoinHook();
        }

        private void HandleHitEvaluated(HitData hit)
        {
            if (targetHintLabel == null)
            {
                return;
            }

            switch (hit.resultType)
            {
                case HitResultType.Perfect:
                    ShowTransientHint("Perfect", new Color(1f, 0.92f, 0.58f), 0.85f, 1.12f);
                    break;
                case HitResultType.Tolerated:
                    ShowTransientHint("Good", new Color(1f, 0.77f, 0.42f), 0.75f, 1.06f);
                    break;
                case HitResultType.NearMiss:
                    ShowTransientHint("Near Miss", new Color(1f, 0.52f, 0.34f), 0.75f, 1.04f);
                    break;
                case HitResultType.WrongSlot:
                    ShowTransientHint("Wrong Slot", new Color(0.96f, 0.38f, 0.30f), 0.8f, 1.06f);
                    break;
                case HitResultType.WrongLetter:
                    ShowTransientHint("Wrong Letter", new Color(0.96f, 0.38f, 0.30f), 0.8f, 1.06f);
                    break;
                case HitResultType.Miss:
                    ShowTransientHint("Miss", new Color(0.96f, 0.38f, 0.30f), 0.8f, 1.05f);
                    break;
            }
        }

        private void RefreshAnswerLabel()
        {
            if (answerLabel == null || _revealedChars == null)
            {
                return;
            }

            _answerBuilder.Clear();
            for (int i = 0; i < _revealedChars.Length; i++)
            {
                bool isCurrentTarget = i == _currentTargetAnswerIndex && _revealedChars[i] == '_';
                bool isUnrevealed = _revealedChars[i] == '_';
                if (isCurrentTarget)
                {
                    float pulse = 0.5f + (Mathf.Sin(Time.time * 6.4f) * 0.5f);
                    Color pulseColor = Color.Lerp(new Color(1f, 0.70f, 0.36f), new Color(1f, 0.92f, 0.68f), pulse);
                    Color boxColor = Color.Lerp(new Color(0.42f, 0.23f, 0.10f, 0.72f), new Color(0.92f, 0.56f, 0.20f, 0.92f), pulse);
                    string colorHex = ColorUtility.ToHtmlStringRGB(pulseColor);
                    string markHex = ColorUtility.ToHtmlStringRGBA(boxColor);
                    _answerBuilder.Append($"<mark=#{markHex}><color=#{colorHex}><b>");
                }
                else if (isUnrevealed)
                {
                    _answerBuilder.Append("<mark=#2B231FCC><color=#C9A073>");
                }

                _answerBuilder.Append(_revealedChars[i]);

                if (isCurrentTarget)
                {
                    _answerBuilder.Append("</b></color></mark>");
                }
                else if (isUnrevealed)
                {
                    _answerBuilder.Append("</color></mark>");
                }

                if (i < _revealedChars.Length - 1)
                {
                    _answerBuilder.Append(' ');
                }
            }

            answerLabel.text = _answerBuilder.ToString();
        }

        private void HandleQuestionFailed()
        {
            if (_feedbackExpiresAt > Time.time)
            {
                return;
            }

            ShowTransientHint("Try Again", new Color(1f, 0.56f, 0.44f), 1.0f, 1.08f);
        }

        private void HandleLevelCompleted(LevelContext context)
        {
            ShowTransientHint($"{GetLocalized("level")} {context.levelId} {GetLocalized("complete_hint")}", new Color(1f, 0.88f, 0.60f), 1.2f, 1.10f);
        }

        private void HandleLanguageChanged(string _)
        {
            if (QuestionLifeManager.Instance != null)
            {
                HandleHeartsChanged(QuestionLifeManager.Instance.CurrentHearts);
            }

            if (scoreLabel != null)
            {
                scoreLabel.text = $"{GetLocalized("score")}: {(_hasScoreState ? _lastScoreTotal : 0)}";
            }

            SyncSoftCurrencyFromSource();
            RefreshCoinHook();
        }

        private void ShowTransientHint(string text, Color color, float duration, float scaleMultiplier)
        {
            if (targetHintLabel == null)
            {
                return;
            }

            targetHintLabel.text = text;
            targetHintLabel.color = color;
            targetHintLabel.rectTransform.localScale = _hintBaseScale * scaleMultiplier;
            targetHintLabel.gameObject.SetActive(true);
            _feedbackExpiresAt = Time.time + duration;
        }

        private void EnsureScoreUi()
        {
            if (scoreLabel != null && multiplierLabel != null)
            {
                return;
            }

            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                return;
            }

            TMP_FontAsset font = questionLabel != null ? questionLabel.font : TMP_Settings.defaultFontAsset;
            if (scoreLabel == null)
            {
                Transform existingScore = canvas.transform.Find("TopBar/Currency");
                if (existingScore != null)
                {
                    scoreLabel = existingScore.GetComponent<TextMeshProUGUI>();
                }
            }

            if (scoreLabel == null)
            {
                Transform topBar = canvas.transform.Find("TopBar");
                Transform scoreParent = topBar != null ? topBar : canvas.transform;
                scoreLabel = CreateHudLabel("ScoreLabel", scoreParent, font, new Vector2(0.82f, 0.5f), new Vector2(220f, 50f), 28f, TextAlignmentOptions.Center);
                scoreLabel.text = $"{GetLocalized("score")}: 0";
                scoreLabel.color = new Color(0.96f, 0.95f, 0.88f);
            }
            else
            {
                scoreLabel.text = $"{GetLocalized("score")}: 0";
                scoreLabel.alignment = TextAlignmentOptions.Center;
                scoreLabel.fontSize = 28f;
                scoreLabel.color = new Color(0.96f, 0.95f, 0.88f);
            }

            if (multiplierLabel == null)
            {
                Transform topBar = scoreLabel != null ? scoreLabel.transform.parent : canvas.transform;
                multiplierLabel = CreateHudLabel("MultiplierLabel", topBar, font, new Vector2(0.82f, 0.18f), new Vector2(180f, 28f), 18f, TextAlignmentOptions.Center);
                multiplierLabel.text = "x1.0";
                multiplierLabel.color = new Color(0.80f, 0.82f, 0.88f);
            }
        }

        private static TextMeshProUGUI CreateHudLabel(string name, Transform parent, TMP_FontAsset font, Vector2 anchor, Vector2 size, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject labelObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.fontSize = fontSize;
            label.alignment = alignment;
            label.enableWordWrapping = false;
            return label;
        }

        private void EnsureCoinHookUi()
        {
            EconomyBalanceProfile balanceProfile = LevelEconomyManager.Instance != null ? LevelEconomyManager.Instance.Profile : null;
            if (balanceProfile != null && !balanceProfile.ShowGameplayCoinHook)
            {
                if (coinHookButton != null)
                {
                    coinHookButton.gameObject.SetActive(false);
                }
                return;
            }

            if (coinHookButton == null)
            {
                GameObject existing = GameObject.Find("CoinHookButton");
                if (existing != null)
                {
                    coinHookButton = existing.GetComponent<Button>();
                }
            }

            if (coinHookButton == null)
            {
                Canvas canvas = GetComponentInParent<Canvas>();
                Transform topBar = canvas != null ? canvas.transform.Find("TopBar") : null;
                if (topBar == null)
                {
                    return;
                }

                GameObject root = new GameObject("CoinHookButton", typeof(RectTransform), typeof(Image), typeof(Button));
                root.transform.SetParent(topBar, false);
                RectTransform rect = root.GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = new Vector2(0f, 0f);
                rect.sizeDelta = new Vector2(170f, 52f);

                Image background = root.GetComponent<Image>();
                background.color = new Color(0.26f, 0.20f, 0.16f, 0.88f);

                coinHookButton = root.GetComponent<Button>();
                coinHookButton.onClick.AddListener(OpenStoreFromCoinHook);

                TMP_FontAsset font = questionLabel != null ? questionLabel.font : TMP_Settings.defaultFontAsset;
                coinHookTitleLabel = CreateHudLabel("CoinHookTitle", root.transform, font, new Vector2(0.5f, 0.68f), new Vector2(150f, 24f), 18f, TextAlignmentOptions.Center);
                coinHookValueLabel = CreateHudLabel("CoinHookValue", root.transform, font, new Vector2(0.5f, 0.28f), new Vector2(150f, 24f), 18f, TextAlignmentOptions.Center);
                coinHookTitleLabel.color = new Color(0.98f, 0.87f, 0.66f);
                coinHookValueLabel.color = Color.white;
            }

            if (coinHookTitleLabel == null && coinHookButton != null)
            {
                Transform found = coinHookButton.transform.Find("CoinHookTitle");
                if (found != null)
                {
                    coinHookTitleLabel = found.GetComponent<TextMeshProUGUI>();
                }
            }

            if (coinHookValueLabel == null && coinHookButton != null)
            {
                Transform found = coinHookButton.transform.Find("CoinHookValue");
                if (found != null)
                {
                    coinHookValueLabel = found.GetComponent<TextMeshProUGUI>();
                }
            }

            if (coinHookButton != null)
            {
                coinHookButton.onClick.RemoveListener(OpenStoreFromCoinHook);
                coinHookButton.onClick.AddListener(OpenStoreFromCoinHook);
                _coinHookBaseScale = coinHookButton.transform.localScale;
            }
        }

        private void RefreshCoinHook()
        {
            if (coinHookButton == null)
            {
                return;
            }

            EconomyBalanceProfile balanceProfile = LevelEconomyManager.Instance != null ? LevelEconomyManager.Instance.Profile : null;
            bool showHook = balanceProfile == null || balanceProfile.ShowGameplayCoinHook;
            coinHookButton.gameObject.SetActive(showHook);
            if (!showHook)
            {
                return;
            }

            SyncSoftCurrencyFromSource();

            if (coinHookTitleLabel != null)
            {
                coinHookTitleLabel.text = GetLocalized("coin_hook_title");
            }

            if (coinHookValueLabel != null)
            {
                coinHookValueLabel.text = _currentSoftCurrency.ToString();
            }
        }

        private void SyncSoftCurrencyFromSource()
        {
            _currentSoftCurrency = EconomyManager.Instance != null ? EconomyManager.Instance.SoftCurrency : 0;
        }

        private static void OpenStoreFromCoinHook()
        {
            SceneNavigator.Instance?.OpenStore();
        }

        private static string GetLocalized(string key)
        {
            string language = SaveManager.Instance != null
                ? GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode)
                : GameConstants.DefaultLanguageCode;

            switch (language)
            {
                case "en":
                    return key switch
                    {
                        "score" => "Score",
                        "hearts" => "Lives",
                        "level" => "Level",
                        "complete_hint" => "Complete",
                        "coin_hook_title" => "Vault",
                        _ => key
                    };
                case "es":
                    return key switch
                    {
                        "score" => "Puntuacion",
                        "hearts" => "Vidas",
                        "level" => "Nivel",
                        "complete_hint" => "Completado",
                        "coin_hook_title" => "Caja",
                        _ => key
                    };
                case "de":
                    return key switch
                    {
                        "score" => "Punktzahl",
                        "hearts" => "Leben",
                        "level" => "Stufe",
                        "complete_hint" => "Abgeschlossen",
                        "coin_hook_title" => "Kiste",
                        _ => key
                    };
                default:
                    return key switch
                    {
                        "score" => "Puan",
                        "hearts" => "Can",
                        "level" => "Seviye",
                        "complete_hint" => "Tamamlandi",
                        "coin_hook_title" => "Kasa",
                        _ => key
                    };
            }
        }
    }

    internal static class LevelFlowControllerFinder
    {
        public static bool TryGetCurrentQuestionText(out string value)
        {
            value = string.Empty;
            LevelFlowController flow = Object.FindObjectOfType<LevelFlowController>();
            if (flow == null || flow.CurrentQuestion == null)
            {
                return false;
            }

            value = flow.CurrentQuestion.GetQuestion(flow.LanguageCode);
            return true;
        }
    }
}
