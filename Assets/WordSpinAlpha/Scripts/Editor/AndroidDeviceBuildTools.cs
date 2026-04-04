using System;
using System.IO;
using System.Linq;
using System.Text;
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

        [MenuItem("Tools/WordSpin Alpha/Android/Check Connected USB Devices")]
        public static void CheckConnectedUsbDevices()
        {
            if (!TryGetAdbPath(out string adbPath, out string adbError))
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", adbError, "OK");
                return;
            }

            if (!TryQueryConnectedDevice(adbPath, out string deviceSerial, out string deviceMessage))
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", deviceMessage, "OK");
                return;
            }

            EditorUtility.DisplayDialog("WordSpin Alpha", $"USB cihaz hazir:\n{deviceSerial}", "OK");
        }

        [MenuItem("Tools/WordSpin Alpha/Android/Restart ADB Server")]
        public static void RestartAdbServer()
        {
            if (!TryGetAdbPath(out string adbPath, out string adbError))
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", adbError, "OK");
                return;
            }

            RunProcess(adbPath, "kill-server", out _, out _);
            bool started = RunProcess(adbPath, "start-server", out int exitCode, out string output);
            if (!started || exitCode != 0)
            {
                EditorUtility.DisplayDialog("WordSpin Alpha", $"ADB server yeniden baslatilamadi.\n\n{output}", "OK");
                return;
            }

            EditorUtility.DisplayDialog("WordSpin Alpha", "ADB server yeniden baslatildi.", "OK");
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

            string adbPath = null;
            string deviceSerial = null;
            if (autoRunPlayer)
            {
                if (!TryGetAdbPath(out adbPath, out string adbError))
                {
                    EditorUtility.DisplayDialog("WordSpin Alpha", adbError, "OK");
                    return;
                }

                if (!TryQueryConnectedDevice(adbPath, out deviceSerial, out string deviceError))
                {
                    EditorUtility.DisplayDialog("WordSpin Alpha", deviceError, "OK");
                    return;
                }
            }

            BuildOptions options = BuildOptions.Development;

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
                if (autoRunPlayer)
                {
                    if (!TryInstallAndLaunchApk(adbPath, deviceSerial, absoluteApkPath, out string deployMessage))
                    {
                        EditorUtility.DisplayDialog(
                            "WordSpin Alpha",
                            $"Build tamamlandi ama cihaza gonderilemedi.\n\n{deployMessage}\n\nAPK:\n{absoluteApkPath}",
                            "OK");
                        return;
                    }
                }

                Debug.Log($"[WordSpinAlpha] Android APK hazir: {absoluteApkPath}");
                EditorUtility.DisplayDialog(
                    "WordSpin Alpha",
                    autoRunPlayer
                        ? $"Build tamamlandi ve APK USB cihaza yuklenip baslatildi.\n\nCihaz: {deviceSerial}\nAPK: {absoluteApkPath}"
                        : $"Build tamamlandi.\n\n{absoluteApkPath}",
                    "OK");
                return;
            }

            EditorUtility.DisplayDialog(
                "WordSpin Alpha",
                $"Android build basarisiz oldu.\nResult: {summary.result}\nErrors: {summary.totalErrors}",
                "OK");
        }

        private static bool TryInstallAndLaunchApk(string adbPath, string deviceSerial, string apkPath, out string message)
        {
            if (!File.Exists(apkPath))
            {
                message = "Olusan APK dosyasi bulunamadi.";
                return false;
            }

            string serialArg = string.IsNullOrWhiteSpace(deviceSerial) ? string.Empty : $"-s \"{deviceSerial}\" ";

            RunProcess(adbPath, "start-server", out _, out _);

            if (!RunProcess(adbPath, $"{serialArg}install -r \"{apkPath}\"", out int installExitCode, out string installOutput) || installExitCode != 0)
            {
                message = $"ADB install basarisiz.\n\n{installOutput}";
                return false;
            }

            string applicationId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
            if (string.IsNullOrWhiteSpace(applicationId))
            {
                message = "Android application identifier bulunamadi. APK yuklendi ama oyun baslatilamadi.";
                return false;
            }

            if (!RunProcess(adbPath, $"{serialArg}shell monkey -p {applicationId} -c android.intent.category.LAUNCHER 1", out int launchExitCode, out string launchOutput) || launchExitCode != 0)
            {
                message = $"APK yuklendi ancak oyun baslatilamadi.\n\n{launchOutput}";
                return false;
            }

            message = "APK yuklendi ve baslatildi.";
            return true;
        }

        private static bool TryQueryConnectedDevice(string adbPath, out string deviceSerial, out string message)
        {
            deviceSerial = string.Empty;

            RunProcess(adbPath, "start-server", out _, out _);
            if (!RunProcess(adbPath, "devices", out int exitCode, out string output) || exitCode != 0)
            {
                message = $"ADB devices calistirilamadi.\n\n{output}";
                return false;
            }

            string[] lines = output
                .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                .Skip(1)
                .ToArray();

            string unauthorized = lines.FirstOrDefault(line => line.Contains("\tunauthorized"));
            if (!string.IsNullOrEmpty(unauthorized))
            {
                message = "Telefon USB hata ayiklama izni bekliyor. Telefonda gelen RSA izin penceresini onayla, sonra tekrar dene.";
                return false;
            }

            string offline = lines.FirstOrDefault(line => line.Contains("\toffline"));
            if (!string.IsNullOrEmpty(offline))
            {
                message = "ADB cihazi offline goruyor. USB kablosunu yeniden tak, gerekirse 'Restart ADB Server' menu komutunu kullan.";
                return false;
            }

            string[] readyDevices = lines
                .Where(line => line.Contains("\tdevice"))
                .Select(line => line.Split('\t')[0].Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToArray();

            if (readyDevices.Length == 0)
            {
                message = "Hazir bir Android cihaz bulunamadi.\n\nKontrol et:\n- USB hata ayiklama acik\n- Telefonda RSA izni verildi\n- Kablo veri kablosu\n- Telefon sadece sarj modunda degil";
                return false;
            }

            deviceSerial = readyDevices[0];
            if (readyDevices.Length > 1)
            {
                message = $"Birden fazla cihaz bulundu. Ilk cihaz secilecek: {deviceSerial}";
                Debug.LogWarning($"[WordSpinAlpha] Birden fazla adb cihazi bulundu. Ilk cihaz secildi: {deviceSerial}");
            }
            else
            {
                message = $"Hazir cihaz bulundu: {deviceSerial}";
            }

            return true;
        }

        private static bool TryGetAdbPath(out string adbPath, out string message)
        {
            adbPath = ResolveAdbPath();
            if (!string.IsNullOrWhiteSpace(adbPath) && File.Exists(adbPath))
            {
                message = string.Empty;
                return true;
            }

            message = "ADB yolu bulunamadi. Unity Preferences > External Tools icinde Android SDK yolunun gecerli oldugunu kontrol et.";
            return false;
        }

        private static string ResolveAdbPath()
        {
            string sdkRoot = TryGetUnityAndroidSdkRoot();
            if (!string.IsNullOrWhiteSpace(sdkRoot))
            {
                string adbCandidate = Path.Combine(sdkRoot, "platform-tools", "adb.exe");
                if (File.Exists(adbCandidate))
                {
                    return adbCandidate;
                }
            }

            string androidSdkRoot = Environment.GetEnvironmentVariable("ANDROID_SDK_ROOT");
            if (!string.IsNullOrWhiteSpace(androidSdkRoot))
            {
                string adbCandidate = Path.Combine(androidSdkRoot, "platform-tools", "adb.exe");
                if (File.Exists(adbCandidate))
                {
                    return adbCandidate;
                }
            }

            string androidHome = Environment.GetEnvironmentVariable("ANDROID_HOME");
            if (!string.IsNullOrWhiteSpace(androidHome))
            {
                string adbCandidate = Path.Combine(androidHome, "platform-tools", "adb.exe");
                if (File.Exists(adbCandidate))
                {
                    return adbCandidate;
                }
            }

            return string.Empty;
        }

        private static string TryGetUnityAndroidSdkRoot()
        {
            try
            {
                Type settingsType = Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings,UnityEditor");
                if (settingsType == null)
                {
                    settingsType = Type.GetType("UnityEditor.Android.AndroidExternalToolsSettings,UnityEditor.Android.Extensions");
                }

                if (settingsType == null)
                {
                    return string.Empty;
                }

                var property = settingsType.GetProperty("sdkRootPath");
                if (property == null)
                {
                    return string.Empty;
                }

                return property.GetValue(null, null) as string ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        private static bool RunProcess(string fileName, string arguments, out int exitCode, out string output)
        {
            exitCode = -1;
            output = string.Empty;

            try
            {
                using (System.Diagnostics.Process process = new System.Diagnostics.Process())
                {
                    process.StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = fileName,
                        Arguments = arguments,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    };

                    StringBuilder builder = new StringBuilder();
                    process.Start();
                    builder.Append(process.StandardOutput.ReadToEnd());
                    builder.Append(process.StandardError.ReadToEnd());
                    process.WaitForExit();

                    exitCode = process.ExitCode;
                    output = builder.ToString().Trim();
                    return true;
                }
            }
            catch (Exception ex)
            {
                output = ex.Message;
                return false;
            }
        }
    }
}
