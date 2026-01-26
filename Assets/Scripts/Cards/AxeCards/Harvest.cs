using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Harvest : AxeCards
{
    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 6;
        Speed = 1;

        myName = "Harvest";
        description = "On hit, heal +1 for each wound stack on the target.";
        evolutionCriteria = "Heal 10.";
        evolutionDescription = "Wound isn't consumed on-hit.";
        MaxEvolutionProgress = 10;

        base.Initialize();
        CardType = CardType.MeleeAttack;
    }

    public override void OnHit()
    {
        base.OnHit();
        int healAmount = Target.GetBuffStacks(Wound.buffName);
        Origin.Heal(healAmount);
        CurrentEvolutionProgress += healAmount;
    }
}
