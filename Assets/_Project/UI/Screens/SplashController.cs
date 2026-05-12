using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen controller for 01_Splash.
/// Wire all [SerializeField] refs in the Inspector.
/// Button OnClick: OnStartPressed, OnLangPressed.
/// </summary>
public class SplashController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] TMP_Text appNameText;
    [SerializeField] TMP_Text taglineText;
    [SerializeField] TMP_Text subtitleText;
    [SerializeField] TMP_Text versionText;
    [SerializeField] TMP_Text startButtonText;
    [SerializeField] TMP_Text langButtonText;

    [Header("Layout")]
    [SerializeField] Image backgroundImage;
    [SerializeField] Image ruleTop;
    [SerializeField] Image ruleBottom;

    [Header("Plate")]
    [SerializeField] OrganPlateUI    organPlate;
    [SerializeField] OrganDefinition heartOrgan;

    [Header("Animation")]
    [SerializeField] CanvasGroup canvasGroup;

    void Start()
    {
        ThemeManager.OnThemeChanged           += Apply;
        LocalizationManager.OnLanguageChanged += RefreshText;
        Apply(ThemeManager.Instance.Current);
        RefreshText();
        if (canvasGroup != null) StartCoroutine(MO2Animator.FadeIn(canvasGroup));
    }

    void OnDestroy()
    {
        ThemeManager.OnThemeChanged           -= Apply;
        LocalizationManager.OnLanguageChanged -= RefreshText;
    }

    void Apply(MO2ThemeData t)
    {
        if (backgroundImage != null) backgroundImage.color = t.paper;
        if (ruleTop         != null) ruleTop.color         = t.ink;
        if (ruleBottom      != null) ruleBottom.color      = t.ink;
        if (taglineText     != null) taglineText.color     = t.ink2;
        if (subtitleText    != null) subtitleText.color    = t.inkMute;
        if (versionText     != null) versionText.color     = t.inkMute;
        if (startButtonText != null) startButtonText.color = t.paper;
        if (langButtonText  != null) langButtonText.color  = t.ink;

        // Apostrophe in accent color via TMP rich text
        if (appNameText != null)
        {
            string hex = ColorUtility.ToHtmlStringRGB(t.accent);
            appNameText.text  = $"M<color=#{hex}>'</color>Organs";
            appNameText.color = t.ink;
        }

        if (organPlate != null && heartOrgan != null)
            organPlate.Bind(heartOrgan, t, showLabel: true, labelOverride: "PL. 01 - C0R");
    }

    void RefreshText()
    {
        var L = LocalizationManager.Instance;
        // appNameText updated in Apply() to preserve accent color
        if (taglineText     != null) taglineText.text     = L.Get("tagline") + ".";
        if (subtitleText    != null) subtitleText.text    = L.Get("sub");
        if (versionText     != null) versionText.text     = L.Get("versionTag");
        if (startButtonText != null) startButtonText.text = L.Get("start") + " →";
        if (langButtonText  != null) langButtonText.text  = AppState.Instance.Language == "id" ? "EN" : "ID";
    }

    public void OnStartPressed()
    {
        string dest = AppState.Instance.OnboardingDone ? "03_Picker" : "02_Onboarding";
        SceneLoader.Instance.Load(dest);
    }

    public void OnLangPressed()
    {
        string next = AppState.Instance.Language == "id" ? "en" : "id";
        LocalizationManager.Instance.SetLanguage(next);
    }
}
