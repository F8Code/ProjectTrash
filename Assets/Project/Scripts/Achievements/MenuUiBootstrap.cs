using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using ProjectTrash.UI;

public class MenuUiBootstrap : MonoBehaviour
{
    static MenuUiBootstrap _instance;
    TMP_Text _languageLabel;

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
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged += _instance.OnLanguageChanged;
    }

    IEnumerator BuildNextFrame()
    {
        yield return null;
        WireAchievementsButton();
        WireAchievementsBackButton();
        CreateLanguageButton();
        if (LocalizationManager.Instance != null)
        {
            LocalizationManager.Instance.LanguageChanged -= OnLanguageChanged;
            LocalizationManager.Instance.LanguageChanged += OnLanguageChanged;
            LocalizationManager.Instance.RefreshAll();
        }
        UpdateLanguageButtonLabel(_languageLabel);
    }

    void OnLanguageChanged() => UpdateLanguageButtonLabel(_languageLabel);

    static void WireAchievementsButton()
    {
        GameObject buttonObject = GameObject.Find("MainButtonAchievements");
        if (buttonObject == null)
        {
            Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (Transform transform in transforms)
            {
                if (transform.name == "MainButtonAchievements")
                {
                    buttonObject = transform.gameObject;
                    break;
                }
            }
        }

        if (buttonObject == null)
            return;

        Button button = buttonObject.GetComponent<Button>();
        if (button == null)
            return;

        button.onClick.RemoveListener(OpenAchievements);
        button.onClick.AddListener(OpenAchievements);
    }

    static void OpenAchievements()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowPanel("UICanvas_Achievements");
    }

    static void WireAchievementsBackButton()
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform transform in transforms)
        {
            if (transform.name != "BackButton" && transform.name != "MainBackButton")
                continue;

            Transform canvas = transform;
            while (canvas != null && canvas.name != "UICanvas_Achievements")
                canvas = canvas.parent;
            if (canvas == null)
                continue;

            Button button = transform.GetComponent<Button>();
            if (button == null)
                continue;

            button.onClick.RemoveListener(OpenStartScreen);
            button.onClick.AddListener(OpenStartScreen);
            return;
        }
    }

    static void OpenStartScreen()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.ShowPanel("UICanvas_Startscreen");
    }

    void CreateLanguageButton()
    {
        VersionDisplay versionDisplay = FindFirstObjectByType<VersionDisplay>(FindObjectsInactive.Include);
        if (versionDisplay == null)
            return;

        Transform existing = versionDisplay.transform.parent.Find("LanguageToggle");
        if (existing != null)
        {
            _languageLabel = existing.GetComponentInChildren<TMP_Text>(true);
            BindLanguageButton(existing.GetComponent<Button>(), _languageLabel);
            UpdateLanguageButtonLabel(_languageLabel);
            return;
        }

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
        _languageLabel = label;

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.highlightedColor = new Color(1f, 1f, 1f, 0.35f);
        button.colors = colors;
        BindLanguageButton(button, label);
        UpdateLanguageButtonLabel(label);
    }

    static void BindLanguageButton(Button button, TMP_Text label)
    {
        if (button == null)
            return;

        button.onClick.RemoveAllListeners();
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
