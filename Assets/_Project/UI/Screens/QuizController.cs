using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-50)]
public class QuizController : MonoBehaviour
{
    [Header("Common")]
    [SerializeField] Image       backgroundImage;
    [SerializeField] Image[]     strongRules;
    [SerializeField] CanvasGroup canvasGroup;

    [Header("Chrome")]
    [SerializeField] TMP_Text backButtonText;
    [SerializeField] TMP_Text chromeRightText;

    [Header("Quiz panel")]
    [SerializeField] GameObject    quizPanel;
    [SerializeField] TMP_Text      organKickerText;
    [SerializeField] TMP_Text      questionText;
    [SerializeField] QuizOptionUI[] options;
    [SerializeField] RectTransform progressStripsContainer;

    [Header("Score panel")]
    [SerializeField] GameObject   scorePanel;
    [SerializeField] OrganPlateUI scorePlate;
    [SerializeField] TMP_Text     scoreLabelText;
    [SerializeField] TMP_Text     scoreText;
    [SerializeField] TMP_Text     scoreTotalText;
    [SerializeField] TMP_Text     retryButtonText;
    [SerializeField] TMP_Text     doneButtonText;

    [Header("Organ definitions")]
    [SerializeField] OrganDefinition[] organDefinitions;

    OrganDefinition _organ;
    string          _lang;
    int             _questionIndex;
    int             _score;
    bool            _picking;
    bool            _inScore;
    Image[]         _progressStrips;

    void Start()
    {
        _organ = FindOrgan(AppState.Instance?.SelectedOrgan ?? "jantung");
        _lang  = AppState.Instance?.Language ?? "id";
        ThemeManager.OnThemeChanged           += Apply;
        LocalizationManager.OnLanguageChanged += OnLanguageChanged;
        Apply(ThemeManager.Instance?.Current);
        if (canvasGroup != null) StartCoroutine(MO2Animator.FadeIn(canvasGroup));
        StartQuiz();
    }

    void OnDestroy()
    {
        ThemeManager.OnThemeChanged           -= Apply;
        LocalizationManager.OnLanguageChanged -= OnLanguageChanged;
    }

    void OnLanguageChanged()
    {
        _lang = AppState.Instance?.Language ?? "id";
        if (_inScore) RefreshScoreTexts();
        else RefreshQuizTexts();
        Apply(ThemeManager.Instance?.Current);
    }

    OrganDefinition FindOrgan(string key)
    {
        foreach (var o in organDefinitions)
            if (o != null && o.organKey == key) return o;
        return organDefinitions.Length > 0 ? organDefinitions[0] : null;
    }

    void Apply(MO2ThemeData t)
    {
        if (t == null) return;
        if (backgroundImage != null) backgroundImage.color = t.paper;
        foreach (var r in strongRules)
            if (r != null) r.color = new Color(t.ink.r, t.ink.g, t.ink.b, 0.85f);
        if (backButtonText  != null) backButtonText.color  = t.ink;
        if (chromeRightText != null) chromeRightText.color = t.inkMute;
        if (organKickerText != null) organKickerText.color = t.inkMute;
        if (questionText    != null) questionText.color    = t.ink;
        if (scoreLabelText  != null) scoreLabelText.color  = t.inkMute;
        if (scoreText       != null) scoreText.color       = t.ink;
        if (scoreTotalText  != null) scoreTotalText.color  = t.inkMute;
        if (retryButtonText != null) retryButtonText.color = t.ink;
        if (doneButtonText  != null) doneButtonText.color  = t.paper;
        ApplyStrips(t);
        if (scorePlate != null && _organ != null) scorePlate.Bind(_organ, t);
    }

    void StartQuiz()
    {
        _questionIndex = 0;
        _score         = 0;
        _picking       = false;
        _inScore       = false;
        quizPanel.SetActive(true);
        scorePanel.SetActive(false);
        CreateProgressStrips();
        ShowQuestion();
    }

