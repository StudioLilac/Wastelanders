using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Struggle : ClasslessCards
{
    public override void Initialize()
    {
        lowerBound = 1;
        upperBound = 1;
        Speed = 1;
        Clashable = false;

        myName = "Struggle";
        description = "You flail about as you are exhausted...";
        CardType = CardType.MeleeAttack;
        base.Initialize();
    }

}
