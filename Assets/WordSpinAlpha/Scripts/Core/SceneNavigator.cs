using UnityEngine;
using UnityEngine.SceneManagement;
using WordSpinAlpha.Content;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Core
{
    public class SceneNavigator : Singleton<SceneNavigator>
    {
        private bool _hasPendingGameplayRequest;
        private bool _resumeSavedSession;
        private int _pendingLevelId;
        private string _returnSceneName = GameConstants.SceneHubPreview;

        public bool OpenEntryMenu()
        {
            _returnSceneName = GameConstants.SceneHubPreview;
            return LoadScene(GameConstants.SceneMainMenu, false);
        }

        public bool OpenMainMenu()
        {
            string target = string.IsNullOrEmpty(_returnSceneName) ? GameConstants.SceneHubPreview : _returnSceneName;
            _returnSceneName = GameConstants.SceneHubPreview;
            return LoadScene(target, false);
        }

        public bool OpenStore()
        {
            string activeScene = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(activeScene) && activeScene != GameConstants.SceneStore && activeScene != GameConstants.SceneBoot)
            {
                _returnSceneName = activeScene;
            }

            if (activeScene == GameConstants.SceneGameplay)
            {
                SessionManager.Instance?.TakeSnapshot();
            }

            return LoadScene(GameConstants.SceneStore, false);
        }

        public bool OpenGameplayForProgress()
        {
            if (CanResumeSavedSession())
            {
                _hasPendingGameplayRequest = true;
                _resumeSavedSession = true;
                _pendingLevelId = SaveManager.Instance.Data.session.levelId;
                return LoadScene(GameConstants.SceneGameplay, true);
            }

            int levelId = ResolvePlayableProgressLevel();
            return OpenGameplayLevel(levelId, true);
        }

        public bool OpenGameplayLevel(int levelId, bool consumeEntryEnergy)
        {
            if (consumeEntryEnergy && (EnergyManager.Instance == null || !EnergyManager.Instance.TryConsumeEntryEnergy()))
            {
                GameEvents.RaiseMetric("energyBlockedLevelStart", $"{{\"levelId\":{levelId}}}");
                return false;
            }

            _hasPendingGameplayRequest = true;
            _resumeSavedSession = false;
            _pendingLevelId = Mathf.Max(1, levelId);
            return LoadScene(GameConstants.SceneGameplay, true);
        }

        public bool TryConsumePendingGameplayRequest(out int levelId, out bool resumeSavedSession)
        {
            levelId = _pendingLevelId;
            resumeSavedSession = _resumeSavedSession;

            bool hadRequest = _hasPendingGameplayRequest;
            _hasPendingGameplayRequest = false;
            _resumeSavedSession = false;
            _pendingLevelId = 0;
            return hadRequest;
        }

        public bool ReturnFromStore()
        {
            if (_returnSceneName == GameConstants.SceneGameplay && CanResumeSavedSession())
            {
                _hasPendingGameplayRequest = true;
                _resumeSavedSession = true;
                _pendingLevelId = SaveManager.Instance.Data.session.levelId;
            }

            return LoadScene(string.IsNullOrEmpty(_returnSceneName) ? GameConstants.SceneHubPreview : _returnSceneName, false);
        }

        public void SetReturnSceneOverride(string sceneName)
        {
            if (!string.IsNullOrEmpty(sceneName))
            {
                _returnSceneName = sceneName;
            }
        }

        private static bool LoadScene(string sceneName, bool forceReloadCurrent)
        {
            if (!forceReloadCurrent && string.Equals(SceneManager.GetActiveScene().name, sceneName))
            {
                return true;
            }

            SceneManager.LoadScene(sceneName);
            return true;
        }

        private static bool CanResumeSavedSession()
        {
            if (SaveManager.Instance == null || !SaveManager.Instance.Data.session.hasActiveSession)
            {
                return false;
            }

            SessionSnapshot session = SaveManager.Instance.Data.session;
            if (session.levelId <= 0)
            {
                return false;
            }

            string currentLanguage = GameConstants.NormalizeLanguageCode(SaveManager.Instance.Data.languageCode);
            string sessionLanguage = GameConstants.NormalizeLanguageCode(session.languageCode);
            return string.Equals(currentLanguage, sessionLanguage, System.StringComparison.OrdinalIgnoreCase);
        }

        private static int ResolvePlayableProgressLevel()
        {
            int requestedLevelId = 1;
            if (SaveManager.Instance != null)
            {
                requestedLevelId = SaveManager.Instance.Data.progress.GetHighestUnlockedLevel(SaveManager.Instance.Data.languageCode);
            }
            LevelCatalog levelCatalog = ContentService.Instance?.LoadLevels();
            LevelDefinition[] levels = levelCatalog != null ? levelCatalog.levels : null;
            if (levels == null || levels.Length == 0)
            {
                return requestedLevelId;
            }

            int smallestLevelId = int.MaxValue;
            int bestLevelId = 0;
            for (int i = 0; i < levels.Length; i++)
            {
                LevelDefinition level = levels[i];
                if (level == null || level.levelId <= 0)
                {
                    continue;
                }

                if (level.levelId < smallestLevelId)
                {
                    smallestLevelId = level.levelId;
                }

                if (level.levelId <= requestedLevelId && level.levelId > bestLevelId)
                {
                    bestLevelId = level.levelId;
                }
            }

            if (bestLevelId > 0)
            {
                return bestLevelId;
            }

            return smallestLevelId != int.MaxValue ? smallestLevelId : requestedLevelId;
        }
    }
}
