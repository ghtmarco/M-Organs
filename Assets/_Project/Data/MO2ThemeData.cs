using UnityEngine;

[CreateAssetMenu(fileName = "MO2Theme", menuName = "MOrgans/Theme")]
public class MO2ThemeData : ScriptableObject
{
    public Color paper       = new Color(1.000f, 0.965f, 0.925f);  // #FFF6EC
    public Color paperDeep   = new Color(1.000f, 0.937f, 0.851f);  // #FFEFD9
    public Color surface     = new Color(1.000f, 1.000f, 1.000f);  // #FFFFFF
    public Color surfaceMute = new Color(0.984f, 0.914f, 0.824f);  // #FBE9D2
    public Color ink         = new Color(0.165f, 0.118f, 0.078f);  // #2A1E14
    public Color ink2        = new Color(0.361f, 0.290f, 0.227f);  // #5C4A3A
    public Color inkMute     = new Color(0.608f, 0.529f, 0.451f);  // #9B8773
    public Color inkFaint    = new Color(0.784f, 0.725f, 0.659f);  // #C8B9A8
    public Color rule        = new Color(0.165f, 0.118f, 0.078f);  // #2A1E14
    public Color ruleSoft    = new Color(0.165f, 0.118f, 0.078f, 0.08f);
    public Color accent      = new Color(1.000f, 0.416f, 0.302f);  // #FF6A4D
    public Color accentInk   = new Color(0.478f, 0.122f, 0.059f);  // #7A1F0F
    public Color ok          = new Color(0.180f, 0.682f, 0.416f);  // #2EAE6A
    public bool  isDark      = false;
}
