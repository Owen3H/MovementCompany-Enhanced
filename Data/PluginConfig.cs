using BepInEx.Configuration;
using System.Linq;

namespace MovementCompanyEnhanced.Core {
    public class Config {
        public bool PLUGIN_ENABLED { get; private set; }
        public bool DISPLAY_DEBUG_INFO { get; private set; }

        public bool INFINITE_STAMINA { get; private set; }
        public float MAX_STAMINA { get; private set; }

        public bool BHOP_IN_FACTORY { get; private set; }
        public bool BHOP_IN_SHIP { get; private set; }
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

        readonly ConfigFile configFile;

        public readonly struct Category {
            public static Category GENERAL => new("0 >> General << 0");
            public static Category STAMINA => new("1 >> Stamina << 1");
            public static Category BHOP => new("2 >> Bhopping << 2");
            public static Category MISC => new("3 >> Miscellaneous << 3");

            readonly string Value;

            private Category(string value) {
                Value = value;
            }

            public string AsString() {
                return Value.ToString();
            }
        }

        public Config(ConfigFile cfg) {
            configFile = cfg;

            PLUGIN_ENABLED = NewEntry("bEnabled", true, "Enable or disable the plugin globally.");
        }

        private T NewEntry<T>(string key, T defaultVal, string desc) {
            return NewEntry(Category.GENERAL, key, defaultVal, desc);
        }

        private T NewEntry<T>(Category category, string key, T defaultVal, string desc) {
            return configFile.Bind(category.AsString(), key, defaultVal, desc).Value;
        }

        public void InitBindings() {
            #region General Values (Enable plugin, base speeds)
            DISPLAY_DEBUG_INFO = NewEntry("bDisplayDebugInfo", false, "Whether to display coordinates, velocity and other debug info.");
            #endregion

            #region Base Movement Values
            MOVEMENT_SPEED = NewEntry("fMovementSpeed", 4.2f,
                "The base speed at which the player moves. This is NOT a multiplier."
            );

            CLIMB_SPEED = NewEntry("fClimbSpeed", 3.9f,
                "The base speed at which the player climbs. This is NOT a multiplier."
            );

            SINK_SPEED_MULTIPLIER = NewEntry("fSinkSpeedMultiplier", 0.16f,
                "Value to multiply the sinking speed by when in quicksand.\n" +
                "Don't want to sink as fast? Decrease this value."
            );
            #endregion

            #region Stamina Values
            //WALK_CONSUMES_STAMINA = NewEntry("bWalkConsumesStamina", false, 
            //    "Whether walking costs the player some of their stamina."
            //);

            //SPRINT_CONSUMES_STAMINA = NewEntry("bSprintConsumesStamina", false,
            //    "Whether sprinting costs the player some of their stamina."
            //);

            //JUMP_STAMINA_CONSUMPTION = NewEntry("bJumpStaminaConsumption", false, 
            //    "Whether jumping costs the player some of their stamina."
            //);

            INFINITE_STAMINA = NewEntry(Category.STAMINA, "bInfiniteStamina", true,
                "Whether the player has infinite stamina (essential for bhopping).\n" +
                "THIS WILL BE REPLACED IN A FUTURE UPDATE."
            );

            // Native sprint meter is clamped between 0 and 1.
            MAX_STAMINA = NewEntry(Category.STAMINA, "fMaxStamina", 200f,
                "The amount at which the sprint meter (aka stamina) is considered full.\nClamped between 0 and 1 in the base game."
            );
            #endregion

            #region Bhop Related Values
            BHOP_IN_FACTORY = NewEntry(Category.BHOP, "bBhopInFactory", false, "Whether bhopping (not general movement) is allowed inside the factory.");
            BHOP_IN_SHIP = NewEntry(Category.BHOP, "bBhopInFactory", false, "Whether bhopping (not general movement) is allowed inside the ship.");

            MAX_JUMP_DURATION = NewEntry(Category.BHOP, "fMaxJumpDuration", 0.0023f,
                "The maximum amount of time a jump can last for."
            );

            ROTATION_THRESHOLD = NewEntry(Category.BHOP, "fRotationThreshold", 0.011f,
                "The magnitude at which to begin applying velocity. Higher = more rotation required."
            );

            JUMP_TIME_MULTIPLIER = NewEntry(Category.BHOP, "fJumpTimeMultiplier", 28f,
                "The value to multiply 'jump time' by, affecting how quickly you hit MaxJumpDuration.\n" +
                "Lower values will cause the player to feel more weightless."
            );

            MAX_AIR_VELOCITY = NewEntry(Category.BHOP, "fMaxAirVelocity", 50f,
                "The value at which velocity will stop being applied when airborne."
            );

            FORWARD_VELOCITY_DAMPER = NewEntry(Category.BHOP, "fForwardVelocityDamper", 1.8f, 
                "After jumping, a forward velocity is applied - which is first dampened by this value.\n" +
                "Note: Increasing this value too much may hinder bhopping."
            );

            AIR_VELOCITY_MULTIPLIER = NewEntry(Category.BHOP, "fAirVelocityMultiplier", 0.005f,
                "The value to multiply the player's velocity by when airborne.\n" +
                "Note: Do not let the small value fool you, anything above the default is veryy fast!"
            );

            GROUND_VELOCITY_MULTIPLIER = NewEntry(Category.BHOP, "fGroundVelocityMultiplier", 2.4f,
                "The value determining how quickly velocity decreases when not airborne.\n" +
                "Essentially, this affects how much the player is slowed down when hitting the ground."
            );
            #endregion
        }
    }
}
