using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilClass;

public class IronSights : PistolCards
{

    public override void Initialize()
    {
        lowerBound = 1;
        upperBound = 4;
	    Speed = 4;

        CardType = CardType.RangedAttack;
        myName = "Iron Sights";
        description = "Gain one Accuracy, then attack.";
        base.Initialize();
    }

    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Accuracy };


    public override void ApplyEffect()
    {
        Origin.AddStacks(Accuracy.buffName, 1);
        base.ApplyEffect();
    }

}
