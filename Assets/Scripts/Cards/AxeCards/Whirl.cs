using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UtilClass;

public class Whirl : AxeCards
{
#nullable enable
    Whirl? activeDuplicateInstance = null;
    bool originalCopy = true;

    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 4;
        Speed = 2;

        myName = "Whirl";
        description = "Apply 1 wound on hit. If not staggered, make this attack once again.";
        evolutionCriteria = "Make this attack 10 times.";
        evolutionDescription = "Cost increased to 3. Make this attack three times, applying 1 wound on hit.";
        MaxEvolutionProgress = 10;
        CardType = CardType.MeleeAttack;
        base.Initialize();
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Wound, Keywords.OnHit };

    // for the "deals damage" portion how will you know? there is no reference to the enemy nor the outcome of a clash. 
    public override void CardIsUnstaggered()
    {
        if (originalCopy)
        {
            if (activeDuplicateInstance == null)
            {
                activeDuplicateInstance = Instantiate(this.GetComponent<Whirl>());
                activeDuplicateInstance.originalCopy = false;
                activeDuplicateInstance.transform.position = new Vector3(-10, 10, 10);
            }
            activeDuplicateInstance.Origin = Origin;
            activeDuplicateInstance.Target = Target;
            BattleQueue.BattleQueueInstance.AddAction(activeDuplicateInstance!);
        }

        base.CardIsUnstaggered();
    }

    public override void OnHit()
    {
        base.OnHit();
        Target.AddStacks(Wound.buffName, 1);
    }

}
