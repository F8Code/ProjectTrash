#if UNITY_EDITOR
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.LowLevel;

public static class CreateChineseFontAsset
{
    const string SourceFontPath = "Assets/Resources/Fonts/ChineseSource.ttc";
    const string OutputPath = "Assets/Resources/Fonts/ChineseSDF.asset";

    [InitializeOnLoadMethod]
    static void AutoCreate()
    {
        if (AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(OutputPath) != null)
            return;
        if (AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath) == null)
            return;

        EditorApplication.delayCall += Create;
    }

    [MenuItem("Project Trash/Create Chinese TMP Font")]
    public static void Create()
    {
        Font source = AssetDatabase.LoadAssetAtPath<Font>(SourceFontPath);
        if (source == null)
        {
            Debug.LogError($"[Localization] Missing source font at {SourceFontPath}. Copy a Simplified Chinese TTF there (for example Microsoft YaHei).");
            return;
        }

        TMP_FontAsset fontAsset = TMP_FontAsset.CreateFontAsset(
            source,
            90,
            8,
            GlyphRenderMode.SDFAA,
            2048,
            2048,
            AtlasPopulationMode.Dynamic);

        if (fontAsset == null)
        {
            Debug.LogError("[Localization] Failed to create Chinese TMP font asset.");
            return;
        }

        fontAsset.name = "ChineseSDF";
        Directory.CreateDirectory("Assets/Resources/Fonts");
        AssetDatabase.CreateAsset(fontAsset, OutputPath);
        if (fontAsset.material != null)
            AssetDatabase.AddObjectToAsset(fontAsset.material, fontAsset);
        if (fontAsset.atlasTextures != null)
        {
            foreach (Texture atlas in fontAsset.atlasTextures)
            {
                if (atlas != null)
                    AssetDatabase.AddObjectToAsset(atlas, fontAsset);
            }
        }

        EditorUtility.SetDirty(fontAsset);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"[Localization] Created Chinese TMP font at {OutputPath}");
    }
}
#endif
