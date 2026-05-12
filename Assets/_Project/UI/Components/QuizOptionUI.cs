using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// One A/B/C/D option row in the Quiz screen.
/// Expected prefab hierarchy:
///   Option (root — this script + Button)
///   ├── Letter   TMP_Text   "A"/"B"/"C"/"D"  mono inkMute
///   ├── Answer   TMP_Text   answer text       display font ink
///   ├── Result   TMP_Text   "✓" or "×"       hidden until answered
///   └── Divider  Image      1 px rule
/// </summary>
public class QuizOptionUI : MonoBehaviour
{
    [SerializeField] TMP_Text letterText;
    [SerializeField] TMP_Text answerText;
    [SerializeField] TMP_Text resultIcon;
    [SerializeField] Image    divider;
    [SerializeField] Button   button;

    static readonly string[] Letters = { "A", "B", "C", "D" };

    int         _index;
    Action<int> _callback;

    public void Setup(string answer, int index, MO2ThemeData theme, Action<int> callback)
    {
        _index    = index;
        _callback = callback;

        if (letterText != null) { letterText.text  = Letters[index]; letterText.color = theme.inkMute; }
        if (answerText != null) { answerText.text  = answer;          answerText.color = theme.ink; }
        if (resultIcon != null)   resultIcon.gameObject.SetActive(false);
        if (divider    != null)   divider.color = new Color(theme.ink.r, theme.ink.g, theme.ink.b, 0.2f);
        if (button     != null)   button.interactable = true;
    }

    public void ShowResult(bool isCorrect, bool isWrong, MO2ThemeData theme)
    {
        if (button != null) button.interactable = false;

        Color c = isCorrect ? theme.ok : isWrong ? theme.accent : theme.ink;
        if (letterText != null) letterText.color = c;
        if (answerText != null) answerText.color = c;

        if (resultIcon != null)
        {
            bool show = isCorrect || isWrong;
            resultIcon.gameObject.SetActive(show);
            if (show) { resultIcon.text = isCorrect ? "✓" : "×"; resultIcon.color = c; }
        }
    }

    public void OnClick() => _callback?.Invoke(_index);
}
