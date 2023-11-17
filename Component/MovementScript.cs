using GameNetcodeStuff;
using UnityEngine;

namespace MovementCompany.Component {
    internal class MovementScript : MonoBehaviour {
        public PlayerControllerB player;

        public static Vector3 wantedVelToAdd;

        private Vector3 previousForward;

        public float jumpTime;

        private static readonly float JUMP_TIME_MULTIPLIER = 9.5f;
        private static readonly float GROUND_VELOCITY_MULTIPLIER = 4.2f;
        private static readonly float AIR_VELOCITY_MULTIPLIER = 0.004f;
        private static readonly float ROTATION_THRESHOLD = 0.015f;

        private static readonly Vector3 newWantedVel = new(0.0005f, 0.0005f, 0.0005f);

        private bool inAir;

        public void Update() {
            UpdateJumpTime();

            bool grounded = player.thisController.isGrounded;
            bool climbing = player.isClimbingLadder;

            if (grounded || climbing) {
                HandleGrounded();
                return;
            }

            SetStamina();
            ApplyBhop();
        }

        private void UpdateJumpTime() {
            if (player.playerBodyAnimator.GetBool("Jumping") && jumpTime < 0.1f) {
                player.fallValue = player.jumpForce;

                jumpTime += Time.deltaTime * JUMP_TIME_MULTIPLIER;
            }
        }

        private void SetStamina(float val = 50f) {
            // Prevents potential weird behaviours.
            if (val <= 0f) {
                val = 0f;
            }
            
            player.sprintMeter = val;
        }

        private void HandleGrounded() {
            float targetVel = Time.deltaTime * GROUND_VELOCITY_MULTIPLIER;
            wantedVelToAdd = Vector3.Lerp(wantedVelToAdd, Vector3.zero, targetVel);

            inAir = false;
            jumpTime = 0;
        }

        private void ApplyBhop() {
            if (!inAir) {
                inAir = true;
                SetVelocityInAir(AIR_VELOCITY_MULTIPLIER);
            }

            wantedVelToAdd.y = 0;
            player.thisController.Move(CurrentForward() * wantedVelToAdd.magnitude);

            Vector3 forwardChange = CurrentForward() - previousForward;
            if (forwardChange.magnitude > ROTATION_THRESHOLD) {
                wantedVelToAdd += newWantedVel;
            }

            previousForward = CurrentForward();
        }

        private void SetVelocityInAir(float multiplier) {
            Vector3 vel = player.thisController.velocity;
            vel.y = 0;

            wantedVelToAdd += vel * multiplier;
        }

        private Vector3 CurrentForward() {
            return player.gameObject.transform.forward;
        }
    }
}
