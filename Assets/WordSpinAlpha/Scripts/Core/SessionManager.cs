using UnityEngine;
using System.Collections.Generic;
using System.Collections;

namespace WordSpinAlpha.Core
{
    public class SessionManager : Singleton<SessionManager>
    {
        [SerializeField] private LevelFlowController levelFlow;
        private Coroutine _pendingRevealSnapshot;

        protected override bool PersistAcrossScenes => false;

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
            {
                return;
            }

            if (levelFlow == null)
            {
                levelFlow = FindObjectOfType<LevelFlowController>();
            }
        }

        private void OnEnable()
        {
            GameEvents.LetterRevealed += HandleLetterRevealed;
        }

        private void OnDisable()
        {
            GameEvents.LetterRevealed -= HandleLetterRevealed;
        }

        private void HandleLetterRevealed(int index, char value)
        {
            if (_pendingRevealSnapshot != null)
            {
                StopCoroutine(_pendingRevealSnapshot);
            }

            _pendingRevealSnapshot = StartCoroutine(TakeSnapshotAfterReveal(index + 1));
        }

        public void TakeSnapshot()
        {
            TakeSnapshot(-1);
        }

        private void TakeSnapshot(int revealedLetterOverride)
        {
            if (levelFlow == null)
            {
                levelFlow = FindObjectOfType<LevelFlowController>();
            }

            if (SaveManager.Instance == null || GameManager.Instance == null || levelFlow == null || levelFlow.CurrentLevel == null)
            {
                return;
            }

            SaveManager.Instance.Data.session.hasActiveSession = true;
            SaveManager.Instance.Data.session.levelId = levelFlow.CurrentLevel.levelId;
            SaveManager.Instance.Data.session.questionIndex = levelFlow.CurrentQuestionIndex;
            SaveManager.Instance.Data.session.revealedLetters = revealedLetterOverride >= 0
                ? revealedLetterOverride
                : levelFlow.CurrentLetterIndex;
            SaveManager.Instance.Data.session.currentTargetSlotIndex = levelFlow.CurrentTargetSlotIndex;
            SaveManager.Instance.Data.session.questionHeartsRemaining = QuestionLifeManager.Instance != null ? QuestionLifeManager.Instance.CurrentHearts : GameConstants.DefaultQuestionHearts;
            SaveManager.Instance.Data.session.pendingFailResolution = GameManager.Instance != null && GameManager.Instance.IsAwaitingFailResolution;
            SaveManager.Instance.Data.session.languageCode = levelFlow.LanguageCode;
            SaveManager.Instance.Data.session.campaignId = levelFlow.CurrentLevel.campaignId;
            SaveManager.Instance.Data.session.themeId = levelFlow.CurrentLevel.themeId;
            SaveManager.Instance.Data.session.revealedSlotIndices = new List<int>(levelFlow.RevealedSlotHistory);
            SaveManager.Instance.Data.session.revealedTipLocalPoints = new List<Vector2>(levelFlow.RevealedTipLocalHistory);
            SaveManager.Instance.Data.session.revealedPinLocalPositions = new List<Vector2>(levelFlow.RevealedPinLocalPositionHistory);
            SaveManager.Instance.Data.session.revealedPinLocalRotations = new List<float>(levelFlow.RevealedPinLocalRotationHistory);
            SaveManager.Instance.Save();
        }

        public void ClearSnapshot()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.session = new SessionSnapshot();
            SaveManager.Instance.Save();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                TakeSnapshot();
            }
        }

        private void OnApplicationQuit()
        {
            TakeSnapshot();
        }

        private IEnumerator TakeSnapshotAfterReveal(int revealedLetterCount)
        {
            yield return null;
            TakeSnapshot(revealedLetterCount);
            _pendingRevealSnapshot = null;
        }
    }
}
