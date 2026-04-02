using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class QuestionLifeManager : Singleton<QuestionLifeManager>
    {
        [SerializeField] private int defaultHearts = GameConstants.DefaultQuestionHearts;

        public int CurrentHearts { get; private set; }

        protected override bool PersistAcrossScenes => true;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            ResetQuestionHearts();
        }

        public void ResetQuestionHearts()
        {
            CurrentHearts = Mathf.Max(1, defaultHearts);
            GameEvents.RaiseQuestionHeartsChanged(CurrentHearts);
        }

        public bool ConsumeHeart()
        {
            CurrentHearts = Mathf.Max(0, CurrentHearts - 1);
            GameEvents.RaiseQuestionHeartsChanged(CurrentHearts);
            if (CurrentHearts <= 0)
            {
                GameEvents.RaiseQuestionFailed();
                return false;
            }

            return true;
        }

        public void Restore(int hearts)
        {
            CurrentHearts = Mathf.Clamp(hearts, 0, defaultHearts);
            GameEvents.RaiseQuestionHeartsChanged(CurrentHearts);
        }
    }
}
