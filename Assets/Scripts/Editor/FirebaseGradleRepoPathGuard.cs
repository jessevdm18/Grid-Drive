#if UNITY_EDITOR
using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Durable guard for Firebase EDM local Maven repo paths.
///
/// EDM Force Resolve rewrites settingsTemplate.gradle to:
///   def unityProjectPath = $/file:///**DIR_UNITYPROJECT**/$.replace(...)
///   url (unityProjectPath + "/Assets/GeneratedLocalRepo/Firebase/m2repository")
///
/// With a space in the project folder ("Rush Out"), Unity expands that to an
/// unencoded file:///C:/.../Rush Out/... URI and Gradle fails on
/// :launcher:checkReleaseDuplicateClasses.
///
/// Hooks:
/// - Menu verify/fix
/// - AssetPostprocessor after EDM regenerates the template
/// - IPreprocessBuildWithReport before Android builds
/// - IPostGenerateGradleAndroidProject on the generated settings.gradle
/// </summary>
public static class FirebaseGradleRepoPathGuard
{
    public const string SettingsTemplateRelativePath =
        "Assets/Plugins/Android/settingsTemplate.gradle";

    private const string BrokenTemplateMarker = "file:///**DIR_UNITYPROJECT**";

    private const string BrokenUnityProjectPathAssignment =
        "def unityProjectPath = $/file:///**DIR_UNITYPROJECT**/$.replace(\"\\\\\", \"/\")";

    private const string BrokenMavenUrlPrefix =
        "url (unityProjectPath + \"/Assets/GeneratedLocalRepo/Firebase/m2repository\")";

    private const string SafeUnityProjectPathAssignment =
        "def unityProjectPath = \"**DIR_UNITYPROJECT**\".replace(\"\\\\\", \"/\")";

    private const string SafeMavenUrlPrefix =
        "url uri(file(unityProjectPath + \"/Assets/GeneratedLocalRepo/Firebase/m2repository\"))";

    // Generated settings.gradle: $/file:///C:/.../Rush Out/$
    private static readonly Regex GeneratedFileUriAssignment = new Regex(
        @"def\s+unityProjectPath\s*=\s*\$/file:///(?<path>.*?)/\$\.replace\(\s*""\\\\""\s*,\s*""/""\s*\)",
        RegexOptions.Compiled);

    private static readonly Regex BrokenMavenUrlLine = new Regex(
        @"url\s*\(\s*unityProjectPath\s*\+\s*""/Assets/GeneratedLocalRepo/Firebase/m2repository""\s*\)",
        RegexOptions.Compiled);

    private static bool isPatchingTemplate;
    private static bool deferredTemplateImportScheduled;

    /// <summary>
    /// True while EDM/GMA ResolveSync (or any AssetDatabase import wave) must not be
    /// interrupted by a synchronous settingsTemplate ImportAsset/Refresh.
    /// </summary>
    public static bool SuppressSynchronousTemplateImport { get; set; }

    [MenuItem("RushOut/Release/Verify Firebase Gradle Repo Path", false, 500)]
    public static void VerifyFromMenu()
    {
        EnsureCustomGradleSettingsTemplateEnabled();
        VerifyResult templateResult = VerifyAndFixTemplate(applyFix: true);
        LogResult("template", templateResult);

        int generatedFixed = FixGeneratedSettingsGradleFiles();
        if (generatedFixed > 0)
        {
            Debug.LogWarning(
                "[FirebaseGradleRepoPath] FIXED — patched " + generatedFixed +
                " generated Library/**/settings.gradle file(s)."
            );
        }
        else
        {
            Debug.Log(
                "[FirebaseGradleRepoPath] generated settings.gradle — no broken Firebase " +
                "file:/// repos found (or not generated yet)."
            );
        }
    }

