using UnityEngine;

public class Brace : FistCards
{

#nullable enable
    Brace? activeDuplicateInstance = null;
    bool originalCopy = true;

    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 3;
        upperBound = 3;
        Speed = 5;

        myName = "Brace";
        description = "Block, then block the next one sided attack currently targeting this character.";
        CardType = CardType.Defense;
        base.Initialize();
    }

    public override void CardIsUnstaggered()
    {
        Origin.AttackAnimation(EntityClass.BLOCK_ANIMATION_NAME);
        BlockNextAttack();
    }

    public override void OnCardStagger()
    {
        base.OnCardStagger();
        BlockNextAttack();
    }

    // Note there is an edge case where a new enemy card is created only after you lose a future clash and since this only checks the queue at the time of instantiation.
    private void BlockNextAttack()
    {
        if (originalCopy)
        {
            if (activeDuplicateInstance == null)
            {
                activeDuplicateInstance = Instantiate(this.GetComponent<Brace>());
                activeDuplicateInstance.originalCopy = false;
                activeDuplicateInstance.transform.position = new Vector3(-10, 10, 10);
            }
            var target = FindUnclashedAttackOrigin();
            if (target == null) return;
            activeDuplicateInstance.Origin = Origin;
            activeDuplicateInstance.Target = target;
            BattleQueue.BattleQueueInstance.AddAction(activeDuplicateInstance);
        }
    }

    private EntityClass? FindUnclashedAttackOrigin()
    {
        var array = BattleQueue.BattleQueueInstance.ProvideArray();

        foreach (var item in array)
        {
            if (!item.IsClashing())
            {
                var action = item.GetTheOnlyExistingAction();
                if (action.Target == Origin)
                {
                    return action.Origin;
                }
            }
        }

        return null;
    }

}
