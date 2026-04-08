using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using WordSpinAlpha.Presentation;

namespace WordSpinAlpha.Editor
{
    public class UiSurfaceTuningWindow : EditorWindow
    {
        private Vector2 _scroll;
        private bool _showGameplayHud = true;
        private bool _showFailModal = true;
        private bool _showInfoCard = true;
        private bool _showResult = true;
        private bool _showStore = true;
        private bool _showMainMenu = true;
        private bool _showMembership = true;

        private GameplayHudPresenter _gameplayHud;
        private FailModalPresenter _failModal;
        private InfoCardPresenter _infoCard;
        private ResultPresenter _result;
        private StorePresenter _store;
        private MainMenuPresenter _mainMenu;
        private MembershipPresenter _membership;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/UI Yuzey Ayarlari")]
        public static void Open()
        {
            GetWindow<UiSurfaceTuningWindow>("UI Yuzey");
        }

        private void OnEnable()
        {
            ResolveTargets();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("UI Yuzey Ayarlari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("HUD, fail modal, info card, result, store ve main menu yuzeylerini tek yerden ayarlar. Bu pencere sadece mevcut sahnede bulunan presenter referanslarini duzenler.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Sahneyi Tara", GUILayout.Height(28f)))
                {
                    ResolveTargets();
                }

                if (GUILayout.Button("Uygula", GUILayout.Height(28f)))
                {
                    ApplyRefresh();
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            _showGameplayHud = EditorGUILayout.BeginFoldoutHeaderGroup(_showGameplayHud, "Gameplay HUD");
            if (_showGameplayHud)
            {
                DrawGameplayHud();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _showFailModal = EditorGUILayout.BeginFoldoutHeaderGroup(_showFailModal, "Fail Modal");
            if (_showFailModal)
            {
                DrawFailModal();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _showInfoCard = EditorGUILayout.BeginFoldoutHeaderGroup(_showInfoCard, "Info Card");
            if (_showInfoCard)
            {
                DrawInfoCard();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _showResult = EditorGUILayout.BeginFoldoutHeaderGroup(_showResult, "Result Ekrani");
            if (_showResult)
            {
                DrawResult();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _showStore = EditorGUILayout.BeginFoldoutHeaderGroup(_showStore, "Store");
            if (_showStore)
            {
                DrawStore();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _showMainMenu = EditorGUILayout.BeginFoldoutHeaderGroup(_showMainMenu, "Main Menu");
            if (_showMainMenu)
            {
                DrawMainMenu();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            _showMembership = EditorGUILayout.BeginFoldoutHeaderGroup(_showMembership, "Membership");
            if (_showMembership)
            {
                DrawMembership();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
        }

        private void ResolveTargets()
        {
            _gameplayHud = Object.FindObjectOfType<GameplayHudPresenter>(true);
            _failModal = Object.FindObjectOfType<FailModalPresenter>(true);
            _infoCard = Object.FindObjectOfType<InfoCardPresenter>(true);
            _result = Object.FindObjectOfType<ResultPresenter>(true);
            _store = Object.FindObjectOfType<StorePresenter>(true);
            _mainMenu = Object.FindObjectOfType<MainMenuPresenter>(true);
            _membership = Object.FindObjectOfType<MembershipPresenter>(true);
        }

        private void TryAutoRefresh()
        {
            if (!WordSpinAlphaEditorSyncUtility.ConsumeChanges(WordSpinAlphaEditorSyncKind.Scene, ref _syncStamp))
            {
                return;
            }

            ResolveTargets();
            Repaint();
        }

        private void DrawGameplayHud()
        {
            if (_gameplayHud == null)
            {
                EditorGUILayout.HelpBox("Sahnede GameplayHudPresenter bulunamadi.", MessageType.Warning);
                return;
            }

            SerializedObject hud = new SerializedObject(_gameplayHud);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Soru Metni", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(hud, "questionLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Cevap Metni", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(hud, "answerLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Can Etiketi", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(hud, "heartsLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Hedef Ipuclari", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(hud, "targetHintLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Test Cevap", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(hud, "debugAnswerLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Puan", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(hud, "scoreLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Carpan", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(hud, "multiplierLabel"));
            EditorGUILayout.Space(4f);
            DrawAnswerBoxColors(hud);
            EditorGUILayout.Space(4f);

            Button coinHookButton = WordSpinAlphaEditorUiTuningUtility.ResolveReference<Button>(hud, "coinHookButton");
            TextMeshProUGUI coinHookTitle = WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(hud, "coinHookTitleLabel");
            TextMeshProUGUI coinHookValue = WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(hud, "coinHookValueLabel");
            WordSpinAlphaEditorUiTuningUtility.DrawButtonBlock("Coin Hook", coinHookButton, coinHookTitle);
            if (coinHookValue != null)
            {
                EditorGUILayout.Space(4f);
                WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Coin Hook Degeri", coinHookValue, includeRect: false);
            }
        }

        private static void DrawAnswerBoxColors(SerializedObject hud)
        {
            EditorGUILayout.LabelField("Cevap Kutusu ve Hedef Renkleri", EditorStyles.boldLabel);
            DrawColorProperty(hud, "unrevealedAnswerTextColor", "Kapali Harf Yazi");
            DrawColorProperty(hud, "unrevealedAnswerBoxColor", "Kapali Harf Kutu");
            DrawColorProperty(hud, "targetPulseTextStartColor", "Hedef Pulse Yazi Baslangic");
            DrawColorProperty(hud, "targetPulseTextEndColor", "Hedef Pulse Yazi Bitis");
            DrawColorProperty(hud, "targetPulseBoxStartColor", "Hedef Pulse Kutu Baslangic");
            DrawColorProperty(hud, "targetPulseBoxEndColor", "Hedef Pulse Kutu Bitis");
        }

        private void DrawFailModal()
        {
            if (_failModal == null)
            {
                EditorGUILayout.HelpBox("Sahnede FailModalPresenter bulunamadi.", MessageType.Warning);
                return;
            }

            SerializedObject so = new SerializedObject(_failModal);
            DrawPanel("Panel", WordSpinAlphaEditorUiTuningUtility.ResolveReference<GameObject>(so, "root"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Baslik", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "titleLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Govde", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "bodyLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Enerji Bilgisi", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "energyLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawButtonBlock("Devam Butonu", WordSpinAlphaEditorUiTuningUtility.ResolveReference<Button>(so, "continueButton"), WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "continueButtonLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawButtonBlock("Retry Butonu", WordSpinAlphaEditorUiTuningUtility.ResolveReference<Button>(so, "retryButton"), WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "retryButtonLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawButtonBlock("Premium Butonu", WordSpinAlphaEditorUiTuningUtility.ResolveReference<Button>(so, "premiumButton"), WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "premiumButtonLabel"));
        }

        private void DrawInfoCard()
        {
            if (_infoCard == null)
            {
                EditorGUILayout.HelpBox("Sahnede InfoCardPresenter bulunamadi.", MessageType.Warning);
                return;
            }

            SerializedObject so = new SerializedObject(_infoCard);
            GameObject root = WordSpinAlphaEditorUiTuningUtility.ResolveReference<GameObject>(so, "root");
            DrawPanel("Kart Paneli", root);
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Baslik", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "titleLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Govde", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "bodyLabel"));

            if (root != null)
            {
                Transform closeButton = root.transform.Find("InfoClose");
                if (closeButton != null)
                {
                    EditorGUILayout.Space(4f);
                    WordSpinAlphaEditorUiTuningUtility.DrawButtonBlock("Kapat Butonu", closeButton.GetComponent<Button>(), closeButton.GetComponentInChildren<TextMeshProUGUI>(true));
                }
            }
        }

        private void DrawResult()
        {
            if (_result == null)
            {
                EditorGUILayout.HelpBox("Sahnede ResultPresenter bulunamadi.", MessageType.Warning);
                return;
            }

            SerializedObject so = new SerializedObject(_result);
            GameObject root = WordSpinAlphaEditorUiTuningUtility.ResolveReference<GameObject>(so, "root");
            DrawPanel("Result Paneli", root);
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Sonuc Metni", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "resultLabel"));

            if (root != null)
            {
                Transform next = root.transform.Find("Next");
                Transform menu = root.transform.Find("Menu");
                if (next != null)
                {
                    EditorGUILayout.Space(4f);
                    WordSpinAlphaEditorUiTuningUtility.DrawButtonBlock("Next Butonu", next.GetComponent<Button>(), next.GetComponentInChildren<TextMeshProUGUI>(true));
                }

                if (menu != null)
                {
                    EditorGUILayout.Space(4f);
                    WordSpinAlphaEditorUiTuningUtility.DrawButtonBlock("Menu Butonu", menu.GetComponent<Button>(), menu.GetComponentInChildren<TextMeshProUGUI>(true));
                }
            }
        }

        private void DrawStore()
        {
            if (_store == null)
            {
                EditorGUILayout.HelpBox("Sahnede StorePresenter bulunamadi.", MessageType.Warning);
                return;
            }

            SerializedObject so = new SerializedObject(_store);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Tema Durumu", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "themeStatusLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Uyelik Durumu", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "membershipStatusLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Coin Durumu", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "coinStatusLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Coming Soon", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "comingSoonLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Tema Fiyati", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "themePriceLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Uyelik Fiyati", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "membershipPriceLabel"));
        }

        private void DrawMainMenu()
        {
            if (_mainMenu == null)
            {
                EditorGUILayout.HelpBox("Sahnede MainMenuPresenter bulunamadi.", MessageType.Warning);
                return;
            }

            SerializedObject so = new SerializedObject(_mainMenu);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Enerji Etiketi", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "energyLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Ipucu Etiketi", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "hintLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Dil Etiketi", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "languageLabel"));
            EditorGUILayout.Space(4f);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Level Summary", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "levelSelectSummaryLabel"));

            GameObject levelSelectRoot = WordSpinAlphaEditorUiTuningUtility.ResolveReference<GameObject>(so, "levelSelectRoot");
            if (levelSelectRoot != null)
            {
                EditorGUILayout.Space(4f);
                DrawPanel("Level Select Root", levelSelectRoot);
            }

            TMP_InputField levelJumpInput = WordSpinAlphaEditorUiTuningUtility.ResolveReference<TMP_InputField>(so, "levelJumpInput");
            if (levelJumpInput != null)
            {
                EditorGUILayout.Space(4f);
                WordSpinAlphaEditorUiTuningUtility.DrawRectTransformBlock("Level Jump Input", levelJumpInput.transform as RectTransform, includeAnchors: true, includePivot: false, includeScale: false);
                if (levelJumpInput.textComponent is TextMeshProUGUI inputText)
                {
                    EditorGUILayout.Space(4f);
                    WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Level Jump Yazisi", inputText, includeRect: false);
                }

                if (levelJumpInput.placeholder is TextMeshProUGUI placeholderText)
                {
                    EditorGUILayout.Space(4f);
                    WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Level Jump Placeholder", placeholderText, includeRect: false);
                }
            }
        }

        private void DrawMembership()
        {
            if (_membership == null)
            {
                EditorGUILayout.HelpBox("Sahnede MembershipPresenter bulunamadi.", MessageType.Warning);
                return;
            }

            SerializedObject so = new SerializedObject(_membership);
            WordSpinAlphaEditorUiTuningUtility.DrawTextBlock("Uyelik Faydalar Yazisi", WordSpinAlphaEditorUiTuningUtility.ResolveReference<TextMeshProUGUI>(so, "benefitsLabel"));
        }

        private static void DrawPanel(string title, GameObject root)
        {
            if (root == null)
            {
                EditorGUILayout.HelpBox($"{title} bulunamadi.", MessageType.Warning);
                return;
            }

            RectTransform rect = root.transform as RectTransform;
            if (rect != null)
            {
                WordSpinAlphaEditorUiTuningUtility.DrawRectTransformBlock($"{title} / Yerlesim", rect, includeAnchors: true, includePivot: false, includeScale: false);
            }

            Image image = root.GetComponent<Image>();
            if (image != null)
            {
                EditorGUILayout.Space(2f);
                EditorGUILayout.LabelField($"{title} / Arka Plan", EditorStyles.boldLabel);
                Color color = EditorGUILayout.ColorField("Renk", image.color);
                if (color != image.color)
                {
                    Undo.RecordObject(image, "UI Tuning Panel Arka Plan");
                    image.color = color;
                    EditorUtility.SetDirty(image);
                    WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.Scene);
                }
            }
        }

        private static void ApplyRefresh()
        {
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

        private static void DrawColorProperty(SerializedObject source, string propertyName, string label)
        {
            SerializedProperty property = source.FindProperty(propertyName);
            if (property == null)
            {
                return;
            }

            EditorGUI.BeginChangeCheck();
            Color color = EditorGUILayout.ColorField(label, property.colorValue);
            if (!EditorGUI.EndChangeCheck())
            {
                return;
            }

            Undo.RecordObject(source.targetObject, $"UI Tuning {label}");
            property.colorValue = color;
            source.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(source.targetObject);
            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.Scene);
        }
    }
}
