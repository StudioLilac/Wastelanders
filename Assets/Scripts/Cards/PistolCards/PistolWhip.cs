using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilClass;

public class PistolWhip : PistolCards
{

    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 3;
        upperBound = 3;
        Speed = 1;
        description = "On kill, gain 1 Accuracy stack";
        myName = "Pistol Whip";
        base.Initialize();
        CardType = CardType.MeleeAttack; //Has to be after otherwise it will get overwritten by superclass
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
