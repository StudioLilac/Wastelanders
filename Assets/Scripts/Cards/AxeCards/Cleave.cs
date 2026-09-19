using UtilClass;

public class Cleave : AxeCards
{
    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 6;
        Speed = 2;

        CostToAddToDeck = IsEvolved ? 1 : 2;

        myName = "Cleave";
        description = "On hit, apply 2 stacks of wound to the target.";
        evolutionCriteria = "Apply 10+ stacks of wound.";
        evolutionDescription = "Reduce cost to 1.";
        MaxEvolutionProgress = 10;

        base.Initialize();
        CardType = CardType.MeleeAttack;
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Wound, Keywords.OnHit };

    public override void OnHit()
    {
        base.OnHit();
        CurrentEvolutionProgress += 2;
        Target.AddStacks(Wound.buffName, 2);
    }
}
