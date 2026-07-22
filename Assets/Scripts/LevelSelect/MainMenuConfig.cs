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
            _ when levelProgress < Get<PrincessFrogFight>().LevelID => startingBackground,
            _ when levelProgress <= Get<IvesFinale>().LevelID && bountyProgress <= 3 => season1Background,
            _ when levelProgress <= Get<IvesFinale>().LevelID && bountyProgress > 3 => season1AltBackground,
            _ when levelProgress > Get<IvesFinale>().LevelID && bountyProgress == 6 => season2Background,
            _ => startingBackground
        };
    }
}