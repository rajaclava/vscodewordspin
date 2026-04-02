using System.Text;
using TMPro;
using UnityEngine;
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

        private void Awake()
        {
            EnsureScoreUi();
            if (targetHintLabel != null)
            {
                _hintBaseScale = targetHintLabel.rectTransform.localScale;
                _defaultHintColor = targetHintLabel.color;
                targetHintLabel.text = string.Empty;
                targetHintLabel.gameObject.SetActive(false);
            }
        }

        private void Update()
        {
            if (targetHintLabel == null)
            {
                return;
            }

            if (_feedbackExpiresAt > 0f && Time.time >= _feedbackExpiresAt)
            {
                _feedbackExpiresAt = 0f;
                targetHintLabel.text = _defaultHintText;
                targetHintLabel.color = _defaultHintColor;
                targetHintLabel.rectTransform.localScale = _hintBaseScale;
                targetHintLabel.gameObject.SetActive(!string.IsNullOrWhiteSpace(_defaultHintText));
            }

            if (_currentTargetAnswerIndex >= 0)
            {
                int pulseStep = Mathf.FloorToInt(Time.time / Mathf.Max(0.01f, targetPulseRefreshInterval));
                if (pulseStep != _lastPulseStep)
                {
                    _lastPulseStep = pulseStep;
                    RefreshAnswerLabel();
                }
            }
        }

        private void OnEnable()
        {
            GameEvents.QuestionStarted += HandleQuestionStarted;
            GameEvents.LetterRevealed += HandleLetterRevealed;
            GameEvents.TargetSlotUpdated += HandleTargetSlotUpdated;
            GameEvents.HitEvaluated += HandleHitEvaluated;
            GameEvents.ScoreChanged += HandleScoreChanged;
            GameEvents.QuestionHeartsChanged += HandleHeartsChanged;
            GameEvents.QuestionFailed += HandleQuestionFailed;
            GameEvents.LevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            GameEvents.QuestionStarted -= HandleQuestionStarted;
            GameEvents.LetterRevealed -= HandleLetterRevealed;
            GameEvents.TargetSlotUpdated -= HandleTargetSlotUpdated;
            GameEvents.HitEvaluated -= HandleHitEvaluated;
            GameEvents.ScoreChanged -= HandleScoreChanged;
            GameEvents.QuestionHeartsChanged -= HandleHeartsChanged;
            GameEvents.QuestionFailed -= HandleQuestionFailed;
            GameEvents.LevelCompleted -= HandleLevelCompleted;
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
                scoreLabel.text = "Score: 0";
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
                heartsLabel.text = $"Hearts: {hearts}";
            }
        }

        private void HandleScoreChanged(ScoreStateData state)
        {
            if (scoreLabel != null)
            {
                scoreLabel.text = $"Score: {state.totalScore}";
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
            ShowTransientHint($"Level {context.levelId} Complete", new Color(1f, 0.88f, 0.60f), 1.2f, 1.10f);
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
                scoreLabel.text = "Score: 0";
                scoreLabel.color = new Color(0.96f, 0.95f, 0.88f);
            }
            else
            {
                scoreLabel.text = "Score: 0";
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
