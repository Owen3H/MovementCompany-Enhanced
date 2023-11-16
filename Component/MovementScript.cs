using GameNetcodeStuff;
using UnityEngine;

namespace MovementCompany.Component {
    internal class MovementScript : MonoBehaviour {
        public PlayerControllerB myPlayer;

        public static Vector3 wantedVelToAdd;

        private Vector3 previousForward;

        public float jumpTime;

        private static readonly float JUMP_TIME_MULTIPLIER = 10f;

        private static readonly float GROUND_VELOCITY_MULTIPLIER = 4.2f;
        private static readonly float AIR_VELOCITY_MULTIPLIER = 0.006f;

        private static readonly float ROTATION_THRESHOLD = 0.01f;
        private static readonly Vector3 newWantedVel = new(0.0005f, 0.0005f, 0.0005f);

        private bool inAir;

        public void Update() {
            UpdateJumpTime();
            RefillSprintMeter();

            bool grounded = myPlayer.thisController.isGrounded;
            bool climbing = myPlayer.isClimbingLadder;

            if (grounded || climbing) {
                HandleGrounded();
                return;
            }

            ApplyBhop();
        }

        private void UpdateJumpTime() {
            if (myPlayer.playerBodyAnimator.GetBool("Jumping") && jumpTime < 0.1f) {
                myPlayer.fallValue = myPlayer.jumpForce;

                // TODO: Evaluate if this can be replaced with fixedDeltaTime.
                jumpTime += Time.deltaTime * JUMP_TIME_MULTIPLIER;
            }
        }

        private void RefillSprintMeter(int value = 100) {
            myPlayer.sprintMeter = value;
        }

        private void HandleGrounded() {
            // TODO: Evaluate if this can be replaced with fixedDeltaTime.
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
            myPlayer.thisController.Move(CurrentForward() * wantedVelToAdd.magnitude);

            Vector3 forwardChange = CurrentForward() - previousForward;

            if (forwardChange.magnitude > ROTATION_THRESHOLD)
                wantedVelToAdd += newWantedVel;

            previousForward = CurrentForward();
        }

        private void SetVelocityInAir(float multiplier) {
            Vector3 vel = myPlayer.thisController.velocity;
            vel.y = 0;

            wantedVelToAdd += vel * multiplier;
        }

        private Vector3 CurrentForward() {
            return myPlayer.gameObject.transform.forward;
        }
    }
}
