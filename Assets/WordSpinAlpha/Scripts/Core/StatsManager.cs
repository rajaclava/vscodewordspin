using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class StatsManager : Singleton<StatsManager>
    {
        protected override bool PersistAcrossScenes => true;

        private void OnEnable()
        {
            GameEvents.HitEvaluated += HandleHitEvaluated;
            GameEvents.LevelCompleted += HandleLevelCompleted;
        }

        private void OnDisable()
        {
            GameEvents.HitEvaluated -= HandleHitEvaluated;
            GameEvents.LevelCompleted -= HandleLevelCompleted;
        }

        private void HandleHitEvaluated(HitData hit)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            switch (hit.resultType)
            {
                case HitResultType.Perfect:
                    SaveManager.Instance.Data.metrics.perfectHits++;
                    break;
                case HitResultType.Tolerated:
                    SaveManager.Instance.Data.metrics.toleratedHits++;
                    break;
                case HitResultType.NearMiss:
                    SaveManager.Instance.Data.metrics.nearMisses++;
                    break;
                case HitResultType.WrongLetter:
                    SaveManager.Instance.Data.metrics.wrongLetters++;
                    break;
                case HitResultType.WrongSlot:
                    SaveManager.Instance.Data.metrics.wrongSlots++;
                    break;
            }

            SaveManager.Instance.Save();
        }

        private void HandleLevelCompleted(LevelContext levelContext)
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.metrics.completedLevels++;
            SaveManager.Instance.Data.progress.lastCompletedLevel = Mathf.Max(SaveManager.Instance.Data.progress.lastCompletedLevel, levelContext.levelId);
            SaveManager.Instance.Data.progress.highestUnlockedLevel = Mathf.Max(SaveManager.Instance.Data.progress.highestUnlockedLevel, levelContext.levelId + 1);
            SaveManager.Instance.Save();
        }
    }
}
