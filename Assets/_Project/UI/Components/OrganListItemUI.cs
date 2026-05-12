using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One row in the Organ Picker list.
/// Expected prefab hierarchy:
///   ListItem (root — this script + Button)
///   ├── Thumbnail    OrganPlateUI   small square, no label
///   ├── Info
///   │   ├── NumberKicker  TMP_Text  "01 — Sistem peredaran"
///   │   ├── Name          TMP_Text  large display font
///   │   └── Tagline       TMP_Text  small muted
///   ├── Arrow        TMP_Text       "→"
///   └── Divider      Image          1 px rule
/// </summary>
public class OrganListItemUI : MonoBehaviour
{
    [SerializeField] OrganPlateUI thumbnail;
    [SerializeField] TMP_Text     numberKickerText;
    [SerializeField] TMP_Text     nameText;
    [SerializeField] TMP_Text     taglineText;
    [SerializeField] TMP_Text     arrowText;
    [SerializeField] Image        divider;

    Action<string> _onPick;
    string         _organKey;

    public void Bind(OrganDefinition organ, MO2ThemeData theme, string lang, Action<string> onPick)
    {
        _organKey = organ.organKey;
        _onPick   = onPick;

        if (thumbnail        != null) thumbnail.Bind(organ, theme, showLabel: false);
        if (divider          != null) divider.color          = new Color(theme.ink.r, theme.ink.g, theme.ink.b, 0.2f);
        if (nameText         != null) nameText.color         = theme.ink;
        if (numberKickerText != null) numberKickerText.color = theme.inkMute;
        if (taglineText      != null) taglineText.color      = theme.inkMute;
        if (arrowText        != null) arrowText.color        = theme.ink;

        RefreshLanguage(organ, lang);
    }

    public void RefreshLanguage(OrganDefinition organ, string lang)
    {
        if (numberKickerText != null) numberKickerText.text = $"{organ.number} — {organ.GetKicker(lang)}";
        if (nameText         != null) nameText.text         = organ.GetName(lang);
        if (taglineText      != null) taglineText.text      = organ.GetTagline(lang);
    }

    public void OnClick() => _onPick?.Invoke(_organKey);
}
