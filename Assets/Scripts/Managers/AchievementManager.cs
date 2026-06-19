
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Steamworks {

    public record OnFinishSparring() : IEvent;
    public record OnPlayerHit(int Damage) : IEvent;

    public class AchievementManager : PersistentSingleton<AchievementManager> {
#if STEAMWORKS_NET
        private int enemiesKilled = 0;

        // boolean flag to track whether one player has died in the current combat.
        // reset whenever a scene changes.
        private bool onePlayerDead = false;

        protected override void Awake() {
            base.Awake();

            if (!SteamManager.Initialized) return;

            SteamUserStats.GetStat("KILL_COUNT", out enemiesKilled);

            CheckKillAchievements();
            
            this.Subscribe<TeamWinEvent>(OnPlayersWin);
            this.Subscribe<OnPlayerHit>(HandlePlayerHitCritical);
            this.Subscribe<OnFinishSparring>(HandlePlayerFinishedSparring);
            this.Subscribe<OnBuffsUpdatedEvent>(HandleOnBuffsUpdated);
        }

        private void OnEnable() {
            EntityClass.OnEntityDeath += HandleEntityDeath;
            SceneManager.activeSceneChanged += OnSceneChanged;
        }

        private void OnDisable() {
            EntityClass.OnEntityDeath -= HandleEntityDeath;
            SceneManager.activeSceneChanged -= OnSceneChanged;
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

        private void HandlePlayerHitCritical(OnPlayerHit playerHitEvent) {
            if (playerHitEvent.Damage >= 10) {
                SteamManager.UnlockAchievement("CRITICAL");
            }
        }
        
        private void HandlePlayerFinishedSparring(OnFinishSparring sparringEvent) {
            SteamManager.UnlockAchievement("DEFEAT_IVES");
        }

        private void OnSceneChanged(Scene arg0, Scene arg1) {
            onePlayerDead = false;
        }

        private void OnPlayersWin(TeamWinEvent teamWinEvent) {
            if (teamWinEvent.Team == EntityTeam.PlayerTeam && onePlayerDead) {
                SteamManager.UnlockAchievement("REVENGE");
            }
        }

        private void HandleOnBuffsUpdated(OnBuffsUpdatedEvent buffsUpdatedEvent) {
            if (buffsUpdatedEvent.WhoAmI is not PlayerClass) return;

            if (buffsUpdatedEvent.WhoAmI.GetBuffStacks(Resonate.buffName) >= 5) {
                SteamManager.UnlockAchievement("THE_RESONATOR");
            }
        }
        
#endif // STEAMWORKS_NET
    }
}