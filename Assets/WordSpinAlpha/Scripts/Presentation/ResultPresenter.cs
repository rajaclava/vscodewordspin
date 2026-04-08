using TMPro;
using UnityEngine;
using WordSpinAlpha.Core;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Presentation
{
    public class ResultPresenter : MonoBehaviour
    {
        [SerializeField] private GameObject root;
        [SerializeField] private TextMeshProUGUI resultLabel;

        private int _lastCompletedLevelId;
        private LevelScoreSummaryData _lastScoreSummary;
        private LevelEconomySummaryData _lastEconomySummary;
        private bool _hasScoreSummary;
        private bool _hasEconomySummary;

        private void Awake()
        {
            EnsureLayout();
        }

        private void OnEnable()
        {
            GameEvents.LevelCompleted += HandleLevelCompleted;
            GameEvents.LevelScoreFinalized += HandleLevelScoreFinalized;
            GameEvents.LevelEconomyFinalized += HandleLevelEconomyFinalized;
            GameEvents.LanguageChanged += HandleLanguageChanged;
        }

        private void OnDisable()
        {
            GameEvents.LevelCompleted -= HandleLevelCompleted;
            GameEvents.LevelScoreFinalized -= HandleLevelScoreFinalized;
            GameEvents.LevelEconomyFinalized -= HandleLevelEconomyFinalized;
            GameEvents.LanguageChanged -= HandleLanguageChanged;
        }

        private void HandleLevelCompleted(LevelContext context)
        {
            EnsureLayout();
            if (root != null)
            {
                root.SetActive(true);
            }

            _lastCompletedLevelId = context.levelId;
            PersistPendingResultState();
            RefreshResultText();
            RefreshButtonTexts();
        }

        private void HandleLevelScoreFinalized(LevelScoreSummaryData summary)
        {
            _lastScoreSummary = summary;
            _hasScoreSummary = true;
            PersistPendingResultState();
            if (root != null && root.activeSelf && _lastCompletedLevelId == summary.levelId)
            {
                RefreshResultText();
            }
        }

        private void HandleLevelEconomyFinalized(LevelEconomySummaryData summary)
        {
            _lastEconomySummary = summary;
            _hasEconomySummary = true;
            PersistPendingResultState();
            if (root != null && root.activeSelf && _lastCompletedLevelId == summary.levelId)
            {
                RefreshResultText();
            }
        }

        public void Hide()
        {
            if (root != null)
            {
                root.SetActive(false);
            }
        }

        public void PlayNextLevel()
        {
            int nextLevelId = _lastCompletedLevelId + 1;
            if (!HasLevel(nextLevelId))
            {
                ClearPendingResultState();
                Hide();
                SceneNavigator.Instance?.OpenMainMenu();
                return;
            }

            if (SceneNavigator.Instance != null && SceneNavigator.Instance.OpenGameplayLevel(nextLevelId, false))
            {
                ClearPendingResultState();
                Hide();
            }
        }

        public void ReturnToMenu()
        {
            Hide();
            SceneNavigator.Instance?.OpenMainMenu();
        }

        public void RefreshForEditor()
        {
            RefreshResultText();
            RefreshButtonTexts();
        }

        private void HandleLanguageChanged(string _)
        {
            EnsureLayout();
            RefreshResultText();
            RefreshButtonTexts();
        }

        private void RefreshResultText()
        {
            if (resultLabel == null || _lastCompletedLevelId <= 0)
            {
                return;
            }

            if (_hasScoreSummary && _lastScoreSummary.levelId == _lastCompletedLevelId)
            {
                System.Text.StringBuilder builder = new System.Text.StringBuilder(192);
                builder.AppendLine($"{GetLocalized("level")} {_lastCompletedLevelId} {GetLocalized("complete")}");
                builder.AppendLine($"{GetLocalized("score")}: {_lastScoreSummary.totalScore}");
                builder.AppendLine($"{GetLocalized("hit")}: {_lastScoreSummary.hitScore}  {GetLocalized("clear")}: {_lastScoreSummary.clearBonus + _lastScoreSummary.noMistakeBonus + _lastScoreSummary.allPerfectBonus + _lastScoreSummary.timeBonus}");
                builder.Append($"{GetLocalized("best_multiplier")}: x{_lastScoreSummary.bestMultiplier:0.0}");

                if (_hasEconomySummary && _lastEconomySummary.levelId == _lastCompletedLevelId)
                {
                    builder.AppendLine();
                    builder.AppendLine($"{GetLocalized("stars")}: {_lastEconomySummary.starsEarned}/3");
                    builder.Append($"{GetLocalized("coins")}: +{_lastEconomySummary.coinReward}");
                    if (_lastEconomySummary.firstClear)
                    {
                        builder.Append($"  ({GetLocalized("first_clear")})");
                    }

                    if (_lastEconomySummary.adBonusEligible && _lastEconomySummary.adBonusCoins > 0)
                    {
                        builder.AppendLine();
                        builder.Append($"{GetLocalized("ad_hook_prefix")} +{_lastEconomySummary.adBonusCoins} {GetLocalized("coin_unit")}");
                    }
                }

                resultLabel.text = builder.ToString();
                return;
            }

            resultLabel.text = $"{GetLocalized("level")} {_lastCompletedLevelId} {GetLocalized("complete")}";
        }

        private void RefreshButtonTexts()
        {
            SetButtonLabel("Next", GetLocalized("next"));
            SetButtonLabel("Menu", GetLocalized("menu"));
        }

        private void SetButtonLabel(string objectName, string value)
        {
            if (root == null)
            {
                return;
            }

            Transform target = root.transform.Find(objectName);
            if (target == null)
            {
                return;
            }

            TextMeshProUGUI label = target.GetComponentInChildren<TextMeshProUGUI>(true);
            if (label != null)
            {
                label.text = value;
            }
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
                        "level" => "Level",
                        "complete" => "Complete",
                        "score" => "Score",
                        "hit" => "Hit",
                        "clear" => "Clear",
                        "best_multiplier" => "Best Multiplier",
                        "stars" => "Stars",
                        "coins" => "Coins",
                        "first_clear" => "First Clear",
                        "coin_unit" => "coins",
                        "ad_hook_prefix" => "Watch ad for",
                        "next" => "Next",
                        "menu" => "Menu",
                        _ => key
                    };
                case "es":
                    return key switch
                    {
                        "level" => "Nivel",
                        "complete" => "Completado",
                        "score" => "Puntuacion",
                        "hit" => "Golpe",
                        "clear" => "Bonus",
                        "best_multiplier" => "Mejor Multiplicador",
                        "stars" => "Estrellas",
                        "coins" => "Monedas",
                        "first_clear" => "Primera vez",
                        "coin_unit" => "monedas",
                        "ad_hook_prefix" => "Mira un anuncio por",
                        "next" => "Siguiente",
                        "menu" => "Menu",
                        _ => key
                    };
                case "de":
                    return key switch
                    {
                        "level" => "Level",
                        "complete" => "Abgeschlossen",
                        "score" => "Punktzahl",
                        "hit" => "Treffer",
                        "clear" => "Bonus",
                        "best_multiplier" => "Bester Multiplikator",
                        "stars" => "Sterne",
                        "coins" => "Muenzen",
                        "first_clear" => "Erster Abschluss",
                        "coin_unit" => "Muenzen",
                        "ad_hook_prefix" => "Werbung fuer",
                        "next" => "Weiter",
                        "menu" => "Menu",
                        _ => key
                    };
                default:
                    return key switch
                    {
                        "level" => "Seviye",
                        "complete" => "Tamamlandi",
                        "score" => "Skor",
                        "hit" => "Vurus",
                        "clear" => "Bonus",
                        "best_multiplier" => "En Iyi Carpan",
                        "stars" => "Yildiz",
                        "coins" => "Coin",
                        "first_clear" => "Ilk Tamamlama",
                        "coin_unit" => "coin",
                        "ad_hook_prefix" => "Reklam izle, kazan",
                        "next" => "Siradaki",
                        "menu" => "Menu",
                        _ => key
                    };
            }
        }

        private static bool HasLevel(int levelId)
        {
            if (ContentService.Instance == null)
            {
                return false;
            }

            var catalog = ContentService.Instance.LoadLevels();
            if (catalog == null || catalog.levels == null)
            {
                return false;
            }

            foreach (var level in catalog.levels)
            {
                if (level.levelId == levelId)
                {
                    return true;
                }
            }

            return false;
        }

        public void RestorePendingResultFromSave()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SessionSnapshot session = SaveManager.Instance.Data.session;
            if (!session.pendingLevelResult || session.pendingResultLevelId <= 0)
            {
                return;
            }

            _lastCompletedLevelId = session.pendingResultLevelId;
            _lastScoreSummary = new LevelScoreSummaryData
            {
                levelId = session.pendingResultLevelId,
                totalScore = session.pendingResultTotalScore,
                hitScore = session.pendingResultHitScore,
                clearBonus = session.pendingResultClearScore,
                noMistakeBonus = 0,
                allPerfectBonus = 0,
                timeBonus = 0,
                bestMultiplier = session.pendingResultBestMultiplier
            };
            _hasScoreSummary = session.pendingResultTotalScore > 0 || session.pendingResultHitScore > 0 || session.pendingResultClearScore > 0;
            _lastEconomySummary = new LevelEconomySummaryData
            {
                levelId = session.pendingResultLevelId,
                starsEarned = session.pendingResultStars,
                coinReward = session.pendingResultCoinReward,
                adBonusCoins = session.pendingResultAdBonusCoins,
                adBonusEligible = session.pendingResultAdBonusEligible,
                firstClear = false
            };
            _hasEconomySummary = session.pendingResultStars > 0 || session.pendingResultCoinReward > 0 || session.pendingResultAdBonusCoins > 0;

            if (root != null)
            {
                root.SetActive(true);
            }

            EnsureLayout();
            RefreshResultText();
            RefreshButtonTexts();
        }

        private void EnsureLayout()
        {
            if (root == null)
            {
                return;
            }

            RectTransform rootRect = root.GetComponent<RectTransform>();
            if (rootRect != null)
            {
                rootRect.sizeDelta = new Vector2(760f, 320f);
            }

            if (resultLabel != null)
            {
                RectTransform labelRect = resultLabel.GetComponent<RectTransform>();
                if (labelRect != null)
                {
                    labelRect.anchorMin = new Vector2(0.5f, 0.68f);
                    labelRect.anchorMax = new Vector2(0.5f, 0.68f);
                    labelRect.anchoredPosition = new Vector2(0f, 6f);
                    labelRect.sizeDelta = new Vector2(620f, 190f);
                }

                resultLabel.alignment = TextAlignmentOptions.Center;
                resultLabel.enableWordWrapping = true;
                resultLabel.enableAutoSizing = true;
                resultLabel.fontSizeMin = 18f;
                resultLabel.fontSizeMax = 31f;
                resultLabel.lineSpacing = -8f;
            }

            ApplyButtonLayout("Next", 0.35f);
            ApplyButtonLayout("Menu", 0.65f);
        }

        private void ApplyButtonLayout(string objectName, float anchorX)
        {
            if (root == null)
            {
                return;
            }

            Transform target = root.transform.Find(objectName);
            if (target == null)
            {
                return;
            }

            RectTransform rect = target as RectTransform;
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(anchorX, 0.14f);
            rect.anchorMax = new Vector2(anchorX, 0.14f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(172f, 52f);
        }

        private void PersistPendingResultState()
        {
            if (SaveManager.Instance == null || _lastCompletedLevelId <= 0)
            {
                return;
            }

            SessionSnapshot session = SaveManager.Instance.Data.session;
            session.hasActiveSession = true;
            session.pendingLevelResult = true;
            session.pendingResultLevelId = _lastCompletedLevelId;
            session.pendingResultTotalScore = _hasScoreSummary ? _lastScoreSummary.totalScore : 0;
            session.pendingResultHitScore = _hasScoreSummary ? _lastScoreSummary.hitScore : 0;
            session.pendingResultClearScore = _hasScoreSummary ? _lastScoreSummary.clearBonus + _lastScoreSummary.noMistakeBonus + _lastScoreSummary.allPerfectBonus + _lastScoreSummary.timeBonus : 0;
            session.pendingResultBestMultiplier = _hasScoreSummary ? _lastScoreSummary.bestMultiplier : 0f;
            session.pendingResultStars = _hasEconomySummary ? _lastEconomySummary.starsEarned : 0;
            session.pendingResultCoinReward = _hasEconomySummary ? _lastEconomySummary.coinReward : 0;
            session.pendingResultAdBonusCoins = _hasEconomySummary ? _lastEconomySummary.adBonusCoins : 0;
            session.pendingResultAdBonusEligible = _hasEconomySummary && _lastEconomySummary.adBonusEligible;
            SaveManager.Instance.Save();
        }

        private static void ClearPendingResultState()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SessionSnapshot session = SaveManager.Instance.Data.session;
            session.pendingLevelResult = false;
            session.pendingResultLevelId = 0;
            session.pendingResultTotalScore = 0;
            session.pendingResultHitScore = 0;
            session.pendingResultClearScore = 0;
            session.pendingResultBestMultiplier = 0f;
            session.pendingResultStars = 0;
            session.pendingResultCoinReward = 0;
            session.pendingResultAdBonusCoins = 0;
            session.pendingResultAdBonusEligible = false;
            SaveManager.Instance.Save();
        }
    }
}
