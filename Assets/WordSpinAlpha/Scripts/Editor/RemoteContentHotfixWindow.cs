using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Content;
using WordSpinAlpha.Core;

namespace WordSpinAlpha.Editor
{
    public class RemoteContentHotfixWindow : EditorWindow
    {
        private const string ContentRoot = "Assets/WordSpinAlpha/Resources/Content";
        private const string ConfigRoot = "Assets/WordSpinAlpha/Resources/Configs";

        private static readonly CatalogEntry[] CatalogEntries =
        {
            new CatalogEntry("levels.json", Path.Combine(ContentRoot, "levels.json"), true),
            new CatalogEntry("questions.json", Path.Combine(ContentRoot, "questions.json"), true),
            new CatalogEntry("info_cards.json", Path.Combine(ContentRoot, "info_cards.json"), true),
            new CatalogEntry("themes.json", Path.Combine(ContentRoot, "themes.json"), false),
            new CatalogEntry("campaigns.json", Path.Combine(ContentRoot, "campaigns.json"), false),
            new CatalogEntry("difficulty_profiles.json", Path.Combine(ContentRoot, "difficulty_profiles.json"), false),
            new CatalogEntry("difficulty_tiers.json", Path.Combine(ContentRoot, "difficulty_tiers.json"), false),
            new CatalogEntry("rhythm_profiles.json", Path.Combine(ContentRoot, "rhythm_profiles.json"), false),
            new CatalogEntry("shape_layouts.json", Path.Combine(ContentRoot, "shape_layouts.json"), false),
            new CatalogEntry("energy_config.json", Path.Combine(ConfigRoot, "energy_config.json"), false),
            new CatalogEntry("keyboard_config.json", Path.Combine(ConfigRoot, "keyboard_config.json"), false),
            new CatalogEntry("store_catalog.json", Path.Combine(ConfigRoot, "store_catalog.json"), false),
            new CatalogEntry("membership_profile.json", Path.Combine(ConfigRoot, "membership_profile.json"), false)
        };

        private readonly Dictionary<string, bool> _selectedCatalogs = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        private Vector2 _scroll;
        private RemoteContentManifestDefinition _manifest;
        private bool _copyLocalizedContent = true;
        private WordSpinAlphaEditorSyncStamp _syncStamp;

        [MenuItem("Tools/WordSpin Alpha/Tuning/Uzak Icerik ve Hotfix Ayarlari")]
        public static void Open()
        {
            GetWindow<RemoteContentHotfixWindow>("Uzak Icerik");
        }

        private void OnEnable()
        {
            Reload();
            _syncStamp = WordSpinAlphaEditorSyncUtility.CaptureCurrentStamp();
        }

        private void OnGUI()
        {
            TryAutoRefresh();

            EditorGUILayout.LabelField("Uzak Icerik ve Hotfix Ayarlari", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Bu pencere local manifest sablonunu ve secili kataloglari remote override klasorune yazar. Oyun mekanigine dokunmaz; mevcut remote content provider uzerinden calisir.", MessageType.Info);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Sablonu Yeniden Yukle", GUILayout.Height(28f)))
                {
                    Reload();
                }

                if (GUILayout.Button("Manifesti Kaydet", GUILayout.Height(28f)))
                {
                    SaveTemplate();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Manifesti Uzak Klasore Yayinla", GUILayout.Height(28f)))
                {
                    PublishManifestOnly();
                }

                if (GUILayout.Button("Secili Kataloglari da Yayinla", GUILayout.Height(28f)))
                {
                    PublishSelectedCatalogs();
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Uzak Klasoru Temizle", GUILayout.Height(26f)))
                {
                    ClearRemoteDirectory();
                }

                using (new EditorGUI.DisabledScope(!Application.isPlaying))
                {
                    if (GUILayout.Button("Playde Remote Override Yenile", GUILayout.Height(26f)))
                    {
                        WordSpinAlphaEditorRuntimeRefreshUtility.RefreshRemoteOverrides();
                    }
                }
            }

            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Uzak Klasor", RemoteRootPath);
            EditorGUILayout.Space(4f);

            _scroll = EditorGUILayout.BeginScrollView(_scroll);

            _manifest.manifestVersion = EditorGUILayout.TextField("Manifest Versiyonu", _manifest.manifestVersion);
            _manifest.minClientVersion = EditorGUILayout.TextField("Min Client Versiyonu", _manifest.minClientVersion);
            _manifest.publishedAtUtc = EditorGUILayout.TextField("Yayin Tarihi (UTC)", _manifest.publishedAtUtc);
            _manifest.remoteContentEnabled = EditorGUILayout.Toggle("Remote Icerik Acik", _manifest.remoteContentEnabled);

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Katalog Secimi", EditorStyles.boldLabel);
            _copyLocalizedContent = EditorGUILayout.Toggle("Locale Dosyalarini da Kopyala", _copyLocalizedContent);

            for (int i = 0; i < CatalogEntries.Length; i++)
            {
                CatalogEntry entry = CatalogEntries[i];
                bool selected = _selectedCatalogs.TryGetValue(entry.catalogName, out bool current) && current;
                selected = EditorGUILayout.ToggleLeft(BuildCatalogLabel(entry), selected);
                _selectedCatalogs[entry.catalogName] = selected;
            }

            EditorGUILayout.EndScrollView();
        }

