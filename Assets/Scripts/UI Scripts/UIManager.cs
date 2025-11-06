using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    [Header("Panel Management")]
    [SerializeField] private List<UIPanel> panels = new();

    private UIPanel currentPanel;
    private readonly Dictionary<string, UIPanel> panelDictionary = new();

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
    }

    /// <summary>
    /// Show a panel by name with animation
    /// </summary>
    public void ShowPanel(string panelName)
    {
        if (panelDictionary.TryGetValue(panelName, out UIPanel panel))
            ShowPanel(panel);
        else
            Debug.LogWarning($"Panel '{panelName}' not found!");
    }

    /// <summary>
    /// Show a panel with animation
    /// </summary>
    public void ShowPanel(UIPanel panel)
    {
        if (panel == null || panel.panelObject == null) return;

        // Hide current panel if exists
        if (currentPanel != null && currentPanel != panel)
        {
            HidePanel(currentPanel, () =>
                       {
                           DisplayPanel(panel);
                       });
        }
        else
            DisplayPanel(panel);
    }

    private void DisplayPanel(UIPanel panel)
    {
        // Enable the Canvas (not GameObject)
        panel.SetActive(true);
        currentPanel = panel;

        if (panel.useAnimation && panel.animatedTransform != null)
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
    }

    /// <summary>
    /// Hide a panel with animation
    /// </summary>
    public void HidePanel(string panelName, System.Action onComplete = null)
    {
        if (panelDictionary.TryGetValue(panelName, out UIPanel panel))
        {
            HidePanel(panel, onComplete);
        }
        else
        {
            Debug.LogWarning($"Panel '{panelName}' not found!");
            onComplete?.Invoke();
        }
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

        if (panel.useAnimation && panel.animatedTransform != null)
        {
            // Ensure curve is valid
            AnimationCurve curve = panel.exitCurve;
            if (curve == null || curve.keys.Length == 0)
                curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

            UITweenAnimator.Instance.AnimatePanelExit(
                 panel.animatedTransform,
                 panel.exitDirection,  // Use panel-specific direction
           panel.animationDuration,
                      curve,
                   () =>
                 {
                     panel.SetActive(false);  // Only disables Canvas, not GameObject
                     onComplete?.Invoke();
                 }
                      );
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
    }

    public void LoadScene(string _sceneName) => UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneName);

    public void LoadScene(int _sceneIndex) => UnityEngine.SceneManagement.SceneManager.LoadScene(_sceneIndex);
}