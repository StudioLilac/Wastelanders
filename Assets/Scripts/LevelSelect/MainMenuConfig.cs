using Entities;
using LevelSelectInformation;
using UnityEngine;
using static LevelSelectInformation.StageInformation;

[System.Serializable]
[CreateAssetMenu(fileName = "MainMenuConfig", menuName = "Scriptable Objects/MainMenuConfig")]
public class MainMenuConfig : ScriptableObject
{
    public Sprite backgroundImage;
    public float overlayOpacity;
    public float width;
    public float height;
}


[System.Serializable]
public struct MainMenuConfigHolder
{
    public MainMenuConfig startingBackground;
    public MainMenuConfig season1Background;
    public MainMenuConfig season1AltBackground;
    public MainMenuConfig season2Background;

    public readonly MainMenuConfig GetConfig()
    {
        float? levelProgress = GameStateManager.Instance.CurrentLevelProgress;
        int? bountyProgress = BountyManager.Instance.GetBountyProgress();

        if (levelProgress == null || bountyProgress == null)
            Debug.LogError("Level progress or bounty progress is null. Returning starting background.");
        
        return true switch
        {
            _ when Get<Season2>().UnlockCriteriaMet() => season2Background,
            _ when Get<PrincessFrogFight>().UnlockCriteriaMet() && bountyProgress > 3 => season1AltBackground,
            _ when Get<PrincessFrogFight>().UnlockCriteriaMet() => season1Background,
            _ => startingBackground
        };
    }
}