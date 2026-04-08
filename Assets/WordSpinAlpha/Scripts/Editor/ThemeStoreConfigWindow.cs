using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Content;

namespace WordSpinAlpha.Editor
{
    public class ThemeStoreConfigWindow : EditorWindow
    {
        private Vector2 _scroll;
        private bool _showThemes = true;
        private bool _showStore = true;
        private bool _showMembership = true;
        private bool _showEnergy = true;

        private ThemeCatalog _themeCatalog;
        private StoreCatalogDefinition _storeCatalog;
        private MembershipProfileDefinition _membershipProfile;
        private EnergyConfigDefinition _energyConfig;

        private readonly List<bool> _themeFoldouts = new List<bool>();
        private readonly List<bool> _storeThemeFoldouts = new List<bool>();
        private readonly List<bool> _energyPackFoldouts = new List<bool>();
        private readonly List<bool> _hintPackFoldouts = new List<bool>();
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Tema ve Magaza Config")]
        public static void Open()
        {
            GetWindow<ThemeStoreConfigWindow>("Tema ve Magaza");
        }

        private void OnEnable()
        {
            Reload();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Tema ve Magaza Config", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu pencere tema paketlerini, magaza urunlerini, premium uyeligi ve enerji kurallarini JSON dosyalarina dokunmadan duzenler.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Yeniden Yukle", GUILayout.Height(28f)))
                {
                    Reload();
                }

