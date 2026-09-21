using LevelSelectInformation;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Storybook;
using Systems.Persistence;
using UnityEngine;
using UnityEngine.SceneManagement;


public interface IScenePayload { };
public record CombatPayload(SceneData SceneData, bool JumpToCombat) : IScenePayload;
public record StoryBookEntry(StorybookSceneEnum StorybookEntryNode) : IScenePayload;

#nullable enable
//Singleton Class that keeps track of values representing general Game states
public class GameStateManager : PersistentSingleton<GameStateManager>
{
    public static readonly bool IS_DEVELOPMENT = false;
    public const bool SEASON_1_ACTIVE = true;
    private const float DEV_MODE_PROGRESSION = 999f;
    public const int DEV_MODE_BOUNTIES = 6;

    public SceneData PreviousScene { get; private set; } = SceneData.Get<SceneData.MainMenu>();

    private GameStateData? _data;

    private GameStateData Data
    {
        get
        {
            if (_data == null)
            {
                _data = new GetGameStateData().Query()!;
                seenEnemyActions = _data.SeenEnemyActions.ToHashSet();
            }

            return _data;
        }
    }
    private IScenePayload? scenePayload;
    private HashSet<string> seenEnemyActions = new(); // Private backing field for perf

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

    public bool HasSeenEnemyAction(ActionClass a) {
        if (seenEnemyActions == null) seenEnemyActions = Data.SeenEnemyActions.ToHashSet(); // Bind is not always called
        return seenEnemyActions.Contains(a.GetName());
    }
    
    public void AddEnemyActionToSeen(ActionClass a) {
        if (Data.SeenEnemyActions.Contains(a.GetName())) return;
        Data.SeenEnemyActions.Add(a.GetName());
        seenEnemyActions.Add(a.GetName());
    }

    // Has a side effect of consuming the current payload.
    public bool JumpToCombat =>
        Payload<CombatPayload>() is { JumpToCombat: true, SceneData: var sceneData }
        && sceneData == SceneData.CurrentScene();

    public T? Payload<T>() where T : IScenePayload
    {
        if (scenePayload is T payload)
        {
            scenePayload = null;
            return payload;
        }
        return default(T);
    }

    public void LoadScene(string scene, bool shouldFade = true, IScenePayload? payload = null)
    {
        scenePayload = payload;
        PreviousScene = SceneData.CurrentScene();
        if (shouldFade)
        {
            StartCoroutine(FadeAndLoadScene(scene));
        } else {
            SaveLoadSystem.Instance.SaveGame();
            SceneManager.LoadScene(scene);
        }
    }
    
    public void Restart(IScenePayload? payload = null)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        LoadScene(activeScene.name, true, payload);
    }


    // Returns true if first time seeing the event. 
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
// FirstTimePath

[System.Serializable]
public enum OneTimeEvents 
{
    None = 0,
    ShowPrologueGreeting = 10,
    ShowFinalFightUnlock = 11,
    ExplainBounties = 20,
    ShowSeason1Intro = 30,
}
