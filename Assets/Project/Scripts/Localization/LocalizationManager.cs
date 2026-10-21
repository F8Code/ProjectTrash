using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.LowLevel;
using UnityEngine.UI;

public class LocalizationManager : MonoBehaviour
{
    public const string LanguagePrefKey = "ProjectTrash.Language";
    public const string EnglishCode = "en";
    public const string ChineseCode = "zh";

    public static LocalizationManager Instance { get; private set; }

    static readonly Regex NumericOnly = new(@"^[\d\s./xX:%+\-]+$", RegexOptions.Compiled);

    readonly Dictionary<int, string> _sourceTexts = new();
    readonly Dictionary<int, TMP_FontAsset> _originalFonts = new();
    TMP_FontAsset _chineseFont;
    public string CurrentLanguage { get; private set; } = EnglishCode;
    public bool IsChinese => CurrentLanguage == ChineseCode;

    public event Action LanguageChanged;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Bootstrap()
    {
        if (Instance != null)
            return;

        GameObject host = new("LocalizationManager");
        host.AddComponent<LocalizationManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        CurrentLanguage = PlayerPrefs.GetString(LanguagePrefKey, EnglishCode);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start() => RefreshAll();

    void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _sourceTexts.Clear();
        _originalFonts.Clear();
        RefreshAll();
    }

    public void ToggleLanguage()
    {
        SetLanguage(IsChinese ? EnglishCode : ChineseCode);
    }

    public void SetLanguage(string languageCode)
    {
        CurrentLanguage = languageCode == ChineseCode ? ChineseCode : EnglishCode;
        PlayerPrefs.SetString(LanguagePrefKey, CurrentLanguage);
        PlayerPrefs.Save();
        RefreshAll();
        LanguageChanged?.Invoke();
    }

    public string Get(string source)
    {
        if (string.IsNullOrWhiteSpace(source) || !IsChinese)
            return source;

        string trimmed = source.Trim();
        if (LocalizationTable.Chinese.TryGetValue(trimmed, out string translated))
            return translated;

        if (trimmed.EndsWith(":") && LocalizationTable.Chinese.TryGetValue(trimmed.TrimEnd(':').Trim() + ":", out translated))
            return translated;

        return source;
    }

    public void RefreshAll()
    {
        EnsureChineseFont();

        TMP_Text[] labels = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text label in labels)
            ApplyToLabel(label);

        TMP_Dropdown[] dropdowns = FindObjectsByType<TMP_Dropdown>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Dropdown dropdown in dropdowns)
            ApplyToDropdown(dropdown);
    }

    public void ApplyFont(TMP_Text label)
    {
        if (label == null)
            return;

        int id = label.GetInstanceID();
        if (!_originalFonts.ContainsKey(id) && label.font != null && label.font != _chineseFont)
            _originalFonts[id] = label.font;

        if (IsChinese && _chineseFont != null)
        {
            label.font = _chineseFont;
            if (!string.IsNullOrEmpty(label.text))
                _chineseFont.TryAddCharacters(label.text);
        }
        else if (_originalFonts.TryGetValue(id, out TMP_FontAsset original) && original != null)
        {
            label.font = original;
        }
    }

    void ApplyToLabel(TMP_Text label)
    {
        if (label == null || label.GetComponent<ProjectTrash.UI.VersionDisplay>() != null)
            return;

        int id = label.GetInstanceID();
        if (!_sourceTexts.ContainsKey(id))
        {
            string current = label.text ?? "";
            if (ShouldSkip(current) && label.name != "LanguageToggle" && label.transform.parent?.name != "LanguageToggle")
            {
                ApplyFont(label);
                return;
            }

            _sourceTexts[id] = current;
        }

        string source = _sourceTexts[id];
        label.text = Get(source);
        ApplyFont(label);
    }

    void ApplyToDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown == null)
            return;

        bool changed = false;
        for (int i = 0; i < dropdown.options.Count; i++)
        {
            string source = dropdown.options[i].text;
            string translated = Get(source);
            if (translated != dropdown.options[i].text)
            {
                dropdown.options[i].text = translated;
                changed = true;
            }
        }

        if (changed)
            dropdown.RefreshShownValue();

        if (dropdown.captionText != null)
            ApplyFont(dropdown.captionText);
        if (dropdown.itemText != null)
            ApplyFont(dropdown.itemText);
    }

    void EnsureChineseFont()
    {
        if (_chineseFont != null)
            return;

        _chineseFont = Resources.Load<TMP_FontAsset>("Fonts/ChineseSDF");
        if (_chineseFont != null)
            return;

        Font source = Resources.Load<Font>("Fonts/ChineseSource");
        if (source == null)
        {
            Debug.LogWarning("[Localization] No Chinese font found in Resources/Fonts. Chinese text will show as squares until ChineseSource.ttc is imported.");
            return;
        }

        _chineseFont = TMP_FontAsset.CreateFontAsset(
            source,
            90,
            8,
            GlyphRenderMode.SDFAA,
            2048,
            2048,
            AtlasPopulationMode.Dynamic);

        if (_chineseFont != null)
            _chineseFont.name = "RuntimeChineseSDF";
    }

    static bool ShouldSkip(string text)
    {
        if (string.IsNullOrWhiteSpace(text) || text == "\u200B")
            return true;
        if (text.Contains("Build:"))
            return true;
        if (NumericOnly.IsMatch(text.Trim()))
            return true;
        return false;
    }
}
