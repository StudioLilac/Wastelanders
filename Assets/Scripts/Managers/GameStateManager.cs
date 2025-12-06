using LevelSelectInformation;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Systems.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

//Singleton Class that keeps track of values representing general Game states
public class GameStateManager : PersistentSingleton<GameStateManager>, IBind<GameStateData>
{
    public static readonly bool IS_DEVELOPMENT = true;

    public SceneData PreviousScene { get; private set; } = SceneData.Get<SceneData.MainMenu>();

    //Fields for persistence
    [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
    private GameStateData data;

    public GameStateData Data 
    {
        get
        {
            // Data should only be nullable during development where you can open a scene from any place
            if (data == null)
            {
                SaveLoadSystem.Instance.LoadGameStateInformation();
            }

            return data;
        }
        set
        {
            data = value;
        }
    }

    public void UpdateLevelProgress(ILevelSelectInformation level)
    {
        CurrentLevelProgress = Mathf.Max(CurrentLevelProgress, level.LevelID);
    }

    public float CurrentLevelProgress
    {
        get { return (IS_DEVELOPMENT) ? 999f : Data.CurrentLevelProgress; }
        set => Data.CurrentLevelProgress = value;
    }

    public void Restart()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        LoadScene(activeScene.name);
    }

    public void Bind(GameStateData bindedData)
    {
        this.Data = bindedData;
        this.Data.Id = bindedData.Id;
    }

    public void LoadScene(string scene)
    {
        PreviousScene = SceneData.FromSceneName(SceneManager.GetActiveScene().name);
        StartCoroutine(FadeAndLoadScene(scene));
    }

    private IEnumerator FadeAndLoadScene(string scene)
    {
        yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInDarkScreen(0.6f));
        yield return new WaitForSeconds(0.2f);
        SaveLoadSystem.Instance.SaveGame();
        SceneManager.LoadScene(scene);
        yield return new WaitForEndOfFrame();
        yield return StartCoroutine(UIFadeScreenManager.Instance.FadeInLightScreen(0.6f));
    }

    public const string SORTING_LAYER_TOP = "Top";

    /*
     * TEMPORARY FLAGS
     */

    /*
     * Temporary flag to be set and read by end of combat scene, when the player restarts and should skip dialogue
     * Is set by GameOver prefab upon restart, and read by dialogue classes
     * Dialogue classes should reset this value when read, such that it does not cause unexpected behaviour in upcoming scenes
     */
    public bool JumpToCombat = false;

    // Check for level select whether player finished the game first time to display bounty dialogue
    public bool FirstTimeFinished = false;
}


[System.Serializable]
public class GameStateData : ISaveable
{
    [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();

    /*
     * This is the current state that the player is at
     * The associated values for this should be from [LevelSelectInformation.levelId]
     */
    [field: SerializeField] public float CurrentLevelProgress { get; set; } = 0f;


    public override string ToString()
    {
        var items = new List<string>
        {
            "Id: " + Id,
            "Hexcode: " + RuntimeHelpers.GetHashCode(this),
            "Current player level progress: " + CurrentLevelProgress
        };
        return string.Join(",", items);
    }
}
