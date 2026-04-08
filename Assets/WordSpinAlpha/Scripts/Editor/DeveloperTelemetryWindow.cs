using System.IO;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Core;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Editor
{
    public class DeveloperTelemetryWindow : EditorWindow
    {
        private Vector2 _scroll;
        private TelemetrySnapshotData _snapshot;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Gelistirici Telemetry Paneli")]
        public static void Open()
        {
            GetWindow<DeveloperTelemetryWindow>("WordSpin Telemetry");
        }

        private void OnEnable()
        {
            RefreshSnapshot();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("WordSpin Gelistirici Telemetry Paneli", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu panel yerel telemetry snapshot dosyasini okur. AI hotfix ve cloud yayin akisina gidecek verinin editor tarafindaki ilk gorunumudur.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Snapshot Yenile", GUILayout.Height(26f)))
                {
                    RefreshSnapshot();
                }

                if (GUILayout.Button("Remote Override'lari Yenile", GUILayout.Height(26f)))
                {
                    if (ContentService.Instance != null)
                    {
                        ContentService.Instance.RefreshRemoteOverrides();
                    }

                    RefreshSnapshot();
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Telemetry Dosyasi", SnapshotPath);
            EditorGUILayout.LabelField("Remote Icerik Klasoru", RemoteDirectoryPath);
            EditorGUILayout.Space(8f);

            if (_snapshot == null)
            {
                EditorGUILayout.HelpBox("Henuz telemetry snapshot bulunamadi.", MessageType.Warning);
                return;
            }

            EditorGUILayout.LabelField("Olusturulma", _snapshot.generatedAtUtc ?? "-");
            EditorGUILayout.LabelField("Manifest", _snapshot.manifestVersion ?? "local-only");
            EditorGUILayout.LabelField("Bekleyen Event", _snapshot.pendingEventCount.ToString());

            EditorGUILayout.Space(10f);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (LevelTelemetrySummary summary in _snapshot.levelSummaries)
            {
                DrawLevelSummary(summary);
                EditorGUILayout.Space(10f);
            }
            EditorGUILayout.EndScrollView();
        }

        private void DrawLevelSummary(LevelTelemetrySummary summary)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Seviye {summary.levelId}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Tema", summary.themeId ?? "-");
            EditorGUILayout.LabelField("Zorluk", summary.difficultyProfileId ?? "-");
            EditorGUILayout.LabelField("Ritim", summary.rhythmProfileId ?? "-");
            EditorGUILayout.LabelField("Baslangic", summary.starts.ToString());
            EditorGUILayout.LabelField("Tamamlama", summary.completes.ToString());
            EditorGUILayout.LabelField("Mukemmel Vurus", summary.perfectHits.ToString());
            EditorGUILayout.LabelField("Iyi Vurus", summary.goodHits.ToString());
            EditorGUILayout.LabelField("Yakin Iska", summary.nearMisses.ToString());
            EditorGUILayout.LabelField("Iska", summary.misses.ToString());
            EditorGUILayout.LabelField("Yanlis Slot", summary.wrongSlots.ToString());
            EditorGUILayout.LabelField("Yanlis Harf", summary.wrongLetters.ToString());
            EditorGUILayout.LabelField("Tekrar Dene", summary.retries.ToString());
            EditorGUILayout.LabelField("Devam Et", summary.continues.ToString());
            EditorGUILayout.LabelField("En Yuksek Perfect Combo", summary.highestPerfectCombo.ToString());

            float averageQuestionTime = summary.questionTimeSamples > 0 ? summary.totalQuestionTime / summary.questionTimeSamples : 0f;
            EditorGUILayout.LabelField("Ortalama Soru Suresi", averageQuestionTime.ToString("0.00") + " sn");
            EditorGUILayout.Space(4f);
            EditorGUILayout.HelpBox(summary.recommendation ?? "Oneri yok.", MessageType.None);
            EditorGUILayout.EndVertical();
        }

        private void TryAutoRefresh()
        {
            if (!WordSpinAlphaEditorSyncUtility.ConsumeChanges(WordSpinAlphaEditorSyncKind.Telemetry | WordSpinAlphaEditorSyncKind.RuntimeConfig, ref _syncStamp))
            {
                return;
            }

            RefreshSnapshot();
        }

        private void RefreshSnapshot()
        {
            _snapshot = null;
            if (!File.Exists(SnapshotPath))
            {
                Repaint();
                return;
            }

            try
            {
                string json = File.ReadAllText(SnapshotPath);
                _snapshot = JsonUtility.FromJson<TelemetrySnapshotData>(json);
            }
            catch
            {
                _snapshot = null;
            }

            Repaint();
        }

        private static string SnapshotPath => Path.Combine(Application.persistentDataPath, GameConstants.TelemetrySnapshotFileName);
        private static string RemoteDirectoryPath => Path.Combine(Application.persistentDataPath, GameConstants.RemoteContentDirectory);
    }
}
