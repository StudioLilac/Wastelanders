using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using static Unity.Collections.AllocatorManager;
using static UnityEngine.UI.Image;

public class CounterStrike : StaffCards
{
#nullable enable
    CounterStrike? activeDuplicateInstance = null;
    private bool originalCopy = true;
    public override void Initialize()
    {
        lowerBound = 2;
        upperBound = 4;
        Speed = 2;

        myName = "Counter Strike";
        description = "Block and gain Flow equal to damage blocked. Then make an attack and gain 1 Flow on hit.";
        base.Initialize();
    }

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
            CardType = CardType.Defense;
            if (activeDuplicateInstance == null)
            {
                activeDuplicateInstance = Instantiate(this.GetComponent<CounterStrike>());
                activeDuplicateInstance.originalCopy = false;
                activeDuplicateInstance.transform.position = new Vector3(-10, 10, 10);
                activeDuplicateInstance.CardType = CardType.MeleeAttack;
            }
            activeDuplicateInstance.Origin = Origin;
            activeDuplicateInstance.Target = Target;
            BattleQueue.BattleQueueInstance.AddAction(activeDuplicateInstance!);
        }
    }

    public override void OnDefendClash(ActionClass opposingCard)
    {
        int blockedDamage = Mathf.Min(opposingCard.GetRolledStats().ActualRoll, GetRolledStats().ActualRoll);
        Origin.AddStacks(Flow.buffName, blockedDamage);
        base.OnDefendClash(opposingCard);
    }

    public override void OnHit()
    {
        base.OnHit(); 
        if (CardType == CardType.MeleeAttack)
        {
            Origin.AddStacks(Flow.buffName, 1);
        }
    }
}
