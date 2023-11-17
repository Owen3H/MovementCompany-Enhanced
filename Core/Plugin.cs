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

            if (CheckPluginDisabled(true)) return;

            Config.InitBindings();

            _harmony = new(Metadata.GUID);
            _harmony.PatchAll();

            Logger.LogInfo($"Loaded plugin: {Metadata.GUID}");
        }

        public void OnDestroy() {
            if (CheckPluginDisabled()) return;

            GameObject gameObject = new("MovementAdder");
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<MovementAdder>();

            LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
        }

        public bool CheckPluginDisabled(bool log = false) {
            bool enabled = Config.PLUGIN_ENABLED;
            if (!enabled && log) Logger.LogInfo("MovementCompany disabled globally.");

            return !enabled;
        }
    }
}