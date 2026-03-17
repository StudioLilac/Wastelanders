using Cards.EnemyCards.FrogCards;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Entities
{
    public class PrincessFrog : EnemyClass
    {
        public int StartingHealth { get; set; } = 50;
        public int NumberOfAttacks { get; set; } = 2;
        public BlessBuffTarget EncounterBlessTarget { get; set; } = BlessBuffTarget.Random;
        public List<GameObject> BlessCards { get; private set; } = new();
        public List<GameObject> HurlCards { get; private set; } = new();
        public List<GameObject> BurpCards { get; private set; } = new();
        public List<GameObject> GobbleCards { get; private set; } = new();
        public List<EnemyClass> OwnedMinions { get; set; } = new();
        public AttackDeciderDelegate AttackDecider { private get; set; } = 
            (int enemyCount) => 
                Random.Range(0f, 1f) > enemyCount switch
                {
                    4 => 1f,
                    3 => 0.66f,
                    2 => 0.33f,
                    1 => 0f,
                    0 => 0f,
                    _ => 1.0f
                };

        public delegate bool AttackDeciderDelegate(int opponentCount);

        public override void Start()
        {
            base.Start();

            myName = "Princess Frog";
            Health = MaxHealth = StartingHealth;
            AddStacks(Resonate.buffName, 7);
        }

        public override void InstantiateDeck()
        {
            var actionMapping = new Dictionary<int, List<GameObject>>
            {
                { 0, BlessCards },
                { 1, BurpCards },
                { 2, GobbleCards },
                { 3, HurlCards }
            };

            for (int i = 0; i < availableActions.Count; i++)
            {
                for (int j = 0; j < NumberOfAttacks; ++j)
                {
                    GameObject toAdd = Instantiate(availableActions[i]);
                    ActionClass addedClass = toAdd.GetComponent<ActionClass>();
                    addedClass.Origin = this;
                    if (addedClass is BlessCard blessCard)
                    {
                        blessCard.TargetBuff = EncounterBlessTarget;
                        blessCard.Initialize();
                    }

                    if (actionMapping.TryGetValue(i, out var targetList))
                    {
                        targetList.Add(toAdd);
                    }
                }
            }

        }

        protected override void OnEnable()
        {
            base.OnEnable();

            EntityTookDamage += HandleDamage;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            EntityTookDamage -= HandleDamage;
        }

        private List<EnemyClass> GetStaggeredMinions()
        {
            return OwnedMinions.Where(m => m.IsDead).ToList();
        }

        public override void AddAttack(List<EntityClass> targets)
        {
            var opponents = targets.Where(entity => entity.Team == EntityTeam.PlayerTeam).ToList();
            var neutral = targets.Where(entity => entity.Team == EntityTeam.NeutralTeam).ToList();

            List<EnemyClass> availableDeadMinions = GetStaggeredMinions();
            int activeMinionCount = OwnedMinions.Count - availableDeadMinions.Count + 1;
            int gobblePotentialStacks = 0;

            for (int i = 0; i < NumberOfAttacks; i++)
            {
                bool canBurp = availableDeadMinions.Count > 0;
                bool shouldPlayBurp = canBurp && AttackDecider(activeMinionCount);
                int currentStacks = GetBuffStacks(Resonate.buffName);

                EntityClass burpTarget = null;
                if (shouldPlayBurp)
                {
                    int targetIndex = Random.Range(0, availableDeadMinions.Count);
                    burpTarget = availableDeadMinions[targetIndex];
                    availableDeadMinions.RemoveAt(targetIndex);
                    activeMinionCount++;
                }

                switch (currentStacks)
                {
                    case >= 7:
                    case >= 2 and <= 6:
                        if (shouldPlayBurp) AttackWith(BurpCards[i], burpTarget);
                        else                AttackWith(BlessCards[i], CalculateAttackTarget(opponents));
                        break;
                    case var _ when neutral.Count > 0 && (gobblePotentialStacks + currentStacks) <= 6:
                        AttackWith(GobbleCards[i], CalculateAttackTarget(neutral));
                        gobblePotentialStacks += 3; //Pretends gobble succeeds and makes furthur decisions from there. 
                        break;
                    case 1:
                        AttackWith(BlessCards[i], CalculateAttackTarget(neutral));
                        break;
                    case 0:
                        AttackWith(HurlCards[i], CalculateAttackTarget(neutral));
                        break;
                    default:
                        AttackWith(HurlCards[i], CalculateAttackTarget(neutral));
                        break;
                }

            }
        }


       

        private void HandleDamage(int amount)
        {
            if (amount == 0) return;

            /* Lose stacks when taking (non-zero) damage. Commented out for balance for now*/
            //ReduceStacks(Resonate.buffName, 1);
        }
    }
}