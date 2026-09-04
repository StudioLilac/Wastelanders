using UtilClass;

namespace Cards.EnemyCards.FrogCards
{
    public class Supercharge : FrogAttacks
    {
        public override void Initialize()
        {
            base.Initialize();

            myName = "Supercharge";
            description = "On hit, grant the target 4 flow.";

            CostToAddToDeck = 2;
            lowerBound = upperBound = 4;
            Speed = 5;
            CardType = CardType.RangedAttack;
            frogAttackAnimationName = PRINCESS_FROG_ATTACK_NAME;
        }
        
        protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Flow };
        
        protected override void OnProjectileHit()
        {
            AudioManager.Instance.PlaySFX(SoundID.CB_frog_hit);
            Target.AddStacks(Flow.buffName, 4);
        }
    }
}