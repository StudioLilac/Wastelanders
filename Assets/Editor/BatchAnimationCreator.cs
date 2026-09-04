using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Batch-creates looping SpriteRenderer AnimationClips from a folder of subfolders.
/// Each immediate subfolder of the selected folder becomes one AnimationClip,
/// built from all Sprites found inside it (sorted by asset path/name).
///
/// Usage:
///   1. Put this script anywhere inside an "Editor" folder in your project
///      (e.g. Assets/Editor/BatchAnimationCreator.cs).
///   2. In the Project window, select the ROOT folder that contains your
///      subfolders (e.g. "Backgrounds/SceneA" which contains "Frames" etc.)
///      -- or select the subfolders directly, both are supported.
///   3. Right-click -> "Create Animations From Subfolders".
///   4. Each subfolder gets a matching .anim file saved inside it.
/// </summary>
public static class BatchAnimationCreator
{
    // Standardized framerate for all generated clips. Change as needed.
    private const float FrameRate = 12f;

    [MenuItem("Assets/Create Animations From Subfolders")]
    private static void CreateAnimationsMenuItem()
    {
        var selection = Selection.GetFiltered<UnityEngine.Object>(SelectionMode.Assets);
        int clipsCreated = 0;

        foreach (var obj in selection)
        {
            string path = AssetDatabase.GetAssetPath(obj);
            if (!Directory.Exists(path))
                continue;

            var subfolders = Directory.GetDirectories(path);

            if (subfolders.Length > 0)
            {
                // Treat selected folder as a "root" containing per-animation subfolders.
                foreach (var folder in subfolders)
                {
                    if (TryCreateAnimationForFolder(folder, FrameRate))
                        clipsCreated++;
                }
            }
            else
            {
                // Selected folder itself has no subfolders -- treat it as the
                // animation folder directly (useful if you select subfolders one by one).
                if (TryCreateAnimationForFolder(path, FrameRate))
                    clipsCreated++;
            }
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[BatchAnimationCreator] Created {clipsCreated} animation clip(s).");
    }

    private static bool TryCreateAnimationForFolder(string folderPath, float frameRate)
    {
        // Find all sprite assets directly under this folder (and any sub-sprites
        // inside multi-sprite sheets), sorted by asset path for deterministic order.
        string[] guids = AssetDatabase.FindAssets("t:Sprite", new[] { folderPath });

        var sprites = guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Distinct()
            .OrderBy(p => p, System.StringComparer.OrdinalIgnoreCase)
            .SelectMany(p => AssetDatabase.LoadAllAssetsAtPath(p).OfType<Sprite>()
                .OrderBy(s => s.name, System.StringComparer.OrdinalIgnoreCase))
            .ToArray();

        if (sprites.Length == 0)
        {
            Debug.LogWarning($"[BatchAnimationCreator] No sprites found in '{folderPath}', skipping.");
            return false;
        }

        var clip = new AnimationClip { frameRate = frameRate };

        var spriteBinding = new EditorCurveBinding
        {
            type = typeof(SpriteRenderer),
            path = "", // root object of whatever GameObject this clip is applied to
            propertyName = "m_Sprite"
        };

        var keyframes = new ObjectReferenceKeyframe[sprites.Length];
        for (int i = 0; i < sprites.Length; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = i / frameRate,
                value = sprites[i]
            };
        }

        AnimationUtility.SetObjectReferenceCurve(clip, spriteBinding, keyframes);

        // Make it loop by default.
        var settings = AnimationUtility.GetAnimationClipSettings(clip);
        settings.loopTime = true;
        AnimationUtility.SetAnimationClipSettings(clip, settings);

        string folderName = new DirectoryInfo(folderPath).Name;
        string clipPath = Path.Combine(folderPath, folderName + ".anim").Replace("\\", "/");

        // Avoid overwrite errors if run twice -- delete existing clip of same name first.
        var existing = AssetDatabase.LoadAssetAtPath<AnimationClip>(clipPath);
        if (existing != null)
            AssetDatabase.DeleteAsset(clipPath);

        AssetDatabase.CreateAsset(clip, clipPath);
        return true;
    }
}