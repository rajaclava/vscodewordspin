using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Editor
{
    public class SaveSessionDebugWindow : EditorWindow
    {
        private Vector2 _scroll;

        [MenuItem("Tools/WordSpin Alpha/Debug/Save ve Session Paneli")]
        public static void Open()
        {
            GetWindow<SaveSessionDebugWindow>("Save ve Session");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Save ve Session Paneli", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu panel aktif save durumunu incelemek ve test sirasinda guvenli reset islemleri yapmak icindir. En saglikli kullanim play mod icindedir.", MessageType.Info);

            if (SaveManager.Instance == null)
            {
                EditorGUILayout.HelpBox("SaveManager aktif degil. Bu paneli play modda kullan.", MessageType.Warning);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Session Temizle", GUILayout.Height(26f)))
                {
                    SaveManager.Instance.Data.session = new SessionSnapshot();
                    SaveManager.Instance.Save();
                }

                if (GUILayout.Button("Ekonomiyi Sifirla", GUILayout.Height(26f)))
                {
                    SaveManager.Instance.Data.economy = new EconomyState();
                    SaveManager.Instance.Save();
                    EconomyManager.Instance?.RefreshRuntimeCatalogsForEditor();
                }

                if (GUILayout.Button("Tema Kilitlerini Sifirla", GUILayout.Height(26f)))
                {
                    SaveManager.Instance.Data.themes = new ThemeOwnershipState();
                    SaveManager.Instance.Save();
                    EconomyManager.Instance?.RefreshRuntimeCatalogsForEditor();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Ilerlemeyi Sifirla", GUILayout.Height(26f)))
                {
                    SaveManager.Instance.Data.progress = new ProgressState();
                    SaveManager.Instance.Save();
                }

                if (GUILayout.Button("Telemetry Sayacini Sifirla", GUILayout.Height(26f)))
                {
                    SaveManager.Instance.Data.telemetry.pendingTelemetryEventCount = 0;
                    SaveManager.Instance.Save();
                }

                if (GUILayout.Button("Tum Save'i Varsayilana Dondur", GUILayout.Height(26f)))
                {
                    SaveManager.Instance.ReplaceData(new PlayerSaveData());
                    EconomyManager.Instance?.RefreshRuntimeCatalogsForEditor();
                    EnergyManager.Instance?.RefreshConfigForEditor();
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            DrawSection("Genel", () =>
            {
                EditorGUILayout.LabelField("Dil", SaveManager.Instance.Data.languageCode);
                EditorGUILayout.LabelField("Remote Icerik", SaveManager.Instance.Data.remoteContent.remoteContentEnabled ? "Acik" : "Kapali");
                EditorGUILayout.LabelField("Manifest", SaveManager.Instance.Data.remoteContent.activeManifestVersion);
            });

            DrawSection("Session", () =>
            {
                SessionSnapshot session = SaveManager.Instance.Data.session;
                EditorGUILayout.LabelField("Aktif Session", session.hasActiveSession ? "Evet" : "Hayir");
                EditorGUILayout.LabelField("Level", session.levelId.ToString());
                EditorGUILayout.LabelField("Soru Index", session.questionIndex.ToString());
                EditorGUILayout.LabelField("Acilan Harf", session.revealedLetters.ToString());
                EditorGUILayout.LabelField("Aktif Slot", session.currentTargetSlotIndex.ToString());
                EditorGUILayout.LabelField("Kalan Can", session.questionHeartsRemaining.ToString());
                EditorGUILayout.LabelField("Pending Fail", session.pendingFailResolution ? "Evet" : "Hayir");
                EditorGUILayout.LabelField("Pending Info Card", session.pendingInfoCard ? "Evet" : "Hayir");
                EditorGUILayout.LabelField("Pending Result", session.pendingLevelResult ? "Evet" : "Hayir");
            });

            DrawSection("Ekonomi ve Uyelik", () =>
            {
                EditorGUILayout.LabelField("Coin", SaveManager.Instance.Data.economy.softCurrency.ToString());
                EditorGUILayout.LabelField("Ipucu", SaveManager.Instance.Data.economy.hints.ToString());
                EditorGUILayout.LabelField("Premium Uyelik", SaveManager.Instance.Data.membership.premiumMembershipActive ? "Aktif" : "Pasif");
                EditorGUILayout.LabelField("No Ads", SaveManager.Instance.Data.membership.noAdsOwned ? "Acik" : "Kapali");
                EditorGUILayout.LabelField("Aktif Tema", SaveManager.Instance.Data.themes.activeThemeId);
                EditorGUILayout.LabelField("Acilmis Tema Sayisi", SaveManager.Instance.Data.themes.unlockedThemes.Count.ToString());
            });

            DrawSection("Enerji ve Telemetry", () =>
            {
                EditorGUILayout.LabelField("Enerji", SaveManager.Instance.Data.energy.currentEnergy.ToString());
                EditorGUILayout.LabelField("Max Enerji", EnergyManager.Instance != null ? EnergyManager.Instance.MaxEnergy.ToString() : "-");
                EditorGUILayout.LabelField("Bekleyen Telemetry Event", SaveManager.Instance.Data.telemetry.pendingTelemetryEventCount.ToString());
                EditorGUILayout.LabelField("Telemetry Session Id", SaveManager.Instance.Data.telemetry.currentSessionId);
            });

            DrawSection("Ilerleme ve Metrikler", () =>
            {
                EditorGUILayout.LabelField("Highest Unlocked", SaveManager.Instance.Data.progress.highestUnlockedLevel.ToString());
                EditorGUILayout.LabelField("Last Completed", SaveManager.Instance.Data.progress.lastCompletedLevel.ToString());
                EditorGUILayout.LabelField("Completed Levels", SaveManager.Instance.Data.metrics.completedLevels.ToString());
                EditorGUILayout.LabelField("Perfect Hits", SaveManager.Instance.Data.metrics.perfectHits.ToString());
                EditorGUILayout.LabelField("Good Hits", SaveManager.Instance.Data.metrics.toleratedHits.ToString());
                EditorGUILayout.LabelField("Near Miss", SaveManager.Instance.Data.metrics.nearMisses.ToString());
                EditorGUILayout.LabelField("Wrong Letter", SaveManager.Instance.Data.metrics.wrongLetters.ToString());
                EditorGUILayout.LabelField("Wrong Slot", SaveManager.Instance.Data.metrics.wrongSlots.ToString());
            });

            EditorGUILayout.EndScrollView();
        }

        private static void DrawSection(string title, System.Action drawBody)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            drawBody?.Invoke();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space(4f);
        }
    }
}
