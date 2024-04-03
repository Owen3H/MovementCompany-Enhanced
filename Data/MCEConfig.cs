using System;
using System.Runtime.Serialization;
using BepInEx.Configuration;
using CSync.Lib;
using CSync.Util;

namespace MovementCompanyEnhanced.Core;

public struct ConfigCategory(string value) {
    public static ConfigCategory GENERAL => new("0 >> General << 0");
    public static ConfigCategory MOVEMENT => new("1 >> Movement << 1");
    public static ConfigCategory STAMINA => new("2 >> Stamina << 2");
    public static ConfigCategory BHOP => new("3 >> Bhopping << 3");
    public static ConfigCategory MISC => new("4 >> Miscellaneous << 4");

    public string Value { get; private set; } = value;
}

[DataContract]
public class MCEConfig : SyncedConfig<MCEConfig> {
    #region Client-side entries
    public ConfigEntry<bool> PLUGIN_ENABLED { get; private set; }
    public ConfigEntry<bool> DISPLAY_DEBUG_INFO { get; private set; }
    #endregion

    #region Synced entries
    [DataMember] public SyncedEntry<bool> SYNC_TO_CLIENTS { get; private set; }

    [DataMember] public SyncedEntry<bool> HOLD_TO_CROUCH { get; private set; }
    [DataMember] public SyncedEntry<bool> REMOVE_FIRST_JUMP_DELAY {  get; private set; }
    [DataMember] public SyncedEntry<bool> REMOVE_SECOND_JUMP_DELAY { get; private set; }

    [DataMember] public SyncedEntry<float> MOVEMENT_SPEED { get; private set; }
    [DataMember] public SyncedEntry<float> CLIMB_SPEED { get; private set; }
    [DataMember] public SyncedEntry<float> SINK_SPEED_MULTIPLIER { get; private set; }

    [DataMember] public SyncedEntry<bool> FALL_DAMAGE_ENABLED { get; private set; }
    [DataMember] public SyncedEntry<float> FALL_DAMAGE { get; private set; }
    //[DataMember] public SyncedEntry<bool> FALL_DAMAGE_WEIGHT_AFFECTED { get; private set; }
    //[DataMember] public SyncedEntry<float> FALL_DAMAGE_WEIGHT_MULTIPLIER { get; private set; }

    [DataMember] public SyncedEntry<bool> INFINITE_STAMINA { get; private set; }
    [DataMember] public SyncedEntry<float> MAX_STAMINA { get; private set; }

    [DataMember] public SyncedEntry<bool> BHOP_IN_FACTORY { get; private set; }
    [DataMember] public SyncedEntry<bool> BHOP_IN_SHIP { get; private set; }

    [DataMember] public SyncedEntry<float> MAX_JUMP_DURATION { get; private set; }
    [DataMember] public SyncedEntry<float> ROTATION_THRESHOLD { get; private set; }
    [DataMember] public SyncedEntry<float> JUMP_TIME_MULTIPLIER { get; private set; }
    [DataMember] public SyncedEntry<float> MAX_AIR_VELOCITY { get; private set; }
    [DataMember] public SyncedEntry<float> FORWARD_VELOCITY_DAMPER { get; private set; } 
    [DataMember] public SyncedEntry<float> AIR_VELOCITY_MULTIPLIER { get; private set; }
    [DataMember] public SyncedEntry<float> GROUND_VELOCITY_MULTIPLIER { get; private set; }
    #endregion

    [NonSerialized]
    readonly ConfigFile configFile;

    public MCEConfig(ConfigFile cfg) : base("MovementCompanyEnhanced") {
        ConfigManager.Register(this);

        configFile = cfg;
        PLUGIN_ENABLED = NewEntry(ConfigCategory.GENERAL, "bEnabled", true, "Enable or disable the plugin globally.");

        SyncComplete += (bool success) => {
            if (!success) {
                Resync();
            }
        };
    }

    private ConfigEntry<V> NewEntry<V>(ConfigCategory category, string key, V defaultVal, string desc) =>
        configFile.Bind(category.Value, key, defaultVal, desc);

    private SyncedEntry<V> NewSyncedEntry<V>(ConfigCategory category, string key, V defaultVal, string desc) =>
        configFile.BindSyncedEntry(category.Value, key, defaultVal, desc);

