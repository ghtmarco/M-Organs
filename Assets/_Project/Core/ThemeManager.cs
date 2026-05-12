using System;
using UnityEngine;

[DefaultExecutionOrder(-200)]
public class ThemeManager : MonoBehaviour
{
    public static ThemeManager Instance { get; private set; }
    public static event Action<MO2ThemeData> OnThemeChanged;

    [SerializeField] MO2ThemeData lightTheme;
    [SerializeField] MO2ThemeData darkTheme;
    [SerializeField] MO2ThemeData mintTheme;

    public MO2ThemeData Current { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        // Initialize Current in Awake so it's ready before any Start() calls
        Current = lightTheme;
    }

    void Start()
    {
        // Re-apply with saved theme preference (fires event for all listeners)
        if (AppState.Instance != null)
            ApplyTheme(AppState.Instance.ThemeName);
        else
            OnThemeChanged?.Invoke(Current);
    }

    public void ApplyTheme(string name)
    {
        Current = name == "dark"  ? darkTheme
                : name == "mint"  ? mintTheme ?? lightTheme
                : lightTheme;
        OnThemeChanged?.Invoke(Current);
    }

    public void Refresh()
    {
        if (Current != null) OnThemeChanged?.Invoke(Current);
    }
}
