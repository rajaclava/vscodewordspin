using UnityEditor;

namespace WordSpinAlpha.Editor
{
    public sealed class MainMenuPreviewTexturePostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/WordSpinAlpha/Art/UI/MainMenu/Source/") &&
                !assetPath.StartsWith("Assets/WordSpinAlpha/Art/UI/MainMenu/Cropped/") &&
                !assetPath.StartsWith("Assets/WordSpinAlpha/Art/UI/LevelHubPreview/Source/"))
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.sRGBTexture = true;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            ApplyUncompressedDefault(importer);
            ApplyUncompressedPlatform(importer, "Standalone");
            ApplyUncompressedPlatform(importer, "Android");
            ApplyUncompressedPlatform(importer, "iPhone");
        }

        private static void ApplyUncompressedDefault(TextureImporter importer)
        {
            TextureImporterPlatformSettings settings = importer.GetDefaultPlatformTextureSettings();
            settings.maxTextureSize = 2048;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.crunchedCompression = false;
            importer.SetPlatformTextureSettings(settings);
        }

        private static void ApplyUncompressedPlatform(TextureImporter importer, string platform)
        {
            TextureImporterPlatformSettings settings = importer.GetPlatformTextureSettings(platform);
            settings.overridden = true;
            settings.maxTextureSize = 2048;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.crunchedCompression = false;
            importer.SetPlatformTextureSettings(settings);
        }
    }
}
