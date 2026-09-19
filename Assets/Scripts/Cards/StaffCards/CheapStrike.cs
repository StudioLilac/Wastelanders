using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilClass;

public class CheapStrike : StaffCards
{

    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = IsEvolved ? 3 : 1;
        upperBound = IsEvolved ? 3 : 1;
        CostToAddToDeck = IsEvolved ? 3 : 2;
        Speed = 1;


        myName = "Cheap Strike";
        description = "On hit, gain 3 Flow.";
        evolutionCriteria = "Generate 10+ flow with this card";
        evolutionDescription = "3 cost. This card now has 3 - 3 power, gain 3 flow on hit.";
        MaxEvolutionProgress = 10;
        CardType = CardType.MeleeAttack;
        base.Initialize();
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Flow, Keywords.OnHit };

    public override void OnHit()
    {
        base.OnHit();
        Origin.AddStacks(Flow.buffName, 3);
        CurrentEvolutionProgress += 3;
    }
}
