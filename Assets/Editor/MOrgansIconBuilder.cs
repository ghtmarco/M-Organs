using UnityEditor;
using UnityEngine;
using System.IO;

public static class MOrgansIconBuilder
{
    const string IconPath = "Assets/_Project/Icons";
    const string DataPath = "Assets/_Project/Data";

    public static void BuildIcons()
    {
        EnsureFolder(IconPath);

        CreateIcon("icon_jantung", new Color(0.545f, 0.118f, 0.176f)); // #8B1E2D
        CreateIcon("icon_otak",    new Color(0.247f, 0.165f, 0.431f)); // #3F2A6E
        CreateIcon("icon_lambung", new Color(0.478f, 0.255f, 0.031f)); // #7A4108

        AssetDatabase.Refresh();

        AssignIcons();

        AssetDatabase.SaveAssets();
        Debug.Log("[MOrgans] Icons built and assigned.");
    }

    static void CreateIcon(string name, Color toneColor)
    {
        int sz = 128;
        var tex = new Texture2D(sz, sz, TextureFormat.RGBA32, false);

        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < sz; y++)
            for (int x = 0; x < sz; x++)
                tex.SetPixel(x, y, clear);

        if (name == "icon_jantung")   DrawHeart(tex, sz, toneColor);
        else if (name == "icon_otak") DrawBrain(tex, sz, toneColor);
        else                          DrawStomach(tex, sz, toneColor);

        tex.Apply();

        string assetPath = $"{IconPath}/{name}.asset";

        // Remove existing asset so we can recreate cleanly
        AssetDatabase.DeleteAsset(assetPath);

        AssetDatabase.CreateAsset(tex, assetPath);

        var sprite = Sprite.Create(tex,
            new Rect(0, 0, sz, sz),
            new Vector2(0.5f, 0.5f), 100f);
        sprite.name = name;
        AssetDatabase.AddObjectToAsset(sprite, assetPath);
        AssetDatabase.SaveAssets();

        Debug.Log($"[MOrgans] Created: {assetPath}");
    }

    // ── HEART: two circles + inverted triangle ────────────────────────────
    static void DrawHeart(Texture2D tex, int sz, Color c)
    {
        FillCircle(tex, sz * 0.35f, sz * 0.58f, sz * 0.24f, c);
        FillCircle(tex, sz * 0.65f, sz * 0.58f, sz * 0.24f, c);
        FillTriangle(tex,
            new Vector2(sz * 0.18f, sz * 0.52f),
            new Vector2(sz * 0.82f, sz * 0.52f),
            new Vector2(sz * 0.5f,  sz * 0.22f), c);
    }

    // ── BRAIN: two lobes + bridge + stem ─────────────────────────────────
    static void DrawBrain(Texture2D tex, int sz, Color c)
    {
        FillCircle(tex, sz * 0.34f, sz * 0.52f, sz * 0.22f, c);
        FillCircle(tex, sz * 0.66f, sz * 0.52f, sz * 0.22f, c);
        FillRect(tex, sz * 0.38f, sz * 0.42f, sz * 0.24f, sz * 0.2f,  c);
        FillRect(tex, sz * 0.44f, sz * 0.22f, sz * 0.12f, sz * 0.14f, c);
    }

    // ── STOMACH: ellipse body + cardia neck ───────────────────────────────
    static void DrawStomach(Texture2D tex, int sz, Color c)
    {
        FillEllipse(tex, sz * 0.5f,  sz * 0.48f, sz * 0.28f, sz * 0.34f, c);
        FillRect(tex,   sz * 0.24f, sz * 0.56f, sz * 0.14f, sz * 0.22f,  c);
    }

    // ── PRIMITIVES ────────────────────────────────────────────────────────

    static void FillCircle(Texture2D tex, float cx, float cy, float r, Color c)
    {
        int x0 = Mathf.Max(0, (int)(cx - r)), x1 = Mathf.Min(tex.width  - 1, (int)(cx + r));
        int y0 = Mathf.Max(0, (int)(cy - r)), y1 = Mathf.Min(tex.height - 1, (int)(cy + r));
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                float dx = x - cx, dy = y - cy;
                if (dx * dx + dy * dy <= r * r) tex.SetPixel(x, y, c);
            }
    }

    static void FillEllipse(Texture2D tex, float cx, float cy, float rx, float ry, Color c)
    {
        int x0 = Mathf.Max(0, (int)(cx - rx)), x1 = Mathf.Min(tex.width  - 1, (int)(cx + rx));
        int y0 = Mathf.Max(0, (int)(cy - ry)), y1 = Mathf.Min(tex.height - 1, (int)(cy + ry));
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
            {
                float dx = (x - cx) / rx, dy = (y - cy) / ry;
                if (dx * dx + dy * dy <= 1f) tex.SetPixel(x, y, c);
            }
    }

    static void FillRect(Texture2D tex, float x0, float y0, float w, float h, Color c)
    {
        int minX = Mathf.Max(0, (int)x0), maxX = Mathf.Min(tex.width  - 1, (int)(x0 + w));
        int minY = Mathf.Max(0, (int)y0), maxY = Mathf.Min(tex.height - 1, (int)(y0 + h));
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                tex.SetPixel(x, y, c);
    }

    static void FillTriangle(Texture2D tex, Vector2 a, Vector2 b, Vector2 c, Color col)
    {
        int x0 = Mathf.Max(0, (int)Mathf.Min(a.x, Mathf.Min(b.x, c.x)));
        int x1 = Mathf.Min(tex.width  - 1, (int)Mathf.Max(a.x, Mathf.Max(b.x, c.x)));
        int y0 = Mathf.Max(0, (int)Mathf.Min(a.y, Mathf.Min(b.y, c.y)));
        int y1 = Mathf.Min(tex.height - 1, (int)Mathf.Max(a.y, Mathf.Max(b.y, c.y)));
        for (int y = y0; y <= y1; y++)
            for (int x = x0; x <= x1; x++)
                if (PointInTriangle(new Vector2(x, y), a, b, c))
                    tex.SetPixel(x, y, col);
    }

    static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p,a,b), d2 = Sign(p,b,c), d3 = Sign(p,c,a);
        return !((d1 < 0 || d2 < 0 || d3 < 0) && (d1 > 0 || d2 > 0 || d3 > 0));
    }

    static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        => (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);

    // ── ASSIGN TO ORGAN DEFINITIONS ───────────────────────────────────────

    static void AssignIcons()
    {
        Assign("Organ_Jantung", "icon_jantung");
        Assign("Organ_Otak",    "icon_otak");
        Assign("Organ_Lambung", "icon_lambung");
    }

    static void Assign(string assetName, string iconName)
    {
        var organ = AssetDatabase.LoadAssetAtPath<OrganDefinition>($"{DataPath}/{assetName}.asset");
        if (organ == null) { Debug.LogWarning($"[MOrgans] Not found: {assetName}"); return; }

        string iconAssetPath = $"{IconPath}/{iconName}.asset";
        Sprite sprite = null;
        foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(iconAssetPath))
            if (obj is Sprite s) { sprite = s; break; }

        if (sprite == null) { Debug.LogWarning($"[MOrgans] Sprite not found: {iconName}"); return; }

        var so   = new SerializedObject(organ);
        var prop = so.FindProperty("icon");
        if (prop != null) { prop.objectReferenceValue = sprite; so.ApplyModifiedProperties(); }
        Debug.Log($"[MOrgans] Assigned {iconName} → {assetName}");
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
