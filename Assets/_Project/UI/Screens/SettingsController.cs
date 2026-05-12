using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen controller for 07_Settings.
/// Button OnClick: OnLangID, OnLangEN, OnThemeLight, OnThemeDark,
///                 OnSoundToggle, OnHapticsToggle, OnBackPressed.
/// ThemeMint intentionally not wired in Scene 07 UI (design shows Light/Dark only).
/// </summary>
public class SettingsController : MonoBehaviour
{
    [Header("Chrome")]
    [SerializeField] TMP_Text backButtonText;

    [Header("Header")]
    [SerializeField] TMP_Text appKickerText;
    [SerializeField] TMP_Text titleText;

    [Header("Row labels")]
    [SerializeField] TMP_Text langLabelText;
    [SerializeField] TMP_Text themeLabelText;
    [SerializeField] TMP_Text soundLabelText;
    [SerializeField] TMP_Text hapticsLabelText;

    [Header("Lang pills")]
    [SerializeField] Image    langIDBackground;
    [SerializeField] TMP_Text langIDText;
    [SerializeField] Image    langENBackground;
    [SerializeField] TMP_Text langENText;

    [Header("Theme pills")]
    [SerializeField] Image    themeLightBackground;
    [SerializeField] TMP_Text themeLightText;
    [SerializeField] Image    themeDarkBackground;
    [SerializeField] TMP_Text themeDarkText;

    [Header("Sound pill")]
    [SerializeField] Image    soundBackground;
    [SerializeField] TMP_Text soundText;

    [Header("Haptics pill")]
    [SerializeField] Image    hapticsBackground;
    [SerializeField] TMP_Text hapticsText;

    [Header("Pill borders (root Images, always ink)")]
    [SerializeField] Image[] pillBorders;

    [Header("Layout")]
    [SerializeField] Image   backgroundImage;
    [SerializeField] Image[] strongRules;   // ruleTop, header separator — ink @ 0.85
    [SerializeField] Image[] softRules;     // row separators — ink @ 0.2

    void Start()
    {
        ThemeManager.OnThemeChanged           += Apply;
        LocalizationManager.OnLanguageChanged += RefreshText;
        if (ThemeManager.Instance != null)      Apply(ThemeManager.Instance.Current);
        if (LocalizationManager.Instance != null && AppState.Instance != null) RefreshText();
    }

    void OnDestroy()
    {
        ThemeManager.OnThemeChanged           -= Apply;
        LocalizationManager.OnLanguageChanged -= RefreshText;
    }

    void Apply(MO2ThemeData t)
    {
        if (backgroundImage  != null) backgroundImage.color  = t.paper;
        if (appKickerText    != null) appKickerText.color    = t.inkMute;
        if (backButtonText   != null) backButtonText.color   = t.ink;
        if (titleText        != null) titleText.color        = t.ink;
        if (langLabelText    != null) langLabelText.color    = t.ink;
        if (themeLabelText   != null) themeLabelText.color   = t.ink;
        if (soundLabelText   != null) soundLabelText.color   = t.ink;
        if (hapticsLabelText != null) hapticsLabelText.color = t.ink;

        if (strongRules != null)
            foreach (var r in strongRules)
                if (r != null) r.color = new Color(t.ink.r, t.ink.g, t.ink.b, 0.85f);

        if (softRules != null)
            foreach (var r in softRules)
                if (r != null) r.color = new Color(t.ink.r, t.ink.g, t.ink.b, 0.2f);

        if (pillBorders != null)
            foreach (var b in pillBorders) if (b != null) b.color = t.ink;

        if (AppState.Instance == null) return;
        bool   idActive = AppState.Instance.Language  == "id";
        string theme    = AppState.Instance.ThemeName;

        SetPill(langIDBackground,     langIDText,     idActive,          t);
        SetPill(langENBackground,     langENText,     !idActive,         t);
        SetPill(themeLightBackground, themeLightText, theme == "light",  t);
        SetPill(themeDarkBackground,  themeDarkText,  theme == "dark",   t);
        SetPill(soundBackground,      soundText,      AppState.Instance.SoundEnabled,   t);
        SetPill(hapticsBackground,    hapticsText,    AppState.Instance.HapticsEnabled, t);

        UpdateStateTexts();
    }

