#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

[InitializeOnLoad]
static class SteamworksDefine
{
    const string Define = "STEAMWORKS_NET";

    static SteamworksDefine()
    {
        bool installed = PackageInstalled();
        NamedBuildTarget[] targets =
        {
            NamedBuildTarget.Standalone,
            NamedBuildTarget.Android
        };

        foreach (NamedBuildTarget target in targets)
        {
            PlayerSettings.GetScriptingDefineSymbols(target, out string[] defines);
            bool hasDefine = System.Array.IndexOf(defines, Define) >= 0;
            if (installed && !hasDefine)
            {
                string[] next = new string[defines.Length + 1];
                defines.CopyTo(next, 0);
                next[defines.Length] = Define;
                PlayerSettings.SetScriptingDefineSymbols(target, next);
            }
            else if (!installed && hasDefine)
            {
                var remaining = new System.Collections.Generic.List<string>(defines);
                remaining.Remove(Define);
                PlayerSettings.SetScriptingDefineSymbols(target, remaining.ToArray());
            }
        }

        WriteAppIdFile();
    }

    static bool PackageInstalled()
    {
        if (Directory.Exists(Path.Combine("Packages", "com.rlabrecque.steamworks.net")))
            return true;

        string cache = Path.Combine("Library", "PackageCache");
        if (!Directory.Exists(cache))
            return false;

        foreach (string directory in Directory.GetDirectories(cache, "com.rlabrecque.steamworks.net*"))
        {
            if (Directory.Exists(directory))
                return true;
        }

        return false;
    }

    static void WriteAppIdFile()
    {
        if (SteamAppId.Value == 0)
            return;

        string path = Path.Combine(Directory.GetParent(Application.dataPath)!.FullName, "steam_appid.txt");
        File.WriteAllText(path, SteamAppId.Value.ToString());
    }
}

class SteamAppIdBuildCopy : IPostprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPostprocessBuild(BuildReport report)
    {
        if (SteamAppId.Value == 0)
            return;

        BuildTarget target = report.summary.platform;
        if (target != BuildTarget.StandaloneWindows &&
            target != BuildTarget.StandaloneWindows64 &&
            target != BuildTarget.StandaloneLinux64 &&
            target != BuildTarget.StandaloneOSX)
            return;

        string dir = Path.GetDirectoryName(report.summary.outputPath);
        if (string.IsNullOrEmpty(dir))
            return;

        File.WriteAllText(Path.Combine(dir, "steam_appid.txt"), SteamAppId.Value.ToString());
    }
}
#endif
