using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using WordSpinAlpha.Core;
using WordSpinAlpha.Presentation;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Editor
{
    public static class WordSpinAlphaEditorRuntimeRefreshUtility
    {
        public static void SaveDirtyAssets()
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        public static void ApplyScenePresentationRefresh()
        {
            RefreshKeyboardPresentation();
            RefreshRotatorPresentation();
            RefreshThemePresentation();
            RefreshCurrentTargetState();
        }

        public static void ApplyContentAndConfigRefresh(bool reloadCurrentSession)
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ContentService.Instance?.RefreshEditorContent();
            EconomyManager.Instance?.RefreshRuntimeCatalogsForEditor();
            EnergyManager.Instance?.RefreshConfigForEditor();
            InputManager.Instance?.RefreshKeyboardConfig();

            MainMenuPresenter mainMenu = Object.FindObjectOfType<MainMenuPresenter>(true);
            if (mainMenu != null)
            {
                mainMenu.RefreshEditorContent();
            }

            InfoCardPresenter infoCard = Object.FindObjectOfType<InfoCardPresenter>(true);
            if (infoCard != null)
            {
                infoCard.RefreshContentCache();
            }

            StorePresenter store = Object.FindObjectOfType<StorePresenter>(true);
            if (store != null)
            {
                store.RefreshContentCache();
            }

            ThemeRuntimeController themeRuntime = Object.FindObjectOfType<ThemeRuntimeController>(true);
            if (themeRuntime != null)
            {
                themeRuntime.RefreshForEditor();
            }

            KeyboardPresenter keyboardPresenter = Object.FindObjectOfType<KeyboardPresenter>(true);
            if (keyboardPresenter != null)
            {
                keyboardPresenter.Build();
            }

            if (reloadCurrentSession)
            {
                GameManager.Instance?.ReloadCurrentSessionForEditorContent();
            }
        }

        public static void RefreshKeyboardPresentation()
        {
            KeyboardPresenter keyboardPresenter = Object.FindObjectOfType<KeyboardPresenter>(true);
            if (keyboardPresenter != null)
            {
                keyboardPresenter.Build();
            }
        }

        public static void RefreshRotatorPresentation()
        {
            RotatorPlaquePresenter[] plaquePresenters = Object.FindObjectsOfType<RotatorPlaquePresenter>(true);
            for (int i = 0; i < plaquePresenters.Length; i++)
            {
                if (plaquePresenters[i] != null)
                {
                    plaquePresenters[i].RebuildLayout();
                }
            }
        }

        public static void RefreshThemePresentation()
        {
            ThemeRuntimeController themeRuntime = Object.FindObjectOfType<ThemeRuntimeController>(true);
            if (themeRuntime != null)
            {
                themeRuntime.RefreshForEditor();
            }
        }

        public static void RefreshTelemetryPolicy()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            TelemetryService telemetry = Object.FindObjectOfType<TelemetryService>(true);
            if (telemetry != null)
            {
                telemetry.RefreshPolicyForEditor();
            }
        }

        public static void RefreshRemoteOverrides()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            ContentService.Instance?.RefreshRemoteOverrides();
            ApplyContentAndConfigRefresh(true);
        }

        public static void RefreshUiPresentation()
        {
            GameplayHudPresenter gameplayHud = Object.FindObjectOfType<GameplayHudPresenter>(true);
            if (gameplayHud != null)
            {
                gameplayHud.RefreshForEditor();
            }

            FailModalPresenter failModal = Object.FindObjectOfType<FailModalPresenter>(true);
            if (failModal != null)
            {
                failModal.RefreshForEditor();
            }

            InfoCardPresenter infoCard = Object.FindObjectOfType<InfoCardPresenter>(true);
            if (infoCard != null)
            {
                infoCard.RefreshForEditor();
            }

            ResultPresenter result = Object.FindObjectOfType<ResultPresenter>(true);
            if (result != null)
            {
                result.RefreshForEditor();
            }

            StorePresenter store = Object.FindObjectOfType<StorePresenter>(true);
            if (store != null)
            {
                store.RefreshForEditor();
            }

            MembershipPresenter membership = Object.FindObjectOfType<MembershipPresenter>(true);
            if (membership != null)
            {
                membership.RefreshForEditor();
            }

            MainMenuPresenter mainMenu = Object.FindObjectOfType<MainMenuPresenter>(true);
            if (mainMenu != null)
            {
                mainMenu.RefreshEditorContent();
            }

            Canvas.ForceUpdateCanvases();
        }

        public static void RefreshMobileLayout()
        {
            MobileRuntimeController mobile = Object.FindObjectOfType<MobileRuntimeController>(true);
            if (mobile != null)
            {
                mobile.RefreshForEditor();
            }

            RefreshUiPresentation();
        }

        public static void RefreshCurrentTargetState()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            LevelFlowController levelFlow = Object.FindObjectOfType<LevelFlowController>(true);
            if (levelFlow != null)
            {
                levelFlow.RefreshCurrentTarget();
            }
        }

        public static void ApplyGameplayLayoutRefresh()
        {
            GameplaySceneTuner tuner = Object.FindObjectOfType<GameplaySceneTuner>(true);
            if (tuner != null)
            {
                tuner.ApplyTuning();
            }

            ApplyScenePresentationRefresh();
        }

        public static void MarkCurrentSceneDirty()
        {
            if (Application.isPlaying)
            {
                return;
            }

            Scene activeScene = SceneManager.GetActiveScene();
            if (activeScene.IsValid())
            {
                EditorSceneManager.MarkSceneDirty(activeScene);
            }
        }
    }
}
