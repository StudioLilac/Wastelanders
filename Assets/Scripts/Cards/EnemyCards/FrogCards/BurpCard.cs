using UtilClass;

namespace Cards.EnemyCards.FrogCards
{
    public class BurpCard : FrogAttacks, IPlayablePrincessFrogCard
    {
        public const int BURP_COST = 1;
        private bool NoCost => (Origin == null || Origin.Team != EntityTeam.EnemyTeam) && BountyManager.Instance.IsBountyCompleted(PrincessFrogBounties.PRINCESS_FROG_CHALLENGE);
        public override void Initialize()
        {
            base.Initialize();

            myName = "Burp";
            description = NoCost ? "On monster ally hit, heal ally (rolled power + resonance stacks)." : 
                $"Spend {BURP_COST} Resonance to play. On monster ally hit, heal ally (rolled power + resonance stacks) and refund resonance spent.";

            CostToAddToDeck = 2;
            lowerBound = upperBound = 1;
            Speed = 2;
            CardType = CardType.RangedAttack;
            frogAttackAnimationName = PRINCESS_FROG_ATTACK_NAME;
        }
        
        protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Resonance };

        public override void OnQueue()
        {
            if (!NoCost) Origin.ReduceStacks(Resonate.buffName, BURP_COST);
        }

        public override void OnRetrieveFromQueue()
        {
            if (!NoCost) Origin.AddStacks(Resonate.buffName, BURP_COST);
        }

        public override bool IsPlayableByPlayer(out PopupType popupType)
        {
            bool isPlayable = base.IsPlayableByPlayer(out popupType);
            bool enoughStacks = Origin.GetBuffStacks(Resonate.buffName) >= BURP_COST || NoCost;

            popupType = enoughStacks ? popupType : new PopupType.InsufficientResources(Origin.GetBuffStacks(Resonate.buffName), BURP_COST);

            return isPlayable && enoughStacks;
        }

        protected override void OnProjectileHit()
        {
            if (Target.Team == Origin.Team)
            {
                AudioManager.Instance.PlaySFX(SoundID.CB_frog_hit);

                if (!NoCost) Origin.AddStacks(Resonate.buffName, BURP_COST);
                if (Target.IsDead) Target.Revive();  
                Target.Heal(rolledCardStats.ActualRoll + Origin.GetBuffStacks(Resonate.buffName)); 
            }
            else
            {
                base.OnProjectileHit();
            }
        }
    }
}