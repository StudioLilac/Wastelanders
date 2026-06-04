using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilClass;

public class Execute : AxeCards
{
    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 6;
        Speed = 1;
        CostToAddToDeck = IsEvolved ? 3 : 2;

        myName = "Execute";
        description = "On hit, deal an extra +1 damage for each wound on the target.";
        evolutionCriteria = "Deal 10+ damage in one strike.";
        evolutionDescription = "Increase cost to 3. Each wound on the enemy increases this card's final power by 1.";
        MaxEvolutionProgress = 10;
        base.Initialize();
        CardType = CardType.MeleeAttack;
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Wound, Keywords.OnHit };

    
    public override void OnHit()
    {
        IncrementRoll(Target.GetBuffStacks(Wound.buffName));
        base.OnHit();
        CurrentEvolutionProgress = Mathf.Max(getRolledDamage(), CurrentEvolutionProgress);
    }
}
