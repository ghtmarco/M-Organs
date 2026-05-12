using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class MOrgansFactRowBuilder
{
    const string PrefabPath = "Assets/_Project/Prefabs/FactRowPrefab.prefab";
    const string ScenePath  = "Assets/_Project/Scenes";

    public static void BuildFactRow()
    {
        EnsureFolder("Assets/_Project/Prefabs");

        var prefab = CreateFactRowPrefab();
        AssetDatabase.Refresh();

        string[] scenes = { "04_Detail", "05_AR" };
        foreach (var name in scenes)
            AssignInScene(name, prefab);

        AssetDatabase.SaveAssets();
        Debug.Log("[MOrgans] FactRowPrefab built and assigned.");
    }

    static GameObject CreateFactRowPrefab()
    {
        var labelFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/TMP/MO_Mono_SFMono-Medium SDF.asset");
        var valueFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>("Assets/_Project/Fonts/TMP/MO_Display_FrauncesSoft-Regular SDF.asset");

        var root = new GameObject("FactRowPrefab");
        var rt = root.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 82);

        var labelGO = new GameObject("LabelText");
        labelGO.transform.SetParent(root.transform, false);
        var lRT = labelGO.AddComponent<RectTransform>();
        lRT.anchorMin = new Vector2(0, 0);
        lRT.anchorMax = new Vector2(0.58f, 1);
        lRT.offsetMin = new Vector2(0, 14);
        lRT.offsetMax = new Vector2(0, -10);
        var lTxt = labelGO.AddComponent<TextMeshProUGUI>();
        if (labelFont != null) lTxt.font = labelFont;
        lTxt.text = "LABEL";
        lTxt.fontSize = 14;
        lTxt.characterSpacing = 1.2f;
        lTxt.alignment = TextAlignmentOptions.Left;
        lTxt.color = new Color(0.478f, 0.435f, 0.392f);

        var valueGO = new GameObject("ValueText");
        valueGO.transform.SetParent(root.transform, false);
        var vRT = valueGO.AddComponent<RectTransform>();
        vRT.anchorMin = new Vector2(0.58f, 0);
        vRT.anchorMax = new Vector2(1, 1);
        vRT.offsetMin = new Vector2(0, 4);
        vRT.offsetMax = new Vector2(0, -4);
        var vTxt = valueGO.AddComponent<TextMeshProUGUI>();
        if (valueFont != null) vTxt.font = valueFont;
        vTxt.text = "VALUE";
        vTxt.fontSize = 42;
        vTxt.characterSpacing = -1.5f;
        vTxt.alignment = TextAlignmentOptions.Right;
        vTxt.color = new Color(0.102f, 0.090f, 0.078f);

        var ruleGO = new GameObject("RuleBottom");
        ruleGO.transform.SetParent(root.transform, false);
        var rRT = ruleGO.AddComponent<RectTransform>();
        rRT.anchorMin = new Vector2(0, 0);
        rRT.anchorMax = new Vector2(1, 0);
        rRT.pivot = new Vector2(0.5f, 0);
        rRT.sizeDelta = new Vector2(0, 1);
        var rule = ruleGO.AddComponent<Image>();
        rule.color = new Color(0.102f, 0.090f, 0.078f, 0.2f);

        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        Debug.Log($"[MOrgans] Prefab saved: {PrefabPath}");
        return prefab;
    }

    static void AssignInScene(string sceneName, GameObject prefab)
    {
        string path = $"{ScenePath}/{sceneName}.unity";
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

        foreach (var ft in Object.FindObjectsByType<FactTableUI>(FindObjectsInactive.Include))
        {
            var so = new SerializedObject(ft);
            var prop = so.FindProperty("factRowPrefab");
            if (prop != null)
            {
                prop.objectReferenceValue = prefab;
                so.ApplyModifiedProperties();
                Debug.Log($"[MOrgans] Assigned FactRow → {sceneName}:{ft.gameObject.name}");
            }
        }

        EditorSceneManager.SaveScene(scene);
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        string parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
        string folder  = System.IO.Path.GetFileName(path);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, folder);
    }
}
