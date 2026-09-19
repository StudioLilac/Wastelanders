using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UtilClass;

// REQUIRES TESTING TODO (Haven't done this yet owing to the dearth of STAFF user presumably Ives.
public class Flurry : StaffCards
{
#nullable enable
    Flurry? activeDuplicateInstance = null;
    bool originalCopy = true;

    // Start is called before the first frame update
    public override void Initialize()
    {
        
        CardType = CardType.MeleeAttack;
        myName = "Flurry";
        description = "Make this attack once, then make it again. Each attack on hit grants 1 Flow.";
        lowerBound = 2;
        upperBound = 4;
        Speed = 3;

        base.Initialize();
    }
    
    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Flow, Keywords.OnHit };

    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();
        InsertDuplicate();
    }

    public override void OnCardStagger()
    {
        base.OnCardStagger();
        InsertDuplicate();
    }

    private void InsertDuplicate()
    {
        if (originalCopy)
        {
            if (activeDuplicateInstance == null)
            {
                activeDuplicateInstance = Instantiate(this.GetComponent<Flurry>());
                activeDuplicateInstance.originalCopy = false;
                activeDuplicateInstance.transform.position = new Vector3(-10, 10, 10);
            }
            activeDuplicateInstance.Origin = Origin;
            activeDuplicateInstance.Target = Target;
            BattleQueue.BattleQueueInstance.AddAction(activeDuplicateInstance!);
        }
    }

    public override void OnHit()
    {
        base.OnHit();
        Origin.AddStacks(Flow.buffName, 1);
    }
}