using System.IO;
using UnityEditor;
using UnityEngine;
using WordSpinAlpha.Content;

namespace WordSpinAlpha.Editor
{
    public static class WordSpinAlphaRuntimeConfigRepository
    {
        private const string RootContentPath = "Assets/WordSpinAlpha/Resources/Content";
        private const string RootConfigPath = "Assets/WordSpinAlpha/Resources/Configs";
        private const string ThemesPath = RootContentPath + "/themes.json";
        private const string StoreCatalogPath = RootConfigPath + "/store_catalog.json";
        private const string MembershipPath = RootConfigPath + "/membership_profile.json";
        private const string EnergyConfigPath = RootConfigPath + "/energy_config.json";
        private const string RemoteManifestTemplatePath = RootConfigPath + "/remote_manifest_template.json";
        private const string DifficultyProfilesPath = RootContentPath + "/difficulty_profiles.json";
        private const string DifficultyTiersPath = RootContentPath + "/difficulty_tiers.json";
        private const string RhythmProfilesPath = RootContentPath + "/rhythm_profiles.json";
        private const string ShapeLayoutsPath = RootContentPath + "/shape_layouts.json";

        public static ThemeCatalog LoadThemes()
        {
            return LoadJson<ThemeCatalog>(ThemesPath);
        }

        public static void SaveThemes(ThemeCatalog catalog)
        {
            WriteJson(ThemesPath, catalog ?? new ThemeCatalog());
        }

        public static StoreCatalogDefinition LoadStoreCatalog()
        {
            return LoadJson<StoreCatalogDefinition>(StoreCatalogPath);
        }

        public static void SaveStoreCatalog(StoreCatalogDefinition catalog)
        {
            WriteJson(StoreCatalogPath, catalog ?? new StoreCatalogDefinition());
        }

        public static MembershipProfileDefinition LoadMembershipProfile()
        {
            return LoadJson<MembershipProfileDefinition>(MembershipPath);
        }

        public static void SaveMembershipProfile(MembershipProfileDefinition profile)
        {
            WriteJson(MembershipPath, profile ?? new MembershipProfileDefinition());
        }

        public static EnergyConfigDefinition LoadEnergyConfig()
        {
            return LoadJson<EnergyConfigDefinition>(EnergyConfigPath);
        }

        public static void SaveEnergyConfig(EnergyConfigDefinition config)
        {
            WriteJson(EnergyConfigPath, config ?? new EnergyConfigDefinition());
        }

        public static RemoteContentManifestDefinition LoadRemoteManifestTemplate()
        {
            return LoadJson<RemoteContentManifestDefinition>(RemoteManifestTemplatePath);
        }

        public static void SaveRemoteManifestTemplate(RemoteContentManifestDefinition manifest)
        {
            WriteJson(RemoteManifestTemplatePath, manifest ?? new RemoteContentManifestDefinition());
        }

        public static DifficultyCatalog LoadDifficultyProfiles()
        {
            return LoadJson<DifficultyCatalog>(DifficultyProfilesPath);
        }

        public static void SaveDifficultyProfiles(DifficultyCatalog catalog)
        {
            WriteJson(DifficultyProfilesPath, catalog ?? new DifficultyCatalog());
        }

        public static DifficultyTierCatalog LoadDifficultyTiers()
        {
            return LoadJson<DifficultyTierCatalog>(DifficultyTiersPath);
        }

        public static void SaveDifficultyTiers(DifficultyTierCatalog catalog)
        {
            WriteJson(DifficultyTiersPath, catalog ?? new DifficultyTierCatalog());
        }

        public static RhythmCatalog LoadRhythmProfiles()
        {
            return LoadJson<RhythmCatalog>(RhythmProfilesPath);
        }

        public static void SaveRhythmProfiles(RhythmCatalog catalog)
        {
            WriteJson(RhythmProfilesPath, catalog ?? new RhythmCatalog());
        }

        public static ShapeLayoutCatalog LoadShapeLayouts()
        {
            return LoadJson<ShapeLayoutCatalog>(ShapeLayoutsPath);
        }

        public static void SaveShapeLayouts(ShapeLayoutCatalog catalog)
        {
            WriteJson(ShapeLayoutsPath, catalog ?? new ShapeLayoutCatalog());
        }

        private static T LoadJson<T>(string assetPath) where T : class, new()
        {
            if (!File.Exists(assetPath))
            {
                return new T();
            }

            string raw = File.ReadAllText(assetPath);
            if (string.IsNullOrWhiteSpace(raw))
            {
                return new T();
            }

            T parsed = JsonUtility.FromJson<T>(raw);
            return parsed ?? new T();
        }

        private static void WriteJson<T>(string assetPath, T payload)
        {
            string folder = Path.GetDirectoryName(assetPath);
            if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            File.WriteAllText(assetPath, JsonUtility.ToJson(payload, true));
            AssetDatabase.Refresh();
        }
    }
}
