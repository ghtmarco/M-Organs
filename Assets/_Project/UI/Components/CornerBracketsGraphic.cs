using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws four L-shaped corner brackets inside a RectTransform.
/// Recreates the anatomical-plate bracket aesthetic from the JSX OrganPlate.
/// Place as a full-rect child inside the plate, on top of HatchGraphic.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class CornerBracketsGraphic : MaskableGraphic
{
    [SerializeField] float inset     = 6f;
    [SerializeField] float armLength = 14f;
    [SerializeField] float lineWidth = 1f;

    void Awake()
    {
        raycastTarget = false;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var   r  = rectTransform.rect;
        float l  = r.xMin + inset,  ri = r.xMax - inset;
        float b  = r.yMin + inset,  t  = r.yMax - inset;
        float a  = armLength;

        // Top-left
        AddLine(vh, l,  t,  l + a, t    );
        AddLine(vh, l,  t,  l,     t - a);
        // Top-right
        AddLine(vh, ri, t,  ri - a, t    );
        AddLine(vh, ri, t,  ri,     t - a);
        // Bottom-left
        AddLine(vh, l,  b,  l + a, b    );
        AddLine(vh, l,  b,  l,     b + a);
        // Bottom-right
        AddLine(vh, ri, b,  ri - a, b    );
        AddLine(vh, ri, b,  ri,     b + a);
    }

    void AddLine(VertexHelper vh, float x0, float y0, float x1, float y1)
    {
        Vector2 a    = new Vector2(x0, y0);
        Vector2 b    = new Vector2(x1, y1);
        Vector2 dir  = (b - a).normalized;
        Vector2 perp = new Vector2(-dir.y, dir.x) * (lineWidth * 0.5f);
        int     idx  = vh.currentVertCount;
        vh.AddVert(a + perp, color, Vector2.zero);
        vh.AddVert(a - perp, color, Vector2.zero);
        vh.AddVert(b - perp, color, Vector2.zero);
        vh.AddVert(b + perp, color, Vector2.zero);
        vh.AddTriangle(idx,     idx + 1, idx + 2);
        vh.AddTriangle(idx,     idx + 2, idx + 3);
    }
}
