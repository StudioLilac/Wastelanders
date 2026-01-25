
using UnityEngine;


namespace Steamworks {

    public class AchievementManager : PersistentSingleton<AchievementManager> {
#if STEAMWORKS_NET
        private int enemiesKilled = 0;

        protected override void Awake() {
            base.Awake(); // Handles singleton instance + DontDestroyOnLoad
            
            players = FindObjectsOfType<PlayerClass>();

            if (!SteamManager.Initialized) return;

            // Load current kill count from Steam stats on start
            SteamUserStats.GetStat("KILL_COUNT", out enemiesKilled);

            // In case player already reached milestones before starting the game, sync achievements
            CheckKillAchievements();
        }

        private void OnEnable() {
            EntityClass.OnEntityDeath += HandleEntityDeath;
        }

        private void OnDisable() {
            EntityClass.OnEntityDeath -= HandleEntityDeath;
        }

        private void HandleEntityDeath(EntityClass entity) {
            if (entity is EnemyClass) {
                // blacklist certain types of enemies here.
                if (entity is TrainingDummy) {
                    return;
                }

                if (entity is QueenBeetle) {
                    SteamManager.UnlockAchievement("DEFEAT_QUEEN");
                    return;
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

            // Add more milestones here
            // if (enemiesKilled >= 10) SteamManager.UnlockAchievement("HUNTER");
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

        private void OnPlayersWin() {
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