using BountySystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LevelSelectInformation
{
    // For use in the editor as a serialized placeholder for a level, that is then mapped to an actual static object.  
    public enum Level
    {
        Tutorial,
        FrogSlimeFight,
        BeetleFight,
        QueenFight,
        PrincessFrogFight,
        PrincessFrogBounty,
        IvesFinale
    }

    public interface ILevelSelectInformation
    {
        public float LevelID { get; set; }

        public void UponSelectedEvent();

        // Please update me if you add any new entries in the Level enum
        public static readonly Dictionary<Level, ILevelSelectInformation> LEVEL_INFORMATION = new()
        {
            { Level.Tutorial, StageInformation.TUTORIAL_STAGE },
            { Level.FrogSlimeFight, StageInformation.FROG_SLIME_STAGE },
            { Level.BeetleFight, StageInformation.BEETLE_STAGE },
            { Level.QueenFight, StageInformation.QUEEN_BEETLE_STAGE },
            { Level.PrincessFrogFight, StageInformation.PRINCESS_FROG_FIGHT },
            { Level.PrincessFrogBounty, BountyInformation.PRINCESS_FROG_BOUNTY },
            { Level.IvesFinale, StageInformation.IVES_FINALE_FIGHT },
        };
    }

    // Represents the information needed to load a specfic stage during level select
    public class StageInformation : ILevelSelectInformation
    {
        public static readonly StageInformation TUTORIAL_STAGE = new(sceneName: SceneData.Get<SceneData.TutorialFight>().SceneName, levelId: 0);
        public static readonly StageInformation DECK_SELECTION_TUTORIAL = new(sceneName: SceneData.Get<SceneData.SelectionScreen>().SceneName, levelId: 0.5f);
        public static readonly StageInformation FROG_SLIME_STAGE = new(sceneName: SceneData.Get<SceneData.FrogSlimeFight>().SceneName, levelId: 1f);
        public static readonly StageInformation BEETLE_STAGE = new(sceneName: SceneData.Get<SceneData.BeetleFight>().SceneName, levelId: 2f);
        public static readonly StageInformation QUEEN_PREPARATION_STAGE = new(sceneName: SceneData.Get<SceneData.SelectionScreen>().SceneName, levelId: 2.5f);
        public static readonly StageInformation QUEEN_BEETLE_STAGE = new(sceneName: SceneData.Get<SceneData.PreQueenFight>().SceneName, levelId: 3f);
        public static readonly StageInformation PRINCESS_FROG_FIGHT = new(sceneName: SceneData.Get<SceneData.PrincessFrogBounty>().SceneName, levelId: 4f);
        public static readonly StageInformation IVES_FINALE_FIGHT = new(sceneName: SceneData.Get<SceneData.PrincessFrogBounty>().SceneName, levelId: 5f);
        public string SceneName { get; set; }
        public float LevelID { get; set; }

        public delegate void StageInformationDelegate(string sceneName);
        public static event StageInformationDelegate StageInformationEvent;

        public StageInformation(string sceneName, float levelId)
        {
            SceneName = sceneName;
            LevelID = levelId;
        }

        public void UponSelectedEvent()
        {
            StageInformationEvent?.Invoke(SceneName);
        }
    }

    // Represents the information needed to load a specific bounty stage during level select
    public class BountyInformation : ILevelSelectInformation
    {
        public static readonly BountyInformation PRINCESS_FROG_BOUNTY = new(PrincessFrogBounties.Values, levelId: 4.5f);
        public IEnumerable<IBounties> BountyCollection { get; set; }
        //Bounties are encoded with .5f level id ending
        public float LevelID { get; set; }

        public delegate void BountyInformationDelegate(BountyInformation BountyType);
        public static event BountyInformationDelegate BountyInformationEvent;

        public BountyInformation(IEnumerable<IBounties> bounties, float levelId)
        {
            LevelID = levelId;
            BountyCollection = bounties;
        }

        public void UponSelectedEvent()
        {
            BountyInformationEvent?.Invoke(this);
        }
    }
}