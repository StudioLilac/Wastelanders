using System;
using UI_Toolkit;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Managers {
    public class TimeManager : PersistentSingleton<TimeManager> {
        private const float DEFAULT_TIME_SCALE = 1f;
        private bool isPaused;
        private bool isDoubleSpeed;

        private float timeScale;

        private float lastUnpausedTimeScale = 1f;
        
        public bool IsDoubleSpeed => isDoubleSpeed;

        protected override void Awake() {
            base.Awake();
            
            Time.timeScale = DEFAULT_TIME_SCALE;
            this.Subscribe<PauseStateChangedEvent>(HandlePauseStateChanged);
            this.Subscribe<DoubleSpeedChangedEvent>(HandleDoubleSpeedChanged);
            this.Subscribe<GameStateChanged>(HandleGameStateChanged);
            SceneManager.sceneLoaded += HandleSceneLoaded;
        }

        private void HandlePauseStateChanged(PauseStateChangedEvent e) {
            isPaused = e.paused;
            if (e.paused) {
                lastUnpausedTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            } else {
                Time.timeScale = lastUnpausedTimeScale;
            }
        }

        private void HandleDoubleSpeedChanged(DoubleSpeedChangedEvent e) {
            isDoubleSpeed = e.enabled;
            float targetScale = DEFAULT_TIME_SCALE * (e.enabled ? 2f : 1f);
    
            if (isPaused) {
                lastUnpausedTimeScale = targetScale;
            } else {
                Time.timeScale = targetScale;
            }
        }
        private void HandleGameStateChanged(GameStateChanged e) {
            if (isDoubleSpeed) {
                bool doubleSpeedApplies = e.NewState == GameState.FIGHTING || e.NewState == GameState.SELECTION;
                float targetScale = DEFAULT_TIME_SCALE * (doubleSpeedApplies ? 2f : 1f);

                if (isPaused) {
                    lastUnpausedTimeScale = targetScale;
                } else {
                    Time.timeScale = targetScale;
                }
            }
        }
        
        // I set the speed back to normal here because if we are in double speed mode, we don't get a GameStateEvent to reset us properly.
        private void HandleSceneLoaded(Scene scene, LoadSceneMode mode) {
            lastUnpausedTimeScale = DEFAULT_TIME_SCALE;
            Time.timeScale = DEFAULT_TIME_SCALE;
        }

        private void OnDestroy() {
            SceneManager.sceneLoaded -= HandleSceneLoaded;
        }
    }
    
    
}