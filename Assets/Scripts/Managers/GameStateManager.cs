using LevelSelectInformation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Systems.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

//Singleton Class that keeps track of values representing general Game states
public class GameStateManager : PersistentSingleton<GameStateManager>
{
    public static readonly bool IS_DEVELOPMENT = false;
    public const bool SEASON_1_ACTIVE = true;
    private const float DEV_MODE_PROGRESSION = 999f;

    public SceneData PreviousScene { get; private set; } = SceneData.Get<SceneData.MainMenu>();

    private GameStateData _data;

    private GameStateData Data
    {
        get
        {
            if (_data == null)
            {
                _data = new GetGameStateData().Query();
                seenEnemyActions = _data.SeenEnemyActions.ToHashSet();
            }

            return _data;
        }
    }

    public void UpdateLevelProgress(StageInformation level)
    {
        if (IS_DEVELOPMENT) return;
        CurrentLevelProgress = Mathf.Max(CurrentLevelProgress, level.LevelID);
        SaveLoadSystem.Instance.SaveGame();
    }

    public float CurrentLevelProgress
    {
        get { return (IS_DEVELOPMENT) ? DEV_MODE_PROGRESSION : Data.CurrentLevelProgress; }
        private set => Data.CurrentLevelProgress = value;
    }

    private HashSet<string> seenEnemyActions; // Private backing field for perf
    

    public bool HasSeenEnemyAction(ActionClass a) {
        if (seenEnemyActions == null) seenEnemyActions = Data.SeenEnemyActions.ToHashSet(); // Bind is not always called
        return seenEnemyActions.Contains(a.GetName());
    }
    
    public void AddEnemyActionToSeen(ActionClass a) {
        if (Data.SeenEnemyActions.Contains(a.GetName())) return;
        Data.SeenEnemyActions.Add(a.GetName());
        seenEnemyActions.Add(a.GetName());
    }
    
    public void Restart()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        LoadScene(activeScene.name);
    }

    public void LoadScene(string scene, bool shouldFade = true)
    {
        PreviousScene = SceneData.FromSceneName(SceneManager.GetActiveScene().name);
        if (shouldFade)
        {
            StartCoroutine(FadeAndLoadScene(scene));
        } else {
            SaveLoadSystem.Instance.SaveGame();
            SceneManager.LoadScene(scene);
        }
    }

    public bool RecordFirstTimeEvent(OneTimeEvents eventId)
    {
        if (!Data.SeenOneTimeEvents.Contains(eventId))
        {
            Data.SeenOneTimeEvents.Add(eventId);
            return true;
        }
        return false;
    }


    private bool isFadingOut = false;
    private IEnumerator FadeAndLoadScene(string scene)
    {
        if (!isFadingOut)
        {
            isFadingOut = true;
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInDarkScreen(0.3f));
            yield return new WaitForSeconds(0.1f);
            SaveLoadSystem.Instance.SaveGame();
            SceneManager.LoadScene(scene);
            isFadingOut = false;
            yield return new WaitForEndOfFrame();
            yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(0.5f));
        }
    }

    public const string SORTING_LAYER_TOP = "Top";

    /*
     *
     * TEMPORARY FLAGS
     */

    /*
     * Temporary flag to be set and read by end of combat scene, when the player restarts and should skip dialogue
     * Is set by GameOver prefab upon restart, and read by dialogue classes
     * Dialogue classes should reset this value when read, such that it does not cause unexpected behaviour in upcoming scenes
     */
    public bool JumpToCombat = false;
}


[System.Serializable]
public class GameStateData
{
    /*
     * This is the current state that the player is at
     * The associated values for this should be from [LevelSelectInformation.levelId]
     */
    [field: SerializeField] public float CurrentLevelProgress { get; set; } = 0f;
    [field: SerializeField] public List<string> SeenEnemyActions { get; set; } = new List<string>(); // Must be List<T>, HashSet<T> not serializable
    [field: SerializeField] public List<OneTimeEvents> SeenOneTimeEvents { get; set; } = new List<OneTimeEvents>();

    public override string ToString()
    {
        var items = new List<string>
        {
            "Hexcode: " + RuntimeHelpers.GetHashCode(this),
            "Current player level progress: " + CurrentLevelProgress,
            "Seen Enemy Actions: " + string.Join(";", SeenEnemyActions),
            "Seen One Time Events: " + string.Join(";", SeenOneTimeEvents),
        };
        return string.Join(",", items);
    }
}

[System.Serializable]
public enum OneTimeEvents 
{
    None = 0,
    ExplainBounties = 10,
}
