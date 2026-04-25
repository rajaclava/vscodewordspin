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

            SessionSnapshot localizedSession = SaveManager.Instance.Data.GetCurrentLanguageSession();
            localizedSession.hasActiveSession = true;
            localizedSession.levelId = levelFlow.CurrentLevel.levelId;
            localizedSession.questionIndex = levelFlow.CurrentQuestionIndex;
            localizedSession.revealedLetters = revealedLetterOverride >= 0
                ? revealedLetterOverride
                : levelFlow.CurrentLetterIndex;
            localizedSession.currentTargetSlotIndex = levelFlow.CurrentTargetSlotIndex;
            localizedSession.questionHeartsRemaining = QuestionLifeManager.Instance != null ? QuestionLifeManager.Instance.CurrentHearts : GameConstants.DefaultQuestionHearts;
            localizedSession.pendingFailResolution = GameManager.Instance != null && GameManager.Instance.IsAwaitingFailResolution;
            localizedSession.languageCode = levelFlow.LanguageCode;
            localizedSession.campaignId = levelFlow.CurrentLevel.campaignId;
            localizedSession.themeId = levelFlow.CurrentLevel.themeId;
            localizedSession.revealedSlotIndices = new List<int>(levelFlow.RevealedSlotHistory);
            localizedSession.revealedTipLocalPoints = new List<Vector2>(levelFlow.RevealedTipLocalHistory);
            localizedSession.revealedPinLocalPositions = new List<Vector2>(levelFlow.RevealedPinLocalPositionHistory);
            localizedSession.revealedPinLocalRotations = new List<float>(levelFlow.RevealedPinLocalRotationHistory);
            GameManager.Instance?.PopulateSessionSnapshot(localizedSession);
            SaveManager.Instance.Save();
        }

        public void ClearSnapshot()
        {
            if (SaveManager.Instance == null)
            {
                return;
            }

            SaveManager.Instance.Data.ClearCurrentLanguageSession();
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
