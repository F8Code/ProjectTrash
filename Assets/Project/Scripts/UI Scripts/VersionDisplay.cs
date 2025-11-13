using UnityEngine;
using TMPro;

namespace ProjectTrash.UI
{
    /// <summary>
    /// Displays the application version number on a TextMeshProUGUI component
    /// Automatically updates the text on Start
    /// </summary>
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class VersionDisplay : MonoBehaviour
    {
        [Header("Version Display Settings")]
        [Tooltip("Prefix text before the version number (e.g., 'Version: ', 'v', 'Build ')")]
        [SerializeField] private string _prefix = "v";

        [Tooltip("Suffix text after the version number (e.g., ' Beta', ' Alpha')")]
        [SerializeField] private string _suffix = "";

        [Tooltip("If true, includes build number from Application.buildGUID")]
        [SerializeField] private bool _showBuildInfo = false;

        [Header("Format Settings")]
        [Tooltip("Custom format string. Use {0} for version. Example: 'Version {0}'")]
        [SerializeField] private string _customFormat = "";

        [Header("References")]
        [SerializeField] private TextMeshProUGUI _versionText;

        [Header("Debug")]
        [SerializeField] private bool _showDebugLogs = true;

        private void Awake()
        {
            // Auto-assign if not set
            if (_versionText == null)
                _versionText = GetComponent<TextMeshProUGUI>();
        }

        private void Start()
        {
            UpdateVersionDisplay();
        }

        /// <summary>
        /// Updates the version display text
        /// </summary>
        public void UpdateVersionDisplay()
        {
            if (_versionText == null)
            {
                Debug.LogError($"<color=red>[VersionDisplay] TextMeshProUGUI component not found on {gameObject.name}</color>");
                return;
            }

            string versionString = GetVersionString();
            _versionText.text = versionString;

            if (_showDebugLogs)
                Debug.Log($"<color=cyan>[VersionDisplay] Version set to: {versionString}</color>");
        }

        /// <summary>
        /// Gets the formatted version string
        /// </summary>
        private string GetVersionString()
        {
            string version = Application.version;

            // If custom format is specified, use it
            if (!string.IsNullOrEmpty(_customFormat))
            {
                return string.Format(_customFormat, version);
            }

            // Build the version string with prefix and suffix
            string versionString = $"{_prefix}{version}{_suffix}";

            // Add build info if enabled
            if (_showBuildInfo)
            {
                string buildGuid = Application.buildGUID;
                if (!string.IsNullOrEmpty(buildGuid))
                {
                    // Show only the first 8 characters of the build GUID
                    string shortBuild = buildGuid.Length > 8 ? buildGuid.Substring(0, 8) : buildGuid;
                    versionString += $" (Build: {shortBuild})";
                }
            }

            return versionString;
        }

        /// <summary>
        /// Manually set the version text (useful for testing)
        /// </summary>
        public void SetCustomVersion(string customVersion)
        {
            if (_versionText != null)
            {
                _versionText.text = customVersion;

                if (_showDebugLogs)
                    Debug.Log($"<color=cyan>[VersionDisplay] Custom version set to: {customVersion}</color>");
            }
        }

        /// <summary>
        /// Get the current application version
        /// </summary>
        public static string GetApplicationVersion()
        {
            return Application.version;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Editor-only: Preview the version display
        /// </summary>
        [ContextMenu("Preview Version Display")]
        private void PreviewVersionDisplay()
        {
            if (_versionText == null)
                _versionText = GetComponent<TextMeshProUGUI>();

            UpdateVersionDisplay();
        }

        /// <summary>
        /// Editor-only: Open Project Settings to edit version
        /// </summary>
        [ContextMenu("Open Project Settings (Version)")]
        private void OpenProjectSettings()
        {
            UnityEditor.SettingsService.OpenProjectSettings("Project/Player");
            Debug.Log("<color=cyan>[VersionDisplay] Opening Project Settings > Player. Look for 'Version' field.</color>");
        }
#endif
    }
}
