using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [SerializeField] Image fadePanel;
    [SerializeField] float fadeDuration = 0.18f;

    static readonly string[] AllowedScenes =
    {
        "01_Splash", "02_Onboarding", "03_Picker",
        "04_Detail", "05_AR", "06_Quiz", "07_Settings"
    };

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Load(string sceneName)
    {
        if (System.Array.IndexOf(AllowedScenes, sceneName) < 0)
        {
            Debug.LogError($"[SceneLoader] Scene '{sceneName}' not in allowlist.");
            return;
        }
        StartCoroutine(DoLoad(sceneName));
    }

    IEnumerator DoLoad(string sceneName)
    {
        yield return Fade(0f, 1f);
        SceneManager.LoadScene(sceneName);
        yield return null;
        yield return Fade(1f, 0f);
    }

    IEnumerator Fade(float from, float to)
    {
        if (fadePanel == null) yield break;
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadePanel.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }
        fadePanel.color = new Color(0f, 0f, 0f, to);
    }
}
