using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeOutCanvas : MonoBehaviour
{
    [SerializeField] Image _image;
    [SerializeField] float _timeBeforeFadeOutStart = 0.25f;
    [SerializeField] float _fadeOutTimeSeconds = 0.25f;

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
        
        _image.gameObject.SetActive(false);
    }
}
