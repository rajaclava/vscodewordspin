using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Services;

namespace WordSpinAlpha.Editor
{
    public class ValidationAuditWindow : EditorWindow
    {
        private readonly List<AuditIssue> _issues = new List<AuditIssue>();
        private static readonly HashSet<string> ValidRemoteCatalogNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "levels.json",
            "questions.json",
            "info_cards.json",
            "themes.json",
            "campaigns.json",
            "difficulty_profiles.json",
            "difficulty_tiers.json",
            "rhythm_profiles.json",
            "shape_layouts.json",
            "energy_config.json",
            "keyboard_config.json",
            "store_catalog.json",
            "membership_profile.json"
        };
        private Vector2 _scroll;
        private bool _showErrors = true;
        private bool _showWarnings = true;
        private bool _showInfos = true;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Global Validation ve Referans Taramasi")]
        public static void Open()
        {
            GetWindow<ValidationAuditWindow>("Validation");
        }

        private void OnEnable()
        {
            RunAudit();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Global Validation ve Referans Taramasi", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Icerik, tema kaynaklari, fiyat katalogu ve temel config alanlarini tarar. Bu pencere play gerektirmez.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Tum Taramayi Calistir", GUILayout.Height(28f)))
                {
                    RunAudit();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                _showErrors = EditorGUILayout.ToggleLeft("Hatalar", _showErrors, GUILayout.Width(90f));
                _showWarnings = EditorGUILayout.ToggleLeft("Uyarilar", _showWarnings, GUILayout.Width(90f));
                _showInfos = EditorGUILayout.ToggleLeft("Bilgi", _showInfos, GUILayout.Width(90f));
                GUILayout.FlexibleSpace();
                EditorGUILayout.LabelField($"Toplam: {_issues.Count}", GUILayout.Width(100f));
            }

            _scroll = EditorGUILayout.BeginScrollView(_scroll);
            foreach (AuditIssue issue in _issues)
            {
                if (!ShouldShow(issue))
                {
                    continue;
                }

                MessageType messageType = issue.severity == "Hata"
                    ? MessageType.Error
                    : issue.severity == "Uyari"
                        ? MessageType.Warning
                        : MessageType.Info;
                EditorGUILayout.HelpBox($"[{issue.area}] {issue.message}", messageType);
            }
            EditorGUILayout.EndScrollView();
        }

        private bool ShouldShow(AuditIssue issue)
        {
            return (issue.severity == "Hata" && _showErrors)
                || (issue.severity == "Uyari" && _showWarnings)
                || (issue.severity == "Bilgi" && _showInfos);
        }

        private void RunAudit()
        {
            _issues.Clear();

            WordSpinContentEditorDocument document = WordSpinAlphaContentEditorRepository.LoadDocument();
            List<ContentEditorValidationIssue> contentIssues = WordSpinAlphaContentEditorRepository.Validate(document);
            for (int i = 0; i < contentIssues.Count; i++)
            {
                _issues.Add(new AuditIssue(contentIssues[i].severity, contentIssues[i].area, contentIssues[i].message));
            }

            AuditThemeAndStore();
            AuditDifficultyAndRhythm();
            AuditShapeCatalog();
            AuditEnergyConfig();
            AuditRemoteManifestTemplate();
            AuditTelemetryPolicy();

            if (_issues.Count == 0)
            {
                _issues.Add(new AuditIssue("Bilgi", "Tarama", "Sorun bulunmadi."));
            }
        }

        private void AuditThemeAndStore()
        {
            ThemeCatalog themes = WordSpinAlphaRuntimeConfigRepository.LoadThemes();
            StoreCatalogDefinition store = WordSpinAlphaRuntimeConfigRepository.LoadStoreCatalog();
            HashSet<string> themeIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (ThemePackDefinition theme in themes.themes ?? Array.Empty<ThemePackDefinition>())
            {
                if (theme == null)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(theme.themeId))
                {
                    _issues.Add(new AuditIssue("Hata", "Tema", "Bos themeId bulundu."));
                    continue;
                }

                if (!themeIds.Add(theme.themeId))
                {
                    _issues.Add(new AuditIssue("Hata", $"Tema {theme.themeId}", "Ayni themeId birden fazla kullaniliyor."));
                }

                ValidateHex($"Tema {theme.themeId}", "uiPrimaryHex", theme.uiPrimaryHex);
                ValidateHex($"Tema {theme.themeId}", "uiAccentHex", theme.uiAccentHex);
                ValidateHex($"Tema {theme.themeId}", "uiBackgroundHex", theme.uiBackgroundHex);
                ValidateResourcePath($"Tema {theme.themeId}", "backgroundResourcePath", theme.backgroundResourcePath, typeof(Sprite));
                ValidateResourcePath($"Tema {theme.themeId}", "rotatorResourcePath", theme.rotatorResourcePath, typeof(Sprite));
                ValidateResourcePath($"Tema {theme.themeId}", "pinResourcePath", theme.pinResourcePath, typeof(Sprite));
                ValidateResourcePath($"Tema {theme.themeId}", "bgmResourcePath", theme.bgmResourcePath, typeof(AudioClip));
            }

            foreach (ThemePriceDefinition themePrice in store.themes ?? Array.Empty<ThemePriceDefinition>())
            {
                if (string.IsNullOrWhiteSpace(themePrice.themeId) || !themeIds.Contains(themePrice.themeId))
                {
                    _issues.Add(new AuditIssue("Hata", "Store Tema Fiyati", $"Tema fiyat kaydi gecerli bir themeId bulamadi: {themePrice.themeId}"));
                }

                if (themePrice.softCurrencyPrice < 0)
                {
                    _issues.Add(new AuditIssue("Hata", $"Store Tema {themePrice.themeId}", "Negatif coin fiyati tanimli."));
                }
            }

            if (string.IsNullOrWhiteSpace(store.premiumMembershipProductId))
            {
                _issues.Add(new AuditIssue("Uyari", "Store", "Premium uyelik product id bos."));
            }

            if (string.IsNullOrWhiteSpace(store.noAdsProductId))
            {
                _issues.Add(new AuditIssue("Uyari", "Store", "No ads product id bos."));
            }
        }

        private void AuditDifficultyAndRhythm()
        {
            DifficultyCatalog difficultyCatalog = WordSpinAlphaRuntimeConfigRepository.LoadDifficultyProfiles();
            DifficultyTierCatalog tierCatalog = WordSpinAlphaRuntimeConfigRepository.LoadDifficultyTiers();
            RhythmCatalog rhythmCatalog = WordSpinAlphaRuntimeConfigRepository.LoadRhythmProfiles();

            ValidateUniqueIds("Difficulty Profili", difficultyCatalog.profiles, profile => profile.difficultyProfileId);
            ValidateUniqueIds("Difficulty Tier", tierCatalog.tiers, tier => tier.difficultyTierId);
            ValidateUniqueIds("Rhythm Profili", rhythmCatalog.profiles, profile => profile.rhythmProfileId);
        }

        private void AuditShapeCatalog()
        {
            ShapeLayoutCatalog shapeCatalog = WordSpinAlphaRuntimeConfigRepository.LoadShapeLayouts();
            ValidateUniqueIds("Shape Layout", shapeCatalog.layouts, shape => shape.shapeLayoutId);

            foreach (ShapeLayoutDefinition shape in shapeCatalog.layouts ?? Array.Empty<ShapeLayoutDefinition>())
            {
                if (shape == null)
                {
                    continue;
                }

                if (shape.slotCount <= 0)
                {
                    _issues.Add(new AuditIssue("Hata", $"Shape {shape.shapeLayoutId}", "slotCount sifir veya negatif."));
                }

                if (!string.IsNullOrWhiteSpace(shape.editorReferenceImagePath))
                {
                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(shape.editorReferenceImagePath);
                    if (texture == null)
                    {
                        _issues.Add(new AuditIssue("Uyari", $"Shape {shape.shapeLayoutId}", $"Referans gorsel bulunamadi: {shape.editorReferenceImagePath}"));
                    }
                }
            }
        }

        private void AuditEnergyConfig()
        {
            EnergyConfigDefinition energy = WordSpinAlphaRuntimeConfigRepository.LoadEnergyConfig();
            if (energy.maxEnergy <= 0)
            {
                _issues.Add(new AuditIssue("Hata", "Enerji Config", "Maks enerji sifir veya negatif."));
            }

            if (energy.refillMinutes <= 0)
            {
                _issues.Add(new AuditIssue("Hata", "Enerji Config", "Refill dakikasi sifir veya negatif."));
            }
        }

        private void AuditRemoteManifestTemplate()
        {
            RemoteContentManifestDefinition manifest = WordSpinAlphaRuntimeConfigRepository.LoadRemoteManifestTemplate();
            if (manifest == null)
            {
                _issues.Add(new AuditIssue("Uyari", "Remote Manifest", "Manifest sablonu bulunamadi."));
                return;
            }

            foreach (string catalogName in manifest.availableCatalogs ?? Array.Empty<string>())
            {
                if (!ValidRemoteCatalogNames.Contains(catalogName))
                {
                    _issues.Add(new AuditIssue("Uyari", "Remote Manifest", $"Tanimsiz catalog ismi var: {catalogName}"));
                }
            }
        }

        private void AuditTelemetryPolicy()
        {
            TelemetryPolicyProfile policy = Resources.Load<TelemetryPolicyProfile>(TelemetryPolicyProfile.DefaultResourcePath);
            if (policy == null)
            {
                _issues.Add(new AuditIssue("Uyari", "Telemetry", "Telemetry policy asset'i bulunamadi."));
                return;
            }

            if (policy.WriteThrottleSeconds < 0.05f)
            {
                _issues.Add(new AuditIssue("Hata", "Telemetry", "Dosya yazma araligi 0.05 sn altina dusmus."));
            }

            if (policy.MaxQueuedEvents < 100)
            {
                _issues.Add(new AuditIssue("Hata", "Telemetry", "Maks kuyruk event 100 altina dusmus."));
            }

            if (policy.MaxSnapshotLevelSummaries < 8)
            {
                _issues.Add(new AuditIssue("Hata", "Telemetry", "Snapshot ozet seviyesi 8 altina dusmus."));
            }
        }

        private void ValidateHex(string area, string fieldName, string hex)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                _issues.Add(new AuditIssue("Uyari", area, $"{fieldName} bos."));
                return;
            }

            string normalized = hex.StartsWith("#", StringComparison.Ordinal) ? hex : $"#{hex}";
            if (!ColorUtility.TryParseHtmlString(normalized, out _))
            {
                _issues.Add(new AuditIssue("Hata", area, $"{fieldName} gecersiz HEX: {hex}"));
            }
        }

        private void ValidateResourcePath(string area, string fieldName, string resourcePath, Type type)
        {
            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                _issues.Add(new AuditIssue("Uyari", area, $"{fieldName} bos."));
                return;
            }

            if (Resources.Load(resourcePath, type) == null)
            {
                _issues.Add(new AuditIssue("Uyari", area, $"{fieldName} kaynakta bulunamadi: {resourcePath}"));
            }
        }

        private void ValidateUniqueIds<T>(string areaPrefix, T[] source, Func<T, string> idGetter) where T : class
        {
            HashSet<string> ids = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (T item in source ?? Array.Empty<T>())
            {
                if (item == null)
                {
                    continue;
                }

                string id = idGetter(item);
                if (string.IsNullOrWhiteSpace(id))
                {
                    _issues.Add(new AuditIssue("Hata", areaPrefix, "Bos id bulundu."));
                    continue;
                }

                if (!ids.Add(id))
                {
                    _issues.Add(new AuditIssue("Hata", areaPrefix, $"Ayni id birden fazla kullaniliyor: {id}"));
                }
            }
        }

        private readonly struct AuditIssue
        {
            public AuditIssue(string severity, string area, string message)
            {
                this.severity = severity;
                this.area = area;
                this.message = message;
            }

            public readonly string severity;
            public readonly string area;
            public readonly string message;
        }
    }
}
