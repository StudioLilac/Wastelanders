using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UtilClass;

public class Decimate : AxeCards
{
    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 6;
        Speed = IsEvolved ? 4 : 1;

        myName = "Decimate";
        description = "On hit, double the amount of wounds on the target.";
        evolutionCriteria = "Double 10 wounds.";
        evolutionDescription = "Change this card's speed to 4.";
        MaxEvolutionProgress = 10;

        base.Initialize();
        CardType = CardType.MeleeAttack;
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Wound, Keywords.OnHit };

    public override void OnHit()
    {
        base.OnHit();
        CurrentEvolutionProgress += Target.GetBuffStacks(Wound.buffName);
        Target.AddStacks(Wound.buffName, Target.GetBuffStacks(Wound.buffName));
    }
}