                string saveLabel = Application.isPlaying ? "Kaydet ve Uygula" : "Kaydet";
                if (GUILayout.Button(saveLabel, GUILayout.Height(28f)))
                {
                    Save();
                }
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            _showThemes = EditorGUILayout.BeginFoldoutHeaderGroup(_showThemes, "Tema Paketleri");
            if (_showThemes)
            {
                DrawThemes();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showStore = EditorGUILayout.BeginFoldoutHeaderGroup(_showStore, "Magaza ve Fiyat Kimlikleri");
            if (_showStore)
            {
                DrawStore();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showMembership = EditorGUILayout.BeginFoldoutHeaderGroup(_showMembership, "Premium Uyelik");
            if (_showMembership)
            {
                DrawMembership();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.Space(6f);
            _showEnergy = EditorGUILayout.BeginFoldoutHeaderGroup(_showEnergy, "Enerji Baslangic Kurallari");
            if (_showEnergy)
            {
                DrawEnergy();
            }
            EditorGUILayout.EndFoldoutHeaderGroup();

            EditorGUILayout.EndScrollView();
        }

        private void Reload()
        {
            _themeCatalog = WordSpinAlphaRuntimeConfigRepository.LoadThemes();
            _storeCatalog = WordSpinAlphaRuntimeConfigRepository.LoadStoreCatalog();
            _membershipProfile = WordSpinAlphaRuntimeConfigRepository.LoadMembershipProfile();
            _energyConfig = WordSpinAlphaRuntimeConfigRepository.LoadEnergyConfig();

            if (_themeCatalog.themes == null)
            {
                _themeCatalog.themes = System.Array.Empty<ThemePackDefinition>();
            }
            if (_storeCatalog.themes == null)
            {
                _storeCatalog.themes = System.Array.Empty<ThemePriceDefinition>();
            }
            if (_storeCatalog.energyPacks == null)
            {
                _storeCatalog.energyPacks = System.Array.Empty<EnergyPackDefinition>();
            }
            if (_storeCatalog.hintPacks == null)
            {
                _storeCatalog.hintPacks = System.Array.Empty<HintPackDefinition>();
            }

            SyncFoldouts(_themeFoldouts, _themeCatalog.themes.Length);
            SyncFoldouts(_storeThemeFoldouts, _storeCatalog.themes.Length);
            SyncFoldouts(_energyPackFoldouts, _storeCatalog.energyPacks.Length);
            SyncFoldouts(_hintPackFoldouts, _storeCatalog.hintPacks.Length);
        }

        private void TryAutoRefresh()
        {
            if (!WordSpinAlphaEditorSyncUtility.ConsumeChanges(WordSpinAlphaEditorSyncKind.RuntimeConfig, ref _syncStamp))
            {
                return;
            }

            Reload();
            Repaint();
        }

        private void DrawThemes()
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Yeni Tema Paketi Ekle", GUILayout.Width(200f)))
                {
                    List<ThemePackDefinition> themes = new List<ThemePackDefinition>(_themeCatalog.themes);
                    themes.Add(new ThemePackDefinition
                    {
                        themeId = $"theme_{themes.Count + 1:000}",
                        displayName = $"Yeni Tema {themes.Count + 1}",
                        themeCategory = "standard"
                    });
                    _themeCatalog.themes = themes.ToArray();
                    SyncFoldouts(_themeFoldouts, _themeCatalog.themes.Length);
                    _themeFoldouts[_themeCatalog.themes.Length - 1] = true;
                }
            }

            for (int i = 0; i < _themeCatalog.themes.Length; i++)
            {
                ThemePackDefinition theme = _themeCatalog.themes[i] ?? new ThemePackDefinition();
                _themeCatalog.themes[i] = theme;

                EditorGUILayout.BeginVertical("box");
                using (new EditorGUILayout.HorizontalScope())
                {
                    _themeFoldouts[i] = EditorGUILayout.Foldout(_themeFoldouts[i], $"{theme.displayName ?? "Tema"} | {theme.themeId}", true);
                    GUI.backgroundColor = new Color(0.70f, 0.24f, 0.24f);
                    if (GUILayout.Button("Sil", GUILayout.Width(50f)))
                    {
                        RemoveAt(ref _themeCatalog.themes, i);
                        SyncFoldouts(_themeFoldouts, _themeCatalog.themes.Length);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        return;
                    }
                    GUI.backgroundColor = Color.white;
                }

                if (_themeFoldouts[i])
                {
                    theme.themeId = EditorGUILayout.TextField("Tema Id", theme.themeId);
                    theme.displayName = EditorGUILayout.TextField("Gorunen Ad", theme.displayName);
                    theme.themeCategory = EditorGUILayout.TextField("Kategori", theme.themeCategory);
                    theme.isPremium = EditorGUILayout.Toggle("Premium Paket", theme.isPremium);
                    theme.campaignPackId = EditorGUILayout.TextField("Campaign Pack Id", theme.campaignPackId);
                    theme.questionPackId = EditorGUILayout.TextField("Question Pack Id", theme.questionPackId);
                    theme.infoCardPackId = EditorGUILayout.TextField("Info Card Pack Id", theme.infoCardPackId);
                    theme.backgroundResourcePath = EditorGUILayout.TextField("Arka Plan Sprite Yolu", theme.backgroundResourcePath);
                    theme.rotatorResourcePath = EditorGUILayout.TextField("Rotator Sprite Yolu", theme.rotatorResourcePath);
                    theme.pinResourcePath = EditorGUILayout.TextField("Pin Sprite Yolu", theme.pinResourcePath);
                    theme.keyboardSkinResourcePath = EditorGUILayout.TextField("Klavye Skin Yolu", theme.keyboardSkinResourcePath);
                    theme.launchVfxResourcePath = EditorGUILayout.TextField("Launch VFX Yolu", theme.launchVfxResourcePath);
                    theme.impactVfxResourcePath = EditorGUILayout.TextField("Impact VFX Yolu", theme.impactVfxResourcePath);
                    theme.completeVfxResourcePath = EditorGUILayout.TextField("Complete VFX Yolu", theme.completeVfxResourcePath);
                    theme.bgmResourcePath = EditorGUILayout.TextField("BGM Yolu", theme.bgmResourcePath);
                    theme.hitSfxResourcePath = EditorGUILayout.TextField("Hit SFX Yolu", theme.hitSfxResourcePath);
                    theme.missSfxResourcePath = EditorGUILayout.TextField("Miss SFX Yolu", theme.missSfxResourcePath);
                    theme.completionSfxResourcePath = EditorGUILayout.TextField("Completion SFX Yolu", theme.completionSfxResourcePath);
                    theme.uiPrimaryHex = EditorGUILayout.TextField("UI Primary HEX", theme.uiPrimaryHex);
                    theme.uiAccentHex = EditorGUILayout.TextField("UI Accent HEX", theme.uiAccentHex);
                    theme.uiBackgroundHex = EditorGUILayout.TextField("UI Background HEX", theme.uiBackgroundHex);
                    theme.hitPitchStep = EditorGUILayout.FloatField("Hit Pitch Step", theme.hitPitchStep);
                    theme.entryEnergyBias = EditorGUILayout.FloatField("Giris Enerji Bias", theme.entryEnergyBias);
                    theme.toleranceBias = EditorGUILayout.FloatField("Tolerance Bias", theme.toleranceBias);
                    theme.mechanicalIdentity = EditorGUILayout.TextField("Mechanical Identity", theme.mechanicalIdentity);
                }

                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4f);
            }
        }

        private void DrawStore()
        {
            _storeCatalog.noAdsProductId = EditorGUILayout.TextField("No Ads Product Id", _storeCatalog.noAdsProductId);
            _storeCatalog.premiumMembershipProductId = EditorGUILayout.TextField("Premium Uyelik Product Id", _storeCatalog.premiumMembershipProductId);
            EditorGUILayout.Space(4f);

            DrawThemePrices();
            EditorGUILayout.Space(6f);
            DrawEnergyPacks();
            EditorGUILayout.Space(6f);
            DrawHintPacks();
        }

        private void DrawThemePrices()
        {
            EditorGUILayout.LabelField("Tema Fiyatlari", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Tema Fiyati Ekle", GUILayout.Width(160f)))
                {
                    List<ThemePriceDefinition> items = new List<ThemePriceDefinition>(_storeCatalog.themes);
                    items.Add(new ThemePriceDefinition { themeId = "theme_id", softCurrencyPrice = 1200, iapProductId = "theme.product.id" });
                    _storeCatalog.themes = items.ToArray();
                    SyncFoldouts(_storeThemeFoldouts, _storeCatalog.themes.Length);
                    _storeThemeFoldouts[_storeCatalog.themes.Length - 1] = true;
                }
            }

            for (int i = 0; i < _storeCatalog.themes.Length; i++)
            {
                ThemePriceDefinition item = _storeCatalog.themes[i] ?? new ThemePriceDefinition();
                _storeCatalog.themes[i] = item;
                EditorGUILayout.BeginVertical("box");
                using (new EditorGUILayout.HorizontalScope())
                {
                    _storeThemeFoldouts[i] = EditorGUILayout.Foldout(_storeThemeFoldouts[i], $"{item.themeId} | {item.softCurrencyPrice} coin", true);
                    GUI.backgroundColor = new Color(0.70f, 0.24f, 0.24f);
                    if (GUILayout.Button("Sil", GUILayout.Width(50f)))
                    {
                        RemoveAt(ref _storeCatalog.themes, i);
                        SyncFoldouts(_storeThemeFoldouts, _storeCatalog.themes.Length);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        return;
                    }
                    GUI.backgroundColor = Color.white;
                }

                if (_storeThemeFoldouts[i])
                {
                    item.themeId = EditorGUILayout.TextField("Tema Id", item.themeId);
                    item.softCurrencyPrice = EditorGUILayout.IntField("Coin Fiyati", item.softCurrencyPrice);
                    item.iapProductId = EditorGUILayout.TextField("IAP Product Id", item.iapProductId);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawEnergyPacks()
        {
            EditorGUILayout.LabelField("Enerji Paketleri", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Enerji Paketi Ekle", GUILayout.Width(160f)))
                {
                    List<EnergyPackDefinition> items = new List<EnergyPackDefinition>(_storeCatalog.energyPacks);
                    items.Add(new EnergyPackDefinition { productId = "energy.pack.id", energyAmount = 3, gemPrice = 100 });
                    _storeCatalog.energyPacks = items.ToArray();
                    SyncFoldouts(_energyPackFoldouts, _storeCatalog.energyPacks.Length);
                    _energyPackFoldouts[_storeCatalog.energyPacks.Length - 1] = true;
                }
            }

            for (int i = 0; i < _storeCatalog.energyPacks.Length; i++)
            {
                EnergyPackDefinition item = _storeCatalog.energyPacks[i] ?? new EnergyPackDefinition();
                _storeCatalog.energyPacks[i] = item;
                EditorGUILayout.BeginVertical("box");
                using (new EditorGUILayout.HorizontalScope())
                {
                    _energyPackFoldouts[i] = EditorGUILayout.Foldout(_energyPackFoldouts[i], $"{item.productId} | +{item.energyAmount}", true);
                    GUI.backgroundColor = new Color(0.70f, 0.24f, 0.24f);
                    if (GUILayout.Button("Sil", GUILayout.Width(50f)))
                    {
                        RemoveAt(ref _storeCatalog.energyPacks, i);
                        SyncFoldouts(_energyPackFoldouts, _storeCatalog.energyPacks.Length);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        return;
                    }
                    GUI.backgroundColor = Color.white;
                }

                if (_energyPackFoldouts[i])
                {
                    item.productId = EditorGUILayout.TextField("Product Id", item.productId);
                    item.energyAmount = EditorGUILayout.IntField("Enerji Miktari", item.energyAmount);
                    item.gemPrice = EditorGUILayout.IntField("Gem Fiyati", item.gemPrice);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawHintPacks()
        {
            EditorGUILayout.LabelField("Ipucu Paketleri", EditorStyles.boldLabel);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Ipucu Paketi Ekle", GUILayout.Width(160f)))
                {
                    List<HintPackDefinition> items = new List<HintPackDefinition>(_storeCatalog.hintPacks);
                    items.Add(new HintPackDefinition { productId = "hint.pack.id", hintAmount = 3, gemPrice = 100 });
                    _storeCatalog.hintPacks = items.ToArray();
                    SyncFoldouts(_hintPackFoldouts, _storeCatalog.hintPacks.Length);
                    _hintPackFoldouts[_storeCatalog.hintPacks.Length - 1] = true;
                }
            }

            for (int i = 0; i < _storeCatalog.hintPacks.Length; i++)
            {
                HintPackDefinition item = _storeCatalog.hintPacks[i] ?? new HintPackDefinition();
                _storeCatalog.hintPacks[i] = item;
                EditorGUILayout.BeginVertical("box");
                using (new EditorGUILayout.HorizontalScope())
                {
                    _hintPackFoldouts[i] = EditorGUILayout.Foldout(_hintPackFoldouts[i], $"{item.productId} | +{item.hintAmount}", true);
                    GUI.backgroundColor = new Color(0.70f, 0.24f, 0.24f);
                    if (GUILayout.Button("Sil", GUILayout.Width(50f)))
                    {
                        RemoveAt(ref _storeCatalog.hintPacks, i);
                        SyncFoldouts(_hintPackFoldouts, _storeCatalog.hintPacks.Length);
                        GUI.backgroundColor = Color.white;
                        EditorGUILayout.EndHorizontal();
                        EditorGUILayout.EndVertical();
                        return;
                    }
                    GUI.backgroundColor = Color.white;
                }

                if (_hintPackFoldouts[i])
                {
                    item.productId = EditorGUILayout.TextField("Product Id", item.productId);
                    item.hintAmount = EditorGUILayout.IntField("Ipucu Miktari", item.hintAmount);
                    item.gemPrice = EditorGUILayout.IntField("Gem Fiyati", item.gemPrice);
                }

                EditorGUILayout.EndVertical();
            }
        }

        private void DrawMembership()
        {
            _membershipProfile.membershipId = EditorGUILayout.TextField("Uyelik Id", _membershipProfile.membershipId);
            _membershipProfile.removeAds = EditorGUILayout.Toggle("Reklamlari Kaldir", _membershipProfile.removeAds);
            _membershipProfile.unlimitedEntryEnergy = EditorGUILayout.Toggle("Sinirsiz Giris Enerjisi", _membershipProfile.unlimitedEntryEnergy);
            _membershipProfile.unlockFutureThemes = EditorGUILayout.Toggle("Gelecek Temalari Ac", _membershipProfile.unlockFutureThemes);
        }

        private void DrawEnergy()
        {
            _energyConfig.maxEnergy = EditorGUILayout.IntField("Maks Enerji", _energyConfig.maxEnergy);
            _energyConfig.refillMinutes = EditorGUILayout.IntField("Refill Dakikasi", _energyConfig.refillMinutes);
            _energyConfig.bypassForPremiumMembership = EditorGUILayout.Toggle("Premium Enerji Bypass", _energyConfig.bypassForPremiumMembership);
            _energyConfig.startingHints = EditorGUILayout.IntField("Baslangic Ipucu", _energyConfig.startingHints);
            _energyConfig.startingSoftCurrency = EditorGUILayout.IntField("Baslangic Coin", _energyConfig.startingSoftCurrency);
        }

        private void Save()
        {
            WordSpinAlphaRuntimeConfigRepository.SaveThemes(_themeCatalog);
            WordSpinAlphaRuntimeConfigRepository.SaveStoreCatalog(_storeCatalog);
            WordSpinAlphaRuntimeConfigRepository.SaveMembershipProfile(_membershipProfile);
            WordSpinAlphaRuntimeConfigRepository.SaveEnergyConfig(_energyConfig);
            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.RuntimeConfig);
            WordSpinAlphaEditorRuntimeRefreshUtility.SaveDirtyAssets();
            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.ApplyContentAndConfigRefresh(true);
            }
            else
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.MarkCurrentSceneDirty();
            }
        }

        private static void SyncFoldouts(List<bool> list, int count)
        {
            while (list.Count < count)
            {
                list.Add(false);
            }

            while (list.Count > count)
            {
                list.RemoveAt(list.Count - 1);
            }
        }

        private static void RemoveAt<T>(ref T[] source, int index)
        {
            List<T> items = new List<T>(source);
            items.RemoveAt(index);
            source = items.ToArray();
        }
    }
}
