public class SharpenedDefence : AxeCards
{
    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 2;
        upperBound = 4;
        Speed = 3;
        CardType = CardType.Defense;    
        myName = "Razor Guard";
        description = "Inflict wound equal to damage blocked.";
    }

    public override void OnDefendClash(ActionClass opposingCard)
    {
        int blockedDamage = Mathf.Min(opposingCard.GetRolledStats().ActualRoll, GetRolledStats().ActualRoll);
        Target.AddStacks(Wound.buffName, blockedDamage);
        base.OnDefendClash(opposingCard);
    }
}
