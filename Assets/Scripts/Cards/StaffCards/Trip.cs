using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UtilClass;

public class Trip : StaffCards
{
    int stacksConsumed = 0;
    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 4;

        Speed = 3;
        myName = "Sweep";
        description = "Attack, then gain Flow equal to Flow consumed.";
        CardType = CardType.MeleeAttack;
        base.Initialize();
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Flow };

    public override void ApplyEffect()
    {
        stacksConsumed = Origin.GetBuffStacks(Flow.buffName);
        base.ApplyEffect();
    }

    public override void OnCardStagger()
    {
        base.OnCardStagger();
        if (stacksConsumed > 0)
        {
            Origin.AddStacks(Flow.buffName, stacksConsumed);
            stacksConsumed = 0;
        }
    }

    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();
        if (stacksConsumed > 0)
        {
            Origin.AddStacks(Flow.buffName, stacksConsumed);
            stacksConsumed = 0;
        }
    }
}