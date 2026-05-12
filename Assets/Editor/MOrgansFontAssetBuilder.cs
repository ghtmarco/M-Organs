using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class MOrgansFontAssetBuilder
{
    const string OutputDir = "Assets/_Project/Fonts/TMP";

    static readonly (string source, string output, float faceDilate, float weightNormal)[] Fonts =
    {
        ("Assets/_Project/Fonts/Fraunces/Fraunces_72pt_Soft-Regular.ttf", "MO_Display_FrauncesSoft-Regular SDF.asset", 0.015f, 0.0f),
        ("Assets/_Project/Fonts/Fraunces/Fraunces_72pt_Soft-Italic.ttf", "MO_Display_FrauncesSoft-Italic SDF.asset", 0.015f, 0.0f),
        ("Assets/_Project/Fonts/Inter/Inter_18pt-Regular.ttf", "MO_Body_Inter18-Regular SDF.asset", 0.025f, 0.02f),
        ("Assets/_Project/Fonts/Inter/Inter_18pt-Medium.ttf", "MO_Body_Inter18-Medium SDF.asset", 0.02f, 0.02f),
        ("Assets/_Project/Fonts/Inter/Inter_18pt-SemiBold.ttf", "MO_Body_Inter18-SemiBold SDF.asset", 0.01f, 0.0f),
        ("Assets/_Project/Fonts/SF Mono/SF-Mono-Medium.otf", "MO_Mono_SFMono-Medium SDF.asset", 0.04f, 0.04f),
        ("Assets/_Project/Fonts/SF Mono/SF-Mono-Semibold.otf", "MO_Mono_SFMono-Semibold SDF.asset", 0.025f, 0.02f),
    };

    [InitializeOnLoadMethod]
    static void RebuildOnceWhenMissing()
    {
        EditorApplication.delayCall += () =>
        {
            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{OutputDir}/{Fonts[0].output}") != null)
                return;

            Rebuild();
        };
    }

    [InitializeOnLoadMethod]
    static void ApplyOnceWhenMissing()
    {
        EditorApplication.delayCall += () =>
        {
            const string markerPath = "Assets/_Project/Fonts/TMP/.typography-applied";
            if (File.Exists(markerPath))
                return;

            if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{OutputDir}/{Fonts[0].output}") == null)
                return;

            ApplyTypographyToScenes();
            File.WriteAllText(markerPath, "Typography applied by MOrgansFontAssetBuilder.\n");
            AssetDatabase.ImportAsset(markerPath);
        };
    }

    [MenuItem("MOrgans/Fonts/Rebuild TMP Font Assets")]
    public static void Rebuild()
    {
        if (!AssetDatabase.IsValidFolder(OutputDir))
            AssetDatabase.CreateFolder("Assets/_Project/Fonts", "TMP");

        foreach (var spec in Fonts)
            BuildFontAsset(spec.source, $"{OutputDir}/{spec.output}", spec.faceDilate, spec.weightNormal);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("MOrgans TMP font assets rebuilt.");
    }

    [MenuItem("MOrgans/Fonts/Apply Typography To MOrgans Scenes")]
    public static void ApplyTypographyToScenes()
    {
        Rebuild();

        var display = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{OutputDir}/MO_Display_FrauncesSoft-Regular SDF.asset");
        var displayItalic = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{OutputDir}/MO_Display_FrauncesSoft-Italic SDF.asset");
        var body = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{OutputDir}/MO_Body_Inter18-Regular SDF.asset");
        var bodyMedium = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{OutputDir}/MO_Body_Inter18-Medium SDF.asset");
        var mono = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{OutputDir}/MO_Mono_SFMono-Medium SDF.asset");
        var monoStrong = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>($"{OutputDir}/MO_Mono_SFMono-Semibold SDF.asset");

        foreach (var scenePath in Directory.GetFiles("Assets/_Project/Scenes", "*.unity"))
        {
            var scene = EditorSceneManager.OpenScene(scenePath);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
            {
                var fontName = text.font != null ? text.font.name : "";
                var objectName = text.gameObject.name;
                var lower = (objectName + " " + fontName + " " + text.text).ToLowerInvariant();

                if (IsMonoText(lower))
                    text.font = IsStrongMono(lower) ? monoStrong : mono;
                else if (text.fontStyle.HasFlag(FontStyles.Italic) || lower.Contains("italic") || objectName.Contains("Tagline"))
                    text.font = displayItalic;
                else if (IsDisplayText(lower, text.fontSize))
                    text.font = display;
                else
                    text.font = body;

                if (text.font == null)
                    text.font = bodyMedium != null ? bodyMedium : body;
                text.fontMaterial = null;
                text.SetAllDirty();
                EditorUtility.SetDirty(text);
            }

            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }

        AssetDatabase.SaveAssets();
        Debug.Log("MOrgans typography applied to all MOrgans scenes.");
    }

    static bool IsMonoText(string lower)
    {
        return lower.Contains("mono") ||
               lower.Contains("spacemono") ||
               lower.Contains("sf-mono") ||
               lower.Contains("kicker") ||
               lower.Contains("back") ||
               lower.Contains("button") ||
               lower.Contains("btn") ||
               lower.Contains("label") ||
               lower.Contains("pill") ||
               lower.Contains("progress") ||
               lower.Contains("version") ||
               lower.Contains("rail") ||
               lower.Contains("toggle") ||
               lower.Contains("score") ||
               lower.Contains("01") ||
               lower.Contains("pl.") ||
               lower.Contains("begin") ||
               lower.Contains("kembali") ||
               lower.Contains("tempatkan") ||
               lower.Contains("kuis");
    }

    static bool IsStrongMono(string lower)
    {
        return lower.Contains("button") ||
               lower.Contains("btn") ||
               lower.Contains("begin") ||
               lower.Contains("tempatkan") ||
               lower.Contains("kuis");
    }

    static bool IsDisplayText(string lower, float fontSize)
    {
        return fontSize >= 52f ||
               lower.Contains("title") ||
               lower.Contains("name") ||
               lower.Contains("question") ||
               lower.Contains("m'organs") ||
               lower.Contains("jantung") ||
               lower.Contains("otak") ||
               lower.Contains("lambung") ||
               lower.Contains("pengaturan");
    }

    static void BuildFontAsset(string sourcePath, string outputPath, float faceDilate, float weightNormal)
    {
        var font = AssetDatabase.LoadAssetAtPath<Font>(sourcePath);
        if (font == null)
        {
            Debug.LogWarning($"MOrgans font source missing: {sourcePath}");
            return;
        }

        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(outputPath) != null)
            AssetDatabase.DeleteAsset(outputPath);

        var fontAsset = TMP_FontAsset.CreateFontAsset(
            font,
            90,
            9,
            GlyphRenderMode.SDFAA,
            2048,
            2048,
            AtlasPopulationMode.Dynamic,
            true);
        fontAsset.name = Path.GetFileNameWithoutExtension(outputPath);
        AssetDatabase.CreateAsset(fontAsset, outputPath);
        fontAsset.fallbackFontAssetTable = new List<TMP_FontAsset>();

        var atlas = fontAsset.atlasTexture;
        if (atlas != null)
        {
            atlas.name = fontAsset.name + " Atlas";
            AssetDatabase.AddObjectToAsset(atlas, fontAsset);
        }

        var material = fontAsset.material;
        material.name = fontAsset.name + " Material";
        if (atlas != null) material.SetTexture(ShaderUtilities.ID_MainTex, atlas);
        material.SetFloat(ShaderUtilities.ID_TextureWidth, 2048);
        material.SetFloat(ShaderUtilities.ID_TextureHeight, 2048);
        material.SetFloat(ShaderUtilities.ID_GradientScale, 10f);
        material.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);
        material.SetFloat(ShaderUtilities.ID_WeightNormal, weightNormal);
        material.SetFloat(ShaderUtilities.ID_WeightBold, fontAsset.boldStyle);
        material.SetFloat(ShaderUtilities.ID_OutlineWidth, 0f);
        material.SetFloat(ShaderUtilities.ID_OutlineSoftness, 0f);
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, 0f);
        material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, 0f);
        material.SetFloat(ShaderUtilities.ID_UnderlayDilate, 0f);
        material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, 0f);
        fontAsset.material = material;
        AssetDatabase.AddObjectToAsset(material, fontAsset);

        EditorUtility.SetDirty(fontAsset);
    }
}
