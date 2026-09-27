using System;
using System.Collections.Generic;
using System.Linq;
using BountySystem;
using Cards.EnemyCards.FrogCards;
using Entities;
using UnityEngine;

namespace SceneBuilder
{
    public class PrincessFrogCombatSceneBuilder : SceneBuilder
    {
        [SerializeField] private Vector3 playersPosition;
        [SerializeField] private Vector3 enemiesPosition;

        [SerializeField] private Jackie jackiePrefab;
        [SerializeField] private Ives ivesPrefab;

        [SerializeField] private PrincessFrog princessFrogPrefab;
        [SerializeField] private QueenBeetle queenBeetlePrefab;
        [SerializeField] private Beetle[] beetlePrefabs;
        [SerializeField] private WasteFrog frogPrefab;
        [SerializeField] private SlimeStack slimePrefab;

        [SerializeField] private GameObject entityContainer;
        public bool instakill;

#nullable enable
        private IBounties? bounty = null;
        private readonly List<EnemyClass> currentMinions = new();

        protected override void Build()
        {
            bounty = BountyManager.Instance.ActiveBounty;
            
            SpawnAll(DeterminePlayers(), EntityTeam.PlayerTeam, playersPosition);
            SpawnAll(DetermineEnemies(), EntityTeam.EnemyTeam, enemiesPosition);
        }

        public void OnEnable()
        {
            EntityClass.OnEntitySpawn += HandleEntityChange;
            EntityClass.OnEntityDeath += HandleEntityChange;
        }

        public void OnDisable()
        {
            EntityClass.OnEntitySpawn -= HandleEntityChange;
            EntityClass.OnEntityDeath -= HandleEntityChange;
        }
        private List<GameObject> DetermineBurpSpawnable()
        {
            List<GameObject> list = new();

            if (bounty?.ContractSet.Contains(EnemySpawningContracts.FROG_SPAWN) == true)
            {
                list.Add(frogPrefab.gameObject);
            } 
            else if (bounty?.ContractSet.Contains(EnemySpawningContracts.SLIME_SPAWN) == true)
            {
                list.Add(slimePrefab.gameObject);
            }
            else
            {
                list.AddRange(beetlePrefabs.Select(beetle => beetle.gameObject));
            }

            return list;
        }

        private BlessBuffTarget DetermineBlessTarget() => bounty?.ContractSet switch {
                null => BlessBuffTarget.Resonate,
                var set when set.Contains(EnemySpawningContracts.FROG_SPAWN) => BlessBuffTarget.Flow,
                var set when set.Contains(EnemySpawningContracts.SLIME_SPAWN) => BlessBuffTarget.Accuracy,
                _ => BlessBuffTarget.Resonate
        };

        private GameObject[] DeterminePlayers()
        {
            List<GameObject> list = new();

            if (bounty?.ContractSet.Contains(PlayerContracts.SOLO_JACKIE) == true)
            {
                list.Add(jackiePrefab.gameObject);
            } 
            else
            {
                list.Add(jackiePrefab.gameObject);
                list.Add(ivesPrefab.gameObject);
            }

            return list.ToArray();
        }

        private GameObject[] DetermineEnemies()
        {
            List<GameObject> list = new();

            if (bounty?.ContractSet.Contains(EnemySpawningContracts.FROG_SPAWN) == true)
            {
                list.Add(frogPrefab.gameObject);
                list.Add(frogPrefab.gameObject);
            }
            else if (bounty?.ContractSet.Contains(EnemySpawningContracts.SLIME_SPAWN) == true)
            {
                list.Add(slimePrefab.gameObject);
                list.Add(slimePrefab.gameObject);
            }
            else if (bounty?.ContractSet.Contains(EnemySpawningContracts.QUEEN_BEETLE_SPAWN) == true)
            {
                list.Add(queenBeetlePrefab.gameObject);
            }
            else
            {
                list.AddRange(beetlePrefabs.Select(beetle => beetle.gameObject));
            }

            list.Add(princessFrogPrefab.gameObject);

            return list.ToArray();
        }

        //These adjustments are made one frame before Start runs on princess frog.
        private void AdjustPrincessFrog(PrincessFrog princessFrog)
        {
            princessFrog.EncounterBlessTarget = DetermineBlessTarget();
            if (bounty == null) return;

            if (bounty.ContractSet.Contains(PrincessFrogContracts.ADDITIONAL_ATTACK))
            {
                princessFrog.NumberOfAttacks = 3;
            }

            if (bounty.ContractSet.Contains(PrincessFrogContracts.EXTRA_HEALTH))
                princessFrog.StartingHealth = 75;

            if (bounty.ContractSet.Contains(PrincessFrogContracts.AGGRESIVE_AI))
            {
                princessFrog.AttackDecider =
                    (int currentEnemyCount) =>
                        UnityEngine.Random.Range(0f, 1f) > currentEnemyCount switch
                        {
                            5 => 1f,
                            4 => 0.7f,
                            3 => 0.5f,
                            2 => 0.2f,
                            1 => 0f,
                            0 => 0f,
                            _ => 1.0f
                        };
            }

            if (bounty.ContractSet.Contains(PrincessFrogContracts.EXTRA_RESONANCE))
            {
                princessFrog.AddStacks(Resonate.buffName, 1);
            }
        }

