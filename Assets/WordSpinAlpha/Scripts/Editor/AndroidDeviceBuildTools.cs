using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace WordSpinAlpha.Editor
{
    public static class AndroidDeviceBuildTools
    {
        private const string BuildRoot = "Builds/Android";
        private const string ApkName = "WordSpinAlpha-AlphaDemo.apk";

        [MenuItem("Tools/WordSpin Alpha/Android/Build APK (Device Test)")]
        public static void BuildDeviceTestApk()
        {
            BuildInternal(autoRunPlayer: false);
        }

        [MenuItem("Tools/WordSpin Alpha/Android/Build And Run APK (USB Device)")]
        public static void BuildAndRunDeviceTestApk()
        {
            BuildInternal(autoRunPlayer: true);
        }

        [MenuItem("Tools/WordSpin Alpha/Android/Open Build Folder")]
        public static void OpenBuildFolder()
        {
            string absoluteBuildRoot = Path.Combine(Directory.GetCurrentDirectory(), BuildRoot);
            Directory.CreateDirectory(absoluteBuildRoot);
            EditorUtility.RevealInFinder(absoluteBuildRoot);
        }

        private static void BuildInternal(bool autoRunPlayer)
        {
            string[] enabledScenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (enabledScenes.Length == 0)
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", "Build icin etkin sahne bulunamadi.", "OK");
                return;
            }

            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
            {
                bool switched = EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
                if (!switched)
                {
                    EditorUtility.DisplayDialog("WordSpin Alpha", "Android build target'a gecilemedi.", "OK");
                    return;
                }
            }

            string absoluteBuildRoot = Path.Combine(Directory.GetCurrentDirectory(), BuildRoot);
            Directory.CreateDirectory(absoluteBuildRoot);
            string absoluteApkPath = Path.Combine(absoluteBuildRoot, ApkName);

            BuildOptions options = BuildOptions.Development;
            if (autoRunPlayer)
            {
                options |= BuildOptions.AutoRunPlayer;
            }

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = enabledScenes,
                locationPathName = absoluteApkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = options
            };

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log($"[WordSpinAlpha] Android APK hazir: {absoluteApkPath}");
                EditorUtility.DisplayDialog(
                    "WordSpin Alpha",
                    autoRunPlayer
                        ? $"Build tamamlandi ve USB cihaza gonderme denendi.\n\n{absoluteApkPath}"
                        : $"Build tamamlandi.\n\n{absoluteApkPath}",
                    "OK");
                return;
            }

            EditorUtility.DisplayDialog(
                "WordSpin Alpha",
                $"Android build basarisiz oldu.\nResult: {summary.result}\nErrors: {summary.totalErrors}",
                "OK");
        }
    }
}
