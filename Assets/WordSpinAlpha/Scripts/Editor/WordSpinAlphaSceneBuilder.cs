using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using WordSpinAlpha.Core;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    public static class WordSpinAlphaSceneBuilder
    {
        private const string Scenes = "Assets/WordSpinAlpha/Scenes";
        private const string Prefabs = "Assets/WordSpinAlpha/Generated/Prefabs";
        private const string RotatorShapePrefabs = "Assets/WordSpinAlpha/Resources/RotatorShapes";
        private const string MainMenuSourceArt = "Assets/WordSpinAlpha/Art/UI/MainMenu/Source";
        private const string MainMenuCroppedArt = "Assets/WordSpinAlpha/Art/UI/MainMenu/Cropped";
        private const string LevelHubPreviewArt = "Assets/WordSpinAlpha/Art/UI/LevelHubPreview/Source";
        private const string LevelHubPreviewOynaButtonSpritePath = LevelHubPreviewArt + "/levelsecimhub_oyna_button.png";
        private const string LevelHubNodeSpritePath = LevelHubPreviewArt + "/levelsecimkutu.png";
        private const string LevelHubPreviewBottomNavSpritePath = LevelHubPreviewArt + "/levelsecimhub_sayfanavigasyonbar_boscerceve(1).png";
        private const string LevelHubPreviewPrefabPath = Prefabs + "/LevelHubPreview.prefab";
        private const string LevelHubPreviewScenePath = Scenes + "/" + GameConstants.SceneHubPreview + ".unity";
        private const string PendingHubPreviewRebuildSessionKey = "WordSpinAlpha.PendingHubPreviewRebuild";
        private const string PendingHubPreviewRepairRequestPath = ".repair_hubpreview_request";
        private const string PendingMainMenuPromotionRequestPath = "Assets/WordSpinAlpha/Generated/.promote_mainmenu_request";
        private const float HubPreviewDisplayRotationZ = 0f;
        private static readonly Vector2 MainMenuReferenceSize = new Vector2(864f, 1536f);
        private static readonly Vector2 MainMenuBackgroundCoverSize = new Vector2(1080f, 1920f);
        private static readonly Vector2 LevelHubNodeSize = new Vector2(230f, 138f);
        private static readonly Vector2 LevelHubNodeVisualSize = new Vector2(308f, 462f);
        private static readonly Vector2 LevelHubNodeLabelSize = new Vector2(182f, 84f);
        private static readonly Vector2 LevelHubNodeLabelPosition = new Vector2(0f, 2f);
        private const float LevelHubNodeLabelFontSize = 40f;
        private static readonly Color LevelHubNodeHostColor = new Color(1f, 1f, 1f, 0f);
        private static readonly Vector2 LevelHubPreviewBottomNavMaxSize = new Vector2(710f, 143f);
        private static readonly Vector2 LevelHubPreviewBottomNavAnchoredPosition = new Vector2(0f, -450f);
        private static bool pendingHubPreviewRebuildAfterPlayMode;

        [InitializeOnLoadMethod]
        private static void AutoBuild()
        {
            EditorApplication.delayCall += TryBuild;
            EditorApplication.delayCall += TryRunPendingHubPreviewRebuild;
            EditorApplication.delayCall += TryRunPendingHubPreviewRepair;
            EditorApplication.delayCall += TryRestoreBrokenHubPreviewScene;
            EditorApplication.delayCall += TryRunPendingMainMenuPromotion;
        }

        [MenuItem("Tools/WordSpin Alpha/Rebuild Scenes")]
        public static void RebuildScenes() => BuildAll(false);

        [MenuItem("Tools/WordSpin Alpha/Force Reset Generated Scenes")]
        public static void ForceResetGeneratedScenes() => BuildAll(true);

        [MenuItem("Tools/WordSpin Alpha/Rebuild Figma Hub Preview Scene")]
        public static void RebuildFigmaHubPreviewScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                QueueHubPreviewRebuildAfterPlayMode();
                return;
            }

            RebuildFigmaHubPreviewSceneNow();
        }

        [MenuItem("Tools/WordSpin Alpha/Rebuild Level Hub Preview Scene")]
        public static void RebuildLevelHubPreviewScene()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[WordSpinAlpha] Level Hub Preview rebuild Play Mode sirasinda calistirilamaz. Play Mode'u kapatip tekrar deneyin.");
                return;
            }

            LevelHubPreviewState preservedState = CaptureCurrentLevelHubPreviewState();

            EnsureFolder("Assets/WordSpinAlpha/Generated");
            EnsureFolder(Prefabs);
            EnsureFolder("Assets/WordSpinAlpha/Art/UI/LevelHubPreview");
            EnsureFolder(LevelHubPreviewArt);
            AssetDatabase.Refresh();
            BuildLevelHubPreviewScene(preservedState);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[WordSpinAlpha] Level Hub Preview sahnesi olusturuldu. HubPreview.unity dosyasini Scenes klasoründen acabilirsin.");
        }

        public static void RebuildMainMenuSceneFromConfig()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isUpdating)
            {
                Debug.LogWarning("[WordSpinAlpha] MainMenu rebuild Play Mode veya editor update sirasinda calistirilamaz.");
                return;
            }

            EnsureFolder("Assets/WordSpinAlpha/Generated");
            EnsureFolder(Prefabs);
            BuildMainMenuScene(true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[WordSpinAlpha] MainMenu sahnesi ve generated preview yeniden olusturuldu.");
        }

        [MenuItem("Tools/WordSpin Alpha/Repair Level Hub Preview Node Visuals")]
        public static void RepairLevelHubPreviewNodeVisuals()
        {
            if (!RunLevelHubPreviewRepair())
            {
                Debug.LogWarning("[WordSpinAlpha] Level hub repair tamamlanamadi; tekrar deneme gerekebilir.");
            }
        }

        [MenuItem("Tools/WordSpin Alpha/Promote Approved Hub Preview To Main Menu")]
        public static void PromoteApprovedHubPreviewToMainMenu()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogWarning("[WordSpinAlpha] MainMenu aktarimi Play Mode sirasinda yapilamaz. Play Mode'u kapatip tekrar deneyin.");
                return;
            }

            EnsureFolder("Assets/WordSpinAlpha/Generated");
            EnsureFolder(Prefabs);
            BuildMainMenuPngPreviewPrefab(true);
            BuildMainMenuScene(true);
            BuildHubPreviewScene(true);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneBoot}.unity", true),
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneMainMenu}.unity", true),
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneHub}.unity", true),
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneGameplay}.unity", true),
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneStore}.unity", true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[WordSpinAlpha] Onayli HubPreview tasarimi runtime MainMenu sahnesine aktarildi; HubPreview temiz sandbox'a alindi.");
        }

        private static void RebuildFigmaHubPreviewSceneNow()
        {
            EnsureFolder("Assets/WordSpinAlpha/Generated");
            EnsureFolder(Prefabs);
            EnsureFolder(RotatorShapePrefabs);
            BuildHubPreviewScene(true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[WordSpinAlpha] Hub preview scene generated.");
        }

        private static void QueueHubPreviewRebuildAfterPlayMode()
        {
            pendingHubPreviewRebuildAfterPlayMode = true;
            SessionState.SetBool(PendingHubPreviewRebuildSessionKey, true);
            EditorApplication.playModeStateChanged -= OnPlayModeStateChangedForHubPreviewRebuild;
            EditorApplication.playModeStateChanged += OnPlayModeStateChangedForHubPreviewRebuild;
            Debug.LogWarning("[WordSpinAlpha] HubPreview rebuild Play Mode sirasinda baslatildi. Play Mode kapatiliyor; cikistan sonra preview sahnesi otomatik yenilenecek.");

            if (EditorApplication.isPlaying)
            {
                EditorApplication.isPlaying = false;
            }
        }

        private static void OnPlayModeStateChangedForHubPreviewRebuild(PlayModeStateChange state)
        {
            if ((!pendingHubPreviewRebuildAfterPlayMode && !SessionState.GetBool(PendingHubPreviewRebuildSessionKey, false)) ||
                state != PlayModeStateChange.EnteredEditMode)
            {
                return;
            }

            pendingHubPreviewRebuildAfterPlayMode = false;
            EditorApplication.playModeStateChanged -= OnPlayModeStateChangedForHubPreviewRebuild;
            EditorApplication.delayCall += TryRunPendingHubPreviewRebuild;
        }

        private static void TryRunPendingHubPreviewRebuild()
        {
            if (!SessionState.GetBool(PendingHubPreviewRebuildSessionKey, false))
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += TryRunPendingHubPreviewRebuild;
                return;
            }

            pendingHubPreviewRebuildAfterPlayMode = false;
            SessionState.SetBool(PendingHubPreviewRebuildSessionKey, false);
            RebuildFigmaHubPreviewSceneNow();
        }

        private static void TryRunPendingMainMenuPromotion()
        {
            string requestPath = System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).FullName,
                PendingMainMenuPromotionRequestPath.Replace('/', System.IO.Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(requestPath))
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                EditorApplication.delayCall += TryRunPendingMainMenuPromotion;
                return;
            }

            try
            {
                EditorSceneManager.SaveOpenScenes();
                PromoteApprovedHubPreviewToMainMenu();
                System.IO.File.Delete(requestPath);
                AssetDatabase.Refresh();
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[WordSpinAlpha] MainMenu aktarim istegi calistirilamadi: {exception}");
            }
        }

        private static void TryRunPendingHubPreviewRepair()
        {
            string requestPath = System.IO.Path.Combine(
                System.IO.Directory.GetParent(Application.dataPath).FullName,
                PendingHubPreviewRepairRequestPath.Replace('/', System.IO.Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(requestPath))
            {
                return;
            }

            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryRunPendingHubPreviewRepair;
                return;
            }

            try
            {
                EditorSceneManager.SaveOpenScenes();
                if (RunLevelHubPreviewRepair())
                {
                    System.IO.File.Delete(requestPath);
                    AssetDatabase.Refresh();
                    return;
                }

                EditorApplication.delayCall += TryRunPendingHubPreviewRepair;
            }
            catch (System.Exception exception)
            {
                Debug.LogError($"[WordSpinAlpha] HubPreview repair istegi calistirilamadi: {exception}");
                EditorApplication.delayCall += TryRunPendingHubPreviewRepair;
            }
        }

        private static void TryRestoreBrokenHubPreviewScene()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isUpdating)
            {
                EditorApplication.delayCall += TryRestoreBrokenHubPreviewScene;
                return;
            }

            if (!System.IO.File.Exists(LevelHubPreviewScenePath))
            {
                return;
            }

            string sceneText = System.IO.File.ReadAllText(LevelHubPreviewScenePath);
            bool hasCanvas = sceneText.IndexOf("HubPreviewCanvas", System.StringComparison.Ordinal) >= 0;
            bool hasRoot = sceneText.IndexOf("LevelHubPreviewRoot", System.StringComparison.Ordinal) >= 0;
            bool hasPoolNodes = sceneText.IndexOf("LevelNode_0", System.StringComparison.Ordinal) >= 0;

            if (hasCanvas && hasRoot && hasPoolNodes)
            {
                return;
            }

            Debug.LogWarning("[WordSpinAlpha] HubPreview scene dosyasi eksik yapida gorunuyor; kaynaklardan yeniden olusturuluyor.");
            LevelHubPreviewState preservedState = CaptureCurrentLevelHubPreviewState();
            BuildLevelHubPreviewScene(preservedState);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static bool RunLevelHubPreviewRepair()
        {
            AssetDatabase.Refresh();

            Sprite nodeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LevelHubNodeSpritePath);
            if (nodeSprite == null)
            {
                Debug.LogError("[WordSpinAlpha] Level hub node sprite bulunamadi. levelsecimkutu.png kontrol edilmeli.");
                return false;
            }

            EnsureFolder("Assets/WordSpinAlpha/Generated");
            EnsureFolder(Prefabs);

            bool changed = false;
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(LevelHubPreviewPrefabPath);

            try
            {
                changed |= NormalizeLevelHubNodeVisuals(prefabRoot, nodeSprite);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, LevelHubPreviewPrefabPath);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }

            AssetDatabase.ImportAsset(LevelHubPreviewPrefabPath, ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh();

            GameObject freshPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(LevelHubPreviewPrefabPath);
            if (freshPrefab == null)
            {
                Debug.LogError("[WordSpinAlpha] LevelHubPreview prefab yenilenemedi.");
                return false;
            }

            var scene = EditorSceneManager.OpenScene(LevelHubPreviewScenePath, OpenSceneMode.Single);
            Canvas canvas = FindHubPreviewCanvas();
            if (canvas == null)
            {
                Debug.LogError("[WordSpinAlpha] HubPreviewCanvas bulunamadi. HubPreview sahnesi bozuk olabilir.");
                return false;
            }

            changed |= NormalizeHubPreviewCanvas(canvas);

            LevelHubPreviewController[] controllers = Object.FindObjectsByType<LevelHubPreviewController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            LevelHubPreviewController sourceController = null;
            for (int i = 0; i < controllers.Length; i++)
            {
                LevelHubPreviewController controller = controllers[i];
                if (controller == null || controller.gameObject.scene != scene)
                {
                    continue;
                }

                sourceController = controller;
                break;
            }

            LevelHubPreviewState preservedState = CaptureLevelHubPreviewState(sourceController);

            for (int i = 0; i < controllers.Length; i++)
            {
                LevelHubPreviewController controller = controllers[i];
                if (controller == null || controller.gameObject.scene != scene)
                {
                    continue;
                }

                Object.DestroyImmediate(controller.gameObject);
                changed = true;
            }

            changed |= RemoveRootNodeVisualOrphans(scene) > 0;

            GameObject instance = PrefabUtility.InstantiatePrefab(freshPrefab, canvas.transform) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[WordSpinAlpha] LevelHubPreview prefab instance olusturulamadi.");
                return false;
            }

            LevelHubPreviewController newController = instance.GetComponent<LevelHubPreviewController>();
            changed |= ApplyLevelHubPreviewState(newController, preservedState);
            changed |= NormalizeLevelHubNodeVisuals(instance, nodeSprite);
            changed |= RemoveRootNodeVisualOrphans(scene) > 0;

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log(changed
                ? "[WordSpinAlpha] Level hub prefab/scene kalici olarak temizlendi; orphan NodeVisual ve kirli override'lar giderildi."
                : "[WordSpinAlpha] Level hub node gorunumu zaten gunceldi.");

            return true;
        }

        [MenuItem("Tools/WordSpin Alpha/Rebuild Rotator Shape Prefabs")]
        public static void RebuildRotatorShapePrefabs() => BuildRotatorShapePrefabs(true);

        private static void TryBuild()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneBoot}.unity") != null &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneMainMenu}.unity") != null &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneHub}.unity") != null &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneHubPreview}.unity") != null &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneGameplay}.unity") != null &&
                AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneStore}.unity") != null)
            {
                return;
            }

            BuildAll(false);
        }

        private static void BuildAll(bool forceReset)
        {
            EnsureFolder("Assets/WordSpinAlpha/Generated");
            EnsureFolder(Prefabs);
            EnsureFolder(RotatorShapePrefabs);
            GameObject pinPrefab = BuildPinPrefab(forceReset);
            GameObject keyPrefab = BuildKeyPrefab(forceReset);
            BuildRotatorShapePrefabs(forceReset);
            BuildMainMenuPngPreviewPrefab(forceReset);
            BuildBootScene(forceReset);
            BuildMainMenuScene(forceReset);
            BuildHubScene(forceReset);
            BuildHubPreviewScene(forceReset);
            BuildGameplayScene(pinPrefab, keyPrefab, forceReset);
            BuildStoreScene(forceReset);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneBoot}.unity", true),
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneMainMenu}.unity", true),
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneHub}.unity", true),
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneGameplay}.unity", true),
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneStore}.unity", true)
            };
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[WordSpinAlpha] Scenes generated.");
        }

        private static void BuildBootScene(bool forceReset)
        {
            if (!forceReset && AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneBoot}.unity") != null)
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Cam("Boot Camera", new Color(0.03f, 0.03f, 0.05f));
            GameObject sys = new GameObject("BootSystems");
            sys.AddComponent<SceneBootstrap>();
            sys.AddComponent<BootLoader>();
            EditorSceneManager.SaveScene(scene, $"{Scenes}/{GameConstants.SceneBoot}.unity");
        }

        private static void BuildMainMenuScene(bool forceReset)
        {
            if (!forceReset && AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneMainMenu}.unity") != null)
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Cam("MainMenu Camera", new Color(0.02f, 0.03f, 0.06f));
            EventSystem();
            new GameObject("MainMenuSystems").AddComponent<SceneBootstrap>();
            Canvas canvas = Canvas("MainMenuCanvas");
            BuildDesignedMainMenuRuntimeScene(scene, canvas);
        }

        private static void BuildDesignedMainMenuRuntimeScene(UnityEngine.SceneManagement.Scene scene, Canvas canvas)
        {
            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.referenceResolution = MainMenuReferenceSize;
                scaler.matchWidthOrHeight = 1f;
            }

            GameObject designRoot = BuildMainMenuPngPreviewLayer(canvas.transform);
            MainMenuPresenter presenter = canvas.gameObject.AddComponent<MainMenuPresenter>();

            Ref(presenter, "energyLabel", HiddenPresenterLabel("Energy", canvas.transform));
            Ref(presenter, "hintLabel", HiddenPresenterLabel("Hints", canvas.transform));
            Ref(presenter, "languageLabel", HiddenPresenterLabel("Language", canvas.transform));

            ConfigureRuntimeMainMenuInteractions(designRoot, presenter);
            EditorSceneManager.SaveScene(scene, $"{Scenes}/{GameConstants.SceneMainMenu}.unity");
        }

        private static TextMeshProUGUI HiddenPresenterLabel(string name, Transform parent)
        {
            TextMeshProUGUI label = Label(name, parent, string.Empty, 18f, new Vector2(0.5f, 0.5f), new Vector2(10f, 10f), new Color(1f, 1f, 1f, 0f));
            label.raycastTarget = false;
            label.gameObject.SetActive(false);
            return label;
        }

        private static void ConfigureRuntimeMainMenuInteractions(GameObject designRoot, MainMenuPresenter presenter)
        {
            if (designRoot == null || presenter == null)
            {
                return;
            }

            RectTransform root = designRoot.GetComponent<RectTransform>();
            if (root == null)
            {
                return;
            }

            MainMenuLanguagePreviewHighlighter highlighter = designRoot.GetComponent<MainMenuLanguagePreviewHighlighter>();
            highlighter?.Refresh();

            ConfigureRuntimeLanguageButton(root, "TR", presenter.SetLanguageTR);
            ConfigureRuntimeLanguageButton(root, "EN", presenter.SetLanguageEN);
            ConfigureRuntimeLanguageButton(root, "DE", presenter.SetLanguageDE);
            ConfigureRuntimeLanguageButton(root, "ES", presenter.SetLanguageES);
            ConfigureRuntimePlayButton(root);
        }

        private static void ConfigureRuntimeLanguageButton(RectTransform root, string label, UnityAction action)
        {
            RectTransform rect = FindDirectChildRect(root, $"LanguageButton_{label}");
            if (rect == null)
            {
                return;
            }

            MainMenuPreviewLanguageButton previewButton = rect.GetComponent<MainMenuPreviewLanguageButton>();
            if (previewButton != null)
            {
                Object.DestroyImmediate(previewButton);
            }

            Image image = rect.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = true;
            }

            Button button = rect.GetComponent<Button>();
            if (button == null)
            {
                button = rect.gameObject.AddComponent<Button>();
            }

            button.targetGraphic = image;
            button.onClick.RemoveAllListeners();
            UnityEventTools.AddPersistentListener(button.onClick, action);
        }

        private static void ConfigureRuntimePlayButton(RectTransform root)
        {
            RectTransform rect = FindDirectChildRect(root, "PlayButton_Hitbox");
            if (rect == null)
            {
                return;
            }

            Image image = rect.GetComponent<Image>();
            if (image != null)
            {
                image.raycastTarget = true;
            }

            Button button = rect.GetComponent<Button>();
            if (button != null)
            {
                Object.DestroyImmediate(button);
            }
        }

        private static RectTransform FindDirectChildRect(Transform parent, string childName)
        {
            if (parent == null)
            {
                return null;
            }

            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child as RectTransform;
                }
            }

            return null;
        }

        private static void BuildHubScene(bool forceReset)
        {
            if (!forceReset && AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneHub}.unity") != null)
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Cam("Hub Camera", new Color(0.38f, 0.72f, 0.94f));
            EventSystem();
            new GameObject("HubSystems").AddComponent<SceneBootstrap>();
            Canvas canvas = Canvas("HubCanvas");

            Panel("Backdrop", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(1080f, 1920f), new Color(0.48f, 0.78f, 0.98f, 1f));
            Image cloudA = Panel("CloudA", canvas.transform, new Vector2(0.22f, 0.92f), new Vector2(420f, 180f), new Color(1f, 1f, 1f, 0.20f));
            cloudA.sprite = BuiltinRound();
            cloudA.type = Image.Type.Simple;
            Image cloudB = Panel("CloudB", canvas.transform, new Vector2(0.82f, 0.88f), new Vector2(360f, 160f), new Color(1f, 1f, 1f, 0.16f));
            cloudB.sprite = BuiltinRound();
            cloudB.type = Image.Type.Simple;
            Image sea = Panel("SeaBand", canvas.transform, new Vector2(0.5f, 0.18f), new Vector2(1280f, 420f), new Color(0.16f, 0.55f, 0.84f, 0.24f));
            sea.sprite = BuiltinRound();
            sea.type = Image.Type.Simple;

            Image topBar = Panel("TopBar", canvas.transform, new Vector2(0.5f, 0.955f), new Vector2(1000f, 128f), new Color(0.18f, 0.30f, 0.47f, 0.84f));
            Image avatar = Panel("AvatarChip", topBar.transform, new Vector2(0.08f, 0.5f), new Vector2(86f, 86f), new Color(1f, 0.93f, 0.82f, 1f));
            avatar.sprite = BuiltinRound();
            avatar.type = Image.Type.Simple;
            Label("AvatarLabel", avatar.transform, "WS", 28, new Vector2(0.5f, 0.5f), new Vector2(60f, 40f), new Color(0.25f, 0.18f, 0.12f));
            TextMeshProUGUI energyLabel = Label("EnergyLabel", topBar.transform, "Can 5/5", 24, new Vector2(0.28f, 0.56f), new Vector2(180f, 42f), Color.white);
            TextMeshProUGUI hintLabel = Label("HintLabel", topBar.transform, "Ipucu 0", 24, new Vector2(0.46f, 0.56f), new Vector2(180f, 42f), Color.white);
            TextMeshProUGUI coinLabel = Label("CoinLabel", topBar.transform, "0", 26, new Vector2(0.66f, 0.56f), new Vector2(180f, 42f), Color.white);
            TextMeshProUGUI languageLabel = Label("LanguageLabel", topBar.transform, "TR", 22, new Vector2(0.88f, 0.56f), new Vector2(120f, 38f), new Color(1f, 0.96f, 0.84f));

            Image headerCard = Panel("HeaderCard", canvas.transform, new Vector2(0.5f, 0.865f), new Vector2(920f, 128f), new Color(0.30f, 0.52f, 0.76f, 0.66f));
            TextMeshProUGUI headerTitle = Label("HeaderTitle", headerCard.transform, "Macera Merkezi", 40, new Vector2(0.5f, 0.64f), new Vector2(760f, 52f), Color.white);
            TextMeshProUGUI headerSubtitle = Label("HeaderSubtitle", headerCard.transform, "Alt menuden gecis yap, seviyeni sec ve devam et.", 22, new Vector2(0.5f, 0.28f), new Vector2(760f, 44f), new Color(0.92f, 0.97f, 1f));

            RectTransform pageViewport = Rt(new GameObject("PageViewport", typeof(RectTransform)), canvas.transform, new Vector2(0.5f, 0.45f), new Vector2(1000f, 1290f));
            Image viewportSkin = pageViewport.gameObject.AddComponent<Image>();
            viewportSkin.sprite = Builtin();
            viewportSkin.type = Image.Type.Sliced;
            viewportSkin.color = new Color(1f, 1f, 1f, 0.02f);
            pageViewport.gameObject.AddComponent<RectMask2D>();

            RectTransform journeyPage = Rt(new GameObject("JourneyPage", typeof(RectTransform)), pageViewport, new Vector2(0.5f, 0.5f), pageViewport.sizeDelta);
            RectTransform missionsPage = Rt(new GameObject("MissionsPage", typeof(RectTransform)), pageViewport, new Vector2(0.5f, 0.5f), pageViewport.sizeDelta);
            RectTransform profilePage = Rt(new GameObject("ProfilePage", typeof(RectTransform)), pageViewport, new Vector2(0.5f, 0.5f), pageViewport.sizeDelta);
            RectTransform storePage = Rt(new GameObject("StorePage", typeof(RectTransform)), pageViewport, new Vector2(0.5f, 0.5f), pageViewport.sizeDelta);

            TextMeshProUGUI mapTitle = Label("MapTitle", journeyPage, "Yol Haritasi", 34, new Vector2(0.5f, 0.96f), new Vector2(420f, 48f), new Color(1f, 0.97f, 0.90f));
            TextMeshProUGUI mapSummary = Label("MapSummary", journeyPage, "Ilerleme: 1/1", 22, new Vector2(0.5f, 0.92f), new Vector2(460f, 40f), Color.white);
            GameObject scrollRoot = new GameObject("JourneyScrollView", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            Rt(scrollRoot, journeyPage, new Vector2(0.5f, 0.56f), new Vector2(900f, 920f));
            Image scrollBackground = scrollRoot.GetComponent<Image>();
            scrollBackground.sprite = Builtin();
            scrollBackground.type = Image.Type.Sliced;
            scrollBackground.color = new Color(0.94f, 0.96f, 0.98f, 0.14f);
            scrollRoot.GetComponent<Mask>().showMaskGraphic = true;

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform viewportRect = Rt(viewport, scrollRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(876f, 896f));
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.sprite = Builtin();
            viewportImage.type = Image.Type.Sliced;
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform));
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.SetParent(viewport.transform, false);
            contentRect.anchorMin = new Vector2(0.5f, 1f);
            contentRect.anchorMax = new Vector2(0.5f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(876f, 1680f);

            ScrollRect levelScrollRect = scrollRoot.GetComponent<ScrollRect>();
            levelScrollRect.viewport = viewportRect;
            levelScrollRect.content = contentRect;
            levelScrollRect.horizontal = false;
            levelScrollRect.vertical = true;
            levelScrollRect.scrollSensitivity = 34f;

            Button levelButtonTemplate = Button("LevelButtonTemplate", content.transform, "1", new Vector2(0.5f, 0.5f), new Vector2(308f, 152f), new Color(0.30f, 0.51f, 0.71f, 1f));
            levelButtonTemplate.gameObject.SetActive(false);
            LevelPathMapView levelPathMap = scrollRoot.AddComponent<LevelPathMapView>();
            Ref(levelPathMap, "scrollRect", levelScrollRect);
            Ref(levelPathMap, "viewport", viewportRect);
            Ref(levelPathMap, "content", contentRect);
            Ref(levelPathMap, "levelButtonTemplate", levelButtonTemplate);

            Button playSelectedLevel;
            TextMeshProUGUI selectedLevelTitle;
            TextMeshProUGUI selectedLevelBody;

            TextMeshProUGUI missionsTitle = Label("MissionsTitle", missionsPage, "Gorevler ve Etkinlikler", 36, new Vector2(0.5f, 0.93f), new Vector2(600f, 48f), Color.white);
            TextMeshProUGUI missionsSummary = Label("MissionsSummary", missionsPage, "Gunluk hedefler bu panelde listelenecek.", 24, new Vector2(0.5f, 0.86f), new Vector2(760f, 84f), new Color(0.92f, 0.97f, 1f));
            Panel("MissionCardA", missionsPage, new Vector2(0.5f, 0.65f), new Vector2(820f, 180f), new Color(0.20f, 0.33f, 0.50f, 0.86f));
            Panel("MissionCardB", missionsPage, new Vector2(0.5f, 0.42f), new Vector2(820f, 180f), new Color(0.22f, 0.28f, 0.43f, 0.86f));
            Label("MissionCardALabel", missionsPage, "Gecici gorev karti alani", 28, new Vector2(0.5f, 0.65f), new Vector2(760f, 42f), Color.white);
            Label("MissionCardBLabel", missionsPage, "Etkinlik teklifleri burada gorunecek", 28, new Vector2(0.5f, 0.42f), new Vector2(760f, 42f), Color.white);

            TextMeshProUGUI profileTitle = Label("ProfileTitle", profilePage, "Oyuncu Profili", 36, new Vector2(0.5f, 0.93f), new Vector2(600f, 48f), Color.white);
            TextMeshProUGUI profileSummary = Label("ProfileSummary", profilePage, "Dil, ilerleme ve aktif tema burada ozetlenecek.", 24, new Vector2(0.5f, 0.86f), new Vector2(760f, 84f), new Color(0.92f, 0.97f, 1f));
            Panel("ProfileCardA", profilePage, new Vector2(0.5f, 0.64f), new Vector2(820f, 190f), new Color(0.18f, 0.28f, 0.44f, 0.88f));
            Panel("ProfileCardB", profilePage, new Vector2(0.5f, 0.40f), new Vector2(820f, 190f), new Color(0.20f, 0.25f, 0.38f, 0.88f));
            Label("ProfileCardALabel", profilePage, "Tema ve uyelik durumu", 28, new Vector2(0.5f, 0.64f), new Vector2(760f, 42f), Color.white);
            Label("ProfileCardBLabel", profilePage, "Ilerleme ve ozet istatistikler", 28, new Vector2(0.5f, 0.40f), new Vector2(760f, 42f), Color.white);

            TextMeshProUGUI storeTitle = Label("StoreTitle", storePage, "Teklifler ve Magaza", 36, new Vector2(0.5f, 0.93f), new Vector2(600f, 48f), Color.white);
            TextMeshProUGUI storeSummary = Label("StoreSummary", storePage, "Tema ve kaynak kartlari gecici olarak bu sekmede.", 24, new Vector2(0.5f, 0.86f), new Vector2(760f, 84f), new Color(0.92f, 0.97f, 1f));
            Button storeThemeButton = Button("StoreThemeButton", storePage, "Tema Paketi", new Vector2(0.5f, 0.64f), new Vector2(460f, 92f), new Color(0.81f, 0.47f, 0.22f));
            Button storeHintsButton = Button("StoreHintsButton", storePage, "Ipucu x3", new Vector2(0.5f, 0.51f), new Vector2(460f, 92f), new Color(0.25f, 0.40f, 0.60f));
            Button storeEnergyButton = Button("StoreEnergyButton", storePage, "Can +3", new Vector2(0.5f, 0.38f), new Vector2(460f, 92f), new Color(0.23f, 0.55f, 0.56f));
            Button storeMembershipButton = Button("StoreMembershipButton", storePage, "Premium", new Vector2(0.5f, 0.25f), new Vector2(460f, 92f), new Color(0.42f, 0.28f, 0.22f));

            Button quickGiftButton = Button("QuickGiftButton", canvas.transform, "Hediye", new Vector2(0.90f, 0.75f), new Vector2(156f, 68f), new Color(0.97f, 0.62f, 0.28f));
            Button quickMissionButton = Button("QuickMissionButton", canvas.transform, "Etkinlik", new Vector2(0.90f, 0.67f), new Vector2(156f, 68f), new Color(0.92f, 0.47f, 0.54f));
            Button quickStoreButton = Button("QuickStoreButton", canvas.transform, "Teklif", new Vector2(0.90f, 0.59f), new Vector2(156f, 68f), new Color(0.58f, 0.42f, 0.92f));
            TextMeshProUGUI quickToastLabel = Label("QuickToast", canvas.transform, "Hazir", 22, new Vector2(0.5f, 0.16f), new Vector2(560f, 40f), new Color(1f, 0.95f, 0.88f));

            Button navJourneyButton;
            Button navMissionsButton;
            Button navProfileButton;
            Button navStoreButton;
            Image selectedCard = Panel("SelectedCard", journeyPage, new Vector2(0.5f, 0.11f), new Vector2(760f, 186f), new Color(0.94f, 0.42f, 0.34f, 0.92f));
            selectedLevelTitle = Label("SelectedLevelTitle", selectedCard.transform, "Level 1", 40, new Vector2(0.5f, 0.68f), new Vector2(520f, 54f), Color.white);
            selectedLevelBody = Label("SelectedLevelBody", selectedCard.transform, "Aktif seviyen hazir.", 22, new Vector2(0.5f, 0.40f), new Vector2(600f, 42f), new Color(1f, 0.94f, 0.86f));
            playSelectedLevel = Button("PlaySelectedLevel", selectedCard.transform, "Seviyeyi Oyna", new Vector2(0.5f, 0.12f), new Vector2(280f, 60f), new Color(0.92f, 0.56f, 0.24f));

            Image navBar = Panel("BottomNav", canvas.transform, new Vector2(0.5f, 0.055f), new Vector2(1000f, 144f), new Color(0.18f, 0.31f, 0.47f, 0.88f));
            navJourneyButton = Button("NavJourneyButton", navBar.transform, "Yolculuk", new Vector2(0.16f, 0.52f), new Vector2(186f, 92f), new Color(0.26f, 0.55f, 0.89f));
            navMissionsButton = Button("NavMissionsButton", navBar.transform, "Gorevler", new Vector2(0.39f, 0.52f), new Vector2(186f, 92f), new Color(0.16f, 0.22f, 0.31f));
            navProfileButton = Button("NavProfileButton", navBar.transform, "Profil", new Vector2(0.62f, 0.52f), new Vector2(186f, 92f), new Color(0.16f, 0.22f, 0.31f));
            navStoreButton = Button("NavStoreButton", navBar.transform, "Magaza", new Vector2(0.85f, 0.52f), new Vector2(186f, 92f), new Color(0.16f, 0.22f, 0.31f));

            HubPresenter presenter = canvas.gameObject.AddComponent<HubPresenter>();
            Ref(presenter, "energyLabel", energyLabel);
            Ref(presenter, "hintLabel", hintLabel);
            Ref(presenter, "coinLabel", coinLabel);
            Ref(presenter, "languageLabel", languageLabel);
            Ref(presenter, "headerTitleLabel", headerTitle);
            Ref(presenter, "headerSubtitleLabel", headerSubtitle);
            Ref(presenter, "pageViewport", pageViewport);
            Ref(presenter, "journeyPage", journeyPage);
            Ref(presenter, "missionsPage", missionsPage);
            Ref(presenter, "profilePage", profilePage);
            Ref(presenter, "storePage", storePage);
            Ref(presenter, "levelPathMapView", levelPathMap);
            Ref(presenter, "levelButtonContainer", content.transform);
            Ref(presenter, "levelButtonTemplate", levelButtonTemplate);
            Ref(presenter, "mapTitleLabel", mapTitle);
            Ref(presenter, "mapSummaryLabel", mapSummary);
            Ref(presenter, "selectedLevelTitleLabel", selectedLevelTitle);
            Ref(presenter, "selectedLevelBodyLabel", selectedLevelBody);
            Ref(presenter, "playSelectedLevelButton", playSelectedLevel);
            Ref(presenter, "missionsTitleLabel", missionsTitle);
            Ref(presenter, "missionsSummaryLabel", missionsSummary);
            Ref(presenter, "profileTitleLabel", profileTitle);
            Ref(presenter, "profileSummaryLabel", profileSummary);
            Ref(presenter, "storeTitleLabel", storeTitle);
            Ref(presenter, "storeSummaryLabel", storeSummary);
            Ref(presenter, "quickToastLabel", quickToastLabel);
            Ref(presenter, "navJourneyButton", navJourneyButton);
            Ref(presenter, "navMissionsButton", navMissionsButton);
            Ref(presenter, "navProfileButton", navProfileButton);
            Ref(presenter, "navStoreButton", navStoreButton);
            Ref(presenter, "quickGiftButton", quickGiftButton);
            Ref(presenter, "quickMissionButton", quickMissionButton);
            Ref(presenter, "quickStoreButton", quickStoreButton);
            Ref(presenter, "storeThemeButton", storeThemeButton);
            Ref(presenter, "storeHintsButton", storeHintsButton);
            Ref(presenter, "storeEnergyButton", storeEnergyButton);
            Ref(presenter, "storeMembershipButton", storeMembershipButton);
            EditorSceneManager.SaveScene(scene, $"{Scenes}/{GameConstants.SceneHub}.unity");
        }

        private static LevelHubPreviewState CaptureCurrentLevelHubPreviewState()
        {
            return CaptureLevelHubPreviewState(FindCurrentLevelHubPreviewController());
        }

        private static LevelHubPreviewController FindCurrentLevelHubPreviewController()
        {
            LevelHubPreviewController[] controllers = Object.FindObjectsByType<LevelHubPreviewController>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < controllers.Length; i++)
            {
                LevelHubPreviewController controller = controllers[i];
                if (controller == null)
                {
                    continue;
                }

                var scene = controller.gameObject.scene;
                if (!scene.IsValid() || !scene.isLoaded)
                {
                    continue;
                }

                if (scene.name == GameConstants.SceneHubPreview || scene.path == LevelHubPreviewScenePath)
                {
                    return controller;
                }
            }

            return null;
        }

        private static void BuildLevelHubPreviewScene()
        {
            BuildLevelHubPreviewScene(CaptureCurrentLevelHubPreviewState());
        }

        private static void BuildLevelHubPreviewScene(LevelHubPreviewState preservedState)
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlaying || EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isUpdating)
            {
                Debug.LogWarning("[WordSpinAlpha] Level Hub Preview rebuild Play Mode veya editor update sirasinda calistirilamaz.");
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Cam("LevelHubPreview Camera", new Color(0.08f, 0.06f, 0.04f));
            EventSystem();
            new GameObject("HubPreviewSystems").AddComponent<SceneBootstrap>();

            Canvas canvas = Canvas("HubPreviewCanvas");
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            canvasRect.anchorMin = Vector2.zero;
            canvasRect.anchorMax = Vector2.one;
            canvasRect.pivot = new Vector2(0.5f, 0.5f);
            canvasRect.anchoredPosition = Vector2.zero;
            canvasRect.sizeDelta = Vector2.zero;
            canvasRect.localRotation = Quaternion.identity;
            canvasRect.localScale = Vector3.one;

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            scaler.referenceResolution = MainMenuReferenceSize;
            scaler.matchWidthOrHeight = 1f;

            GameObject prefab = BuildLevelHubPreviewPrefab();
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, canvas.transform) as GameObject;
            if (instance == null)
            {
                Debug.LogError("[WordSpinAlpha] LevelHubPreview prefab instance olusturulamadi.");
                return;
            }

            instance.name = "LevelHubPreviewRoot";
            RectTransform instanceRect = instance.GetComponent<RectTransform>();
            instanceRect.anchorMin = Vector2.zero;
            instanceRect.anchorMax = Vector2.one;
            instanceRect.pivot = new Vector2(0.5f, 0.5f);
            instanceRect.anchoredPosition = Vector2.zero;
            instanceRect.sizeDelta = Vector2.zero;
            instanceRect.localRotation = Quaternion.identity;
            instanceRect.localScale = Vector3.one;

            LevelHubPreviewController newController = instance.GetComponent<LevelHubPreviewController>();
            ApplyLevelHubPreviewState(newController, preservedState);

            EditorSceneManager.SaveScene(scene, $"{Scenes}/{GameConstants.SceneHubPreview}.unity");
        }

        private static GameObject BuildLevelHubPreviewPrefab()
        {
            string path = LevelHubPreviewPrefabPath;

            // Root: tam ekran, transparan image (drag input için), controller
            GameObject root = new GameObject("LevelHubPreviewRoot", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = Vector2.zero;
            rootRect.anchorMax = Vector2.one;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = Vector2.zero;
            rootRect.sizeDelta = Vector2.zero;
            rootRect.localRotation = Quaternion.identity;
            rootRect.localScale = Vector3.one;

            Image rootBg = root.GetComponent<Image>();
            rootBg.sprite = Builtin();
            rootBg.color = new Color(0f, 0f, 0f, 0f);
            rootBg.raycastTarget = true;

            LevelHubPreviewController controller = root.AddComponent<LevelHubPreviewController>();

            // Arka plan (tam ekran stretch)
            GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            RectTransform bgRect = bgGo.GetComponent<RectTransform>();
            bgRect.SetParent(rootRect, false);
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.pivot = new Vector2(0.5f, 0.5f);
            bgRect.anchoredPosition = Vector2.zero;
            bgRect.sizeDelta = Vector2.zero;
            bgRect.localScale = Vector3.one;
            Image bgImage = bgGo.GetComponent<Image>();
            bgImage.sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{LevelHubPreviewArt}/arkaplan.png");
            bgImage.type = Image.Type.Simple;
            bgImage.preserveAspect = false;
            bgImage.color = bgImage.sprite != null ? Color.white : new Color(0.22f, 0.16f, 0.10f, 1f);
            bgImage.raycastTarget = false;

            HubPreviewLayoutTuningProfile.LayoutElementTuning alttasLayout = HubPreviewLayoutTuningProfile.ResolveAlttas();

            // Alttas (aktif level platformu, path'in arkasında render olacak)
            Sprite alttasSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{LevelHubPreviewArt}/levelsecimkutu-alttas.png");
            GameObject alttasGo = new GameObject("Alttas", typeof(RectTransform), typeof(Image));
            RectTransform alttasRect = alttasGo.GetComponent<RectTransform>();
            alttasRect.SetParent(rootRect, false);
            alttasRect.anchorMin = new Vector2(0.5f, 0.5f);
            alttasRect.anchorMax = new Vector2(0.5f, 0.5f);
            alttasRect.pivot = new Vector2(0.5f, 0.5f);
            alttasRect.localScale = Vector3.one;
            Image alttasImage = alttasGo.GetComponent<Image>();
            alttasImage.sprite = alttasSprite;
            alttasImage.type = Image.Type.Simple;
            alttasImage.color = alttasImage.sprite != null ? Color.white : new Color(0.58f, 0.42f, 0.18f, 1f);
            alttasImage.raycastTarget = false;
            alttasLayout.ApplyTo(alttasRect, alttasImage);

            // PathContainer (node'lar için organizasyon katmanı, alttas'ın üzerinde render olur)
            GameObject pathContainer = new GameObject("PathContainer", typeof(RectTransform));
            RectTransform pathRect = pathContainer.GetComponent<RectTransform>();
            pathRect.SetParent(rootRect, false);
            pathRect.anchorMin = Vector2.zero;
            pathRect.anchorMax = Vector2.one;
            pathRect.pivot = new Vector2(0.5f, 0.5f);
            pathRect.anchoredPosition = Vector2.zero;
            pathRect.sizeDelta = Vector2.zero;
            pathRect.localScale = Vector3.one;

            // Sibling sırasını garantile: bg(0) < alttas(1) < pathContainer(2)
            bgGo.transform.SetSiblingIndex(0);
            alttasGo.transform.SetSiblingIndex(1);
            pathContainer.transform.SetSiblingIndex(2);

            HubPreviewLayoutTuningProfile.LayoutElementTuning bottomPageNavLayout = HubPreviewLayoutTuningProfile.ResolveBottomPageNav();
            GameObject bottomPageNav = BuildBottomPageNav(rootRect, bottomPageNavLayout, controller);

            // Placeholder sayfaları (Görevler, Profil, Mağaza)
            GameObject missionsPlaceholder = BuildTabPlaceholder(rootRect, "MissionsPlaceholder", "Gorevler\nHazirlaniyor...");
            GameObject profilePlaceholder = BuildTabPlaceholder(rootRect, "ProfilePlaceholder", "Profil\nHazirlaniyor...");
            GameObject storePlaceholder = BuildTabPlaceholder(rootRect, "StorePlaceholder", "Magaza\nHazirlaniyor...");

            // 7 level node (pool)
            Sprite nodeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LevelHubNodeSpritePath);

            const int poolSize = 7;
            var nodeRects = new RectTransform[poolSize];
            var numLabels = new TextMeshProUGUI[poolSize];

            for (int i = 0; i < poolSize; i++)
            {
                GameObject nodeGo = new GameObject($"LevelNode_{i}", typeof(RectTransform), typeof(Image), typeof(Button));
                RectTransform nodeRect = nodeGo.GetComponent<RectTransform>();
                nodeRect.SetParent(pathRect, false);
                nodeRect.anchorMin = new Vector2(0.5f, 0.5f);
                nodeRect.anchorMax = new Vector2(0.5f, 0.5f);
                nodeRect.pivot = new Vector2(0.5f, 0.5f);
                nodeRect.anchoredPosition = new Vector2(0f, -300f);
                nodeRect.sizeDelta = LevelHubNodeSize;
                nodeRect.localScale = Vector3.one;

                Image nodeImage = nodeGo.GetComponent<Image>();
                nodeImage.sprite = null;
                nodeImage.type = Image.Type.Simple;
                nodeImage.preserveAspect = false;
                nodeImage.color = LevelHubNodeHostColor;
                nodeImage.raycastTarget = true;

                Button nodeBtn = nodeGo.GetComponent<Button>();
                GameObject visualGo = new GameObject("NodeVisual", typeof(RectTransform), typeof(Image));
                RectTransform visualRect = visualGo.GetComponent<RectTransform>();
                visualRect.SetParent(nodeRect, false);
                visualRect.anchorMin = new Vector2(0.5f, 0.5f);
                visualRect.anchorMax = new Vector2(0.5f, 0.5f);
                visualRect.pivot = new Vector2(0.5f, 0.5f);
                visualRect.anchoredPosition = Vector2.zero;
                visualRect.sizeDelta = LevelHubNodeVisualSize;
                visualRect.localScale = Vector3.one;

                Image visualImage = visualGo.GetComponent<Image>();
                visualImage.sprite = nodeSprite;
                visualImage.type = Image.Type.Simple;
                visualImage.preserveAspect = true;
                visualImage.color = visualImage.sprite != null ? Color.white : new Color(0.52f, 0.36f, 0.16f, 1f);
                visualImage.raycastTarget = false;

                nodeBtn.targetGraphic = visualImage;

                // Seviye numarası label
                GameObject labelGo = new GameObject("LevelNumLabel", typeof(RectTransform), typeof(TextMeshProUGUI));
                RectTransform labelRect = labelGo.GetComponent<RectTransform>();
                labelRect.SetParent(nodeRect, false);
                labelRect.anchorMin = new Vector2(0.5f, 0.5f);
                labelRect.anchorMax = new Vector2(0.5f, 0.5f);
                labelRect.pivot = new Vector2(0.5f, 0.5f);
                labelRect.anchoredPosition = LevelHubNodeLabelPosition;
                labelRect.sizeDelta = LevelHubNodeLabelSize;
                labelRect.localScale = Vector3.one;

                TextMeshProUGUI numLabel = labelGo.GetComponent<TextMeshProUGUI>();
                TMP_FontAsset font = LoadDefaultTmpFont();
                if (font != null) numLabel.font = font;
                numLabel.text = (i + 1).ToString();
                numLabel.fontSize = LevelHubNodeLabelFontSize;
                numLabel.fontStyle = FontStyles.Bold;
                numLabel.alignment = TextAlignmentOptions.Center;
                numLabel.color = Color.white;
                numLabel.raycastTarget = false;

                nodeRects[i] = nodeRect;
                numLabels[i] = numLabel;
            }

            // OYNA alanı (sabit, path'in üzerinde)
            HubPreviewLayoutTuningProfile.LayoutElementTuning oynaLayout = HubPreviewLayoutTuningProfile.ResolveOynaButton();
            Sprite oynaSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LevelHubPreviewOynaButtonSpritePath);

            GameObject oynaGo = new GameObject("OynaBg", typeof(RectTransform), typeof(Image), typeof(Button));
            RectTransform oynaRect = oynaGo.GetComponent<RectTransform>();
            oynaRect.SetParent(rootRect, false);
            oynaRect.anchorMin = new Vector2(0.5f, 0.5f);
            oynaRect.anchorMax = new Vector2(0.5f, 0.5f);
            oynaRect.pivot = new Vector2(0.5f, 0.5f);
            oynaRect.localScale = Vector3.one;

            Image oynaImage = oynaGo.GetComponent<Image>();
            oynaImage.sprite = oynaSprite;
            oynaImage.type = Image.Type.Simple;
            oynaImage.preserveAspect = false;
            oynaImage.color = oynaImage.sprite != null ? Color.white : Color.clear;
            oynaImage.raycastTarget = true;

            Button oynaButton = oynaGo.GetComponent<Button>();
            oynaButton.targetGraphic = oynaImage;

            oynaLayout.ApplyTo(oynaRect, oynaImage);

            TextMeshProUGUI oynaTitle = LayeredText("OynaTitle", oynaRect, "OYNA", new Vector2(0f, 16f), new Vector2(360f, 52f), 38f, Color.white, FontStyles.Bold);
            oynaTitle.raycastTarget = false;

            TextMeshProUGUI oynaSubtitle = LayeredText("OynaSubtitle", oynaRect, "Seviye 1'den basla", new Vector2(0f, -18f), new Vector2(420f, 32f), 20f, new Color(1f, 0.94f, 0.82f), FontStyles.Normal);
            oynaSubtitle.raycastTarget = false;

            bottomPageNav.transform.SetSiblingIndex(3);
            oynaGo.transform.SetSiblingIndex(4);

            // Referans görseli (gizli, sadece editörde doğrulama için)
            Sprite refSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{LevelHubPreviewArt}/levelseçimtekparça.png");
            if (refSprite == null)
                refSprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{LevelHubPreviewArt}/levelsecimtekparca.png");

            GameObject refGo = new GameObject("Reference_Hidden", typeof(RectTransform), typeof(Image));
            RectTransform refRect = refGo.GetComponent<RectTransform>();
            refRect.SetParent(rootRect, false);
            refRect.anchorMin = new Vector2(0.5f, 0.5f);
            refRect.anchorMax = new Vector2(0.5f, 0.5f);
            refRect.pivot = new Vector2(0.5f, 0.5f);
            refRect.anchoredPosition = Vector2.zero;
            refRect.sizeDelta = MainMenuReferenceSize;
            refRect.localScale = Vector3.one;
            Image refImage = refGo.GetComponent<Image>();
            refImage.sprite = refSprite;
            refImage.type = Image.Type.Simple;
            refImage.preserveAspect = false;
            refImage.color = new Color(1f, 1f, 1f, 0.45f);
            refImage.raycastTarget = false;
            refGo.SetActive(false);

            // Controller referanslarını bağla
            var nodeRectObjs = new Object[poolSize];
            var numLabelObjs = new Object[poolSize];
            for (int i = 0; i < poolSize; i++)
            {
                nodeRectObjs[i] = nodeRects[i];
                numLabelObjs[i] = numLabels[i];
            }

            Refs(controller, "levelNodes", nodeRectObjs);
            Refs(controller, "levelNumberLabels", numLabelObjs);
            Ref(controller, "oynaSubtitleLabel", oynaSubtitle);
            Ref(controller, "alttasRoot", alttasGo);
            Ref(controller, "pathContainerRoot", pathContainer);
            Ref(controller, "oynaBgRoot", oynaGo);
            Ref(controller, "missionsPlaceholder", missionsPlaceholder);
            Ref(controller, "profilePlaceholder", profilePlaceholder);
            Ref(controller, "storePlaceholder", storePlaceholder);
            UnityEventTools.AddPersistentListener(oynaButton.onClick, controller.OnOynaPressed);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildTabPlaceholder(RectTransform parent, string name, string displayText)
        {
            GameObject placeholder = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform phRect = placeholder.GetComponent<RectTransform>();
            phRect.SetParent(parent, false);
            phRect.anchorMin = Vector2.zero;
            phRect.anchorMax = Vector2.one;
            phRect.offsetMin = Vector2.zero;
            phRect.offsetMax = Vector2.zero;
            phRect.localScale = Vector3.one;
            Image phImage = placeholder.GetComponent<Image>();
            phImage.color = new Color(0.04f, 0.06f, 0.08f, 0.96f);
            phImage.raycastTarget = true;

            TextMeshProUGUI label = LayeredText(name + "_Text", phRect, displayText, Vector2.zero, new Vector2(800f, 150f), 48f, Color.white, FontStyles.Bold);
            label.enableWordWrapping = true;

            placeholder.SetActive(false);
            return placeholder;
        }

        private static GameObject BuildBottomPageNav(Transform parent, HubPreviewLayoutTuningProfile.LayoutElementTuning tuning, LevelHubPreviewController controller = null)
        {
            GameObject root = new GameObject("BottomPageNav", typeof(RectTransform), typeof(CanvasGroup));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.SetParent(parent, false);
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.localScale = Vector3.one;

            CanvasGroup group = root.GetComponent<CanvasGroup>();
            group.alpha = 1f;
            group.interactable = true;
            group.blocksRaycasts = true;

            GameObject frameGo = new GameObject("FrameImage", typeof(RectTransform), typeof(Image));
            RectTransform frameRect = frameGo.GetComponent<RectTransform>();
            frameRect.SetParent(rootRect, false);
            frameRect.anchorMin = Vector2.zero;
            frameRect.anchorMax = Vector2.one;
            frameRect.pivot = new Vector2(0.5f, 0.5f);
            frameRect.anchoredPosition = Vector2.zero;
            frameRect.sizeDelta = Vector2.zero;
            frameRect.localScale = Vector3.one;

            Image frameImage = frameGo.GetComponent<Image>();
            Sprite frameSprite = AssetDatabase.LoadAssetAtPath<Sprite>(LevelHubPreviewBottomNavSpritePath);
            frameImage.sprite = frameSprite != null ? frameSprite : Builtin();
            frameImage.type = Image.Type.Simple;
            frameImage.preserveAspect = true;
            frameImage.color = frameSprite != null ? Color.white : Color.clear;
            frameImage.raycastTarget = false;

            if (tuning != null)
            {
                tuning.ApplyTo(rootRect, frameImage);
            }
            else
            {
                rootRect.anchoredPosition = Vector2.zero;
                rootRect.sizeDelta = LevelHubPreviewBottomNavMaxSize;
            }

            frameGo.transform.SetSiblingIndex(0);

            // Hitbox butonları — her biri nav bar'ın 1/4'ünü kaplar
            GameObject hitboxes = new GameObject("Hitboxes", typeof(RectTransform));
            RectTransform hrt = hitboxes.GetComponent<RectTransform>();
            hrt.SetParent(rootRect, false);
            hrt.anchorMin = Vector2.zero;
            hrt.anchorMax = Vector2.one;
            hrt.offsetMin = Vector2.zero;
            hrt.offsetMax = Vector2.zero;
            hrt.localScale = Vector3.one;

            string[] tabNames = { "Journey", "Missions", "Profile", "Store" };
            for (int i = 0; i < 4; i++)
            {
                GameObject btnObj = new GameObject($"Btn_{tabNames[i]}", typeof(RectTransform), typeof(Image), typeof(Button));
                RectTransform brt = btnObj.GetComponent<RectTransform>();
                brt.SetParent(hrt, false);
                brt.anchorMin = new Vector2(i * 0.25f, 0f);
                brt.anchorMax = new Vector2((i + 1) * 0.25f, 1f);
                brt.offsetMin = Vector2.zero;
                brt.offsetMax = Vector2.zero;
                Image bImg = btnObj.GetComponent<Image>();
                bImg.color = new Color(0f, 0f, 0f, 0f);
                bImg.raycastTarget = true;

                if (controller != null)
                {
                    Button btn = btnObj.GetComponent<Button>();
                    if (i == 0) UnityEventTools.AddPersistentListener(btn.onClick, controller.OpenJourneyTab);
                    else if (i == 1) UnityEventTools.AddPersistentListener(btn.onClick, controller.OpenMissionsTab);
                    else if (i == 2) UnityEventTools.AddPersistentListener(btn.onClick, controller.OpenProfileTab);
                    else if (i == 3) UnityEventTools.AddPersistentListener(btn.onClick, controller.OpenStoreTab);
                }
            }

            return root;
        }

        private static void BuildHubPreviewScene(bool forceReset)
        {
            if (!forceReset && AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneHubPreview}.unity") != null)
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Cam("Hub Preview Camera", new Color(0.02f, 0.03f, 0.06f));
            EventSystem();
            Canvas canvas = Canvas("HubPreviewCanvas");
            RectTransform canvasRect = canvas.GetComponent<RectTransform>();
            if (canvasRect != null)
            {
                canvasRect.anchorMin = Vector2.zero;
                canvasRect.anchorMax = Vector2.one;
                canvasRect.pivot = new Vector2(0.5f, 0.5f);
                canvasRect.anchoredPosition = Vector2.zero;
                canvasRect.sizeDelta = Vector2.zero;
                canvasRect.localRotation = Quaternion.identity;
                canvasRect.localScale = Vector3.one;
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                scaler.referenceResolution = MainMenuReferenceSize;
                scaler.matchWidthOrHeight = 1f;
            }

            Label("HubPreviewSandboxTitle", canvas.transform, "HubPreview Sandbox", 42f, new Vector2(0.5f, 0.56f), new Vector2(760f, 80f), new Color(1f, 0.84f, 0.58f));
            TextMeshProUGUI body = Label("HubPreviewSandboxBody", canvas.transform, "Yeni sayfa tasarimlari once burada denenir. Onaylanan tasarim ayrica runtime sahnesine aktarilir.", 24f, new Vector2(0.5f, 0.49f), new Vector2(820f, 110f), new Color(0.88f, 0.91f, 0.96f));
            body.enableWordWrapping = true;

            EditorSceneManager.SaveScene(scene, $"{Scenes}/{GameConstants.SceneHubPreview}.unity");
        }

        private static GameObject BuildMainMenuPngPreviewLayer(Transform canvas)
        {
            GameObject prefab = BuildMainMenuPngPreviewPrefab(true);
            GameObject instance = PrefabUtility.InstantiatePrefab(prefab, canvas) as GameObject;
            if (instance == null)
            {
                return null;
            }

            instance.name = "MainMenuPngPreviewRoot";
            RectTransform rect = instance.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = new Vector2(0.5f, 0.5f);
                rect.anchorMax = new Vector2(0.5f, 0.5f);
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = MainMenuReferenceSize;
                rect.localRotation = Quaternion.Euler(0f, 0f, HubPreviewDisplayRotationZ);
                rect.localScale = Vector3.one;
            }

            return instance;
        }

        private static GameObject BuildMainMenuPngPreviewPrefab(bool forceReset)
        {
            string path = $"{Prefabs}/MainMenuPngPreview.prefab";
            if (!forceReset)
            {
                GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (existing != null)
                {
                    return existing;
                }
            }

            GameObject root = new GameObject("MainMenuPngPreviewRoot", typeof(RectTransform), typeof(CanvasGroup));
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = new Vector2(0.5f, 0.5f);
            rootRect.anchorMax = new Vector2(0.5f, 0.5f);
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.sizeDelta = MainMenuReferenceSize;
            rootRect.localRotation = Quaternion.Euler(0f, 0f, HubPreviewDisplayRotationZ);
            rootRect.localScale = Vector3.one;
            CanvasGroup group = root.GetComponent<CanvasGroup>();
            group.interactable = true;
            group.blocksRaycasts = true;
            group.alpha = 1f;

            Image reference = LayeredAssetImage("Reference_MainMenu_Full_Hidden", rootRect, $"{MainMenuSourceArt}/mainmenu_reference.png", Vector2.zero, MainMenuReferenceSize, 0f);
            reference.gameObject.SetActive(false);

            LayeredAssetImage("Background", rootRect, $"{MainMenuSourceArt}/arkaplanmainmenu.png", Vector2.zero, MainMenuBackgroundCoverSize, 1f);
            LayeredAssetImage("Logo", rootRect, $"{MainMenuCroppedArt}/logo_crop.png", new Vector2(0f, 540f), new Vector2(640f, 260f), 1f);
            LayeredAssetImage("LanguageSelect", rootRect, $"{MainMenuCroppedArt}/Dilsecimi_crop.png", new Vector2(0f, 380f), new Vector2(440f, 70f), 1f);
            CanvasGroup[] languageHighlights = BuildLanguageHighlightOverlays(rootRect);
            HubPreviewLayoutTuningProfile.LayoutElementTuning rotatorLayout = HubPreviewLayoutTuningProfile.ResolveMainMenuRotator();
            LayeredAssetImage("Rotator", rootRect, $"{MainMenuCroppedArt}/rotator_crop.png", rotatorLayout.anchoredPosition, rotatorLayout.GetSafeSizeDelta(), 1f, rotatorLayout.GetSafePreserveAspect());
            Image playButtonImage = LayeredAssetImage("PlayButton", rootRect, $"{MainMenuCroppedArt}/playbutton_crop.png", new Vector2(0f, -430f), new Vector2(520f, 170f), 1f);
            TextMeshProUGUI playTitle = LayeredText("PlayButton_Title", rootRect, "OYNA", new Vector2(0f, -416f), new Vector2(380f, 50f), 36, Color.white, FontStyles.Bold);
            TextMeshProUGUI playSubtitle = LayeredText("PlayButton_Subtitle", rootRect, "Maceraya Basla", new Vector2(0f, -448f), new Vector2(360f, 30f), 19, new Color(1f, 0.94f, 0.8f), FontStyles.Bold);
            LayeredText("StartLevel_Label", rootRect, "Seviye 1'den basla", new Vector2(0f, -550f), new Vector2(420f, 32f), 20, Color.white, FontStyles.Bold);

            MainMenuLanguagePreviewHighlighter languageHighlighter = root.AddComponent<MainMenuLanguagePreviewHighlighter>();
            Refs(languageHighlighter, "languageHighlights", languageHighlights);
            BuildLanguageHitboxes(rootRect, languageHighlighter);
            BuildPlayButtonHitbox(rootRect, playButtonImage, playTitle, playSubtitle);

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static void BuildLanguageHitboxes(RectTransform parent, MainMenuLanguagePreviewHighlighter highlighter)
        {
            string[] labels = { "TR", "EN", "DE", "ES" };
            string[] codes = { "tr", "en", "de", "es" };
            float[] xPositions = { -165f, -55f, 55f, 165f };

            for (int i = 0; i < labels.Length; i++)
            {
                Image hitbox = LayeredColorImage($"LanguageButton_{labels[i]}", parent, new Vector2(xPositions[i], 380f), new Vector2(104f, 62f), new Color(1f, 1f, 1f, 0f));
                hitbox.raycastTarget = true;
                MainMenuPreviewLanguageButton languageButton = hitbox.gameObject.AddComponent<MainMenuPreviewLanguageButton>();
                languageButton.Configure(highlighter, codes[i]);
                hitbox.gameObject.AddComponent<ButtonPressEffect>();
            }
        }

        private static void BuildPlayButtonHitbox(RectTransform parent, Image playButtonImage, TextMeshProUGUI playTitle, TextMeshProUGUI playSubtitle)
        {
            Image hitbox = LayeredColorImage("PlayButton_Hitbox", parent, new Vector2(0f, -430f), new Vector2(520f, 170f), new Color(1f, 1f, 1f, 0f));
            hitbox.raycastTarget = true;
            MainMenuPreviewPressEffect effect = hitbox.gameObject.AddComponent<MainMenuPreviewPressEffect>();
            effect.Configure(
                new[] { playButtonImage.rectTransform, playTitle.rectTransform, playSubtitle.rectTransform },
                new Graphic[] { playButtonImage, playTitle, playSubtitle });
        }

        private static CanvasGroup[] BuildLanguageHighlightOverlays(RectTransform parent)
        {
            string[] labels = { "TR", "EN", "DE", "ES" };
            float[] xPositions = { -165f, -55f, 55f, 165f };
            CanvasGroup[] highlights = new CanvasGroup[labels.Length];

            for (int i = 0; i < labels.Length; i++)
            {
                GameObject groupObject = new GameObject($"LanguageHighlight_{labels[i]}", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
                RectTransform groupRect = groupObject.GetComponent<RectTransform>();
                groupRect.SetParent(parent, false);
                groupRect.anchorMin = new Vector2(0.5f, 0.5f);
                groupRect.anchorMax = new Vector2(0.5f, 0.5f);
                groupRect.pivot = new Vector2(0.5f, 0.5f);
                groupRect.anchoredPosition = new Vector2(xPositions[i], 380f);
                groupRect.sizeDelta = new Vector2(98f, 54f);
                groupRect.localScale = Vector3.one;

                CanvasGroup highlightGroup = groupObject.GetComponent<CanvasGroup>();
                highlightGroup.alpha = 0f;
                highlightGroup.interactable = false;
                highlightGroup.blocksRaycasts = false;

                Image glow = groupObject.GetComponent<Image>();
                glow.sprite = Builtin();
                glow.type = Image.Type.Sliced;
                glow.color = new Color(1f, 0.72f, 0.18f, 0.26f);
                glow.raycastTarget = false;

                Image rim = LayeredColorImage($"LanguageHighlight_{labels[i]}_Rim", groupRect, Vector2.zero, new Vector2(88f, 42f), new Color(1f, 0.91f, 0.46f, 0.34f));
                rim.type = Image.Type.Sliced;

                Image underline = LayeredColorImage($"LanguageHighlight_{labels[i]}_Underline", groupRect, new Vector2(0f, -22f), new Vector2(58f, 5f), new Color(1f, 0.78f, 0.20f, 0.92f));
                underline.type = Image.Type.Sliced;

                Image topShine = LayeredColorImage($"LanguageHighlight_{labels[i]}_TopShine", groupRect, new Vector2(0f, 17f), new Vector2(62f, 4f), new Color(1f, 0.96f, 0.70f, 0.50f));
                topShine.type = Image.Type.Sliced;

                groupObject.SetActive(false);
                highlights[i] = highlightGroup;
            }

            return highlights;
        }

        private static Image LayeredColorImage(string name, RectTransform parent, Vector2 anchoredPosition, Vector2 size, Color color)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;

            Image image = go.GetComponent<Image>();
            image.sprite = Builtin();
            image.type = Image.Type.Simple;
            image.color = color;
            image.raycastTarget = false;
            return image;
        }

        private static Image LayeredAssetImage(string name, RectTransform parent, string assetPath, Vector2 anchoredPosition, Vector2 size, float alpha, bool preserveAspect = false)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
            rect.localScale = Vector3.one;

            Image image = go.GetComponent<Image>();
            image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(assetPath);
            image.type = Image.Type.Simple;
            image.preserveAspect = preserveAspect;
            image.color = image.sprite != null ? new Color(1f, 1f, 1f, Mathf.Clamp01(alpha)) : new Color(1f, 0f, 0f, 0.35f);
            image.raycastTarget = false;
            return image;
        }

        private static Vector2 FitWithin(Vector2 size, Vector2 maxSize)
        {
            if (size.x <= 0f || size.y <= 0f)
            {
                return maxSize;
            }

            float scale = Mathf.Min(maxSize.x / size.x, maxSize.y / size.y, 1f);
            return new Vector2(size.x * scale, size.y * scale);
        }

        private static TextMeshProUGUI LayeredText(string name, RectTransform parent, string text, Vector2 anchoredPosition, Vector2 size, float fontSize, Color color, FontStyles style)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            RectTransform rect = go.GetComponent<RectTransform>();
            rect.SetParent(parent, false);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TextMeshProUGUI label = go.GetComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
            return label;
        }

        private static void BuildGameplayScene(GameObject pinPrefab, GameObject keyPrefab, bool forceReset)
        {
            if (!forceReset && AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneGameplay}.unity") != null)
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            GameObject cameraObject = Cam("Gameplay Camera", new Color(0.05f, 0.04f, 0.07f));
            EventSystem();
            new GameObject("GameplayBootstrap").AddComponent<SceneBootstrap>();

            GameObject backgroundMatte = Sprite("BackgroundMatte", Builtin(), new Vector3(0f, 0.0f, 8f), new Vector3(8.6f, 12.4f, 1f), new Color(0.11f, 0.08f, 0.08f, 1f));
            backgroundMatte.GetComponent<SpriteRenderer>().sortingOrder = -30;

            GameObject backgroundGlow = Sprite("BackgroundGlow", BuiltinRound(), new Vector3(0f, -1.00f, 6f), new Vector3(8.4f, 8.4f, 1f), new Color(1f, 0.58f, 0.20f, 0.04f));
            backgroundGlow.GetComponent<SpriteRenderer>().sortingOrder = -20;
            backgroundGlow.AddComponent<PulseSprite>();

            GameObject leftAura = Sprite("LeftAura", BuiltinRound(), new Vector3(-3.2f, -2.2f, 2f), new Vector3(1.7f, 1.7f, 1f), new Color(1f, 0.48f, 0.16f, 0.22f));
            leftAura.GetComponent<SpriteRenderer>().sortingOrder = -8;
            leftAura.AddComponent<PulseSprite>();

            GameObject rightAura = Sprite("RightAura", BuiltinRound(), new Vector3(3.2f, -2.2f, 2f), new Vector3(1.55f, 1.55f, 1f), new Color(1f, 0.48f, 0.16f, 0.18f));
            rightAura.GetComponent<SpriteRenderer>().sortingOrder = -8;
            rightAura.AddComponent<PulseSprite>();

            GameObject leftPillar = Sprite("LeftPillar", Builtin(), new Vector3(-3.35f, -0.45f, 1f), new Vector3(0.34f, 2.55f, 1f), new Color(0.25f, 0.23f, 0.26f, 0.94f));
            leftPillar.GetComponent<SpriteRenderer>().sortingOrder = -4;
            GameObject rightPillar = Sprite("RightPillar", Builtin(), new Vector3(3.35f, -0.45f, 1f), new Vector3(0.34f, 2.55f, 1f), new Color(0.25f, 0.23f, 0.26f, 0.94f));
            rightPillar.GetComponent<SpriteRenderer>().sortingOrder = -4;

            GameObject flightLane = Sprite("FlightLane", BuiltinRound(), new Vector3(0f, -0.55f, 0f), new Vector3(0.08f, 5.0f, 1f), new Color(1f, 0.56f, 0.18f, 0.12f));
            flightLane.GetComponent<SpriteRenderer>().sortingOrder = -2;
            GameObject launcher = Sprite("Launcher", new Vector3(0f, -2.85f, 0f), new Vector3(1.8f, 0.40f, 1f), new Color(0.24f, 0.24f, 0.28f));
            launcher.GetComponent<SpriteRenderer>().sortingOrder = 6;
            GameObject spawn = new GameObject("PinSpawnPoint");
            spawn.transform.SetParent(launcher.transform, false);
            spawn.transform.localPosition = new Vector3(0f, 0.38f, 0f);
            GameObject rotator = new GameObject("Rotator");
            rotator.transform.position = new Vector3(0f, 1.08f, 0f);
            TargetRotator rotatorComp = rotator.AddComponent<TargetRotator>();
            GameObject rotatorVisual = new GameObject("RotatorVisual");
            rotatorVisual.transform.SetParent(rotator.transform, false);
            GameObject anchorRoot = new GameObject("AnchorRoot");
            anchorRoot.transform.SetParent(rotator.transform, false);
            GameObject outerHalo = Sprite("OuterHalo", BuiltinRound(), Vector3.zero, new Vector3(3.45f, 3.45f, 1f), new Color(1f, 0.66f, 0.24f, 0.06f));
            outerHalo.transform.SetParent(rotatorVisual.transform, false);
            outerHalo.GetComponent<SpriteRenderer>().sortingOrder = 0;
            GameObject orbitRing = Sprite("OrbitRing", BuiltinRound(), Vector3.zero, new Vector3(3.10f, 3.10f, 1f), new Color(0.90f, 0.74f, 0.50f, 0.16f));
            orbitRing.transform.SetParent(rotatorVisual.transform, false);
            orbitRing.GetComponent<SpriteRenderer>().sortingOrder = 1;
            orbitRing.AddComponent<PulseSprite>();
            GameObject diskShadow = Sprite("DiskShadow", BuiltinRound(), new Vector3(0f, -0.04f, 0f), new Vector3(3.04f, 3.04f, 1f), new Color(0f, 0f, 0f, 0.30f));
            diskShadow.transform.SetParent(rotatorVisual.transform, false);
            diskShadow.GetComponent<SpriteRenderer>().sortingOrder = 2;
            GameObject outerRim = Sprite("OuterRim", BuiltinRound(), Vector3.zero, new Vector3(2.92f, 2.92f, 1f), new Color(0.24f, 0.16f, 0.11f, 1f));
            outerRim.transform.SetParent(rotatorVisual.transform, false);
            outerRim.GetComponent<SpriteRenderer>().sortingOrder = 3;
            GameObject outerRimInset = Sprite("OuterRimInset", BuiltinRound(), Vector3.zero, new Vector3(2.70f, 2.70f, 1f), new Color(0.46f, 0.31f, 0.18f, 0.98f));
            outerRimInset.transform.SetParent(rotatorVisual.transform, false);
            outerRimInset.GetComponent<SpriteRenderer>().sortingOrder = 4;
            GameObject runeBandOuter = Sprite("RuneBandOuter", BuiltinRound(), Vector3.zero, new Vector3(2.44f, 2.44f, 1f), new Color(0.20f, 0.14f, 0.10f, 0.98f));
            runeBandOuter.transform.SetParent(rotatorVisual.transform, false);
            runeBandOuter.GetComponent<SpriteRenderer>().sortingOrder = 5;
            GameObject runeBandInner = Sprite("RuneBandInner", BuiltinRound(), Vector3.zero, new Vector3(2.14f, 2.14f, 1f), new Color(0.58f, 0.38f, 0.21f, 0.94f));
            runeBandInner.transform.SetParent(rotatorVisual.transform, false);
            runeBandInner.GetComponent<SpriteRenderer>().sortingOrder = 6;
            GameObject runeBandGlow = Sprite("RuneBandGlow", BuiltinRound(), Vector3.zero, new Vector3(2.24f, 2.24f, 1f), new Color(1f, 0.62f, 0.24f, 0.14f));
            runeBandGlow.transform.SetParent(rotatorVisual.transform, false);
            runeBandGlow.GetComponent<SpriteRenderer>().sortingOrder = 7;
            runeBandGlow.AddComponent<PulseSprite>();
            GameObject rotatorArt = Sprite("RotatorArt", BuiltinRound(), Vector3.zero, new Vector3(2.56f, 2.56f, 1f), new Color(0.55f, 0.34f, 0.17f, 0.22f));
            rotatorArt.transform.SetParent(rotatorVisual.transform, false);
            rotatorArt.GetComponent<SpriteRenderer>().sortingOrder = 7;
            GameObject rotatorCore = Sprite("RotatorCore", BuiltinRound(), Vector3.zero, new Vector3(1.66f, 1.66f, 1f), new Color(0.18f, 0.13f, 0.10f, 1f));
            rotatorCore.transform.SetParent(rotatorVisual.transform, false);
            rotatorCore.GetComponent<SpriteRenderer>().sortingOrder = 8;
            GameObject coreInset = Sprite("CoreInset", BuiltinRound(), Vector3.zero, new Vector3(1.18f, 1.18f, 1f), new Color(0.63f, 0.41f, 0.22f, 0.92f));
            coreInset.transform.SetParent(rotatorVisual.transform, false);
            coreInset.GetComponent<SpriteRenderer>().sortingOrder = 9;
            GameObject coreEmblem = Sprite("CoreEmblem", Builtin(), Vector3.zero, new Vector3(0.28f, 0.92f, 1f), new Color(0.16f, 0.10f, 0.04f, 0.72f));
            coreEmblem.transform.SetParent(rotatorVisual.transform, false);
            coreEmblem.GetComponent<SpriteRenderer>().sortingOrder = 10;
            Slot[] slots = new Slot[8];
            for (int i = 0; i < slots.Length; i++)
            {
                float a = i * Mathf.PI * 2f / slots.Length;
                GameObject go = new GameObject($"Slot_{i}");
                go.transform.SetParent(anchorRoot.transform, false);
                go.transform.localPosition = new Vector3(Mathf.Sin(a) * 1.02f, Mathf.Cos(a) * 1.02f, 0f);
                go.transform.localRotation = Quaternion.Euler(0f, 0f, -Mathf.Rad2Deg * a);

                GameObject plaqueShadow = Sprite("PlaqueShadow", BuiltinRound(), new Vector3(0f, -0.01f, 0f), new Vector3(0.30f, 0.18f, 1f), new Color(0f, 0f, 0f, 0.16f));
                plaqueShadow.transform.SetParent(go.transform, false);
                plaqueShadow.GetComponent<SpriteRenderer>().sortingOrder = 11;

                GameObject plaqueRing = Sprite("PlaqueRing", BuiltinRound(), Vector3.zero, new Vector3(0.30f, 0.18f, 1f), new Color(0.23f, 0.15f, 0.10f, 0.96f));
                plaqueRing.transform.SetParent(go.transform, false);
                plaqueRing.GetComponent<SpriteRenderer>().sortingOrder = 12;

                GameObject plaqueCore = Sprite("PlaqueCore", BuiltinRound(), Vector3.zero, new Vector3(0.22f, 0.11f, 1f), new Color(0.82f, 0.62f, 0.34f, 0.92f));
                plaqueCore.transform.SetParent(go.transform, false);
                SpriteRenderer slotRenderer = plaqueCore.GetComponent<SpriteRenderer>();
                slotRenderer.sortingOrder = 13;

                GameObject runeGlyph = Sprite("RuneGlyph", Builtin(), Vector3.zero, new Vector3(0.02f, 0.08f, 1f), new Color(0.22f, 0.12f, 0.04f, 0.52f));
                runeGlyph.transform.SetParent(go.transform, false);
                runeGlyph.GetComponent<SpriteRenderer>().sortingOrder = 14;

                CircleCollider2D c = go.AddComponent<CircleCollider2D>();
                c.isTrigger = true;
                c.radius = 0.09f;
                Slot slot = go.AddComponent<Slot>();
                GameObject glow = Sprite("Glow", BuiltinRound(), Vector3.zero, new Vector3(0.34f, 0.22f, 1f), new Color(1f, 0.64f, 0.22f, 0.82f));
                glow.transform.SetParent(go.transform, false);
                SpriteRenderer glowRenderer = glow.GetComponent<SpriteRenderer>();
                glowRenderer.enabled = false;
                glowRenderer.sortingOrder = 15;
                glow.AddComponent<PulseSprite>();
                Ref(slot, "glowRenderer", glowRenderer);
                Ref(slot, "bodyRenderer", slotRenderer);
                slots[i] = slot;
            }

            GameObject systems = new GameObject("GameplaySystems");
            InputBuffer inputBuffer = systems.AddComponent<InputBuffer>();
            SlotManager slotManager = systems.AddComponent<SlotManager>();
            FireGate fireGate = systems.AddComponent<FireGate>();
            LevelFlowController flow = systems.AddComponent<LevelFlowController>();
            HitEvaluator evaluator = systems.AddComponent<HitEvaluator>();
            SessionManager session = systems.AddComponent<SessionManager>();
            GameManager manager = systems.AddComponent<GameManager>();
            PinLauncher launcherComp = systems.AddComponent<PinLauncher>();
            Refs(slotManager, "slots", slots);
            Ref(slotManager, "launcherTransform", launcher.transform);
            Ref(fireGate, "slotManager", slotManager);
            Ref(flow, "slotManager", slotManager);
            Ref(flow, "targetRotator", rotatorComp);
            Ref(flow, "inputBuffer", inputBuffer);
            Ref(evaluator, "slotManager", slotManager);
            Ref(session, "levelFlow", flow);
            Ref(manager, "levelFlow", flow);
            Ref(manager, "hitEvaluator", evaluator);
            Ref(manager, "sessionManager", session);
            Ref(launcherComp, "pinPrefab", pinPrefab.GetComponent<Pin>());
            Ref(launcherComp, "pinSpawnPoint", spawn.transform);
            Ref(launcherComp, "fireGate", fireGate);
            Ref(launcherComp, "inputBuffer", inputBuffer);

            Canvas canvas = Canvas("GameplayCanvas");
            Image topBar = Panel("TopBar", canvas.transform, new Vector2(0.5f, 0.96f), new Vector2(980, 80), new Color(0.16f, 0.13f, 0.11f, 0.95f));
            TextMeshProUGUI level = Label("Level", topBar.transform, "Level 1", 28, new Vector2(0.2f, 0.5f), new Vector2(260, 50), new Color(1f, 0.90f, 0.70f));
            TextMeshProUGUI currency = Label("Currency", topBar.transform, "0", 28, new Vector2(0.82f, 0.5f), new Vector2(220, 50), Color.white);
            Image questionPanel = Panel("QuestionPanel", canvas.transform, new Vector2(0.5f, 0.79f), new Vector2(900, 220), new Color(0.14f, 0.14f, 0.16f, 0.95f));
            questionPanel.rectTransform.anchoredPosition = new Vector2(0f, 132f);
            TextMeshProUGUI question = Label("Question", questionPanel.transform, "Question", 30, new Vector2(0.5f, 0.66f), new Vector2(760, 80), Color.white);
            TextMeshProUGUI answer = Label("Answer", questionPanel.transform, "_ _ _ _", 36, new Vector2(0.5f, 0.28f), new Vector2(760, 70), new Color(1f, 0.88f, 0.66f));
            TextMeshProUGUI debugAnswer = Label("DebugAnswer", questionPanel.transform, "Test Cevap: -", 20, new Vector2(0.5f, 0.08f), new Vector2(760, 36), new Color(0.82f, 0.86f, 0.92f));
            TextMeshProUGUI hearts = Label("Hearts", questionPanel.transform, "Hearts: 3", 24, new Vector2(0.80f, 0.88f), new Vector2(220, 40), new Color(1f, 0.72f, 0.56f));
            TextMeshProUGUI targetHint = Label("TargetHint", canvas.transform, "Target slot glowing", 24, new Vector2(0.5f, 0.33f), new Vector2(520, 40), new Color(1f, 0.75f, 0.50f));
            Image bottom = Panel("Bottom", canvas.transform, new Vector2(0.5f, 0.10f), new Vector2(1080, 362), new Color(0.12f, 0.12f, 0.13f, 0.90f));
            bottom.rectTransform.anchoredPosition = new Vector2(0f, -18f);
            Image keyboardSkinFrame = Panel("KeyboardSkinFrame", bottom.transform, new Vector2(0.5f, 0.5f), new Vector2(1000, 320), new Color(0.26f, 0.20f, 0.16f, 0.35f));
            keyboardSkinFrame.rectTransform.anchoredPosition = new Vector2(0f, -8f);
            TextMeshProUGUI swipeHint = Label("SwipeHint", bottom.transform, "Tap a letter, then swipe up", 22, new Vector2(0.5f, 1.24f), new Vector2(520, 36), new Color(0.95f, 0.84f, 0.72f));
            GameObject gridRoot = new GameObject("KeyboardGrid", typeof(RectTransform), typeof(GridLayoutGroup));
            RectTransform gridRect = Rt(gridRoot, bottom.transform, new Vector2(0.5f, 0.5f), new Vector2(964, 292));
            gridRect.anchoredPosition = new Vector2(0f, -2f);
            GridLayoutGroup grid = gridRoot.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = 10;
            grid.cellSize = new Vector2(72, 82);
            grid.spacing = new Vector2(2, 6);
            grid.padding = new RectOffset(10, 10, 8, 8);
            Button menuOpen = Button("MenuOpen", bottom.transform, "Menu", new Vector2(0.14f, 1f), new Vector2(156, 52), new Color(0.22f, 0.24f, 0.30f));
            Button storeOpen = Button("StoreOpen", bottom.transform, "Store", new Vector2(0.86f, 1f), new Vector2(156, 52), new Color(0.34f, 0.20f, 0.18f));
            menuOpen.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 34f);
            storeOpen.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, 34f);
            GameObject ui = new GameObject("UIPresenters");
            ui.transform.SetParent(canvas.transform, false);
            ThemeRuntimeController theme = ui.AddComponent<ThemeRuntimeController>();
            GameplayHudPresenter hud = ui.AddComponent<GameplayHudPresenter>();
            KeyboardPresenter keyboard = ui.AddComponent<KeyboardPresenter>();
            InfoCardPresenter info = ui.AddComponent<InfoCardPresenter>();
            ResultPresenter result = ui.AddComponent<ResultPresenter>();
            ImpactFeedbackController impactFeedback = ui.AddComponent<ImpactFeedbackController>();
            ui.AddComponent<FailModalPresenter>();
            ui.AddComponent<RotatorPlaquePresenter>();
            ui.AddComponent<GameplayPausePresenter>();
            GameSceneNavigationButtons sceneButtons = ui.AddComponent<GameSceneNavigationButtons>();
            GameObject tuningObject = new GameObject("GameplayTuning");
            GameplaySceneTuner tuner = tuningObject.AddComponent<GameplaySceneTuner>();
            Ref(theme, "topBar", topBar);
            Ref(theme, "questionPanel", questionPanel);
            Ref(theme, "bottomBar", bottom);
            Ref(theme, "keyboardSkinFrame", keyboardSkinFrame);
            Ref(theme, "levelLabel", level);
            Ref(theme, "currencyLabel", currency);
            Ref(theme, "gameplayCamera", cameraObject.GetComponent<Camera>());
            Ref(theme, "backgroundMatte", backgroundMatte.GetComponent<SpriteRenderer>());
            Ref(theme, "backgroundGlow", backgroundGlow.GetComponent<SpriteRenderer>());
            Ref(theme, "ambienceLeft", leftAura.GetComponent<SpriteRenderer>());
            Ref(theme, "ambienceRight", rightAura.GetComponent<SpriteRenderer>());
            Ref(theme, "orbitRing", orbitRing.GetComponent<SpriteRenderer>());
            Ref(theme, "rotatorArt", rotatorArt.GetComponent<SpriteRenderer>());
            Ref(theme, "rotatorCore", rotatorCore.GetComponent<SpriteRenderer>());
            Ref(theme, "launcherBody", launcher.GetComponent<SpriteRenderer>());
            Ref(theme, "flightLane", flightLane.GetComponent<SpriteRenderer>());
            Ref(theme, "targetHintLabel", targetHint);
            Ref(impactFeedback, "gameplayCamera", cameraObject.GetComponent<Camera>());
            Ref(hud, "questionLabel", question);
            Ref(hud, "answerLabel", answer);
            Ref(hud, "debugAnswerLabel", debugAnswer);
            Ref(hud, "heartsLabel", hearts);
            Ref(hud, "targetHintLabel", targetHint);
            Ref(keyboard, "container", gridRoot.transform);
            Ref(keyboard, "keyPrefab", keyPrefab.GetComponent<Button>());
            String(keyboard, "languageCode", "tr");
            GameObject infoRoot = Panel("InfoCard", canvas.transform, new Vector2(0.5f, 0.45f), new Vector2(760, 320), new Color(0.11f, 0.11f, 0.14f, 0.96f)).gameObject;
            TextMeshProUGUI infoTitle = Label("InfoTitle", infoRoot.transform, "Info", 34, new Vector2(0.5f, 0.80f), new Vector2(620, 60), new Color(1f, 0.85f, 0.60f));
            TextMeshProUGUI infoBody = Label("InfoBody", infoRoot.transform, "Body", 24, new Vector2(0.5f, 0.52f), new Vector2(620, 140), Color.white);
            Button infoClose = Button("InfoClose", infoRoot.transform, "Continue", new Vector2(0.5f, 0.14f), new Vector2(220, 62), new Color(0.84f, 0.43f, 0.16f));
            infoRoot.SetActive(false);
            Ref(info, "root", infoRoot);
            Ref(info, "titleLabel", infoTitle);
            Ref(info, "bodyLabel", infoBody);
            UnityEventTools.AddPersistentListener(infoClose.onClick, info.Hide);
            GameObject resultRoot = Panel("Result", canvas.transform, new Vector2(0.5f, 0.46f), new Vector2(760, 320), new Color(0.12f, 0.12f, 0.15f, 0.98f)).gameObject;
            TextMeshProUGUI resultLabel = Label("ResultLabel", resultRoot.transform, "Level Complete", 30, new Vector2(0.5f, 0.68f), new Vector2(620, 190), new Color(1f, 0.85f, 0.60f));
            resultLabel.enableWordWrapping = true;
            resultLabel.enableAutoSizing = true;
            resultLabel.fontSizeMin = 18f;
            resultLabel.fontSizeMax = 31f;
            resultLabel.lineSpacing = -8f;
            Button next = Button("Next", resultRoot.transform, "Next", new Vector2(0.35f, 0.14f), new Vector2(172, 52), new Color(0.84f, 0.43f, 0.16f));
            Button menu = Button("Menu", resultRoot.transform, "Menu", new Vector2(0.65f, 0.14f), new Vector2(172, 52), new Color(0.22f, 0.24f, 0.30f));
            resultRoot.SetActive(false);
            Ref(result, "root", resultRoot);
            Ref(result, "resultLabel", resultLabel);
            UnityEventTools.AddPersistentListener(next.onClick, result.PlayNextLevel);
            UnityEventTools.AddPersistentListener(menu.onClick, result.ReturnToMenu);
            UnityEventTools.AddPersistentListener(menuOpen.onClick, sceneButtons.OpenMainMenu);
            UnityEventTools.AddPersistentListener(storeOpen.onClick, sceneButtons.OpenStore);
            Ref(tuner, "gameplayCamera", cameraObject.GetComponent<Camera>());
            Ref(tuner, "rotatorRoot", rotator.transform);
            Ref(tuner, "rotatorVisualRoot", rotatorVisual.transform);
            Ref(tuner, "targetRotator", rotatorComp);
            Transform[] slotAnchorRefs = new Transform[slots.Length];
            for (int i = 0; i < slots.Length; i++)
            {
                slotAnchorRefs[i] = slots[i].transform;
            }
            Refs(tuner, "slotAnchors", slotAnchorRefs);
            Ref(tuner, "launcherBody", launcher.transform);
            Ref(tuner, "pinSpawnPoint", spawn.transform);
            Ref(tuner, "flightLane", flightLane.GetComponent<SpriteRenderer>());
            Ref(tuner, "pinLauncher", launcherComp);
            Ref(tuner, "topBar", topBar.rectTransform);
            Ref(tuner, "questionPanel", questionPanel.rectTransform);
            Ref(tuner, "bottomBar", bottom.rectTransform);
            Ref(tuner, "keyboardSkinFrame", keyboardSkinFrame.rectTransform);
            Ref(tuner, "keyboardGrid", gridRect);
            Ref(tuner, "menuButton", menuOpen.GetComponent<RectTransform>());
            Ref(tuner, "storeButton", storeOpen.GetComponent<RectTransform>());
            Ref(tuner, "swipeHint", swipeHint.rectTransform);
            Ref(tuner, "keyboardGridLayout", grid);
            EditorSceneManager.SaveScene(scene, $"{Scenes}/{GameConstants.SceneGameplay}.unity");
        }

        private static void BuildStoreScene(bool forceReset)
        {
            if (!forceReset && AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneStore}.unity") != null)
            {
                return;
            }

            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            Cam("Store Camera", new Color(0.07f, 0.06f, 0.09f));
            EventSystem();
            new GameObject("StoreSystems").AddComponent<SceneBootstrap>();
            Canvas canvas = Canvas("StoreCanvas");
            Label("Title", canvas.transform, "Theme Store", 64, new Vector2(0.5f, 0.86f), new Vector2(700, 90), new Color(1f, 0.84f, 0.58f));
            TextMeshProUGUI theme = Label("Theme", canvas.transform, "Mythology Theme: Locked", 28, new Vector2(0.5f, 0.72f), new Vector2(700, 50), Color.white);
            TextMeshProUGUI membership = Label("Membership", canvas.transform, "Membership: Inactive", 28, new Vector2(0.5f, 0.67f), new Vector2(700, 50), Color.white);
            TextMeshProUGUI benefits = Label("Benefits", canvas.transform, "Premium inactive", 24, new Vector2(0.5f, 0.61f), new Vector2(760, 70), new Color(0.88f, 0.88f, 0.92f));
            Button themeBtn = Button("ThemeButton", canvas.transform, "Unlock Theme", new Vector2(0.5f, 0.48f), new Vector2(360, 82), new Color(0.84f, 0.43f, 0.16f));
            Button hintsBtn = Button("HintsButton", canvas.transform, "Buy Hints", new Vector2(0.5f, 0.38f), new Vector2(360, 82), new Color(0.22f, 0.24f, 0.30f));
            Button energyBtn = Button("EnergyButton", canvas.transform, "Buy Energy", new Vector2(0.5f, 0.28f), new Vector2(360, 82), new Color(0.22f, 0.24f, 0.30f));
            Button premiumBtn = Button("PremiumButton", canvas.transform, "Membership", new Vector2(0.5f, 0.18f), new Vector2(360, 82), new Color(0.42f, 0.27f, 0.16f));
            Button backBtn = Button("BackButton", canvas.transform, "Back", new Vector2(0.5f, 0.08f), new Vector2(220, 62), new Color(0.22f, 0.24f, 0.30f));
            StorePresenter store = canvas.gameObject.AddComponent<StorePresenter>();
            MembershipPresenter mp = canvas.gameObject.AddComponent<MembershipPresenter>();
            Ref(store, "themeStatusLabel", theme);
            Ref(store, "membershipStatusLabel", membership);
            Ref(mp, "benefitsLabel", benefits);
            UnityEventTools.AddPersistentListener(themeBtn.onClick, store.BuyMythologyTheme);
            UnityEventTools.AddPersistentListener(hintsBtn.onClick, store.BuyHints);
            UnityEventTools.AddPersistentListener(energyBtn.onClick, store.BuyEnergy);
            UnityEventTools.AddPersistentListener(premiumBtn.onClick, store.BuyMembership);
            UnityEventTools.AddPersistentListener(backBtn.onClick, store.BackToMenu);
            EditorSceneManager.SaveScene(scene, $"{Scenes}/{GameConstants.SceneStore}.unity");
        }

        private static GameObject BuildPinPrefab(bool forceReset)
        {
            string path = $"{Prefabs}/Pin.prefab";
            if (!forceReset)
            {
                GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (existing != null)
                {
                    return existing;
                }
            }

            GameObject go = new GameObject("Pin");
            GameObject ring = Sprite("OuterRing", BuiltinRound(), Vector3.zero, new Vector3(0.24f, 0.24f, 1f), new Color(0.24f, 0.18f, 0.12f, 0.96f));
            ring.transform.SetParent(go.transform, false);
            SpriteRenderer ringRenderer = ring.GetComponent<SpriteRenderer>();
            ringRenderer.sortingOrder = 10;

            GameObject shaft = Sprite("Shaft", Builtin(), new Vector3(0f, -0.38f, 0f), new Vector3(0.11f, 0.74f, 1f), new Color(0.24f, 0.18f, 0.12f, 0.96f));
            shaft.transform.SetParent(go.transform, false);
            shaft.GetComponent<SpriteRenderer>().sortingOrder = 9;

            GameObject core = Sprite("Core", BuiltinRound(), Vector3.zero, new Vector3(0.20f, 0.20f, 1f), new Color(0.95f, 0.66f, 0.24f));
            core.transform.SetParent(go.transform, false);
            SpriteRenderer coreRenderer = core.GetComponent<SpriteRenderer>();
            coreRenderer.sortingOrder = 11;

            GameObject sheen = Sprite("Sheen", BuiltinRound(), new Vector3(-0.02f, 0.02f, 0f), new Vector3(0.08f, 0.08f, 1f), new Color(1f, 0.92f, 0.80f, 0.55f));
            sheen.transform.SetParent(go.transform, false);
            sheen.GetComponent<SpriteRenderer>().sortingOrder = 12;

            Rigidbody2D rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.bodyType = RigidbodyType2D.Kinematic;
            CircleCollider2D col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.13f;
            Pin pin = go.AddComponent<Pin>();
            GameObject labelObject = new GameObject("LetterLabel", typeof(TextMesh));
            labelObject.transform.SetParent(go.transform, false);
            labelObject.transform.localPosition = new Vector3(0f, -0.80f, -0.1f);
            TextMesh label = labelObject.GetComponent<TextMesh>();
            label.text = "?";
            label.characterSize = 0.10f;
            label.anchor = TextAnchor.MiddleCenter;
            label.alignment = TextAlignment.Center;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 52;
            label.color = new Color(0.20f, 0.12f, 0.04f);
            MeshRenderer meshRenderer = labelObject.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                meshRenderer.sortingOrder = 11;
            }
            Ref(pin, "letterLabel", label);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static void BuildRotatorShapePrefabs(bool forceReset)
        {
            EnsureFolder(RotatorShapePrefabs);
            BuildRotatorShapePrefab("CircleClassic", new Vector2(1f, 1f), false, forceReset);
            BuildRotatorShapePrefab("OvalFlow", new Vector2(1.34f, 0.78f), false, forceReset);
            BuildRotatorShapePrefab("DiamondDrive", new Vector2(1.14f, 1.14f), true, forceReset);
        }

        private static GameObject BuildRotatorShapePrefab(string prefabName, Vector2 globalScale, bool diamondStyle, bool forceReset)
        {
            string path = $"{RotatorShapePrefabs}/{prefabName}.prefab";
            if (!forceReset)
            {
                GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (existing != null)
                {
                    return existing;
                }
            }

            GameObject root = new GameObject(prefabName);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;
            root.transform.localScale = Vector3.one;

            CreateShapeDiskLayer("ActionShadow", root.transform, BuiltinRound(), Vector3.zero, globalScale * 2.62f, new Color(0f, 0f, 0f, 0.42f), 30, 0f);
            CreateShapeDiskLayer("ActionGlow", root.transform, BuiltinRound(), Vector3.zero, globalScale * 2.54f, new Color(1f, 0.56f, 0.18f, 0.12f), 31, 0f);
            CreateShapeDiskLayer("ActionOuterRim", root.transform, diamondStyle ? Builtin() : BuiltinRound(), Vector3.zero, globalScale * 2.32f, new Color(0.15f, 0.09f, 0.07f, 1f), 32, diamondStyle ? 45f : 0f);
            CreateShapeDiskLayer("ActionRing", root.transform, diamondStyle ? Builtin() : BuiltinRound(), Vector3.zero, globalScale * 2.12f, new Color(0.30f, 0.18f, 0.14f, 0.98f), 33, diamondStyle ? 45f : 0f);
            CreateShapeDiskLayer("PlaqueBand", root.transform, BuiltinRound(), Vector3.zero, globalScale * 1.92f, new Color(0.55f, 0.34f, 0.19f, 0.34f), 34, 0f);
            CreateShapeDiskLayer("GapRim", root.transform, diamondStyle ? Builtin() : BuiltinRound(), Vector3.zero, globalScale * 1.64f, new Color(0.20f, 0.13f, 0.10f, 0.98f), 35, diamondStyle ? 45f : 0f);
            CreateShapeDiskLayer("GapCutout", root.transform, diamondStyle ? Builtin() : BuiltinRound(), Vector3.zero, globalScale * 1.42f, new Color(0.10f, 0.08f, 0.08f, 0.98f), 36, diamondStyle ? 45f : 0f);
            CreateShapeDiskLayer("CenterShadow", root.transform, BuiltinRound(), new Vector3(0f, -0.02f, 0f), globalScale * 1.18f, new Color(0f, 0f, 0f, 0.24f), 37, 0f);
            CreateShapeDiskLayer("CenterOuter", root.transform, diamondStyle ? Builtin() : BuiltinRound(), Vector3.zero, globalScale * 1.08f, new Color(0.25f, 0.16f, 0.12f, 1f), 38, diamondStyle ? 45f : 0f);
            CreateShapeDiskLayer("CenterFace", root.transform, BuiltinRound(), Vector3.zero, globalScale * 0.88f, new Color(0.14f, 0.10f, 0.09f, 0.98f), 39, 0f);
            CreateShapeDiskLayer("CenterInset", root.transform, BuiltinRound(), Vector3.zero, globalScale * 0.52f, new Color(0.72f, 0.44f, 0.22f, 0.18f), 40, 0f);

            if (prefabName == "OvalFlow")
            {
                CreateShapeDiskLayer("FlowWingLeft", root.transform, Builtin(), new Vector3(-1.58f, 0f, 0f), new Vector2(0.58f, 1.28f), new Color(0.48f, 0.30f, 0.18f, 0.48f), 34, 90f);
                CreateShapeDiskLayer("FlowWingRight", root.transform, Builtin(), new Vector3(1.58f, 0f, 0f), new Vector2(0.58f, 1.28f), new Color(0.48f, 0.30f, 0.18f, 0.48f), 34, 90f);
                CreateShapeDiskLayer("FlowSpine", root.transform, Builtin(), Vector3.zero, new Vector2(2.72f, 0.24f), new Color(0.76f, 0.48f, 0.24f, 0.22f), 35, 0f);
                CreateShapeDiskLayer("FlowBridgeTop", root.transform, Builtin(), new Vector3(0f, 0.86f, 0f), new Vector2(1.92f, 0.16f), new Color(0.62f, 0.38f, 0.18f, 0.18f), 35, 0f);
                CreateShapeDiskLayer("FlowBridgeBottom", root.transform, Builtin(), new Vector3(0f, -0.86f, 0f), new Vector2(1.92f, 0.16f), new Color(0.62f, 0.38f, 0.18f, 0.18f), 35, 0f);
            }
            else if (prefabName == "DiamondDrive")
            {
                CreateShapeDiskLayer("DiamondTop", root.transform, Builtin(), new Vector3(0f, 1.56f, 0f), new Vector2(0.42f, 0.72f), new Color(0.62f, 0.38f, 0.18f, 0.32f), 34, 45f);
                CreateShapeDiskLayer("DiamondRight", root.transform, Builtin(), new Vector3(1.56f, 0f, 0f), new Vector2(0.42f, 0.72f), new Color(0.62f, 0.38f, 0.18f, 0.32f), 34, 135f);
                CreateShapeDiskLayer("DiamondBottom", root.transform, Builtin(), new Vector3(0f, -1.56f, 0f), new Vector2(0.42f, 0.72f), new Color(0.62f, 0.38f, 0.18f, 0.32f), 34, 225f);
                CreateShapeDiskLayer("DiamondLeft", root.transform, Builtin(), new Vector3(-1.56f, 0f, 0f), new Vector2(0.42f, 0.72f), new Color(0.62f, 0.38f, 0.18f, 0.32f), 34, 315f);
                CreateShapeDiskLayer("DiamondCrossA", root.transform, Builtin(), Vector3.zero, new Vector2(2.34f, 0.18f), new Color(0.76f, 0.50f, 0.24f, 0.20f), 35, 45f);
                CreateShapeDiskLayer("DiamondCrossB", root.transform, Builtin(), Vector3.zero, new Vector2(2.34f, 0.18f), new Color(0.76f, 0.50f, 0.24f, 0.20f), 35, -45f);
            }

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            Object.DestroyImmediate(root);
            return prefab;
        }

        private static GameObject BuildKeyPrefab(bool forceReset)
        {
            string path = $"{Prefabs}/KeyButton.prefab";
            if (!forceReset)
            {
                GameObject existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (existing != null)
                {
                    return existing;
                }
            }

            GameObject go = new GameObject("KeyButton", typeof(RectTransform), typeof(Image), typeof(Button));
            go.GetComponent<Image>().sprite = Builtin();
            go.GetComponent<Image>().type = Image.Type.Sliced;
            go.GetComponent<Image>().color = new Color(0.24f, 0.24f, 0.28f, 0.96f);
            go.GetComponent<Button>().targetGraphic = go.GetComponent<Image>();
            Rt(go, null, Vector2.zero, new Vector2(84, 72));
            Label("Label", go.transform, "A", 26, new Vector2(0.5f, 0.5f), new Vector2(80, 40), Color.white);
            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject Cam(string name, Color color)
        {
            GameObject go = new GameObject(name);
            Camera cam = go.AddComponent<Camera>();
            go.AddComponent<AudioListener>();
            go.transform.position = new Vector3(0f, 0f, -10f);
            cam.orthographic = true;
            cam.orthographicSize = 5.4f;
            cam.backgroundColor = color;
            cam.clearFlags = CameraClearFlags.SolidColor;
            return go;
        }

        private static void EventSystem() => new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(StandaloneInputModule));
        private static Canvas Canvas(string name)
        {
            GameObject go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.transform.localScale = Vector3.one;
            RectTransform rect = go.GetComponent<RectTransform>();
            if (rect != null)
            {
                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
                rect.localRotation = Quaternion.identity;
                rect.localScale = Vector3.one;
            }

            Canvas c = go.GetComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            CanvasScaler s = go.GetComponent<CanvasScaler>();
            s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            s.referenceResolution = new Vector2(1080, 1920);
            s.matchWidthOrHeight = 1f;
            return c;
        }
        private static Image Panel(string name, Transform p, Vector2 a, Vector2 size, Color c) { GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image)); Rt(go, p, a, size); Image i = go.GetComponent<Image>(); i.sprite = Builtin(); i.type = Image.Type.Sliced; i.color = c; return i; }
        private static RawImage RawAssetImage(string name, Transform p, string assetPath, Vector2 a, Vector2 size, Color fallbackTint, float loadedAlpha = 1f)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(RawImage));
            Rt(go, p, a, size);
            RawImage image = go.GetComponent<RawImage>();
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            image.texture = texture;
            image.color = texture != null ? new Color(1f, 1f, 1f, Mathf.Clamp01(loadedAlpha)) : fallbackTint;
            image.raycastTarget = false;

            if (texture == null)
            {
                TextMeshProUGUI fallback = Label("MissingAssetLabel", go.transform, System.IO.Path.GetFileName(assetPath), 18f, new Vector2(0.5f, 0.5f), new Vector2(size.x - 24f, 32f), new Color(1f, 1f, 1f, 0.72f));
                fallback.raycastTarget = false;
            }

            return image;
        }
        private static Button Button(string name, Transform p, string text, Vector2 a, Vector2 size, Color c) { Image i = Panel(name, p, a, size, c); Button b = i.gameObject.AddComponent<Button>(); b.targetGraphic = i; Label("Label", i.transform, text, 26, new Vector2(0.5f, 0.5f), new Vector2(size.x - 24f, 42f), Color.white); return b; }
        private static TMP_InputField InputField(string name, Transform p, string placeholder, Vector2 a, Vector2 size)
        {
            GameObject root = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
            Rt(root, p, a, size);
            Image background = root.GetComponent<Image>();
            background.sprite = Builtin();
            background.type = Image.Type.Sliced;
            background.color = new Color(0.12f, 0.13f, 0.16f, 0.98f);

            TextMeshProUGUI text = Label("Text", root.transform, string.Empty, 26, new Vector2(0.5f, 0.5f), new Vector2(size.x - 36f, size.y - 18f), Color.white);
            text.alignment = TextAlignmentOptions.MidlineLeft;
            text.margin = new Vector4(18f, 0f, 18f, 0f);

            TextMeshProUGUI placeholderLabel = Label("Placeholder", root.transform, placeholder, 24, new Vector2(0.5f, 0.5f), new Vector2(size.x - 36f, size.y - 18f), new Color(0.72f, 0.72f, 0.76f, 0.72f));
            placeholderLabel.alignment = TextAlignmentOptions.MidlineLeft;
            placeholderLabel.margin = new Vector4(18f, 0f, 18f, 0f);

            TMP_InputField inputField = root.GetComponent<TMP_InputField>();
            inputField.textViewport = root.GetComponent<RectTransform>();
            inputField.textComponent = text;
            inputField.placeholder = placeholderLabel;
            inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
            inputField.lineType = TMP_InputField.LineType.SingleLine;
            return inputField;
        }
        private static TextMeshProUGUI Label(string name, Transform p, string t, float fs, Vector2 a, Vector2 size, Color c) { GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI)); Rt(go, p, a, size); TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>(); TMP_FontAsset font = LoadDefaultTmpFont(); if (font != null) tmp.font = font; tmp.text = t; tmp.fontSize = fs; tmp.alignment = TextAlignmentOptions.Center; tmp.color = c; tmp.enableWordWrapping = true; return tmp; }
        private static RectTransform Rt(GameObject go, Transform p, Vector2 a, Vector2 size) { if (p != null) go.transform.SetParent(p, false); RectTransform rt = go.GetComponent<RectTransform>(); if (rt == null) rt = go.AddComponent<RectTransform>(); rt.anchorMin = a; rt.anchorMax = a; rt.anchoredPosition = Vector2.zero; rt.sizeDelta = size; rt.localScale = Vector3.one; return rt; }
        private static GameObject Sprite(string name, Vector3 pos, Vector3 scale, Color c) { return Sprite(name, Builtin(), pos, scale, c); }
        private static GameObject Sprite(string name, Sprite sprite, Vector3 pos, Vector3 scale, Color c) { GameObject go = new GameObject(name, typeof(SpriteRenderer)); go.transform.position = pos; go.transform.localScale = scale; SpriteRenderer sr = go.GetComponent<SpriteRenderer>(); sr.sprite = sprite; sr.color = c; return go; }
        private static SpriteRenderer CreateShapeDiskLayer(string name, Transform parent, Sprite sprite, Vector3 localPosition, Vector2 size, Color color, int sortingOrder, float rotationZ)
        {
            GameObject go = new GameObject(name, typeof(SpriteRenderer));
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localRotation = Quaternion.Euler(0f, 0f, rotationZ);
            go.transform.localScale = new Vector3(size.x, size.y, 1f);
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return renderer;
        }

        private struct LevelHubPreviewState
        {
            public LevelHubPreviewController.RailPoint[] railPoints;
            public int totalLevels;
            public float dragPixelsPerLevel;
            public float snapSharpness;
        }

        private static LevelHubPreviewState CaptureLevelHubPreviewState(LevelHubPreviewController controller)
        {
            var state = new LevelHubPreviewState
            {
                railPoints = null,
                totalLevels = 25,
                dragPixelsPerLevel = 160f,
                snapSharpness = 12f
            };

            if (controller == null)
            {
                return state;
            }

            SerializedObject so = new SerializedObject(controller);
            SerializedProperty railPointsProperty = so.FindProperty("railPoints");
            if (railPointsProperty != null && railPointsProperty.isArray && railPointsProperty.arraySize >= 2)
            {
                state.railPoints = new LevelHubPreviewController.RailPoint[railPointsProperty.arraySize];
                for (int i = 0; i < railPointsProperty.arraySize; i++)
                {
                    SerializedProperty point = railPointsProperty.GetArrayElementAtIndex(i);
                    Vector2 position = point.FindPropertyRelative("position").vector2Value;
                    float scale = point.FindPropertyRelative("scale").floatValue;
                    float rotation = point.FindPropertyRelative("rotation").floatValue;
                    float alpha = point.FindPropertyRelative("alpha").floatValue;
                    state.railPoints[i] = new LevelHubPreviewController.RailPoint(position, scale, rotation, alpha);
                }
            }

            SerializedProperty totalLevelsProperty = so.FindProperty("totalLevels");
            if (totalLevelsProperty != null)
            {
                state.totalLevels = Mathf.Max(1, totalLevelsProperty.intValue);
            }

            SerializedProperty dragPixelsPerLevelProperty = so.FindProperty("dragPixelsPerLevel");
            if (dragPixelsPerLevelProperty != null)
            {
                state.dragPixelsPerLevel = Mathf.Max(20f, dragPixelsPerLevelProperty.floatValue);
            }

            SerializedProperty snapSharpnessProperty = so.FindProperty("snapSharpness");
            if (snapSharpnessProperty != null)
            {
                state.snapSharpness = Mathf.Max(1f, snapSharpnessProperty.floatValue);
            }

            return state;
        }

        private static bool ApplyLevelHubPreviewState(LevelHubPreviewController controller, LevelHubPreviewState state)
        {
            if (controller == null)
            {
                return false;
            }

            bool changed = false;
            SerializedObject so = new SerializedObject(controller);
            SerializedProperty railPointsProperty = so.FindProperty("railPoints");
            if (railPointsProperty != null && railPointsProperty.isArray && state.railPoints != null && state.railPoints.Length >= 2)
            {
                if (railPointsProperty.arraySize != state.railPoints.Length)
                {
                    railPointsProperty.arraySize = state.railPoints.Length;
                    changed = true;
                }

                for (int i = 0; i < state.railPoints.Length; i++)
                {
                    SerializedProperty point = railPointsProperty.GetArrayElementAtIndex(i);
                    changed |= SetVector2Property(point.FindPropertyRelative("position"), state.railPoints[i].position);
                    changed |= SetFloatProperty(point.FindPropertyRelative("scale"), state.railPoints[i].scale);
                    changed |= SetFloatProperty(point.FindPropertyRelative("rotation"), state.railPoints[i].rotation);
                    changed |= SetFloatProperty(point.FindPropertyRelative("alpha"), state.railPoints[i].alpha);
                }
            }

            changed |= SetIntProperty(so.FindProperty("totalLevels"), Mathf.Max(1, state.totalLevels));
            changed |= SetFloatProperty(so.FindProperty("dragPixelsPerLevel"), Mathf.Max(20f, state.dragPixelsPerLevel));
            changed |= SetFloatProperty(so.FindProperty("snapSharpness"), Mathf.Max(1f, state.snapSharpness));

            if (!changed)
            {
                return false;
            }

            so.ApplyModifiedPropertiesWithoutUndo();
            controller.EditorRefresh();
            PrefabUtility.RecordPrefabInstancePropertyModifications(controller);
            return true;
        }

        private static bool NormalizeHubPreviewCanvas(Canvas canvas)
        {
            if (canvas == null)
            {
                return false;
            }

            bool changed = false;
            RectTransform rect = canvas.GetComponent<RectTransform>();
            if (rect != null)
            {
                if (rect.anchorMin != Vector2.zero)
                {
                    rect.anchorMin = Vector2.zero;
                    changed = true;
                }

                if (rect.anchorMax != Vector2.one)
                {
                    rect.anchorMax = Vector2.one;
                    changed = true;
                }

                if (rect.pivot != new Vector2(0.5f, 0.5f))
                {
                    rect.pivot = new Vector2(0.5f, 0.5f);
                    changed = true;
                }

                if (rect.anchoredPosition != Vector2.zero)
                {
                    rect.anchoredPosition = Vector2.zero;
                    changed = true;
                }

                if (rect.sizeDelta != Vector2.zero)
                {
                    rect.sizeDelta = Vector2.zero;
                    changed = true;
                }

                if (rect.localRotation != Quaternion.identity)
                {
                    rect.localRotation = Quaternion.identity;
                    changed = true;
                }

                if (rect.localScale != Vector3.one)
                {
                    rect.localScale = Vector3.one;
                    changed = true;
                }
            }

            CanvasScaler scaler = canvas.GetComponent<CanvasScaler>();
            if (scaler != null)
            {
                if (scaler.uiScaleMode != CanvasScaler.ScaleMode.ScaleWithScreenSize)
                {
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    changed = true;
                }

                if ((scaler.referenceResolution - MainMenuReferenceSize).sqrMagnitude > 0.001f)
                {
                    scaler.referenceResolution = MainMenuReferenceSize;
                    changed = true;
                }

                if (!Mathf.Approximately(scaler.matchWidthOrHeight, 1f))
                {
                    scaler.matchWidthOrHeight = 1f;
                    changed = true;
                }
            }

            return changed;
        }

        private static Canvas FindHubPreviewCanvas()
        {
            Canvas[] canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < canvases.Length; i++)
            {
                Canvas canvas = canvases[i];
                if (canvas != null && canvas.name == "HubPreviewCanvas")
                {
                    return canvas;
                }
            }

            return canvases.Length > 0 ? canvases[0] : null;
        }

        private static int RemoveRootNodeVisualOrphans(UnityEngine.SceneManagement.Scene scene)
        {
            int removed = 0;
            GameObject[] roots = scene.GetRootGameObjects();
            for (int i = 0; i < roots.Length; i++)
            {
                GameObject root = roots[i];
                if (root == null || root.name != "NodeVisual")
                {
                    continue;
                }

                Object.DestroyImmediate(root);
                removed++;
            }

            return removed;
        }

        private static bool SetIntProperty(SerializedProperty property, int value)
        {
            if (property == null || property.intValue == value)
            {
                return false;
            }

            property.intValue = value;
            return true;
        }

        private static bool SetFloatProperty(SerializedProperty property, float value)
        {
            if (property == null || Mathf.Approximately(property.floatValue, value))
            {
                return false;
            }

            property.floatValue = value;
            return true;
        }

        private static bool SetVector2Property(SerializedProperty property, Vector2 value)
        {
            if (property == null || (property.vector2Value - value).sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            property.vector2Value = value;
            return true;
        }

        private static bool NormalizeLevelHubNodeVisuals(GameObject root, Sprite nodeSprite)
        {
            if (root == null || nodeSprite == null)
            {
                return false;
            }

            bool changed = false;
            RectTransform[] nodes = root.GetComponentsInChildren<RectTransform>(true);
            Color hiddenRootColor = LevelHubNodeHostColor;

            for (int i = 0; i < nodes.Length; i++)
            {
                RectTransform nodeRect = nodes[i];
                if (nodeRect == null || !nodeRect.name.StartsWith("LevelNode_"))
                {
                    continue;
                }

                if (nodeRect.sizeDelta != LevelHubNodeSize)
                {
                    nodeRect.sizeDelta = LevelHubNodeSize;
                    changed = true;
                }

                Image nodeImage = nodeRect.GetComponent<Image>();
                if (nodeImage == null)
                {
                    nodeImage = nodeRect.gameObject.AddComponent<Image>();
                    changed = true;
                }

                if (nodeImage != null)
                {
                    if (nodeImage.sprite != null)
                    {
                        nodeImage.sprite = null;
                        changed = true;
                    }

                    if (nodeImage.preserveAspect)
                    {
                        nodeImage.preserveAspect = false;
                        changed = true;
                    }

                    if (nodeImage.type != Image.Type.Simple)
                    {
                        nodeImage.type = Image.Type.Simple;
                        changed = true;
                    }

                    if (nodeImage.color != hiddenRootColor)
                    {
                        nodeImage.color = hiddenRootColor;
                        changed = true;
                    }

                    if (!nodeImage.raycastTarget)
                    {
                        nodeImage.raycastTarget = true;
                        changed = true;
                    }
                }

                RectTransform visualRect = nodeRect.Find("NodeVisual") as RectTransform;
                if (visualRect == null)
                {
                    GameObject visualGo = new GameObject("NodeVisual", typeof(RectTransform), typeof(Image));
                    visualRect = visualGo.GetComponent<RectTransform>();
                    visualRect.SetParent(nodeRect, false);
                    changed = true;
                }

                if (visualRect.anchorMin != new Vector2(0.5f, 0.5f))
                {
                    visualRect.anchorMin = new Vector2(0.5f, 0.5f);
                    changed = true;
                }

                if (visualRect.anchorMax != new Vector2(0.5f, 0.5f))
                {
                    visualRect.anchorMax = new Vector2(0.5f, 0.5f);
                    changed = true;
                }

                if (visualRect.pivot != new Vector2(0.5f, 0.5f))
                {
                    visualRect.pivot = new Vector2(0.5f, 0.5f);
                    changed = true;
                }

                if (visualRect.anchoredPosition != Vector2.zero)
                {
                    visualRect.anchoredPosition = Vector2.zero;
                    changed = true;
                }

                if (visualRect.sizeDelta != LevelHubNodeVisualSize)
                {
                    visualRect.sizeDelta = LevelHubNodeVisualSize;
                    changed = true;
                }

                if (visualRect.localScale != Vector3.one)
                {
                    visualRect.localScale = Vector3.one;
                    changed = true;
                }

                if (visualRect.GetSiblingIndex() != 0)
                {
                    visualRect.SetSiblingIndex(0);
                    changed = true;
                }

                Image visualImage = visualRect.GetComponent<Image>();
                if (visualImage == null)
                {
                    visualImage = visualRect.gameObject.AddComponent<Image>();
                    changed = true;
                }

                if (visualImage.sprite != nodeSprite)
                {
                    visualImage.sprite = nodeSprite;
                    changed = true;
                }

                if (visualImage.type != Image.Type.Simple)
                {
                    visualImage.type = Image.Type.Simple;
                    changed = true;
                }

                if (!visualImage.preserveAspect)
                {
                    visualImage.preserveAspect = true;
                    changed = true;
                }

                if (visualImage.color != Color.white)
                {
                    visualImage.color = Color.white;
                    changed = true;
                }

                if (visualImage.raycastTarget)
                {
                    visualImage.raycastTarget = false;
                    changed = true;
                }

                Button button = nodeRect.GetComponent<Button>();
                if (button != null && button.targetGraphic != visualImage)
                {
                    button.targetGraphic = visualImage;
                    changed = true;
                }

                TextMeshProUGUI label = nodeRect.Find("LevelNumLabel")?.GetComponent<TextMeshProUGUI>();
                if (label == null)
                {
                    label = nodeRect.GetComponentInChildren<TextMeshProUGUI>(true);
                }

                if (label == null)
                {
                    continue;
                }

                RectTransform labelRect = label.rectTransform;
                if (labelRect.anchoredPosition != LevelHubNodeLabelPosition)
                {
                    labelRect.anchoredPosition = LevelHubNodeLabelPosition;
                    changed = true;
                }

                if (labelRect.sizeDelta != LevelHubNodeLabelSize)
                {
                    labelRect.sizeDelta = LevelHubNodeLabelSize;
                    changed = true;
                }

                if (!Mathf.Approximately(label.fontSize, LevelHubNodeLabelFontSize))
                {
                    label.fontSize = LevelHubNodeLabelFontSize;
                    changed = true;
                }

                if (label.alignment != TextAlignmentOptions.Center)
                {
                    label.alignment = TextAlignmentOptions.Center;
                    changed = true;
                }

                if (label.raycastTarget)
                {
                    label.raycastTarget = false;
                    changed = true;
                }
            }

            return changed;
        }
        private static Sprite Builtin() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        private static Sprite BuiltinRound() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        private static TMP_FontAsset LoadDefaultTmpFont() => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        private static void EnsureFolder(string path) { string[] parts = path.Split('/'); string current = parts[0]; for (int i = 1; i < parts.Length; i++) { string next = current + "/" + parts[i]; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]); current = next; } }
        private static void Ref(Object target, string field, Object value) { SerializedObject so = new SerializedObject(target); SerializedProperty p = so.FindProperty(field); if (p != null) { p.objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); } }
        private static void Refs(Object target, string field, Object[] values) { SerializedObject so = new SerializedObject(target); SerializedProperty p = so.FindProperty(field); if (p != null && p.isArray) { p.arraySize = values.Length; for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; so.ApplyModifiedPropertiesWithoutUndo(); } }
        private static void Bool(Object target, string field, bool value) { SerializedObject so = new SerializedObject(target); SerializedProperty p = so.FindProperty(field); if (p != null) { p.boolValue = value; so.ApplyModifiedPropertiesWithoutUndo(); } }
        private static void ColorValue(Object target, string field, Color value) { SerializedObject so = new SerializedObject(target); SerializedProperty p = so.FindProperty(field); if (p != null) { p.colorValue = value; so.ApplyModifiedPropertiesWithoutUndo(); } }
        private static void String(Object target, string field, string value) { SerializedObject so = new SerializedObject(target); SerializedProperty p = so.FindProperty(field); if (p != null) { p.stringValue = value; so.ApplyModifiedPropertiesWithoutUndo(); } }

    }
}
