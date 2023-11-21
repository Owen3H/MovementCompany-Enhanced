using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MovementCompany.Component;
using MovementCompany.Patches;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace MovementCompany.Core {
    [BepInPlugin(Metadata.GUID, Metadata.NAME, Metadata.VERSION)]
    public class Plugin : BaseUnityPlugin {
        internal static new ManualLogSource Logger { get; private set; }
        public static new PluginConfig Config { get; private set; }

        private Harmony patcher;

        private void Awake() {
            Logger = base.Logger;
            Config = new(base.Config);

            if (!PluginEnabled(logIfDisabled: true)) return;

            Config.InitBindings();

            patcher = new(Metadata.GUID);
            patcher.PatchAll(typeof(PlayerControllerPatch));
           
            Logger.LogInfo($"Loaded plugin: {Metadata.GUID}");

            IEnumerable patches = patcher.GetPatchedMethods();
            foreach (MethodBase patch in patches) {
                Logger.LogInfo($"Patched {patch.Name}");
            }
        }

        public void OnDestroy() {
            if (!PluginEnabled()) return;

            GameObject movementObj = new("MovementAdder");
            movementObj.AddComponent<MovementAdder>();
            DontDestroyOnLoad(movementObj);

            LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
        }

        public bool PluginEnabled(bool logIfDisabled = false) {
            bool enabled = Config.PLUGIN_ENABLED;
            if (!enabled && logIfDisabled) {
                Logger.LogInfo("MovementCompany disabled globally.");
            }

            return enabled;
        }
    }
}