        private void Reload()
        {
            _manifest = WordSpinAlphaRuntimeConfigRepository.LoadRemoteManifestTemplate();
            if (_manifest == null)
            {
                _manifest = new RemoteContentManifestDefinition();
            }

            HashSet<string> selected = new HashSet<string>(_manifest.availableCatalogs ?? Array.Empty<string>(), StringComparer.OrdinalIgnoreCase);
            _selectedCatalogs.Clear();
            for (int i = 0; i < CatalogEntries.Length; i++)
            {
                _selectedCatalogs[CatalogEntries[i].catalogName] = selected.Contains(CatalogEntries[i].catalogName);
            }
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

        private void SaveTemplate()
        {
            _manifest.availableCatalogs = BuildSelectedCatalogArray();
            WordSpinAlphaRuntimeConfigRepository.SaveRemoteManifestTemplate(_manifest);
            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.RuntimeConfig);
            WordSpinAlphaEditorRuntimeRefreshUtility.SaveDirtyAssets();
        }

        private void PublishManifestOnly()
        {
            SaveTemplate();
            EnsureRemoteRoot();
            _manifest.publishedAtUtc = DateTime.UtcNow.ToString("O");
            File.WriteAllText(Path.Combine(RemoteRootPath, GameConstants.RemoteManifestFileName), JsonUtility.ToJson(_manifest, true));
            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshRemoteOverrides();
            }
        }

        private void PublishSelectedCatalogs()
        {
            SaveTemplate();
            EnsureRemoteRoot();
            PublishManifestOnly();

            for (int i = 0; i < CatalogEntries.Length; i++)
            {
                CatalogEntry entry = CatalogEntries[i];
                if (!_selectedCatalogs.TryGetValue(entry.catalogName, out bool selected) || !selected)
                {
                    continue;
                }

                PublishCatalog(entry);
            }

            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshRemoteOverrides();
            }
        }

        private void PublishCatalog(CatalogEntry entry)
        {
            if (File.Exists(entry.sourceAssetPath))
            {
                File.Copy(entry.sourceAssetPath, Path.Combine(RemoteRootPath, entry.catalogName), true);
            }

            if (!entry.isLocalized || !_copyLocalizedContent)
            {
                return;
            }

            string localeRoot = Path.Combine(ContentRoot, "Locales");
            for (int i = 0; i < Lang.All.Length; i++)
            {
                string language = Lang.All[i];
                string localPath = Path.Combine(localeRoot, language, entry.catalogName);
                if (!File.Exists(localPath))
                {
                    continue;
                }

                string targetDirectory = Path.Combine(RemoteRootPath, "Locales", language);
                if (!Directory.Exists(targetDirectory))
                {
                    Directory.CreateDirectory(targetDirectory);
                }

                File.Copy(localPath, Path.Combine(targetDirectory, entry.catalogName), true);
            }
        }

        private void ClearRemoteDirectory()
        {
            if (!Directory.Exists(RemoteRootPath))
            {
                return;
            }

            Directory.Delete(RemoteRootPath, true);
            Directory.CreateDirectory(RemoteRootPath);
            WordSpinAlphaEditorSyncUtility.NotifyChanged(WordSpinAlphaEditorSyncKind.RuntimeConfig);

            if (Application.isPlaying)
            {
                WordSpinAlphaEditorRuntimeRefreshUtility.RefreshRemoteOverrides();
            }
        }

        private void EnsureRemoteRoot()
        {
            if (!Directory.Exists(RemoteRootPath))
            {
                Directory.CreateDirectory(RemoteRootPath);
            }
        }

        private string[] BuildSelectedCatalogArray()
        {
            List<string> selected = new List<string>();
            foreach (KeyValuePair<string, bool> pair in _selectedCatalogs)
            {
                if (pair.Value)
                {
                    selected.Add(pair.Key);
                }
            }

            selected.Sort(StringComparer.OrdinalIgnoreCase);
            return selected.ToArray();
        }

        private static string BuildCatalogLabel(CatalogEntry entry)
        {
            return entry.isLocalized
                ? $"{entry.catalogName} (locale destekli)"
                : entry.catalogName;
        }

        private static string RemoteRootPath => Path.Combine(Application.persistentDataPath, GameConstants.RemoteContentDirectory);

        private readonly struct CatalogEntry
        {
            public CatalogEntry(string catalogName, string sourceAssetPath, bool isLocalized)
            {
                this.catalogName = catalogName;
                this.sourceAssetPath = sourceAssetPath;
                this.isLocalized = isLocalized;
            }

            public readonly string catalogName;
            public readonly string sourceAssetPath;
            public readonly bool isLocalized;
        }
    }
}