    /// <summary>
    /// Ensures Publishing Settings → Custom Gradle Settings Template is enabled.
    /// </summary>
    public static void EnsureCustomGradleSettingsTemplateEnabled()
    {
        UnityEngine.Object[] assets = AssetDatabase.LoadAllAssetsAtPath(
            "ProjectSettings/ProjectSettings.asset"
        );
        if (assets == null || assets.Length == 0)
        {
            Debug.LogError(
                "[FirebaseGradleRepoPath] Could not load ProjectSettings.asset to verify " +
                "useCustomGradleSettingsTemplate."
            );
            return;
        }

        var so = new SerializedObject(assets[0]);
        SerializedProperty prop =
            so.FindProperty("useCustomGradleSettingsTemplate") ??
            so.FindProperty("AndroidUseCustomGradleSettingsTemplate");

        if (prop == null)
        {
            Debug.LogWarning(
                "[FirebaseGradleRepoPath] useCustomGradleSettingsTemplate property not found. " +
                "Confirm Custom Gradle Settings Template is enabled in Player Settings."
            );
            return;
        }

        if (prop.propertyType == SerializedPropertyType.Boolean && !prop.boolValue)
        {
            prop.boolValue = true;
            so.ApplyModifiedPropertiesWithoutUndo();
            Debug.LogWarning(
                "[FirebaseGradleRepoPath] ENABLED useCustomGradleSettingsTemplate " +
                "(settingsTemplate.gradle must be used by Android builds)."
            );
        }
    }

    public static VerifyResult VerifyAndFixTemplate(bool applyFix)
    {
        string absolutePath = GetTemplateAbsolutePath();
        if (!File.Exists(absolutePath))
        {
            return new VerifyResult(
                VerifyStatus.MissingFile,
                SettingsTemplateRelativePath + " not found."
            );
        }

        string contents = File.ReadAllText(absolutePath);
        bool isBroken = contents.Contains(BrokenTemplateMarker) ||
                        contents.Contains(BrokenMavenUrlPrefix);
        bool isSafe = contents.Contains(SafeUnityProjectPathAssignment) &&
                      contents.Contains(SafeMavenUrlPrefix) &&
                      !contents.Contains(BrokenTemplateMarker);

        if (isSafe)
        {
            return new VerifyResult(
                VerifyStatus.Ok,
                SettingsTemplateRelativePath +
                " OK — uses space-safe uri(file(...)) Maven repo path."
            );
        }

        if (!isBroken)
        {
            return new VerifyResult(
                VerifyStatus.Unexpected,
                SettingsTemplateRelativePath +
                " does not match known EDM Firebase repo patterns. Manual review required."
            );
        }

        if (!applyFix)
        {
            return new VerifyResult(
                VerifyStatus.FixRequired,
                SettingsTemplateRelativePath +
                " FIX REQUIRED — contains broken file:///**DIR_UNITYPROJECT** Maven repo URI."
            );
        }

        string fixedContents = ApplyTemplateFix(contents);
        if (fixedContents == contents ||
            !fixedContents.Contains(SafeUnityProjectPathAssignment) ||
            !fixedContents.Contains(SafeMavenUrlPrefix) ||
            fixedContents.Contains(BrokenTemplateMarker))
        {
            return new VerifyResult(
                VerifyStatus.Unexpected,
                SettingsTemplateRelativePath +
                " looked broken but automatic rewrite failed. File not written."
            );
        }

        isPatchingTemplate = true;
        try
        {
            File.WriteAllText(absolutePath, fixedContents);

            // Never ImportAsset/Refresh while GMA/EDM ResolveSync is copying AARs —
            // that races GeneratedLocalRepo/*.aar.meta writes (build failure).
            // AssetPostprocessor sets SuppressSynchronousTemplateImport during resolve.
            // PreprocessBuild order 50 clears it after ResolveSync and imports safely.
            if (SuppressSynchronousTemplateImport)
            {
                ScheduleDeferredTemplateImport();
            }
            else
            {
                AssetDatabase.ImportAsset(SettingsTemplateRelativePath);
            }
        }
        finally
        {
            isPatchingTemplate = false;
        }

        return new VerifyResult(
            VerifyStatus.Fixed,
            SettingsTemplateRelativePath +
            " FIXED — replaced file:/// concat with uri(file(**DIR_UNITYPROJECT**/...))."
        );
    }

