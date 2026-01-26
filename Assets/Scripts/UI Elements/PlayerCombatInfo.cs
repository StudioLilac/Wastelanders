using System.Collections;
using UnityEngine;

namespace UI_Elements {
    public class PlayerCombatInfo : CombatInfo {
        [SerializeField] private Animator animator;
        private const float SPEED_LERP_DURATION = 0.5f;

        private Coroutine speedRoutine;

        public override void Update() {}

        public override void ActivateCrosshair(float speed = 1) {
            base.ActivateCrosshair(speed);

            float targetSpeed = Mathf.Min(speed, animator.speed);

            if (targetSpeed < animator.speed) {
                StartSpeedLerp(targetSpeed);
            }
        }

        public override void DeactivateCrosshair() {
            base.DeactivateCrosshair();

            StopSpeedLerp();
            animator.speed = 1f;
        }

        private void StartSpeedLerp(float targetSpeed) {
            StopSpeedLerp();
            speedRoutine = StartCoroutine(LerpAnimatorSpeed(animator.speed, targetSpeed));
        }

        private void StopSpeedLerp() {
            if (speedRoutine != null) {
                StopCoroutine(speedRoutine);
                speedRoutine = null;
            }
        }

        private IEnumerator LerpAnimatorSpeed(float start, float target) {
            float elapsed = 0f;

            while (elapsed < SPEED_LERP_DURATION) {
                elapsed += Time.deltaTime;
                float t = elapsed / SPEED_LERP_DURATION;
                animator.speed = Mathf.Lerp(start, target, t);
                yield return null;
            }

            animator.speed = target;
            speedRoutine = null;
        }
    }
}