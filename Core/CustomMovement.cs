using GameNetcodeStuff;
using MovementCompanyEnhanced.Core;
using System;
using UnityEngine;

namespace MovementCompanyEnhanced.Component;

internal class CustomMovement : MonoBehaviour {
    public PlayerControllerB player { get; internal set; }

    internal Config cfg => Config.Instance;

    float jumpTime;

    bool inAir;

    Vector3 wantedVelToAdd;

    Vector3 velToAdd;

    Vector3 previousForward;

    void OnGUI() {
        if (!Config.Default.DISPLAY_DEBUG_INFO.Value) return;
        if (player.thisController == null) return;

        Vector3 pos = player.thisController.transform.position;
        GUI.Label(new(10, 10, 500, 500), Vec3ToString(pos));

        GUI.Label(new(10, 40, 500, 500), "Current Velocity: " + Math.Round(CurrentVelocity(), 3));
        GUI.Label(new(10, 60, 500, 500), "Wanted Velocity: " + Math.Round(wantedVelToAdd.magnitude, 3));

        GUI.Label(new(10, 90, 500, 500), "Jump Time: " + jumpTime);
        GUI.Label(new(10, 110, 500, 500), "Airborne: " + inAir);
        GUI.Label(new(10, 130, 500, 500), "Airborne (Actual): " + Airborne());
    }

    void Start() {
        if (Config.IsHost) {
            ApplyConfigSpeeds(true);
        }

        velToAdd = new(0.0002f, 0.0002f, 0.0002f);

        // Player is spawned slightly in the air, ground them so their
        // velocity wont increase and cause them to fly around the ship.
        MovePlayer(0, -0.5f, 0);
    }

    void Update() {
        if (!player) return;

        if (player.isInHangarShipRoom && !cfg.BHOP_IN_SHIP.Value) {
            return;
        }

        if (player.isInsideFactory && !cfg.BHOP_IN_FACTORY.Value) {
            return;
        }

        bool jumping = player.playerBodyAnimator?.GetBool("Jumping") ?? false;

        // Allows infinite bhopping by keeping the "sprint meter" full.
        if (cfg.INFINITE_STAMINA.Value) {
            SetStamina(cfg.MAX_STAMINA.Value);
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

    internal void ApplyConfigSpeeds(bool host = false) {
        var prefix = host ? "Host" : "Client";

        Plugin.Logger.LogDebug($"{prefix} - move speed set to: {cfg?.MOVEMENT_SPEED?.Value}");

        player.movementSpeed = ValNonNegative(cfg.MOVEMENT_SPEED.Value);
        player.climbSpeed = ValNonNegative(cfg.CLIMB_SPEED.Value);
        player.sinkingSpeedMultiplier = ValNonNegative(cfg.SINK_SPEED_MULTIPLIER.Value);
    }

    private void UpdateJumpTime(bool jumping) {
        if (jumping && jumpTime < cfg.MAX_JUMP_DURATION.Value) {
            player.fallValue = player.jumpForce;
            jumpTime += Time.deltaTime * cfg.JUMP_TIME_MULTIPLIER.Value / 100;
        }
    }

    private void LerpToGround() {
        float timeToGround = Time.deltaTime * cfg.GROUND_VELOCITY_MULTIPLIER.Value;
        wantedVelToAdd = Vector3.Lerp(wantedVelToAdd, Vector3.zero, timeToGround);

        inAir = false;
        jumpTime = 0;
    }

    private void ApplyBhop() {
        if (!inAir) {
            inAir = true;
            AddJumpVelocity(cfg.AIR_VELOCITY_MULTIPLIER.Value);
        }

        wantedVelToAdd.y = 0;

        // TODO: Check if player is walking/sprinting before doing this.
        // Currently, stationary jumping will move them forward :/
        MovePlayer(CurrentForward() * (wantedVelToAdd.magnitude / cfg.FORWARD_VELOCITY_DAMPER.Value));
        AddRotationVelocity(cfg.ROTATION_THRESHOLD.Value);
    }

    private void AddJumpVelocity(float multiplier) {
        Vector3 vel = player.thisController.velocity;
        vel.y = 0;

        if (wantedVelToAdd.magnitude < 0.2f) {
            wantedVelToAdd += vel * multiplier;
        }
    }

    private void AddRotationVelocity(float threshold) {
        Vector3 fwdDiff = CurrentForward() - previousForward;
        bool applyVelocity = fwdDiff.magnitude > threshold;

        if (applyVelocity) {
            wantedVelToAdd += velToAdd;
        }

        previousForward = CurrentForward();
    }

    private void MovePlayer(Vector3 motion) {
        player.thisController.Move(motion);
    }

    private void MovePlayer(float x, float y, float z) {
        player.thisController.Move(new Vector3(x, y, z));
    }

    public bool Airborne() {
        return !player.thisController.isGrounded && !player.isClimbingLadder;
    }

    public Vector3 CurrentForward() {
        return player.transform.forward;
    }

    public float CurrentVelocity() {
        return player.thisController.velocity.magnitude;
    }

    public bool ReachedMaxVelocity() {
        return CurrentVelocity() >= cfg.MAX_AIR_VELOCITY.Value;
    }

    private void SetStamina(float val) {
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