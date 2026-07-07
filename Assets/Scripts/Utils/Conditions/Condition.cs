using LevelSelectInformation;
using Systems.Persistence;
using static Condition;

/// Evil ahh enum that represents a conditional for the sake of composibility. 
public enum Condition 
{
    None = 0,
    BountyIsZero = 1,
    PrincessFrogUnlocked = 2,
}

public static class ConditionExtensions
{
    public static bool IsMet(this Condition condition)
    {
        return condition switch
        {
            None => true,
            BountyIsZero => new GetBountyStateData().Query()?.GetNumCompletedBounties() == 0,
            PrincessFrogUnlocked => StageInformation.Get<StageInformation.PrincessFrogFight>().UnlockCriteriaMet(),
            _ => false

        };
    }
}