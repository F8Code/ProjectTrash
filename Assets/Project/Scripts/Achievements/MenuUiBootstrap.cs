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
        WireStartGameButtons();
        FixMenuHoverIcons();
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

    static void FixMenuHoverIcons()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Button button in buttons)
        {
            if (!button.name.StartsWith("MainButton"))
                continue;
            if (button.transform.Find("Logo") == null)
                continue;
            if (button.GetComponent<MenuHoverIcon>() == null)
                button.gameObject.AddComponent<MenuHoverIcon>();
        }
    }

    static void WireStartGameButtons()
    {
        WireNamedButton("MainButtonStartMistake", StartMistakeMode);
        WireNamedButton("MainButtonStartTime", StartTimeMode);
        WireNamedButton("MainButtonStart", StartMistakeMode);
    }

    static void StartMistakeMode()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.LoadScene(2);
    }

    static void StartTimeMode()
    {
        if (UIManager.Instance != null)
            UIManager.Instance.LoadScene(4);
    }

    static void WireNamedButton(string objectName, UnityEngine.Events.UnityAction action)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform transform in transforms)
        {
            if (transform.name != objectName)
                continue;

            Button button = transform.GetComponent<Button>();
            if (button == null)
                continue;

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }
    }

    void CreateLanguageButton()
    {
        VersionDisplay versionDisplay = FindFirstObjectByType<VersionDisplay>(FindObjectsInactive.Include);
        if (versionDisplay == null)
            return;

        Transform parent = versionDisplay.transform.parent;
        Transform existing = parent.Find("LanguageToggle");
        if (existing != null)
            Destroy(existing.gameObject);

        RectTransform versionRect = versionDisplay.GetComponent<RectTransform>();
        const float edgePadding = 48f;
        versionRect.anchoredPosition = new Vector2(-edgePadding, 16f);

        GameObject buttonObject = CreateLanguageButtonFromTemplate(parent);
        if (buttonObject == null)
            buttonObject = CreatePlainLanguageButton(parent, versionDisplay.GetComponent<TMP_Text>());

        buttonObject.name = "LanguageToggle";
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        rect.SetParent(parent, false);
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.sizeDelta = new Vector2(280f, 56f);
        rect.anchoredPosition = new Vector2(-edgePadding, 74f);
        rect.localScale = Vector3.one;

        Transform logo = buttonObject.transform.Find("Logo");
        if (logo != null)
            logo.gameObject.SetActive(false);

        TextMeshProUGUI label = buttonObject.GetComponentInChildren<TextMeshProUGUI>(true);
        if (label != null)
        {
            RectTransform labelRect = label.rectTransform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            labelRect.anchoredPosition = Vector2.zero;
            label.alignment = TextAlignmentOptions.Center;
            label.enableAutoSizing = true;
            label.fontSizeMin = 22f;
            label.fontSizeMax = 44f;
            label.raycastTarget = false;
            TMP_Text versionText = versionDisplay.GetComponent<TMP_Text>();
            if (versionText != null)
            {
                label.font = versionText.font;
                label.fontSharedMaterial = versionText.fontSharedMaterial;
                label.color = versionText.color;
            }
        }

        _languageLabel = label;
        Button button = buttonObject.GetComponent<Button>();
        if (button != null)
        {
            button.onClick = new Button.ButtonClickedEvent();
            BindLanguageButton(button, label);
        }

        UpdateLanguageButtonLabel(label);
    }

    static GameObject CreateLanguageButtonFromTemplate(Transform parent)
    {
        string[] preferredNames =
        {
            "MainButtonSettings",
            "MainButtonCredits",
            "MainButtonLeaderBoard",
            "MainButtonAchievements"
        };

        foreach (string name in preferredNames)
        {
            GameObject template = FindButtonNamed(name);
            if (template == null)
                continue;

            GameObject clone = Instantiate(template, parent);
            clone.name = "LanguageToggle";
            return clone;
        }

        return null;
    }

    static GameObject FindButtonNamed(string objectName)
    {
        Transform[] transforms = FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (Transform transform in transforms)
        {
            if (transform.name == objectName && transform.GetComponent<Button>() != null)
                return transform.gameObject;
        }

        return null;
    }

    static GameObject CreatePlainLanguageButton(Transform parent, TMP_Text versionText)
    {
        GameObject buttonObject = new("LanguageToggle", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = new Color(0.20784315f, 0.20784315f, 0.20784315f, 0f);
        image.sprite = Resources.GetBuiltinResource<Sprite>("UI/Skin/UISprite.psd");

        GameObject labelObject = new("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(buttonObject.transform, false);
        TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
        if (versionText != null)
        {
            label.font = versionText.font;
            label.fontSharedMaterial = versionText.fontSharedMaterial;
            label.color = versionText.color;
        }

        return buttonObject;
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
