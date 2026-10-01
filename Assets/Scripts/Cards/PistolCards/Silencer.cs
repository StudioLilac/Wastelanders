using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilClass;

public class Silencer : PistolCards
{

    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 1;
        upperBound = 4;
        Speed = 3;
        description = "If not staggered, gain 2 Accuracy stacks.";
        myName = "Silencer";
        CardType = CardType.RangedAttack;
        base.Initialize();
    }

    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Accuracy, };

    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();
        Origin.AddStacks(Accuracy.buffName, 2);
    }
}
