using System.Runtime.CompilerServices;
using UnityEngine;

namespace UI_Elements {
    public class PlayerCombatInfo : CombatInfo {
        [SerializeField] private Animator animator;

        public override void Update() {}

        public override void ActivateCrosshair(float speed = 1) {
            base.ActivateCrosshair(speed);
            animator.speed = speed;
        }

        public override void DeactivateCrosshair() {
            base.DeactivateCrosshair();
            animator.speed = 1;
        }
    }
}