        private void AdjustPlayerClass(PlayerClass playerClass)
        {
            if (bounty?.ContractSet.Contains(PlayerContracts.DECREASED_HAND_SIZE) == true)
            {
                playerClass.maxHandSize = 3;
            } else if (instakill)
            {
                playerClass.AddStacks(Accuracy.buffName, 900);
                playerClass.AddStacks(Resonate.buffName, 900);
            }
        }

        private void AdjustEnemyClass(EnemyClass enemyClass)
        {
            enemyClass.TargetingWeights = delegate(EntityClass entity)
            {
                return entity.Team switch
                {
                    EntityTeam.PlayerTeam => 100,
                    EntityTeam.NeutralTeam => 20,
                    _ => 0
                };
            };
        }

        private void SpawnAll(GameObject[] prefabs, EntityTeam team, Vector3 position)
        {
            var positions = PositionsFrom(position, team, prefabs.Length);
            for (var i = 0; i < prefabs.Length; i++)
            {
                Spawn(prefabs[i], positions[i]);
            }
        }

        private void Spawn(GameObject prefab, Vector3 position)
        {
            var spawn = Instantiate(prefab, position, Quaternion.identity, entityContainer.transform);
            var entity = spawn.GetComponent<EntityClass>();

            if (entity is PrincessFrog princessFrog)
            {
                princessFrog.OwnedMinions = currentMinions;
                AdjustPrincessFrog(princessFrog);
            } 
            else if (entity is QueenBeetle queen)
            {
                queen.IntializeChildBeetles(new());
                SetupStaggerOverride(queen);
            }
            else if (entity is EnemyClass enemyClass)
            {
                AdjustEnemyClass(enemyClass);
                SetupStaggerOverride(enemyClass);
            }
            else if (entity is PlayerClass playerClass)
            {
                AdjustPlayerClass(playerClass);
            }
        }

        private void SetupStaggerOverride(EnemyClass minion)
        {
            currentMinions.Add(minion);
            minion.DeathHandler = minion.PassOut;
        }

        private void UpdateEnemyLayout()
        {
            List<EntityClass> spawns = new GetTeammates(EntityTeam.EnemyTeam).Query() ?? new();

            var positions = PositionsFrom(enemiesPosition, EntityTeam.EnemyTeam, spawns.Count);
            for (var i = 0; i < spawns.Count; i++)
            {
                spawns[i].SetReturnPosition(entityContainer.transform.position + positions[i]);
            }
        }

        private void UpdatePlayerLayout()
        {
            List<EntityClass> players = new GetTeammates(EntityTeam.PlayerTeam).Query() ?? new();


            var positions = PositionsFrom(playersPosition, EntityTeam.PlayerTeam, players.Count);
            for (var i = 0; i < players.Count; i++)
            {
                players[i].SetReturnPosition(entityContainer.transform.position + positions[i]);
            }
        }

        private void HandleEntityChange(EntityClass entity)
        {
            UpdatePlayerLayout();
            if (ContractExists(EnemySpawningContracts.QUEEN_BEETLE_SPAWN)) UpdateEnemyLayout();
        }

        private Vector3[] PositionsFrom(Vector2 centerCoordinate, EntityTeam team, int count)
        {
            var dx = -1f * Mathf.Sign(centerCoordinate.x);
            var dy = DetermineDY(team, count);

            var height = (count - 1) * dy;
            var top = centerCoordinate.y + height / 2f;

            var positions = new Vector3[count];
            for (var i = 0; i < count; i++)
            {
                positions[i] = new Vector3(centerCoordinate.x + (i % 2 == 0 ? 1f : -1f) * dx, top - i * dy);
            }

            return positions;
        }

        float DetermineDY(EntityTeam team, int count) => team switch {
            EntityTeam.EnemyTeam when ContractExists(EnemySpawningContracts.QUEEN_BEETLE_SPAWN) && count == 2 => 2.5f,
            EntityTeam.EnemyTeam when ContractExists(EnemySpawningContracts.QUEEN_BEETLE_SPAWN) => 1.4f,
            EntityTeam.PlayerTeam => 1.1f,
            _ => 1f,
        };

        private bool ContractExists(IContracts contract)
        {
            return bounty?.ContractSet.Contains(contract) == true;
        }
    }
}