using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cards.EnemyCards.FrogCards
{
    public class BurpCard : FrogAttacks, IPlayablePrincessFrogCard
    {
        [SerializeField] public List<GameObject> SerializedSpawnableEnemies = new();
        public const int BURP_COST = 2;
        public override void Initialize()
        {
            base.Initialize();

            myName = "Burp";
            description = $"Spend +{BURP_COST} Resonance to play. On Hit: Gain 1 Resonance and spawn a random monster.";

            CostToAddToDeck = 2;
            lowerBound = upperBound = 1;
            Speed = 2;
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

            popupType = enoughStacks ? popupType : PopupType.InsufficientResources;

            return isPlayable && enoughStacks;
        }

        protected override void OnProjectileHit()
        {
            base.OnProjectileHit();
            Origin.AddStacks(Resonate.buffName, 1);

            var projectileDirection = Vector3.down + (Origin.IsFacingRight() ? Vector3.left : Vector3.right);
            var position = Target.transform.position + projectileDirection;

            var prefab = SerializedSpawnableEnemies[Random.Range(0, SerializedSpawnableEnemies.Count)];
            var parent = Origin.transform.parent;
            var spawn = Instantiate(prefab, position, Quaternion.identity, parent);
            var entity = spawn.GetComponent<EntityClass>();
            entity.Team = Origin.Team;
        }
    }
}