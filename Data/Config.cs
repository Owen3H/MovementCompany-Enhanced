using System;
using BepInEx.Configuration;

using MovementCompanyEnhanced.Data;
using MovementCompanyEnhanced.Patches;

using Unity.Netcode;
using Unity.Collections;
using System.Runtime.Serialization;

namespace MovementCompanyEnhanced.Core;

public struct ConfigCategory {
    public static ConfigCategory GENERAL => new("0 >> General << 0");
    public static ConfigCategory MOVEMENT => new("1 >> Movement << 1");
    public static ConfigCategory STAMINA => new("2 >> Stamina << 2");
    public static ConfigCategory BHOP => new("3 >> Bhopping << 3");
    public static ConfigCategory MISC => new("4 >> Miscellaneous << 4");

    public string Value { get; private set; }

    private ConfigCategory(string value) {
        Value = value;
    }
}

[DataContract]
public class Config : SyncedInstance<Config> {
    [DataMember] public bool PLUGIN_ENABLED { get; private set; }
    [DataMember] public bool DISPLAY_DEBUG_INFO { get; private set; }
    [DataMember] public bool SYNC_TO_CLIENTS { get; private set; }

    [DataMember] public bool HOLD_TO_CROUCH { get; private set; }
    [DataMember] public bool REMOVE_FIRST_JUMP_DELAY {  get; private set; }
    [DataMember] public bool REMOVE_SECOND_JUMP_DELAY { get; private set; }

    [DataMember] public ConfigEntry<float> MOVEMENT_SPEED { get; private set; }
    [DataMember] public float CLIMB_SPEED { get; private set; }
    [DataMember] public float SINK_SPEED_MULTIPLIER { get; private set; }

    [DataMember] public bool FALL_DAMAGE_ENABLED { get; private set; }
    [DataMember] public float FALL_DAMAGE { get; private set; }
    //public bool FALL_DAMAGE_WEIGHT_AFFECTED { get; private set; }
    //public float FALL_DAMAGE_WEIGHT_MULTIPLIER { get; private set; }

    [DataMember] public bool INFINITE_STAMINA { get; private set; }
    [DataMember] public float MAX_STAMINA { get; private set; }

    [DataMember] public bool BHOP_IN_FACTORY { get; private set; }
    [DataMember] public bool BHOP_IN_SHIP { get; private set; }

    [DataMember] public float MAX_JUMP_DURATION { get; private set; }
    [DataMember] public float ROTATION_THRESHOLD { get; private set; }
    [DataMember] public float JUMP_TIME_MULTIPLIER { get; private set; }
    [DataMember] public float MAX_AIR_VELOCITY { get; private set; }
    [DataMember] public float FORWARD_VELOCITY_DAMPER { get; private set; } 
    [DataMember] public float AIR_VELOCITY_MULTIPLIER { get; private set; }
    [DataMember] public float GROUND_VELOCITY_MULTIPLIER { get; private set; }

    [NonSerialized]
    readonly ConfigFile configFile;

    public Config(ConfigFile cfg) {
        InitInstance(this);

        configFile = cfg;
        PLUGIN_ENABLED = NewEntry("bEnabled", true, "Enable or disable the plugin globally.");
    }

    private T NewEntry<T>(string key, T defaultVal, string desc) {
        return NewEntry(ConfigCategory.GENERAL, key, defaultVal, desc);
    }

    private T NewEntry<T>(ConfigCategory category, string key, T defaultVal, string desc) {
        return configFile.Bind(category.Value, key, defaultVal, desc).Value;
    }

