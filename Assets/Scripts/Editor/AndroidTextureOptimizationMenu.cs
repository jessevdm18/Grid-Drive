using System;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only: preview/apply Android TextureImporter overrides for audited Art categories.
/// Does not touch scenes, prefabs, ScriptableObjects, audio, or packages.
/// </summary>
public static class AndroidTextureOptimizationMenu
{
    private const string PreviewMenu = "RushOut/Release/Preview Android Texture Optimization";
    private const string ApplyMenu = "RushOut/Release/Apply Android Texture Optimization";

    private const string AndroidPlatform = "Android";

    private enum TextureCategory
    {
        VehicleSkin,
        Background,
        UiIcon,
        LogoMenuArt,
        Excluded
    }

    private sealed class Proposal
    {
        public string assetPath;
        public TextureCategory category;
        public int width;
        public int height;
        public bool hasAlpha;
        public int currentMaxSize;
        public TextureImporterFormat currentFormat;
        public bool currentOverridden;
        public bool currentMipMaps;
        public bool currentReadable;
        public int proposedMaxSize;
        public TextureImporterFormat proposedFormat;
        public bool wouldChange;
        public string skipReason;
    }

    [MenuItem(PreviewMenu, priority = 100)]
    private static void Preview()
    {
        List<Proposal> proposals = BuildProposals();
        LogReport("PREVIEW Android Texture Optimization", proposals, applied: false);
        int changable = CountChangable(proposals);
        EditorUtility.DisplayDialog(
            "Preview Android Texture Optimization",
            "Scanned " + proposals.Count + " textures under audited categories.\n" +
            "Would change: " + changable + "\n" +
            "Excluded / no-op: " + (proposals.Count - changable) + "\n\n" +
            "See Console for the full table.\n" +
            "No import settings were modified.",
            "OK"
        );
    }

    [MenuItem(ApplyMenu, priority = 101)]
    private static void Apply()
    {
        if (EditorApplication.isPlaying)
        {
            EditorUtility.DisplayDialog(
                "Apply Android Texture Optimization",
                "Exit Play Mode before applying texture importer changes.",
                "OK"
            );
            return;
        }

        List<Proposal> proposals = BuildProposals();
        int changable = CountChangable(proposals);
        if (changable == 0)
        {
            EditorUtility.DisplayDialog(
                "Apply Android Texture Optimization",
                "Nothing to change — audited textures already match the proposed Android settings.",
                "OK"
            );
            return;
        }

        bool ok = EditorUtility.DisplayDialog(
            "Apply Android Texture Optimization",
            "Apply Android TextureImporter overrides?\n\n" +
            "Textures that would change: " + changable + "\n" +
            "Categories: Vehicle Skins, Backgrounds, UI Icons, Logo/Menu art.\n\n" +
            "Store icons / fonts / non-audited paths stay excluded.\n" +
            "Scenes, prefabs, ScriptableObjects, audio, packages: NOT modified.\n\n" +
            "Reimport may take a while.",
            "APPLY",
            "CANCEL"
        );
        if (!ok)
        {
            return;
        }

        int changed = ApplyProposals(proposals);
        LogReport("APPLIED Android Texture Optimization", proposals, applied: true);
        EditorUtility.DisplayDialog(
            "Apply Android Texture Optimization",
            "Done.\n\n" +
            "Textures modified: " + changed + "\n" +
            "See Console for details.\n\n" +
            "Measure savings with the next Android AAB build.",
            "OK"
        );
    }

    private static int CountChangable(List<Proposal> proposals)
    {
        int n = 0;
        for (int i = 0; i < proposals.Count; i++)
        {
            if (proposals[i].wouldChange)
            {
                n++;
            }
        }

        return n;
    }

    private static List<Proposal> BuildProposals()
    {
        string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { "Assets/Art" });
        List<Proposal> list = new List<Proposal>(guids.Length);

