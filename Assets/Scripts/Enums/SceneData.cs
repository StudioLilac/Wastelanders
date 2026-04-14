using LevelSelectInformation;
using System;
using System.Collections.Generic;
using System.Linq;
using Context;
using UnityEngine;
using static SceneDataHelpers;

#nullable enable
public abstract class SceneData : Enum<SceneData>
{
    public abstract string SceneName { get; }
    public abstract SceneAudio GetAudio(AudioDatabase database);
    
    // Prefabs in here are initialized on EVERY scene.
    protected virtual MonoBehaviour[] AlwaysPresentPrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
    {
        prefabs.uiFadeScreenManager,
        prefabs.audioManager,
        prefabs.timeManager
    };

    // Other prefabs specific to the scene go here.
    public virtual MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => Array.Empty<MonoBehaviour>();
    
    public MonoBehaviour[] RequiredPrefabs(SceneInitializerPrefabs prefabs) =>
        AlwaysPresentPrefabs(prefabs).Concat(ScenePrefabs(prefabs)).ToArray();

    // Default values. Override per-scene if necessary.
    public virtual UIContext UIContextOnEntry => new UIContext.Dialogue();

    public class SplashScreen : SceneData {
        public override string SceneName => "SplashScreen";
        public override SceneAudio GetAudio(AudioDatabase database) => SelectMainMenuMusic(database);
        
        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            {  };
        
        public override UIContext UIContextOnEntry => new UIContext.None();
    }
    
    public class SplashScreenWebGL : SceneData {
        public override string SceneName => "SplashScreenWebGL";
        public override SceneAudio GetAudio(AudioDatabase database) => SelectMainMenuMusic(database);
        
        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            {  };
        
        public override UIContext UIContextOnEntry => new UIContext.None();
    }

    public class MainMenu : SceneData
    {
        public override string SceneName => "MainMenu";
        public override SceneAudio GetAudio(AudioDatabase database) => SelectMainMenuMusic(database);

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.pauseMenuV2, prefabs.popupManager, prefabs.bountyManager };
        
