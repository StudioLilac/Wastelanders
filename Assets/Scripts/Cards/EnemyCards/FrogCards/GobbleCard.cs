using Entities;
using UnityEngine;

namespace Cards.EnemyCards.FrogCards
{
    public class GobbleCard : ActionClass, IPlayablePrincessFrogCard
    {
        [SerializeField] public AnimationClip animationClip;
        public override void Initialize()
        {
            base.Initialize();

            myName = "Gobble";
            description = "If attacking a crystal: Instantly destroy it.";

            lowerBound = upperBound = 1;
            CostToAddToDeck = 1;
            Speed = 4;
            CardType = CardType.MeleeAttack;
        }

        public override void CardIsUnstaggered()
        {
            IPlayableEnemyCard.ApplyForeignAttackAnimation(Origin, animationClip, FrogAttacks.PRINCESS_FROG_ATTACK_NAME);
            Origin.AttackAnimation(FrogAttacks.PRINCESS_FROG_ATTACK_NAME);
        }

        public override void OnHit()
        {
            AudioManager.Instance.PlaySFX(SoundID.CB_excavate);

            if (Target is Crystals) rolledCardStats.ActualRoll = Target.Health;
            base.OnHit();
        }
    }
}