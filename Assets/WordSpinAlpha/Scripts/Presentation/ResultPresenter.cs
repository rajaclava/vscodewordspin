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
        private bool _hasScoreSummary;

        private void OnEnable()
        {
            GameEvents.LevelCompleted += HandleLevelCompleted;
            GameEvents.LevelScoreFinalized += HandleLevelScoreFinalized;
        }

        private void OnDisable()
        {
            GameEvents.LevelCompleted -= HandleLevelCompleted;
            GameEvents.LevelScoreFinalized -= HandleLevelScoreFinalized;
        }

        private void HandleLevelCompleted(LevelContext context)
        {
            if (root != null)
            {
                root.SetActive(true);
            }

            _lastCompletedLevelId = context.levelId;
            if (resultLabel != null)
            {
                if (_hasScoreSummary && _lastScoreSummary.levelId == context.levelId)
                {
                    resultLabel.text =
                        $"Level {context.levelId} Complete\n" +
                        $"Score: {_lastScoreSummary.totalScore}\n" +
                        $"Hit: {_lastScoreSummary.hitScore}  Clear: {_lastScoreSummary.clearBonus + _lastScoreSummary.noMistakeBonus + _lastScoreSummary.allPerfectBonus + _lastScoreSummary.timeBonus}\n" +
                        $"Best Multiplier: x{_lastScoreSummary.bestMultiplier:0.0}";
                }
                else
                {
                    resultLabel.text = $"Level {context.levelId} Complete";
                }
            }
        }

        private void HandleLevelScoreFinalized(LevelScoreSummaryData summary)
        {
            _lastScoreSummary = summary;
            _hasScoreSummary = true;
            if (root != null && root.activeSelf && resultLabel != null && _lastCompletedLevelId == summary.levelId)
            {
                resultLabel.text =
                    $"Level {summary.levelId} Complete\n" +
                    $"Score: {summary.totalScore}\n" +
                    $"Hit: {summary.hitScore}  Clear: {summary.clearBonus + summary.noMistakeBonus + summary.allPerfectBonus + summary.timeBonus}\n" +
                    $"Best Multiplier: x{summary.bestMultiplier:0.0}";
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
                Hide();
                SceneNavigator.Instance?.OpenMainMenu();
                return;
            }

            if (SceneNavigator.Instance != null && SceneNavigator.Instance.OpenGameplayLevel(nextLevelId, false))
            {
                Hide();
            }
        }

        public void ReturnToMenu()
        {
            Hide();
            SceneNavigator.Instance?.OpenMainMenu();
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
    }
}
