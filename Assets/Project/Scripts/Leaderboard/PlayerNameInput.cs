using UnityEngine;
using TMPro;

/// <summary>
/// Place this script in your Main Menu scene to allow players to set their name
/// The name will be used in the leaderboard automatically
/// </summary>
public class PlayerNameInput : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField _nameInputField;
    
    [Header("Settings")]
    [SerializeField] private int _maxNameLength = 20;
    [SerializeField] private string _defaultName = "Anonymous";

    private void Start()
    {
        // Setup input field
        if (_nameInputField != null)
        {
      _nameInputField.characterLimit = _maxNameLength;
     
// Load saved name
     string savedName = LeaderboardUI.GetCurrentPlayerName();
            _nameInputField.text = savedName;
            
    // Listen for changes
            _nameInputField.onEndEdit.AddListener(OnNameChanged);
        }
      else
        {
       Debug.LogWarning("[PlayerNameInput] Name Input Field is not assigned!");
        }
    }

    private void OnDestroy()
    {
        if (_nameInputField != null)
    {
    _nameInputField.onEndEdit.RemoveListener(OnNameChanged);
        }
    }

  /// <summary>
  /// Called when player finishes editing their name
    /// </summary>
    private void OnNameChanged(string newName)
    {
    SavePlayerName(newName);
    }

    /// <summary>
    /// Saves the player name to PlayerPrefs
    /// </summary>
    public void SavePlayerName(string playerName)
    {
        // Trim whitespace
        playerName = playerName?.Trim() ?? "";
  
        // Use default if empty
      if (string.IsNullOrWhiteSpace(playerName))
        {
            playerName = _defaultName;
        }
        
  // Save to PlayerPrefs
 LeaderboardUI.SetPlayerName(playerName);
        
        // Update input field display
        if (_nameInputField != null)
        {
   _nameInputField.text = playerName;
  }
        
        Debug.Log($"<color=green>[PlayerNameInput] Name saved: {playerName}</color>");
    }

    /// <summary>
    /// Call this from a button to explicitly save the name
    /// </summary>
  public void OnSaveButtonClicked()
    {
        if (_nameInputField != null)
        {
      SavePlayerName(_nameInputField.text);
   }
    }

    /// <summary>
    /// Resets the name to default
    /// </summary>
    public void ResetToDefault()
    {
        SavePlayerName(_defaultName);
        Debug.Log($"<color=yellow>[PlayerNameInput] Name reset to default: {_defaultName}</color>");
    }
}
