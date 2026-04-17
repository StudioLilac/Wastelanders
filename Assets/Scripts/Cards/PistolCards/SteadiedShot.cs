using UnityEngine;
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

    public override void CardIsUnstaggered()
    {
        Origin.AttackAnimation(EntityClass.BLOCK_ANIMATION_NAME); // Override Block Animations
    }

    public override void ApplyEffect()
    {
        StatusEffectModifyValueDelegate originalHandler = Origin.SetBuffsOnHitHandler(Accuracy.buffName, (ref int damage) => { });
        this.Subscribe<GameStateChanged>(ResetBuffHandler);
        void ResetBuffHandler(GameStateChanged e)
        {
            if (e.NewState != GameState.FIGHTING)
            {
                this.UnSubscribe<GameStateChanged>(ResetBuffHandler);
                Origin.SetBuffsOnHitHandler(Accuracy.buffName, originalHandler);
            }
        }
        base.ApplyEffect();
    }
}
