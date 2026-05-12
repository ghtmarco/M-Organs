using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Dynamically builds a facts table from OrganDefinition.facts[].
/// FactRowPrefab must have named children: LabelText (TMP_Text), ValueText (TMP_Text), RuleBottom (Image).
/// </summary>
public class FactTableUI : MonoBehaviour
{
    [SerializeField] GameObject factRowPrefab;
    [SerializeField] Transform  container;

    [Header("Runtime Typography")]
    [SerializeField] TMP_FontAsset labelFont;
    [SerializeField] TMP_FontAsset valueFont;
    [SerializeField] float labelFontSize = 14f;
    [SerializeField] float valueFontSize = 42f;
    [SerializeField] float labelCharacterSpacing = 1.2f;
    [SerializeField] float valueCharacterSpacing = -1.5f;

    readonly List<(TMP_Text label, TMP_Text value, Image rule)> _rows = new();
    MO2ThemeData _theme;

    public void Populate(OrganDefinition organ, string lang)
    {
        if (container == null)     { Debug.LogWarning("[FactTableUI] container is null");      return; }
        if (factRowPrefab == null) { Debug.LogWarning("[FactTableUI] factRowPrefab is null");  return; }

        EnsureContainerLayout();
        foreach (Transform child in container)
            Destroy(child.gameObject);
        _rows.Clear();

        foreach (var fact in organ.facts)
        {
            var row = Instantiate(factRowPrefab, container);
            ConfigureRow(row);

            var labelText = FindTMP(row, "LabelText");
            var valueText = FindTMP(row, "ValueText");
            var rule      = FindImage(row, "RuleBottom");

            if (labelText == null || valueText == null)
            {
                Debug.LogWarning($"[FactTableUI] Row missing LabelText or ValueText in prefab '{factRowPrefab.name}'");
                continue;
            }

            ConfigureRowTypography(labelText, valueText);
            labelText.text = lang == "id" ? fact.labelID : fact.labelEN;
            valueText.text = fact.value;
            _rows.Add((labelText, valueText, rule));
        }

        if (_theme != null) Apply(_theme);
    }

    void EnsureContainerLayout()
    {
        var layout = container.GetComponent<VerticalLayoutGroup>();
        if (layout == null) layout = container.gameObject.AddComponent<VerticalLayoutGroup>();

        layout.padding = new RectOffset(0, 0, 0, 0);
        layout.spacing = 0f;
        layout.childAlignment = TextAnchor.UpperLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.childScaleWidth = false;
        layout.childScaleHeight = false;
    }

    void ConfigureRow(GameObject row)
    {
        var rect = row.GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(0f, 82f);
        }

        var layoutElement = row.GetComponent<LayoutElement>();
        if (layoutElement == null) layoutElement = row.AddComponent<LayoutElement>();
        layoutElement.minHeight = 82f;
        layoutElement.preferredHeight = 82f;
        layoutElement.flexibleHeight = 0f;
    }

    void ConfigureRowTypography(TMP_Text label, TMP_Text value)
    {
        if (label != null)
        {
            if (labelFont != null) label.font = labelFont;
            label.enableAutoSizing = false;
            label.fontSize = labelFontSize;
            label.characterSpacing = labelCharacterSpacing;
            label.wordSpacing = 0f;
            label.fontStyle = FontStyles.Normal;
            label.alignment = TextAlignmentOptions.Left;
            label.overflowMode = TextOverflowModes.Overflow;
        }

        if (value != null)
        {
            if (valueFont != null) value.font = valueFont;
            value.enableAutoSizing = false;
            value.fontSize = valueFontSize;
            value.characterSpacing = valueCharacterSpacing;
            value.wordSpacing = 0f;
            value.fontStyle = FontStyles.Normal;
            value.alignment = TextAlignmentOptions.Right;
            value.overflowMode = TextOverflowModes.Overflow;
        }
    }

    public void Apply(MO2ThemeData theme)
    {
        _theme = theme;
        var ruleColor = new Color(theme.ink.r, theme.ink.g, theme.ink.b, 0.2f);
        foreach (var (label, value, rule) in _rows)
        {
            if (label != null) label.color = theme.inkMute;
            if (value != null) value.color = theme.ink;
            if (rule  != null) rule.color  = ruleColor;
        }
    }

    static TMP_Text FindTMP(GameObject root, string childName)
    {
        var t = root.transform.Find(childName);
        if (t == null)
        {
            // Deep search fallback
            foreach (var tmp in root.GetComponentsInChildren<TMP_Text>(true))
                if (tmp.name == childName) return tmp;
            return null;
        }
        return t.GetComponent<TMP_Text>();
    }

    static Image FindImage(GameObject root, string childName)
    {
        var t = root.transform.Find(childName);
        if (t == null)
        {
            foreach (var img in root.GetComponentsInChildren<Image>(true))
                if (img.name == childName) return img;
            return null;
        }
        return t.GetComponent<Image>();
    }
}
