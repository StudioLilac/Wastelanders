using UnityEngine;
using Random = UnityEngine.Random;

namespace Cards.EnemyCards.FrogCards
{
    public class BlessCard : ActionClass, IPlayablePrincessFrogCard
    {
        public const int BLESS_COST = 1;
        public const string BLESS_ANIMATION = "IsBlessing";
        [SerializeField] private AnimationClip animationClip;
        public override void Initialize()
        {
            base.Initialize();

            myName = "Bless";
            description =
                $"Spend +{BLESS_COST} Resonance to play. If not staggered: Gain 1 Resonance and give all teammates a random positive buff.";

            CostToAddToDeck = 2;
            lowerBound = upperBound = 1;
            Speed = 1;
            CardType = CardType.Defense;
        }

        public override void OnQueue()
        {
            Origin.ReduceStacks(Resonate.buffName, BLESS_COST);
        }

        public override void OnRetrieveFromQueue()
        {
            Origin.AddStacks(Resonate.buffName, BLESS_COST);
        }

        public override bool IsPlayableByPlayer(out PopupType popupType)
        {
            bool isPlayable = base.IsPlayableByPlayer(out popupType);
            bool enoughStacks = Origin.GetBuffStacks(Resonate.buffName) >= BLESS_COST;

            popupType = enoughStacks ? popupType : new PopupType.InsufficientResources(Origin.GetBuffStacks(Resonate.buffName), BLESS_COST);

            return isPlayable && enoughStacks;
        }

        public override void CardIsUnstaggered()
        {
            IPlayableEnemyCard.ApplyForeignAttackAnimation(Origin, animationClip, BLESS_ANIMATION);
            Origin.AttackAnimation(BLESS_ANIMATION);
            Origin.AddStacks(Resonate.buffName, BLESS_COST);

            var teamMates = Origin.Team.GetTeamMates();
            var buffs = new[] { Accuracy.buffName, Flow.buffName, Resonate.buffName };
            foreach (var enemy in teamMates)
            {
                enemy.AddStacks(buffs[Random.Range(0, buffs.Length)], 1);
            }
        }
    }
}