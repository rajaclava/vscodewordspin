using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WordSpinAlpha.Editor
{
    [InitializeOnLoad]
    internal static class HubPreviewTopBarAutoSync
    {
        private const string SourceFolder = "Assets/WordSpinAlpha/Art/UI/LevelHubPreview/Source";
        private const string PrefabPath = "Assets/WordSpinAlpha/Generated/Prefabs/LevelHubPreview.prefab";
        private const string HubPreviewScenePath = "Assets/WordSpinAlpha/Scenes/HubPreview.unity";
        private const string PreferredTopBarSpritePath = SourceFolder + "/hubpreview_topbar.png";
        private const string PreferredTopBarSpritePathAlt = SourceFolder + "/hubpreview_ustbar.png";

        static HubPreviewTopBarAutoSync()
        {
            EditorApplication.delayCall += SyncNow;
        }

        internal static Sprite ResolveTopBarSprite()
        {
            Sprite preferred = AssetDatabase.LoadAssetAtPath<Sprite>(PreferredTopBarSpritePath);
            if (preferred != null)
            {
                return preferred;
            }

            Sprite preferredAlt = AssetDatabase.LoadAssetAtPath<Sprite>(PreferredTopBarSpritePathAlt);
            if (preferredAlt != null)
            {
                return preferredAlt;
            }

            string[] spriteGuids = AssetDatabase.FindAssets("t:Sprite", new[] { SourceFolder });
            Array.Sort(spriteGuids, StringComparer.Ordinal);

            for (int i = 0; i < spriteGuids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(spriteGuids[i]);
                if (!IsTopBarCandidate(path))
                {
                    continue;
                }

                Sprite candidate = AssetDatabase.LoadAssetAtPath<Sprite>(path);
                if (candidate != null)
                {
                    return candidate;
                }
            }

            return null;
        }

        internal static void SyncNow()
        {
            Sprite sprite = ResolveTopBarSprite();
            if (sprite == null)
            {
                return;
            }

            bool prefabChanged = TrySyncPrefab(sprite);
            bool sceneChanged = TrySyncOpenHubPreviewScene(sprite);

            if (prefabChanged)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.ImportAsset(PrefabPath, ImportAssetOptions.ForceSynchronousImport);
            }

            if (sceneChanged)
            {
                SceneView.RepaintAll();
            }
        }

        private static bool TrySyncPrefab(Sprite sprite)
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            if (prefabRoot == null)
            {
                return false;
            }

            try
            {
                Image topBarImage = FindTopBarImage(prefabRoot.transform);
                if (topBarImage == null)
                {
                    return false;
                }

                bool changed = false;
                if (topBarImage.sprite != sprite)
                {
                    topBarImage.sprite = sprite;
                    changed = true;
                }

                if (topBarImage.color != Color.white)
                {
                    topBarImage.color = Color.white;
                    changed = true;
                }

                if (!changed)
                {
                    return false;
                }

                PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
                return true;
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static bool TrySyncOpenHubPreviewScene(Sprite sprite)
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || !string.Equals(scene.path, HubPreviewScenePath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            GameObject[] roots = scene.GetRootGameObjects();
            bool changed = false;
            for (int i = 0; i < roots.Length; i++)
            {
                Image topBarImage = FindTopBarImage(roots[i].transform);
                if (topBarImage == null)
                {
                    continue;
                }

                if (topBarImage.sprite != sprite)
                {
                    Undo.RecordObject(topBarImage, "Sync HubPreview TopBar Sprite");
                    topBarImage.sprite = sprite;
                    changed = true;
                }

                if (topBarImage.color != Color.white)
                {
                    Undo.RecordObject(topBarImage, "Sync HubPreview TopBar Color");
                    topBarImage.color = Color.white;
                    changed = true;
                }
            }

            if (changed)
            {
                EditorSceneManager.MarkSceneDirty(scene);
            }

            return changed;
        }

        private static Image FindTopBarImage(Transform root)
        {
            if (root == null)
            {
                return null;
            }

            Transform topBar = root.Find("TopBarWidget");
            if (topBar == null)
            {
                topBar = root.Find("LevelHubPreviewRoot/TopBarWidget");
            }

            return topBar != null ? topBar.GetComponent<Image>() : null;
        }

        private static bool IsTopBarCandidate(string path)
        {
            if (string.IsNullOrWhiteSpace(path) || !path.StartsWith(SourceFolder, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            string fileName = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
            return fileName.Contains("topbar") || fileName.Contains("ustbar");
        }
    }

    internal sealed class HubPreviewTopBarAssetPostprocessor : AssetPostprocessor
    {
        private const string SourceFolder = "Assets/WordSpinAlpha/Art/UI/LevelHubPreview/Source";

        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            if (!HasTopBarAssetChange(importedAssets) && !HasTopBarAssetChange(movedAssets) && !HasTopBarAssetChange(deletedAssets))
            {
                return;
            }

            EditorApplication.delayCall += HubPreviewTopBarAutoSync.SyncNow;
        }

        private static bool HasTopBarAssetChange(string[] assets)
        {
            if (assets == null)
            {
                return false;
            }

            for (int i = 0; i < assets.Length; i++)
            {
                string path = assets[i];
                if (string.IsNullOrWhiteSpace(path) || !path.StartsWith(SourceFolder, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (!path.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                string fileName = System.IO.Path.GetFileNameWithoutExtension(path).ToLowerInvariant();
                if (fileName.Contains("topbar") || fileName.Contains("ustbar") || fileName == "hubpreview_topbar" || fileName == "hubpreview_ustbar")
                {
                    return true;
                }
            }

            return false;
        }
    }
}
