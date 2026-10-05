#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using Debug = UnityEngine.Debug;

/// <summary>
/// Uploads the IL2CPP symbols zip from a successful Android Player build.
/// Upload failure is logged and does not fail the build.
/// </summary>
public sealed class CrashlyticsSymbolUploadPostprocessor : IPostprocessBuildWithReport
{
    private const string EnabledPrefKey = "RushOut_AutoUploadCrashlyticsSymbols";
    private const string MenuPath = "Rush Out/Build/Auto Upload Crashlytics Symbols";
    private const string GoogleServicesRelativePath = "google-services.json";
    private const int UploadTimeoutMs = 180000;

    public int callbackOrder => 1000;

    [MenuItem(MenuPath, false, 500)]
    private static void ToggleAutoUpload()
    {
        EditorPrefs.SetBool(EnabledPrefKey, !IsAutoUploadEnabled());
    }

    [MenuItem(MenuPath, true)]
    private static bool ToggleAutoUploadValidate()
    {
        Menu.SetChecked(MenuPath, IsAutoUploadEnabled());
        return true;
    }

    public void OnPostprocessBuild(BuildReport report)
    {
        try
        {
            if (report == null || report.summary.platform != BuildTarget.Android)
            {
                return;
            }

            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.Log(
                    "[Crashlytics] Symbol upload skipped because the Android build did not succeed.");
                return;
            }

            if (!IsAutoUploadEnabled())
            {
                Debug.Log(
                    "[Crashlytics] Symbol upload skipped. " +
                    "Turn it on with Rush Out > Build > Auto Upload Crashlytics Symbols.");
                return;
            }

            string symbolsZip = FindSymbolsZip(report);
            if (string.IsNullOrEmpty(symbolsZip))
            {
                return;
            }

            string appId = ReadAndroidAppId();
            if (string.IsNullOrEmpty(appId))
            {
                Debug.LogWarning(
                    "[Crashlytics] Symbol upload skipped. Could not read mobilesdk_app_id " +
                    "for the Android package from Assets/google-services.json. " +
                    "The Android build itself succeeded.");
                return;
            }

            string firebaseCli = ResolveFirebaseCli();
            if (string.IsNullOrEmpty(firebaseCli))
            {
                string expected = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "npm",
                    "firebase.cmd");
                Debug.LogWarning(
                    "[Crashlytics] Firebase CLI not found. Expected " + expected +
                    ", or firebase.cmd / firebase on PATH. " +
                    "The Android build itself succeeded. Symbols zip: " + symbolsZip);
                return;
            }

