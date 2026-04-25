using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    [CustomEditor(typeof(LevelHubPreviewController))]
    public sealed class LevelHubPreviewControllerEditor : UnityEditor.Editor
    {
        private const int RailSamplesPerSegment = 20;
        private SerializedProperty levelNodesProperty;
        private SerializedProperty levelNumberLabelsProperty;
        private SerializedProperty oynaSubtitleLabelProperty;
        private SerializedProperty railPointsProperty;
        private SerializedProperty totalLevelsProperty;
        private SerializedProperty dragPixelsPerLevelProperty;
        private SerializedProperty snapSharpnessProperty;
        private int selectedPointIndex;
        private LevelHubPreviewController pendingRailSceneSaveController;

        [MenuItem("Tools/WordSpin Alpha/Hub Preview/Select Level Hub Rail Editor")]
        public static void SelectActiveRailEditor()
        {
            LevelHubPreviewController controller = FindActiveSceneController();
            if (controller == null)
            {
                EditorUtility.DisplayDialog(
                    "WordSpin Alpha",
                    "Aktif sahnede LevelHubPreviewController bulunamadi. Once HubPreview sahnesini veya level hub preview prefab instance'ini ac.",
                    "Tamam");
                return;
            }

            Selection.activeObject = controller;
            EditorGUIUtility.PingObject(controller);
            SceneView.lastActiveSceneView?.FrameSelected();
        }

        private void OnEnable()
        {
            levelNodesProperty = serializedObject.FindProperty("levelNodes");
            levelNumberLabelsProperty = serializedObject.FindProperty("levelNumberLabels");
            oynaSubtitleLabelProperty = serializedObject.FindProperty("oynaSubtitleLabel");
            railPointsProperty = serializedObject.FindProperty("railPoints");
            totalLevelsProperty = serializedObject.FindProperty("totalLevels");
            dragPixelsPerLevelProperty = serializedObject.FindProperty("dragPixelsPerLevel");
            snapSharpnessProperty = serializedObject.FindProperty("snapSharpness");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            LevelHubPreviewController controller = (LevelHubPreviewController)target;
            EditorGUILayout.HelpBox(
                "Ray noktalarini Scene View uzerinde, arka plan gorselini gorerek surukle. Veri bu controller uzerinde serialize edilir; HubPreview sahnesi ana Hub'a kopyalanirsa ray ayarlari da beraber gider.",
                MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Scene View'de Goster"))
                {
                    Selection.activeObject = controller;
                    EditorGUIUtility.PingObject(controller);
                    SceneView.lastActiveSceneView?.FrameSelected();
                }

                if (GUILayout.Button("Ray'i Varsayilana Al"))
                {
                    Undo.RecordObject(controller, "Reset Level Hub Rail");
                    controller.ResetRailToDefault();
                    EditorUtility.SetDirty(controller);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
                    MarkAndSaveScene(controller.gameObject.scene);
                    serializedObject.Update();
                }
            }

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Preview Davranisi", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(totalLevelsProperty, new GUIContent("Toplam Level"));
            EditorGUILayout.PropertyField(dragPixelsPerLevelProperty, new GUIContent("Drag Hassasiyeti"));
            EditorGUILayout.PropertyField(snapSharpnessProperty, new GUIContent("Snap Hiz"));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Ray Noktalari", EditorStyles.boldLabel);
            DrawRailPoints(controller);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Baglantilar", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(levelNodesProperty, true);
            EditorGUILayout.PropertyField(levelNumberLabelsProperty, true);
            EditorGUILayout.PropertyField(oynaSubtitleLabelProperty);

            if (serializedObject.ApplyModifiedProperties())
            {
                controller.EditorRefresh();
                EditorUtility.SetDirty(controller);
                PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
                MarkAndSaveScene(controller.gameObject.scene);
                SceneView.RepaintAll();
            }
        }

        private void OnSceneGUI()
        {
            LevelHubPreviewController controller = (LevelHubPreviewController)target;
            RectTransform root = controller.transform as RectTransform;
            if (root == null || controller.RailPointCount == 0)
            {
                return;
            }

            if (Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            }

            Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
            DrawRailLines(controller, root);
            DrawPointHandles(controller, root);

            if (pendingRailSceneSaveController != null && IsMouseUpEvent(Event.current))
            {
                MarkAndSaveScene(pendingRailSceneSaveController.gameObject.scene);
                pendingRailSceneSaveController = null;
            }
        }

        private void DrawRailPoints(LevelHubPreviewController controller)
        {
            if (railPointsProperty == null)
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Nokta Ekle"))
                {
                    int insertIndex = Mathf.Clamp(selectedPointIndex + 1, 0, railPointsProperty.arraySize);
                    railPointsProperty.InsertArrayElementAtIndex(insertIndex);
                    SerializedProperty inserted = railPointsProperty.GetArrayElementAtIndex(insertIndex);
                    Vector2 position = Vector2.zero;
                    float scale = 0.5f;
                    if (insertIndex > 0)
                    {
                        SerializedProperty previous = railPointsProperty.GetArrayElementAtIndex(insertIndex - 1);
                        position = previous.FindPropertyRelative("position").vector2Value + new Vector2(0f, 90f);
                        scale = previous.FindPropertyRelative("scale").floatValue;
                    }

                    inserted.FindPropertyRelative("position").vector2Value = position;
                    inserted.FindPropertyRelative("scale").floatValue = scale;
                    inserted.FindPropertyRelative("rotation").floatValue = 0f;
                    inserted.FindPropertyRelative("alpha").floatValue = 1f;
                    selectedPointIndex = insertIndex;
                }

                using (new EditorGUI.DisabledScope(railPointsProperty.arraySize <= 2))
                {
                    if (GUILayout.Button("Secili Noktayi Sil"))
                    {
                        int removeIndex = Mathf.Clamp(selectedPointIndex, 0, railPointsProperty.arraySize - 1);
                        railPointsProperty.DeleteArrayElementAtIndex(removeIndex);
                        selectedPointIndex = Mathf.Clamp(removeIndex - 1, 0, railPointsProperty.arraySize - 1);
                    }
                }
            }

            for (int i = 0; i < railPointsProperty.arraySize; i++)
            {
                SerializedProperty point = railPointsProperty.GetArrayElementAtIndex(i);
                SerializedProperty position = point.FindPropertyRelative("position");
                SerializedProperty scale = point.FindPropertyRelative("scale");
                SerializedProperty rotation = point.FindPropertyRelative("rotation");
                SerializedProperty alpha = point.FindPropertyRelative("alpha");

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        bool selected = selectedPointIndex == i;
                        if (GUILayout.Toggle(selected, $"Ray Noktasi {i}", "Button", GUILayout.Width(120f)) && !selected)
                        {
                            selectedPointIndex = i;
                            SceneView.RepaintAll();
                        }

                        EditorGUILayout.PropertyField(position, GUIContent.none);
                    }

                    EditorGUILayout.Slider(scale, 0.05f, 2f, new GUIContent("Scale"));
                    EditorGUILayout.PropertyField(rotation, new GUIContent("Rotation"));
                    EditorGUILayout.Slider(alpha, 0f, 1f, new GUIContent("Alpha"));
                }
            }
        }

        private void DrawRailLines(LevelHubPreviewController controller, RectTransform root)
        {
            if (controller.RailPointCount < 2)
            {
                return;
            }

            int segmentCount = controller.RailPointCount - 1;
            int sampleCount = segmentCount * RailSamplesPerSegment + 1;
            Vector3[] worldPoints = new Vector3[Mathf.Max(2, sampleCount)];

            for (int i = 0; i < worldPoints.Length; i++)
            {
                float t = (float)i / (worldPoints.Length - 1) * segmentCount;
                worldPoints[i] = root.TransformPoint(controller.SampleRailPosition(t));
            }

            Handles.color = new Color(1f, 0.7f, 0.12f, 0.95f);
            Handles.DrawAAPolyLine(5f, worldPoints);
        }

        private void DrawPointHandles(LevelHubPreviewController controller, RectTransform root)
        {
            float handleSize = HandleUtility.GetHandleSize(root.position) * 0.045f;

            for (int i = 0; i < controller.RailPointCount; i++)
            {
                LevelHubPreviewController.RailPoint point = controller.GetRailPoint(i);
                Vector3 world = root.TransformPoint(point.position);
                Handles.color = i == selectedPointIndex ? new Color(0.2f, 1f, 0.55f, 1f) : new Color(1f, 0.78f, 0.16f, 1f);

                if (Handles.Button(world, Quaternion.identity, handleSize, handleSize * 1.25f, Handles.SphereHandleCap))
                {
                    selectedPointIndex = i;
                    Repaint();
                }

                Handles.Label(world + Vector3.up * handleSize * 1.6f, $"R{i}");

                if (i != selectedPointIndex)
                {
                    continue;
                }

                EditorGUI.BeginChangeCheck();
                Vector3 movedWorld = Handles.PositionHandle(world, Quaternion.identity);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(controller, "Move Level Hub Rail Point");
                    point.position = root.InverseTransformPoint(movedWorld);
                    controller.SetRailPoint(i, point);
                    EditorUtility.SetDirty(controller);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
                    EditorSceneManager.MarkSceneDirty(controller.gameObject.scene);
                    pendingRailSceneSaveController = controller;
                }
            }
        }

        private static LevelHubPreviewController FindActiveSceneController()
        {
            LevelHubPreviewController[] controllers = Resources.FindObjectsOfTypeAll<LevelHubPreviewController>();
            for (int i = 0; i < controllers.Length; i++)
            {
                LevelHubPreviewController controller = controllers[i];
                if (controller == null || !controller.gameObject.scene.IsValid())
                {
                    continue;
                }

                return controller;
            }

            return null;
        }

        private static bool IsMouseUpEvent(Event currentEvent)
        {
            return currentEvent != null && (currentEvent.type == EventType.MouseUp || currentEvent.rawType == EventType.MouseUp);
        }

        private static void MarkAndSaveScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            if (!string.IsNullOrWhiteSpace(scene.path))
            {
                EditorSceneManager.SaveScene(scene);
            }
        }
    }
}