    void RefreshText()
    {
        if (LocalizationManager.Instance == null || AppState.Instance == null) return;
        var    L            = LocalizationManager.Instance;
        string settingsLabel = L.Get("settings");

        if (appKickerText    != null) appKickerText.text    = L.Get("appName") + " — " + settingsLabel;
        if (backButtonText   != null) backButtonText.text   = "← " + L.Get("backLabel").ToUpper();
        if (titleText        != null) titleText.text        = settingsLabel + ".";
        if (langLabelText    != null) langLabelText.text    = L.Get("language");
        if (themeLabelText   != null) themeLabelText.text   = L.Get("theme");
        if (soundLabelText   != null) soundLabelText.text   = L.Get("sound");
        if (hapticsLabelText != null) hapticsLabelText.text = L.Get("haptics");
        if (langIDText       != null) langIDText.text       = "ID";
        if (langENText       != null) langENText.text       = "EN";
        if (themeLightText   != null) themeLightText.text   = L.Get("light").ToUpper();
        if (themeDarkText    != null) themeDarkText.text    = L.Get("dark").ToUpper();

        UpdateStateTexts();
    }

    void UpdateStateTexts()
    {
        if (AppState.Instance == null || LocalizationManager.Instance == null) return;
        string onText  = LocalizationManager.Instance.Get("on");
        string offText = LocalizationManager.Instance.Get("off");
        if (soundText   != null) soundText.text   = AppState.Instance.SoundEnabled   ? onText : offText;
        if (hapticsText != null) hapticsText.text = AppState.Instance.HapticsEnabled ? onText : offText;
    }

    static void SetPill(Image bg, TMP_Text txt, bool active, MO2ThemeData t)
    {
        if (bg  != null) bg.color  = active ? t.ink : t.paper;
        if (txt != null) txt.color = active ? t.paper : t.ink;
    }

    public void OnLangID()
    {
        if (LocalizationManager.Instance == null) return;
        LocalizationManager.Instance.SetLanguage("id");
        if (ThemeManager.Instance != null) Apply(ThemeManager.Instance.Current);
    }

    public void OnLangEN()
    {
        if (LocalizationManager.Instance == null) return;
        LocalizationManager.Instance.SetLanguage("en");
        if (ThemeManager.Instance != null) Apply(ThemeManager.Instance.Current);
    }

    public void OnThemeLight()
    {
        if (ThemeManager.Instance == null) return;
        ThemeManager.Instance.ApplyTheme("light");
        Apply(ThemeManager.Instance.Current);
    }

    public void OnThemeDark()
    {
        if (ThemeManager.Instance == null) return;
        ThemeManager.Instance.ApplyTheme("dark");
        Apply(ThemeManager.Instance.Current);
    }

    public void OnThemeMint()
    {
        if (ThemeManager.Instance == null) return;
        ThemeManager.Instance.ApplyTheme("mint");
        Apply(ThemeManager.Instance.Current);
    }

    public void OnSoundToggle()
    {
        if (AppState.Instance == null || ThemeManager.Instance == null) return;
        AppState.Instance.SetSound(!AppState.Instance.SoundEnabled);
        Apply(ThemeManager.Instance.Current);
    }

    public void OnHapticsToggle()
    {
        if (AppState.Instance == null || ThemeManager.Instance == null) return;
        AppState.Instance.SetHaptics(!AppState.Instance.HapticsEnabled);
        Apply(ThemeManager.Instance.Current);
    }

    public void OnBackPressed() => SceneLoader.Instance?.Load("01_Splash");
}
