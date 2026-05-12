using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// List item in the 03_Picker organ list.
/// Hierarchy:
///   Item (root — this script + Button)
///   ├── Thumbnail  Image   (theme.paper bg)
///   │   └── Icon   Image   (organ.icon sprite, tinted organ.paletteDeep)
///   └── Content    (vertical stack)
///       ├── CategoryText  TMP_Text  ("01 — CIRCULATORY")
///       ├── NameText      TMP_Text  ("Heart")
///       └── DescText      TMP_Text  ("A muscle that never rests.")
/// </summary>
public class OrganCardUI : MonoBehaviour
{
    [SerializeField] Image     thumbnailBg;
    [SerializeField] Image     iconImage;
    [SerializeField] TMP_Text  categoryText;
    [SerializeField] TMP_Text  nameText;
    [SerializeField] TMP_Text  descText;
    [SerializeField] Transform plateFrame;

    static readonly Dictionary<string, (string idCat, string enCat, string idDesc, string enDesc)> Data = new()
    {
        ["jantung"] = ("01 — PEREDARAN DARAH", "01 — CIRCULATORY",    "Otot yang tak pernah berhenti.",     "A muscle that never rests."),
        ["otak"]    = ("02 — SISTEM SARAF",    "02 — NERVOUS SYSTEM", "Dilipat untuk muat lebih banyak.",   "Folded to fit more in."),
        ["lambung"] = ("03 — PENCERNAAN",      "03 — DIGESTIVE",      "Kantong asam yang melindungi diri.", "An acid bag that protects itself."),
    };

    Action<string> _onPick;
    string         _organKey;

    void Awake() => GetComponent<Button>()?.onClick.AddListener(OnClick);

    public void Bind(OrganDefinition organ, MO2ThemeData theme, string lang, Action<string> onPick)
    {
        _organKey = organ.organKey;
        _onPick   = onPick;

        if (thumbnailBg  != null) thumbnailBg.color  = theme.paperDeep;
        if (nameText     != null) nameText.color     = theme.ink;
        if (categoryText != null) categoryText.color = theme.inkMute;
        if (descText     != null) descText.color     = theme.inkMute;

        if (iconImage != null)
        {
            iconImage.sprite         = organ.icon;
            iconImage.color          = organ.paletteDeep;
            iconImage.preserveAspect = true;
        }

        if (plateFrame != null)
            foreach (Transform child in plateFrame)
            {
                var img = child.GetComponent<Image>();
                if (img != null)
                    img.color = child.name.StartsWith("Bracket_") ? theme.ink : theme.ruleSoft;
            }

        RefreshLanguage(organ, lang);
    }

    public void RefreshLanguage(OrganDefinition organ, string lang)
    {
        if (nameText != null) nameText.text = organ.GetName(lang);

        if (Data.TryGetValue(organ.organKey, out var d))
        {
            if (categoryText != null) categoryText.text = lang == "id" ? d.idCat : d.enCat;
            if (descText     != null) descText.text     = lang == "id" ? d.idDesc : d.enDesc;
        }
        else
        {
            if (categoryText != null) categoryText.text = string.Empty;
            if (descText     != null) descText.text     = string.Empty;
            Debug.LogWarning($"[OrganCardUI] No data for organ key '{organ.organKey}'");
        }
    }

    void OnClick() => _onPick?.Invoke(_organKey);
}
