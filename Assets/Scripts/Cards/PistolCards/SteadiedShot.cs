using UnityEngine;
using UtilClass;
using static StatusEffect;

public class SteadiedShot : PistolCards
{
    // Start is called before the first frame update
    public override void Initialize()
    {
        lowerBound = 3;
        upperBound = 3;
        Speed = 5;

        myName = "Steadied Shot";
        description = "Block, then do not lose Accuracy when you get hit this round.";
        base.Initialize();
        CardType = CardType.Defense;
    }
    

    protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Accuracy };

    public override void CardIsUnstaggered()
    {
        Origin.AttackAnimation(EntityClass.BLOCK_ANIMATION_NAME); // Override Block Animations
    }

    public override void ApplyEffect()
    {
        Origin.AddStacks(Steadied.buffName, 1);
        base.ApplyEffect();
    }
}
