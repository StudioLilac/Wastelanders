using System;
using System.Collections.Generic;
using UnityEngine;

#nullable enable
public abstract class SceneData : Enum<SceneData>
{
    public abstract string SceneName { get; }
    public abstract SceneAudio GetAudio(AudioDatabase database);

    // All Prefabs placed in this array will be instantiated in that scene on start
    public virtual MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => Array.Empty<MonoBehaviour>();

    public class SplashScreen : SceneData {
        public override string SceneName => "SplashScreen";
        public override SceneAudio GetAudio(AudioDatabase database) => database.MainMenu;
        
        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager };
    }
    
    public class SplashScreenWebGL : SceneData {
        public override string SceneName => "SplashScreenWebGL";
        public override SceneAudio GetAudio(AudioDatabase database) => database.MainMenu;
        
        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager };
    }

    public class MainMenu : SceneData
    {
        public override string SceneName => "MainMenu";
        public override SceneAudio GetAudio(AudioDatabase database) => database.MainMenu;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.pauseMenuV2 };
    }

    public class SelectionScreen : SceneData
    {
        public override string SceneName => "SelectionScreen";
        public override SceneAudio GetAudio(AudioDatabase database) => database.MainMenu;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.pauseMenuV2, prefabs.dialogueManager, prefabs.deckSelectV2 };
    }

    public class LevelSelect : SceneData
    {
        public override string SceneName => "LevelSelect";
        public override SceneAudio GetAudio(AudioDatabase database) => database.MainMenu;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.pauseMenuV2, prefabs.dialogueManager, prefabs.popupManager };
    }

    public class ContractSelect : SceneData
    {
        public override string SceneName => "ContractSelect";
        public override SceneAudio GetAudio(AudioDatabase database) => database.MainMenu;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.pauseMenuV2 };
    }

    public class Credits : SceneData
    {
        public override string SceneName => "Credits";
        public override SceneAudio GetAudio(AudioDatabase database) => database.Credits;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.pauseMenuV2 };
    }

    public class TutorialFight : SceneData
    {
        public override string SceneName => "TutorialScene";
        public override SceneAudio GetAudio(AudioDatabase database) => database.TutorialFight;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.combatFadeScreenManager, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro, prefabs.dialogueBoxV2 };
    }

    public class FrogSlimeFight : SceneData
    {
        public override string SceneName => "FrogSlimeFight";
        public override SceneAudio GetAudio(AudioDatabase database) => database.FrogSlimeFight;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.combatFadeScreenManager, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro, prefabs.dialogueBoxV2  };
    }

    public class BeetleFight : SceneData
    {
        public override string SceneName => "BeetleFightScene";
        public override SceneAudio GetAudio(AudioDatabase database) => database.BeetleFight;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.combatFadeScreenManager, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro, prefabs.dialogueBoxV2  };
    }

    public class PreQueenFight : SceneData
    {
        public override string SceneName => "PreQueenFightScene";
        public override SceneAudio GetAudio(AudioDatabase database) => database.PreQueenFight;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.combatFadeScreenManager, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro  };
    }

    public class PostQueenFight : SceneData
    {
        public override string SceneName => "PostQueenBeetle";
        public override SceneAudio GetAudio(AudioDatabase database) => database.PostQueenFight;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.combatFadeScreenManager, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.dialogueManager, prefabs.combatFadeScreenManager };
    }

    public class PrincessFrogBounty : SceneData
    {
        public override string SceneName => "PrincessFrogCombatScene";
        public override SceneAudio GetAudio(AudioDatabase database) => database.PrincessFrogBounty;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.combatFadeScreenManager, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro  };
    }

    public class Epilogue : SceneData
    {
        public override string SceneName => "Epilogue";
        public override SceneAudio GetAudio(AudioDatabase database) => database.Empty;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.pauseMenuV2 };
    }

    public class PreBounty0 : SceneData {
        public override string SceneName => "PreBounty_0";
        public override SceneAudio GetAudio(AudioDatabase database) => database.Empty;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.pauseMenuV2, prefabs.dialogueManager };
    }

    public class PreBounty_1 : SceneData
    {
        public override string SceneName => "PreBounty_1";
        public override SceneAudio GetAudio(AudioDatabase database) => database.TutorialFight;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.audioManager, prefabs.uiFadeScreenManager, prefabs.pauseMenuV2, prefabs.dialogueManager };
    }

    public class PreBounty2 : SceneData
    {
        public override string SceneName => "PreBounty_2";

        public override SceneAudio GetAudio(AudioDatabase database) => database.Empty;

        public override MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
        {
            prefabs.audioManager,
            prefabs.pauseMenuV2,
            prefabs.dialogueManager,
            prefabs.uiFadeScreenManager,
            prefabs.dialogueBoxV2
        };
    }

    private static readonly Dictionary<string, SceneData> _sceneLookup = new();

    static SceneData()
    {
        foreach (var sceneDataItem in Values)
        {
            if (!_sceneLookup.ContainsKey(sceneDataItem.SceneName))
            {
                _sceneLookup.Add(sceneDataItem.SceneName, sceneDataItem);
            }
            else
            {
                Debug.LogError($"Duplicate SceneName '{sceneDataItem.SceneName}' found in SceneData subclasses. " +
                               $"Each SceneData must have a unique SceneName.");
            }
        }
    }

    public static SceneData FromSceneName(string sceneName)
    {
        if (!_sceneLookup.TryGetValue(sceneName, out var sceneData) || sceneData == null)
            throw new ArgumentException($"Scene '{sceneName}' not found in scene data. Please add an entry.");
        return sceneData;
    }


    public static SceneData Get<T>() where T : SceneData => ParseFromType(typeof(T));
}