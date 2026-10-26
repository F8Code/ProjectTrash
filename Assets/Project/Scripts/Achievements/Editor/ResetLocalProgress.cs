#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

static class ResetLocalProgress
{
    [MenuItem("Project Trash/Reset Local Achievements and Leaderboard")]
    static void Reset()
    {
        PlayerPrefs.DeleteKey("ProjectTrash.Achievements");
        PlayerPrefs.DeleteKey("LeaderboardData");
        PlayerPrefs.Save();
        Debug.Log("[Project Trash] Local achievements and leaderboard cleared. Enter Play Mode to load a fresh board.");
        EditorUtility.DisplayDialog(
            "Progress reset",
            "Local achievements and leaderboard were cleared.\n\nStop Play Mode if it is running, then enter Play Mode again to test from a locked state.",
            "OK");
    }
}
#endif
