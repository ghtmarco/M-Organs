using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Binds an OrganDefinition to the OrganPlate prefab children.
/// Expected prefab hierarchy:
///   OrganPlate (root — this script)
///   ├── Background  Image                  paletteSoft color (falls back to paperDeep)
///   ├── Hatch       HatchGraphic           paletteHue @ 30% opacity
///   ├── Brackets    CornerBracketsGraphic  paletteDeep color (falls back to ink)
///   ├── Icon        Image                  organ sprite (centered)
///   └── Label       TMP_Text               mono label bottom-center
/// </summary>
public class OrganPlateUI : MonoBehaviour
{
    [SerializeField] Image                 background;
    [SerializeField] HatchGraphic          hatch;
    [SerializeField] CornerBracketsGraphic brackets;
    [SerializeField] Image                 iconImage;
    [SerializeField] TMP_Text              labelText;

    public void Bind(OrganDefinition organ, MO2ThemeData theme, bool showLabel = true, string labelOverride = null)
    {
        bool hasPalette = organ.paletteSoft.a > 0.01f;

        if (background != null)
            background.color = hasPalette ? organ.paletteSoft : theme.paperDeep;

        if (hatch != null)
        {
            // Use paletteHue alpha as-is — control opacity via asset inspector
            hatch.color = hasPalette ? organ.paletteHue : new Color(theme.ink.r, theme.ink.g, theme.ink.b, 0.30f);
        }

        if (brackets != null)
            brackets.color = theme.ink;

        if (iconImage != null && organ.icon != null)
        {
            Color iconColor = hasPalette ? organ.paletteDeep : theme.ink;
            iconImage.sprite = organ.icon;
            iconImage.color  = iconColor;
        }

        if (labelText != null)
        {
            labelText.gameObject.SetActive(showLabel);
            if (showLabel)
            {
                string lang    = AppState.Instance ? AppState.Instance.Language : "id";
                labelText.text  = labelOverride ?? organ.GetPlateLabel(lang);
                labelText.color = theme.inkMute;
            }
        }
    }
}
