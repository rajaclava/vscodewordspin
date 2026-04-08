using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    public class WordSpinAlphaUnifiedEditorWindow : EditorWindow
    {
        private const float OuterPadding = 8f;
        private const float HeaderHeight = 104f;
        private const float SidebarWidth = 248f;
        private const float SectionSpacing = 8f;
        private const float PanelGap = 8f;
        private const float PanelPadding = 10f;

        private readonly List<ModuleSection> _sections = new List<ModuleSection>();
        private readonly Dictionary<string, EmbeddedEditorWindowHost> _windowHosts = new Dictionary<string, EmbeddedEditorWindowHost>(StringComparer.Ordinal);
        private string _selectedModuleId;
        private Vector2 _sidebarScroll;
        private Vector2 _contentScroll;
        private UnityEditor.Editor _gameplaySceneTunerEditor;
        private GameplaySceneTuner _cachedSceneTuner;

        [MenuItem("Tools/WordSpin Alpha/Toplu Tek Editor")]
        public static void Open()
        {
            GetWindow<WordSpinAlphaUnifiedEditorWindow>("WordSpin Toplu Editor");
        }

        private void OnEnable()
        {
            BuildSections();
            if (string.IsNullOrWhiteSpace(_selectedModuleId))
            {
                _selectedModuleId = "overview";
            }
        }

        private void OnDisable()
        {
            foreach (EmbeddedEditorWindowHost host in _windowHosts.Values)
            {
                host.Dispose();
            }

            _windowHosts.Clear();

            if (_gameplaySceneTunerEditor != null)
            {
                DestroyImmediate(_gameplaySceneTunerEditor);
                _gameplaySceneTunerEditor = null;
            }
        }

        private void OnGUI()
        {
            if (_sections.Count == 0)
            {
                BuildSections();
            }

            Rect headerRect = new Rect(OuterPadding, OuterPadding, position.width - (OuterPadding * 2f), HeaderHeight);
            Rect mainRect = new Rect(
                OuterPadding,
                headerRect.yMax + PanelGap,
                position.width - (OuterPadding * 2f),
                position.height - headerRect.height - (OuterPadding * 2f) - PanelGap);

            float contentWidth = Mathf.Max(220f, mainRect.width - SidebarWidth - PanelGap);
            Rect sidebarRect = new Rect(mainRect.x, mainRect.y, SidebarWidth, mainRect.height);
            Rect contentRect = new Rect(sidebarRect.xMax + PanelGap, mainRect.y, contentWidth, mainRect.height);

            DrawHeader(headerRect);
            DrawSidebar(sidebarRect);
            DrawContent(contentRect);
        }

        private void DrawHeader(Rect headerRect)
        {
            GUI.Box(headerRect, GUIContent.none);
            Rect innerRect = new Rect(headerRect.x + PanelPadding, headerRect.y + PanelPadding, headerRect.width - (PanelPadding * 2f), headerRect.height - (PanelPadding * 2f));

            GUILayout.BeginArea(innerRect);
            EditorGUILayout.LabelField("WordSpin Alpha Toplu Tek Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Tum tuning, icerik, validation, telemetry ve operasyon yuzeyleri burada tek shell icinde toplanir. Mevcut alt editorler bozulmadan host edilir.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Tum Modulleri Yenile", GUILayout.Height(28f), GUILayout.Width(180f)))
                {
                    ForceReinitializeHosts();
                }

                if (GUILayout.Button("Aktif Modulu Ayri Pencerede Ac", GUILayout.Height(28f), GUILayout.Width(220f)))
                {
                    OpenActiveStandaloneWindow();
                }

                GUILayout.FlexibleSpace();
            }
            GUILayout.EndArea();
        }

        private void DrawSidebar(Rect sidebarRect)
        {
            GUI.Box(sidebarRect, GUIContent.none);

            Rect contentRect = new Rect(sidebarRect.x + PanelPadding, sidebarRect.y + PanelPadding, sidebarRect.width - (PanelPadding * 2f), sidebarRect.height - (PanelPadding * 2f));
            GUILayout.BeginArea(contentRect);
            _sidebarScroll = EditorGUILayout.BeginScrollView(_sidebarScroll);

            for (int i = 0; i < _sections.Count; i++)
            {
                ModuleSection section = _sections[i];
                EditorGUILayout.LabelField(section.title, EditorStyles.boldLabel);
                EditorGUILayout.Space(2f);

                for (int j = 0; j < section.modules.Length; j++)
                {
                    ModuleDescriptor module = section.modules[j];
                    DrawSidebarButton(module);
                }

                EditorGUILayout.Space(SectionSpacing);
            }

            EditorGUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawSidebarButton(ModuleDescriptor module)
        {
            GUIStyle style = new GUIStyle(EditorStyles.miniButton)
            {
                alignment = TextAnchor.MiddleLeft,
                fixedHeight = 28f,
                wordWrap = true,
                fontStyle = _selectedModuleId == module.id ? FontStyle.Bold : FontStyle.Normal
            };

            Color previous = GUI.backgroundColor;
            GUI.backgroundColor = _selectedModuleId == module.id
                ? new Color(0.28f, 0.42f, 0.60f)
                : new Color(0.24f, 0.24f, 0.24f);

            if (GUILayout.Button(module.title, style))
            {
                _selectedModuleId = module.id;
            }

            GUI.backgroundColor = previous;
        }

        private void DrawContent(Rect contentRect)
        {
            GUI.Box(contentRect, GUIContent.none);

            Rect bodyRect = new Rect(contentRect.x + PanelPadding, contentRect.y + PanelPadding, contentRect.width - (PanelPadding * 2f), contentRect.height - (PanelPadding * 2f));
            ModuleDescriptor module = FindModule(_selectedModuleId);

            if (module == null || module.kind == ModuleKind.Overview || module.kind == ModuleKind.GameplaySceneTuner || module.kind == ModuleKind.Toolbox)
            {
                GUILayout.BeginArea(bodyRect);
                _contentScroll = EditorGUILayout.BeginScrollView(_contentScroll);

                if (module == null || module.kind == ModuleKind.Overview)
                {
                    DrawOverview();
                }
                else if (module.kind == ModuleKind.GameplaySceneTuner)
                {
                    DrawGameplaySceneTunerModule(bodyRect);
                }
                else
                {
                    DrawToolboxModule();
                }

                EditorGUILayout.EndScrollView();
                GUILayout.EndArea();
                return;
            }

            GUILayout.BeginArea(bodyRect);
            DrawHostedWindowModule(module, bodyRect);
            GUILayout.EndArea();
        }

        private void DrawOverview()
        {
            EditorGUILayout.LabelField("Genel Bakis", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Sol panelden bir modul sec. Icerik, shape, ekonomi, mobile, UI, fail, theme, telemetry ve hotfix yuzeyleri bu shell icinden ayarlanabilir.", MessageType.None);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Hazir Moduller", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("- Icerik ve level");
            EditorGUILayout.LabelField("- Gameplay layout");
            EditorGUILayout.LabelField("- Klavye ve mobile runtime");
            EditorGUILayout.LabelField("- Slot, hit, pin, input");
            EditorGUILayout.LabelField("- Ritim, fail, UI yuzeyleri");
            EditorGUILayout.LabelField("- Ekonomi, store, tema");
            EditorGUILayout.LabelField("- Ambiyans/pulse, validation, debug");
            EditorGUILayout.LabelField("- Telemetry politikasi ve uzak icerik hotfix");
        }

        private void DrawGameplaySceneTunerModule(Rect hostRect)
        {
            EditorGUILayout.LabelField("Gameplay Layout", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu modul sahnedeki GameplaySceneTuner component'ini dogrudan duzenler.", MessageType.Info);

            GameplaySceneTuner tuner = UnityEngine.Object.FindObjectOfType<GameplaySceneTuner>(true);
            if (tuner == null)
            {
                EditorGUILayout.HelpBox("Sahnede GameplaySceneTuner bulunamadi.", MessageType.Warning);
                return;
            }

            if (_cachedSceneTuner != tuner || _gameplaySceneTunerEditor == null)
            {
                if (_gameplaySceneTunerEditor != null)
                {
                    DestroyImmediate(_gameplaySceneTunerEditor);
                }

                _cachedSceneTuner = tuner;
                _gameplaySceneTunerEditor = UnityEditor.Editor.CreateEditor(tuner, typeof(GameplaySceneTunerEditor));
            }

            if (_gameplaySceneTunerEditor != null)
            {
                _gameplaySceneTunerEditor.OnInspectorGUI();
            }
        }

        private void DrawHostedWindowModule(ModuleDescriptor module, Rect hostRect)
        {
            EmbeddedEditorWindowHost host = GetOrCreateHost(module);
            if (host == null)
            {
                EditorGUILayout.HelpBox($"Modul acilamadi: {module.title}", MessageType.Error);
                return;
            }

            host.Draw(new Rect(0f, 0f, hostRect.width, hostRect.height));
        }

        private void DrawToolboxModule()
        {
            EditorGUILayout.LabelField("Sistem Araclari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu panel build, scene rebuild ve icerik generate komutlarini tek yerden cagirir.", MessageType.Info);

            DrawExecuteMenuButton("USB Cihaza APK Build ve Run", "Tools/WordSpin Alpha/Android/Build And Run APK (USB Device)");
            DrawExecuteMenuButton("APK Build", "Tools/WordSpin Alpha/Android/Build APK (Device Test)");
            DrawExecuteMenuButton("USB Cihazlari Kontrol Et", "Tools/WordSpin Alpha/Android/Check Connected USB Devices");
            DrawExecuteMenuButton("ADB Restart", "Tools/WordSpin Alpha/Android/Restart ADB Server");
            DrawExecuteMenuButton("Build Klasorunu Ac", "Tools/WordSpin Alpha/Android/Open Build Folder");

            EditorGUILayout.Space(8f);
            DrawExecuteMenuButton("Scene Rebuild", "Tools/WordSpin Alpha/Rebuild Scenes");
            DrawExecuteMenuButton("Generated Scene Reset", "Tools/WordSpin Alpha/Force Reset Generated Scenes");
            DrawExecuteMenuButton("Rotator Shape Prefablerini Rebuild Et", "Tools/WordSpin Alpha/Rebuild Rotator Shape Prefabs");

            EditorGUILayout.Space(8f);
            DrawExecuteMenuButton("Sorulardan Turkce Levelleri Generate Et", "Tools/WordSpin Alpha/Generate Levels/For Turkish");
            DrawExecuteMenuButton("Sorulardan Ingilizce Levelleri Generate Et", "Tools/WordSpin Alpha/Generate Levels/For English");
            DrawExecuteMenuButton("Sorulardan Ispanyolca Levelleri Generate Et", "Tools/WordSpin Alpha/Generate Levels/For Spanish");
            DrawExecuteMenuButton("Sorulardan Almanca Levelleri Generate Et", "Tools/WordSpin Alpha/Generate Levels/For German");
        }

        private static void DrawExecuteMenuButton(string label, string menuPath)
        {
            if (GUILayout.Button(label, GUILayout.Height(28f)))
            {
                EditorApplication.ExecuteMenuItem(menuPath);
            }
        }

        private EmbeddedEditorWindowHost GetOrCreateHost(ModuleDescriptor module)
        {
            if (_windowHosts.TryGetValue(module.id, out EmbeddedEditorWindowHost existing))
            {
                return existing;
            }

            if (module.windowType == null)
            {
                return null;
            }

            EmbeddedEditorWindowHost host = new EmbeddedEditorWindowHost(module.windowType);
            _windowHosts[module.id] = host;
            return host;
        }

        private void ForceReinitializeHosts()
        {
            foreach (EmbeddedEditorWindowHost host in _windowHosts.Values)
            {
                host.Reinitialize();
            }

            if (_gameplaySceneTunerEditor != null)
            {
                DestroyImmediate(_gameplaySceneTunerEditor);
                _gameplaySceneTunerEditor = null;
                _cachedSceneTuner = null;
            }
        }

        private void OpenActiveStandaloneWindow()
        {
            ModuleDescriptor module = FindModule(_selectedModuleId);
            if (module == null)
            {
                return;
            }

            switch (_selectedModuleId)
            {
                case "overview":
                    return;
                case "layout_gameplay":
                    Selection.activeObject = UnityEngine.Object.FindObjectOfType<GameplaySceneTuner>(true);
                    return;
                case "content":
                    WordSpinAlphaContentEditorWindow.Open();
                    return;
                case "economy":
                    EconomyBalanceWindow.Open();
                    return;
                case "theme_store":
                    ThemeStoreConfigWindow.Open();
                    return;
                case "theme_preview":
                    ThemePackagePreviewWindow.Open();
                    return;
                case "theme_ambient":
                    AmbientPulseTuningWindow.Open();
                    return;
                case "keyboard":
                    KeyboardLayoutTuningWindow.Open();
                    return;
                case "mobile":
                    MobileRuntimeTuningWindow.Open();
                    return;
                case "slot_hit":
                    SlotHitTuningWindow.Open();
                    return;
                case "pin_input":
                    PinInputTuningWindow.Open();
                    return;
                case "rotator_rhythm":
                    RotatorRhythmTuningWindow.Open();
                    return;
                case "fail_flow":
                    QuestionFailFlowTuningWindow.Open();
                    return;
                case "ui_surfaces":
                    UiSurfaceTuningWindow.Open();
                    return;
                case "feel_visual":
                    FeelVisualTuningWindow.Open();
                    return;
                case "validation":
                    ValidationAuditWindow.Open();
                    return;
                case "save_debug":
                    SaveSessionDebugWindow.Open();
                    return;
                case "telemetry_debug":
                    DeveloperTelemetryWindow.Open();
                    return;
                case "telemetry_policy":
                    TelemetryPolicyWindow.Open();
                    return;
                case "remote_hotfix":
                    RemoteContentHotfixWindow.Open();
                    return;
            }
        }

        private ModuleDescriptor FindModule(string id)
        {
            for (int i = 0; i < _sections.Count; i++)
            {
                ModuleDescriptor[] modules = _sections[i].modules;
                for (int j = 0; j < modules.Length; j++)
                {
                    if (modules[j].id == id)
                    {
                        return modules[j];
                    }
                }
            }

            return null;
        }

        private void BuildSections()
        {
            _sections.Clear();

            _sections.Add(new ModuleSection("Genel", new[]
            {
                new ModuleDescriptor("overview", "Genel Bakis", ModuleKind.Overview, null)
            }));

            _sections.Add(new ModuleSection("Icerik", new[]
            {
                new ModuleDescriptor("content", "Icerik, Level ve Sekil", ModuleKind.HostedWindow, typeof(WordSpinAlphaContentEditorWindow))
            }));

            _sections.Add(new ModuleSection("Gameplay", new[]
            {
                new ModuleDescriptor("layout_gameplay", "Gameplay Layout", ModuleKind.GameplaySceneTuner, null),
                new ModuleDescriptor("slot_hit", "Slot ve Hit", ModuleKind.HostedWindow, typeof(SlotHitTuningWindow)),
                new ModuleDescriptor("pin_input", "Pin ve Input", ModuleKind.HostedWindow, typeof(PinInputTuningWindow)),
                new ModuleDescriptor("rotator_rhythm", "Ritmik Donus ve Zorluk", ModuleKind.HostedWindow, typeof(RotatorRhythmTuningWindow)),
                new ModuleDescriptor("fail_flow", "Soru ve Fail Akisi", ModuleKind.HostedWindow, typeof(QuestionFailFlowTuningWindow))
            }));

            _sections.Add(new ModuleSection("Arayuz ve Mobil", new[]
            {
                new ModuleDescriptor("ui_surfaces", "UI Yuzeyleri", ModuleKind.HostedWindow, typeof(UiSurfaceTuningWindow)),
                new ModuleDescriptor("keyboard", "Klavye Yerlesimi", ModuleKind.HostedWindow, typeof(KeyboardLayoutTuningWindow)),
                new ModuleDescriptor("mobile", "Mobil Runtime ve Cihaz", ModuleKind.HostedWindow, typeof(MobileRuntimeTuningWindow))
            }));

            _sections.Add(new ModuleSection("Tema ve Gorsel", new[]
            {
                new ModuleDescriptor("feel_visual", "Hissiyat ve Gorsel", ModuleKind.HostedWindow, typeof(FeelVisualTuningWindow)),
                new ModuleDescriptor("theme_store", "Tema ve Magaza Config", ModuleKind.HostedWindow, typeof(ThemeStoreConfigWindow)),
                new ModuleDescriptor("theme_preview", "Tema Paketi Onizleme", ModuleKind.HostedWindow, typeof(ThemePackagePreviewWindow)),
                new ModuleDescriptor("theme_ambient", "Ambiyans ve Pulse", ModuleKind.HostedWindow, typeof(AmbientPulseTuningWindow))
            }));

            _sections.Add(new ModuleSection("Ekonomi ve Operasyon", new[]
            {
                new ModuleDescriptor("economy", "Ekonomi ve Sandbox", ModuleKind.HostedWindow, typeof(EconomyBalanceWindow)),
                new ModuleDescriptor("telemetry_debug", "Gelistirici Telemetry", ModuleKind.HostedWindow, typeof(DeveloperTelemetryWindow)),
                new ModuleDescriptor("telemetry_policy", "Telemetry Politikasi", ModuleKind.HostedWindow, typeof(TelemetryPolicyWindow)),
                new ModuleDescriptor("remote_hotfix", "Uzak Icerik ve Hotfix", ModuleKind.HostedWindow, typeof(RemoteContentHotfixWindow))
            }));

            _sections.Add(new ModuleSection("Sistem", new[]
            {
                new ModuleDescriptor("validation", "Validation ve Referans Taramasi", ModuleKind.HostedWindow, typeof(ValidationAuditWindow)),
                new ModuleDescriptor("save_debug", "Save ve Session Paneli", ModuleKind.HostedWindow, typeof(SaveSessionDebugWindow)),
                new ModuleDescriptor("toolbox", "Build, Generate ve Sistem Araclari", ModuleKind.Toolbox, null)
            }));
        }

        private sealed class EmbeddedEditorWindowHost : IDisposable
        {
            private readonly Type _windowType;
            private readonly MethodInfo _onEnableMethod;
            private readonly MethodInfo _onGuiMethod;
            private EditorWindow _instance;

            public EmbeddedEditorWindowHost(Type windowType)
            {
                _windowType = windowType;
                _onEnableMethod = _windowType.GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                _onGuiMethod = _windowType.GetMethod("OnGUI", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                Reinitialize();
            }

            public void Reinitialize()
            {
                DisposeInstance();
                _instance = ScriptableObject.CreateInstance(_windowType) as EditorWindow;
                _onEnableMethod?.Invoke(_instance, null);
            }

            public void Draw(Rect bounds)
            {
                if (_instance == null)
                {
                    Reinitialize();
                }

                if (_instance == null || _onGuiMethod == null)
                {
                    return;
                }

                _instance.position = bounds;
                _onGuiMethod.Invoke(_instance, null);
            }

            public void Dispose()
            {
                DisposeInstance();
            }

            private void DisposeInstance()
            {
                if (_instance != null)
                {
                    DestroyImmediate(_instance);
                    _instance = null;
                }
            }
        }

        private enum ModuleKind
        {
            Overview,
            HostedWindow,
            GameplaySceneTuner,
            Toolbox
        }

        private sealed class ModuleSection
        {
            public ModuleSection(string title, ModuleDescriptor[] modules)
            {
                this.title = title;
                this.modules = modules;
            }

            public readonly string title;
            public readonly ModuleDescriptor[] modules;
        }

        private sealed class ModuleDescriptor
        {
            public ModuleDescriptor(string id, string title, ModuleKind kind, Type windowType)
            {
                this.id = id;
                this.title = title;
                this.kind = kind;
                this.windowType = windowType;
            }

            public readonly string id;
            public readonly string title;
            public readonly ModuleKind kind;
            public readonly Type windowType;
        }
    }
}
