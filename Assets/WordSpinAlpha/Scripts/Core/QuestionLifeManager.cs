using UnityEngine;

namespace WordSpinAlpha.Core
{
    public class QuestionLifeManager : Singleton<QuestionLifeManager>
    {
        [SerializeField] private int defaultHearts = GameConstants.DefaultQuestionHearts;

        public int CurrentHearts { get; private set; }
        public int DefaultHearts => defaultHearts;

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
            CurrentHearts = ResolveMaxHearts();
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
            CurrentHearts = Mathf.Clamp(hearts, 0, ResolveMaxHearts());
            GameEvents.RaiseQuestionHeartsChanged(CurrentHearts);
        }

        public void RefreshForTesting(bool resetToMax)
        {
            if (resetToMax)
            {
                ResetQuestionHearts();
                return;
            }

            CurrentHearts = Mathf.Clamp(CurrentHearts, 0, ResolveMaxHearts());
            GameEvents.RaiseQuestionHeartsChanged(CurrentHearts);
        }

        public void ApplyEditorTuning(int hearts, bool resetCurrentHearts)
        {
            defaultHearts = Mathf.Max(1, hearts);
            if (resetCurrentHearts)
            {
                ResetQuestionHearts();
                return;
            }

            CurrentHearts = Mathf.Clamp(CurrentHearts, 0, ResolveMaxHearts());
            GameEvents.RaiseQuestionHeartsChanged(CurrentHearts);
        }

        private int ResolveMaxHearts()
        {
            if (TestPlayerModeManager.Instance != null &&
                TestPlayerModeManager.Instance.TryGetQuestionHeartsOverride(out int overrideHearts))
            {
                return overrideHearts;
            }

            return Mathf.Max(1, defaultHearts);
        }
    }
}
