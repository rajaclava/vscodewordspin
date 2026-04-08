using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Core;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    public class QuestionFailFlowTuningWindow : EditorWindow
    {
        private int _defaultHearts = 3;
        private bool _resetCurrentHeartsOnApply = true;
        private float _targetPulseRefreshInterval = 0.05f;
        private int _previewRestoreHeartsOnContinue = 1;
        private bool _previewPremiumContinueAvailable;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Soru ve Fail Akisi Ayarlari")]
        public static void Open()
        {
            GetWindow<QuestionFailFlowTuningWindow>("Soru ve Fail");
        }

        private void OnEnable()
        {
            ReadFromScene();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Soru ve Fail Akisi Ayarlari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Soru canlarini, hedef pulse yenilemesini ve fail modal preview akisini ayarlar. Fail modal boyut, renk ve yazi yerlesimi UI Yuzey Ayarlari penceresindedir.", MessageType.Info);

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

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Soru Canlari", EditorStyles.boldLabel);
            _defaultHearts = EditorGUILayout.IntField("Varsayilan Can", _defaultHearts);
            _resetCurrentHeartsOnApply = EditorGUILayout.Toggle("Anlik Cani Resetle", _resetCurrentHeartsOnApply);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("HUD Hedef Pulse", EditorStyles.boldLabel);
            _targetPulseRefreshInterval = EditorGUILayout.FloatField("Pulse Yenileme Araligi", _targetPulseRefreshInterval);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Fail Modal Preview", EditorStyles.boldLabel);
            _previewRestoreHeartsOnContinue = EditorGUILayout.IntField("Preview Devam Can Sayisi", _previewRestoreHeartsOnContinue);
            _previewPremiumContinueAvailable = EditorGUILayout.Toggle("Preview Premium Devam", _previewPremiumContinueAvailable);

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Fail Modal Ac"))
                    {
                        GameEvents.RaiseFailModalRequested(new FailModalContext
                        {
                            levelId = 1,
                            questionIndex = 0,
                            restoreHeartsOnContinue = Mathf.Max(1, _previewRestoreHeartsOnContinue),
                            premiumContinueAvailable = _previewPremiumContinueAvailable
                        });
                    }

                    if (GUILayout.Button("Fail Modal Yenile"))
                    {
                        FailModalPresenter failModal = Object.FindObjectOfType<FailModalPresenter>(true);
                        if (failModal != null)
                        {
                            failModal.RefreshForEditor();
                        }
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Canlari Bitir"))
                    {
                        QuestionLifeManager.Instance?.Restore(0);
                    }

                    if (GUILayout.Button("Canlari Maks Yap"))
                    {
                        QuestionLifeManager.Instance?.ResetQuestionHearts();
                    }
                }
            }
        }

        private void ReadFromScene()
        {
            QuestionLifeManager questionLife = QuestionLifeManager.Instance != null
                ? QuestionLifeManager.Instance
                : Object.FindObjectOfType<QuestionLifeManager>(true);
            if (questionLife != null)
            {
                _defaultHearts = questionLife.DefaultHearts;
            }

            GameplayHudPresenter hud = Object.FindObjectOfType<GameplayHudPresenter>(true);
            if (hud != null)
            {
                SerializedObject hudObject = new SerializedObject(hud);
                _targetPulseRefreshInterval = hudObject.FindProperty("targetPulseRefreshInterval").floatValue;
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
            QuestionLifeManager questionLife = QuestionLifeManager.Instance != null
                ? QuestionLifeManager.Instance
                : Object.FindObjectOfType<QuestionLifeManager>(true);
            if (questionLife != null)
            {
                questionLife.ApplyEditorTuning(Mathf.Max(1, _defaultHearts), _resetCurrentHeartsOnApply);
                EditorUtility.SetDirty(questionLife);
            }

            GameplayHudPresenter hud = Object.FindObjectOfType<GameplayHudPresenter>(true);
            if (hud != null)
            {
                SerializedObject hudObject = new SerializedObject(hud);
                hudObject.FindProperty("targetPulseRefreshInterval").floatValue = Mathf.Clamp(_targetPulseRefreshInterval, 0.01f, 0.5f);
                hudObject.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(hud);
            }

            WordSpinAlphaEditorRuntimeRefreshUtility.SaveDirtyAssets();
            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshUiPresentation();
            }
            else
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.MarkCurrentSceneDirty();
            }

            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.Scene);
        }
    }
}
