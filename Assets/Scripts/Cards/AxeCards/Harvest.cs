using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UtilClass;

public class Harvest : AxeCards
{
    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 6;
        Speed = 1;

        myName = "Harvest";
        description = "On hit, consume all wound stacks and heal that much.";
        evolutionCriteria = "Heal 10.";
        evolutionDescription = "Wound isn't consumed on-hit.";
        MaxEvolutionProgress = 10;

        base.Initialize();
        CardType = CardType.MeleeAttack;
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Wound, Keywords.OnHit };

    public override void OnHit()
    {
        base.OnHit();
        int healAmount = Target.GetBuffStacks(Wound.buffName);
        Origin.Heal(healAmount);
        Target.ReduceStacks(Wound.buffName, healAmount);
        CurrentEvolutionProgress += healAmount;
    }
}
