using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MovementCompany.Component;
using UnityEngine;

namespace MovementCompany.Core {
    [BepInPlugin(Metadata.GUID, Metadata.NAME, Metadata.VERSION)]
    public class Plugin : BaseUnityPlugin {
        public static new ManualLogSource Logger { get; private set; }
        public static new PluginConfig Config { get; private set; }

        private Harmony _harmony;

        private void Awake() {
            Logger = base.Logger;
            Config = new(base.Config);

            if (!PluginEnabled(true)) return;

            Config.InitBindings();

            _harmony = new(Metadata.GUID);
            _harmony.PatchAll();

            Logger.LogInfo($"Loaded plugin: {Metadata.GUID}");
        }

        public void OnDestroy() {
            if (!PluginEnabled()) return;

            GameObject movementObj = new("MovementAdder");
            movementObj.AddComponent<MovementAdder>();
            DontDestroyOnLoad(movementObj);

            LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
        }

        public bool PluginEnabled(bool logDisabled = false) {
            bool enabled = Config.PLUGIN_ENABLED;
            if (!enabled && logDisabled) {
                Logger.LogInfo("MovementCompany disabled globally.");
            }

            return enabled;
        }
    }
}