using GameNetcodeStuff;
using MovementCompany.Core;
using System;
using UnityEngine;

namespace MovementCompany.Component {
    internal class MovementScript : MonoBehaviour {
        public PlayerControllerB player;

        private PluginConfig cfg;

        public static Vector3 wantedVelToAdd;

        private static readonly Vector3 velToAdd = new(0.0002f, 0.0002f, 0.0002f);

        private Vector3 previousForward;

        public float jumpTime;

        private bool inAir;

        void OnGUI() {
            if (!cfg.DISPLAY_DEBUG_INFO) return;

            Vector3 pos = player.thisController.transform.position;
            GUI.Label(new Rect(10, 10, 500, 500), Vec3ToString(pos));

            GUI.Label(new Rect(10, 40, 500, 500), "Current Velocity: " + Math.Round(CurrentVelocity(), 3));
            GUI.Label(new Rect(10, 60, 500, 500), "Wanted Velocity: " + Math.Round(wantedVelToAdd.magnitude, 3));
            GUI.Label(new Rect(10, 80, 500, 500), "Reached Max Velocity: " + ReachedMaxVelocity());

            GUI.Label(new Rect(10, 120, 500, 500), "Airborne: " + Airborne());
            GUI.Label(new Rect(10, 140, 500, 500), "Jump Time: " + jumpTime);
        }

        public void Start() {
            cfg = Plugin.Config;
            ApplyConfigSpeeds();
        }

        public void Update() {
            UpdateJumpTime();

            // Allows infinite bhopping by keeping the "sprint meter" full.
            if (!cfg.BHOP_CONSUMES_STAMINA) {
                SetStamina(cfg.MAX_STAMINA);
            }

            // No longer in air, slowly decrease velocity.
            if (!Airborne()) {
                LerpToGround();
                return;
            }

            if (ReachedMaxVelocity()) return;
            ApplyBhop();
        }

        public void ApplyConfigSpeeds() {
            player.movementSpeed = ValNonNegative(cfg.MOVEMENT_SPEED);
            player.climbSpeed = ValNonNegative(cfg.CLIMB_SPEED);
            player.sinkingSpeedMultiplier = ValNonNegative(cfg.SINK_SPEED_MULTIPLIER);
        }

        public void UpdateJumpTime() {
            bool jumping = player.playerBodyAnimator.GetBool("Jumping");
            if (jumping && jumpTime < cfg.MAX_JUMP_DURATION) {
                player.fallValue = player.jumpForce;
                jumpTime += Time.deltaTime / 100 * cfg.JUMP_TIME_MULTIPLIER;
            }
        }

        public void LerpToGround() {
            float targetVel = Time.deltaTime * cfg.GROUND_VELOCITY_MULTIPLIER;
            wantedVelToAdd = Vector3.Lerp(wantedVelToAdd, Vector3.zero, targetVel);

            inAir = false;
            jumpTime = 0;
        }

        public void ApplyBhop() {
            if (!inAir) {
                inAir = true;
                AddJumpVelocity(cfg.AIR_VELOCITY_MULTIPLIER);
            }

            wantedVelToAdd.y = 0;
            player.thisController.Move(CurrentForward() * (wantedVelToAdd.magnitude / cfg.FORWARD_VELOCITY_DAMPER));

            AddRotationVelocity(cfg.ROTATION_THRESHOLD);
        }

        public void AddJumpVelocity(float multiplier) {
            Vector3 vel = player.thisController.velocity;
            vel.y = 0;

            wantedVelToAdd += vel * multiplier;
        }

        public void AddRotationVelocity(float threshold) {
            Vector3 fwdDiff = CurrentForward() - previousForward;
            bool applyVelocity = fwdDiff.magnitude > threshold;

            if (applyVelocity) {
                wantedVelToAdd += velToAdd;
            }

            previousForward = CurrentForward();
        }

        public bool Airborne() {
            if (player.thisController.isGrounded) return false;
            if (player.isSinking) return false;
            if (player.isUnderwater) return false;
            if (player.isClimbingLadder) return false;

            return true;
        }

        public Vector3 CurrentForward() {
            return player.gameObject.transform.forward;
        }

        public float CurrentVelocity() {
            return player.thisController.velocity.magnitude;
        }

        public bool ReachedMaxVelocity() {
            return CurrentVelocity() >= cfg.MAX_AIR_VELOCITY;
        }

        public void SetStamina(float val) {
            // Prevents potential weird behaviours.
            player.sprintMeter = ValNonNegative(val);
        }

        private float ValNonNegative(float newVal) {
            if (newVal <= 0f) {
                newVal = 0f;
            }

            return newVal;
        }

        public String Vec3ToString(Vector3 vec) {
            float x = (float) Math.Round(vec.x, 1);
            float y = (float) Math.Round(vec.y, 1);
            float z = (float) Math.Round(vec.z, 1);

            return $"X: {x}, Y: {y}, Z: {z}";
        }
    }
}