    public void InitBindings() {
        #region General Values (Enable plugin, debugging etc)
        DISPLAY_DEBUG_INFO = NewEntry("bDisplayDebugInfo", false, "Whether to display coordinates, velocity and other debug info.");

        SYNC_TO_CLIENTS = NewEntry("bSyncToClients", true,
            "As the host, should clients be forced to use our config values?\n" +
            "Setting this to `false` will allow clients to use their own config."
        );
        #endregion

        #region Movement Related Values (Speeds, Fall Damage)
        HOLD_TO_CROUCH = NewEntry(ConfigCategory.MOVEMENT, "bHoldToCrouch", true, 
            "Whether the player should hold to crouch instead of a toggle.\n" +
            "NOTE: This setting is client-side and cannot be forced by the host."
        );

        MOVEMENT_SPEED = NewEntry(ConfigCategory.MOVEMENT, "fMovementSpeed", 4.1f,
            "The base speed at which the player moves. This is NOT a multiplier."
        );

        CLIMB_SPEED = NewEntry(ConfigCategory.MOVEMENT, "fClimbSpeed", 3.9f,
            "The base speed at which the player climbs. This is NOT a multiplier."
        );

        SINK_SPEED_MULTIPLIER = NewEntry(ConfigCategory.MOVEMENT, "fSinkSpeedMultiplier", 0.16f,
            "Value to multiply the sinking speed by when in quicksand.\n" +
            "Don't want to sink as fast? Decrease this value."
        );

        REMOVE_FIRST_JUMP_DELAY = NewEntry(ConfigCategory.MOVEMENT, "bRemoveFirstJumpDelay", true,
            "Removes the immediate jump delay of 150ms after jumping."
        );

        REMOVE_SECOND_JUMP_DELAY = NewEntry(ConfigCategory.MOVEMENT, "bRemoveSecondJumpDelay", true,
            "Removes the jump delay of 100ms before jumping can end."
        );

        FALL_DAMAGE_ENABLED = NewEntry(ConfigCategory.MOVEMENT, "bFallDamageEnabled", true, "Whether you take fall damage. 4Head");

        FALL_DAMAGE = NewEntry(ConfigCategory.MOVEMENT, "fFallDamage", 16.5f,
            "How much base HP the player loses from every fall. Clamped between 0-100."
        );

        //FALL_DAMAGE_WEIGHT_AFFECTED = NewEntry(ConfigCategory.MOVEMENT, "bWeightAffectsFallDamage", true,
        //    "If bWeightAffectsFallDamage is true, this value determines how much it affects it."
        //);

        //FALL_DAMAGE_WEIGHT_MULTIPLIER = NewEntry(ConfigCategory.MOVEMENT, "bFallDamageWeightMultiplier", 0.5f, 
        //    "The value which fFallDamage is multiplied by \n" +
        //    "For example: 0.8f would take 80% of the weight from the player's HP."
        //);
        #endregion

        #region Stamina Values (Consumption, Infinite, Max)
        //WALK_CONSUMES_STAMINA = NewEntry("bWalkConsumesStamina", false, 
        //    "Whether walking costs the player some of their stamina."
        //);

        //SPRINT_CONSUMES_STAMINA = NewEntry("bSprintConsumesStamina", false,
        //    "Whether sprinting costs the player some of their stamina."
        //);

        //JUMP_STAMINA_CONSUMPTION = NewEntry("bJumpStaminaConsumption", false, 
        //    "Whether jumping costs the player some of their stamina."
        //);

        INFINITE_STAMINA = NewEntry(ConfigCategory.STAMINA, "bInfiniteStamina", true,
            "Whether the player has infinite stamina (essential for bhopping).\n" +
            "Only applies wherever bhopping is allowed.\n" +
            "NOTE: THIS WILL BE REPLACED IN FUTURE."
        );

        // Native sprint meter is clamped between 0 and 1.
        MAX_STAMINA = NewEntry(ConfigCategory.STAMINA, "fMaxStamina", 200f,
            "The amount at which the sprint meter (aka stamina) is considered full.\nClamped between 0 and 1 in the base game."
        );
        #endregion

        #region Bhop Related Values (Velocity, Jumping, Rotation)
        BHOP_IN_FACTORY = NewEntry(ConfigCategory.BHOP, "bBhopInFactory", false, "Whether bhopping (not general movement) is allowed inside the factory.");
        BHOP_IN_SHIP = NewEntry(ConfigCategory.BHOP, "bBhopInFactory", false, "Whether bhopping (not general movement) is allowed inside the ship.");

        MAX_JUMP_DURATION = NewEntry(ConfigCategory.BHOP, "fMaxJumpDuration", 0.0025f,
            "The maximum amount of time a jump can last for."
        );

        ROTATION_THRESHOLD = NewEntry(ConfigCategory.BHOP, "fRotationThreshold", 0.0115f,
            "The magnitude at which to begin applying velocity. Higher = more rotation required."
        );

        JUMP_TIME_MULTIPLIER = NewEntry(ConfigCategory.BHOP, "fJumpTimeMultiplier", 29f,
            "The value to multiply 'jump time' by, affecting how quickly you hit MaxJumpDuration.\n" +
            "Lower values will cause the player to feel more weightless."
        );

        MAX_AIR_VELOCITY = NewEntry(ConfigCategory.BHOP, "fMaxAirVelocity", 60f,
            "The value at which velocity will stop being applied when airborne."
        );

        FORWARD_VELOCITY_DAMPER = NewEntry(ConfigCategory.BHOP, "fForwardVelocityDamper", 1.65f, 
            "After jumping, a forward velocity is applied - which is first dampened by this value.\n" +
            "Note: Increasing this value too much may hinder bhopping."
        );

        AIR_VELOCITY_MULTIPLIER = NewEntry(ConfigCategory.BHOP, "fAirVelocityMultiplier", 0.0046f,
            "The value to multiply the player's velocity by when airborne.\n" +
            "Note: Do not let the small value fool you, anything above the default is veryy fast!"
        );

        GROUND_VELOCITY_MULTIPLIER = NewEntry(ConfigCategory.BHOP, "fGroundVelocityMultiplier", 1.9f,
            "The value determining how quickly velocity decreases when not airborne.\n" +
            "Essentially, this affects how much the player is slowed down when hitting the ground."
        );
        #endregion
    }

    internal static void RequestSync() {
        if (!IsClient) return;

        using FastBufferWriter stream = new(INT_SIZE, Allocator.Temp);

        // Method `OnRequestSync` will then get called on host.
        SendMessage("MCE_OnRequestConfigSync", 0uL, stream);
    }

    internal static void OnRequestSync(ulong clientId, FastBufferReader _) {
        if (!IsHost) return;

        LogDebug($"Config sync request received from client: {clientId}");

        byte[] array = SerializeToBytes(Instance);
        int value = array.Length;

        using FastBufferWriter stream = new(value + INT_SIZE, Allocator.Temp);

        try {
            stream.WriteValueSafe(in value, default);
            stream.WriteBytesSafe(array);

            SendMessage("MCE_OnReceiveConfigSync", clientId, stream);
        } catch(Exception e) {
            LogErr($"Error occurred syncing config with client: {clientId}\n{e}");
        }
    }

    internal static void OnReceiveSync(ulong _, FastBufferReader reader) {
        if (!reader.TryBeginRead(INT_SIZE)) {
            LogErr("Config sync error: Could not begin reading buffer.");
            return;
        }

        reader.ReadValueSafe(out int val, default);
        if (!reader.TryBeginRead(val)) {
            LogErr("Config sync error: Host could not sync.");
            return;
        }

        byte[] data = new byte[val];
        reader.ReadBytesSafe(ref data, val);

        SyncInstance(data);
        PlayerControllerPatch.movementScript.ApplyConfigSpeeds();
    }
}