    public static bool IsPatchingTemplate => isPatchingTemplate;

    /// <summary>
    /// Import settingsTemplate after the current AssetDatabase import/resolve batch.
    /// Coalesces multiple schedule requests into one delayCall.
    /// </summary>
    public static void ScheduleDeferredTemplateImport()
    {
        if (deferredTemplateImportScheduled)
        {
            return;
        }

        deferredTemplateImportScheduled = true;
        EditorApplication.delayCall += FlushDeferredTemplateImport;
    }

    private static void FlushDeferredTemplateImport()
    {
        deferredTemplateImportScheduled = false;
        if (!File.Exists(GetTemplateAbsolutePath()))
        {
            return;
        }

        // Still inside player build preprocess — disk is source of truth until order 50.
        if (BuildPipeline.isBuildingPlayer)
        {
            return;
        }

        isPatchingTemplate = true;
        try
        {
            AssetDatabase.ImportAsset(SettingsTemplateRelativePath);
        }
        finally
        {
            isPatchingTemplate = false;
        }
    }

    /// <summary>
    /// Patches generated Gradle settings files under Library/Bee.
    /// Returns number of files modified.
    /// </summary>
    public static int FixGeneratedSettingsGradleFiles()
    {
        string[] candidates =
        {
            Path.Combine(
                Application.dataPath,
                "..",
                "Library",
                "Bee",
                "Android",
                "Prj",
                "IL2CPP",
                "Gradle",
                "settings.gradle"
            ),
            Path.Combine(
                Application.dataPath,
                "..",
                "Library",
                "Bee",
                "artifacts",
                "Android",
                "Gradle",
                "settings.gradle"
            )
        };

        int fixedCount = 0;
        for (int i = 0; i < candidates.Length; i++)
        {
            string path = Path.GetFullPath(candidates[i]);
            if (TryFixGeneratedSettingsGradle(path))
            {
                fixedCount++;
            }
        }

        return fixedCount;
    }

    /// <summary>
    /// Patches a single generated settings.gradle (expanded file:///C:/.../Rush Out/...).
    /// </summary>
    public static bool TryFixGeneratedSettingsGradle(string absolutePath)
    {
        if (string.IsNullOrEmpty(absolutePath) || !File.Exists(absolutePath))
        {
            return false;
        }

        string contents = File.ReadAllText(absolutePath);
        if (!contents.Contains("GeneratedLocalRepo/Firebase/m2repository"))
        {
            return false;
        }

        // Already safe?
        if (contents.Contains("url uri(file(unityProjectPath +") &&
            !GeneratedFileUriAssignment.IsMatch(contents) &&
            !contents.Contains("url (unityProjectPath +"))
        {
            return false;
        }

        // Nothing recognizable to fix?
        if (!GeneratedFileUriAssignment.IsMatch(contents) &&
            !BrokenMavenUrlLine.IsMatch(contents) &&
            !contents.Contains(BrokenMavenUrlPrefix) &&
            !contents.Contains(BrokenTemplateMarker))
        {
            return false;
        }

        string fixedContents = ApplyGeneratedFix(contents);
        if (fixedContents == contents)
        {
            return false;
        }

        if (HasUnsafeHandBuiltFileUri(fixedContents))
        {
            Debug.LogError(
                "[FirebaseGradleRepoPath] generated rewrite still contains unsafe file:/// URI: " +
                absolutePath
            );
            return false;
        }

        File.WriteAllText(absolutePath, fixedContents);
        Debug.LogWarning(
            "[FirebaseGradleRepoPath] FIXED generated — " + absolutePath
        );
        return true;
    }

    /// <summary>
    /// Called from IPostGenerateGradleAndroidProject. <paramref name="unityLibraryPath"/>
    /// is the generated unityLibrary module folder.
    /// </summary>
    public static void FixGeneratedGradleProject(string unityLibraryPath)
    {
        if (string.IsNullOrEmpty(unityLibraryPath))
        {
            return;
        }

        string settingsPath = Path.GetFullPath(
            Path.Combine(unityLibraryPath, "..", "settings.gradle")
        );
        TryFixGeneratedSettingsGradle(settingsPath);
    }

