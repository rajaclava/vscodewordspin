using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using WordSpinAlpha.Core;
using WordSpinAlpha.Presentation;
using Object = UnityEngine.Object;

namespace WordSpinAlpha.Editor
{
    [CustomEditor(typeof(HubPreviewLayoutTuningProfile))]
    public sealed class HubPreviewLayoutTuningProfileEditor : UnityEditor.Editor
    {
        private const string MenuPath = "Tools/WordSpin Alpha/Hub Preview/Alttas Layout Tuner";
        private const string MainMenuPreviewRootName = "MainMenuPngPreviewRoot";
        private const string MainMenuRotatorObjectName = "Rotator";
        private const string HubPreviewRootName = "LevelHubPreviewRoot";
        private const string BottomPageNavObjectName = "BottomPageNav";
        private const string OynaButtonObjectName = "OynaBg";
        private static RectTransform s_AlttasMoveTarget;
        private static bool s_AlttasMoveUndoRecorded;
        private static RectTransform s_AlttasResizeTarget;
        private static bool s_AlttasResizeUndoRecorded;
        private static RectTransform s_BottomPageNavMoveTarget;
        private static bool s_BottomPageNavMoveUndoRecorded;
        private static RectTransform s_BottomPageNavResizeTarget;
        private static bool s_BottomPageNavResizeUndoRecorded;
        private static RectTransform s_MainMenuRotatorMoveTarget;
        private static bool s_MainMenuRotatorMoveUndoRecorded;
        private static RectTransform s_OynaButtonMoveTarget;
        private static bool s_OynaButtonMoveUndoRecorded;
        private static RectTransform s_OynaButtonResizeTarget;
        private static bool s_OynaButtonResizeUndoRecorded;

        [InitializeOnLoadMethod]
        private static void RegisterSceneViewCallbacks()
        {
            SceneView.duringSceneGui -= HandleSceneViewGUI;
            SceneView.duringSceneGui += HandleSceneViewGUI;
            // PHASE 1: Disable forced layout sync on scene save
            // EditorSceneManager.sceneSaving -= SyncHubPreviewLayoutProfileBeforeSceneSave;
            // EditorSceneManager.sceneSaving += SyncHubPreviewLayoutProfileBeforeSceneSave;
        }

        [MenuItem(MenuPath)]
        public static void OpenTuner()
        {
            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile == null)
            {
                Debug.LogWarning("[WordSpinAlpha] HubPreviewLayoutTuningProfile asset not found.");
                return;
            }

            Selection.activeObject = profile;
            EditorGUIUtility.PingObject(profile);
        }

