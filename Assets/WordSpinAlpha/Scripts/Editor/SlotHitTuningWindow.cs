using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Editor
{
    public class SlotHitTuningWindow : EditorWindow
    {
        private Vector2 _scroll;
        private bool _showSizes = true;
        private bool _showVisuals = true;
        private bool _showFeedback = true;

        private Vector2 _plaqueSize = new Vector2(0.30f, 0.18f);
        private Vector2 _perfectZoneSize = new Vector2(0.14f, 0.08f);
        private float _nearMissPadding = 0.08f;
        private float _hitBandDepth = 0.11f;
        private float _hitBandInset = 0.02f;
        private float _activationAngle = 18f;
        private Color _inactiveColor = new Color(0.88f, 0.88f, 0.94f, 0.78f);
        private Color _activeColor = new Color(1f, 0.72f, 0.28f, 1f);
        private float _activeScaleMultiplier = 1.08f;
        private Color _perfectFeedbackColor = new Color(1f, 0.93f, 0.55f, 1f);
        private Color _toleratedFeedbackColor = new Color(1f, 0.74f, 0.28f, 1f);
        private Color _failFeedbackColor = new Color(0.96f, 0.35f, 0.27f, 1f);
        private float _feedbackDuration = 0.16f;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Slot ve Hit Ayarlari")]
        public static void Open()
        {
            GetWindow<SlotHitTuningWindow>("Slot ve Hit");
        }

        private void OnEnable()
        {
            ReadFromScene();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Slot ve Hit Ayarlari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu pencere slot boyutlari, hit bantlari ve plaque geri bildirimlerini toplu ayarlar. Mekanik akisi degistirmez; sadece mevcut alanlari kontrollu sekilde yazar.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Sahneden Oku", GUILayout.Height(28f)))
                {
                    ReadFromScene();
                }

                string applyLabel = Application.isPlaying ? "Uygula" : "Tum Slotlara Uygula";
                if (GUILayout.Button(applyLabel, GUILayout.Height(28f)))
                {
                    ApplyToScene();
                }
            }

            EditorGUILayout.Space(8f);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            _showSizes = EditorGUILayout.BeginFoldoutHeaderGroup(_showSizes, "Boyut ve Hassasiyet");
            if (_showSizes)
            {
                _plaqueSize = EditorGUILayout.Vector2Field("Plaque Boyutu", _plaqueSize);
                _perfectZoneSize = EditorGUILayout.Vector2Field("Perfect Bolgesi", _perfectZoneSize);
                _nearMissPadding = EditorGUILayout.FloatField("Near Miss Boslugu", _nearMissPadding);
                _hitBandDepth = EditorGUILayout.FloatField("Hit Bant Derinligi", _hitBandDepth);
                _hitBandInset = EditorGUILayout.FloatField("Hit Bant Iceri Girisi", _hitBandInset);
                _activationAngle = EditorGUILayout.FloatField("Aktif Slot Acisi", _activationAngle);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showVisuals = EditorGUILayout.BeginFoldoutHeaderGroup(_showVisuals, "Pasif ve Aktif Gorunum");
            if (_showVisuals)
            {
                _inactiveColor = EditorGUILayout.ColorField("Pasif Renk", _inactiveColor);
                _activeColor = EditorGUILayout.ColorField("Aktif Renk", _activeColor);
                _activeScaleMultiplier = EditorGUILayout.FloatField("Aktif Buyume", _activeScaleMultiplier);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showFeedback = EditorGUILayout.BeginFoldoutHeaderGroup(_showFeedback, "Geri Bildirim");
            if (_showFeedback)
            {
                _perfectFeedbackColor = EditorGUILayout.ColorField("Perfect Rengi", _perfectFeedbackColor);
                _toleratedFeedbackColor = EditorGUILayout.ColorField("Good Rengi", _toleratedFeedbackColor);
                _failFeedbackColor = EditorGUILayout.ColorField("Hata Rengi", _failFeedbackColor);
                _feedbackDuration = EditorGUILayout.FloatField("Geri Bildirim Suresi", _feedbackDuration);
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
        }

        private void ReadFromScene()
        {
            Slot slot = FindRepresentativeSlot();
            if (slot == null)
            {
                return;
            }

            SerializedObject slotObject = new SerializedObject(slot);
            _plaqueSize = slotObject.FindProperty("plaqueSize").vector2Value;
            _perfectZoneSize = slotObject.FindProperty("perfectZoneSize").vector2Value;
            _nearMissPadding = slotObject.FindProperty("nearMissPadding").floatValue;
            _hitBandDepth = slotObject.FindProperty("hitBandDepth").floatValue;
            _hitBandInset = slotObject.FindProperty("hitBandInset").floatValue;
            _inactiveColor = slotObject.FindProperty("inactiveColor").colorValue;
            _activeColor = slotObject.FindProperty("activeColor").colorValue;
            _activeScaleMultiplier = slotObject.FindProperty("activeScaleMultiplier").floatValue;
            _perfectFeedbackColor = slotObject.FindProperty("perfectFeedbackColor").colorValue;
            _toleratedFeedbackColor = slotObject.FindProperty("toleratedFeedbackColor").colorValue;
            _failFeedbackColor = slotObject.FindProperty("failFeedbackColor").colorValue;
            _feedbackDuration = slotObject.FindProperty("feedbackDuration").floatValue;

            SlotManager manager = Object.FindObjectOfType<SlotManager>(true);
            if (manager != null)
            {
                SerializedObject managerObject = new SerializedObject(manager);
                _activationAngle = managerObject.FindProperty("activationAngle").floatValue;
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
            Slot[] slots = Object.FindObjectsOfType<Slot>(true);
            for (int i = 0; i < slots.Length; i++)
            {
                Slot slot = slots[i];
                if (slot == null)
                {
                    continue;
                }

                Undo.RecordObject(slot, "Slot Hit Tuning");
                slot.ApplyEditorTuning(
                    _plaqueSize,
                    _perfectZoneSize,
                    _nearMissPadding,
                    _hitBandDepth,
                    _hitBandInset,
                    _inactiveColor,
                    _activeColor,
                    _activeScaleMultiplier,
                    _perfectFeedbackColor,
                    _toleratedFeedbackColor,
                    _failFeedbackColor,
                    _feedbackDuration);
                EditorUtility.SetDirty(slot);
            }

            SlotManager manager = Object.FindObjectOfType<SlotManager>(true);
            if (manager != null)
            {
                SerializedObject managerObject = new SerializedObject(manager);
                managerObject.FindProperty("activationAngle").floatValue = Mathf.Clamp(_activationAngle, 1f, 90f);
                managerObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(manager);
            }

            WordSpinAlphaEditorRuntimeRefreshUtility.SaveDirtyAssets();
            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshRotatorPresentation();
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshCurrentTargetState();
            }
            else
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.MarkCurrentSceneDirty();
            }

            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.Scene);
        }

        private static Slot FindRepresentativeSlot()
        {
            Slot[] slots = Object.FindObjectsOfType<Slot>(true);
            for (int i = 0; i < slots.Length; i++)
            {
                if (slots[i] != null)
                {
                    return slots[i];
                }
            }

            return null;
        }
    }
}
