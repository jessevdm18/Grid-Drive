using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// Release WebGL build of PlayableAd.unity only.
/// WebGL player settings changed for the build are restored afterward.
/// Android player settings are not edited.
/// </summary>
public static class PlayableAdWebBuild
{
    private const string ScenePath = "Assets/PlayableAd/Scenes/PlayableAd.unity";
    private const string OutputDir = "Builds/PlayableAd/Web";
    private const string Define = "PLAYABLE_AD";

    [MenuItem("RushOut/Marketing/Build Playable Ad")]
    public static void Build()
    {
        if (!File.Exists(ScenePath))
        {
            PlayableAdSceneBuilder.CreateScene();
        }

        if (!File.Exists(ScenePath))
        {
            Debug.LogError("[PlayableAd] Missing " + ScenePath);
            return;
        }

        Directory.CreateDirectory(OutputDir);

        BuildTarget previousTarget = EditorUserBuildSettings.activeBuildTarget;
        BuildTargetGroup previousGroup = BuildPipeline.GetBuildTargetGroup(previousTarget);
        NamedBuildTarget webTarget = NamedBuildTarget.WebGL;

        string previousDefines = PlayerSettings.GetScriptingDefineSymbols(webTarget);
        WebGLExceptionSupport previousExceptions = PlayerSettings.WebGL.exceptionSupport;
        WebGLCompressionFormat previousCompression = PlayerSettings.WebGL.compressionFormat;
        Il2CppCompilerConfiguration previousIl2Cpp =
            PlayerSettings.GetIl2CppCompilerConfiguration(webTarget);
        ManagedStrippingLevel previousStripping =
            PlayerSettings.GetManagedStrippingLevel(webTarget);
        bool previousStripEngine = PlayerSettings.stripEngineCode;

        object previousCodeOptimization = GetCodeOptimization();

        try
        {
            PlayerSettings.SetScriptingDefineSymbols(
                webTarget,
                AddDefine(previousDefines, Define));
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.None;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.SetIl2CppCompilerConfiguration(
                webTarget,
                Il2CppCompilerConfiguration.Master);
            PlayerSettings.SetManagedStrippingLevel(webTarget, ManagedStrippingLevel.High);
            PlayerSettings.stripEngineCode = true;
            SetCodeOptimizationDiskSize();

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = OutputDir,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            if (report.summary.result != BuildResult.Succeeded)
            {
                Debug.LogError(
                    "[PlayableAd] Web build " + report.summary.result +
                    " errors=" + report.summary.totalErrors);
                return;
            }

            ReportSizes(OutputDir);
            Debug.Log("[PlayableAd] Web build succeeded at " + OutputDir);
        }
        finally
        {
            PlayerSettings.SetScriptingDefineSymbols(webTarget, previousDefines);
            PlayerSettings.WebGL.exceptionSupport = previousExceptions;
            PlayerSettings.WebGL.compressionFormat = previousCompression;
            PlayerSettings.SetIl2CppCompilerConfiguration(webTarget, previousIl2Cpp);
            PlayerSettings.SetManagedStrippingLevel(webTarget, previousStripping);
            PlayerSettings.stripEngineCode = previousStripEngine;
            if (previousCodeOptimization != null)
            {
                SetCodeOptimization(previousCodeOptimization);
            }

            if (previousTarget != BuildTarget.WebGL &&
                previousTarget != BuildTarget.NoTarget)
            {
                EditorUserBuildSettings.SwitchActiveBuildTarget(previousGroup, previousTarget);
            }
        }
    }

    private static string AddDefine(string existing, string define)
    {
        if (string.IsNullOrEmpty(existing))
        {
            return define;
        }

        string[] parts = existing.Split(';');
        if (Array.IndexOf(parts, define) >= 0)
        {
            return existing;
        }

        return existing + ";" + define;
    }

    private static object GetCodeOptimization()
    {
        Type type = FindUserBuildSettingsType();
        if (type == null)
        {
            return null;
        }

        var property = type.GetProperty("codeOptimization");
        return property != null ? property.GetValue(null) : null;
    }

    private static void SetCodeOptimizationDiskSize()
    {
        Type type = FindUserBuildSettingsType();
        if (type == null)
        {
            Debug.LogWarning("[PlayableAd] WebGL codeOptimization API not found. Disk Size LTO was not set.");
            return;
        }

        var property = type.GetProperty("codeOptimization");
        if (property == null)
        {
            return;
        }

        string[] preferred = { "DiskSizeLTO", "DiskSize" };
        foreach (string name in preferred)
        {
            try
            {
                object value = Enum.Parse(property.PropertyType, name);
                property.SetValue(null, value);
                Debug.Log("[PlayableAd] WebGL codeOptimization=" + name);
                return;
            }
            catch (ArgumentException)
            {
            }
        }
    }

    private static void SetCodeOptimization(object value)
    {
        Type type = FindUserBuildSettingsType();
        var property = type != null ? type.GetProperty("codeOptimization") : null;
        if (property != null && value != null)
        {
            property.SetValue(null, value);
        }
    }

    private static Type FindUserBuildSettingsType()
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            Type type = assembly.GetType("UnityEditor.WebGL.UserBuildSettings");
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    private static void ReportSizes(string outputDir)
    {
        string[] files = Directory.GetFiles(outputDir, "*", SearchOption.AllDirectories);
        long total = 0;
        var ranked = files
            .Select(path => new FileInfo(path))
            .OrderByDescending(info => info.Length)
            .ToList();
        foreach (FileInfo info in ranked)
        {
            total += info.Length;
        }

        long wasm = Sum(ranked, ".wasm");
        long data = Sum(ranked, ".data");
        long framework = ranked
            .Where(info => info.Name.IndexOf("framework", StringComparison.OrdinalIgnoreCase) >= 0 &&
                           info.Extension == ".js")
            .Sum(info => info.Length);

        Debug.Log(
            "[PlayableAd] files=" + files.Length +
            " totalBytes=" + total +
            " wasmBytes=" + wasm +
            " dataBytes=" + data +
            " frameworkJsBytes=" + framework);
        int limit = Mathf.Min(20, ranked.Count);
        for (int i = 0; i < limit; i++)
        {
            Debug.Log(
                "[PlayableAd] size " + (i + 1) + " " +
                ranked[i].Length + " " +
                ranked[i].FullName.Substring(outputDir.Length));
        }
    }

    private static long Sum(System.Collections.Generic.List<FileInfo> files, string extension)
    {
        long sum = 0;
        for (int i = 0; i < files.Count; i++)
        {
            if (string.Equals(files[i].Extension, extension, StringComparison.OrdinalIgnoreCase) ||
                files[i].Name.EndsWith(extension + ".br", StringComparison.OrdinalIgnoreCase))
            {
                sum += files[i].Length;
            }
        }

        return sum;
    }
}
