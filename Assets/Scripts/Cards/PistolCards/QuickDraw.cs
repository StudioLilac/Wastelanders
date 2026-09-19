using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilClass;

public class QuickDraw : PistolCards
{
    public override void OnCardStagger()
    {
        base.OnCardStagger();
        Origin.AddStacks(Accuracy.buffName, 1);
    }

    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();
        Origin.AddStacks(Accuracy.buffName, 1);
    }

    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 1; // MAKE SURE BOUNDS ARE SET BEFORE CALLING SUPERCLASS START
        upperBound = 4;
        Speed = 5;
        myName = "Quick Draw";
        description = "Make this attack, then gain 1 Accuracy even if staggered.";
        CardType = CardType.RangedAttack;
        base.Initialize();
    }

    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Accuracy };

}
