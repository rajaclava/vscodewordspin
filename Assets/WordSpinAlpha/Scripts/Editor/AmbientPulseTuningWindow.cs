using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    public class AmbientPulseTuningWindow : EditorWindow
    {
        private Vector2 _scroll;
        private ThemeRuntimeController _themeRuntime;
        private SerializedObject _themeRuntimeObject;
        private bool _showPlacement = true;
        private bool _showScale = true;
        private bool _showPulse = true;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Ambiyans ve Pulse Ayarlari")]
        public static void Open()
        {
            GetWindow<AmbientPulseTuningWindow>("Ambiyans ve Pulse");
        }

        private void OnEnable()
        {
            ResolveTarget();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Ambiyans ve Pulse Ayarlari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu pencere tema runtime icindeki arka glow, orbit pulse ve yan ambiyans yuzeylerinin mikro davranisini ayarlar. Sadece presentation katmanina etki eder.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Sahneyi Yeniden Tara", GUILayout.Height(28f)))
                {
                    ResolveTarget();
                }

                string saveLabel = Application.isPlaying ? "Uygula" : "Kaydet";
                using (new EditorGUI.DisabledScope(_themeRuntimeObject == null))
                {
                    if (GUILayout.Button(saveLabel, GUILayout.Height(28f)))
                    {
                        Save();
                    }
                }
            }

            if (_themeRuntimeObject == null)
            {
                EditorGUILayout.HelpBox("Sahnede ThemeRuntimeController bulunamadi.", MessageType.Warning);
                return;
            }

            _themeRuntimeObject.Update();
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            _showPlacement = EditorGUILayout.BeginFoldoutHeaderGroup(_showPlacement, "Konumlar");
            if (_showPlacement)
            {
                DrawVector3("Arka Glow Konumu", "backgroundGlowPosition");
                DrawVector3("Sol Ambiyans Konumu", "ambienceLeftPosition");
                DrawVector3("Sag Ambiyans Konumu", "ambienceRightPosition");
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showScale = EditorGUILayout.BeginFoldoutHeaderGroup(_showScale, "Olcek ve Nefes");
            if (_showScale)
            {
                DrawFloat("Glow Temel Olcek Carpani", "backgroundGlowBaseScaleMultiplier");
                DrawFloat("Ambiyans Min Olcek Carpani", "ambienceBaseScaleMultiplierMin");
                DrawFloat("Ambiyans Max Olcek Carpani", "ambienceBaseScaleMultiplierMax");
                DrawFloat("Glow Nefes Etkisi", "backgroundGlowBreathScale");
                DrawFloat("Glow Flow Etkisi", "backgroundGlowFlowScaleBoost");
                DrawFloat("Ambiyans Dalga Etkisi", "ambienceWaveScale");
                DrawFloat("Ambiyans Flow Etkisi", "ambienceFlowScaleBoost");
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showPulse = EditorGUILayout.BeginFoldoutHeaderGroup(_showPulse, "Pulse Bantlari");
            if (_showPulse)
            {
                DrawPulseBand("Orbit Pulse", "orbitPulseTuning");
                EditorGUILayout.Space(4f);
                DrawPulseBand("Glow Pulse", "glowPulseTuning");
                EditorGUILayout.Space(4f);
                DrawPulseBand("Sol Ambiyans Pulse", "leftAmbientPulseTuning");
                EditorGUILayout.Space(4f);
                DrawPulseBand("Sag Ambiyans Pulse", "rightAmbientPulseTuning");
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
            _themeRuntimeObject.ApplyModifiedProperties();
        }

        private void ResolveTarget()
        {
            _themeRuntime = Object.FindObjectOfType<ThemeRuntimeController>(true);
            _themeRuntimeObject = _themeRuntime != null ? new SerializedObject(_themeRuntime) : null;
        }

        private void TryAutoRefresh()
        {
            if (!WordSpinAlphaEditorSyncUtility.ConsumeChanges(WordSpinAlphaEditorSyncKind.Scene, ref _syncStamp))
            {
                return;
            }

            ResolveTarget();
            Repaint();
        }

        private void DrawFloat(string label, string propertyName)
        {
            EditorGUILayout.PropertyField(_themeRuntimeObject.FindProperty(propertyName), new GUIContent(label));
        }

        private void DrawVector3(string label, string propertyName)
        {
            EditorGUILayout.PropertyField(_themeRuntimeObject.FindProperty(propertyName), new GUIContent(label));
        }

        private void DrawPulseBand(string label, string propertyName)
        {
            SerializedProperty band = _themeRuntimeObject.FindProperty(propertyName);
            if (band == null)
            {
                EditorGUILayout.HelpBox($"{label} bulunamadi.", MessageType.Warning);
                return;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(band.FindPropertyRelative("baseSpeed"), new GUIContent("Temel Hiz"));
            EditorGUILayout.PropertyField(band.FindPropertyRelative("flowSpeed"), new GUIContent("Flow Hiz Katkisi"));
            EditorGUILayout.PropertyField(band.FindPropertyRelative("momentumSpeedStep"), new GUIContent("Momentum Hiz Adimi"));
            EditorGUILayout.PropertyField(band.FindPropertyRelative("baseScaleAmplitude"), new GUIContent("Temel Olcek Genligi"));
            EditorGUILayout.PropertyField(band.FindPropertyRelative("flowScaleAmplitude"), new GUIContent("Flow Olcek Katkisi"));
            EditorGUILayout.PropertyField(band.FindPropertyRelative("baseAlphaAmplitude"), new GUIContent("Temel Alfa Genligi"));
            EditorGUILayout.PropertyField(band.FindPropertyRelative("flowAlphaAmplitude"), new GUIContent("Flow Alfa Katkisi"));
            EditorGUILayout.EndVertical();
        }

        private void Save()
        {
            if (_themeRuntime == null)
            {
                return;
            }

            EditorUtility.SetDirty(_themeRuntime);
            WordSpinAlphaEditorRuntimeRefreshUtility.SaveDirtyAssets();
            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshThemePresentation();
            }
            else
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.MarkCurrentSceneDirty();
            }

            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.Scene);
        }
    }
}
