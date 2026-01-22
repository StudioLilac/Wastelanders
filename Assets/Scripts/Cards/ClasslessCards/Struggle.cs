using System.Collections;
using System.Collections.Generic;
using UnityEngine;

<<<<<<<< HEAD:Assets/Scripts/Cards/FistCards/LowBlow.cs
public class LowBlow : FistCards
========
public class Struggle : ClasslessCards
>>>>>>>> c8807af1 (Initial impl of struggle card):Assets/Scripts/Cards/ClasslessCards/Struggle.cs
{
    public override void Initialize()
    {
        lowerBound = 3;
        upperBound = 3;
        Speed = 5;
        clashable = false;

        myName = "Low Blow";
        description = "This attack doesn't clash with enemy actions.";
        CardType = CardType.MeleeAttack;
        Renderer renderer = GetComponent<Renderer>();
        base.Initialize();
    }

}
