using GameNetcodeStuff;
using MovementCompanyEnhanced.Core;
using System;
using UnityEngine;

namespace MovementCompanyEnhanced.Component {
    internal class CustomMovement : MonoBehaviour {
        public PlayerControllerB player;

        private Config cfg;

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
            //GUI.Label(new Rect(10, 80, 500, 500), "Reached Max Velocity: " + ReachedMaxVelocity());

            GUI.Label(new Rect(10, 90, 500, 500), "Jump Time: " + jumpTime);
            GUI.Label(new Rect(10, 110, 500, 500), "Airborne: " + inAir);
            GUI.Label(new Rect(10, 130, 500, 500), "Airborne (Actual): " + Airborne());
        }

        public void Start() {
            cfg = Plugin.Config;
            ApplyConfigSpeeds();

            // Player is spawned slightly in the air, ground them so their
            // velocity wont increase and cause them to fly around the ship.
            MovePlayer(0, -0.5f, 0);
        }

        public void Update() {
            if (player.isInHangarShipRoom && !Plugin.Config.BHOP_IN_SHIP) {
                return;
            }

            if (player.isInsideFactory && !Plugin.Config.BHOP_IN_FACTORY) {
                return;
            }

            bool jumping = player.playerBodyAnimator.GetBool("Jumping");

            // Allows infinite bhopping by keeping the "sprint meter" full.
            if (cfg.INFINITE_STAMINA) {
                SetStamina(cfg.MAX_STAMINA);
            }

            UpdateJumpTime(jumping);

            // No longer in air, slowly decrease velocity.
            if (!Airborne()) {
                LerpToGround();
                return;
            }

            if (!ReachedMaxVelocity()) {
                ApplyBhop();
            }
        }

        public void ApplyConfigSpeeds() {
            player.movementSpeed = ValNonNegative(cfg.MOVEMENT_SPEED);
            player.climbSpeed = ValNonNegative(cfg.CLIMB_SPEED);
            player.sinkingSpeedMultiplier = ValNonNegative(cfg.SINK_SPEED_MULTIPLIER);
        }

        public void UpdateJumpTime(bool jumping) {
            if (jumping && jumpTime < cfg.MAX_JUMP_DURATION) {
                Plugin.Logger.LogDebug("Updating jump time");

                player.fallValue = player.jumpForce;
                jumpTime += Time.deltaTime * cfg.JUMP_TIME_MULTIPLIER / 100;
            }
        }

        public void LerpToGround() {
            float timeToGround = Time.deltaTime * cfg.GROUND_VELOCITY_MULTIPLIER;
            wantedVelToAdd = Vector3.Lerp(wantedVelToAdd, Vector3.zero, timeToGround);

            inAir = false;
            jumpTime = 0;
        }

        public void ApplyBhop() {
            if (!inAir) {
                inAir = true;
                AddJumpVelocity(cfg.AIR_VELOCITY_MULTIPLIER);
            }

            wantedVelToAdd.y = 0;

            // TODO: Check if player is walking/sprinting before doing this.
            // Currently, stationary jumping will move them forward :/
            MovePlayer(CurrentForward() * (wantedVelToAdd.magnitude / cfg.FORWARD_VELOCITY_DAMPER));
            AddRotationVelocity(cfg.ROTATION_THRESHOLD);
        }

        public void AddJumpVelocity(float multiplier) {
            Vector3 vel = player.thisController.velocity;
            vel.y = 0;

            if (wantedVelToAdd.magnitude < 0.2f) {
                wantedVelToAdd += vel * multiplier;
            }
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
            return !player.thisController.isGrounded && !player.isClimbingLadder;
        }

        public void MovePlayer(Vector3 motion) {
            player.thisController.Move(motion);
        }

        public void MovePlayer(float x, float y, float z) {
            player.thisController.Move(new Vector3(x, y, z));
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

        public string Vec3ToString(Vector3 vec) {
            float x = (float) Math.Round(vec.x, 1);
            float y = (float) Math.Round(vec.y, 1);
            float z = (float) Math.Round(vec.z, 1);

            return $"X: {x}, Y: {y}, Z: {z}";
        }
    }
}