        public override void OnInspectorGUI()
        {
            HubPreviewLayoutTuningProfile profile = (HubPreviewLayoutTuningProfile)target;
            if (profile == null)
            {
                EditorGUILayout.HelpBox("Profile not found.", MessageType.Warning);
                return;
            }

            bool hadAlttasBefore = profile.GetElement(HubPreviewLayoutTuningProfile.AlttasElementId) != null;
            bool hadMainMenuRotatorBefore = profile.GetElement(HubPreviewLayoutTuningProfile.MainMenuRotatorElementId) != null;
            bool hadBottomPageNavBefore = profile.GetElement(HubPreviewLayoutTuningProfile.BottomPageNavElementId) != null;
            bool hadOynaButtonBefore = profile.GetElement(HubPreviewLayoutTuningProfile.HubPreviewOynaButtonElementId) != null;
            profile.EnsureDefaults();
            bool normalized = profile.NormalizeLayoutElements();
            if (!hadAlttasBefore || !hadMainMenuRotatorBefore || !hadBottomPageNavBefore || !hadOynaButtonBefore || normalized)
            {
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.HelpBox(
                "Alttas current tuning and baseline reference live in the same panel. Reset returns current values to the baseline without touching the scene; Apply pushes the current values to the scene/prefab. Capture Selected + Apply and Rebuild stay explicit actions.",
                MessageType.Info);

            DrawAlttasLayoutSection(
                profile,
                HubPreviewLayoutTuningProfile.AlttasElementId,
                "Alttas",
                "Edit HubPreview Alttas Layout",
                "Capture HubPreview Alttas Layout",
                "Set Current As Baseline",
                "Select Alttas + Align View",
                "Capture Selected + Apply",
                "Apply",
                "Reset To Baseline",
                "Rebuild Preview From Config",
                FindSceneAlttas,
                FindSelectedSceneAlttas,
                WordSpinAlphaSceneBuilder.RebuildLevelHubPreviewScene,
                "No Alttas was found in the active scene. Open the HubPreview scene first.",
                "No Alttas was found in the active scene. Open the HubPreview scene first and select Alttas.",
                "No Alttas was found in the active scene. Open the HubPreview scene first.",
                AlignSceneViewForAlttasEdit);

            DrawBottomPageNavSection(profile);

            DrawOynaButtonSection(profile);

            DrawLayoutSection(
                profile,
                HubPreviewLayoutTuningProfile.MainMenuRotatorElementId,
                "MainMenu Rotator",
                "Edit MainMenu Rotator Layout",
                "Capture MainMenu Rotator Layout",
                "Reset MainMenu Rotator Layout",
                "Select Scene Rotator",
                "Capture Selected + Apply",
                "Rebuild Preview From Config",
                "Reset Rotator To Defaults + Apply",
                FindSceneMainMenuRotator,
                FindSelectedSceneMainMenuRotator,
                WordSpinAlphaSceneBuilder.RebuildMainMenuSceneFromConfig,
                "No MainMenu Rotator was found in the active scene. Open the MainMenu scene first.",
                "No MainMenu Rotator was found in the active scene. Open the MainMenu scene first and select Rotator.");
        }

        private static void DrawBottomPageNavSection(HubPreviewLayoutTuningProfile profile)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Bottom Nav", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Select Scene Bottom Nav", GUILayout.Height(28f)))
                    {
                        SelectSceneLayout(
                            FindSceneBottomPageNav,
                            "No BottomPageNav was found in the active HubPreview scene. Open the HubPreview scene first.");
                    }

                    if (GUILayout.Button("Capture Selected + Apply", GUILayout.Height(28f)))
                    {
                        CaptureSelectedSceneLayout(
                            profile,
                            HubPreviewLayoutTuningProfile.BottomPageNavElementId,
                            "Capture Bottom Nav Layout",
                            FindSelectedSceneBottomPageNav,
                            "No BottomPageNav was found in the active HubPreview scene. Open the HubPreview scene first and select BottomPageNav.");
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Rebuild Preview From Config", GUILayout.Height(28f)))
                    {
                        AssetDatabase.SaveAssets();
                        EditorApplication.delayCall += WordSpinAlphaSceneBuilder.RebuildLevelHubPreviewScene;
                    }

                    if (GUILayout.Button("Reset Bottom Nav To Defaults + Apply", GUILayout.Height(28f)))
                    {
                        ResetSceneLayout(
                            profile,
                            HubPreviewLayoutTuningProfile.BottomPageNavElementId,
                            "Reset Bottom Nav To Defaults",
                            FindSceneBottomPageNav,
                            "No BottomPageNav was found in the active HubPreview scene. Open the HubPreview scene first.");
                    }
                }
            }
        }

        private static void DrawOynaButtonSection(HubPreviewLayoutTuningProfile profile)
        {
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Oyna Button", EditorStyles.boldLabel);

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Select Scene Oyna", GUILayout.Height(28f)))
                    {
                        SelectSceneLayout(
                            FindSceneOynaButton,
                            "No OynaBg was found in the active HubPreview scene. Open the HubPreview scene first.");
                    }

                    if (GUILayout.Button("Capture Selected + Apply", GUILayout.Height(28f)))
                    {
                        CaptureSelectedSceneLayout(
                            profile,
                            HubPreviewLayoutTuningProfile.HubPreviewOynaButtonElementId,
                            "Capture Oyna Button Layout",
                            FindSelectedSceneOynaButton,
                            "No OynaBg was found in the active HubPreview scene. Open the HubPreview scene first and select OynaBg.");
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Rebuild Preview From Config", GUILayout.Height(28f)))
                    {
                        AssetDatabase.SaveAssets();
                        EditorApplication.delayCall += WordSpinAlphaSceneBuilder.RebuildLevelHubPreviewScene;
                    }

                    if (GUILayout.Button("Reset Oyna To Defaults + Apply", GUILayout.Height(28f)))
                    {
                        ResetSceneLayout(
                            profile,
                            HubPreviewLayoutTuningProfile.HubPreviewOynaButtonElementId,
                            "Reset Oyna Button Layout",
                            FindSceneOynaButton,
                            "No OynaBg was found in the active HubPreview scene. Open the HubPreview scene first.");
                    }
                }
            }
        }

        private static void DrawLayoutSection(
            HubPreviewLayoutTuningProfile profile,
            string elementId,
            string sectionLabel,
            string editUndoName,
            string captureUndoName,
            string resetUndoName,
            string selectButtonLabel,
            string captureButtonLabel,
            string rebuildButtonLabel,
            string resetButtonLabel,
            Func<RectTransform> sceneFinder,
            Func<RectTransform> selectedFinder,
            Action rebuildAction,
            string selectMissingMessage,
            string captureMissingMessage,
            Action<RectTransform> selectAction = null)
        {
            HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(elementId);

            Vector2 anchoredPosition = tuning.anchoredPosition;
            float width = tuning.width;
            float height = tuning.height;
            bool preserveAspect = tuning.preserveAspect;

            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(sectionLabel, EditorStyles.boldLabel);
                anchoredPosition = EditorGUILayout.Vector2Field("Anchored Position", anchoredPosition);
                width = Mathf.Max(1f, EditorGUILayout.FloatField("Width", width));
                height = Mathf.Max(1f, EditorGUILayout.FloatField("Height", height));
                preserveAspect = EditorGUILayout.Toggle("Preserve Aspect", preserveAspect);
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(profile, editUndoName);
                tuning.anchoredPosition = anchoredPosition;
                tuning.width = width;
                tuning.height = height;
                tuning.preserveAspect = preserveAspect;
                tuning.NormalizeInPlace();
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.Space(8f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(selectButtonLabel, GUILayout.Height(28f)))
                {
                    SelectSceneLayout(sceneFinder, selectMissingMessage, selectAction);
                }

                if (GUILayout.Button(captureButtonLabel, GUILayout.Height(28f)))
                {
                    CaptureSelectedSceneLayout(profile, elementId, captureUndoName, selectedFinder, captureMissingMessage);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(rebuildButtonLabel, GUILayout.Height(28f)))
                {
                    AssetDatabase.SaveAssets();
                    rebuildAction?.Invoke();
                }

                if (GUILayout.Button(resetButtonLabel, GUILayout.Height(28f)))
                {
                    ResetSceneLayout(profile, elementId, resetUndoName, sceneFinder, selectMissingMessage);
                }
            }
        }

        private static void DrawAlttasLayoutSection(
            HubPreviewLayoutTuningProfile profile,
            string elementId,
            string sectionLabel,
            string editUndoName,
            string captureUndoName,
            string baselineUndoName,
            string selectButtonLabel,
            string captureButtonLabel,
            string applyButtonLabel,
            string resetButtonLabel,
            string rebuildButtonLabel,
            Func<RectTransform> sceneFinder,
            Func<RectTransform> selectedFinder,
            Action rebuildAction,
            string selectMissingMessage,
            string captureMissingMessage,
            string applyMissingMessage,
            Action<RectTransform> selectAction = null)
        {
            HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(elementId);

            Vector2 anchoredPosition = tuning.anchoredPosition;
            float width = tuning.width;
            float height = tuning.height;
            bool preserveAspect = tuning.preserveAspect;

            EditorGUI.BeginChangeCheck();
            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField(sectionLabel, EditorStyles.boldLabel);

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Current Live Tuning", EditorStyles.boldLabel);
                    anchoredPosition = EditorGUILayout.Vector2Field("Anchored Position", anchoredPosition);
                    width = Mathf.Max(1f, EditorGUILayout.FloatField("Width", width));
                    height = Mathf.Max(1f, EditorGUILayout.FloatField("Height", height));
                    preserveAspect = EditorGUILayout.Toggle("Preserve Aspect", preserveAspect);
                }

                using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Baseline Reference", EditorStyles.boldLabel);
                    DrawReadOnlyLayoutValueFields(tuning.BaselineValue);
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(profile, editUndoName);
                tuning.anchoredPosition = anchoredPosition;
                tuning.width = width;
                tuning.height = height;
                tuning.preserveAspect = preserveAspect;
                tuning.NormalizeInPlace();
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.Space(8f);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(selectButtonLabel, GUILayout.Height(28f)))
                {
                    SelectSceneLayout(sceneFinder, selectMissingMessage, selectAction);
                }

                if (GUILayout.Button(captureButtonLabel, GUILayout.Height(28f)))
                {
                    CaptureSelectedSceneLayout(profile, elementId, captureUndoName, selectedFinder, captureMissingMessage);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(resetButtonLabel, GUILayout.Height(28f)))
                {
                    ResetAlttasToBaseline(profile, elementId, resetButtonLabel);
                }

                if (GUILayout.Button(applyButtonLabel, GUILayout.Height(28f)))
                {
                    ApplyCurrentSceneLayout(profile, elementId, sceneFinder, applyMissingMessage);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button(baselineUndoName, GUILayout.Height(28f)))
                {
                    SetCurrentAsBaseline(profile, elementId, baselineUndoName);
                }

                if (GUILayout.Button(rebuildButtonLabel, GUILayout.Height(28f)))
                {
                    AssetDatabase.SaveAssets();
                    rebuildAction?.Invoke();
                }
            }
        }

        private static void DrawReadOnlyLayoutValueFields(HubPreviewLayoutTuningProfile.LayoutElementTuningValue value)
        {
            if (value == null)
            {
                return;
            }

            using (new EditorGUI.DisabledScope(true))
            {
                EditorGUILayout.Vector2Field("Anchored Position", value.anchoredPosition);
                EditorGUILayout.FloatField("Width", value.width);
                EditorGUILayout.FloatField("Height", value.height);
                EditorGUILayout.Toggle("Preserve Aspect", value.preserveAspect);
            }
        }

        private static void SelectSceneLayout(
            Func<RectTransform> finder,
            string missingMessage,
            Action<RectTransform> selectAction = null)
        {
            RectTransform target = finder != null ? finder() : null;
            if (target == null)
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", missingMessage, "OK");
                return;
            }

            Selection.activeGameObject = target.gameObject;
            EditorGUIUtility.PingObject(target.gameObject);

            selectAction?.Invoke(target);
        }

        private static void SyncHubPreviewLayoutProfileBeforeSceneSave(Scene scene, string path)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (!IsHubPreviewScene(scene))
            {
                return;
            }

            LevelHubPreviewController controller = FindHubPreviewControllerInScene(scene);
            if (controller == null)
            {
                return;
            }

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile == null)
            {
                return;
            }

            bool changed = false;
            changed |= SyncSceneRectToProfileAndPrefab(profile, HubPreviewLayoutTuningProfile.AlttasElementId, FindDirectChildRect(controller.transform, HubPreviewLayoutTuningProfile.AlttasElementId));
            changed |= SyncSceneRectToProfileAndPrefab(profile, HubPreviewLayoutTuningProfile.BottomPageNavElementId, FindDirectChildRect(controller.transform, BottomPageNavObjectName));
            changed |= SyncSceneRectToProfileAndPrefab(profile, HubPreviewLayoutTuningProfile.HubPreviewOynaButtonElementId, FindDirectChildRect(controller.transform, OynaButtonObjectName));
            changed |= SyncControllerRailFromSceneNodes(controller);
            changed |= SyncControllerRailToPrefabSource(controller);

            if (!changed)
            {
                return;
            }

            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        private static bool SyncControllerRailFromSceneNodes(LevelHubPreviewController controller)
        {
            if (controller == null)
            {
                return false;
            }

            SerializedObject so = new SerializedObject(controller);
            SerializedProperty railPoints = so.FindProperty("railPoints");
            if (railPoints == null || !railPoints.isArray || railPoints.arraySize < 2)
            {
                return false;
            }

            SerializedProperty levelNodes = so.FindProperty("levelNodes");
            bool changed = false;

            for (int i = 0; i < railPoints.arraySize; i++)
            {
                RectTransform node = FindRailNodeRect(controller, levelNodes, i);
                if (node == null)
                {
                    continue;
                }

                SerializedProperty point = railPoints.GetArrayElementAtIndex(i);
                if (point == null)
                {
                    continue;
                }

                changed |= CopyVector2Value(point.FindPropertyRelative("position"), node.anchoredPosition);
            }

            if (!changed)
            {
                return false;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            EditorUtility.SetDirty(controller);
            return true;
        }

        private static RectTransform FindRailNodeRect(LevelHubPreviewController controller, SerializedProperty levelNodes, int index)
        {
            if (levelNodes != null && levelNodes.isArray && index >= 0 && index < levelNodes.arraySize)
            {
                Object reference = levelNodes.GetArrayElementAtIndex(index).objectReferenceValue;
                RectTransform nodeFromArray = reference as RectTransform;
                if (nodeFromArray != null)
                {
                    return nodeFromArray;
                }
            }

            if (controller == null)
            {
                return null;
            }

            Transform pathContainer = controller.transform.Find("PathContainer");
            if (pathContainer == null)
            {
                return null;
            }

            Transform fallbackNode = pathContainer.Find($"LevelNode_{index}");
            return fallbackNode as RectTransform;
        }

        private static bool SyncSceneRectToProfileAndPrefab(HubPreviewLayoutTuningProfile profile, string elementId, RectTransform rect)
        {
            if (profile == null || rect == null)
            {
                return false;
            }

            HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(elementId);
            Vector2 beforePosition = tuning.anchoredPosition;
            float beforeWidth = tuning.width;
            float beforeHeight = tuning.height;
            bool beforePreserveAspect = tuning.preserveAspect;

            tuning.CaptureFrom(rect, rect.GetComponent<Image>());
            bool prefabChanged = ApplyLayoutToPrefabSource(rect, tuning, false, false);
            return prefabChanged || (beforePosition - tuning.anchoredPosition).sqrMagnitude > 0.0001f
                || !Mathf.Approximately(beforeWidth, tuning.width)
                || !Mathf.Approximately(beforeHeight, tuning.height)
                || beforePreserveAspect != tuning.preserveAspect;
        }

        private static bool SyncControllerRailToPrefabSource(LevelHubPreviewController controller)
        {
            if (controller == null)
            {
                return false;
            }

            GameObject outermostPrefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(controller.gameObject);
            string prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(controller.gameObject);
            if (outermostPrefabRoot == null || string.IsNullOrWhiteSpace(prefabAssetPath))
            {
                return false;
            }

            string relativePath = GetRelativeHierarchyPath(controller.transform, outermostPrefabRoot.transform);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabAssetPath);
            if (prefabRoot == null)
            {
                return false;
            }

            try
            {
                RectTransform prefabRect = FindRelativeRectTransform(prefabRoot.transform, relativePath);
                LevelHubPreviewController prefabController = prefabRect != null
                    ? prefabRect.GetComponent<LevelHubPreviewController>()
                    : prefabRoot.GetComponent<LevelHubPreviewController>();
                if (prefabController == null)
                {
                    return false;
                }

                SerializedObject source = new SerializedObject(controller);
                SerializedObject target = new SerializedObject(prefabController);
                bool changed = false;
                changed |= CopyRailPoints(source.FindProperty("railPoints"), target.FindProperty("railPoints"));
                changed |= CopyIntProperty(source.FindProperty("totalLevels"), target.FindProperty("totalLevels"));
                changed |= CopyFloatProperty(source.FindProperty("dragPixelsPerLevel"), target.FindProperty("dragPixelsPerLevel"));
                changed |= CopyFloatProperty(source.FindProperty("snapSharpness"), target.FindProperty("snapSharpness"));

                if (!changed)
                {
                    return false;
                }

                target.ApplyModifiedPropertiesWithoutUndo();
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabAssetPath);
                AssetDatabase.ImportAsset(prefabAssetPath, ImportAssetOptions.ForceSynchronousImport);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static bool CopyRailPoints(SerializedProperty source, SerializedProperty target)
        {
            if (source == null || target == null || !source.isArray || !target.isArray)
            {
                return false;
            }

            bool changed = false;
            if (target.arraySize != source.arraySize)
            {
                target.arraySize = source.arraySize;
                changed = true;
            }

            for (int i = 0; i < source.arraySize; i++)
            {
                SerializedProperty sourcePoint = source.GetArrayElementAtIndex(i);
                SerializedProperty targetPoint = target.GetArrayElementAtIndex(i);
                changed |= CopyVector2Property(sourcePoint.FindPropertyRelative("position"), targetPoint.FindPropertyRelative("position"));
                changed |= CopyFloatProperty(sourcePoint.FindPropertyRelative("scale"), targetPoint.FindPropertyRelative("scale"));
                changed |= CopyFloatProperty(sourcePoint.FindPropertyRelative("rotation"), targetPoint.FindPropertyRelative("rotation"));
                changed |= CopyFloatProperty(sourcePoint.FindPropertyRelative("alpha"), targetPoint.FindPropertyRelative("alpha"));
            }

            return changed;
        }

        private static bool CopyVector2Property(SerializedProperty source, SerializedProperty target)
        {
            if (source == null || target == null)
            {
                return false;
            }

            Vector2 value = source.vector2Value;
            if ((target.vector2Value - value).sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            target.vector2Value = value;
            return true;
        }

        private static bool CopyVector2Value(SerializedProperty target, Vector2 value)
        {
            if (target == null)
            {
                return false;
            }

            if ((target.vector2Value - value).sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            target.vector2Value = value;
            return true;
        }

        private static bool CopyFloatProperty(SerializedProperty source, SerializedProperty target)
        {
            if (source == null || target == null)
            {
                return false;
            }

            float value = source.floatValue;
            if (Mathf.Approximately(target.floatValue, value))
            {
                return false;
            }

            target.floatValue = value;
            return true;
        }

        private static bool CopyIntProperty(SerializedProperty source, SerializedProperty target)
        {
            if (source == null || target == null)
            {
                return false;
            }

            int value = source.intValue;
            if (target.intValue == value)
            {
                return false;
            }

            target.intValue = value;
            return true;
        }

        private static void HandleSceneViewGUI(SceneView sceneView)
        {
            RectTransform alttas = GetSelectedSceneAlttasForResize();
            if (alttas != null)
            {
                DrawAlttasMoveHandle(sceneView, alttas);
                DrawAlttasResizeHandles(sceneView, alttas);
            }

            if (s_AlttasMoveTarget != null && IsMouseUpEvent(Event.current))
            {
                CommitAlttasMove(sceneView);
            }

            if (s_AlttasResizeTarget != null && IsMouseUpEvent(Event.current))
            {
                CommitAlttasResize(sceneView);
            }

            RectTransform bottomPageNav = GetSelectedSceneBottomPageNavForLiveEdit();
            if (bottomPageNav != null)
            {
                DrawBottomPageNavMoveHandle(sceneView, bottomPageNav);
                DrawBottomPageNavResizeHandles(sceneView, bottomPageNav);
            }

            if (s_BottomPageNavResizeTarget != null && IsMouseUpEvent(Event.current))
            {
                CommitBottomPageNavResize(sceneView);
            }

            if (s_BottomPageNavMoveTarget != null && IsMouseUpEvent(Event.current))
            {
                CommitBottomPageNavMove(sceneView);
            }

            RectTransform oynaButton = GetSelectedSceneOynaButtonForMove();
            if (oynaButton != null)
            {
                DrawOynaButtonMoveHandle(sceneView, oynaButton);
                DrawOynaButtonResizeHandles(sceneView, oynaButton);
            }

            if (s_OynaButtonResizeTarget != null && IsMouseUpEvent(Event.current))
            {
                CommitOynaButtonResize(sceneView);
            }

            if (s_OynaButtonMoveTarget != null && IsMouseUpEvent(Event.current))
            {
                CommitOynaButtonMove(sceneView);
            }

            RectTransform mainMenuRotator = GetSelectedSceneMainMenuRotatorForMove();
            if (mainMenuRotator != null)
            {
                DrawMainMenuRotatorMoveHandle(sceneView, mainMenuRotator);
                DrawMainMenuRotatorResizeHandle(sceneView, mainMenuRotator);
            }

            if (s_MainMenuRotatorMoveTarget != null && IsMouseUpEvent(Event.current))
            {
                CommitMainMenuRotatorMove(sceneView);
            }
        }

        private static RectTransform GetSelectedSceneAlttasForResize()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null || selected.name != HubPreviewLayoutTuningProfile.AlttasElementId)
            {
                return null;
            }

            RectTransform rect = selected.GetComponent<RectTransform>();
            if (rect == null)
            {
                return null;
            }

            LevelHubPreviewController controller = rect.GetComponentInParent<LevelHubPreviewController>();
            if (controller == null)
            {
                return null;
            }

            Scene scene = rect.gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return null;
            }

            return rect;
        }

        private static void DrawAlttasResizeHandles(SceneView sceneView, RectTransform alttas)
        {
            if (sceneView == null || alttas == null)
            {
                return;
            }

            Vector3 worldCenter = alttas.TransformPoint(new Vector3(alttas.rect.center.x, alttas.rect.center.y, 0f));
            Vector3 widthAxis = alttas.right.normalized;
            Vector3 heightAxis = alttas.up.normalized;
            float halfWidth = Mathf.Max(0.5f, alttas.rect.width * 0.5f);
            float halfHeight = Mathf.Max(0.5f, alttas.rect.height * 0.5f);

            Vector3 bottomLeft = worldCenter - widthAxis * halfWidth - heightAxis * halfHeight;
            Vector3 bottomRight = worldCenter + widthAxis * halfWidth - heightAxis * halfHeight;
            Vector3 topRight = worldCenter + widthAxis * halfWidth + heightAxis * halfHeight;
            Vector3 topLeft = worldCenter - widthAxis * halfWidth + heightAxis * halfHeight;

            UnityEngine.Rendering.CompareFunction previousZTest = Handles.zTest;
            Color previousColor = Handles.color;
            try
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.DrawSolidRectangleWithOutline(
                    new[] { bottomLeft, bottomRight, topRight, topLeft },
                    new Color(0.17f, 0.78f, 1f, 0.05f),
                    new Color(0.17f, 0.78f, 1f, 0.9f));

                float handleSize = Mathf.Max(0.08f, HandleUtility.GetHandleSize(worldCenter) * 0.08f);
                Vector3 widthHandlePosition = worldCenter + widthAxis * halfWidth;
                Vector3 heightHandlePosition = worldCenter + heightAxis * halfHeight;
                bool widthChanged = false;
                bool heightChanged = false;
                float widthDelta = 0f;
                float heightDelta = 0f;

                Handles.color = new Color(0.26f, 0.84f, 1f, 1f);
                EditorGUI.BeginChangeCheck();
                Vector3 movedWidthHandlePosition = Handles.Slider(widthHandlePosition, widthAxis, handleSize, Handles.SphereHandleCap, 0f);
                if (EditorGUI.EndChangeCheck())
                {
                    widthChanged = true;
                    widthDelta = Vector3.Dot(movedWidthHandlePosition - widthHandlePosition, widthAxis) * 2f;
                }

                Handles.color = new Color(1f, 0.78f, 0.28f, 1f);
                EditorGUI.BeginChangeCheck();
                Vector3 movedHeightHandlePosition = Handles.Slider(heightHandlePosition, heightAxis, handleSize, Handles.SphereHandleCap, 0f);
                if (EditorGUI.EndChangeCheck())
                {
                    heightChanged = true;
                    heightDelta = Vector3.Dot(movedHeightHandlePosition - heightHandlePosition, heightAxis) * 2f;
                }

                if (widthChanged || heightChanged)
                {
                    if (!s_AlttasResizeUndoRecorded)
                    {
                        HubPreviewLayoutTuningProfile profileForUndo = HubPreviewLayoutTuningProfile.Load();
                        Undo.RecordObject(alttas, "Resize Alttas");
                        if (profileForUndo != null)
                        {
                            Undo.RecordObject(profileForUndo, "Resize Alttas");
                        }

                        s_AlttasResizeUndoRecorded = true;
                        s_AlttasResizeTarget = alttas;
                    }

                    Vector2 sizeDelta = alttas.sizeDelta;
                    if (widthChanged)
                    {
                        sizeDelta.x = Mathf.Max(1f, sizeDelta.x + widthDelta);
                    }

                    if (heightChanged)
                    {
                        sizeDelta.y = Mathf.Max(1f, sizeDelta.y + heightDelta);
                    }

                    alttas.sizeDelta = sizeDelta;
                    EditorUtility.SetDirty(alttas);
                    PrefabUtility.RecordPrefabInstancePropertyModifications(alttas);
                    EditorSceneManager.MarkSceneDirty(alttas.gameObject.scene);
                    s_AlttasResizeTarget = alttas;
                    sceneView.Repaint();
                }
            }
            finally
            {
                Handles.color = previousColor;
                Handles.zTest = previousZTest;
            }
        }

        private static void DrawAlttasMoveHandle(SceneView sceneView, RectTransform alttas)
        {
            if (sceneView == null || alttas == null)
            {
                return;
            }

            Vector3 worldPosition = alttas.position;
            float handleSize = Mathf.Max(0.08f, HandleUtility.GetHandleSize(worldPosition) * 0.08f);

            UnityEngine.Rendering.CompareFunction previousZTest = Handles.zTest;
            Color previousColor = Handles.color;
            try
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.color = new Color(0.17f, 0.78f, 1f, 1f);

                EditorGUI.BeginChangeCheck();
                Vector3 movedWorldPosition = Handles.FreeMoveHandle(
                    worldPosition,
                    handleSize,
                    Vector3.zero,
                    Handles.RectangleHandleCap);
                if (!EditorGUI.EndChangeCheck())
                {
                    return;
                }

                if (!s_AlttasMoveUndoRecorded)
                {
                    HubPreviewLayoutTuningProfile profileForUndo = HubPreviewLayoutTuningProfile.Load();
                    Undo.RecordObject(alttas, "Move Alttas");
                    if (profileForUndo != null)
                    {
                        Undo.RecordObject(profileForUndo, "Move Alttas");
                    }

                    s_AlttasMoveUndoRecorded = true;
                    s_AlttasMoveTarget = alttas;
                }

                Vector2 anchoredPosition = WorldPositionToAnchoredPosition(alttas, movedWorldPosition);
                alttas.anchoredPosition = anchoredPosition;

                HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
                if (profile != null)
                {
                    HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.AlttasElementId);
                    tuning.anchoredPosition = anchoredPosition;
                    tuning.NormalizeInPlace(alttas.GetComponent<Image>());
                    EditorUtility.SetDirty(profile);
                }

                PrefabUtility.RecordPrefabInstancePropertyModifications(alttas);
                EditorSceneManager.MarkSceneDirty(alttas.gameObject.scene);
                s_AlttasMoveTarget = alttas;
                sceneView.Repaint();
            }
            finally
            {
                Handles.color = previousColor;
                Handles.zTest = previousZTest;
            }
        }

        private static void DrawMainMenuRotatorMoveHandle(SceneView sceneView, RectTransform rotator)
        {
            if (sceneView == null || rotator == null)
            {
                return;
            }

            Vector3 worldPosition = rotator.position;
            float handleSize = Mathf.Max(0.08f, HandleUtility.GetHandleSize(worldPosition) * 0.08f);

            UnityEngine.Rendering.CompareFunction previousZTest = Handles.zTest;
            Color previousColor = Handles.color;
            try
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.color = new Color(1f, 0.74f, 0.22f, 1f);

                EditorGUI.BeginChangeCheck();
                Vector3 movedWorldPosition = Handles.FreeMoveHandle(
                    worldPosition,
                    handleSize,
                    Vector3.zero,
                    Handles.RectangleHandleCap);
                if (!EditorGUI.EndChangeCheck())
                {
                    return;
                }

                RectTransform parent = rotator.parent as RectTransform;
                if (parent == null)
                {
                    return;
                }

                if (!s_MainMenuRotatorMoveUndoRecorded)
                {
                    HubPreviewLayoutTuningProfile profileForUndo = HubPreviewLayoutTuningProfile.Load();
                    Undo.RecordObject(rotator, "Move MainMenu Rotator");
                    if (profileForUndo != null)
                    {
                        Undo.RecordObject(profileForUndo, "Move MainMenu Rotator");
                    }

                    s_MainMenuRotatorMoveUndoRecorded = true;
                    s_MainMenuRotatorMoveTarget = rotator;
                }

                Vector2 anchoredPosition = WorldPositionToAnchoredPosition(rotator, movedWorldPosition);
                ApplyMainMenuRotatorMove(rotator, anchoredPosition);
                sceneView.Repaint();
            }
            finally
            {
                Handles.color = previousColor;
                Handles.zTest = previousZTest;
            }
        }

        private static void DrawMainMenuRotatorResizeHandle(SceneView sceneView, RectTransform rotator)
        {
            if (sceneView == null || rotator == null)
            {
                return;
            }

            Image image = rotator.GetComponent<Image>();
            Vector3 worldCenter = rotator.TransformPoint(new Vector3(rotator.rect.center.x, rotator.rect.center.y, 0f));
            Vector3 widthAxis = rotator.right.normalized;
            float halfWidth = Mathf.Max(0.5f, rotator.rect.width * 0.5f);
            Vector3 widthHandlePosition = worldCenter + widthAxis * halfWidth;
            float handleSize = Mathf.Max(0.08f, HandleUtility.GetHandleSize(widthHandlePosition) * 0.08f);

            UnityEngine.Rendering.CompareFunction previousZTest = Handles.zTest;
            Color previousColor = Handles.color;
            try
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.color = new Color(0.22f, 0.96f, 0.58f, 1f);

                EditorGUI.BeginChangeCheck();
                Vector3 movedWidthHandlePosition = Handles.Slider(
                    widthHandlePosition,
                    widthAxis,
                    handleSize,
                    Handles.SphereHandleCap,
                    0f);
                if (!EditorGUI.EndChangeCheck())
                {
                    return;
                }

                float widthDelta = Vector3.Dot(movedWidthHandlePosition - widthHandlePosition, widthAxis) * 2f;
                if (Mathf.Approximately(widthDelta, 0f))
                {
                    return;
                }

                if (!s_MainMenuRotatorMoveUndoRecorded)
                {
                    HubPreviewLayoutTuningProfile profileForUndo = HubPreviewLayoutTuningProfile.Load();
                    Undo.RecordObject(rotator, "Resize MainMenu Rotator");
                    if (image != null)
                    {
                        Undo.RecordObject(image, "Resize MainMenu Rotator");
                    }

                    if (profileForUndo != null)
                    {
                        Undo.RecordObject(profileForUndo, "Resize MainMenu Rotator");
                    }

                    s_MainMenuRotatorMoveUndoRecorded = true;
                    s_MainMenuRotatorMoveTarget = rotator;
                }

                ApplyMainMenuRotatorResize(rotator, widthDelta, image);
                sceneView.Repaint();
            }
            finally
            {
                Handles.color = previousColor;
                Handles.zTest = previousZTest;
            }
        }

        private static void ApplyMainMenuRotatorResize(RectTransform rotator, float widthDelta, Image image)
        {
            if (rotator == null)
            {
                return;
            }

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile != null
                ? profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.MainMenuRotatorElementId)
                : HubPreviewLayoutTuningProfile.ResolveMainMenuRotator();

            tuning.width = Mathf.Max(1f, tuning.width + widthDelta);
            tuning.NormalizeInPlace(image);
            tuning.ApplyTo(rotator, image);

            if (profile != null)
            {
                EditorUtility.SetDirty(profile);
            }

            if (image != null)
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(image);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(rotator);
            EditorSceneManager.MarkSceneDirty(rotator.gameObject.scene);
            s_MainMenuRotatorMoveTarget = rotator;
        }

        private static RectTransform GetSelectedSceneMainMenuRotatorForMove()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null || selected.name != MainMenuRotatorObjectName)
            {
                return null;
            }

            RectTransform rect = selected.GetComponent<RectTransform>();
            if (rect == null)
            {
                return null;
            }

            if (!IsMainMenuPreviewContext(rect))
            {
                return null;
            }

            return rect.parent is RectTransform ? rect : null;
        }

        private static void ApplyMainMenuRotatorMove(RectTransform rotator, Vector2 anchoredPosition)
        {
            if (rotator == null)
            {
                return;
            }

            rotator.anchoredPosition = anchoredPosition;

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.MainMenuRotatorElementId);
                tuning.anchoredPosition = anchoredPosition;
                EditorUtility.SetDirty(profile);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(rotator);
            EditorSceneManager.MarkSceneDirty(rotator.gameObject.scene);
            s_MainMenuRotatorMoveTarget = rotator;
        }

        private static void CommitMainMenuRotatorMove(SceneView sceneView)
        {
            RectTransform rotator = s_MainMenuRotatorMoveTarget;
            if (rotator == null)
            {
                ResetMainMenuRotatorMoveState();
                return;
            }

            if (!rotator.gameObject.scene.IsValid() || !rotator.gameObject.scene.isLoaded)
            {
                ResetMainMenuRotatorMoveState();
                return;
            }

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.MainMenuRotatorElementId);
                tuning.anchoredPosition = rotator.anchoredPosition;
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();

                if (!ApplyLayoutToPrefabSource(rotator, tuning))
                {
                    MarkAndSaveScene(rotator.gameObject.scene);
                }
            }
            else
            {
                MarkAndSaveScene(rotator.gameObject.scene);
            }

            if (sceneView != null)
            {
                sceneView.Repaint();
            }

            SceneView.RepaintAll();
            ResetMainMenuRotatorMoveState();
        }

        private static Vector2 WorldPositionToAnchoredPosition(RectTransform target, Vector3 worldPosition)
        {
            if (target == null)
            {
                return Vector2.zero;
            }

            RectTransform parent = target.parent as RectTransform;
            if (parent == null)
            {
                return target.anchoredPosition;
            }

            Vector3 parentLocalPosition3D = parent.InverseTransformPoint(worldPosition);
            Vector2 parentLocalPosition = new Vector2(parentLocalPosition3D.x, parentLocalPosition3D.y);
            Rect parentRect = parent.rect;

            Vector2 anchorMinPoint = new Vector2(
                Mathf.Lerp(parentRect.xMin, parentRect.xMax, target.anchorMin.x),
                Mathf.Lerp(parentRect.yMin, parentRect.yMax, target.anchorMin.y));
            Vector2 anchorMaxPoint = new Vector2(
                Mathf.Lerp(parentRect.xMin, parentRect.xMax, target.anchorMax.x),
                Mathf.Lerp(parentRect.yMin, parentRect.yMax, target.anchorMax.y));
            Vector2 anchorReferencePoint = new Vector2(
                Mathf.Lerp(anchorMinPoint.x, anchorMaxPoint.x, target.pivot.x),
                Mathf.Lerp(anchorMinPoint.y, anchorMaxPoint.y, target.pivot.y));

            return parentLocalPosition - anchorReferencePoint;
        }

        private static bool IsMouseUpEvent(Event currentEvent)
        {
            return currentEvent != null && (currentEvent.type == EventType.MouseUp || currentEvent.rawType == EventType.MouseUp);
        }

        private static void ResetMainMenuRotatorMoveState()
        {
            s_MainMenuRotatorMoveTarget = null;
            s_MainMenuRotatorMoveUndoRecorded = false;
        }

        private static void ResetAlttasMoveState()
        {
            s_AlttasMoveTarget = null;
            s_AlttasMoveUndoRecorded = false;
        }

        private static void ResetAlttasResizeState()
        {
            s_AlttasResizeTarget = null;
            s_AlttasResizeUndoRecorded = false;
        }

        private static void ResetBottomPageNavMoveState()
        {
            s_BottomPageNavMoveTarget = null;
            s_BottomPageNavMoveUndoRecorded = false;
        }

        private static void ResetBottomPageNavResizeState()
        {
            s_BottomPageNavResizeTarget = null;
            s_BottomPageNavResizeUndoRecorded = false;
        }

        private static void ResetOynaButtonMoveState()
        {
            s_OynaButtonMoveTarget = null;
            s_OynaButtonMoveUndoRecorded = false;
        }

        private static void ResetOynaButtonResizeState()
        {
            s_OynaButtonResizeTarget = null;
            s_OynaButtonResizeUndoRecorded = false;
        }

        private static void AlignSceneViewForAlttasEdit(RectTransform target)
        {
            if (target == null)
            {
                return;
            }

            SceneView sceneView = SceneView.lastActiveSceneView;
            if (sceneView == null)
            {
                return;
            }

            GameObject frameObject = GetAlttasSceneViewFrameObject(target);
            bool temporarilySwappedSelection = frameObject != null && frameObject != target.gameObject;

            if (temporarilySwappedSelection)
            {
                Selection.activeGameObject = frameObject;
            }

            sceneView.in2DMode = true;
            sceneView.FrameSelected();
            sceneView.Repaint();

            if (temporarilySwappedSelection)
            {
                Selection.activeGameObject = target.gameObject;
            }
        }

        private static GameObject GetAlttasSceneViewFrameObject(RectTransform target)
        {
            LevelHubPreviewController controller = target != null ? target.GetComponentInParent<LevelHubPreviewController>() : null;
            if (controller != null)
            {
                return controller.gameObject;
            }

            return target != null ? target.gameObject : null;
        }

        private static void CaptureSelectedSceneLayout(
            HubPreviewLayoutTuningProfile profile,
            string elementId,
            string undoName,
            Func<RectTransform> finder,
            string missingMessage)
        {
            if (profile == null)
            {
                return;
            }

            RectTransform rect = finder != null ? finder() : null;
            if (rect == null)
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", missingMessage, "OK");
                return;
            }

            Image image = rect.GetComponent<Image>();
            Undo.RecordObject(profile, undoName);
            HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(elementId);
            tuning.CaptureFrom(rect, image);
            tuning.NormalizeInPlace(image);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            if (!ApplyLayoutToPrefabSource(rect, tuning))
            {
                tuning.ApplyTo(rect, image);
                MarkAndSaveScene(rect.gameObject.scene);
            }

            SceneView.RepaintAll();
        }

        private static void ApplyCurrentSceneLayout(
            HubPreviewLayoutTuningProfile profile,
            string elementId,
            Func<RectTransform> finder,
            string missingMessage)
        {
            if (profile == null)
            {
                return;
            }

            RectTransform rect = finder != null ? finder() : null;
            if (rect == null)
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", missingMessage, "OK");
                return;
            }

            HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(elementId);
            AssetDatabase.SaveAssets();

            Image image = rect.GetComponent<Image>();
            if (!ApplyLayoutToPrefabSource(rect, tuning))
            {
                tuning.ApplyTo(rect, image);
                MarkAndSaveScene(rect.gameObject.scene);
            }

            SceneView.RepaintAll();
        }

        private static void ResetAlttasToBaseline(
            HubPreviewLayoutTuningProfile profile,
            string elementId,
            string undoName)
        {
            if (profile == null)
            {
                return;
            }

            Undo.RecordObject(profile, undoName);
            HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.ResetElementToDefault(elementId);
            tuning.NormalizeInPlace();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        private static void SetCurrentAsBaseline(
            HubPreviewLayoutTuningProfile profile,
            string elementId,
            string undoName)
        {
            if (profile == null)
            {
                return;
            }

            HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(elementId);
            Undo.RecordObject(profile, undoName);
            tuning.CopyCurrentToBaseline();
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();
        }

        private static void ResetSceneLayout(
            HubPreviewLayoutTuningProfile profile,
            string elementId,
            string undoName,
            Func<RectTransform> finder,
            string missingMessage)
        {
            if (profile == null)
            {
                return;
            }

            RectTransform rect = finder != null ? finder() : null;
            if (rect == null)
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", missingMessage, "OK");
                return;
            }

            Image image = rect.GetComponent<Image>();
            Undo.RecordObject(profile, undoName);
            HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.ResetElementToDefault(elementId);
            tuning.NormalizeInPlace(image);
            EditorUtility.SetDirty(profile);
            AssetDatabase.SaveAssets();

            if (!ApplyLayoutToPrefabSource(rect, tuning))
            {
                tuning.ApplyTo(rect, image);
                MarkAndSaveScene(rect.gameObject.scene);
            }

            SceneView.RepaintAll();
        }

        private static bool ApplyLayoutToPrefabSource(
            RectTransform sceneRect,
            HubPreviewLayoutTuningProfile.LayoutElementTuning tuning,
            bool saveScene = true,
            bool markSceneDirty = true)
        {
            if (sceneRect == null || tuning == null)
            {
                return false;
            }

            GameObject outermostPrefabRoot = PrefabUtility.GetOutermostPrefabInstanceRoot(sceneRect.gameObject);
            string prefabAssetPath = PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(sceneRect.gameObject);
            if (outermostPrefabRoot == null || string.IsNullOrWhiteSpace(prefabAssetPath))
            {
                return false;
            }

            string relativePath = GetRelativeHierarchyPath(sceneRect.transform, outermostPrefabRoot.transform);
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabAssetPath);
            if (prefabRoot == null)
            {
                return false;
            }

            try
            {
                RectTransform prefabRect = FindRelativeRectTransform(prefabRoot.transform, relativePath);
                if (prefabRect == null)
                {
                    return false;
                }

                Image prefabImage = prefabRect.GetComponent<Image>();
                tuning.ApplyTo(prefabRect, prefabImage);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabAssetPath);
                AssetDatabase.ImportAsset(prefabAssetPath, ImportAssetOptions.ForceSynchronousImport);

                Image sceneImage = sceneRect.GetComponent<Image>();
                tuning.ApplyTo(sceneRect, sceneImage);
                PrefabUtility.RecordPrefabInstancePropertyModifications(sceneRect);
                if (sceneImage != null)
                {
                    PrefabUtility.RecordPrefabInstancePropertyModifications(sceneImage);
                }

                if (markSceneDirty)
                {
                    EditorSceneManager.MarkSceneDirty(sceneRect.gameObject.scene);
                }

                if (saveScene)
                {
                    SaveSceneIfPossible(sceneRect.gameObject.scene);
                }

                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static void DrawBottomPageNavMoveHandle(SceneView sceneView, RectTransform bottomPageNav)
        {
            if (sceneView == null || bottomPageNav == null)
            {
                return;
            }

            Vector3 worldPosition = bottomPageNav.position;
            float handleSize = Mathf.Max(0.08f, HandleUtility.GetHandleSize(worldPosition) * 0.08f);

            UnityEngine.Rendering.CompareFunction previousZTest = Handles.zTest;
            Color previousColor = Handles.color;
            try
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.color = new Color(0.2f, 0.86f, 1f, 1f);

                EditorGUI.BeginChangeCheck();
                Vector3 movedWorldPosition = Handles.FreeMoveHandle(
                    worldPosition,
                    handleSize,
                    Vector3.zero,
                    Handles.RectangleHandleCap);
                if (!EditorGUI.EndChangeCheck())
                {
                    return;
                }

                if (!s_BottomPageNavMoveUndoRecorded)
                {
                    HubPreviewLayoutTuningProfile profileForUndo = HubPreviewLayoutTuningProfile.Load();
                    Undo.RecordObject(bottomPageNav, "Move Bottom Page Nav");
                    if (profileForUndo != null)
                    {
                        Undo.RecordObject(profileForUndo, "Move Bottom Page Nav");
                    }

                    s_BottomPageNavMoveUndoRecorded = true;
                    s_BottomPageNavMoveTarget = bottomPageNav;
                }

                Vector2 anchoredPosition = WorldPositionToAnchoredPosition(bottomPageNav, movedWorldPosition);
                ApplyBottomPageNavMove(bottomPageNav, anchoredPosition);
                sceneView.Repaint();
            }
            finally
            {
                Handles.color = previousColor;
                Handles.zTest = previousZTest;
            }
        }

        private static void DrawBottomPageNavResizeHandles(SceneView sceneView, RectTransform bottomPageNav)
        {
            if (sceneView == null || bottomPageNav == null)
            {
                return;
            }

            Vector3 worldCenter = bottomPageNav.TransformPoint(new Vector3(bottomPageNav.rect.center.x, bottomPageNav.rect.center.y, 0f));
            Vector3 widthAxis = bottomPageNav.right.normalized;
            Vector3 heightAxis = bottomPageNav.up.normalized;
            float halfWidth = Mathf.Max(0.5f, bottomPageNav.rect.width * 0.5f);
            float halfHeight = Mathf.Max(0.5f, bottomPageNav.rect.height * 0.5f);

            Vector3 bottomLeft = worldCenter - widthAxis * halfWidth - heightAxis * halfHeight;
            Vector3 bottomRight = worldCenter + widthAxis * halfWidth - heightAxis * halfHeight;
            Vector3 topRight = worldCenter + widthAxis * halfWidth + heightAxis * halfHeight;
            Vector3 topLeft = worldCenter - widthAxis * halfWidth + heightAxis * halfHeight;

            UnityEngine.Rendering.CompareFunction previousZTest = Handles.zTest;
            Color previousColor = Handles.color;
            try
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.DrawSolidRectangleWithOutline(
                    new[] { bottomLeft, bottomRight, topRight, topLeft },
                    new Color(0.2f, 0.86f, 1f, 0.04f),
                    new Color(0.2f, 0.86f, 1f, 0.88f));

                float handleSize = Mathf.Max(0.08f, HandleUtility.GetHandleSize(worldCenter) * 0.08f);
                Vector3 widthHandlePosition = worldCenter + widthAxis * halfWidth;
                Vector3 heightHandlePosition = worldCenter + heightAxis * halfHeight;
                bool widthChanged = false;
                bool heightChanged = false;
                float widthDelta = 0f;
                float heightDelta = 0f;

                Handles.color = new Color(0.14f, 0.82f, 1f, 1f);
                EditorGUI.BeginChangeCheck();
                Vector3 movedWidthHandlePosition = Handles.Slider(widthHandlePosition, widthAxis, handleSize, Handles.SphereHandleCap, 0f);
                if (EditorGUI.EndChangeCheck())
                {
                    widthChanged = true;
                    widthDelta = Vector3.Dot(movedWidthHandlePosition - widthHandlePosition, widthAxis) * 2f;
                }

                Handles.color = new Color(1f, 0.72f, 0.25f, 1f);
                EditorGUI.BeginChangeCheck();
                Vector3 movedHeightHandlePosition = Handles.Slider(heightHandlePosition, heightAxis, handleSize, Handles.SphereHandleCap, 0f);
                if (EditorGUI.EndChangeCheck())
                {
                    heightChanged = true;
                    heightDelta = Vector3.Dot(movedHeightHandlePosition - heightHandlePosition, heightAxis) * 2f;
                }

                if (!widthChanged && !heightChanged)
                {
                    return;
                }

                if (!s_BottomPageNavResizeUndoRecorded)
                {
                    HubPreviewLayoutTuningProfile profileForUndo = HubPreviewLayoutTuningProfile.Load();
                    Undo.RecordObject(bottomPageNav, "Resize Bottom Page Nav");
                    if (profileForUndo != null)
                    {
                        Undo.RecordObject(profileForUndo, "Resize Bottom Page Nav");
                    }

                    s_BottomPageNavResizeUndoRecorded = true;
                    s_BottomPageNavResizeTarget = bottomPageNav;
                }

                ApplyBottomPageNavResize(bottomPageNav, widthDelta, heightDelta);
                sceneView.Repaint();
            }
            finally
            {
                Handles.color = previousColor;
                Handles.zTest = previousZTest;
            }
        }

        private static void ApplyBottomPageNavMove(RectTransform bottomPageNav, Vector2 anchoredPosition)
        {
            if (bottomPageNav == null)
            {
                return;
            }

            bottomPageNav.anchoredPosition = anchoredPosition;

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.BottomPageNavElementId);
                tuning.anchoredPosition = anchoredPosition;
                EditorUtility.SetDirty(profile);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(bottomPageNav);
            EditorSceneManager.MarkSceneDirty(bottomPageNav.gameObject.scene);
            s_BottomPageNavMoveTarget = bottomPageNav;
        }

        private static void ApplyBottomPageNavResize(RectTransform bottomPageNav, float widthDelta, float heightDelta)
        {
            if (bottomPageNav == null)
            {
                return;
            }

            Vector2 sizeDelta = bottomPageNav.sizeDelta;
            bool sizeChanged = false;

            if (!Mathf.Approximately(widthDelta, 0f))
            {
                sizeDelta.x = Mathf.Max(1f, sizeDelta.x + widthDelta);
                sizeChanged = true;
            }

            if (!Mathf.Approximately(heightDelta, 0f))
            {
                sizeDelta.y = Mathf.Max(1f, sizeDelta.y + heightDelta);
                sizeChanged = true;
            }

            if (!sizeChanged)
            {
                return;
            }

            bottomPageNav.sizeDelta = sizeDelta;

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.BottomPageNavElementId);
                tuning.width = sizeDelta.x;
                tuning.height = sizeDelta.y;
                tuning.NormalizeInPlace();
                EditorUtility.SetDirty(profile);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(bottomPageNav);
            EditorSceneManager.MarkSceneDirty(bottomPageNav.gameObject.scene);
            s_BottomPageNavResizeTarget = bottomPageNav;
        }

        private static void DrawOynaButtonMoveHandle(SceneView sceneView, RectTransform oynaButton)
        {
            if (sceneView == null || oynaButton == null)
            {
                return;
            }

            Vector3 worldPosition = oynaButton.position;
            float handleSize = Mathf.Max(0.08f, HandleUtility.GetHandleSize(worldPosition) * 0.08f);

            UnityEngine.Rendering.CompareFunction previousZTest = Handles.zTest;
            Color previousColor = Handles.color;
            try
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.color = new Color(0.95f, 0.58f, 0.22f, 1f);

                EditorGUI.BeginChangeCheck();
                Vector3 movedWorldPosition = Handles.FreeMoveHandle(
                    worldPosition,
                    handleSize,
                    Vector3.zero,
                    Handles.RectangleHandleCap);
                if (!EditorGUI.EndChangeCheck())
                {
                    return;
                }

                if (!s_OynaButtonMoveUndoRecorded)
                {
                    HubPreviewLayoutTuningProfile profileForUndo = HubPreviewLayoutTuningProfile.Load();
                    Undo.RecordObject(oynaButton, "Move Oyna Button");
                    if (profileForUndo != null)
                    {
                        Undo.RecordObject(profileForUndo, "Move Oyna Button");
                    }

                    s_OynaButtonMoveUndoRecorded = true;
                    s_OynaButtonMoveTarget = oynaButton;
                }

                Vector2 anchoredPosition = WorldPositionToAnchoredPosition(oynaButton, movedWorldPosition);
                ApplyOynaButtonMove(oynaButton, anchoredPosition);
                sceneView.Repaint();
            }
            finally
            {
                Handles.color = previousColor;
                Handles.zTest = previousZTest;
            }
        }

        private static void DrawOynaButtonResizeHandles(SceneView sceneView, RectTransform oynaButton)
        {
            if (sceneView == null || oynaButton == null)
            {
                return;
            }

            Vector3 worldCenter = oynaButton.TransformPoint(new Vector3(oynaButton.rect.center.x, oynaButton.rect.center.y, 0f));
            Vector3 widthAxis = oynaButton.right.normalized;
            Vector3 heightAxis = oynaButton.up.normalized;
            float halfWidth = Mathf.Max(0.5f, oynaButton.rect.width * 0.5f);
            float halfHeight = Mathf.Max(0.5f, oynaButton.rect.height * 0.5f);

            Vector3 bottomLeft = worldCenter - widthAxis * halfWidth - heightAxis * halfHeight;
            Vector3 bottomRight = worldCenter + widthAxis * halfWidth - heightAxis * halfHeight;
            Vector3 topRight = worldCenter + widthAxis * halfWidth + heightAxis * halfHeight;
            Vector3 topLeft = worldCenter - widthAxis * halfWidth + heightAxis * halfHeight;

            UnityEngine.Rendering.CompareFunction previousZTest = Handles.zTest;
            Color previousColor = Handles.color;
            try
            {
                Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.DrawSolidRectangleWithOutline(
                    new[] { bottomLeft, bottomRight, topRight, topLeft },
                    new Color(0.2f, 0.9f, 0.5f, 0.04f),
                    new Color(0.2f, 0.9f, 0.5f, 0.88f));

                float handleSize = Mathf.Max(0.08f, HandleUtility.GetHandleSize(worldCenter) * 0.08f);
                Vector3 widthHandlePosition = worldCenter + widthAxis * halfWidth;
                Vector3 heightHandlePosition = worldCenter + heightAxis * halfHeight;
                bool widthChanged = false;
                bool heightChanged = false;
                float widthDelta = 0f;
                float heightDelta = 0f;

                Handles.color = new Color(0.12f, 0.9f, 0.46f, 1f);
                EditorGUI.BeginChangeCheck();
                Vector3 movedWidthHandlePosition = Handles.Slider(widthHandlePosition, widthAxis, handleSize, Handles.SphereHandleCap, 0f);
                if (EditorGUI.EndChangeCheck())
                {
                    widthChanged = true;
                    widthDelta = Vector3.Dot(movedWidthHandlePosition - widthHandlePosition, widthAxis) * 2f;
                }

                Handles.color = new Color(1f, 0.72f, 0.25f, 1f);
                EditorGUI.BeginChangeCheck();
                Vector3 movedHeightHandlePosition = Handles.Slider(heightHandlePosition, heightAxis, handleSize, Handles.SphereHandleCap, 0f);
                if (EditorGUI.EndChangeCheck())
                {
                    heightChanged = true;
                    heightDelta = Vector3.Dot(movedHeightHandlePosition - heightHandlePosition, heightAxis) * 2f;
                }

                if (!widthChanged && !heightChanged)
                {
                    return;
                }

                if (!s_OynaButtonResizeUndoRecorded)
                {
                    HubPreviewLayoutTuningProfile profileForUndo = HubPreviewLayoutTuningProfile.Load();
                    Undo.RecordObject(oynaButton, "Resize Oyna Button");
                    if (profileForUndo != null)
                    {
                        Undo.RecordObject(profileForUndo, "Resize Oyna Button");
                    }

                    s_OynaButtonResizeUndoRecorded = true;
                    s_OynaButtonResizeTarget = oynaButton;
                }

                ApplyOynaButtonResize(oynaButton, widthDelta, heightDelta);
                sceneView.Repaint();
            }
            finally
            {
                Handles.color = previousColor;
                Handles.zTest = previousZTest;
            }
        }

        private static void ApplyOynaButtonMove(RectTransform oynaButton, Vector2 anchoredPosition)
        {
            if (oynaButton == null)
            {
                return;
            }

            oynaButton.anchoredPosition = anchoredPosition;

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.HubPreviewOynaButtonElementId);
                tuning.anchoredPosition = anchoredPosition;
                EditorUtility.SetDirty(profile);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(oynaButton);
            EditorSceneManager.MarkSceneDirty(oynaButton.gameObject.scene);
            s_OynaButtonMoveTarget = oynaButton;
        }

        private static void ApplyOynaButtonResize(RectTransform oynaButton, float widthDelta, float heightDelta)
        {
            if (oynaButton == null)
            {
                return;
            }

            Vector2 sizeDelta = oynaButton.sizeDelta;
            bool sizeChanged = false;

            if (!Mathf.Approximately(widthDelta, 0f))
            {
                sizeDelta.x = Mathf.Max(1f, sizeDelta.x + widthDelta);
                sizeChanged = true;
            }

            if (!Mathf.Approximately(heightDelta, 0f))
            {
                sizeDelta.y = Mathf.Max(1f, sizeDelta.y + heightDelta);
                sizeChanged = true;
            }

            if (!sizeChanged)
            {
                return;
            }

            oynaButton.sizeDelta = sizeDelta;

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.HubPreviewOynaButtonElementId);
                tuning.width = sizeDelta.x;
                tuning.height = sizeDelta.y;
                tuning.NormalizeInPlace();
                EditorUtility.SetDirty(profile);
            }

            PrefabUtility.RecordPrefabInstancePropertyModifications(oynaButton);
            EditorSceneManager.MarkSceneDirty(oynaButton.gameObject.scene);
            s_OynaButtonResizeTarget = oynaButton;
        }

        private static void CommitOynaButtonMove(SceneView sceneView)
        {
            RectTransform oynaButton = s_OynaButtonMoveTarget;
            if (oynaButton == null)
            {
                ResetOynaButtonMoveState();
                return;
            }

            if (!oynaButton.gameObject.scene.IsValid() || !oynaButton.gameObject.scene.isLoaded)
            {
                ResetOynaButtonMoveState();
                return;
            }

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.HubPreviewOynaButtonElementId);
                tuning.anchoredPosition = oynaButton.anchoredPosition;
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();

                PrefabUtility.RecordPrefabInstancePropertyModifications(oynaButton);
                if (!ApplyLayoutToPrefabSource(oynaButton, tuning))
                {
                    MarkAndSaveScene(oynaButton.gameObject.scene);
                }
            }
            else
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(oynaButton);
                MarkAndSaveScene(oynaButton.gameObject.scene);
            }

            if (sceneView != null)
            {
                sceneView.Repaint();
            }

            SceneView.RepaintAll();
            ResetOynaButtonMoveState();
        }

        private static void CommitOynaButtonResize(SceneView sceneView)
        {
            RectTransform oynaButton = s_OynaButtonResizeTarget;
            if (oynaButton == null)
            {
                ResetOynaButtonResizeState();
                return;
            }

            if (!oynaButton.gameObject.scene.IsValid() || !oynaButton.gameObject.scene.isLoaded)
            {
                ResetOynaButtonResizeState();
                return;
            }

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.HubPreviewOynaButtonElementId);
                tuning.width = Mathf.Max(1f, oynaButton.sizeDelta.x);
                tuning.height = Mathf.Max(1f, oynaButton.sizeDelta.y);
                tuning.NormalizeInPlace();
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();

                PrefabUtility.RecordPrefabInstancePropertyModifications(oynaButton);
                if (!ApplyLayoutToPrefabSource(oynaButton, tuning))
                {
                    MarkAndSaveScene(oynaButton.gameObject.scene);
                }
            }
            else
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(oynaButton);
                MarkAndSaveScene(oynaButton.gameObject.scene);
            }

            if (sceneView != null)
            {
                sceneView.Repaint();
            }

            SceneView.RepaintAll();
            ResetOynaButtonResizeState();
        }

        private static void CommitAlttasMove(SceneView sceneView)
        {
            RectTransform alttas = s_AlttasMoveTarget;
            if (alttas == null)
            {
                ResetAlttasMoveState();
                return;
            }

            if (!alttas.gameObject.scene.IsValid() || !alttas.gameObject.scene.isLoaded)
            {
                ResetAlttasMoveState();
                return;
            }

            Image image = alttas.GetComponent<Image>();
            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.AlttasElementId);
                tuning.CaptureFrom(alttas, image);
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();

                PrefabUtility.RecordPrefabInstancePropertyModifications(alttas);
                if (!ApplyLayoutToPrefabSource(alttas, tuning))
                {
                    MarkAndSaveScene(alttas.gameObject.scene);
                }
            }
            else
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(alttas);
                MarkAndSaveScene(alttas.gameObject.scene);
            }

            if (sceneView != null)
            {
                sceneView.Repaint();
            }

            SceneView.RepaintAll();
            ResetAlttasMoveState();
        }

        private static void CommitAlttasResize(SceneView sceneView)
        {
            RectTransform alttas = s_AlttasResizeTarget;
            if (alttas == null)
            {
                ResetAlttasResizeState();
                return;
            }

            if (!alttas.gameObject.scene.IsValid() || !alttas.gameObject.scene.isLoaded)
            {
                ResetAlttasResizeState();
                return;
            }

            Image image = alttas.GetComponent<Image>();
            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.AlttasElementId);
                tuning.CaptureFrom(alttas, image);
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();

                PrefabUtility.RecordPrefabInstancePropertyModifications(alttas);
                if (!ApplyLayoutToPrefabSource(alttas, tuning))
                {
                    MarkAndSaveScene(alttas.gameObject.scene);
                }
            }
            else
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(alttas);
                MarkAndSaveScene(alttas.gameObject.scene);
            }

            if (sceneView != null)
            {
                sceneView.Repaint();
            }

            SceneView.RepaintAll();
            ResetAlttasResizeState();
        }

        private static void CommitBottomPageNavMove(SceneView sceneView)
        {
            RectTransform bottomPageNav = s_BottomPageNavMoveTarget;
            if (bottomPageNav == null)
            {
                ResetBottomPageNavMoveState();
                return;
            }

            if (!bottomPageNav.gameObject.scene.IsValid() || !bottomPageNav.gameObject.scene.isLoaded)
            {
                ResetBottomPageNavMoveState();
                return;
            }

            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.BottomPageNavElementId);
                tuning.anchoredPosition = bottomPageNav.anchoredPosition;
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();

                PrefabUtility.RecordPrefabInstancePropertyModifications(bottomPageNav);
                if (!ApplyLayoutToPrefabSource(bottomPageNav, tuning))
                {
                    MarkAndSaveScene(bottomPageNav.gameObject.scene);
                }
            }
            else
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(bottomPageNav);
                MarkAndSaveScene(bottomPageNav.gameObject.scene);
            }

            if (sceneView != null)
            {
                sceneView.Repaint();
            }

            SceneView.RepaintAll();
            ResetBottomPageNavMoveState();
        }

        private static void CommitBottomPageNavResize(SceneView sceneView)
        {
            RectTransform bottomPageNav = s_BottomPageNavResizeTarget;
            if (bottomPageNav == null)
            {
                ResetBottomPageNavResizeState();
                return;
            }

            if (!bottomPageNav.gameObject.scene.IsValid() || !bottomPageNav.gameObject.scene.isLoaded)
            {
                ResetBottomPageNavResizeState();
                return;
            }

            Image image = bottomPageNav.GetComponent<Image>();
            HubPreviewLayoutTuningProfile profile = HubPreviewLayoutTuningProfile.Load();
            if (profile != null)
            {
                HubPreviewLayoutTuningProfile.LayoutElementTuning tuning = profile.GetOrCreateElement(HubPreviewLayoutTuningProfile.BottomPageNavElementId);
                tuning.CaptureFrom(bottomPageNav, image);
                EditorUtility.SetDirty(profile);
                AssetDatabase.SaveAssets();

                PrefabUtility.RecordPrefabInstancePropertyModifications(bottomPageNav);
                if (!ApplyLayoutToPrefabSource(bottomPageNav, tuning))
                {
                    MarkAndSaveScene(bottomPageNav.gameObject.scene);
                }
            }
            else
            {
                PrefabUtility.RecordPrefabInstancePropertyModifications(bottomPageNav);
                MarkAndSaveScene(bottomPageNav.gameObject.scene);
            }

            if (sceneView != null)
            {
                sceneView.Repaint();
            }

            SceneView.RepaintAll();
            ResetBottomPageNavResizeState();
        }

        private static string GetRelativeHierarchyPath(Transform transform, Transform root)
        {
            if (transform == null)
            {
                return string.Empty;
            }

            List<string> pathSegments = new List<string>();
            for (Transform current = transform; current != null && current != root; current = current.parent)
            {
                pathSegments.Insert(0, current.name);
            }

            return string.Join("/", pathSegments);
        }

        private static RectTransform FindRelativeRectTransform(Transform root, string relativePath)
        {
            if (root == null)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(relativePath))
            {
                return root as RectTransform;
            }

            Transform current = root;
            string[] pathSegments = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < pathSegments.Length; i++)
            {
                if (current == null)
                {
                    return null;
                }

                current = current.Find(pathSegments[i]);
            }

            return current as RectTransform;
        }

        private static RectTransform FindSelectedSceneAlttas()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                if (selected.name == HubPreviewLayoutTuningProfile.AlttasElementId)
                {
                    RectTransform selectedAlttas = selected.GetComponent<RectTransform>();
                    if (selectedAlttas != null)
                    {
                        return selectedAlttas;
                    }
                }

                Transform directChild = selected.transform.Find(HubPreviewLayoutTuningProfile.AlttasElementId);
                if (directChild is RectTransform childRect)
                {
                    return childRect;
                }
            }

            return FindSceneAlttas();
        }

        private static RectTransform FindDirectChildRect(Transform parent, string childName)
        {
            if (parent == null || string.IsNullOrWhiteSpace(childName))
            {
                return null;
            }

            Transform child = parent.Find(childName);
            return child as RectTransform;
        }

        private static LevelHubPreviewController FindHubPreviewControllerInScene(Scene scene)
        {
            if (!IsHubPreviewScene(scene))
            {
                return null;
            }

            LevelHubPreviewController[] controllers = Object.FindObjectsByType<LevelHubPreviewController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                LevelHubPreviewController controller = controllers[i];
                if (controller == null || controller.gameObject.scene != scene)
                {
                    continue;
                }

                return controller;
            }

            return null;
        }

        private static bool IsHubPreviewScene(Scene scene)
        {
            return scene.IsValid() && scene.isLoaded && scene.name == GameConstants.SceneHubPreview;
        }

        private static RectTransform FindSceneAlttas()
        {
            LevelHubPreviewController[] controllers = Object.FindObjectsByType<LevelHubPreviewController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                LevelHubPreviewController controller = controllers[i];
                if (controller == null)
                {
                    continue;
                }

                if (!controller.gameObject.scene.IsValid() || !controller.gameObject.scene.isLoaded)
                {
                    continue;
                }

                RectTransform alttasRect = controller.transform.Find("Alttas") as RectTransform;
                if (alttasRect != null)
                {
                    return alttasRect;
                }
            }

            return null;
        }

        private static RectTransform FindSelectedSceneBottomPageNav()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                if (selected.name == BottomPageNavObjectName)
                {
                    RectTransform selectedBottomPageNav = selected.GetComponent<RectTransform>();
                    if (selectedBottomPageNav != null && IsHubPreviewContext(selectedBottomPageNav))
                    {
                        return selectedBottomPageNav;
                    }
                }

                Transform directChild = selected.transform.Find(BottomPageNavObjectName);
                if (directChild is RectTransform childRect && IsHubPreviewContext(childRect))
                {
                    return childRect;
                }
            }

            return FindSceneBottomPageNav();
        }

        private static RectTransform FindSceneBottomPageNav()
        {
            LevelHubPreviewController[] controllers = Object.FindObjectsByType<LevelHubPreviewController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                LevelHubPreviewController controller = controllers[i];
                if (controller == null)
                {
                    continue;
                }

                Scene scene = controller.gameObject.scene;
                if (!scene.IsValid() || !scene.isLoaded || scene.name != GameConstants.SceneHubPreview)
                {
                    continue;
                }

                RectTransform bottomPageNavRect = controller.transform.Find(BottomPageNavObjectName) as RectTransform;
                if (bottomPageNavRect != null && IsHubPreviewContext(bottomPageNavRect))
                {
                    return bottomPageNavRect;
                }
            }

            return null;
        }

        private static RectTransform FindSelectedSceneOynaButton()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                if (selected.name == OynaButtonObjectName)
                {
                    RectTransform selectedOynaButton = selected.GetComponent<RectTransform>();
                    if (selectedOynaButton != null && IsHubPreviewContext(selectedOynaButton))
                    {
                        return selectedOynaButton;
                    }
                }

                Transform directChild = selected.transform.Find(OynaButtonObjectName);
                if (directChild is RectTransform childRect && IsHubPreviewContext(childRect))
                {
                    return childRect;
                }
            }

            return FindSceneOynaButton();
        }

        private static RectTransform FindSceneOynaButton()
        {
            LevelHubPreviewController[] controllers = Object.FindObjectsByType<LevelHubPreviewController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                LevelHubPreviewController controller = controllers[i];
                if (controller == null)
                {
                    continue;
                }

                Scene scene = controller.gameObject.scene;
                if (!scene.IsValid() || !scene.isLoaded || scene.name != GameConstants.SceneHubPreview)
                {
                    continue;
                }

                RectTransform oynaButtonRect = controller.transform.Find(OynaButtonObjectName) as RectTransform;
                if (oynaButtonRect != null && IsHubPreviewContext(oynaButtonRect))
                {
                    return oynaButtonRect;
                }
            }

            return null;
        }

        private static RectTransform GetSelectedSceneBottomPageNavForLiveEdit()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null)
            {
                return null;
            }

            for (Transform current = selected.transform; current != null; current = current.parent)
            {
                if (current.name == BottomPageNavObjectName && current is RectTransform rect && IsHubPreviewContext(rect))
                {
                    return rect;
                }
            }

            return null;
        }

        private static RectTransform GetSelectedSceneOynaButtonForMove()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null || selected.name != OynaButtonObjectName)
            {
                return null;
            }

            RectTransform rect = selected.GetComponent<RectTransform>();
            if (rect == null)
            {
                return null;
            }

            if (!IsHubPreviewContext(rect))
            {
                return null;
            }

            return rect;
        }

        private static bool IsHubPreviewContext(RectTransform rect)
        {
            if (rect == null)
            {
                return false;
            }

            Scene scene = rect.gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded || scene.name != GameConstants.SceneHubPreview)
            {
                return false;
            }

            for (Transform current = rect.transform; current != null; current = current.parent)
            {
                if (current.name == HubPreviewRootName)
                {
                    return true;
                }
            }

            return false;
        }

        private static RectTransform FindSelectedSceneMainMenuRotator()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected != null)
            {
                if (selected.name == MainMenuRotatorObjectName)
                {
                    RectTransform selectedRotator = selected.GetComponent<RectTransform>();
                    if (selectedRotator != null && IsMainMenuPreviewContext(selectedRotator))
                    {
                        return selectedRotator;
                    }
                }

                Transform directChild = selected.transform.Find(MainMenuRotatorObjectName);
                if (directChild is RectTransform childRect && IsMainMenuPreviewContext(childRect))
                {
                    return childRect;
                }
            }

            return FindSceneMainMenuRotator();
        }

        private static RectTransform FindSceneMainMenuRotator()
        {
            RectTransform[] rectTransforms = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < rectTransforms.Length; i++)
            {
                RectTransform rect = rectTransforms[i];
                if (rect == null || rect.name != MainMenuRotatorObjectName)
                {
                    continue;
                }

                if (IsMainMenuPreviewContext(rect))
                {
                    return rect;
                }
            }

            return null;
        }

        private static bool IsMainMenuPreviewContext(RectTransform rect)
        {
            if (rect == null)
            {
                return false;
            }

            Scene scene = rect.gameObject.scene;
            if (!scene.IsValid() || !scene.isLoaded || scene.name != GameConstants.SceneMainMenu)
            {
                return false;
            }

            for (Transform current = rect.transform; current != null; current = current.parent)
            {
                if (current.name == MainMenuPreviewRootName)
                {
                    return true;
                }
            }

            return false;
        }

        private static void MarkAndSaveScene(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return;
            }

            EditorSceneManager.MarkSceneDirty(scene);
            SaveSceneIfPossible(scene);
        }

        private static void SaveSceneIfPossible(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded || string.IsNullOrWhiteSpace(scene.path))
            {
                return;
            }

            EditorSceneManager.SaveScene(scene);
        }
    }
}
