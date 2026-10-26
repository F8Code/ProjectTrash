using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MenuHoverIcon : MonoBehaviour
{
    const float LogoSize = 50f;
    const float LogoX = 28f;
    const float TextPad = 100f;
    const float TextMargin = 12f;

    RectTransform _logo;
    RectTransform _text;
    TMP_Text _label;

    void Awake()
    {
        _logo = FindChildRect("Logo");
        if (_logo == null)
        {
            enabled = false;
            return;
        }

        _text = FindChildRect("Text") ?? FindChildRect("Text (TMP)");
        if (_text == null)
        {
            _label = GetComponentInChildren<TMP_Text>(true);
            if (_label != null)
                _text = _label.rectTransform;
        }
        else
        {
            _label = _text.GetComponent<TMP_Text>();
        }

        Image image = _logo.GetComponent<Image>();
        if (image != null)
            image.raycastTarget = false;

        CanvasGroup group = _logo.GetComponent<CanvasGroup>();
        if (group != null)
        {
            group.blocksRaycasts = false;
            group.interactable = false;
        }
    }

    void OnEnable()
    {
        Canvas.willRenderCanvases += ApplyLayout;
        ApplyLayout();
    }

    void OnDisable()
    {
        Canvas.willRenderCanvases -= ApplyLayout;
    }

    void LateUpdate() => ApplyLayout();

    void ApplyLayout()
    {
        if (_logo == null)
            return;

        _logo.anchorMin = new Vector2(0f, 0.5f);
        _logo.anchorMax = new Vector2(0f, 0.5f);
        _logo.pivot = new Vector2(0.5f, 0.5f);
        _logo.sizeDelta = new Vector2(LogoSize, LogoSize);
        _logo.anchoredPosition = new Vector2(LogoX, 0f);

        if (_text == null)
            return;

        _text.anchorMin = Vector2.zero;
        _text.anchorMax = Vector2.one;
        _text.pivot = new Vector2(0.5f, 0.5f);
        _text.offsetMin = new Vector2(TextPad, 0f);
        _text.offsetMax = Vector2.zero;

        if (_label != null)
        {
            _label.alignment = TextAlignmentOptions.MidlineLeft;
            Vector4 margin = _label.margin;
            _label.margin = new Vector4(TextMargin, margin.y, margin.z, margin.w);
        }
    }

    RectTransform FindChildRect(string childName)
    {
        Transform child = transform.Find(childName);
        return child != null ? child as RectTransform : null;
    }
}
