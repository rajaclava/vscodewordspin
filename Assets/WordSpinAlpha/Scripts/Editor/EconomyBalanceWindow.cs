using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Editor
{
    public class EconomyBalanceWindow : EditorWindow
    {
        private const string DefaultAssetPath = "Assets/WordSpinAlpha/Resources/Configs/EconomyBalanceProfile.asset";
        private const string FreeAssetPath = "Assets/WordSpinAlpha/Resources/Configs/EconomyBalanceProfile_Free.asset";
        private const string PremiumAssetPath = "Assets/WordSpinAlpha/Resources/Configs/EconomyBalanceProfile_Premium.asset";
        private const string TestModeAssetPath = "Assets/WordSpinAlpha/Resources/Configs/TestPlayerModeProfile.asset";
        private const string LevelsPath = "Assets/WordSpinAlpha/Resources/Content/levels.json";

        private EconomyBalanceProfile _profile;
        private TestPlayerModeProfile _testModeProfile;
        private SerializedObject _serializedProfile;
        private SerializedObject _serializedTestModeProfile;
        private Vector2 _scroll;
        private TestPlayerMode _editingEconomyMode = TestPlayerMode.Default;
        private TestPlayerMode _copyTargetEconomyMode = TestPlayerMode.FreePlayer;
        private int _simulatedLevelId = 1;
        private int _simulatedStars = 3;
        private bool _simulatedFirstClear = true;
        private bool _simulatedMembership;
        private int _simulatedLevelsPerDay = 8;
        private readonly Dictionary<int, bool> _levelFoldouts = new Dictionary<int, bool>();
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Ekonomi Denge Editoru")]
        public static void Open()
        {
            GetWindow<EconomyBalanceWindow>("Ekonomi Editoru");
        }

        private void OnEnable()
        {
            _profile = EnsureProfileAsset(_editingEconomyMode);
            SyncLevelEntries();
            _serializedProfile = new SerializedObject(_profile);
            _testModeProfile = EnsureTestModeProfileAsset();
            _serializedTestModeProfile = new SerializedObject(_testModeProfile);
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            _testModeProfile = EnsureTestModeProfileAsset();
            _profile = EnsureProfileAsset(_editingEconomyMode);
            if (_profile == null || _testModeProfile == null)
            {
                EditorGUILayout.HelpBox("Ekonomi veya test modu profil asset'i olusturulamadi.", MessageType.Error);
                return;
            }

            if (_serializedProfile == null || _serializedProfile.targetObject != _profile)
            {
                _serializedProfile = new SerializedObject(_profile);
            }

            if (_serializedTestModeProfile == null || _serializedTestModeProfile.targetObject != _testModeProfile)
            {
                _serializedTestModeProfile = new SerializedObject(_testModeProfile);
            }

            _serializedProfile.Update();
            _serializedTestModeProfile.Update();

            EditorGUILayout.LabelField("Ekonomi Denge Editoru", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Default profil gercek oyun ekonomisidir. Free ve Premium profilleri yalnizca test sandbox'idir. Bu sayede free/premium denemeleri default veriye karismaz.", MessageType.Info);

            TestPlayerMode nextEditingMode = (TestPlayerMode)EditorGUILayout.EnumPopup("Duzenlenen Ekonomi Profili", _editingEconomyMode);
            if (nextEditingMode != _editingEconomyMode)
            {
                _editingEconomyMode = nextEditingMode;
                _profile = EnsureProfileAsset(_editingEconomyMode);
                _serializedProfile = new SerializedObject(_profile);
                _serializedProfile.Update();
            }

            EditorGUILayout.LabelField($"Aktif Sandbox: {GetModeLabel(_editingEconomyMode)}", EditorStyles.miniBoldLabel);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Profil Assetini Sec", GUILayout.Height(26f)))
                {
                    Selection.activeObject = _profile;
                    EditorGUIUtility.PingObject(_profile);
                }

                if (GUILayout.Button("Level Listesini Senkronla", GUILayout.Height(26f)))
                {
                    SyncLevelEntries();
                }

                if (GUILayout.Button("Secili Profili Sifirla", GUILayout.Height(26f)))
                {
                    Undo.RecordObject(_profile, "Ekonomi Profilini Sifirla");
                    _profile.ResetToDefaults();
                    SyncLevelEntries();
                    EditorUtility.SetDirty(_profile);
                    AssetDatabase.SaveAssets();
                    RefreshRuntimeEconomyBindings();
                    _serializedProfile = new SerializedObject(_profile);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _copyTargetEconomyMode = (TestPlayerMode)EditorGUILayout.EnumPopup("Secili Profili Su Moda Kopyala", _copyTargetEconomyMode);
                using (new EditorGUI.DisabledScope(_copyTargetEconomyMode == _editingEconomyMode))
                {
                    if (GUILayout.Button("Profili Kopyala", GUILayout.Height(24f), GUILayout.Width(150f)))
                    {
                        CopyEconomyProfile(_editingEconomyMode, _copyTargetEconomyMode);
                    }
                }
            }

            EditorGUILayout.Space(10f);
            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            DrawGeneralRewardSection();
            EditorGUILayout.Space(10f);
            DrawStarRulesSection();
            EditorGUILayout.Space(10f);
            DrawHookSection();
            EditorGUILayout.Space(10f);
            DrawTestModeSection();
            EditorGUILayout.Space(10f);
            DrawLevelCoinSection();
            EditorGUILayout.Space(10f);
            DrawThemeOfferSection();
            EditorGUILayout.Space(10f);
            DrawRegionalPreviewSection();
            EditorGUILayout.Space(10f);
            DrawSimulationSection();

            EditorGUILayout.EndScrollView();

            bool profileChanged = _serializedProfile.ApplyModifiedProperties();
            bool testModeChanged = _serializedTestModeProfile.ApplyModifiedProperties();
            if (profileChanged || testModeChanged)
            {
                EditorUtility.SetDirty(_profile);
                EditorUtility.SetDirty(_testModeProfile);
                AssetDatabase.SaveAssets();
                RefreshRuntimeEconomyBindings();
            }
        }

        private void TryAutoRefresh()
        {
            if (!WordSpinAlphaEditorSyncUtility.ConsumeChanges(WordSpinAlphaEditorSyncKind.Content, ref _syncStamp))
            {
                return;
            }

            _profile = EnsureProfileAsset(_editingEconomyMode);
            SyncLevelEntries();
            _serializedProfile = new SerializedObject(_profile);
            Repaint();
        }

        private void DrawGeneralRewardSection()
        {
            EditorGUILayout.LabelField("Genel Coin Odulu", EditorStyles.boldLabel);
            DrawProperty("awardCoinsOnlyOnFirstCompletion", "Coin sadece ilk tamamlamada verilsin");
            DrawProperty("defaultFirstClearCoins", "Varsayilan ilk tamamlama coin'i");
            DrawProperty("defaultReplayCoins", "Varsayilan tekrar oynama coin'i");
            DrawProperty("premiumMembershipCoinMultiplier", "Uyelik coin carpani");
        }

        private void DrawStarRulesSection()
        {
            EditorGUILayout.LabelField("Yildiz ve Reklam Kurallari", EditorStyles.boldLabel);
            DrawProperty("maxStars", "Maksimum yildiz");
            DrawProperty("zeroMistakeStars", "0 hata yildizi");
            DrawProperty("oneMistakeStars", "1 hata yildizi");
            DrawProperty("twoOrMoreMistakesStars", "2+ hata yildizi");
            DrawProperty("maxStarsAfterContinue", "Continue sonrasi max yildiz");
            DrawProperty("threeStarCoinMultiplier", "3 yildiz coin carpani");
            DrawProperty("twoStarCoinMultiplier", "2 yildiz coin carpani");
            DrawProperty("oneStarCoinMultiplier", "1 yildiz coin carpani");
            DrawProperty("enableAdCatchupHook", "Reklam catch-up hook aktif");
            DrawProperty("adCatchupOnlyOnFirstClear", "Reklam hook sadece ilk tamamlama");
            DrawProperty("adCatchupTargetStars", "Reklam hedef yildiz");
            DrawProperty("adCatchupBonusMultiplier", "Reklam bonus carpani");
        }

        private void DrawHookSection()
        {
            EditorGUILayout.LabelField("HUD ve Store Hook", EditorStyles.boldLabel);
            DrawProperty("showGameplayCoinHook", "Gameplay coin kasasi goster");
            DrawProperty("showUpcomingThemesTeaser", "Store yakinda teaser goster");
        }

        private void DrawTestModeSection()
        {
            EditorGUILayout.LabelField("Test Oyuncu Modu", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu katman ana runtime ekonomisini bozmaz. Free ve premium oyuncu akisini test etmek icin override verisi tutar. Production oncesi kapatilabilir, editor'de kalabilir.", MessageType.Info);
            EditorGUILayout.HelpBox("Aktif Test Modu dropdown'i tek basina runtime'i degistirmez. Runtime'a gecirmek icin 'Aktif Modu Kayda Uygula' butonunu kullan.", MessageType.None);

            SerializedProperty activeMode = _serializedTestModeProfile.FindProperty("activeMode");
            SerializedProperty freePlayer = _serializedTestModeProfile.FindProperty("freePlayer");
            SerializedProperty premiumPlayer = _serializedTestModeProfile.FindProperty("premiumPlayer");

            if (activeMode != null)
            {
                EditorGUILayout.PropertyField(activeMode, new GUIContent("Aktif Test Modu"));
            }

            DrawModeTuningSection("Free Oyuncu Ayarlari", freePlayer);
            EditorGUILayout.Space(4f);
            DrawModeTuningSection("Premium Oyuncu Ayarlari", premiumPlayer);
            EditorGUILayout.Space(6f);

            using (new EditorGUI.DisabledScope(!Application.isPlaying))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Aktif Modu Kayda Uygula", GUILayout.Height(28f)))
                    {
                        TestPlayerModeManager.Instance?.ApplyActiveModeToSaveState();
                    }

                    if (GUILayout.Button("Puan ve Sonucu Sifirla", GUILayout.Height(28f)))
                    {
                        TestPlayerModeManager.Instance?.ResetPendingScoreAndSession();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Coinleri Sifirla", GUILayout.Height(26f)))
                    {
                        TestPlayerModeManager.Instance?.ResetSoftCurrency();
                    }

                    if (GUILayout.Button("Tema Kilitlerini Sifirla", GUILayout.Height(26f)))
                    {
                        TestPlayerModeManager.Instance?.ResetThemeOwnership();
                    }
                }

                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Uyelik Flaglerini Sifirla", GUILayout.Height(26f)))
                    {
                        TestPlayerModeManager.Instance?.ResetMembershipFlags();
                    }

                    if (GUILayout.Button("Enerjiyi Moda Gore Doldur", GUILayout.Height(26f)))
                    {
                        TestPlayerModeManager.Instance?.FillEnergyForCurrentMode();
                    }
                }
            }

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("Kayit ve runtime reset butonlari play modda aktif olur. Profil ayarlari ise her zaman kaydedilir.", MessageType.None);
            }
        }

        private void DrawLevelCoinSection()
        {
            EditorGUILayout.LabelField("Bolum Bazli Coin Ayarlari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Level satirlari varsayilan olarak kapali gelir. Duzenlemek istedigin leveli ac.", MessageType.None);
            SerializedProperty list = _serializedProfile.FindProperty("levelCoinOverrides");
            if (list == null)
            {
                return;
            }

            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                SerializedProperty levelId = element.FindPropertyRelative("levelId");
                SerializedProperty firstClearCoins = element.FindPropertyRelative("firstClearCoins");
                SerializedProperty replayCoins = element.FindPropertyRelative("replayCoins");
                int levelKey = levelId != null ? levelId.intValue : i + 1;
                bool isOpen = GetLevelFoldout(levelKey);

                EditorGUILayout.BeginVertical("box");
                bool nextOpen = EditorGUILayout.Foldout(isOpen, $"Seviye {levelKey}", true);
                SetLevelFoldout(levelKey, nextOpen);
                if (nextOpen)
                {
                    EditorGUILayout.PropertyField(firstClearCoins, new GUIContent("Ilk Tamamlama Coin"));
                    EditorGUILayout.PropertyField(replayCoins, new GUIContent("Tekrar Oynama Coin"));
                }
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawThemeOfferSection()
        {
            EditorGUILayout.LabelField("Tema Paket Ayarlari", EditorStyles.boldLabel);
            SerializedProperty list = _serializedProfile.FindProperty("themeOffers");
            if (list == null)
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Tema Paketi Ekle", GUILayout.Width(150f)))
                {
                    list.InsertArrayElementAtIndex(list.arraySize);
                }

                using (new EditorGUI.DisabledScope(list.arraySize <= 0))
                {
                    if (GUILayout.Button("Son Paketi Sil", GUILayout.Width(150f)))
                    {
                        list.DeleteArrayElementAtIndex(list.arraySize - 1);
                    }
                }
            }

            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(element.FindPropertyRelative("themeId"), new GUIContent("Tema Id"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("softCurrencyPriceOverride"), new GUIContent("Coin Fiyati"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("allowCoinPurchase"), new GUIContent("Coin ile alinabilir"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("allowPremiumMembership"), new GUIContent("Uyelik ile acilir"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("allowDirectPurchase"), new GUIContent("Dogrudan satin alinabilir"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("iapTierId"), new GUIContent("IAP Tier Id"));
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawRegionalPreviewSection()
        {
            EditorGUILayout.LabelField("Bolgesel Fiyat Taslagi", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu alan sadece planlama ve taslak tier takibi icindir. Runtime fiyatlar dili degil storefront bolgesini izleyecek.", MessageType.None);
            EditorGUILayout.HelpBox("Mevcut store preview'i test kolayligi icin dil -> varsayilan bolge eslemesi kullanir: tr->TR, en->US, de->DE, es->ES. Nihai surumde bu esleme kalkacak ve fiyat dogrudan Play Billing urun detayindan okunacak.", MessageType.Info);

            SerializedProperty list = _serializedProfile.FindProperty("regionalPricePreviews");
            if (list == null)
            {
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Bolge Satiri Ekle", GUILayout.Width(150f)))
                {
                    list.InsertArrayElementAtIndex(list.arraySize);
                }

                using (new EditorGUI.DisabledScope(list.arraySize <= 0))
                {
                    if (GUILayout.Button("Son Bolgeyi Sil", GUILayout.Width(150f)))
                    {
                        list.DeleteArrayElementAtIndex(list.arraySize - 1);
                    }
                }
            }

            for (int i = 0; i < list.arraySize; i++)
            {
                SerializedProperty element = list.GetArrayElementAtIndex(i);
                EditorGUILayout.BeginVertical("box");
                EditorGUILayout.PropertyField(element.FindPropertyRelative("regionCode"), new GUIContent("Bolge"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("currencyCode"), new GUIContent("Para Birimi"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("membershipPrice"), new GUIContent("Uyelik Taslak Fiyati"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("themePackPrice"), new GUIContent("Tema Paket Taslak Fiyati"));
                EditorGUILayout.PropertyField(element.FindPropertyRelative("notes"), new GUIContent("Not"));
                EditorGUILayout.EndVertical();
            }
        }

        private void DrawSimulationSection()
        {
            EditorGUILayout.LabelField("Basit Simulasyon", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu alan dengeyi goz karari degil, yaklasik kazanma hizi ile kontrol etmek icin var. Nihai ticari fiyatlar burada kesinlenmeyecek.", MessageType.Info);

            List<int> levelIds = LoadLevelIds();
            if (levelIds.Count == 0)
            {
                levelIds.Add(1);
            }

            int selectedLevelIndex = Mathf.Max(0, levelIds.IndexOf(_simulatedLevelId));
            string[] levelOptions = levelIds.Select(id => $"Seviye {id}").ToArray();
            selectedLevelIndex = EditorGUILayout.Popup("Test Seviye", selectedLevelIndex, levelOptions);
            _simulatedLevelId = levelIds[Mathf.Clamp(selectedLevelIndex, 0, levelIds.Count - 1)];
            _simulatedStars = EditorGUILayout.IntSlider("Yildiz", _simulatedStars, 1, 3);
            _simulatedFirstClear = EditorGUILayout.Toggle("Ilk Tamamlama", _simulatedFirstClear);
            _simulatedMembership = EditorGUILayout.Toggle("Premium Uyelik Aktif", _simulatedMembership);
            _simulatedLevelsPerDay = EditorGUILayout.IntSlider("Gunluk Tamamlama", _simulatedLevelsPerDay, 1, 30);

            int baseCoins = _profile.ResolveBaseCoinReward(_simulatedLevelId, _simulatedFirstClear);
            int earnedCoins = _profile.ResolveCoinReward(baseCoins, _simulatedStars, _simulatedMembership);
            int adBonusCoins = _profile.ResolveAdCatchupBonus(baseCoins, _simulatedStars, _simulatedFirstClear, _simulatedMembership);
            EconomyBalanceProfile.ThemeOfferTuning targetTheme = _profile.ThemeOffers.Count > 0 ? _profile.ThemeOffers[0] : null;
            int targetCost = targetTheme != null ? Mathf.Max(1, targetTheme.softCurrencyPriceOverride) : 1;
            int coinsPerDay = Mathf.Max(1, earnedCoins * Mathf.Max(1, _simulatedLevelsPerDay));
            float daysToUnlock = targetCost / (float)coinsPerDay;

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Base Coin: {baseCoins}");
            EditorGUILayout.LabelField($"Kazanilan Coin: {earnedCoins}");
            EditorGUILayout.LabelField($"Reklam Catch-up Bonus: {adBonusCoins}");
            if (targetTheme != null)
            {
                EditorGUILayout.LabelField($"Hedef Tema: {targetTheme.themeId}");
                EditorGUILayout.LabelField($"Hedef Coin Fiyati: {targetCost}");
                EditorGUILayout.LabelField($"Tahmini Kilit Acma Suresi: {daysToUnlock:0.0} gun");
            }
            EditorGUILayout.EndVertical();
        }

        private void DrawProperty(string propertyName, string label)
        {
            SerializedProperty property = _serializedProfile.FindProperty(propertyName);
            if (property != null)
            {
                EditorGUILayout.PropertyField(property, new GUIContent(label));
            }
        }

        private void SyncLevelEntries()
        {
            _profile = EnsureProfileAsset(_editingEconomyMode);
            if (_profile == null)
            {
                return;
            }

            Undo.RecordObject(_profile, "Ekonomi Level Listesini Senkronla");
            _profile.EnsureDefaults();
            _profile.EnsureLevelEntries(LoadLevelIds());
            EditorUtility.SetDirty(_profile);
            AssetDatabase.SaveAssets();
            RefreshRuntimeEconomyBindings();
        }

        private static void RefreshRuntimeEconomyBindings()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            LevelEconomyManager.Instance?.RefreshProfileCache();
            EconomyManager.Instance?.RefreshEconomyProfile();
            TestPlayerModeManager.Instance?.RefreshProfileCache();
            QuestionLifeManager.Instance?.RefreshForTesting(false);
            if (TestPlayerModeManager.Instance != null)
            {
                TestPlayerModeManager.Instance.BroadcastRuntimeState();
            }
            else if (EconomyManager.Instance != null)
            {
                GameEvents.RaiseSoftCurrencyChanged(EconomyManager.Instance.SoftCurrency, 0);
            }
        }

        private bool GetLevelFoldout(int levelId)
        {
            if (_levelFoldouts.TryGetValue(levelId, out bool isOpen))
            {
                return isOpen;
            }

            return false;
        }

        private void SetLevelFoldout(int levelId, bool isOpen)
        {
            _levelFoldouts[levelId] = isOpen;
        }

        private void CopyEconomyProfile(TestPlayerMode sourceMode, TestPlayerMode targetMode)
        {
            EconomyBalanceProfile source = EnsureProfileAsset(sourceMode);
            EconomyBalanceProfile target = EnsureProfileAsset(targetMode);
            if (source == null || target == null)
            {
                return;
            }

            Undo.RecordObject(target, $"Ekonomi Profilini Kopyala ({sourceMode} -> {targetMode})");
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(source), target);
            target.EnsureDefaults();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();

            if (_editingEconomyMode == targetMode)
            {
                _profile = target;
                _serializedProfile = new SerializedObject(_profile);
            }

            RefreshRuntimeEconomyBindings();
        }

        private static EconomyBalanceProfile EnsureProfileAsset(TestPlayerMode mode)
        {
            string assetPath = GetEconomyAssetPath(mode);
            EconomyBalanceProfile profile = AssetDatabase.LoadAssetAtPath<EconomyBalanceProfile>(assetPath);
            if (profile != null)
            {
                profile.EnsureDefaults();
                return profile;
            }

            EnsureFolder("Assets/WordSpinAlpha/Resources");
            EnsureFolder("Assets/WordSpinAlpha/Resources/Configs");

            profile = CreateInstance<EconomyBalanceProfile>();
            if (mode == TestPlayerMode.Default)
            {
                profile.ResetToDefaults();
            }
            else
            {
                EconomyBalanceProfile defaultProfile = AssetDatabase.LoadAssetAtPath<EconomyBalanceProfile>(DefaultAssetPath);
                if (defaultProfile != null)
                {
                    JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(defaultProfile), profile);
                }
                else
                {
                    profile.ResetToDefaults();
                }
            }
            profile.EnsureDefaults();
            AssetDatabase.CreateAsset(profile, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return profile;
        }

        private static TestPlayerModeProfile EnsureTestModeProfileAsset()
        {
            TestPlayerModeProfile profile = AssetDatabase.LoadAssetAtPath<TestPlayerModeProfile>(TestModeAssetPath);
            if (profile != null)
            {
                profile.EnsureDefaults();
                return profile;
            }

            EnsureFolder("Assets/WordSpinAlpha/Resources");
            EnsureFolder("Assets/WordSpinAlpha/Resources/Configs");

            profile = CreateInstance<TestPlayerModeProfile>();
            profile.ResetToDefaults();
            profile.EnsureDefaults();
            AssetDatabase.CreateAsset(profile, TestModeAssetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return profile;
        }

        private static void DrawModeTuningSection(string title, SerializedProperty tuningProperty)
        {
            if (tuningProperty == null)
            {
                return;
            }

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("overrideMembership"), new GUIContent("Uyelik Durumunu Override Et"));
            using (new EditorGUI.DisabledScope(!tuningProperty.FindPropertyRelative("overrideMembership").boolValue))
            {
                EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("premiumMembershipActive"), new GUIContent("Premium Uyelik Aktif"));
                EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("noAdsOwned"), new GUIContent("Reklamsiz Mod Aktif"));
            }

            EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("overrideEnergyRules"), new GUIContent("Enerji Kurallarini Override Et"));
            using (new EditorGUI.DisabledScope(!tuningProperty.FindPropertyRelative("overrideEnergyRules").boolValue))
            {
                EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("maxEnergy"), new GUIContent("Maksimum Enerji"));
                EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("refillMinutes"), new GUIContent("Enerji Dolum Dakikasi"));
                EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("bypassEntryEnergy"), new GUIContent("Giris Enerjisini Bypass Et"));
            }

            EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("overrideQuestionHearts"), new GUIContent("Soru Canini Override Et"));
            using (new EditorGUI.DisabledScope(!tuningProperty.FindPropertyRelative("overrideQuestionHearts").boolValue))
            {
                EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("questionHearts"), new GUIContent("Soru Basi Can"));
            }

            EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("requireRewardedContinue"), new GUIContent("Continue Icin Rewarded Zorunlu"));
            using (new EditorGUI.DisabledScope(!tuningProperty.FindPropertyRelative("requireRewardedContinue").boolValue))
            {
                EditorGUILayout.PropertyField(tuningProperty.FindPropertyRelative("rewardedAdSeconds"), new GUIContent("Sahte Reklam Suresi (sn)"));
            }
            EditorGUILayout.EndVertical();
        }

        private static string GetEconomyAssetPath(TestPlayerMode mode)
        {
            switch (mode)
            {
                case TestPlayerMode.FreePlayer:
                    return FreeAssetPath;
                case TestPlayerMode.PremiumPlayer:
                    return PremiumAssetPath;
                default:
                    return DefaultAssetPath;
            }
        }

        private static string GetModeLabel(TestPlayerMode mode)
        {
            switch (mode)
            {
                case TestPlayerMode.FreePlayer:
                    return "Free Sandbox";
                case TestPlayerMode.PremiumPlayer:
                    return "Premium Sandbox";
                default:
                    return "Default / Gercek Oyun";
            }
        }

        private static List<int> LoadLevelIds()
        {
            TextAsset levelsAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(LevelsPath);
            if (levelsAsset == null)
            {
                return new List<int>();
            }

            LevelCatalog catalog = JsonUtility.FromJson<LevelCatalog>(levelsAsset.text);
            List<int> levelIds = new List<int>();
            if (catalog == null || catalog.levels == null)
            {
                return levelIds;
            }

            for (int i = 0; i < catalog.levels.Length; i++)
            {
                LevelDefinition level = catalog.levels[i];
                if (level != null && level.levelId > 0)
                {
                    levelIds.Add(level.levelId);
                }
            }

            levelIds.Sort();
            return levelIds;
        }

        private static void EnsureFolder(string path)
        {
            string[] parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, parts[i]);
                }

                current = next;
            }
        }
    }
}
