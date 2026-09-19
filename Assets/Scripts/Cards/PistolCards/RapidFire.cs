using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UtilClass;

public class RapidFire : PistolCards
{
    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 1;
        upperBound = 4;
        Speed = 2;
        description = "Attack, then continuously consumes 1 Accuracy to use Rapid Fire again. On kill, gain 1 accuracy.";
        CardType = CardType.RangedAttack;
        myName = "Rapid Fire";
        base.Initialize();
    }

    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Accuracy };


    public override void OnHit()
    {
        base.OnHit();
        if (Target.IsDead)
        {
            Origin.AddStacks(Accuracy.buffName, 1);
        }
    }

    public override void OnCardStagger()
    {
        base.OnCardStagger();
        if (Origin.GetBuffStacks(Accuracy.buffName) > 0)
        {
            Origin.ReduceStacks(Accuracy.buffName, 1);
            BattleQueue.BattleQueueInstance.AddAction(this);
        }
    }

    public override void CardIsUnstaggered()
    {
        base.CardIsUnstaggered();
        if (Origin.GetBuffStacks(Accuracy.buffName) > 0)
        {
            Origin.ReduceStacks(Accuracy.buffName, 1);
            BattleQueue.BattleQueueInstance.AddAction(this);
        }
    }
}