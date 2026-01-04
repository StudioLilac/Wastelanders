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
        description = "Inflict 2 wound on the target.";
    }

    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();
        Target.AddStacks(Wound.buffName, 2);
    }

    public override void OnCardStagger()
    {
        base.OnCardStagger();
        Target.AddStacks(Wound.buffName, 2);
    }
}
