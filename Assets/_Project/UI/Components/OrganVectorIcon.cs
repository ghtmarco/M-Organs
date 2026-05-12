using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Procedural organ icon using polyline approximations of the JSX SVG paths.
/// Add this to a UI GameObject as an alternative to a Sprite icon.
/// Coordinate space: SVG 0-64 viewBox, scaled to fit RectTransform.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class OrganVectorIcon : MaskableGraphic
{
    public enum IconKind { Jantung, Otak, Lambung, Paru, Ginjal, Hati }

    [SerializeField] public IconKind kind      = IconKind.Jantung;
    [SerializeField] public float    lineWidth = 2.5f;

    // Polyline approximations derived from JSX SVG paths (viewBox 0 0 64 64)
    static readonly Dictionary<IconKind, Vector2[][]> Paths = new()
    {
        [IconKind.Jantung] = new[]
        {
            // Heart outline — approximated from M32 54s-20-11-20-26a11 11 0 0 1 20-6 11 11 0 0 1 20 6c0 15-20 26-20 26z
            new Vector2[] {
                new(32,54), new(25,49), new(18,43), new(13,36), new(12,28),
                new(14,22), new(18,18), new(24,18), new(28,21), new(32,26),
                new(36,21), new(40,18), new(46,18), new(50,22), new(52,28),
                new(51,36), new(46,43), new(39,49), new(32,54),
            },
            // ECG wave — M22 26l4 4 4-6 4 8 4-4
            new Vector2[] { new(22,26), new(26,30), new(30,24), new(34,32), new(38,28) },
        },
        [IconKind.Otak] = new[]
        {
            // Brain outline — simplified from JSX path
            new Vector2[] {
                new(32,12), new(24,13), new(18,17), new(14,23), new(13,30),
                new(15,35), new(13,40), new(16,46), new(22,50), new(32,52),
                new(42,50), new(48,46), new(51,40), new(49,35), new(51,30),
                new(50,23), new(46,17), new(40,13), new(32,12),
            },
            // Center vertical line — M32 14v40
            new Vector2[] { new(32,14), new(32,52) },
            // Fold lines
            new Vector2[] { new(22,24), new(30,26) },
            new Vector2[] { new(42,28), new(34,26) },
            new Vector2[] { new(24,38), new(32,36) },
            new Vector2[] { new(42,42), new(34,40) },
        },
        [IconKind.Lambung] = new[]
        {
            // Stomach body — M22 12v8c-7 2-10 9-9 16 1 8 7 14 16 14s14-4 16-12c1-4-2-8-6-8h-3
            new Vector2[] {
                new(22,12), new(22,20), new(17,23), new(14,30), new(15,40),
                new(20,48), new(30,50), new(38,48), new(44,42), new(46,34),
                new(44,28), new(40,26), new(37,26),
            },
            // Top curve — M30 14c2 0 6 0 8 4
            new Vector2[] { new(30,14), new(34,14), new(38,18) },
            // Side detail — M44 30c2-1 4 0 4 3
            new Vector2[] { new(44,30), new(48,30), new(48,33) },
        },
        [IconKind.Paru] = new[]
        {
            // Center bronchus — M32 12v32
            new Vector2[] { new(32,12), new(32,44) },
            // Left lung — M22 18c-8 4-10 14-8 24 1 6 8 8 12 4V18z
            new Vector2[] {
                new(32,18), new(26,20), new(20,24), new(16,32), new(18,42),
                new(26,46), new(32,44),
            },
            // Right lung — M42 18c8 4 10 14 8 24-1 6-8 8-12 4V18z
            new Vector2[] {
                new(32,18), new(38,20), new(44,24), new(48,32), new(46,42),
                new(38,46), new(32,44),
            },
        },
        [IconKind.Ginjal] = new[]
        {
            // Left kidney
            new Vector2[] {
                new(22,14), new(16,16), new(10,22), new(8,30), new(8,38),
                new(14,44), new(22,48), new(28,46), new(30,40),
                new(30,32), new(28,24), new(22,14),
            },
            // Right kidney
            new Vector2[] {
                new(42,14), new(48,16), new(54,22), new(56,30), new(56,38),
                new(50,44), new(42,48), new(36,46), new(34,40),
                new(34,32), new(36,24), new(42,14),
            },
            // Center connector — M30 32h4
            new Vector2[] { new(30,32), new(34,32) },
        },
        [IconKind.Hati] = new[]
        {
            // Liver outline — M8 22c0-4 4-8 8-8h32c4 0 8 4 8 8 0 14-12 28-24 28S8 36 8 22z
            new Vector2[] {
                new(8,22), new(9,18), new(14,14), new(20,14), new(32,14),
                new(44,14), new(50,14), new(55,18), new(56,22),
                new(54,32), new(48,42), new(40,48), new(32,50),
                new(24,48), new(16,42), new(10,32), new(8,22),
            },
            // Vertical line — M28 18v22
            new Vector2[] { new(28,18), new(28,40) },
            // Horizontal line — M16 26h12
            new Vector2[] { new(16,26), new(28,26) },
        },
    };

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        if (!Paths.TryGetValue(kind, out var polylines)) return;

        Rect    r     = rectTransform.rect;
        float   scale = Mathf.Min(r.width, r.height) / 64f;
        float   hw    = lineWidth * scale * 0.5f;

        foreach (var poly in polylines)
        {
            for (int i = 0; i < poly.Length - 1; i++)
            {
                Vector2 a = SvgToLocal(poly[i],     r, scale);
                Vector2 b = SvgToLocal(poly[i + 1], r, scale);
                AddSegment(vh, a, b, hw, color);
            }
        }
    }

    static Vector2 SvgToLocal(Vector2 pt, Rect r, float scale)
    {
        // SVG: (0,0)=top-left. Unity UI: (0,0)=rect center.
        float x = r.xMin + pt.x * scale;
        float y = r.yMax - pt.y * scale;
        return new Vector2(x, y);
    }

    static void AddSegment(VertexHelper vh, Vector2 a, Vector2 b, float hw, Color c)
    {
        if ((b - a).sqrMagnitude < 0.0001f) return;

        Vector2 dir  = (b - a).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * hw;

        int idx = vh.currentVertCount;
        AddVert(vh, a - perp, c);
        AddVert(vh, a + perp, c);
        AddVert(vh, b + perp, c);
        AddVert(vh, b - perp, c);
        vh.AddTriangle(idx,     idx + 1, idx + 2);
        vh.AddTriangle(idx,     idx + 2, idx + 3);
    }

    static void AddVert(VertexHelper vh, Vector2 pos, Color c)
    {
        var v = UIVertex.simpleVert;
        v.position = pos;
        v.color    = c;
        vh.AddVert(v);
    }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        SetVerticesDirty();
    }
#endif
}
