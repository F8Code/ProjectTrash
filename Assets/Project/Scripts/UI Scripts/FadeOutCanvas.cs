using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutCanvas : MonoBehaviour
{
    [SerializeField] Image _image;
    [SerializeField] float _timeBeforeFadeOutStart = 0.25f;
    [SerializeField] float _timeBeforeFadeInStart = 0.25f;
    [SerializeField] float _fadeOutTimeSeconds = 0.25f;
    [SerializeField] float _fadeInTimeSeconds = 0.25f;

    void Awake()
    {
        StartCoroutine(FadeOut());
    }

    IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(_timeBeforeFadeOutStart);

        Color color = _image.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < _fadeOutTimeSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _fadeOutTimeSeconds);
            color.a = Mathf.Lerp(startAlpha, 0f, t);
            _image.color = color;
            yield return null;
        }

        color.a = 0f;
        _image.color = color;

        _image.gameObject.SetActive(false);
    }

    public IEnumerator FadeIn()
    {
        _image.gameObject.SetActive(true);

        yield return new WaitForSeconds(_timeBeforeFadeInStart);

        Color color = _image.color;
        float startAlpha = color.a;
        float elapsed = 0f;

        while (elapsed < _fadeInTimeSeconds)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / _fadeInTimeSeconds);
            color.a = Mathf.Lerp(startAlpha, 1f, t);
            _image.color = color;
            yield return null;
        }

        color.a = 1f;
        _image.color = color;
    }
}
