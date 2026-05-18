using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilClass;

public class Vitals : PistolCards
{
    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 1;
        upperBound = 7;

        Speed = 4;
        
        myName = "Vitals";
        description = "On kill, gain 1 accuracy.";
        CardType = CardType.RangedAttack;
        base.Initialize();
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Accuracy };

    public override void OnHit()
    {
        base.OnHit();
        if (Target.IsDead)
        {
            Origin.AddStacks(Accuracy.buffName, 1);
        }
    }

}
