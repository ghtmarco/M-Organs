using UnityEngine;

[DefaultExecutionOrder(-300)]
public class AppState : MonoBehaviour
{
    public static AppState Instance { get; private set; }

    public string SelectedOrgan  { get; set; }         = "jantung";
    public string Language       { get; private set; } = "id";
    public string ThemeName      { get; private set; } = "light";
    public bool   SoundEnabled   { get; private set; } = true;
    public bool   HapticsEnabled { get; private set; } = false;
    public bool   OnboardingDone { get; private set; } = false;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Load()
    {
        var lang  = PlayerPrefs.GetString("mo_lang",  "id");
        var theme = PlayerPrefs.GetString("mo_theme", "light");
        Language       = (lang  == "id"    || lang  == "en")                       ? lang  : "id";
        ThemeName      = (theme == "light" || theme == "dark" || theme == "mint")  ? theme : "light";
        SoundEnabled   = PlayerPrefs.GetInt("mo_sound",  1) == 1;
        HapticsEnabled = PlayerPrefs.GetInt("mo_haptic", 0) == 1;
        OnboardingDone = PlayerPrefs.GetInt("mo_onb",    0) == 1;
    }

    public void SetLanguage(string lang)
    {
        if (lang != "id" && lang != "en")
        {
            Debug.LogWarning($"[AppState] Invalid language '{lang}', defaulting to 'id'.");
            lang = "id";
        }
        Language = lang;
        PlayerPrefs.SetString("mo_lang", lang);
        PlayerPrefs.Save();
        ThemeManager.Instance?.Refresh();
    }

    public void SetTheme(string theme)
    {
        if (theme != "light" && theme != "dark" && theme != "mint")
        {
            Debug.LogWarning($"[AppState] Invalid theme '{theme}', defaulting to 'light'.");
            theme = "light";
        }
        ThemeName = theme;
        PlayerPrefs.SetString("mo_theme", theme);
        PlayerPrefs.Save();
        ThemeManager.Instance?.ApplyTheme(theme);
    }

    public void SetSound(bool on)
    {
        SoundEnabled = on;
        PlayerPrefs.SetInt("mo_sound", on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void SetHaptics(bool on)
    {
        HapticsEnabled = on;
        PlayerPrefs.SetInt("mo_haptic", on ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void MarkOnboardingDone()
    {
        OnboardingDone = true;
        PlayerPrefs.SetInt("mo_onb", 1);
        PlayerPrefs.Save();
    }
}
