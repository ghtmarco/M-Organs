using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Add to any Image or TMP_Text to auto-apply a theme color role when the theme changes.
/// </summary>
[RequireComponent(typeof(Graphic))]
public class ThemedGraphic : MonoBehaviour
{
    public enum ColorRole
    {
        Paper, PaperDeep, Surface,
        Ink, Ink2, InkMute, InkFaint,
        Rule, RuleSoft, Accent, Ok
    }

    [SerializeField] ColorRole role = ColorRole.Ink;

    Graphic _graphic;

    void Awake()
    {
        _graphic = GetComponent<Graphic>();
        ThemeManager.OnThemeChanged += Apply;
    }

    void Start()
    {
        if (ThemeManager.Instance?.Current != null)
            Apply(ThemeManager.Instance.Current);
    }

    void OnDestroy() => ThemeManager.OnThemeChanged -= Apply;

    void Apply(MO2ThemeData t)
    {
        _graphic.color = role switch
        {
            ColorRole.Paper     => t.paper,
            ColorRole.PaperDeep => t.paperDeep,
            ColorRole.Surface   => t.surface,
            ColorRole.Ink       => t.ink,
            ColorRole.Ink2      => t.ink2,
            ColorRole.InkMute   => t.inkMute,
            ColorRole.InkFaint  => t.inkFaint,
            ColorRole.Rule      => t.rule,
            ColorRole.RuleSoft  => t.ruleSoft,
            ColorRole.Accent    => t.accent,
            ColorRole.Ok        => t.ok,
            _                   => _graphic.color,
        };
    }
}
