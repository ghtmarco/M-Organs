using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen controller for 02_Onboarding (3-step flow).
/// Shown once on first launch; permanently skippable via PlayerPrefs (mo_onb).
/// Button OnClick: OnNextPressed, OnSkipPressed, OnBackPressed.
/// </summary>
public class OnboardingController : MonoBehaviour
{
    [Header("Text")]
    [SerializeField] TMP_Text kickerText;
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text bodyText;
    [SerializeField] TMP_Text nextButtonText;
    [SerializeField] TMP_Text skipText;
    [SerializeField] TMP_Text progressText;

    [Header("Layout")]
    [SerializeField] Image   backgroundImage;
    [SerializeField] Image   ruleTop;
    [SerializeField] Image   ruleBottom;
    [SerializeField] Image[] progressDots;       // 3 elements — wire in Inspector

    [Header("Plate")]
    [SerializeField] OrganPlateUI    organPlate;
    [SerializeField] OrganDefinition[] onboardingOrgans; // [0]=jantung [1]=otak [2]=lambung

    [Header("Animation")]
    [SerializeField] CanvasGroup canvasGroup;

    const int TotalSteps = 3;
    int _step = 0;

    void Start()
    {
        ThemeManager.OnThemeChanged           += Apply;
        LocalizationManager.OnLanguageChanged += RefreshStep;
        Apply(ThemeManager.Instance.Current);
        RefreshStep();
        if (canvasGroup != null) StartCoroutine(MO2Animator.FadeIn(canvasGroup));
    }

    void OnDestroy()
    {
        ThemeManager.OnThemeChanged           -= Apply;
        LocalizationManager.OnLanguageChanged -= RefreshStep;
    }

    void Apply(MO2ThemeData t)
    {
        if (backgroundImage != null) backgroundImage.color = t.paper;
        if (ruleTop         != null) ruleTop.color         = t.ink;
        if (ruleBottom      != null) ruleBottom.color      = t.ink;
        if (kickerText      != null) kickerText.color      = t.inkMute;
        if (titleText       != null) titleText.color       = t.ink;
        if (bodyText        != null) bodyText.color        = t.ink2;
        if (nextButtonText  != null) nextButtonText.color  = t.paper;
        if (skipText        != null) skipText.color        = t.inkMute;
        UpdateDots(t);
        BindPlate(t);
    }

    void UpdateDots(MO2ThemeData t)
    {
        for (int i = 0; i < progressDots.Length; i++)
            if (progressDots[i] != null)
                progressDots[i].color = i == _step ? t.ink : t.ruleSoft;
    }

    void RefreshStep()
    {
        var    L    = LocalizationManager.Instance;
        string pfx  = $"onb{_step}";

        if (kickerText    != null) kickerText.text    = $"{L.Get(pfx + "_kicker")} — {L.Get("appName")}";
        if (titleText     != null) titleText.text     = L.Get(pfx + "_title") + ".";
        if (bodyText      != null) bodyText.text      = L.Get(pfx + "_body");
        if (progressText  != null) progressText.text  = $"{(_step + 1):00} / {TotalSteps:00}";

        bool isLast = _step == TotalSteps - 1;
        if (nextButtonText != null)
            nextButtonText.text = (isLast ? L.Get("start") : L.Get("next")) + " →";
        if (skipText != null) skipText.text = L.Get("skip");

        var t = ThemeManager.Instance?.Current;
        if (t != null) { UpdateDots(t); BindPlate(t); }
    }

    void BindPlate(MO2ThemeData t)
    {
        if (organPlate == null || onboardingOrgans == null || _step >= onboardingOrgans.Length) return;
        var organ = onboardingOrgans[_step];
        if (organ == null) return;
        string plateName = !string.IsNullOrEmpty(organ.nameID) ? organ.nameID : organ.nameEN;
        organPlate.Bind(organ, t, showLabel: true,
            labelOverride: $"PL. {organ.number} — {plateName}");
    }

    public void OnNextPressed()
    {
        if (_step < TotalSteps - 1)
        {
            _step++;
            RefreshStep();
        }
        else
        {
            AppState.Instance.MarkOnboardingDone();
            SceneLoader.Instance.Load("03_Picker");
        }
    }

    public void OnSkipPressed()
    {
        AppState.Instance.MarkOnboardingDone();
        SceneLoader.Instance.Load("03_Picker");
    }

    public void OnBackPressed() => SceneLoader.Instance.Load("01_Splash");
}