    public static bool HasUnsafeHandBuiltFileUri(string gradleContents)
    {
        if (string.IsNullOrEmpty(gradleContents))
        {
            return false;
        }

        if (gradleContents.Contains(BrokenTemplateMarker))
        {
            return true;
        }

        if (GeneratedFileUriAssignment.IsMatch(gradleContents))
        {
            return true;
        }

        if (BrokenMavenUrlLine.IsMatch(gradleContents) ||
            gradleContents.Contains(BrokenMavenUrlPrefix))
        {
            return true;
        }

        return false;
    }

    private static string ApplyTemplateFix(string contents)
    {
        string result = contents;
        if (result.Contains(BrokenUnityProjectPathAssignment))
        {
            result = result.Replace(
                BrokenUnityProjectPathAssignment,
                SafeUnityProjectPathAssignment
            );
        }

        if (result.Contains(BrokenMavenUrlPrefix))
        {
            result = result.Replace(BrokenMavenUrlPrefix, SafeMavenUrlPrefix);
        }

        return result;
    }

    private static string ApplyGeneratedFix(string contents)
    {
        string result = contents;

        result = GeneratedFileUriAssignment.Replace(
            result,
            match =>
            {
                string path = match.Groups["path"].Value;
                path = path.TrimEnd('/');
                path = path.Replace('\\', '/');
                return "def unityProjectPath = \"" + path + "\".replace(\"\\\\\", \"/\")";
            }
        );

        result = BrokenMavenUrlLine.Replace(
            result,
            "url uri(file(unityProjectPath + \"/Assets/GeneratedLocalRepo/Firebase/m2repository\"))"
        );

        if (result.Contains(BrokenMavenUrlPrefix))
        {
            result = result.Replace(BrokenMavenUrlPrefix, SafeMavenUrlPrefix);
        }

        return result;
    }

    private static string GetTemplateAbsolutePath()
    {
        return Path.GetFullPath(
            Path.Combine(Application.dataPath, "..", SettingsTemplateRelativePath)
        );
    }

    private static void LogResult(string scope, VerifyResult result)
    {
        string prefix = "[FirebaseGradleRepoPath][" + scope + "] ";
        switch (result.Status)
        {
            case VerifyStatus.Ok:
                Debug.Log(prefix + "OK — " + result.Message);
                break;
            case VerifyStatus.Fixed:
                Debug.LogWarning(prefix + "FIXED — " + result.Message);
                break;
            case VerifyStatus.FixRequired:
                Debug.LogWarning(prefix + "FIX REQUIRED — " + result.Message);
                break;
            default:
                Debug.LogError(prefix + result.Status + " — " + result.Message);
                break;
        }
    }

    public enum VerifyStatus
    {
        Ok,
        FixRequired,
        Fixed,
        MissingFile,
        Unexpected
    }

    public readonly struct VerifyResult
    {
        public VerifyStatus Status { get; }
        public string Message { get; }

        public VerifyResult(VerifyStatus status, string message)
        {
            Status = status;
            Message = message ?? string.Empty;
        }
    }
}

