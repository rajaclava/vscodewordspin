using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Editor
{
    public class TelemetryPolicyWindow : EditorWindow
    {
        private const string AssetPath = "Assets/WordSpinAlpha/Resources/Configs/TelemetryPolicyProfile.asset";

        private TelemetryPolicyProfile _profile;
        private SerializedObject _profileObject;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Telemetry Politikasi")]
        public static void Open()
        {
            GetWindow<TelemetryPolicyWindow>("Telemetry Politikasi");
        }

        private void OnEnable()
        {
            _profile = EnsureProfile();
            _profileObject = _profile != null ? new SerializedObject(_profile) : null;
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Telemetry Politikasi", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu pencere telemetry kuyruk limiti, yazma araligi ve snapshot agirligini ayarlar. Mekaniklere dokunmaz; sadece telemetry runtime maliyetini kontrol eder.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Profil Assetini Sec", GUILayout.Height(28f)))
                {
                    Selection.activeObject = _profile;
                    EditorGUIUtility.PingObject(_profile);
                }

                string saveLabel = Application.isPlaying ? "Kaydet ve Uygula" : "Kaydet";
                if (GUILayout.Button(saveLabel, GUILayout.Height(28f)))
                {
                    Save();
                }
            }

            if (_profileObject == null)
            {
                EditorGUILayout.HelpBox("Telemetry policy asset'i bulunamadi.", MessageType.Warning);
                return;
            }

            _profileObject.Update();
            EditorGUILayout.PropertyField(_profileObject.FindProperty("telemetryEnabled"), new GUIContent("Telemetry Acik"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("writeThrottleSeconds"), new GUIContent("Dosya Yazma Araligi (sn)"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("maxQueuedEvents"), new GUIContent("Maks Kuyruk Event"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("maxSnapshotLevelSummaries"), new GUIContent("Maks Snapshot Ozet Seviyesi"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("trimQueueOnLoad"), new GUIContent("Acilista Kuyrugu Kisa"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("savePendingEventCountToSaveData"), new GUIContent("Pending Sayisini Save Dosyasina Yaz"));
            EditorGUILayout.PropertyField(_profileObject.FindProperty("flushOnApplicationPause"), new GUIContent("Pause Oldugunda Flush Et"));
            _profileObject.ApplyModifiedProperties();
        }

        private void TryAutoRefresh()
        {
            if (!WordSpinAlphaEditorSyncUtility.ConsumeChanges(WordSpinAlphaEditorSyncKind.ScriptableAssets | WordSpinAlphaEditorSyncKind.Telemetry, ref _syncStamp))
            {
                return;
            }

            _profile = EnsureProfile();
            _profileObject = _profile != null ? new SerializedObject(_profile) : null;
            Repaint();
        }

        private void Save()
        {
            if (_profile == null)
            {
                return;
            }

            _profile.ClampToSafeDefaults();
            EditorUtility.SetDirty(_profile);
            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.ScriptableAssets | WordSpinAlphaEditorSyncKind.Telemetry);
            WordSpinAlphaEditorRuntimeRefreshUtility.SaveDirtyAssets();
            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshTelemetryPolicy();
            }
        }

        private static TelemetryPolicyProfile EnsureProfile()
        {
            TelemetryPolicyProfile profile = AssetDatabase.LoadAssetAtPath<TelemetryPolicyProfile>(AssetPath);
            if (profile != null)
            {
                profile.ClampToSafeDefaults();
                return profile;
            }

            EnsureFolder("Assets/WordSpinAlpha/Resources");
            EnsureFolder("Assets/WordSpinAlpha/Resources/Configs");

            profile = CreateInstance<TelemetryPolicyProfile>();
            profile.ClampToSafeDefaults();
            AssetDatabase.CreateAsset(profile, AssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return profile;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
