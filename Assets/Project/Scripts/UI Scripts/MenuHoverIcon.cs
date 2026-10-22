using UnityEngine;
using UnityEngine.UI;

public class MenuHoverIcon : MonoBehaviour
{
    RectTransform _logo;
    RectTransform _text;

    void Awake()
    {
        Transform logo = transform.Find("Logo");
        if (logo == null)
        {
            enabled = false;
            return;
        }

        _logo = logo.GetComponent<RectTransform>();
        _text = transform.Find("Text") as RectTransform;

        Image image = logo.GetComponent<Image>();
        if (image != null)
            image.raycastTarget = false;

        CanvasGroup group = logo.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    void LateUpdate()
    {
        if (_logo == null)
            return;

        _logo.anchorMin = new Vector2(0f, 0.5f);
        _logo.anchorMax = new Vector2(0f, 0.5f);
        _logo.pivot = new Vector2(0.5f, 0.5f);
        _logo.sizeDelta = new Vector2(50f, 50f);
        _logo.anchoredPosition = new Vector2(28f, 0f);

        if (_text == null)
            return;

        Vector2 min = _text.offsetMin;
        if (min.x < 56f)
            _text.offsetMin = new Vector2(56f, min.y);
    }
}
