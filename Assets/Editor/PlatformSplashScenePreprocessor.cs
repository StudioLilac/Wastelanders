#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using static SceneData;

public class PlatformSplashScenePreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        bool isWebGL = report.summary.platform == BuildTarget.WebGL;

        var scenes = EditorBuildSettings.scenes;
        bool modified = false;

        for (int i = 0; i < scenes.Length; i++)
        {
            if (scenes[i].path.EndsWith($"{Get<SplashScreenWebGL>().SceneName}.unity"))
            {
                if (scenes[i].enabled != isWebGL)
                {
                    scenes[i].enabled = isWebGL;
                    modified = true;
                }
            }
            else if (scenes[i].path.EndsWith($"{Get<SplashScreen>().SceneName}.unity"))
            {
                if (scenes[i].enabled == isWebGL)
                {
                    scenes[i].enabled = !isWebGL;
                    modified = true;
                }
            }
        }

        if (modified)
        {
            EditorBuildSettings.scenes = scenes;
            Debug.Log($"[BuildPreprocessor] Automatically configured Splash scene for {report.summary.platform}: SplashScreenWebGL={(isWebGL ? "Enabled" : "Disabled")}, SplashScreen={(isWebGL ? "Disabled" : "Enabled")}");
        }
    }
}
#endif
