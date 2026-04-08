using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Editor
{
    public class MobileRuntimeTuningWindow : EditorWindow
    {
        private float _extraHorizontalSafeMargin = 18f;
        private float _extraBottomSafeMargin = 12f;
        private float _extraTopSafeMargin = 8f;
        private Vector2 _referenceResolution = new Vector2(1080f, 1920f);
        private float _canvasMatch = 1f;
        private int _targetFrameRate = 60;
        private bool _forcePortrait = true;
        private bool _forceFullscreen = true;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Mobil Runtime ve Cihaz Ayarlari")]
        public static void Open()
        {
            GetWindow<MobileRuntimeTuningWindow>("Mobil Runtime");
        }

        private void OnEnable()
        {
            ReadFromScene();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Mobil Runtime ve Cihaz Ayarlari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Safe area bosluklari, canvas referans cozunurlugu ve mobil runtime davranisini tek yerden ayarlar. Play sirasinda canli apply destekler.", MessageType.Info);

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
            EditorGUILayout.LabelField("Safe Area", EditorStyles.boldLabel);
            _extraHorizontalSafeMargin = EditorGUILayout.FloatField("Yatay Ek Bosluk", _extraHorizontalSafeMargin);
            _extraBottomSafeMargin = EditorGUILayout.FloatField("Alt Ek Bosluk", _extraBottomSafeMargin);
            _extraTopSafeMargin = EditorGUILayout.FloatField("Ust Ek Bosluk", _extraTopSafeMargin);

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Canvas ve Performans", EditorStyles.boldLabel);
            _referenceResolution = EditorGUILayout.Vector2Field("Referans Cozunurluk", _referenceResolution);
            _canvasMatch = EditorGUILayout.Slider("Genislik/Yukseklik Match", _canvasMatch, 0f, 1f);
            _targetFrameRate = EditorGUILayout.IntField("Hedef FPS", _targetFrameRate);
            _forcePortrait = EditorGUILayout.Toggle("Portreyi Zorla", _forcePortrait);
            _forceFullscreen = EditorGUILayout.Toggle("Tam Ekran Zorla", _forceFullscreen);
        }

        private void ReadFromScene()
        {
            MobileRuntimeController controller = FindController();
            if (controller == null)
            {
                return;
            }

            _extraHorizontalSafeMargin = controller.ExtraHorizontalSafeMargin;
            _extraBottomSafeMargin = controller.ExtraBottomSafeMargin;
            _extraTopSafeMargin = controller.ExtraTopSafeMargin;
            _referenceResolution = controller.ReferenceResolution;
            _canvasMatch = controller.CanvasMatchWidthOrHeight;
            _targetFrameRate = controller.TargetFrameRate;
            _forcePortrait = controller.ForcePortrait;
            _forceFullscreen = controller.ForceFullscreen;
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
            MobileRuntimeController controller = FindController();
            if (controller == null)
            {
                EditorUtility.DisplayDialog("Mobil Runtime", "Sahnede MobileRuntimeController bulunamadi.", "Tamam");
                return;
            }

            Undo.RecordObject(controller, "Mobil Runtime Tuning");
            controller.ApplyEditorTuning(
                _extraHorizontalSafeMargin,
                _extraBottomSafeMargin,
                _extraTopSafeMargin,
                _referenceResolution,
                _canvasMatch,
                _targetFrameRate,
                _forcePortrait,
                _forceFullscreen);
            EditorUtility.SetDirty(controller);

            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshMobileLayout();
            }
            else
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.MarkCurrentSceneDirty();
            }

            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.Scene);
        }

        private static MobileRuntimeController FindController()
        {
            return Object.FindObjectOfType<MobileRuntimeController>(true);
        }
    }
}
