using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Draws a diagonal line-hatch pattern inside a RectTransform.
/// Recreates the repeating-linear-gradient(45deg) from the JSX OrganPlate background.
/// Place as a full-rect child inside the plate, behind the icon.
/// </summary>
[RequireComponent(typeof(CanvasRenderer))]
public class HatchGraphic : MaskableGraphic
{
    [SerializeField] float spacing   = 8f;
    [SerializeField] float lineWidth = 0.5f;
    [SerializeField] float angleDeg  = 45f;

    void Awake()
    {
        raycastTarget = false;
    }

    protected override void OnPopulateMesh(VertexHelper vh)
    {
        vh.Clear();
        var     rect = rectTransform.rect;
        float   rad  = angleDeg * Mathf.Deg2Rad;
        float   cos  = Mathf.Cos(rad);
        float   sin  = Mathf.Sin(rad);
        float   diag = Mathf.Sqrt(rect.width * rect.width + rect.height * rect.height);
        Vector2 ctr  = rect.center;
        Vector2 dir  = new Vector2(cos, sin);
        Vector2 perp = new Vector2(-sin, cos);

        for (float d = -diag; d < diag; d += spacing)
        {
            Vector2 p0 = ctr + perp * d + dir * (-diag);
            Vector2 p1 = ctr + perp * d + dir *   diag;
            AddLine(vh, p0, p1);
        }
    }

    void AddLine(VertexHelper vh, Vector2 a, Vector2 b)
    {
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
