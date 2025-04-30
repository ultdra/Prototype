using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FadeController : MonoBehaviour
{
    private static FadeController _instance;
    [SerializeField]
    private Image m_fadeImage;

    public static FadeController Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindAnyObjectByType<FadeController>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject("FadeController");
                    _instance = obj.AddComponent<FadeController>();
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void FadeIn(float duration, System.Action onComplete = null)
    {
        FadeTo(1f, duration, onComplete);
    }

    public void FadeOut(float duration, System.Action onComplete = null)
    {
        FadeTo(0f, duration, onComplete);
    }

    public void FadeTo(float targetAlpha, float duration, System.Action onComplete = null)
    {
        if (m_fadeImage == null) return;
        StartCoroutine(DoFade(targetAlpha, duration, onComplete));
    }

    private IEnumerator DoFade(float targetAlpha, float duration, System.Action onComplete)
    {
        float startAlpha = m_fadeImage.color.a;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            m_fadeImage.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }
        onComplete?.Invoke();
    }
}
