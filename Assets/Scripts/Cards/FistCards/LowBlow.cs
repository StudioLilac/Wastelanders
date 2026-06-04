using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowBlow : FistCards
{
    public override void Initialize()
    {
        lowerBound = 3;
        upperBound = 3;
        Speed = 4;
        Clashable = false;

        myName = "Low Blow";
        description = "This attack doesn't clash with enemy actions.";
        CardType = CardType.MeleeAttack;
        base.Initialize();
    }

}