        public override UIContext UIContextOnEntry => new UIContext.None();
    }

    public class SelectionScreen : SceneData
    {
        public override string SceneName => "SelectionScreen";
        public override SceneAudio GetAudio(AudioDatabase database) => SelectMainMenuMusic(database);

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.pauseMenuV2, prefabs.dialogueManager, prefabs.deckSelectV2, prefabs.dialogueBoxV2, prefabs.bountyManager };
        
        public override UIContext UIContextOnEntry => new UIContext.Custom(UIContextCustomFlags.DialogueLog);
    }

    public class LevelSelect : SceneData
    {
        public override string SceneName => "LevelSelect";
        public override SceneAudio GetAudio(AudioDatabase database) => SelectMainMenuMusic(database);

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.pauseMenuV2, prefabs.dialogueManager, prefabs.popupManager, prefabs.bountyManager };
        
        public override UIContext UIContextOnEntry => new UIContext.Custom(UIContextCustomFlags.DialogueLog);
    }

    public class ContractSelect : SceneData
    {
        public override string SceneName => "ContractSelect";
        public override SceneAudio GetAudio(AudioDatabase database) => SelectMainMenuMusic(database);

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.pauseMenuV2, prefabs.bountyManager };
    }

    public class Credits : SceneData
    {
        public override string SceneName => "Credits";
        public override SceneAudio GetAudio(AudioDatabase database) => database.Credits;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.pauseMenuV2 };
        
        public override UIContext UIContextOnEntry => new UIContext.Custom(UIContextCustomFlags.SkipDialogue);
    }

    public class TutorialFight : SceneData
    {
        public override string SceneName => "TutorialScene";
        public override SceneAudio GetAudio(AudioDatabase database) => database.TutorialFight;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.combatFadeScreenManager, prefabs.combatManager, prefabs.battleQueue, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.tooltip, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro, prefabs.dialogueBoxV2, prefabs.arrowIndicatorManager };
    }

    public class FrogSlimeFight : SceneData
    {
        public override string SceneName => "FrogSlimeFight";
        public override SceneAudio GetAudio(AudioDatabase database) => database.FrogSlimeFight;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.combatFadeScreenManager, prefabs.combatManager, prefabs.battleQueue, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.tooltip, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro, prefabs.dialogueBoxV2, prefabs.arrowIndicatorManager };
    }

    public class BeetleFight : SceneData
    {
        public override string SceneName => "BeetleFightScene";
        public override SceneAudio GetAudio(AudioDatabase database) => database.BeetleFight;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.combatFadeScreenManager, prefabs.combatManager, prefabs.battleQueue, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.tooltip, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro, prefabs.dialogueBoxV2, prefabs.arrowIndicatorManager };
    }

    public class PreQueenFight : SceneData
    {
        public override string SceneName => "PreQueenFightScene";
        public override SceneAudio GetAudio(AudioDatabase database) => database.PreQueenFight;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.combatFadeScreenManager, prefabs.combatManager, prefabs.battleQueue, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.tooltip, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro, prefabs.dialogueBoxV2, prefabs.arrowIndicatorManager };
    }

    public class PostQueenFight : SceneData
    {
        public override string SceneName => "PostQueenBeetle";
        public override SceneAudio GetAudio(AudioDatabase database) => database.PostQueenFight;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.pauseMenuV2, prefabs.dialogueManager, prefabs.dialogueBoxV2 };
    }

    public class PrincessFrogBounty : SceneData
    {
        public override string SceneName => "PrincessFrogCombatScene";
        public override SceneAudio GetAudio(AudioDatabase database) => database.PrincessFrogBounty;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.combatFadeScreenManager, prefabs.combatManager, prefabs.battleQueue, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.tooltip, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro, prefabs.dialogueBoxV2, prefabs.arrowIndicatorManager };
    }

    public class Epilogue : SceneData
    {
        public override string SceneName => "Epilogue";
        public override SceneAudio GetAudio(AudioDatabase database) => database.Empty;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.pauseMenuV2, prefabs.bountyManager };
    }
    
    public class PreBounty0 : SceneData {
        public override string SceneName => "PreBounty_0";
        public override SceneAudio GetAudio(AudioDatabase database) => database.Tundra;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.pauseMenuV2, prefabs.dialogueManager, prefabs.dialogueBoxV2};
    }

    public class PreBounty_1 : SceneData
    {
        public override string SceneName => "PreBounty_1";
        public override SceneAudio GetAudio(AudioDatabase database) => database.TutorialFight;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.pauseMenuV2, prefabs.dialogueManager, prefabs.dialogueBoxV2 };
    }

    public class PreBounty2 : SceneData
    {
        public override string SceneName => "PreBounty_2";

        public override SceneAudio GetAudio(AudioDatabase database) => database.Empty;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
        {
            prefabs.pauseMenuV2,
            prefabs.dialogueManager,
            prefabs.dialogueBoxV2
        };
    }

    public class Epilogue_3 : SceneData
    {
        public override string SceneName => "Epilogue_3";

        public override SceneAudio GetAudio(AudioDatabase database) => database.Epilogue3;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
            { prefabs.combatFadeScreenManager, prefabs.combatManager, prefabs.battleQueue, prefabs.pauseMenuV2, prefabs.hudV2, prefabs.tooltip, prefabs.dialogueManager, prefabs.popupManager,  prefabs.gameOver, prefabs.battleIntro, prefabs.dialogueBoxV2, prefabs.arrowIndicatorManager  };
    };

    public class Epilogue_4 : SceneData
    {
        public override string SceneName => "Epilogue_4";

        public override SceneAudio GetAudio(AudioDatabase database) => database.Epilogue4;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
        {
            prefabs.pauseMenuV2,
            prefabs.dialogueManager,
            prefabs.dialogueBoxV2
        };
    }
    
    public class Epilogue_5 : SceneData 
    {
        public override string SceneName => "Epilogue_5";
        
        public override SceneAudio GetAudio(AudioDatabase database) => database.Empty;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
        {
            prefabs.pauseMenuV2,
            prefabs.dialogueManager,
            prefabs.dialogueBoxV2
        };
    }

    public class Epilogue_6 : SceneData
    {
        public override string SceneName => "Epilogue_6";

        public override SceneAudio GetAudio(AudioDatabase database) => database.Epilogue6;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
        {
            prefabs.pauseMenuV2,
            prefabs.dialogueManager,
            prefabs.dialogueBoxV2
        };
    }
    public class Epilogue_7 : SceneData
    {
        public override string SceneName => "Epilogue_7";

        public override SceneAudio GetAudio(AudioDatabase database) => database.Epilogue7;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
        {
            prefabs.pauseMenuV2,
            prefabs.dialogueManager,
            prefabs.dialogueBoxV2
        };
    }

    public class Epilogue_8 : SceneData
    {
        public override string SceneName => "Epilogue_8";

        public override SceneAudio GetAudio(AudioDatabase database) => database.Epilogue8;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
        {
            prefabs.pauseMenuV2,
            prefabs.dialogueManager,
            prefabs.dialogueBoxV2
        };
    }

    public class Epilogue_9 : SceneData
    {
        public override string SceneName => "Epilogue_9";

        public override SceneAudio GetAudio(AudioDatabase database) => database.Epilogue8;

        public override MonoBehaviour[] ScenePrefabs(SceneInitializerPrefabs prefabs) => new MonoBehaviour[]
        {
            prefabs.pauseMenuV2,
            prefabs.dialogueManager,
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

public static class SceneDataHelpers
{
    public static SceneAudio SelectMainMenuMusic(AudioDatabase database) => GameStateManager.Instance.CurrentLevelProgress switch
    {
        var progress when progress < StageInformation.Get<StageInformation.PrincessFrogFight>().LevelID => database.MainMenu,
        var progress when progress >= StageInformation.Get<StageInformation.PrincessFrogFight>().LevelID => database.Tundra,
        _ => database.MainMenu,
    };
}