/// <summary>
/// Re-applies the safe repo path whenever EDM regenerates settingsTemplate.gradle.
/// Must NOT call AssetDatabase.ImportAsset/Refresh synchronously — EDM's
/// GradleTemplateResolver.CopySrcAars may still be writing GeneratedLocalRepo metas
/// in the same import/resolve wave (GMA AndroidBuildPreProcessor.ResolveSync).
/// </summary>
public sealed class FirebaseGradleRepoPathAssetPostprocessor : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(
        string[] importedAssets,
        string[] deletedAssets,
        string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        if (importedAssets == null || FirebaseGradleRepoPathGuard.IsPatchingTemplate)
        {
            return;
        }

        for (int i = 0; i < importedAssets.Length; i++)
        {
            string asset = importedAssets[i];
            if (!string.Equals(
                    asset,
                    FirebaseGradleRepoPathGuard.SettingsTemplateRelativePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            bool previous = FirebaseGradleRepoPathGuard.SuppressSynchronousTemplateImport;
            FirebaseGradleRepoPathGuard.SuppressSynchronousTemplateImport = true;
            try
            {
                FirebaseGradleRepoPathGuard.EnsureCustomGradleSettingsTemplateEnabled();
                var result = FirebaseGradleRepoPathGuard.VerifyAndFixTemplate(applyFix: true);
                if (result.Status == FirebaseGradleRepoPathGuard.VerifyStatus.Fixed)
                {
                    Debug.LogWarning(
                        "[FirebaseGradleRepoPath] EDM overwrite detected — auto-FIXED " +
                        "settingsTemplate.gradle (deferred ImportAsset; no mid-resolve Refresh)."
                    );
                }
            }
            finally
            {
                FirebaseGradleRepoPathGuard.SuppressSynchronousTemplateImport = previous;
            }

            break;
        }
    }
}

/// <summary>
/// Android pre-build: patch template AFTER GMA's ResolveSync (callbackOrder -1).
/// Running before ResolveSync is useless — EDM rewrites the template during resolve.
/// Sync ImportAsset during that resolve (via AssetPostprocessor) races AAR metas.
/// </summary>
public sealed class FirebaseGradleRepoPathPreprocessBuild : IPreprocessBuildWithReport
{
    // After GoogleMobileAds.AndroidBuildPreProcessor (-1) and ManifestProcessor (0).
    public int callbackOrder => 50;

    public void OnPreprocessBuild(BuildReport report)
    {
        if (report == null || report.summary.platform != BuildTarget.Android)
        {
            return;
        }

        FirebaseGradleRepoPathGuard.EnsureCustomGradleSettingsTemplateEnabled();

        // ResolveSync has finished; safe to sync-import the patched template now.
        FirebaseGradleRepoPathGuard.SuppressSynchronousTemplateImport = false;
        var result = FirebaseGradleRepoPathGuard.VerifyAndFixTemplate(applyFix: true);
        if (result.Status == FirebaseGradleRepoPathGuard.VerifyStatus.Fixed ||
            result.Status == FirebaseGradleRepoPathGuard.VerifyStatus.Ok)
        {
            // Bring AssetDatabase in sync with any disk-only postprocessor patch.
            if (result.Status == FirebaseGradleRepoPathGuard.VerifyStatus.Ok)
            {
                AssetDatabase.ImportAsset(
                    FirebaseGradleRepoPathGuard.SettingsTemplateRelativePath);
            }

            Debug.Log(
                "[FirebaseGradleRepoPath][prebuild] " + result.Status + " — " + result.Message
            );
        }
        else
        {
            Debug.LogError(
                "[FirebaseGradleRepoPath][prebuild] " + result.Status + " — " + result.Message
            );
        }
    }
}

/// <summary>
/// Last line of defense: patch the Gradle project Unity just generated.
/// </summary>
public sealed class FirebaseGradleRepoPathPostGenerate : IPostGenerateGradleAndroidProject
{
    public int callbackOrder => 9999;

    public void OnPostGenerateGradleAndroidProject(string path)
    {
        FirebaseGradleRepoPathGuard.FixGeneratedGradleProject(path);

        string settingsPath = Path.GetFullPath(Path.Combine(path, "..", "settings.gradle"));
        if (!File.Exists(settingsPath))
        {
            return;
        }

        string contents = File.ReadAllText(settingsPath);
        if (FirebaseGradleRepoPathGuard.HasUnsafeHandBuiltFileUri(contents))
        {
            throw new BuildFailedException(
                "[FirebaseGradleRepoPath] Generated settings.gradle still contains an unsafe " +
                "hand-built file:/// Firebase Maven URI (space in project path). " +
                "Build aborted. Path: " + settingsPath
            );
        }

        Debug.Log(
            "[FirebaseGradleRepoPath][postgenerate] settings.gradle verified space-safe: " +
            settingsPath
        );
    }
}
#endif
