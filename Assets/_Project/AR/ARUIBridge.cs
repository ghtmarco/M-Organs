using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// AR HUD overlay for 05_AR.
/// Manages: reticle visibility, top pills, right rail, collapsible bottom sheet.
/// Button OnClick: OnPlacePressed, OnInfoToggled, OnSharePressed, OnHelpPressed,
///                 OnQuizPressed, OnBackPressed.
/// </summary>
public class ARUIBridge : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] MOrgansVuforiaARBridge vuforiaBridge; // optional — Vuforia

    [Header("Top HUD")]
    [SerializeField] TMP_Text backButtonText;
    [SerializeField] TMP_Text organPillText;   // right pill — "Pl. 01 — Jantung"

    [Header("Status")]
    [SerializeField] TMP_Text    aimingText;
    [SerializeField] CanvasGroup reticlePulseGroup;
    [SerializeField] CanvasGroup statusDotGroup;

    [Header("Reticle")]
    [SerializeField] GameObject reticleRoot;

    [Header("Organ preview")]
    [SerializeField] Image organIconImage;

    [Header("Right rail labels (static symbols)")]
    [SerializeField] TMP_Text infoText;
    [SerializeField] TMP_Text quizText;
    [SerializeField] TMP_Text shareText;
    [SerializeField] TMP_Text helpText;

    [Header("Bottom action")]
    [SerializeField] TMP_Text infoToggleText;
    [SerializeField] TMP_Text placeButtonText;

    [Header("Bottom sheet")]
    [SerializeField] RectTransform bottomSheet;
    [SerializeField] Image         sheetBackground;
    [SerializeField] Image         sheetHandle;
    [SerializeField] TMP_Text      sheetKickerText;
    [SerializeField] TMP_Text      sheetCloseText;
    [SerializeField] TMP_Text      organNameText;
    [SerializeField] TMP_Text      organTaglineText;
    [SerializeField] TMP_Text      organParagraphText;
    [SerializeField] FactTableUI   factTable;

    [Header("Sheet animation")]
    [SerializeField] float sheetAnimSeconds = 0.22f;

    [Header("Organ definitions")]
    [SerializeField] OrganDefinition[] organDefinitions;

    bool      _sheetOpen = false;
    bool      _placed    = false;
    Coroutine _sheetRoutine;

    void Start()
    {
        ThemeManager.OnThemeChanged           += Apply;
        LocalizationManager.OnLanguageChanged += RefreshLabels;

        EnsureSheetHeaderControls();
        ConfigureSheetContentLayout();
        if (ThemeManager.Instance != null) Apply(ThemeManager.Instance.Current);
        RefreshLabels();
        SetSheetOpen(false, instant: true);

        if (reticlePulseGroup != null) StartCoroutine(MO2Animator.PulseSoft(reticlePulseGroup));
        if (statusDotGroup    != null) StartCoroutine(MO2Animator.PulseSoft(statusDotGroup, 1.4f));
    }

    void OnDestroy()
    {
        ThemeManager.OnThemeChanged           -= Apply;
        LocalizationManager.OnLanguageChanged -= RefreshLabels;
    }

    void Update()
    {
        if (reticleRoot != null) reticleRoot.SetActive(false);
        if (aimingText  != null) aimingText.gameObject.SetActive(!_placed);
        if (statusDotGroup != null && statusDotGroup.gameObject.activeSelf != !_placed)
            statusDotGroup.gameObject.SetActive(!_placed);
    }

    void Apply(MO2ThemeData t)
    {
        // Only bottom sheet uses theme colors; AR HUD chrome uses fixed dark overlay (set in scene)
        if (sheetBackground    != null) sheetBackground.color    = t.paper;
        if (sheetHandle        != null) sheetHandle.color        = t.ruleSoft;
        if (organNameText      != null) organNameText.color      = t.ink;
        if (organTaglineText   != null) organTaglineText.color   = t.ink2;
        if (organParagraphText != null) organParagraphText.color = t.ink2;
        if (sheetKickerText    != null) sheetKickerText.color    = t.inkMute;
        if (sheetCloseText     != null) sheetCloseText.color     = t.ink;
        if (factTable          != null) factTable.Apply(t);

        string lang = AppState.Instance?.Language ?? "id";
        BindOrgan(AppState.Instance?.SelectedOrgan, lang);
    }

    void RefreshLabels()
    {
        if (LocalizationManager.Instance == null) return;
        var    L    = LocalizationManager.Instance;
        string lang = AppState.Instance?.Language ?? "id";

        if (backButtonText  != null) backButtonText.text  = "← " + L.Get("backLabel").ToUpper();
        if (placeButtonText != null) placeButtonText.text = L.Get("place").ToUpper();
        if (aimingText      != null) aimingText.text      = L.Get("aiming").ToUpper();
        if (sheetCloseText  != null) sheetCloseText.text  = "× " + L.Get("done").ToUpper();

        // Right rail: static symbols, not localized
        if (infoText  != null) infoText.text  = "i";
        if (quizText  != null) quizText.text  = "q";
        if (shareText != null) shareText.text = "→";
        if (helpText  != null) helpText.text  = "?";

        UpdateInfoToggleLabel();
        BindOrgan(AppState.Instance?.SelectedOrgan, lang);
    }

    void UpdateInfoToggleLabel()
    {
        if (infoToggleText == null || LocalizationManager.Instance == null) return;
        string notes = LocalizationManager.Instance.Get("info").ToUpper();
        infoToggleText.text = (_sheetOpen ? "× " : "+ ") + notes;
    }

    void BindOrgan(string organKey, string lang)
    {
        var organ = FindOrgan(organKey);
        if (organ == null) return;

        if (organPillText != null)
            organPillText.text = $"Pl. {organ.number} — {organ.GetName(lang)}";
        if (sheetKickerText != null)
            sheetKickerText.text = $"Pl. {organ.number} — {organ.GetKicker(lang)}";

        if (organIconImage != null && organ.icon != null)
        {
            organIconImage.sprite = organ.icon;
            organIconImage.color  = new Color(0.945f, 0.929f, 0.863f, 1f); // #F1ECE2
        }

        if (organNameText      != null) organNameText.text      = organ.GetName(lang);
        if (organTaglineText   != null) organTaglineText.text   = organ.GetTagline(lang);
        if (organParagraphText != null) organParagraphText.text = organ.GetParagraph(lang);
        if (factTable          != null) factTable.Populate(organ, lang);
    }

    void EnsureSheetHeaderControls()
    {
        if (bottomSheet == null) return;

        if (sheetKickerText == null)
        {
            sheetKickerText = CreateSheetLabel(
                "SheetKickerText",
                bottomSheet,
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(0f, 1f),
                new Vector2(64f, -52f),
                new Vector2(520f, 56f),
                TextAlignmentOptions.Left);
        }

        if (sheetCloseText != null) return;

        var buttonObject = new GameObject("CloseButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(bottomSheet, false);

        var rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-64f, -52f);
        rect.sizeDelta = new Vector2(260f, 56f);

        var image = buttonObject.GetComponent<Image>();
        image.color = new Color(0f, 0f, 0f, 0f);
        image.raycastTarget = true;

        var button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(OnCloseSheetPressed);

        sheetCloseText = CreateSheetLabel(
            "Label",
            buttonObject.transform,
            Vector2.zero,
            Vector2.one,
            new Vector2(0.5f, 0.5f),
            Vector2.zero,
            Vector2.zero,
            TextAlignmentOptions.Right);
    }

    TMP_Text CreateSheetLabel(
        string name,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 pivot,
        Vector2 anchoredPosition,
        Vector2 sizeDelta,
        TextAlignmentOptions alignment)
    {
        var textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        var rect = textObject.GetComponent<RectTransform>();
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;

        var text = textObject.GetComponent<TextMeshProUGUI>();
        TMP_Text fontSource = infoToggleText != null ? infoToggleText : backButtonText;
        if (fontSource != null) text.font = fontSource.font;
        text.fontSize = 25f;
        text.characterSpacing = 2f;
        text.enableWordWrapping = false;
        text.alignment = alignment;
        text.raycastTarget = false;
        return text;
    }

    void ConfigureSheetContentLayout()
    {
        SetRect(sheetKickerText,  new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f),   new Vector2(64f, -34f),  new Vector2(520f, 42f));
        SetRect(sheetCloseText,   Vector2.zero,        Vector2.one,        new Vector2(0.5f, 0.5f), Vector2.zero,           Vector2.zero);
        SetRect(organNameText,    new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),   new Vector2(0f, -84f),   new Vector2(-176f, 80f));
        SetRect(organTaglineText, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f),   new Vector2(0f, -174f),  new Vector2(-176f, 64f));
        SetRect(organParagraphText, new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -264f),  new Vector2(-176f, 188f));

        if (factTable != null)
        {
            var factRect = factTable.GetComponent<RectTransform>();
            if (factRect != null)
            {
                factRect.anchorMin = new Vector2(0f, 1f);
                factRect.anchorMax = new Vector2(1f, 1f);
                factRect.pivot = new Vector2(0.5f, 1f);
                factRect.anchoredPosition = new Vector2(0f, -500f);
                factRect.sizeDelta = new Vector2(-176f, 340f);
            }
        }

        if (sheetKickerText != null)
        {
            sheetKickerText.fontSize = 20f;
            sheetKickerText.characterSpacing = 2f;
            sheetKickerText.enableWordWrapping = false;
        }
        if (sheetCloseText != null)
        {
            var closeButtonRect = sheetCloseText.transform.parent as RectTransform;
            if (closeButtonRect != null && closeButtonRect != bottomSheet)
            {
                closeButtonRect.anchorMin = new Vector2(1f, 1f);
                closeButtonRect.anchorMax = new Vector2(1f, 1f);
                closeButtonRect.pivot = new Vector2(1f, 1f);
                closeButtonRect.anchoredPosition = new Vector2(-64f, -34f);
                closeButtonRect.sizeDelta = new Vector2(260f, 42f);
            }

            sheetCloseText.fontSize = 20f;
            sheetCloseText.characterSpacing = 2f;
            sheetCloseText.enableWordWrapping = false;
        }
        if (organNameText != null)
        {
            organNameText.fontSize = 72f;
            organNameText.characterSpacing = -1f;
            organNameText.enableWordWrapping = false;
        }
        if (organTaglineText != null)
        {
            organTaglineText.fontSize = 38f;
            organTaglineText.enableWordWrapping = true;
        }
        if (organParagraphText != null)
        {
            organParagraphText.fontSize = 38f;
            organParagraphText.lineSpacing = -6f;
            organParagraphText.enableWordWrapping = true;
        }
    }

    static void SetRect(TMP_Text text, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
    {
        if (text == null) return;
        var rect = text.GetComponent<RectTransform>();
        if (rect == null) return;
        rect.anchorMin = anchorMin;
        rect.anchorMax = anchorMax;
        rect.pivot = pivot;
        rect.anchoredPosition = anchoredPosition;
        rect.sizeDelta = sizeDelta;
    }

    OrganDefinition FindOrgan(string key)
    {
        if (organDefinitions == null || organDefinitions.Length == 0) return null;
        foreach (var o in organDefinitions)
            if (o != null && o.organKey == key) return o;
        return organDefinitions[0];
    }

    float SheetExpandedY => 0f;
    float SheetCollapsedY
    {
        get
        {
            if (bottomSheet == null) return -1344f;
            float height = bottomSheet.rect.height;
            if (height <= 0.1f) height = 1344f;
            return -height;
        }
    }

    void SetSheetImmediate(float y)
    {
        if (bottomSheet == null) return;
        var pos = bottomSheet.anchoredPosition;
        pos.y = y;
        bottomSheet.anchoredPosition = pos;
    }

    void SetSheetOpen(bool open, bool instant = false)
    {
        _sheetOpen = open;
        UpdateInfoToggleLabel();

        float target = open ? SheetExpandedY : SheetCollapsedY;
        if (_sheetRoutine != null)
        {
            StopCoroutine(_sheetRoutine);
            _sheetRoutine = null;
        }

        if (instant || bottomSheet == null || sheetAnimSeconds <= 0f)
        {
            SetSheetImmediate(target);
            return;
        }

        _sheetRoutine = StartCoroutine(AnimateSheet(target));
    }

    IEnumerator AnimateSheet(float target)
    {
        if (bottomSheet == null) yield break;
        float start   = bottomSheet.anchoredPosition.y;
        float elapsed = 0f;
        while (elapsed < sheetAnimSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / sheetAnimSeconds);
            SetSheetImmediate(Mathf.Lerp(start, target, t));
            yield return null;
        }
        SetSheetImmediate(target);
        _sheetRoutine = null;
    }

    public void OnPlacePressed()
    {
        _placed = true;
        if (vuforiaBridge != null) vuforiaBridge.OnPlacePressed();
    }

    public void OnInfoToggled()
    {
        SetSheetOpen(!_sheetOpen);
    }

    public void OnCloseSheetPressed() => SetSheetOpen(false);
    public void OnQuizPressed()  => SceneLoader.Instance?.Load("06_Quiz");
    public void OnSharePressed() { }
    public void OnHelpPressed()  { }
    public void OnBackPressed()  => SceneLoader.Instance?.Load("04_Detail");
}
