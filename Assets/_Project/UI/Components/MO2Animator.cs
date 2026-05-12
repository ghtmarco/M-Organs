using System.Collections;
using UnityEngine;

/// <summary>
/// Provides mo2-fade (screen enter) and mo2-pulse-soft (infinite opacity pulse)
/// animations matching the JSX CSS keyframes.
/// Usage: call FadeIn(group) from a screen controller's Start/OnEnable,
///        call PulseSoft(group) for the AR reticle / status dot.
/// </summary>
public static class MO2Animator
{
    // mo2-fade: opacity 0→1 + Y -6px→0 over 0.4s ease-out
    public static IEnumerator FadeIn(CanvasGroup group, float duration = 0.4f,
                                     RectTransform rect = null, float yOffset = -18f)
    {
        float t = 0f;
        Vector2 startPos = rect != null ? rect.anchoredPosition : Vector2.zero;
        Vector2 endPos   = startPos + new Vector2(0, yOffset);

        group.alpha = 0f;
        if (rect != null) rect.anchoredPosition = endPos;

        while (t < duration)
        {
            t += Time.deltaTime;
            float p = EaseOut(Mathf.Clamp01(t / duration));
            group.alpha = p;
            if (rect != null)
                rect.anchoredPosition = Vector2.Lerp(endPos, startPos, p);
            yield return null;
        }

        group.alpha = 1f;
        if (rect != null) rect.anchoredPosition = startPos;
    }

    // mo2-pulse-soft: opacity 0.5 ↔ 1, 2s period, loops until stopped
    public static IEnumerator PulseSoft(CanvasGroup group, float period = 2f,
                                        float minAlpha = 0.5f, float maxAlpha = 1f)
    {
        while (true)
        {
            float t = 0f;
            while (t < period)
            {
                t += Time.deltaTime;
                float sine = Mathf.Sin(Mathf.PI * t / period); // 0→1→0
                group.alpha = Mathf.Lerp(minAlpha, maxAlpha, sine);
                yield return null;
            }
        }
    }

    static float EaseOut(float t) => 1f - (1f - t) * (1f - t);
}
