using UnityEngine;

public class ChargeUpPlayable : ActionClass, IPlayableFrogCard
{
    [SerializeField]
    private GameObject hurlPrefab;
    [SerializeField] private AnimationClip animationClip;

    public override void Initialize()
    {
        base.Initialize();
        lowerBound = 1;
        upperBound = 3;
        Speed = 1;
        CostToAddToDeck = 2;

        description = "Block, if not staggered, use 'Hurl' next turn";

        myName = "Charge Up";
        CardType = CardType.Defense;
    }

    public override void CardIsUnstaggered()
    {
        PlayerClass player = (PlayerClass)this.Origin;
        player.InjectCard(hurlPrefab);
        IPlayableEnemyCard.ApplyForeignAttackAnimation(player, animationClip, ChargeUp.CHARGE_UP_ANIMATION_NAME);
        Origin.AttackAnimation(ChargeUp.CHARGE_UP_ANIMATION_NAME);
    }
}