    public void InitBindings() {
        #region General Values (Enable plugin, debugging etc)
        DISPLAY_DEBUG_INFO = NewEntry(ConfigCategory.GENERAL,
            "bDisplayDebugInfo", false, "Whether to display coordinates, velocity and other debug info."
        );

        SYNC_TO_CLIENTS = NewSyncedEntry(ConfigCategory.GENERAL, "bSyncToClients", true,
            "As the host, should clients be forced to use our config values?\n" +
            "Setting this to `false` will allow clients to use their own config."
        );
        #endregion

        #region Movement Related Values (Speeds, Fall Damage)
        MOVEMENT_SPEED = NewSyncedEntry(ConfigCategory.MOVEMENT, "fMovementSpeed", 4.1f,
            "The base speed at which the player moves. This is NOT a multiplier."
        );

        CLIMB_SPEED = NewSyncedEntry(ConfigCategory.MOVEMENT, "fClimbSpeed", 3.9f,
            "The base speed at which the player climbs. This is NOT a multiplier."
        );

        SINK_SPEED_MULTIPLIER = NewSyncedEntry(ConfigCategory.MOVEMENT, "fSinkSpeedMultiplier", 0.16f,
            "Value to multiply the sinking speed by when in quicksand.\n" +
            "Don't want to sink as fast? Decrease this value."
        );

        HOLD_TO_CROUCH = NewSyncedEntry(ConfigCategory.MOVEMENT, "bHoldToCrouch", true, 
            "Whether the player should hold to crouch instead of a toggle.\n" +
            "NOTE: This setting is client-side and cannot be forced by the host."
        );

        REMOVE_FIRST_JUMP_DELAY = NewSyncedEntry(ConfigCategory.MOVEMENT, "bRemoveFirstJumpDelay", true,
            "Removes the immediate jump delay of 150ms after jumping."
        );

        REMOVE_SECOND_JUMP_DELAY = NewSyncedEntry(ConfigCategory.MOVEMENT, "bRemoveSecondJumpDelay", true,
            "Removes the jump delay of 100ms before jumping can end."
        );

        FALL_DAMAGE_ENABLED = NewSyncedEntry(ConfigCategory.MOVEMENT, "bFallDamageEnabled", true, "Whether you take fall damage. 4Head");

        FALL_DAMAGE = NewSyncedEntry(ConfigCategory.MOVEMENT, "fFallDamage", 16.5f,
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

        INFINITE_STAMINA = NewSyncedEntry(ConfigCategory.STAMINA, "bInfiniteStamina", true,
            "Whether the player has infinite stamina (essential for bhopping).\n" +
            "Only applies wherever bhopping is allowed.\n" +
            "NOTE: THIS WILL BE REPLACED IN FUTURE."
        );

        // Native sprint meter is clamped between 0 and 1.
        MAX_STAMINA = NewSyncedEntry(ConfigCategory.STAMINA, "fMaxStamina", 200f,
            "The amount at which the sprint meter (aka stamina) is considered full.\nClamped between 0 and 1 in the base game."
        );
        #endregion

        #region Bhop Related Values (Velocity, Jumping, Rotation)
        BHOP_IN_FACTORY = NewSyncedEntry(ConfigCategory.BHOP, "bBhopInFactory", false, "Whether bhopping (not general movement) is allowed inside the factory.");
        BHOP_IN_SHIP = NewSyncedEntry(ConfigCategory.BHOP, "bBhopInFactory", false, "Whether bhopping (not general movement) is allowed inside the ship.");

        MAX_JUMP_DURATION = NewSyncedEntry(ConfigCategory.BHOP, "fMaxJumpDuration", 0.0025f,
            "The maximum amount of time a jump can last for."
        );

        ROTATION_THRESHOLD = NewSyncedEntry(ConfigCategory.BHOP, "fRotationThreshold", 0.0115f,
            "The magnitude at which to begin applying velocity. Higher = more rotation required."
        );

        JUMP_TIME_MULTIPLIER = NewSyncedEntry(ConfigCategory.BHOP, "fJumpTimeMultiplier", 29f,
            "The value to multiply 'jump time' by, affecting how quickly you hit MaxJumpDuration.\n" +
            "Lower values will cause the player to feel more weightless."
        );

        MAX_AIR_VELOCITY = NewSyncedEntry(ConfigCategory.BHOP, "fMaxAirVelocity", 60f,
            "The value at which velocity will stop being applied when airborne."
        );

        FORWARD_VELOCITY_DAMPER = NewSyncedEntry(ConfigCategory.BHOP, "fForwardVelocityDamper", 1.65f, 
            "After jumping, a forward velocity is applied - which is first dampened by this value.\n" +
            "Note: Increasing this value too much may hinder bhopping."
        );

        AIR_VELOCITY_MULTIPLIER = NewSyncedEntry(ConfigCategory.BHOP, "fAirVelocityMultiplier", 0.0046f,
            "The value to multiply the player's velocity by when airborne.\n" +
            "Note: Do not let the small value fool you, anything above the default is veryy fast!"
        );

        GROUND_VELOCITY_MULTIPLIER = NewSyncedEntry(ConfigCategory.BHOP, "fGroundVelocityMultiplier", 1.9f,
            "The value determining how quickly velocity decreases when not airborne.\n" +
            "Essentially, this affects how much the player is slowed down when hitting the ground."
        );
        #endregion

        EnableHostSyncControl(SYNC_TO_CLIENTS);
    }
}
