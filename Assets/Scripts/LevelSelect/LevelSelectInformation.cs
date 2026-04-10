using BountySystem;
using System.Collections.Generic;
using System.Linq;

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
        float LevelID { get; }
        void UponSelectedEvent();
        Level? SelectableLevel { get; }

        static readonly Dictionary<Level, ILevelSelectInformation> LEVEL_INFORMATION;
        static ILevelSelectInformation()
        {
            LEVEL_INFORMATION = StageInformation.AllStages.Cast<ILevelSelectInformation>()
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

        public class Tutorial : StageInformation
        {
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
            public override string SceneName => SceneData.Get<SceneData.FrogSlimeFight>().SceneName;
            public override float LevelID => 1f;
            public override Level? SelectableLevel => Level.FrogSlimeFight;
        }

        public class Beetle : StageInformation
        {
            public override string SceneName => SceneData.Get<SceneData.BeetleFight>().SceneName;
            public override float LevelID => 2f;
            public override Level? SelectableLevel => Level.BeetleFight;
        }

        public class QueenPreparation : StageInformation
        {
            public override string SceneName => SceneData.Get<SceneData.SelectionScreen>().SceneName;
            public override float LevelID => 2.5f;
        }

        public class QueenBeetle : StageInformation
        {
            public override string SceneName => SceneData.Get<SceneData.PreQueenFight>().SceneName;
            public override float LevelID => 3f;
            public override Level? SelectableLevel => Level.QueenFight;
        }

        public class PrincessFrogFight : StageInformation
        {
            public override string SceneName => SceneData.Get<SceneData.PrincessFrogBounty>().SceneName;
            public override float LevelID => 4f;
            public override Level? SelectableLevel => Level.PrincessFrogFight;
        }

        public class IvesFinale : StageInformation
        {
            public override string SceneName => SceneData.Get<SceneData.PrincessFrogBounty>().SceneName;
            public override float LevelID => 5f;
            public override Level? SelectableLevel => Level.IvesFinale;
        }
        public void UponSelectedEvent() => new StageInformationEvent(SceneName).Invoke();
        public static StageInformation Get<T>() where T : StageInformation => ParseFromType(typeof(T));
        public static IEnumerable<StageInformation> AllStages => Values;
    }

    public record BountyInformationEvent(BountyInformation bountyType) : IEvent;

    // Represents the information needed to load a specific bounty stage during level select
    public abstract class BountyInformation : Enum<BountyInformation>, ILevelSelectInformation
    {
        public abstract IEnumerable<IBounties> BountyCollection { get; }
        public abstract float LevelID { get; }
        public virtual Level? SelectableLevel => null;

        public class PrincessFrogBounty : BountyInformation
        {
            public override IEnumerable<IBounties> BountyCollection => PrincessFrogBounties.Values;
            public override float LevelID => 4.5f;
            public override Level? SelectableLevel => Level.PrincessFrogBounty;
        }
        public void UponSelectedEvent() => new BountyInformationEvent(this).Invoke();

        public static BountyInformation Get<T>() where T : BountyInformation => ParseFromType(typeof(T));
        public static IEnumerable<BountyInformation> Bounties => Values;
    }
}
