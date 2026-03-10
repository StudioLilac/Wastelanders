using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barf : FrogAttacks, IPlayablePrincessFrogCard
{
    // Start is called before the first frame update
    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 1;
        upperBound = 7;

        Speed = 4;

        CostToAddToDeck = 2;

        myName = "Barf";
        description = "On kill, gain 2 Resonance.";
        frogAttackAnimationName = PRINCESS_FROG_ATTACK_NAME;
    }

    public override void OnHit()
    {
        base.OnHit();
        if (Target.IsDead)
        {
            Origin.AddStacks(Resonate.buffName, 2);
        }
    }
}

