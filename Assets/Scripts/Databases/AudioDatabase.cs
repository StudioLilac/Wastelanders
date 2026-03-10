using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Audio Database", menuName = "Audio Database")]
public class AudioDatabase : ScriptableObject
{
    public SceneAudio Empty;

    [Header("Menu & Navigation Scenes")]
    public SceneAudio MainMenu;
    public SceneAudio Credits;

    [Header("Combat & Story Scenes")]
    public SceneAudio TutorialFight;
    public SceneAudio FrogSlimeFight;
    public SceneAudio BeetleFight;
    public SceneAudio PreQueenFight;
    public SceneAudio PostQueenFight;
    public SceneAudio PrincessFrogBounty;
    public SceneAudio TeaserScene;

    [Header("Epilogue Scenes")]
    public SceneAudio Epilogue6;
    public SceneAudio Epilogue7;
    public SceneAudio Epilogue8;
}
