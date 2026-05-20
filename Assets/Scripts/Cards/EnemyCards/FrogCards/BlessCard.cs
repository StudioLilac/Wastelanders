using UnityEngine;
using UtilClass;

namespace Cards.EnemyCards.FrogCards
{
    public class BlessCard : ActionClass, IPlayablePrincessFrogCard
    {
        public const int BLESS_COST = 1;
        public const string BLESS_ANIMATION = "IsBlessing";

        [SerializeField] private AnimationClip animationClip;

        public BlessBuffTarget TargetBuff { get; set; } = BlessBuffTarget.Resonate;

        public override void Initialize()
        {
            base.Initialize();
            myName = $"Bless";
            description = $"Spend {BLESS_COST} Resonance to play. If not staggered, give all teammates 1 {TargetBuff.GetBuffDescription()}.";

            CostToAddToDeck = 2;
            lowerBound = upperBound = 1;
            Speed = 1;
            CardType = CardType.Defense;
        }
        
        protected override GlossaryNode[] GetChildrenGlossaryNodes() => new[] { StatusEffects.Resonance };

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

            var teamMates = Origin.Team.GetTeamMates();
            string buffToApply = TargetBuff.GetBuff();

            foreach (var enemy in teamMates)
            {
                enemy.AddStacks(buffToApply, 1);
            }
        }
    }

    public enum BlessBuffTarget
    {
        Accuracy,
        Flow,
        Resonate,
        Random,
    }

    public static class BlessCardExtensions
    {
        public static string GetBuffDescription(this BlessBuffTarget target)
        {
            return target switch
            {
                BlessBuffTarget.Accuracy => "Accuracy",
                BlessBuffTarget.Flow => "Flow",
                BlessBuffTarget.Resonate => "Resonance",
                BlessBuffTarget.Random => "a random buff (Accuracy, Flow, or Resonance)",
                _ => string.Empty
            };
        }

        public static string GetBuff(this BlessBuffTarget target) => target switch
        {
            BlessBuffTarget.Accuracy => Accuracy.buffName,
            BlessBuffTarget.Flow => Flow.buffName,
            BlessBuffTarget.Resonate => Resonate.buffName,
            BlessBuffTarget.Random => new[] { Accuracy.buffName, Flow.buffName, Resonate.buffName }[Random.Range(0, 3)],
            _ => string.Empty
        };        
    }
}