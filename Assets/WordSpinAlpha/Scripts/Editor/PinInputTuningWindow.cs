using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Editor
{
    public class PinInputTuningWindow : EditorWindow
    {
        private Vector2 _scroll;
        private bool _showFlight = true;
        private bool _showVisual = true;
        private bool _showInput = true;

        private float _loadTweenDuration = 0.30f;
        private float _pinVisualScale = 1.18f;
        private float _pinSpeed = 9.5f;
        private float _tipOffset = 0.40f;
        private float _maxTravelDistance = 7.5f;
        private Vector3 _shaftLocalPosition = new Vector3(0f, -0.38f, 0f);
        private Vector3 _shaftLocalScale = new Vector3(0.11f, 0.74f, 1f);
        private Vector3 _letterLocalPosition = new Vector3(0f, -0.80f, -0.1f);
        private float _letterCharacterSize = 0.10f;
        private int _letterFontSize = 52;
        private Vector3 _ringLocalScale = new Vector3(0.28f, 0.28f, 1f);
        private Vector3 _coreLocalScale = new Vector3(0.20f, 0.20f, 1f);
        private Vector3 _sheenLocalPosition = new Vector3(-0.02f, 0.04f, 0f);
        private Vector3 _sheenLocalScale = new Vector3(0.08f, 0.08f, 1f);
        private Color _ringColor = new Color(0.24f, 0.18f, 0.12f, 0.96f);
        private Color _coreColor = new Color(1f, 1f, 1f, 1f);
        private Color _sheenColor = new Color(1f, 1f, 1f, 0.55f);
        private Color _shaftColor = new Color(0.24f, 0.18f, 0.12f, 0.96f);
        private bool _allowPhysicalKeyboard = true;
        private float _minSwipeDistance = 30f;
        private int _bufferCapacity = 3;
        private float _launchCooldown = 0.3f;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Pin ve Input Ayarlari")]
        public static void Open()
        {
            GetWindow<PinInputTuningWindow>("Pin ve Input");
        }

        private void OnEnable()
        {
            ReadFromScene();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Pin ve Input Ayarlari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu pencere pin ucusunu, pin gorsel iskeletini ve input esiklerini birlikte ayarlar. Play sirasinda canli apply destekler.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Sahneden Oku", GUILayout.Height(28f)))
                {
                    ReadFromScene();
                }

                if (GUILayout.Button("Uygula", GUILayout.Height(28f)))
                {
                    ApplyToScene();
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            _showFlight = EditorGUILayout.BeginFoldoutHeaderGroup(_showFlight, "Pin Ucusu");
            if (_showFlight)
            {
                _loadTweenDuration = EditorGUILayout.FloatField("Yukleme Tween Suresi", _loadTweenDuration);
                _pinVisualScale = EditorGUILayout.FloatField("Yuklu Pin Olcegi", _pinVisualScale);
                _pinSpeed = EditorGUILayout.FloatField("Pin Hiz", _pinSpeed);
                _tipOffset = EditorGUILayout.FloatField("Pin Uc Ofseti", _tipOffset);
                _maxTravelDistance = EditorGUILayout.FloatField("Maks Ucus Mesafesi", _maxTravelDistance);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showVisual = EditorGUILayout.BeginFoldoutHeaderGroup(_showVisual, "Pin Gorsel Iskeleti");
            if (_showVisual)
            {
                _shaftLocalPosition = EditorGUILayout.Vector3Field("Govde Pozisyonu", _shaftLocalPosition);
                _shaftLocalScale = EditorGUILayout.Vector3Field("Govde Olcegi", _shaftLocalScale);
                _letterLocalPosition = EditorGUILayout.Vector3Field("Harf Pozisyonu", _letterLocalPosition);
                _letterCharacterSize = EditorGUILayout.FloatField("Harf Karakter Boyutu", _letterCharacterSize);
                _letterFontSize = EditorGUILayout.IntField("Harf Font Boyutu", _letterFontSize);
                _ringLocalScale = EditorGUILayout.Vector3Field("Dis Halka Olcegi", _ringLocalScale);
                _coreLocalScale = EditorGUILayout.Vector3Field("Merkez Olcegi", _coreLocalScale);
                _sheenLocalPosition = EditorGUILayout.Vector3Field("Parlama Pozisyonu", _sheenLocalPosition);
                _sheenLocalScale = EditorGUILayout.Vector3Field("Parlama Olcegi", _sheenLocalScale);
                _ringColor = EditorGUILayout.ColorField("Dis Halka Rengi", _ringColor);
                _coreColor = EditorGUILayout.ColorField("Merkez Rengi", _coreColor);
                _sheenColor = EditorGUILayout.ColorField("Parlama Rengi", _sheenColor);
                _shaftColor = EditorGUILayout.ColorField("Govde Rengi", _shaftColor);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showInput = EditorGUILayout.BeginFoldoutHeaderGroup(_showInput, "Input ve Atesleme");
            if (_showInput)
            {
                _allowPhysicalKeyboard = EditorGUILayout.Toggle("Fiziksel Klavye Acik", _allowPhysicalKeyboard);
                _minSwipeDistance = EditorGUILayout.FloatField("Min Swipe Mesafesi", _minSwipeDistance);
                _bufferCapacity = EditorGUILayout.IntField("Input Buffer Kapasitesi", _bufferCapacity);
                _launchCooldown = EditorGUILayout.FloatField("Atesleme Cooldown", _launchCooldown);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
        }

        private void ReadFromScene()
        {
            PinLauncher launcher = Object.FindObjectOfType<PinLauncher>(true);
            if (launcher != null)
            {
                SerializedObject launcherObject = new SerializedObject(launcher);
                _loadTweenDuration = launcherObject.FindProperty("loadTweenDuration").floatValue;
                _pinVisualScale = launcherObject.FindProperty("pinVisualScale").floatValue;
            }

            Pin pinTemplate = ResolvePinTemplate(launcher);
            if (pinTemplate != null)
            {
                SerializedObject pinObject = new SerializedObject(pinTemplate);
                _pinSpeed = pinObject.FindProperty("speed").floatValue;
                _tipOffset = pinObject.FindProperty("tipOffset").floatValue;
                _maxTravelDistance = pinObject.FindProperty("maxTravelDistance").floatValue;
                _shaftLocalPosition = pinObject.FindProperty("shaftLocalPosition").vector3Value;
                _shaftLocalScale = pinObject.FindProperty("shaftLocalScale").vector3Value;
                _letterLocalPosition = pinObject.FindProperty("letterLocalPosition").vector3Value;
                _letterCharacterSize = pinObject.FindProperty("letterCharacterSize").floatValue;
                _letterFontSize = pinObject.FindProperty("letterFontSize").intValue;
                _ringLocalScale = pinObject.FindProperty("ringLocalScale").vector3Value;
                _coreLocalScale = pinObject.FindProperty("coreLocalScale").vector3Value;
                _sheenLocalPosition = pinObject.FindProperty("sheenLocalPosition").vector3Value;
                _sheenLocalScale = pinObject.FindProperty("sheenLocalScale").vector3Value;
                ReadRendererColors(pinTemplate);
            }

            InputManager input = InputManager.Instance != null ? InputManager.Instance : Object.FindObjectOfType<InputManager>(true);
            if (input != null)
            {
                SerializedObject inputObject = new SerializedObject(input);
                _allowPhysicalKeyboard = inputObject.FindProperty("allowPhysicalKeyboard").boolValue;
                _minSwipeDistance = inputObject.FindProperty("minSwipeDistance").floatValue;
            }

            InputBuffer buffer = Object.FindObjectOfType<InputBuffer>(true);
            if (buffer != null)
            {
                SerializedObject bufferObject = new SerializedObject(buffer);
                _bufferCapacity = bufferObject.FindProperty("maxCapacity").intValue;
            }

            FireGate gate = Object.FindObjectOfType<FireGate>(true);
            if (gate != null)
            {
                SerializedObject gateObject = new SerializedObject(gate);
                _launchCooldown = gateObject.FindProperty("launchCooldown").floatValue;
            }
        }

        private void TryAutoRefresh()
        {
            if (!WordSpinAlphaEditorSyncUtility.ConsumeChanges(WordSpinAlphaEditorSyncKind.Scene, ref _syncStamp))
            {
                return;
            }

            ReadFromScene();
            Repaint();
        }

        private void ApplyToScene()
        {
            PinLauncher launcher = Object.FindObjectOfType<PinLauncher>(true);
            if (launcher != null)
            {
                Undo.RecordObject(launcher, "Pin Launcher Tuning");
                launcher.ApplyTuning(_loadTweenDuration, _pinVisualScale);
                EditorUtility.SetDirty(launcher);
            }

            Pin pinTemplate = ResolvePinTemplate(launcher);
            if (pinTemplate != null)
            {
                Undo.RecordObject(pinTemplate, "Pin Template Tuning");
                pinTemplate.ApplyEditorFlightTuning(_pinSpeed, _tipOffset, _maxTravelDistance);
                pinTemplate.ApplyEditorVisualTuning(
                    _shaftLocalPosition,
                    _shaftLocalScale,
                    _letterLocalPosition,
                    _letterCharacterSize,
                    _letterFontSize,
                    _ringLocalScale,
                    _coreLocalScale,
                    _sheenLocalPosition,
                    _sheenLocalScale);
                ApplyRendererColors(pinTemplate);
                EditorUtility.SetDirty(pinTemplate);
            }

            Pin[] livePins = Object.FindObjectsOfType<Pin>(true);
            for (int i = 0; i < livePins.Length; i++)
            {
                if (livePins[i] == null)
                {
                    continue;
                }

                livePins[i].ApplyEditorFlightTuning(_pinSpeed, _tipOffset, _maxTravelDistance);
                livePins[i].ApplyEditorVisualTuning(
                    _shaftLocalPosition,
                    _shaftLocalScale,
                    _letterLocalPosition,
                    _letterCharacterSize,
                    _letterFontSize,
                    _ringLocalScale,
                    _coreLocalScale,
                    _sheenLocalPosition,
                    _sheenLocalScale);
                ApplyRendererColors(livePins[i]);
                EditorUtility.SetDirty(livePins[i]);
            }

            ApplyInputFields();
            WordSpinAlphaEditorRuntimeRefreshUtility.SaveDirtyAssets();
            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshCurrentTargetState();
            }
            else
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.MarkCurrentSceneDirty();
            }

            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.Scene);
        }

        private void ApplyInputFields()
        {
            InputManager input = InputManager.Instance != null ? InputManager.Instance : Object.FindObjectOfType<InputManager>(true);
            if (input != null)
            {
                SerializedObject inputObject = new SerializedObject(input);
                inputObject.FindProperty("allowPhysicalKeyboard").boolValue = _allowPhysicalKeyboard;
                inputObject.FindProperty("minSwipeDistance").floatValue = Mathf.Clamp(_minSwipeDistance, 1f, 300f);
                inputObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(input);
            }

            InputBuffer buffer = Object.FindObjectOfType<InputBuffer>(true);
            if (buffer != null)
            {
                SerializedObject bufferObject = new SerializedObject(buffer);
                bufferObject.FindProperty("maxCapacity").intValue = Mathf.Clamp(_bufferCapacity, 1, 12);
                bufferObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(buffer);
            }

            FireGate gate = Object.FindObjectOfType<FireGate>(true);
            if (gate != null)
            {
                SerializedObject gateObject = new SerializedObject(gate);
                gateObject.FindProperty("launchCooldown").floatValue = Mathf.Clamp(_launchCooldown, 0.01f, 2f);
                gateObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(gate);
            }
        }

        private void ReadRendererColors(Pin pin)
        {
            SpriteRenderer ring = FindRenderer(pin.transform, "OuterRing");
            SpriteRenderer core = FindRenderer(pin.transform, "Core");
            SpriteRenderer sheen = FindRenderer(pin.transform, "Sheen");
            SpriteRenderer shaft = FindRenderer(pin.transform, "Shaft");
            if (ring != null) _ringColor = ring.color;
            if (core != null) _coreColor = core.color;
            if (sheen != null) _sheenColor = sheen.color;
            if (shaft != null) _shaftColor = shaft.color;
        }

        private void ApplyRendererColors(Pin pin)
        {
            SpriteRenderer ring = FindRenderer(pin.transform, "OuterRing");
            SpriteRenderer core = FindRenderer(pin.transform, "Core");
            SpriteRenderer sheen = FindRenderer(pin.transform, "Sheen");
            SpriteRenderer shaft = FindRenderer(pin.transform, "Shaft");
            if (ring != null) ring.color = _ringColor;
            if (core != null) core.color = _coreColor;
            if (sheen != null) sheen.color = _sheenColor;
            if (shaft != null) shaft.color = _shaftColor;
        }

        private static Pin ResolvePinTemplate(PinLauncher launcher)
        {
            if (launcher != null && launcher.PinTemplate != null)
            {
                return launcher.PinTemplate;
            }

            Pin[] pins = Resources.FindObjectsOfTypeAll<Pin>();
            for (int i = 0; i < pins.Length; i++)
            {
                if (pins[i] != null && !pins[i].gameObject.scene.IsValid())
                {
                    return pins[i];
                }
            }

            return Object.FindObjectOfType<Pin>(true);
        }

        private static SpriteRenderer FindRenderer(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            Transform child = root.Find(childName);
            return child != null ? child.GetComponent<SpriteRenderer>() : null;
        }
    }
}
