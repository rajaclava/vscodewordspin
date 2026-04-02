using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    [CustomEditor(typeof(GameplaySceneTuner))]
    public class GameplaySceneTunerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.HelpBox("Gameplay oranlarini burada canli ayarla. Apply Now play mode'da da guvenle calisir.", MessageType.Info);

            DrawSection(
                "Scene References",
                "gameplayCamera",
                "rotatorRoot",
                "rotatorVisualRoot",
                "targetRotator",
                "slotAnchors",
                "launcherBody",
                "pinSpawnPoint",
                "flightLane",
                "pinLauncher",
                "topBar",
                "questionPanel",
                "bottomBar",
                "keyboardGrid",
                "keyboardGridLayout");

            DrawSection("Camera", "cameraSize");
            DrawSection("Rotator", "rotatorPosition", "rotatorScale", "rotatorSpeed", "clockwise");
            DrawSection("Anchors", "anchorRadius");
            DrawSection("Launcher & Pin", "launcherPosition", "launcherScale", "pinSpawnLocalOffset", "pinScale", "pinLoadTweenDuration");
            DrawSection("Flight Lane", "flightLanePosition", "flightLaneScale");
            DrawSection("Panels", "topBarAnchoredPosition", "topBarSize", "questionPanelAnchoredPosition", "questionPanelSize", "bottomBarAnchoredPosition", "bottomBarSize");
            DrawSection("Keyboard", "keyboardGridAnchoredPosition", "keyboardGridSize", "keyboardCellSize", "keyboardSpacing");
            DrawSection("Advanced", "autoApplyInEditMode");

            EditorGUILayout.Space(10f);

            GameplaySceneTuner tuner = (GameplaySceneTuner)target;
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Apply Now", GUILayout.Height(28f)))
                {
                    tuner.ApplyTuning();
                    EditorUtility.SetDirty(tuner);
                    if (!Application.isPlaying)
                    {
                        EditorSceneManager.MarkSceneDirty(tuner.gameObject.scene);
                    }
                }

                if (GUILayout.Button("Capture From Scene", GUILayout.Height(28f)))
                {
                    Undo.RecordObject(tuner, "Capture Gameplay Tuning");
                    tuner.CaptureFromScene();
                    EditorUtility.SetDirty(tuner);
                }
            }

            if (GUILayout.Button("Reset To Builder Defaults", GUILayout.Height(28f)))
            {
                Undo.RecordObject(tuner, "Reset Gameplay Tuning");
                tuner.ResetToDefaults();
                EditorUtility.SetDirty(tuner);
                if (!Application.isPlaying)
                {
                    EditorSceneManager.MarkSceneDirty(tuner.gameObject.scene);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawSection(string title, params string[] propertyNames)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            foreach (string propertyName in propertyNames)
            {
                SerializedProperty property = serializedObject.FindProperty(propertyName);
                if (property != null)
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }
        }
    }
}
