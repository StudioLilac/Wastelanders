using LevelSelectInformation;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "MainMenuConfig", menuName = "Scriptable Objects/MainMenuConfig")]
public class MainMenuConfig : ScriptableObject
{
    public Sprite backgroundImage;
    public float overlayOpacity;
    public float width;
    public float height;
}

public record GetLevelProgress(): IQuery<float>;
public record GetBountyProgress(): IQuery<float>;

[System.Serializable]
public struct MainMenuConfigHolder
{
    public MainMenuConfig startingBackground;
    public MainMenuConfig season1Background;
    public MainMenuConfig season1AltBackground;
    public MainMenuConfig season2Background;

    public readonly MainMenuConfig GetConfig()
    {
        float levelProgress = new GetLevelProgress().Query();
        float bountyProgress = new GetBountyProgress().Query();

        return true switch
        {
            _ when levelProgress < StageInformation.PRINCESS_FROG_FIGHT.LevelID => startingBackground,
            _ when levelProgress <= StageInformation.IVES_FINALE_FIGHT.LevelID && bountyProgress < 3 => season1Background,
            _ when levelProgress <= StageInformation.IVES_FINALE_FIGHT.LevelID => season1AltBackground,
            _ when levelProgress > StageInformation.IVES_FINALE_FIGHT.LevelID => season2Background,
            _ => startingBackground
        };
    }
}