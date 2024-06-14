using System;
using GameNetcodeStuff;
using MovementCompanyEnhanced.Core;
using UnityEngine;

namespace MovementCompanyEnhanced.Component;

internal class CustomMovement : MonoBehaviour {
    public PlayerControllerB player { get; internal set; } = null;

    internal MCEConfig cfg => MCEConfig.Instance;

    float jumpTime;

    bool inAir;

    Vector3 wantedVelToAdd;

    Vector3 velToAdd;

    Vector3 previousForward;

    double Round (float val) => Math.Round(val, 3);

    void OnGUI() {
        if (!MCEConfig.Default.DISPLAY_DEBUG_INFO.Value) return;
        if (player.thisController == null) return;

        Vector3 pos = player.thisController.transform.position;
        GUI.Label(new(10, 10, 500, 500), pos.ToString("F3"));

        GUI.Label(new(10, 40, 500, 500), $"Current Velocity: {Round(CurrentVelocity())}");
        GUI.Label(new(10, 60, 500, 500), $"Wanted Velocity: {Round(wantedVelToAdd.magnitude)}");

        GUI.Label(new(10, 90, 500, 500), "Jump Time: " + jumpTime);
        GUI.Label(new(10, 110, 500, 500), "Airborne: " + inAir);
        GUI.Label(new(10, 130, 500, 500), "Airborne (Actual): " + Airborne());
    }

    void Start() {
        ApplyConfigSpeeds(MCEConfig.IsHost);

        velToAdd = new(0.0002f, 0.0002f, 0.0002f);

        // Player is spawned slightly in the air, ground them so their
        // velocity wont increase and cause them to fly around the ship.
        MovePlayer(0, -0.5f, 0);
    }

    void Update() {
        if (!player) return;

        if (player.isInHangarShipRoom && !cfg.BHOP_IN_SHIP) {
            return;
        }

        if (player.isInsideFactory && !cfg.BHOP_IN_FACTORY) {
            return;
        }

        bool jumping = player.playerBodyAnimator?.GetBool("Jumping") ?? false;

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

    internal void ApplyConfigSpeeds(bool host = false) {
        Plugin.Logger.LogDebug("Attempting to apply config speeds!");

        if (cfg == null) {
            Plugin.Logger.LogWarning("Config instance is null! Could not apply speeds.");
            return;
        }

        var prefix = host ? "Host" : "Client";
        Plugin.Logger.LogDebug($"{prefix} - move speed set to: {cfg?.MOVEMENT_SPEED?.Value ?? player.movementSpeed}");

        player.movementSpeed = ValNonNegative(cfg.MOVEMENT_SPEED);
        player.climbSpeed = ValNonNegative(cfg.CLIMB_SPEED);
        player.sinkingSpeedMultiplier = ValNonNegative(cfg.SINK_SPEED_MULTIPLIER);
    }

    private void UpdateJumpTime(bool jumping) {
        if (!jumping || jumpTime >= cfg.MAX_JUMP_DURATION)
            return;

        player.fallValue = player.jumpForce;
        jumpTime += Time.deltaTime * cfg.JUMP_TIME_MULTIPLIER / 100;
    }

    private void LerpToGround() {
        float timeToGround = Time.deltaTime * cfg.GROUND_VELOCITY_MULTIPLIER;
        wantedVelToAdd = Vector3.Lerp(wantedVelToAdd, Vector3.zero, timeToGround);

        inAir = false;
        jumpTime = 0;
    }

    private void ApplyBhop() {
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
        player.thisController.Move(new(x, y, z));
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
        return CurrentVelocity() >= cfg.MAX_AIR_VELOCITY;
    }

    private void SetStamina(float val) {
        player.sprintMeter = ValNonNegative(val);
    }

    // Prevents potential weird behaviours.
    private float ValNonNegative(float newVal) {
        if (newVal <= 0f) {
            newVal = 0f;
        }

        return newVal;
    }
}