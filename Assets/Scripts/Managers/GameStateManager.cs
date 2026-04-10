using LevelSelectInformation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Systems.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;

public record GetLevelProgress() : IQuery<float?>;

//Singleton Class that keeps track of values representing general Game states
public class GameStateManager : PersistentSingleton<GameStateManager>, IBind<GameStateData>
{
    public static readonly bool IS_DEVELOPMENT = true;
    public const bool SEASON_1_ACTIVE = true;

    public SceneData PreviousScene { get; private set; } = SceneData.Get<SceneData.MainMenu>();

    //Fields for persistence
    [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();
    private GameStateData _data;

    public GameStateData Data 
    {
        get
        {
            // Data should only be nullable during development where you can open a scene from any place
            if (_data == null)
            {
                SaveLoadSystem.Instance.LoadGameStateInformation();
            }

            return _data;
        }
        set
        {
            _data = value;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        this.Answer<GetLevelProgress, float?>(_ => CurrentLevelProgress);
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

    public void Bind(GameStateData bindedData) {
        this.Data = bindedData;
        this.Data.Id = bindedData.Id;
        seenEnemyActions = bindedData.SeenEnemyActions.ToHashSet();
    }

    public void LoadScene(string scene)
    {
        PreviousScene = SceneData.FromSceneName(SceneManager.GetActiveScene().name);
        StartCoroutine(FadeAndLoadScene(scene));
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
public class GameStateData : ISaveable
{
    [field: SerializeField] public SerializableGuid Id { get; set; } = SerializableGuid.NewGuid();

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
            "Id: " + Id,
            "Hexcode: " + RuntimeHelpers.GetHashCode(this),
            "Current player level progress: " + CurrentLevelProgress
        };
        return string.Join(",", items);
    }
}

public enum OneTimeEvents 
{
    None = 0,
    ExplainBounties = 10,
}
