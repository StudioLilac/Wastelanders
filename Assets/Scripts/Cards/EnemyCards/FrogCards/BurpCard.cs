using Entities;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cards.EnemyCards.FrogCards
{
    public class BurpCard : FrogAttacks, IPlayablePrincessFrogCard
    {
        public const int BURP_COST = 2;
        public override void Initialize()
        {
            base.Initialize();

            myName = "Burp";
            description = $"Spend +{BURP_COST} Resonance to play. On ally hit, refund resonance and heal ally rolled power + resonance stacks instead.";

            CostToAddToDeck = 2;
            lowerBound = upperBound = 1;
            Speed = 4;
            CardType = CardType.RangedAttack;
            frogAttackAnimationName = PRINCESS_FROG_ATTACK_NAME;
        }

        public override void OnQueue()
        {
            Origin.ReduceStacks(Resonate.buffName, BURP_COST);
        }

        public override void OnRetrieveFromQueue()
        {
            Origin.AddStacks(Resonate.buffName, BURP_COST);
        }

        public override bool IsPlayableByPlayer(out PopupType popupType)
        {
            bool isPlayable = base.IsPlayableByPlayer(out popupType);
            bool enoughStacks = Origin.GetBuffStacks(Resonate.buffName) >= BURP_COST;

            popupType = enoughStacks ? popupType : new PopupType.InsufficientResources(Origin.GetBuffStacks(Resonate.buffName), BURP_COST);

            return isPlayable && enoughStacks;
        }

        protected override void OnProjectileHit()
        {
            if (Target.Team == Origin.Team)
            {
                AudioManager.Instance.PlaySFX(SoundID.CB_frog_hit);

                Origin.AddStacks(Resonate.buffName, BURP_COST);
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