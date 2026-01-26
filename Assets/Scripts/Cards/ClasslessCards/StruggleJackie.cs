using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StruggleJackie : ClasslessCards
{
    public override void Initialize()
    {
        lowerBound = 1;
        upperBound = 1;
        Speed = 1;

        myName = "Struggle";
        description = "You flail about as you are exhausted...";
        CardType = CardType.MeleeAttack;
        base.Initialize();
    }

}
