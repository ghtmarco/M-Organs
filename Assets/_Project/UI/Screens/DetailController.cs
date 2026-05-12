using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen controller for 04_Detail.
/// Reads AppState.SelectedOrgan on Start to pick the OrganDefinition to display.
/// Button OnClick: OnARPressed, OnQuizPressed, OnBackPressed.
/// </summary>
public class DetailController : MonoBehaviour
{
    [Header("Chrome")]
    [SerializeField] TMP_Text backButtonText;

    [Header("Text")]
    [SerializeField] TMP_Text chapterText;
    [SerializeField] TMP_Text kickerText;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text taglineText;
    [SerializeField] TMP_Text paragraphText;
    [SerializeField] TMP_Text funfactLabelText;
    [SerializeField] TMP_Text funfactText;
    [SerializeField] TMP_Text arButtonText;
    [SerializeField] TMP_Text quizButtonText;

    [Header("Layout")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Image ruleTop;

    [Header("Plate")]
    [SerializeField] OrganPlateUI heroPlate;

    [Header("Facts table")]
    [SerializeField] FactTableUI factTable;

    [Header("All organ definitions (jantung, otak, lambung)")]
    [SerializeField] OrganDefinition[] organDefinitions;

    [Header("Animation")]
    [SerializeField] CanvasGroup canvasGroup;

    OrganDefinition _organ;

    void Start()
    {
        _organ = AppState.Instance != null
            ? FindOrgan(AppState.Instance.SelectedOrgan)
            : (organDefinitions.Length > 0 ? organDefinitions[0] : null);

        ThemeManager.OnThemeChanged           += Apply;
        LocalizationManager.OnLanguageChanged += RefreshText;
        if (ThemeManager.Instance != null)      Apply(ThemeManager.Instance.Current);
        if (LocalizationManager.Instance != null && AppState.Instance != null) RefreshText();
        if (canvasGroup != null) StartCoroutine(MO2Animator.FadeIn(canvasGroup));
    }

    void OnDestroy()
    {
        ThemeManager.OnThemeChanged           -= Apply;
        LocalizationManager.OnLanguageChanged -= RefreshText;
    }

    OrganDefinition FindOrgan(string key)
    {
        foreach (var o in organDefinitions)
            if (o != null && o.organKey == key) return o;
        return organDefinitions.Length > 0 ? organDefinitions[0] : null;
    }

    void Apply(MO2ThemeData t)
    {
        if (_organ == null) return;
        if (backgroundImage  != null) backgroundImage.color  = t.paper;
        if (backButtonText   != null) backButtonText.color   = t.ink;
        if (ruleTop          != null) ruleTop.color          = t.ink;
        if (kickerText       != null) kickerText.color       = t.inkMute;
        if (nameText         != null) nameText.color         = t.ink;
        if (taglineText      != null) taglineText.color      = t.ink2;
        if (paragraphText    != null) paragraphText.color    = t.ink2;
        if (funfactLabelText != null) funfactLabelText.color = t.inkMute;
        if (funfactText      != null) funfactText.color      = t.ink;
        if (chapterText      != null) chapterText.color      = t.inkMute;
        if (arButtonText     != null) arButtonText.color     = t.paper;
        if (quizButtonText   != null) quizButtonText.color   = t.ink;
        if (heroPlate        != null) heroPlate.Bind(_organ, t);
        if (factTable        != null) factTable.Apply(t);
    }

    void RefreshText()
    {
        if (_organ == null || LocalizationManager.Instance == null || AppState.Instance == null) return;
        string lang = AppState.Instance.Language;
        var    L    = LocalizationManager.Instance;

        if (backButtonText   != null) backButtonText.text   = "← " + L.Get("backLabel").ToUpper();
        if (chapterText      != null) chapterText.text      = $"{L.Get("chapter")} {_organ.number}";
        if (kickerText       != null) kickerText.text       = _organ.GetKicker(lang);
        if (nameText         != null) nameText.text         = _organ.GetName(lang) + ".";
        if (taglineText      != null) taglineText.text      = _organ.GetTagline(lang);
        if (paragraphText    != null) paragraphText.text    = _organ.GetParagraph(lang);
        if (funfactLabelText != null) funfactLabelText.text = L.Get("funfact");
        if (funfactText      != null) funfactText.text      = $"“{_organ.GetFunfact(lang)}”";
        if (arButtonText     != null) arButtonText.text     = L.Get("placeAR");
        if (quizButtonText   != null) quizButtonText.text   = L.Get("quiz");
        if (factTable        != null) factTable.Populate(_organ, lang);
    }

    public void OnARPressed()   => SceneLoader.Instance.Load("05_AR");
    public void OnQuizPressed() => SceneLoader.Instance.Load("06_Quiz");
    public void OnBackPressed() => SceneLoader.Instance.Load("03_Picker");
}
