using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
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

        [InitializeOnLoadMethod]
        private static void AutoBuild() => EditorApplication.delayCall += TryBuild;

        [MenuItem("Tools/WordSpin Alpha/Rebuild Scenes")]
        public static void RebuildScenes() => BuildAll(false);

        [MenuItem("Tools/WordSpin Alpha/Force Reset Generated Scenes")]
        public static void ForceResetGeneratedScenes() => BuildAll(true);

        [MenuItem("Tools/WordSpin Alpha/Rebuild Rotator Shape Prefabs")]
        public static void RebuildRotatorShapePrefabs() => BuildRotatorShapePrefabs(true);

        private static void TryBuild()
        {
            if (EditorApplication.isCompiling || EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            if (AssetDatabase.LoadAssetAtPath<SceneAsset>($"{Scenes}/{GameConstants.SceneGameplay}.unity") != null)
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
            BuildBootScene(forceReset);
            BuildMainMenuScene(forceReset);
            BuildGameplayScene(pinPrefab, keyPrefab, forceReset);
            BuildStoreScene(forceReset);
            EditorBuildSettings.scenes = new[]
            {
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneBoot}.unity", true),
                new EditorBuildSettingsScene($"{Scenes}/{GameConstants.SceneMainMenu}.unity", true),
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
            Cam("MainMenu Camera", new Color(0.07f, 0.06f, 0.09f));
            EventSystem();
            new GameObject("MainMenuSystems").AddComponent<SceneBootstrap>();
            Canvas canvas = Canvas("MainMenuCanvas");
            Label("Title", canvas.transform, "WORDSPIN", 72, new Vector2(0.5f, 0.86f), new Vector2(700, 90), new Color(1f, 0.84f, 0.58f));
            TextMeshProUGUI energy = Label("Energy", canvas.transform, "Energy: 5/5", 28, new Vector2(0.5f, 0.72f), new Vector2(400, 50), Color.white);
            TextMeshProUGUI hints = Label("Hints", canvas.transform, "Hints: 0", 28, new Vector2(0.5f, 0.67f), new Vector2(400, 50), Color.white);
            TextMeshProUGUI language = Label("Language", canvas.transform, "Dil: TR", 24, new Vector2(0.5f, 0.62f), new Vector2(300, 40), new Color(0.94f, 0.94f, 0.98f));
            Button langTr = Button("LangTR", canvas.transform, "TR", new Vector2(0.36f, 0.57f), new Vector2(110, 52), new Color(0.22f, 0.24f, 0.30f));
            Button langEn = Button("LangEN", canvas.transform, "EN", new Vector2(0.46f, 0.57f), new Vector2(110, 52), new Color(0.22f, 0.24f, 0.30f));
            Button langEs = Button("LangES", canvas.transform, "ES", new Vector2(0.56f, 0.57f), new Vector2(110, 52), new Color(0.22f, 0.24f, 0.30f));
            Button langDe = Button("LangDE", canvas.transform, "DE", new Vector2(0.66f, 0.57f), new Vector2(110, 52), new Color(0.22f, 0.24f, 0.30f));
            Button play = Button("PlayButton", canvas.transform, "Play", new Vector2(0.5f, 0.52f), new Vector2(320, 86), new Color(0.84f, 0.43f, 0.16f));
            Button levels = Button("LevelsButton", canvas.transform, "Level Seç", new Vector2(0.5f, 0.42f), new Vector2(320, 86), new Color(0.36f, 0.24f, 0.17f));
            Button store = Button("StoreButton", canvas.transform, "Store", new Vector2(0.5f, 0.42f), new Vector2(320, 86), new Color(0.22f, 0.24f, 0.30f));
            store.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.32f);
            store.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.32f);

            GameObject levelSelectRoot = Panel("LevelSelectOverlay", canvas.transform, new Vector2(0.5f, 0.5f), new Vector2(980, 1280), new Color(0.07f, 0.06f, 0.09f, 0.96f)).gameObject;
            TextMeshProUGUI levelSelectTitle = Label("LevelSelectTitle", levelSelectRoot.transform, "Level Seçimi", 44, new Vector2(0.5f, 0.94f), new Vector2(520, 56), new Color(1f, 0.84f, 0.58f));
            TextMeshProUGUI levelSelectSummary = Label("LevelSelectSummary", levelSelectRoot.transform, "Toplam 0 level", 24, new Vector2(0.5f, 0.89f), new Vector2(720, 42), new Color(0.92f, 0.92f, 0.96f));
            Button closeLevels = Button("CloseLevelsButton", levelSelectRoot.transform, "Kapat", new Vector2(0.84f, 0.94f), new Vector2(180, 54), new Color(0.22f, 0.24f, 0.30f));

            TMP_InputField levelJumpInput = InputField("LevelJumpInput", levelSelectRoot.transform, "Level no", new Vector2(0.34f, 0.81f), new Vector2(280, 64));
            Button jumpButton = Button("JumpButton", levelSelectRoot.transform, "Git", new Vector2(0.64f, 0.81f), new Vector2(160, 64), new Color(0.84f, 0.43f, 0.16f));

            GameObject scrollRoot = new GameObject("LevelScrollView", typeof(RectTransform), typeof(Image), typeof(Mask), typeof(ScrollRect));
            RectTransform scrollRectTransform = Rt(scrollRoot, levelSelectRoot.transform, new Vector2(0.5f, 0.38f), new Vector2(860, 840));
            Image scrollBackground = scrollRoot.GetComponent<Image>();
            scrollBackground.sprite = Builtin();
            scrollBackground.type = Image.Type.Sliced;
            scrollBackground.color = new Color(0.11f, 0.12f, 0.15f, 0.96f);
            scrollRoot.GetComponent<Mask>().showMaskGraphic = true;

            GameObject viewport = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            RectTransform viewportRect = Rt(viewport, scrollRoot.transform, new Vector2(0.5f, 0.5f), new Vector2(840, 820));
            Image viewportImage = viewport.GetComponent<Image>();
            viewportImage.sprite = Builtin();
            viewportImage.type = Image.Type.Sliced;
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            viewport.GetComponent<Mask>().showMaskGraphic = false;

            GameObject content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.SetParent(viewport.transform, false);
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(0f, 0f);

            GridLayoutGroup levelGrid = content.GetComponent<GridLayoutGroup>();
            levelGrid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            levelGrid.constraintCount = 3;
            levelGrid.cellSize = new Vector2(250, 86);
            levelGrid.spacing = new Vector2(18, 18);
            levelGrid.padding = new RectOffset(18, 18, 18, 18);

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

            ScrollRect levelScrollRect = scrollRoot.GetComponent<ScrollRect>();
            levelScrollRect.viewport = viewportRect;
            levelScrollRect.content = contentRect;
            levelScrollRect.horizontal = false;
            levelScrollRect.vertical = true;
            levelScrollRect.scrollSensitivity = 28f;

            Button levelButtonTemplate = Button("LevelButtonTemplate", content.transform, "Level 1", new Vector2(0.5f, 0.5f), new Vector2(250, 86), new Color(0.22f, 0.24f, 0.30f));
            levelButtonTemplate.gameObject.SetActive(false);
            levelSelectRoot.SetActive(false);

            MainMenuPresenter presenter = canvas.gameObject.AddComponent<MainMenuPresenter>();
            Ref(presenter, "energyLabel", energy);
            Ref(presenter, "hintLabel", hints);
            Ref(presenter, "levelSelectRoot", levelSelectRoot);
            Ref(presenter, "levelButtonContainer", content.transform);
            Ref(presenter, "levelButtonTemplate", levelButtonTemplate);
            Ref(presenter, "levelJumpInput", levelJumpInput);
            Ref(presenter, "levelSelectSummaryLabel", levelSelectSummary);
            Ref(presenter, "languageLabel", language);
            UnityEventTools.AddPersistentListener(play.onClick, presenter.StartCurrentProgressLevel);
            UnityEventTools.AddPersistentListener(levels.onClick, presenter.OpenLevelSelect);
            UnityEventTools.AddPersistentListener(store.onClick, presenter.OpenStore);
            UnityEventTools.AddPersistentListener(closeLevels.onClick, presenter.CloseLevelSelect);
            UnityEventTools.AddPersistentListener(jumpButton.onClick, presenter.OpenTypedLevel);
            UnityEventTools.AddPersistentListener(langTr.onClick, presenter.SetLanguageTR);
            UnityEventTools.AddPersistentListener(langEn.onClick, presenter.SetLanguageEN);
            UnityEventTools.AddPersistentListener(langEs.onClick, presenter.SetLanguageES);
            UnityEventTools.AddPersistentListener(langDe.onClick, presenter.SetLanguageDE);
            EditorSceneManager.SaveScene(scene, $"{Scenes}/{GameConstants.SceneMainMenu}.unity");
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
        private static Canvas Canvas(string name) { GameObject go = new GameObject(name, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster)); Canvas c = go.GetComponent<Canvas>(); c.renderMode = RenderMode.ScreenSpaceOverlay; CanvasScaler s = go.GetComponent<CanvasScaler>(); s.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; s.referenceResolution = new Vector2(1080, 1920); s.matchWidthOrHeight = 1f; return c; }
        private static Image Panel(string name, Transform p, Vector2 a, Vector2 size, Color c) { GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image)); Rt(go, p, a, size); Image i = go.GetComponent<Image>(); i.sprite = Builtin(); i.type = Image.Type.Sliced; i.color = c; return i; }
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
        private static Sprite Builtin() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");
        private static Sprite BuiltinRound() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/Knob.psd");
        private static TMP_FontAsset LoadDefaultTmpFont() => AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/TextMesh Pro/Resources/Fonts & Materials/LiberationSans SDF.asset");
        private static void EnsureFolder(string path) { string[] parts = path.Split('/'); string current = parts[0]; for (int i = 1; i < parts.Length; i++) { string next = current + "/" + parts[i]; if (!AssetDatabase.IsValidFolder(next)) AssetDatabase.CreateFolder(current, parts[i]); current = next; } }
        private static void Ref(Object target, string field, Object value) { SerializedObject so = new SerializedObject(target); SerializedProperty p = so.FindProperty(field); if (p != null) { p.objectReferenceValue = value; so.ApplyModifiedPropertiesWithoutUndo(); } }
        private static void Refs(Object target, string field, Object[] values) { SerializedObject so = new SerializedObject(target); SerializedProperty p = so.FindProperty(field); if (p != null && p.isArray) { p.arraySize = values.Length; for (int i = 0; i < values.Length; i++) p.GetArrayElementAtIndex(i).objectReferenceValue = values[i]; so.ApplyModifiedPropertiesWithoutUndo(); } }
        private static void String(Object target, string field, string value) { SerializedObject so = new SerializedObject(target); SerializedProperty p = so.FindProperty(field); if (p != null) { p.stringValue = value; so.ApplyModifiedPropertiesWithoutUndo(); } }

    }
}
