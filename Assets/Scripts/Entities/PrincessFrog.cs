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
        public BlessBuffTarget EncounterBlessTarget { get; set; } = BlessBuffTarget.Resonate;
        public List<GameObject> BlessCards { get; private set; } = new();
        public List<GameObject> HurlCards { get; private set; } = new();
        public List<GameObject> BurpCards { get; private set; } = new();
        public List<GameObject> GobbleCards { get; private set; } = new();
        public List<EnemyClass> OwnedMinions { get; set; } = new();

        public delegate bool AttackDeciderDelegate(int opponentCount);


        public override void Start()
        {
            base.Start();

            myName = "Princess Frog";
            Health = MaxHealth = StartingHealth;
            AddStacks(Resonate.buffName, 6);
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

        private List<EnemyClass> GetStaggeredMinions()
        {
            return OwnedMinions.Where(m => m.IsDead).ToList();
        }

        public override void AddAttack(List<EntityClass> targets)
        {
            var blessPlayedThisTurn = 0;
            var opponents = targets.Where(entity => entity.Team == EntityTeam.PlayerTeam).ToList();
            var neutral = targets.Where(entity => entity.Team == EntityTeam.NeutralTeam).ToList();
            var hurtTeammates = OwnedMinions.Where(entity => entity.Health < entity.MaxHealth && !entity.IsDead).ToList();
            var aliveTeammates = OwnedMinions.Where(entity => !entity.IsDead).ToList();

            List <EnemyClass> availableDeadMinions = GetStaggeredMinions();
            int activeMinionCount = OwnedMinions.Count - availableDeadMinions.Count + 1;
            int gobblePotentialStacks = 0;
            bool shouldBarf = false;
            bool shouldGobble = true;

            for (int i = 0; i < NumberOfAttacks; i++)
            {
                bool shouldPlayBurp = availableDeadMinions.Count > 0;
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
                    case >= 4:
                        if (shouldPlayBurp) AttackWith(BurpCards[i], burpTarget);
                        else if (blessPlayedThisTurn < NumberOfAttacks - 1)
                        {
                            AttackWith(BlessCards[i], CalculateAttackTarget(opponents));
                            blessPlayedThisTurn++;
                        }
                        else if (hurtTeammates.Count > 0)
                        {
                            EntityClass hurtTarget = hurtTeammates[Random.Range(0, hurtTeammates.Count)];
                            AttackWith(BurpCards[i], hurtTarget);
                        } else
                        {
                            AttackWith(BlessCards[i], CalculateAttackTarget(opponents));
                            blessPlayedThisTurn++;
                        }
                        break;
                    case var _ when shouldGobble && neutral.Count > 0 && (gobblePotentialStacks + currentStacks) < 4:
                        AttackWith(GobbleCards[i], CalculateAttackTarget(neutral));
                        gobblePotentialStacks += 3; //Pretends gobble succeeds and makes furthur decisions from there.
                        shouldGobble = false; // Gobble only once per turn. 
                        new CardUsed<GobbleCard>().Invoke();
                        break;
                    case >= 1:
                        if (shouldPlayBurp) AttackWith(BurpCards[i], burpTarget);
                        else if (blessPlayedThisTurn < NumberOfAttacks - 1)
                        {
                            AttackWith(BlessCards[i], CalculateAttackTarget(opponents));
                            blessPlayedThisTurn++;
                        }
                        else if (hurtTeammates.Count > 0)
                        {
                            EntityClass hurtTarget = hurtTeammates[Random.Range(0, hurtTeammates.Count)];
                            AttackWith(BurpCards[i], hurtTarget);
                        }
                        else if (aliveTeammates.Count > 0)
                        {
                            EntityClass aliveTarget = aliveTeammates[Random.Range(0, aliveTeammates.Count)];
                            AttackWith(BurpCards[i], aliveTarget);
                        }
                        else
                        {
                            AttackWith(BlessCards[i], CalculateAttackTarget(opponents));
                            blessPlayedThisTurn++;
                        }
                        break;
                    default:
                        if (shouldBarf) AttackWith(HurlCards[i], CalculateAttackTarget(opponents));
                        shouldBarf = true; // Only barf if you fall through to default twice. Otherwise, loose one attack. 
                        break;
                }

            }
        }
    }
}