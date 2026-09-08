using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;

public class SpriteMaxSizeSetter : EditorWindow
{
    [MenuItem("Tools/Sprites/Set Max Size To 1024")]
    public static void SetMaxSizeOnSelectedFolder()
    {
        // Get the folder(s) currently selected in the Project window
        string[] selectedGuids = Selection.assetGUIDs;

        if (selectedGuids.Length == 0)
        {
            EditorUtility.DisplayDialog("No Folder Selected",
                "Please select a folder in the Project window first.", "OK");
            return;
        }

        int changedCount = 0;
        int totalCount = 0;

        foreach (string guid in selectedGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (!AssetDatabase.IsValidFolder(path))
                continue;

            // Find all texture assets recursively under this folder
            string[] textureGuids = AssetDatabase.FindAssets("t:Texture2D", new[] { path });

            foreach (string texGuid in textureGuids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(texGuid);
                TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

                if (importer == null)
                    continue;

                // Only touch sprites
                if (importer.textureType != TextureImporterType.Sprite)
                    continue;

                totalCount++;

                bool changed = false;

                // Default max size
                if (importer.maxTextureSize != 1024)
                {
                    importer.maxTextureSize = 1024;
                    changed = true;
                }

                // Per-platform overrides (Standalone, Android, iOS, WebGL, etc.)
                foreach (string platform in new[] { "Standalone", "Android", "iPhone", "WebGL" })
                {
                    var settings = importer.GetPlatformTextureSettings(platform);
                    if (settings.overridden && settings.maxTextureSize != 1024)
                    {
                        settings.maxTextureSize = 1024;
                        importer.SetPlatformTextureSettings(settings);
                        changed = true;
                    }
                }

                if (changed)
                {
                    EditorUtility.SetDirty(importer);
                    importer.SaveAndReimport();
                    changedCount++;
                }
            }
        }

        AssetDatabase.Refresh();

        Debug.Log($"Sprite Max Size Setter: Checked {totalCount} sprites, updated {changedCount} to maxSize=1024.");
    }
}