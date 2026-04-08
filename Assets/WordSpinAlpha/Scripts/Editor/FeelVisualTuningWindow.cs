using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Core;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    public class FeelVisualTuningWindow : EditorWindow
    {
        private const string ScoreProfileAssetPath = "Assets/WordSpinAlpha/Resources/Configs/ScoreTuningProfile.asset";

        private Vector2 _scroll;
        private bool _showScore = true;
        private bool _showImpact = true;
        private bool _showThemeRuntime = true;

        private ScoreTuningProfile _scoreProfile;
        private SerializedObject _scoreProfileObject;
        private ImpactFeedbackController _impactController;
        private SerializedObject _impactControllerObject;
        private ImpactFeelProfile _impactProfile;
        private SerializedObject _impactProfileObject;
        private ThemeRuntimeController _themeRuntime;
        private SerializedObject _themeRuntimeObject;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Hissiyat ve Gorsel Ayarlari")]
        public static void Open()
        {
            GetWindow<FeelVisualTuningWindow>("Hissiyat ve Gorsel");
        }

        private void OnEnable()
        {
            ResolveTargets();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Hissiyat ve Gorsel Ayarlari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu pencere skor profili, impact hissi ve aktif tema runtime tonlamalarini tek yerden ayarlar.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Varliklari Yeniden Oku", GUILayout.Height(28f)))
                {
                    ResolveTargets();
                }

                string saveLabel = Application.isPlaying ? "Kaydet ve Uygula" : "Kaydet";
                if (GUILayout.Button(saveLabel, GUILayout.Height(28f)))
                {
                    Save();
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            _showScore = EditorGUILayout.BeginFoldoutHeaderGroup(_showScore, "Skor Profili");
            if (_showScore)
            {
                DrawScoreProfile();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showImpact = EditorGUILayout.BeginFoldoutHeaderGroup(_showImpact, "Impact ve Vurus Hissi");
            if (_showImpact)
            {
                DrawImpactProfile();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showThemeRuntime = EditorGUILayout.BeginFoldoutHeaderGroup(_showThemeRuntime, "Tema Runtime Tonlamalari");
            if (_showThemeRuntime)
            {
                DrawThemeRuntime();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
        }

        private void ResolveTargets()
        {
            _scoreProfile = AssetDatabase.LoadAssetAtPath<ScoreTuningProfile>(ScoreProfileAssetPath);
            _scoreProfileObject = _scoreProfile != null ? new SerializedObject(_scoreProfile) : null;

            _impactController = Object.FindObjectOfType<ImpactFeedbackController>(true);
            _impactControllerObject = _impactController != null ? new SerializedObject(_impactController) : null;

            if (_impactControllerObject != null)
            {
                _impactProfile = _impactControllerObject.FindProperty("profile").objectReferenceValue as ImpactFeelProfile;
            }

            _impactProfileObject = _impactProfile != null ? new SerializedObject(_impactProfile) : null;
            _themeRuntime = Object.FindObjectOfType<ThemeRuntimeController>(true);
            _themeRuntimeObject = _themeRuntime != null ? new SerializedObject(_themeRuntime) : null;
        }

        private void TryAutoRefresh()
        {
            if (!WordSpinAlphaEditorSyncUtility.ConsumeChanges(WordSpinAlphaEditorSyncKind.Scene | WordSpinAlphaEditorSyncKind.ScriptableAssets, ref _syncStamp))
            {
                return;
            }

            ResolveTargets();
            Repaint();
        }

        private void DrawScoreProfile()
        {
            if (_scoreProfileObject == null)
            {
                EditorGUILayout.HelpBox("ScoreTuningProfile asset'i bulunamadi.", MessageType.Warning);
                return;
            }

            _scoreProfileObject.Update();
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("perfectBasePoints"), new GUIContent("Perfect Baz Puan"));
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("goodBasePoints"), new GUIContent("Good Baz Puan"));
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("goodUsesCurrentMultiplier"), new GUIContent("Good Mevcut Carpan Kullanir"));
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("goodResetsPerfectChain"), new GUIContent("Good Perfect Zinciri Sifirlar"));
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("perfectMultiplierTiers"), new GUIContent("Perfect Zincir Katlari"), true);
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("reactionBonusWindows"), new GUIContent("Reaksiyon Bonus Pencereleri"), true);
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("levelClearBonus"), new GUIContent("Seviye Bitirme Bonusu"));
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("noMistakeBonus"), new GUIContent("Hatasiz Bonus"));
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("allPerfectBonus"), new GUIContent("Tum Mukemmel Bonus"));
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("clearTimeBonusWindows"), new GUIContent("Bitis Suresi Bonuslari"), true);
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("defaultTimedModeLimitSeconds"), new GUIContent("Varsayilan Sureli Mod Limiti"));
            EditorGUILayout.PropertyField(_scoreProfileObject.FindProperty("remainingTimeBonusPerSecond"), new GUIContent("Kalan Sure Bonus / Sn"));
            _scoreProfileObject.ApplyModifiedProperties();
        }

        private void DrawImpactProfile()
        {
            if (_impactControllerObject == null)
            {
                EditorGUILayout.HelpBox("Sahnede ImpactFeedbackController bulunamadi.", MessageType.Warning);
                return;
            }

            _impactControllerObject.Update();
            EditorGUILayout.PropertyField(_impactControllerObject.FindProperty("vibrationEnabled"), new GUIContent("Titresim Acik"));
            _impactControllerObject.ApplyModifiedProperties();

            if (_impactProfileObject == null)
            {
                EditorGUILayout.HelpBox("ImpactFeelProfile referansi bulunamadi.", MessageType.Warning);
                return;
            }

            _impactProfileObject.Update();
            EditorGUILayout.PropertyField(_impactProfileObject.FindProperty("entries"), new GUIContent("Vurus Tipi Ayarlari"), true);
            _impactProfileObject.ApplyModifiedProperties();
        }

        private void DrawThemeRuntime()
        {
            if (_themeRuntimeObject == null)
            {
                EditorGUILayout.HelpBox("Sahnede ThemeRuntimeController bulunamadi.", MessageType.Warning);
                return;
            }

            _themeRuntimeObject.Update();
            EditorGUILayout.PropertyField(_themeRuntimeObject.FindProperty("perfectPitchStep"), new GUIContent("Mukemmel Ses Adimi"));
            EditorGUILayout.PropertyField(_themeRuntimeObject.FindProperty("perfectPitchMaxBoost"), new GUIContent("Mukemmel Ses Tavan Artisi"));
            EditorGUILayout.PropertyField(_themeRuntimeObject.FindProperty("mobileGlowAlphaScale"), new GUIContent("Glow Alfa Carpani"));
            EditorGUILayout.PropertyField(_themeRuntimeObject.FindProperty("mobileAmbientScale"), new GUIContent("Ambiyans Olcegi"));
            EditorGUILayout.PropertyField(_themeRuntimeObject.FindProperty("cameraContrastBias"), new GUIContent("Kamera Kontrast Baskisi"));
            _themeRuntimeObject.ApplyModifiedProperties();
        }

        private void Save()
        {
            if (_scoreProfile != null)
            {
                EditorUtility.SetDirty(_scoreProfile);
            }

            if (_impactController != null)
            {
                EditorUtility.SetDirty(_impactController);
            }

            if (_impactProfile != null)
            {
                EditorUtility.SetDirty(_impactProfile);
            }

            if (_themeRuntime != null)
            {
                EditorUtility.SetDirty(_themeRuntime);
            }

            WordSpinAlphaEditorRuntimeRefreshUtility.SaveDirtyAssets();
            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshThemePresentation();
            }
            else
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.MarkCurrentSceneDirty();
            }

            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.Scene | WordSpinAlphaEditorSyncKind.ScriptableAssets);
        }
    }
}
