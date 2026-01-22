using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Struggle : ClasslessCards
{
    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 1;
        upperBound = 1;
        Speed = 1;

        myName = "Struggle";
        description = "You flail about as you are exhausted...";
        CardType = CardType.MeleeAttack;
        Renderer renderer = GetComponent<Renderer>();
        base.Initialize();
    }

}
