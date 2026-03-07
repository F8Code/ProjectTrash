using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles Pause Menu UI button interactions
/// </summary>
public class PauseMenuUI : MonoBehaviour
{
    [Header("Debug")]
    [SerializeField] private bool _showDebugLogs = true;

    private void OnEnable()
    {
        if (_showDebugLogs)
            Debug.Log("<color=green>[PauseMenuUI] Pause menu enabled</color>");
    }

    private void OnDisable()
    {
        if (_showDebugLogs)
            Debug.Log("<color=yellow>[PauseMenuUI] Pause menu disabled</color>");
    }

    /// <summary>
    /// Resume game - called by Resume Work button
    /// </summary>
    public void OnResumeClicked()
    {
        if (_showDebugLogs)
            Debug.Log("<color=cyan>[PauseMenuUI] Resume button clicked</color>");

        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();
    }

    /// <summary>
    /// Restart game - called by Restart button
    /// </summary>
    public void OnRestartClicked()
    {
        if (_showDebugLogs)
            Debug.Log("<color=cyan>[PauseMenuUI] Restart button clicked</color>");

        // Resume first to unpause the game
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();

        // Load game scene
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    /// <summary>
    /// Go to main menu - called by Main Menu button
    /// </summary>
    public void OnMainMenuClicked()
    {
        if (_showDebugLogs)
            Debug.Log("<color=cyan>[PauseMenuUI] Main Menu button clicked</color>");

        // Resume first to unpause the game
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();

        // Load main menu scene
        SceneManager.LoadScene(GameConstants.Scene.MainMenu);
    }

    /// <summary>
    /// Quit game - called by Quit button
    /// </summary>
    public void OnQuitClicked()
    {
        if (_showDebugLogs)
            Debug.Log("<color=cyan>[PauseMenuUI] Quit button clicked</color>");

        // Resume first to unpause the game
        if (GameManager.Instance != null)
            GameManager.Instance.ResumeGame();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        Debug.Log("<color=yellow>[PauseMenuUI] Quit - Editor stopped</color>");
#else
        Application.Quit();
    Debug.Log("<color=yellow>[PauseMenuUI] Quit - Application closed</color>");
#endif
    }
}