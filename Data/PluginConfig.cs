using BepInEx.Configuration;

namespace MovementCompanyEnhanced.Core {
    public class PluginConfig {
        readonly ConfigFile configFile;

        public bool PLUGIN_ENABLED { get; private set; }
        public bool DISPLAY_DEBUG_INFO { get; private set; }

        public bool INFINITE_STAMINA { get; private set; }

        public float MAX_STAMINA { get; private set; }
        public float MAX_JUMP_DURATION { get; private set; }
        public float ROTATION_THRESHOLD { get; private set; }
        public float JUMP_TIME_MULTIPLIER { get; private set; }
        public float MAX_AIR_VELOCITY { get; private set; }
        public float FORWARD_VELOCITY_DAMPER { get; private set; } 
        public float AIR_VELOCITY_MULTIPLIER { get; private set; }
        public float GROUND_VELOCITY_MULTIPLIER { get; private set; }
        public float SINK_SPEED_MULTIPLIER { get; private set; }
        public float MOVEMENT_SPEED { get; private set; }
        public float CLIMB_SPEED { get; private set; }

        public PluginConfig(ConfigFile cfg) {
            configFile = cfg;
            PLUGIN_ENABLED = NewEntry("bEnabled", true, "Enable or disable the plugin globally.");
        }

        private T NewEntry<T>(string key, T defaultVal, string description) {
            return configFile.Bind(Metadata.GUID, key, defaultVal, description).Value;
        }

        public void InitBindings() {
            DISPLAY_DEBUG_INFO = NewEntry("bDisplayDebugInfo", true, "Whether to display coordinates, velocity and other debug info.");

            //WALK_CONSUMES_STAMINA = NewEntry("bJumpConsumesStamina", false, 
            //    "Whether walking costs the player some of their stamina."
            //);

            //SPRINT_CONSUMES_STAMINA = NewEntry("bJumpConsumesStamina", false,
            //    "Whether sprinting costs the player some of their stamina."
            //);

            //JUMP_CONSUMES_STAMINA = NewEntry("bJumpConsumesStamina", false, 
            //    "Whether jumping costs the player some of their stamina."
            //);            

            INFINITE_STAMINA = NewEntry("bInfiniteStamina", true,
                "Whether the player has infinite stamina (essential for bhopping).\n" +
                "THIS WILL BE REPLACED IN A FUTURE UPDATE."
            );

            // Native sprint meter is clamped between 0 and 1.
            MAX_STAMINA = NewEntry("fMaxStamina", 200f,
                "The amount at which the sprint meter (aka stamina) is considered full.\nClamped between 0 and 1 in the base game."
            );

            MAX_JUMP_DURATION = NewEntry("fMaxJumpDuration", 0.0015f,
                "The maximum amount of time a jump can last for."
            );

            ROTATION_THRESHOLD = NewEntry("fRotationThreshold", 0.011f,
                "The magnitude at which to begin applying velocity. Higher = more rotation required."
            );

            JUMP_TIME_MULTIPLIER = NewEntry("fJumpTimeMultiplier", 38f,
                "The value to multiply 'jump time' by, affecting how quickly you hit MaxJumpDuration.\n" +
                "Lower values will cause the player to feel more weightless."
            );

            MAX_AIR_VELOCITY = NewEntry("fMaxAirVelocity", 40f,
                "The value at which velocity will stop being applied when airborne."
            );

            FORWARD_VELOCITY_DAMPER = NewEntry("fForwardVelocityDamper", 2.5f, 
                "After jumping, a forward velocity is applied - which is first dampened by this value.\n" +
                "Note: Increasing this value too much may hinder bhopping."
            );

            AIR_VELOCITY_MULTIPLIER = NewEntry("fAirVelocityMultiplier", 0.005f,
                "The value to multiply the player's velocity by when airborne.\n" +
                "Note: Do not let the small value fool you, anything above the default is veryy fast!"
            );

            GROUND_VELOCITY_MULTIPLIER = NewEntry("fGroundVelocityMultiplier", 1.1f,
                "The value to multiply inverse velocity by when not airborne.\n" +
                "Essentially, this affects how much the player is slowed down when hitting the ground."
            );

            SINK_SPEED_MULTIPLIER = NewEntry("fSinkSpeedMultiplier", 0.16f,
                "Value to multiply the sinking speed by when in quicksand.\n"+
                "Don't want to sink as fast? Decrease this value."
            );

            MOVEMENT_SPEED = NewEntry("fMovementSpeed", 4.2f,
                "The base speed at which the player moves. This is NOT a multiplier."
            );

            CLIMB_SPEED = NewEntry("fClimbSpeed", 3.9f,
                "The base speed at which the player climbs. This is NOT a multiplier."
            );
        }
    }
}
