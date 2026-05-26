using BountySystem;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable enable
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
        string Title { get; }
        float LevelID { get; }
        Level? SelectableLevel { get; }
        bool LevelEnabled { get; }
        void UponSelectedEvent();
        bool UnlockCriteriaMet();
        string UnlockRequirementsText();

        static readonly Dictionary<Level, ILevelSelectInformation> LEVEL_INFORMATION;
        static ILevelSelectInformation()
        {
            LEVEL_INFORMATION = StageInformation.Stages.Cast<ILevelSelectInformation>()
                .Concat(BountyInformation.Bounties)
                .Where(v => v.SelectableLevel.HasValue)
                .ToDictionary(v => v.SelectableLevel!.Value);
        }
    }

    public record StageInformationEvent(string SceneName) : IEvent;

    // Represents the information needed to load a specific stage during level select
    public abstract class StageInformation : Enum<StageInformation>, ILevelSelectInformation
    {
        public abstract string SceneName { get; }
        public abstract float LevelID { get; }
        public virtual Level? SelectableLevel => null;
        public virtual string Title => string.Empty;
        public virtual bool LevelEnabled => true;
        public virtual bool UnlockCriteriaMet() => LevelID <= GameStateManager.Instance.CurrentLevelProgress;
        public virtual string UnlockRequirementsText() => $"Complete previous levels to unlock.";

        public class Tutorial : StageInformation
        {
            public override string Title => "1. TUTORIAL";
            public override string SceneName => SceneData.Get<SceneData.TutorialFight>().SceneName;
            public override float LevelID => 0f;
            public override Level? SelectableLevel => Level.Tutorial;
        }

        public class DeckSelectionTutorial : StageInformation
        {
            public override string SceneName => SceneData.Get<SceneData.SelectionScreen>().SceneName;
            public override float LevelID => 0.5f;
        }

        public class FrogSlime : StageInformation
        {
            public override string Title => "2. THE EXAM BEGINS!";
            public override string SceneName => SceneData.Get<SceneData.FrogSlimeFight>().SceneName;
            public override float LevelID => 1f;
            public override Level? SelectableLevel => Level.FrogSlimeFight;
        }

        public class Beetle : StageInformation
        {
            public override string Title => "3. HOARDERS";
            public override string SceneName => SceneData.Get<SceneData.BeetleFight>().SceneName;
            public override float LevelID => 2f;
            public override Level? SelectableLevel => Level.BeetleFight;
        }

        public class QueenBeetle : StageInformation
        {
            public override string Title => "4. CRYSTALLIZATION";
            public override string SceneName => SceneData.Get<SceneData.PreQueenFight>().SceneName;
            public override float LevelID => 3f;
            public override Level? SelectableLevel => Level.QueenFight;
        }

        public class PrincessFrogFight : StageInformation
        {
            public override string Title => "EX 1. CORONATION";
            public override string SceneName => SceneData.Get<SceneData.PreBounty_1>().SceneName;
            public override float LevelID => 4f;
            public override bool LevelEnabled => GameStateManager.SEASON_1_ACTIVE;
            public override Level? SelectableLevel => Level.PrincessFrogFight;
            public override bool UnlockCriteriaMet() => LevelID <= GameStateManager.Instance.CurrentLevelProgress && GameStateManager.SEASON_1_ACTIVE;
            public override string UnlockRequirementsText() => true switch
            {
                !GameStateManager.SEASON_1_ACTIVE => "Coming Soon!",
                _ => $"Complete previous levels to unlock."
            };
        }

        public class IvesFinale : StageInformation
        {
            public override string Title => "EX 2. SUCCESSION";
            public override string SceneName => SceneData.Get<SceneData.PrincessFrogBounty>().SceneName;
            public override float LevelID => 5f;
            public override bool LevelEnabled => GameStateManager.SEASON_1_ACTIVE;
            public override Level? SelectableLevel => Level.IvesFinale;
            public override bool UnlockCriteriaMet() => 
                LevelID <= GameStateManager.Instance.CurrentLevelProgress && 
                BountyManager.Instance.GetBountyProgress() >= 6 && 
                GameStateManager.SEASON_1_ACTIVE;
            public override string UnlockRequirementsText() => true switch
            {
                !GameStateManager.SEASON_1_ACTIVE => "Coming Soon!",
                _ when BountyManager.Instance.GetBountyProgress() < 6 => "Complete all bounties to unlock.",
                _ => $"Complete previous levels to unlock."
            };
        }

        public class Season2 : StageInformation
        {
            public override string Title => string.Empty;
            public override string SceneName => SceneData.Get<SceneData.PrincessFrogBounty>().SceneName;
            public override float LevelID => 6f;
            public override bool LevelEnabled => false;
            public override bool UnlockCriteriaMet() => LevelID <= GameStateManager.Instance.CurrentLevelProgress && Get<IvesFinale>().UnlockCriteriaMet();
        }

        public void UponSelectedEvent() => new StageInformationEvent(SceneName).Invoke();
        public static StageInformation Get<T>() where T : StageInformation => ParseFromType(typeof(T));
        public static IEnumerable<StageInformation> Stages => Values;
    }

    public record BountyInformationEvent(BountyInformation BountyType) : IEvent;

    // Represents the information needed to load a specific bounty stage during level select
    public abstract class BountyInformation : Enum<BountyInformation>, ILevelSelectInformation
    {
        public abstract IEnumerable<IBounties> BountyCollection { get; }
        public abstract float LevelID { get; }
        public virtual Level? SelectableLevel => null;
        public virtual string Title => string.Empty;
        public virtual bool LevelEnabled => true;
        public virtual bool UnlockCriteriaMet() => LevelID <= GameStateManager.Instance.CurrentLevelProgress;
        public virtual string UnlockRequirementsText() => $"Complete previous levels to unlock.";
        public class PrincessFrogBounty : BountyInformation
        {
            public override IEnumerable<IBounties> BountyCollection => PrincessFrogBounties.Values;
            public override float LevelID => 4.5f;
            public override Level? SelectableLevel => Level.PrincessFrogBounty;
            public override bool LevelEnabled => GameStateManager.SEASON_1_ACTIVE;
            public override string Title => "BOUNTY BOARD";
            public override bool UnlockCriteriaMet() => LevelID <= GameStateManager.Instance.CurrentLevelProgress && GameStateManager.SEASON_1_ACTIVE;
            public override string UnlockRequirementsText() => true switch
            {
                !GameStateManager.SEASON_1_ACTIVE => "Coming Soon!",
                _ => $"Complete previous levels to unlock."
            };
        }
        public void UponSelectedEvent() => new BountyInformationEvent(this).Invoke();

        public static BountyInformation Get<T>() where T : BountyInformation => ParseFromType(typeof(T));
        public static IEnumerable<BountyInformation> Bounties => Values;
    }
}
