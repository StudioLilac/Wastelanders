
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Steamworks {

    public class AchievementManager : PersistentSingleton<AchievementManager> {
#if STEAMWORKS_NET
        private int enemiesKilled = 0;

        // boolean flag to track whether one player has died in the current combat.
        // reset whenever a scene changes.
        private bool onePlayerDead = false;

        private List<EntityClass> players = new();

        protected override void Awake() {
            base.Awake();

            if (!SteamManager.Initialized) return;

            SteamUserStats.GetStat("KILL_COUNT", out enemiesKilled);

            CheckKillAchievements();
            
            this.Subscribe<PlayersWin>(OnPlayersWin);
            this.Subscribe<GameStateChanged>(HandleGameStateChanged);
        }

        private void OnEnable() {
            EntityClass.OnEntityDeath += HandleEntityDeath;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDisable() {
            EntityClass.OnEntityDeath -= HandleEntityDeath;
            SceneManager.activeSceneChanged -= OnSceneChanged;
        }

        private void HandleGameStateChanged(GameStateChanged stateChanged) {
            if (stateChanged.next == GameState.SELECTION) {
                players = CombatManager.Instance.GetPlayers();
                
                foreach (EntityClass player in players) {
                    player.BuffsUpdatedEvent -= HandlePlayerBuffsUpdated;
                    player.BuffsUpdatedEvent += HandlePlayerBuffsUpdated;
                }
            } else if (stateChanged.next != GameState.FIGHTING) {
                foreach (EntityClass player in players) {
                    player.BuffsUpdatedEvent -= HandlePlayerBuffsUpdated;
                }
                players.Clear();
            }
        }

        private void HandleEntityDeath(EntityClass entity) {
            if (entity is EnemyClass) {
                // blacklist certain types of enemies here.
                if (entity is TrainingDummy) {
                    return;
                }

                if (entity is QueenBeetle) {
                    SteamManager.UnlockAchievement("DEFEAT_QUEEN");
                }
                enemiesKilled++;

                SteamManager.UpdateStat("KILL_COUNT", enemiesKilled);

                CheckKillAchievements();
            } else if (entity is PlayerClass) {
                onePlayerDead = true;
            }
        }

        private void CheckKillAchievements() {
            if (enemiesKilled >= 1) {
                SteamManager.UnlockAchievement("FIRST_BLOOD");
            }

            if (enemiesKilled >= 5) {
                SteamManager.UnlockAchievement("WARMING_UP");
            }

            if (enemiesKilled >= 15) {
                SteamManager.UnlockAchievement("KILLING_SPREE");
            }
        }

        public void HandlePlayerHitCritical() {
            SteamManager.UnlockAchievement("CRITICAL");
        }
        
        public void HandlePlayerFinishedSparring() {
            SteamManager.UnlockAchievement("DEFEAT_IVES");
        }

        private void OnSceneChanged(Scene arg0, Scene arg1) {
            onePlayerDead = false;
        }

        private void OnPlayersWin(PlayersWin playersWinEvent) {
            if (onePlayerDead) {
                SteamManager.UnlockAchievement("REVENGE");
            }
        }

        private void HandlePlayerBuffsUpdated(EntityClass player) {
            if (player is not PlayerClass) return;

            if (player.GetBuffStacks(Resonate.buffName) >= 5) {
                SteamManager.UnlockAchievement("THE_RESONATOR");
            }
        }
        
#endif // STEAMWORKS_NET
    }
}