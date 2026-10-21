using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementUI : MonoBehaviour
{
    [SerializeField] Transform _content;
    [SerializeField] TMP_FontAsset _font;

    void OnEnable()
    {
        Refresh();
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged += Refresh;
    }

    void OnDisable()
    {
        if (LocalizationManager.Instance != null)
            LocalizationManager.Instance.LanguageChanged -= Refresh;
    }

    public void Refresh()
    {
        if (_content == null || AchievementManager.Instance == null)
            return;

        for (int i = _content.childCount - 1; i >= 0; i--)
            Destroy(_content.GetChild(i).gameObject);

        TMP_FontAsset font = _font;
        if (font == null)
        {
            TMP_Text existing = GetComponentInChildren<TMP_Text>(true);
            if (existing != null)
                font = existing.font;
        }

        foreach (AchievementDefinition definition in AchievementManager.Instance.Definitions)
        {
            bool unlocked = AchievementManager.Instance.IsUnlocked(definition.Id);
            GameObject card = CreateCard(definition, unlocked, font);
            card.transform.SetParent(_content, false);
        }

        LocalizationManager.Instance?.RefreshAll();
    }

    static GameObject CreateCard(AchievementDefinition definition, bool unlocked, TMP_FontAsset font)
    {
        GameObject card = new($"Achievement_{definition.Id}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.GetComponent<LayoutElement>().preferredWidth = 320f;
        card.GetComponent<LayoutElement>().preferredHeight = 420f;
        card.GetComponent<LayoutElement>().minWidth = 280f;
        card.GetComponent<Image>().color = unlocked ? new Color(1f, 1f, 1f, 0.16f) : new Color(0.15f, 0.15f, 0.15f, 0.55f);

        VerticalLayoutGroup column = card.AddComponent<VerticalLayoutGroup>();
        column.padding = new RectOffset(20, 20, 24, 24);
        column.spacing = 12f;
        column.childAlignment = TextAnchor.UpperCenter;
        column.childForceExpandHeight = false;
        column.childForceExpandWidth = true;

        GameObject iconObject = new("Icon", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        iconObject.transform.SetParent(card.transform, false);
        iconObject.GetComponent<LayoutElement>().preferredHeight = 140f;
        iconObject.GetComponent<LayoutElement>().preferredWidth = 140f;
        Image icon = iconObject.GetComponent<Image>();
        icon.color = unlocked ? new Color(0.95f, 0.82f, 0.35f, 1f) : new Color(0.35f, 0.35f, 0.35f, 1f);
        icon.sprite = definition.Icon;

        CreateText(card.transform, definition.Title, 32f, font, FontStyles.Bold);
        CreateText(card.transform, definition.Description, 22f, font, FontStyles.Normal);
        CreateText(card.transform, unlocked ? "Unlocked" : "Locked", 20f, font, FontStyles.Italic);
        return card;
    }

    static void CreateText(Transform parent, string value, float size, TMP_FontAsset font, FontStyles style)
    {
        GameObject textObject = new("Label", typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);
        TextMeshProUGUI label = textObject.GetComponent<TextMeshProUGUI>();
        label.text = value;
        label.fontSize = size;
        label.alignment = TextAlignmentOptions.Center;
        label.enableWordWrapping = true;
        label.fontStyle = style;
        label.color = Color.white;
        if (font != null)
            label.font = font;
        textObject.GetComponent<LayoutElement>().preferredHeight = size * 2.4f;
    }
}