            Upload(firebaseCli, appId, symbolsZip);
        }
        catch (Exception ex)
        {
            Debug.LogWarning(
                "[Crashlytics] Symbol upload failed with an editor exception. " +
                "The Android build itself succeeded.\n" + ex.Message);
        }
    }

    private static bool IsAutoUploadEnabled()
    {
        return EditorPrefs.GetBool(EnabledPrefKey, true);
    }

    /// <summary>
    /// Unity 6 names the package
    /// {outputFileName}-{bundleVersion}-v{versionCode}-IL2CPP.symbols.zip
    /// next to the APK/AAB. BuildReport does not expose that path.
    /// </summary>
    private static string FindSymbolsZip(BuildReport report)
    {
        string outputPath = report.summary.outputPath;
        if (string.IsNullOrEmpty(outputPath))
        {
            Debug.LogWarning(
                "[Crashlytics] Symbol upload skipped. The Android build report has no output path.");
            return null;
        }

        if (Directory.Exists(outputPath))
        {
            Debug.LogWarning(
                "[Crashlytics] Symbol upload skipped. This Android build exported a project " +
                "instead of an APK/AAB, so there is no IL2CPP symbols zip to upload.");
            return null;
        }

        string directory = Path.GetDirectoryName(outputPath);
        string stem = Path.GetFileNameWithoutExtension(outputPath);
        if (string.IsNullOrEmpty(directory) || string.IsNullOrEmpty(stem))
        {
            Debug.LogWarning(
                "[Crashlytics] Symbol upload skipped. Could not resolve the build output folder.");
            return null;
        }

        string expectedName =
            stem + "-" + PlayerSettings.bundleVersion +
            "-v" + PlayerSettings.Android.bundleVersionCode +
            "-IL2CPP.symbols.zip";
        string expectedPath = Path.Combine(directory, expectedName);

        string reported = FindReportedZip(report, expectedName);
        if (!string.IsNullOrEmpty(reported))
        {
            if (IsFreshSymbolsZip(reported, report))
            {
                return reported;
            }

            Debug.LogWarning(
                "[Crashlytics] Symbol upload skipped. Build report listed " + reported +
                " but it is older than this build.");
            return null;
        }

        if (IsFreshSymbolsZip(expectedPath, report))
        {
            return expectedPath;
        }

        if (File.Exists(expectedPath))
        {
            Debug.LogWarning(
                "[Crashlytics] Symbol upload skipped. " + expectedPath +
                " exists but was not written by this build.");
            return null;
        }

        Debug.LogWarning(
            "[Crashlytics] No IL2CPP symbols zip for this Android build. " +
            "In File > Build Profiles > Android, set Debug Symbols to Debugging " +
            "and Symbols output options to .zip. Looked for " + expectedPath);
        return null;
    }

    private static string FindReportedZip(BuildReport report, string expectedName)
    {
        BuildFile[] files;
        try
        {
            files = report.GetFiles();
        }
        catch (Exception)
        {
            return null;
        }

        if (files == null)
        {
            return null;
        }

        string match = null;
        for (int i = 0; i < files.Length; i++)
        {
            string path = files[i].path;
            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            if (!string.Equals(
                    Path.GetFileName(path),
                    expectedName,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (match != null &&
                !string.Equals(match, path, StringComparison.OrdinalIgnoreCase))
            {
                Debug.LogWarning(
                    "[Crashlytics] Symbol upload skipped. The build report listed more than one " +
                    expectedName + ".");
                return null;
            }

            match = path;
        }

        return match;
    }

    private static bool IsFreshSymbolsZip(string path, BuildReport report)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            return false;
        }

        if (!path.EndsWith("IL2CPP.symbols.zip", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        DateTime writtenUtc = File.GetLastWriteTimeUtc(path);
        DateTime startedUtc = ToUtc(report.summary.buildStartedAt);
        return writtenUtc >= startedUtc.AddMinutes(-2) &&
               writtenUtc <= DateTime.UtcNow.AddMinutes(5);
    }

    private static DateTime ToUtc(DateTime value)
    {
        if (value.Kind == DateTimeKind.Local)
        {
            return value.ToUniversalTime();
        }

        // Unity's buildStartedAt is UTC. Unspecified is treated as UTC,
        // matching BuildSummary samples that call ToLocalTime().
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }

    private static string ReadAndroidAppId()
    {
        string path = Path.Combine(Application.dataPath, GoogleServicesRelativePath);
        if (!File.Exists(path))
        {
            return null;
        }

        string json;
        try
        {
            json = File.ReadAllText(path);
        }
        catch (Exception)
        {
            return null;
        }

        string packageId = PlayerSettings.GetApplicationIdentifier(BuildTargetGroup.Android);
        string packageToken = "\"package_name\"";
        string packageValue = "\"" + packageId + "\"";
        int packageAt = json.IndexOf(packageToken, StringComparison.Ordinal);
        while (packageAt >= 0)
        {
            int valueAt = json.IndexOf(packageValue, packageAt, StringComparison.Ordinal);
            if (valueAt >= 0 && valueAt - packageAt < 80)
            {
                int windowStart = Math.Max(0, packageAt - 400);
                string window = json.Substring(windowStart, packageAt - windowStart);
                string appId = ReadJsonStringAfter(window, "\"mobilesdk_app_id\"");
                if (!string.IsNullOrEmpty(appId))
                {
                    return appId;
                }
            }

            packageAt = json.IndexOf(packageToken, packageAt + packageToken.Length, StringComparison.Ordinal);
        }

        return null;
    }

    private static string ReadJsonStringAfter(string json, string key)
    {
        int keyAt = json.LastIndexOf(key, StringComparison.Ordinal);
        if (keyAt < 0)
        {
            return null;
        }

        int colon = json.IndexOf(':', keyAt + key.Length);
        if (colon < 0)
        {
            return null;
        }

        int open = json.IndexOf('"', colon + 1);
        if (open < 0)
        {
            return null;
        }

        int close = json.IndexOf('"', open + 1);
        if (close <= open)
        {
            return null;
        }

        return json.Substring(open + 1, close - open - 1);
    }

    private static string ResolveFirebaseCli()
    {
        string appDataCli = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "npm",
            "firebase.cmd");
        if (File.Exists(appDataCli))
        {
            return appDataCli;
        }

        string fromPath = FindOnPath("firebase.cmd");
        if (!string.IsNullOrEmpty(fromPath))
        {
            return fromPath;
        }

        return FindOnPath("firebase");
    }

    private static string FindOnPath(string fileName)
    {
        string pathEnv = Environment.GetEnvironmentVariable("PATH");
        if (string.IsNullOrEmpty(pathEnv))
        {
            return null;
        }

        string[] parts = pathEnv.Split(Path.PathSeparator);
        for (int i = 0; i < parts.Length; i++)
        {
            if (string.IsNullOrEmpty(parts[i]))
            {
                continue;
            }

            string candidate = Path.Combine(parts[i].Trim(), fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }
        }

        return null;
    }

    private static void Upload(string firebaseCli, string appId, string symbolsZip)
    {
        string command =
            Quote(firebaseCli) +
            " crashlytics:symbols:upload --app=" + Quote(appId) +
            " " + Quote(symbolsZip);

        string shell = Environment.GetEnvironmentVariable("ComSpec");
        if (string.IsNullOrEmpty(shell))
        {
            shell = "cmd.exe";
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = shell,
            Arguments = "/d /s /c \"" + command + "\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            WorkingDirectory = Path.GetDirectoryName(symbolsZip) ?? Environment.CurrentDirectory
        };

        Debug.Log("[Crashlytics] Uploading IL2CPP symbols: " + symbolsZip);

        using (Process process = new Process())
        {
            process.StartInfo = startInfo;
            var stdout = new StringBuilder();
            var stderr = new StringBuilder();
            process.OutputDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    stdout.AppendLine(e.Data);
                }
            };
            process.ErrorDataReceived += (_, e) =>
            {
                if (e.Data != null)
                {
                    stderr.AppendLine(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            if (!process.WaitForExit(UploadTimeoutMs))
            {
                try
                {
                    process.Kill();
                }
                catch (Exception)
                {
                    // The process may already have exited.
                }

                Debug.LogWarning(
                    "[Crashlytics] Symbol upload timed out after " +
                    (UploadTimeoutMs / 1000) +
                    "s. The Android build itself succeeded. Symbols zip: " + symbolsZip);
                return;
            }

            process.WaitForExit();

            string output = stdout.ToString().Trim();
            string error = stderr.ToString().Trim();
            if (process.ExitCode == 0)
            {
                Debug.Log(
                    "[Crashlytics] Uploaded IL2CPP symbols for Android " +
                    PlayerSettings.bundleVersion +
                    " (version code " + PlayerSettings.Android.bundleVersionCode + ").\n" +
                    symbolsZip +
                    (string.IsNullOrEmpty(output) ? string.Empty : "\n" + output));
                return;
            }

            Debug.LogWarning(
                "[Crashlytics] Symbol upload failed with exit code " + process.ExitCode +
                ". The Android build itself succeeded.\n" +
                symbolsZip +
                (string.IsNullOrEmpty(error) ? string.Empty : "\n" + error) +
                (string.IsNullOrEmpty(output) ? string.Empty : "\n" + output));
        }
    }

    private static string Quote(string value)
    {
        return "\"" + (value ?? string.Empty).Replace("\"", "\"\"") + "\"";
    }
}
#endif
