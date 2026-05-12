using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Screen controller for 03_Picker.
/// Wire organs[] and cards[] in the same order: [0]=jantung [1]=otak [2]=lambung.
/// Wire soonHeaderText, soonIcons[] (3 Image), soonLabels[] (3 TMP_Text).
/// Button OnClick: OnBackPressed.
/// </summary>
public class PickerController : MonoBehaviour
{
    [Header("Chrome")]
    [SerializeField] TMP_Text backButtonText;

    [Header("Text")]
    [SerializeField] TMP_Text titleText;
    [SerializeField] TMP_Text subtitleText;

    [Header("Coming Soon")]
    [SerializeField] TMP_Text   soonHeaderText;
    [SerializeField] Image[]    soonIcons;
    [SerializeField] TMP_Text[] soonLabels;
    [SerializeField] TMP_Text   soonNoteText;
    [SerializeField] Image[]    soonBoxImages;

    [Header("Layout")]
    [SerializeField] Image backgroundImage;

    [Header("Data (3 items, same order as cards)")]
    [SerializeField] OrganDefinition[] organs;

    [Header("Cards (3 items, same order as organs)")]
    [SerializeField] OrganCardUI[] cards;

    [Header("Animation")]
    [SerializeField] CanvasGroup canvasGroup;

    void Start()
    {
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

    static readonly string[] SoonLocKeys = { "soonParu", "soonGinjal", "soonHati" };

    void Apply(MO2ThemeData t)
    {
        if (backgroundImage != null) backgroundImage.color = t.paper;
        if (backButtonText  != null) backButtonText.color  = t.ink;
        if (titleText       != null) titleText.color       = t.ink;
        if (subtitleText    != null) subtitleText.color    = t.inkMute;
        if (soonHeaderText  != null) soonHeaderText.color  = t.inkMute;
        if (soonNoteText    != null) soonNoteText.color    = t.inkMute;

        if (soonIcons != null)
            foreach (var ico in soonIcons)
                if (ico != null) ico.color = new Color(t.ink.r, t.ink.g, t.ink.b, 0.35f);
        if (soonLabels != null)
            foreach (var lbl in soonLabels)
                if (lbl != null) lbl.color = t.inkMute;
        if (soonBoxImages != null)
            foreach (var box in soonBoxImages)
                if (box != null) box.color = t.ruleSoft;

        if (AppState.Instance == null) return;
        string lang = AppState.Instance.Language;
        for (int i = 0; i < cards.Length; i++)
            if (i < organs.Length && cards[i] != null && organs[i] != null)
                cards[i].Bind(organs[i], t, lang, OnOrganPicked);
    }

    void RefreshText()
    {
        if (LocalizationManager.Instance == null || AppState.Instance == null) return;
        var    L    = LocalizationManager.Instance;
        string lang = AppState.Instance.Language;

        if (backButtonText != null) backButtonText.text = "← " + L.Get("backLabel").ToUpper();
        if (titleText      != null) titleText.text      = L.Get("pickerTitle");
        if (subtitleText   != null) subtitleText.text   = L.Get("pickerSub");
        if (soonHeaderText != null) soonHeaderText.text = L.Get("soonHead");
        if (soonNoteText   != null) soonNoteText.text   = L.Get("soonNote") + ".";

        if (soonLabels != null)
            for (int i = 0; i < soonLabels.Length && i < SoonLocKeys.Length; i++)
                if (soonLabels[i] != null) soonLabels[i].text = L.Get(SoonLocKeys[i]).ToUpper();

        for (int i = 0; i < cards.Length; i++)
            if (i < organs.Length && cards[i] != null && organs[i] != null)
                cards[i].RefreshLanguage(organs[i], lang);
    }

    public void OnOrganPicked(string organKey)
    {
        AppState.Instance.SelectedOrgan = organKey;
        SceneLoader.Instance.Load("04_Detail");
    }

    public void OnBackPressed() => SceneLoader.Instance.Load("01_Splash");
}
