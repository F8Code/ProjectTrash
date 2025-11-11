using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized UI panel manager with animation support
/// Singleton pattern for easy access throughout the game
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Panel Management")]
    [SerializeField] private List<UIPanel> panels = new();

    private UIPanel currentPanel;
    private readonly Dictionary<string, UIPanel> panelDictionary = new();

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start() => InitializePanels();

    private void InitializePanels()
    {
        panelDictionary.Clear();

        foreach (var panel in panels)
        {
            panel.Initialize();

            if (!string.IsNullOrEmpty(panel.panelName))
                panelDictionary[panel.panelName] = panel;

            if (panel.startActive && panel.panelObject != null)
                currentPanel = panel;
        }

        Debug.Log($"<color=green>[UIManager] Initialized {panelDictionary.Count} panels</color>");
    }

    /// <summary>
    /// Show a panel by name with animation
    /// </summary>
    public void ShowPanel(string panelName)
    {
        if (panelDictionary.TryGetValue(panelName, out UIPanel panel))
            ShowPanel(panel);
    }

    /// <summary>
    /// Show a panel with animation
    /// </summary>
    public void ShowPanel(UIPanel panel)
    {
        if (panel == null || panel.panelObject == null) return;

        Debug.Log($"<color=cyan>[UIManager] Showing panel: {panel.panelName}</color>");

        // Hide current panel if exists
        if (currentPanel != null && currentPanel != panel)
        {
            HidePanel(currentPanel, () =>
            {
                DisplayPanel(panel);
            });
        }
        else
        {
            DisplayPanel(panel);
        }
    }

    private void DisplayPanel(UIPanel panel)
    {
        // Enable the Canvas (not GameObject)
        panel.SetActive(true);
        currentPanel = panel;

        // Play entry sound if enabled
        if (panel.playSound && panel.entrySoundClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(
                panel.entrySoundClip,
                panel.entrySoundVolume,
                panel.soundPriority,
                mixerType: AudioManager.AudioMixerType.SFX);
        }

        if (panel.useAnimation)
        {
            if (panel.animationType == UIPanel.AnimationType.Slide && panel.animatedTransform != null)
            {
                // Ensure curve is valid
                AnimationCurve curve = panel.entryCurve;
                if (curve == null || curve.keys.Length == 0)
                    curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

                UITweenAnimator.Instance.AnimatePanelEntry(
                    panel.animatedTransform,
                    panel.entryDirection,
                    panel.animationDuration,
                    curve
                );
            }
            else if (panel.animationType == UIPanel.AnimationType.Fade && panel.canvasGroup != null)
            {
                // Ensure curve is valid
                AnimationCurve curve = panel.fadeInCurve;
                if (curve == null || curve.keys.Length == 0)
                    curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

                UITweenAnimator.Instance.AnimateFadeIn(
                    panel.canvasGroup,
                    panel.animationDuration,
                    curve
                );
            }
        }
    }

    /// <summary>
    /// Hide a panel with animation
    /// </summary>
    public void HidePanel(string panelName, System.Action onComplete = null)
    {
        if (panelDictionary.TryGetValue(panelName, out UIPanel panel))
            HidePanel(panel, onComplete);
        else
            onComplete?.Invoke();
    }

    /// <summary>
    /// Hide a panel with animation
    /// </summary>
    public void HidePanel(UIPanel panel, System.Action onComplete = null)
    {
        if (panel == null || panel.panelObject == null)
        {
            onComplete?.Invoke();
            return;
        }

        // Play exit sound if enabled
        if (panel.playSound && panel.exitSoundClip != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayAudio(
                panel.exitSoundClip,
                panel.exitSoundVolume,
                panel.soundPriority,
                mixerType: AudioManager.AudioMixerType.SFX);
        }

        if (panel.useAnimation)
        {
            if (panel.animationType == UIPanel.AnimationType.Slide && panel.animatedTransform != null)
            {
                // Ensure curve is valid
                AnimationCurve curve = panel.exitCurve;
                if (curve == null || curve.keys.Length == 0)
                    curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

                UITweenAnimator.Instance.AnimatePanelExit(
                    panel.animatedTransform,
                    panel.exitDirection,
                    panel.animationDuration,
                    curve,
                    () =>
                    {
                        panel.SetActive(false);
                        onComplete?.Invoke();
                    }
                );
            }
            else if (panel.animationType == UIPanel.AnimationType.Fade && panel.canvasGroup != null)
            {
                // Ensure curve is valid
                AnimationCurve curve = panel.fadeOutCurve;
                if (curve == null || curve.keys.Length == 0)
                    curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

                UITweenAnimator.Instance.AnimateFadeOut(
                    panel.canvasGroup,
                    panel.animationDuration,
                    curve,
                    () =>
                    {
                        panel.SetActive(false);
                        onComplete?.Invoke();
                    }
                );
            }
        }
        else
        {
            panel.SetActive(false);
            onComplete?.Invoke();
        }
    }

    /// <summary>
    /// Hide all panels
    /// </summary>
    public void HideAllPanels()
    {
        foreach (var panel in panels)
        {
            if (panel.panelObject != null)
                panel.SetActive(false);
        }
        currentPanel = null;
        Debug.Log("<color=cyan>[UIManager] All panels hidden</color>");
    }

    public void LoadScene(string _sceneName) => UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneName);

    public void LoadScene(int _sceneIndex) => UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneIndex);

    public void QuitGame() => Application.Quit();
}