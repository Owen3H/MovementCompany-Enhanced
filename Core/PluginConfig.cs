using BepInEx.Configuration;

namespace MovementCompany.Core {
    public class PluginConfig {
        readonly ConfigFile configFile;

        public bool PLUGIN_ENABLED { get; private set; }
        public bool BHOP_CONSUMES_STAMINA { get; private set; }
        public float MAX_STAMINA { get; private set; }
        public float JUMP_TIME_MULTIPLIER { get; private set; }
        public float GROUND_VELOCITY_MULTIPLIER { get; private set; }
        public float AIR_VELOCITY_MULTIPLIER { get; private set; }
        public float ROTATION_THRESHOLD { get; private set; }

        public PluginConfig(ConfigFile cfg) {
            configFile = cfg;
            PLUGIN_ENABLED = NewEntry("bEnabled", true, "Enable or disable the plugin globally.");
        }

        private T NewEntry<T>(string key, T defaultVal, string description) {
            return configFile.Bind(Metadata.GUID, key, defaultVal, description).Value;
        }

        public void InitBindings() {
            JUMP_TIME_MULTIPLIER = NewEntry("fJumpTimeMultiplier", 9.6f,
                ""
            );

            GROUND_VELOCITY_MULTIPLIER = NewEntry("fGroundVelocityMultiplier", 4.2f,
                ""
            );

            AIR_VELOCITY_MULTIPLIER = NewEntry("fAirVelocityMultiplier", 0.0045f,
                "The value to multiply velocity by when in the air."
            );

            ROTATION_THRESHOLD = NewEntry("fRotationThreshold", 0.013f,
                "The magnitude at which to begin applying velocity. Higher = more rotation required."
            );

            BHOP_CONSUMES_STAMINA = NewEntry("bBhopConsumesStamina", true, "");

            // Native sprint meter is clamped between 0 and 1.
            MAX_STAMINA = NewEntry("fMaxStamina", 20f,
                "The amount at which the sprint meter (aka stamina) is considered full.\nClamped between 0 and 1 in the base game."
            );
        }
    }
}