    void CreateProgressStrips()
    {
        if (progressStripsContainer == null || _organ == null) return;
        foreach (Transform child in progressStripsContainer) Destroy(child.gameObject);

        int   n     = _organ.quiz.Length;
        float gap   = 3f;
        float total = progressStripsContainer.rect.width;
        float sw    = Mathf.Max(1f, (total - (n - 1) * gap) / n);
        var   t     = ThemeManager.Instance?.Current;

        _progressStrips = new Image[n];
        for (int i = 0; i < n; i++)
        {
            var go = new GameObject("Strip_" + i, typeof(RectTransform));
            go.transform.SetParent(progressStripsContainer, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot     = new Vector2(0f, 0.5f);
            rt.sizeDelta = new Vector2(sw, 6f);
            rt.anchoredPosition = new Vector2(i * (sw + gap), 0f);
            var img = go.AddComponent<Image>();
            img.color = t != null ? t.ruleSoft : Color.gray;
            _progressStrips[i] = img;
        }
    }

    void ApplyStrips(MO2ThemeData t)
    {
        if (_progressStrips == null || t == null) return;
        for (int i = 0; i < _progressStrips.Length; i++)
            if (_progressStrips[i] != null)
                _progressStrips[i].color = i <= _questionIndex ? t.ink : t.ruleSoft;
    }

    void ShowQuestion()
    {
        if (_organ == null) return;
        _picking = false;
        RefreshQuizTexts();
        ApplyStrips(ThemeManager.Instance?.Current);
    }

    void RefreshQuizTexts()
    {
        if (_organ == null || _questionIndex >= _organ.quiz.Length) return;
        var    L       = LocalizationManager.Instance;
        var    q       = _organ.quiz[_questionIndex];
        string[] ans   = _lang == "id" ? q.answersID : q.answersEN;

        if (backButtonText  != null) backButtonText.text  = "← " + (L?.Get("backLabel")?.ToUpper() ?? "KEMBALI");
        if (chromeRightText != null) chromeRightText.text = $"{(_questionIndex + 1):00} / {_organ.quiz.Length:00}";
        if (organKickerText != null) organKickerText.text = $"{L?.Get("quiz")?.ToUpper() ?? "KUIS"} — {_organ.GetName(_lang).ToUpper()}";
        if (questionText    != null) questionText.text    = _lang == "id" ? q.questionID : q.questionEN;

        var t = ThemeManager.Instance?.Current;
        if (t == null) return;
        for (int i = 0; i < options.Length; i++)
            if (options[i] != null)
                options[i].Setup(ans != null && i < ans.Length ? ans[i] : "", i, t, OnOptionChosen);
    }

    void OnOptionChosen(int idx)
    {
        if (_picking) return;
        _picking = true;

        var  q       = _organ.quiz[_questionIndex];
        bool correct = idx == q.correctIndex;
        if (correct) _score++;

        var t = ThemeManager.Instance?.Current;
        if (t == null) return;
        for (int i = 0; i < options.Length; i++)
            if (options[i] != null)
                options[i].ShowResult(i == q.correctIndex, i == idx && !correct, t);

        StartCoroutine(AdvanceAfterDelay(0.9f));
    }

    IEnumerator AdvanceAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _questionIndex++;
        if (_questionIndex >= _organ.quiz.Length) ShowScore();
        else ShowQuestion();
    }

    void ShowScore()
    {
        _inScore = true;
        quizPanel.SetActive(false);
        scorePanel.SetActive(true);
        RefreshScoreTexts();
        Apply(ThemeManager.Instance?.Current);
    }

    void RefreshScoreTexts()
    {
        var L = LocalizationManager.Instance;
        if (backButtonText  != null) backButtonText.text  = "← " + (L?.Get("backLabel")?.ToUpper() ?? "KEMBALI");
        if (chromeRightText != null) chromeRightText.text = L?.Get("quizDone")?.ToUpper() ?? "SELESAI";
        if (scoreLabelText  != null) scoreLabelText.text  = L?.Get("score")?.ToUpper() ?? "SKOR";
        if (retryButtonText != null) retryButtonText.text = L?.Get("retry")?.ToUpper() ?? "ULANG";
        if (doneButtonText  != null) doneButtonText.text  = L?.Get("done")?.ToUpper() ?? "SELESAI";
        if (scoreText       != null) scoreText.text       = $"{_score}";
        if (scoreTotalText  != null) scoreTotalText.text  = $"/{_organ?.quiz.Length}";
    }

    public void OnRetryPressed() => StartQuiz();
    public void OnDonePressed()  => SceneLoader.Instance?.Load("04_Detail");
    public void OnBackPressed()  => SceneLoader.Instance?.Load("04_Detail");
}
