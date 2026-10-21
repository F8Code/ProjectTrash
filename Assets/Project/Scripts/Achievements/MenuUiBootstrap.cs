using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectTrash.UI;

public class MenuUiBootstrap : MonoBehaviour
{
    static MenuUiBootstrap _instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (_instance != null)
            return;

        GameObject host = new("MenuUiBootstrap");
        DontDestroyOnLoad(host);
        _instance = host.AddComponent<MenuUiBootstrap>();
        _instance.StartCoroutine(_instance.BuildNextFrame());
        SceneManager.sceneLoaded += (_, _) => _instance.StartCoroutine(_instance.BuildNextFrame());
    }

    IEnumerator BuildNextFrame()
    {
        yield return null;
        CreateLanguageButton();
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.RefreshAll();
    }

    void CreateLanguageButton()
    {
        VersionDisplay versionDisplay = FindFirstObjectByType<VersionDisplay>(FindObjectsInactive.Include);
        if (versionDisplay == null || versionDisplay.transform.parent.Find("LanguageToggle") != null)
            return;

        RectTransform versionRect = versionDisplay.GetComponent<RectTransform>();
        TMP_Text versionText = versionDisplay.GetComponent<TMP_Text>();

        GameObject buttonObject = new("LanguageToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(versionRect.parent, false);

        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = new Vector2(220f, 48f);
        rect.anchoredPosition = new Vector2(versionRect.anchoredPosition.x, versionRect.anchoredPosition.y + versionRect.sizeDelta.y + 8f);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(1f, 1f, 1f, 0.12f);

        GameObject labelObject = new("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        RectTransform labelRect = labelObject.GetComponent<RectTransform>();
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = Vector2.zero;
        labelRect.offsetMax = Vector2.zero;

        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        if (versionText != null)
        {
            label.font = versionText.font;
            label.fontSharedMaterial = versionText.fontSharedMaterial;
            label.color = versionText.color;
        }
        label.fontSize = 28f;
        label.alignment = TextAlignmentOptions.MidlineRight;
        label.enableAutoSizing = true;
        label.fontSizeMin = 16f;
        label.fontSizeMax = 32f;
        label.raycastTarget = false;
        UpdateLanguageButtonLabel(label);

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.35f);
        button.colors = colors;
        button.onClick.AddListener(() =>
        {
            LocalizationManager.Instance?.ToggleLanguage();
            UpdateLanguageButtonLabel(label);
        });
    }

    static void UpdateLanguageButtonLabel(TMP_Text label)
    {
        if (label == null)
            return;

        bool chinese = LocalizationManager.Instance != null && LocalizationManager.Instance.IsChinese;
        label.text = chinese ? "English" : "中文";
        LocalizationManager.Instance?.ApplyFont(label);
    }
}
