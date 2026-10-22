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
    readonly Dictionary<int, Vector3> _originalFontSizes = new();
    readonly Dictionary<int, string[]> _dropdownSources = new();
    TMP_FontAsset _chineseFont;
    const float TitleFontScale = 0.55f;
    const float MenuFontScale = 0.8f;
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
        _originalFontSizes.Clear();
        _dropdownSources.Clear();
        RefreshAll();
        StartCoroutine(RefreshAfterLateSpawn());
    }

    System.Collections.IEnumerator RefreshAfterLateSpawn()
    {
        yield return null;
        yield return null;
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

    public void SetLabelSource(TMP_Text label, string englishSource)
    {
        if (label == null || string.IsNullOrWhiteSpace(englishSource))
            return;

        _sourceTexts[label.GetInstanceID()] = englishSource;
        label.text = Get(englishSource);
        ApplyFont(label);
    }

    public string Get(string source)
    {
        if (string.IsNullOrWhiteSpace(source))
            return source;

        if (!IsChinese)
        {
            if (ContainsCjk(source) && TryGetEnglishSource(source, out string english))
                return english;
            return source;
        }

        if (TryTranslate(source, out string translated))
            return translated;

        if (source.TrimStart().StartsWith("FPS:", StringComparison.OrdinalIgnoreCase))
        {
            int colon = source.IndexOf(':');
            return Get("FPS:") + source[(colon + 1)..];
        }

        return source;
    }

    static bool TryTranslate(string source, out string translated)
    {
        foreach (string key in CandidateKeys(source))
        {
            if (LocalizationTable.Chinese.TryGetValue(key, out translated))
                return true;
        }

        translated = null;
        return false;
    }

    static IEnumerable<string> CandidateKeys(string source)
    {
        string trimmed = source.Trim().Trim('\u200B');
        yield return trimmed;

        string collapsed = Regex.Replace(trimmed, @"\s+", " ");
        if (collapsed != trimmed)
            yield return collapsed;

        string unspacedLetters = CollapseSpacedLetters(collapsed);
        if (unspacedLetters != collapsed)
            yield return unspacedLetters;

        if (collapsed.EndsWith(":"))
            yield return collapsed.TrimEnd(':').Trim() + ":";
    }

    static string CollapseSpacedLetters(string source)
    {
        string[] parts = source.Split((char[])null, StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
            return source;

        foreach (string part in parts)
        {
            if (part.Length != 1)
                return source;
        }

        return string.Concat(parts);
    }

    static bool TryGetEnglishSource(string chinese, out string english)
    {
        string trimmed = Regex.Replace(chinese.Trim().Trim('\u200B'), @"\s+", " ");
        foreach (KeyValuePair<string, string> pair in LocalizationTable.Chinese)
        {
            if (string.Equals(pair.Value, trimmed, StringComparison.Ordinal))
            {
                english = pair.Key;
                return true;
            }
        }

        english = null;
        return false;
    }

    public void RefreshAll()
    {
        EnsureChineseFont();

        TMP_Text[] labels = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text label in labels)
        {
            ApplyFontScale(label);
            ApplyToLabel(label);
        }

        TMP_Dropdown[] dropdowns = FindObjectsByType<TMP_Dropdown>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Dropdown dropdown in dropdowns)
            ApplyToDropdown(dropdown);
    }

    void ApplyFontScale(TMP_Text label)
    {
        if (label == null)
            return;

        int id = label.GetInstanceID();
        if (!_originalFontSizes.ContainsKey(id))
            _originalFontSizes[id] = new Vector3(label.fontSize, label.fontSizeMin, label.fontSizeMax);

        Vector3 sizes = _originalFontSizes[id];
        bool worldSpace = sizes.x > 0f && sizes.x < 8f;
        if (worldSpace)
            return;

        float scale = sizes.x >= 120f ? TitleFontScale : MenuFontScale;
        if (!IsChinese)
            scale *= sizes.x >= 120f ? 1.2f : 1.3f;
        label.fontSize = sizes.x * scale;
        label.fontSizeMin = sizes.y * scale;
        label.fontSizeMax = Mathf.Max(sizes.z * scale, label.fontSize);
        label.enableWordWrapping = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Overflow;
    }

    public void ApplyFont(TMP_Text label)
    {
        if (label == null)
            return;

        int id = label.GetInstanceID();
        if (!_originalFonts.ContainsKey(id) && label.font != null && label.font != _chineseFont)
            _originalFonts[id] = label.font;

        ApplyFontScale(label);

        bool needsCjkFont = _chineseFont != null && (IsChinese || IsLanguageToggle(label));
        if (needsCjkFont)
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

        if (IsLanguageToggle(label))
        {
            ApplyFont(label);
            return;
        }

        int id = label.GetInstanceID();
        if (!_sourceTexts.ContainsKey(id))
        {
            string current = label.text ?? "";
            if (ShouldSkip(current))
            {
                ApplyFont(label);
                return;
            }

            _sourceTexts[id] = ResolveSource(current.Trim().Trim('\u200B'));
        }

        _sourceTexts[id] = ResolveSource(_sourceTexts[id]);
        string source = _sourceTexts[id];
        label.text = Get(source);
        ApplyFont(label);
    }

    void ApplyToDropdown(TMP_Dropdown dropdown)
    {
        if (dropdown == null)
            return;

        int id = dropdown.GetInstanceID();
        if (!_dropdownSources.ContainsKey(id))
        {
            string[] sources = new string[dropdown.options.Count];
            for (int i = 0; i < dropdown.options.Count; i++)
                sources[i] = ResolveSource(dropdown.options[i].text);
            _dropdownSources[id] = sources;
        }

        bool changed = false;
        string[] originals = _dropdownSources[id];
        for (int i = 0; i < dropdown.options.Count && i < originals.Length; i++)
        {
            string translated = Get(originals[i]);
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

#if UNITY_WEBGL
        Debug.LogWarning("[Localization] Skipping runtime Chinese font atlas on WebGL.");
        return;
#endif

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

    string ResolveSource(string current)
    {
        if (ContainsCjk(current) && TryGetEnglishSource(current, out string english))
            return english;
        return current;
    }

    static bool IsLanguageToggle(TMP_Text label)
    {
        if (label.name == "LanguageToggle")
            return true;
        Transform parent = label.transform.parent;
        return parent != null && parent.name == "LanguageToggle";
    }

    static bool ContainsCjk(string text)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        foreach (char character in text)
        {
            if (character >= 0x4E00 && character <= 0x9FFF)
                return true;
        }

        return false;
    }
}