        for (int i = 0; i < guids.Length; i++)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[i]);
            if (string.IsNullOrEmpty(path) || !IsRasterImagePath(path))
            {
                continue;
            }

            TextureCategory category = Classify(path, out string skipReason);
            if (category == TextureCategory.Excluded)
            {
                // Still list exclusions that live under Art for transparency when path is audited-ish.
                if (IsUnderAuditedRoots(path) || IsStoreOrAdaptiveIcon(path) || IsLogoCandidate(path))
                {
                    Proposal excluded = new Proposal
                    {
                        assetPath = path,
                        category = TextureCategory.Excluded,
                        skipReason = skipReason,
                        wouldChange = false
                    };
                    FillCurrentImporterInfo(excluded);
                    list.Add(excluded);
                }

                continue;
            }

            Proposal proposal = BuildProposal(path, category);
            list.Add(proposal);
        }

        list.Sort((a, b) => string.CompareOrdinal(a.assetPath, b.assetPath));
        return list;
    }

    private static Proposal BuildProposal(string path, TextureCategory category)
    {
        Proposal p = new Proposal
        {
            assetPath = path,
            category = category
        };

        TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
        if (importer == null)
        {
            p.skipReason = "Not a TextureImporter";
            p.wouldChange = false;
            return p;
        }

        importer.GetSourceTextureWidthAndHeight(out p.width, out p.height);
        p.hasAlpha = importer.DoesSourceTextureHaveAlpha();
        p.currentMipMaps = importer.mipmapEnabled;
        p.currentReadable = importer.isReadable;

        TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings(AndroidPlatform);
        p.currentOverridden = android.overridden;
        p.currentMaxSize = android.overridden
            ? android.maxTextureSize
            : importer.maxTextureSize;
        p.currentFormat = android.overridden
            ? android.format
            : TextureImporterFormat.Automatic;

        p.proposedMaxSize = ProposeMaxSize(category, Mathf.Max(p.width, p.height));
        p.proposedFormat = ProposeFormat(category);

        p.wouldChange =
            !android.overridden ||
            android.maxTextureSize != p.proposedMaxSize ||
            android.format != p.proposedFormat ||
            importer.mipmapEnabled ||
            importer.isReadable;

        return p;
    }

    private static void FillCurrentImporterInfo(Proposal p)
    {
        TextureImporter importer = AssetImporter.GetAtPath(p.assetPath) as TextureImporter;
        if (importer == null)
        {
            return;
        }

        importer.GetSourceTextureWidthAndHeight(out p.width, out p.height);
        p.hasAlpha = importer.DoesSourceTextureHaveAlpha();
        p.currentMipMaps = importer.mipmapEnabled;
        p.currentReadable = importer.isReadable;
        TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings(AndroidPlatform);
        p.currentOverridden = android.overridden;
        p.currentMaxSize = android.overridden ? android.maxTextureSize : importer.maxTextureSize;
        p.currentFormat = android.overridden ? android.format : TextureImporterFormat.Automatic;
        p.proposedMaxSize = p.currentMaxSize;
        p.proposedFormat = p.currentFormat;
    }

    private static int ApplyProposals(List<Proposal> proposals)
    {
        int changed = 0;
        List<string> touchedPaths = new List<string>();

        try
        {
            AssetDatabase.StartAssetEditing();
            for (int i = 0; i < proposals.Count; i++)
            {
                Proposal p = proposals[i];
                if (!p.wouldChange || p.category == TextureCategory.Excluded)
                {
                    continue;
                }

                if (ApplyOne(p))
                {
                    changed++;
                    touchedPaths.Add(p.assetPath);
                }
            }
        }
        finally
        {
            AssetDatabase.StopAssetEditing();
        }

        // Ensure dirty importers are processed after the batch edit scope.
        for (int i = 0; i < touchedPaths.Count; i++)
        {
            AssetDatabase.ImportAsset(touchedPaths[i], ImportAssetOptions.ForceUpdate);
        }

        AssetDatabase.Refresh();

        Debug.Log(
            "[AndroidTextureOptimization] textures changed count=" + changed + "\n" +
            "Categories touched: VehicleSkin, Background, UiIcon, LogoMenuArt (as applicable).\n" +
            "No MB savings claimed — measure via next Android AAB."
        );

        return changed;
    }

    private static bool ApplyOne(Proposal p)
    {
        TextureImporter importer = AssetImporter.GetAtPath(p.assetPath) as TextureImporter;
        if (importer == null)
        {
            return false;
        }

        bool dirty = false;

        if (importer.mipmapEnabled)
        {
            importer.mipmapEnabled = false;
            dirty = true;
        }

        if (importer.isReadable)
        {
            importer.isReadable = false;
            dirty = true;
        }

        TextureImporterPlatformSettings android = importer.GetPlatformTextureSettings(AndroidPlatform);
        if (!android.overridden ||
            android.maxTextureSize != p.proposedMaxSize ||
            android.format != p.proposedFormat)
        {
            android.name = AndroidPlatform;
            android.overridden = true;
            android.maxTextureSize = p.proposedMaxSize;
            android.format = p.proposedFormat;
            android.compressionQuality = 50;
            android.allowsAlphaSplitting = false;
            importer.SetPlatformTextureSettings(android);
            dirty = true;
        }

        if (!dirty)
        {
            return false;
        }

        AssetDatabase.WriteImportSettingsIfDirty(p.assetPath);
        return true;
    }

    private static int ProposeMaxSize(TextureCategory category, int maxDim)
    {
        switch (category)
        {
            case TextureCategory.VehicleSkin:
                if (maxDim <= 512)
                {
                    return 512;
                }

                return 1024;

            case TextureCategory.Background:
                return 1024;

            case TextureCategory.UiIcon:
                if (maxDim <= 256)
                {
                    return 256;
                }

                return 512;

            case TextureCategory.LogoMenuArt:
                // Conservative: keep store-facing menu logos sharp.
                if (maxDim > 1024)
                {
                    return 2048;
                }

                return 1024;

            default:
                return 1024;
        }
    }

    private static TextureImporterFormat ProposeFormat(TextureCategory category)
    {
        // ASTC supports alpha — required for sprites with transparency.
        switch (category)
        {
            case TextureCategory.LogoMenuArt:
                return TextureImporterFormat.ASTC_4x4;
            case TextureCategory.Background:
                return TextureImporterFormat.ASTC_6x6;
            case TextureCategory.VehicleSkin:
                return TextureImporterFormat.ASTC_6x6;
            case TextureCategory.UiIcon:
                return TextureImporterFormat.ASTC_6x6;
            default:
                return TextureImporterFormat.ASTC_6x6;
        }
    }

    private static TextureCategory Classify(string path, out string skipReason)
    {
        skipReason = string.Empty;
        string normalized = path.Replace('\\', '/');

        if (IsStoreOrAdaptiveIcon(normalized))
        {
            skipReason = "Store / adaptive icon exclusion";
            return TextureCategory.Excluded;
        }

        if (normalized.IndexOf("/Fonts/", StringComparison.OrdinalIgnoreCase) >= 0 ||
            normalized.EndsWith(".ttf", StringComparison.OrdinalIgnoreCase) ||
            normalized.EndsWith(".otf", StringComparison.OrdinalIgnoreCase))
        {
            skipReason = "Font exclusion";
            return TextureCategory.Excluded;
        }

        if (normalized.StartsWith("Assets/Art/Vehicles/Skins/", StringComparison.OrdinalIgnoreCase))
        {
            return TextureCategory.VehicleSkin;
        }

        if (normalized.StartsWith(
                "Assets/Art/Environment/Backgrounds/",
                StringComparison.OrdinalIgnoreCase))
        {
            return TextureCategory.Background;
        }

        if (normalized.StartsWith("Assets/Art/UI/Icons/", StringComparison.OrdinalIgnoreCase))
        {
            return TextureCategory.UiIcon;
        }

        // Menu / brand artwork (not Play store icons).
        string file = System.IO.Path.GetFileName(normalized);
        if (string.Equals(file, "Logo.png", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(file, "Logo_Background.png", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(file, "GridDriveLogo.png", StringComparison.OrdinalIgnoreCase))
        {
            return TextureCategory.LogoMenuArt;
        }

        skipReason = "Outside audited categories";
        return TextureCategory.Excluded;
    }

    private static bool IsStoreOrAdaptiveIcon(string normalizedPath)
    {
        string file = System.IO.Path.GetFileName(normalizedPath);
        return string.Equals(file, "GridDriveIcon.png", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(file, "GridDriveAdaptiveIcon.png", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(
                   file,
                   "GridDriveAdaptiveBackground.png",
                   StringComparison.OrdinalIgnoreCase) ||
               string.Equals(
                   file,
                   "GridDriveAdaptiveForeground.png",
                   StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsUnderAuditedRoots(string path)
    {
        string n = path.Replace('\\', '/');
        return n.StartsWith("Assets/Art/Vehicles/Skins/", StringComparison.OrdinalIgnoreCase) ||
               n.StartsWith(
                   "Assets/Art/Environment/Backgrounds/",
                   StringComparison.OrdinalIgnoreCase) ||
               n.StartsWith("Assets/Art/UI/Icons/", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsLogoCandidate(string path)
    {
        string file = System.IO.Path.GetFileName(path.Replace('\\', '/'));
        return string.Equals(file, "Logo.png", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(file, "Logo_Background.png", StringComparison.OrdinalIgnoreCase) ||
               string.Equals(file, "GridDriveLogo.png", StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsRasterImagePath(string path)
    {
        string lower = path.ToLowerInvariant();
        return lower.EndsWith(".png") ||
               lower.EndsWith(".jpg") ||
               lower.EndsWith(".jpeg") ||
               lower.EndsWith(".tga");
    }

    private static void LogReport(string title, List<Proposal> proposals, bool applied)
    {
        int vehicle = 0;
        int background = 0;
        int icons = 0;
        int logos = 0;
        int excluded = 0;
        int wouldChange = 0;

        StringBuilder sb = new StringBuilder(64 * 1024);
        sb.AppendLine("=== " + title + " ===");
        sb.AppendLine(
            "Asset | Category | Dims | Alpha | CurMax | PropMax | CurAndroidFmt | PropFmt | Change"
        );

        for (int i = 0; i < proposals.Count; i++)
        {
            Proposal p = proposals[i];
            switch (p.category)
            {
                case TextureCategory.VehicleSkin:
                    vehicle++;
                    break;
                case TextureCategory.Background:
                    background++;
                    break;
                case TextureCategory.UiIcon:
                    icons++;
                    break;
                case TextureCategory.LogoMenuArt:
                    logos++;
                    break;
                default:
                    excluded++;
                    break;
            }

            if (p.wouldChange)
            {
                wouldChange++;
            }

            if (p.category == TextureCategory.Excluded)
            {
                sb.AppendLine(
                    p.assetPath + " | EXCLUDED | " +
                    p.width + "x" + p.height + " | - | " +
                    p.currentMaxSize + " | - | " +
                    FormatName(p.currentFormat) + " | - | no (" + p.skipReason + ")"
                );
                continue;
            }

            sb.AppendLine(
                p.assetPath + " | " +
                p.category + " | " +
                p.width + "x" + p.height + " | " +
                (p.hasAlpha ? "yes" : "no") + " | " +
                p.currentMaxSize +
                (p.currentOverridden ? "" : " (default)") + " | " +
                p.proposedMaxSize + " | " +
                FormatName(p.currentFormat) +
                (p.currentOverridden ? "" : " (no override)") + " | " +
                FormatName(p.proposedFormat) + " | " +
                (p.wouldChange ? "YES" : "no")
            );
        }

        sb.AppendLine();
        sb.AppendLine("COUNTS");
        sb.AppendLine("VehicleSkin=" + vehicle);
        sb.AppendLine("Background=" + background);
        sb.AppendLine("UiIcon=" + icons);
        sb.AppendLine("LogoMenuArt=" + logos);
        sb.AppendLine("ExcludedListed=" + excluded);
        sb.AppendLine("WouldChangeOrChanged=" + wouldChange);
        sb.AppendLine(
            "Rules: Skins max 1024 (512 if source<=512) ASTC_6x6; " +
            "Backgrounds max 1024 ASTC_6x6; " +
            "UI Icons max 256/512 ASTC_6x6; " +
            "Logos max 1024/2048 ASTC_4x4; mipmaps OFF; R/W OFF."
        );
        sb.AppendLine(
            applied
                ? "Apply complete — measure with next AAB (no estimated MB claimed)."
                : "Preview only — no importer writes."
        );

        Debug.Log(sb.ToString());
    }

    private static string FormatName(TextureImporterFormat format)
    {
        return format.ToString();
    }
}
