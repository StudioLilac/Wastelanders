using UnityEngine;

public class FocusedDefense : StaffCards
{
    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 4;

        Speed = 5;


        myName = "Focused Defense";
        description = "Gain 1 Focus, If this card is unstaggerd, Gain another Focus";
        Renderer renderer = GetComponent<Renderer>();
        base.Initialize();
        CardType = CardType.Defense;
    }

    public override void CardIsUnstaggered()
    {
        Origin.AddStacks(Flow.buffName, 1);
    }
    public override void ApplyEffect()
    {
        Origin.AddStacks(Flow.buffName, 1);
        base.ApplyEffect();
    }
}
