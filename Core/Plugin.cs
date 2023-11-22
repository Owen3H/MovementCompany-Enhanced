using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MovementCompanyEnhanced.Patches;

namespace MovementCompanyEnhanced.Core {
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

            InitPatcher();
            Logger.LogInfo("Plugin loaded.");
        }

        public void OnDestroy() {
            if (!PluginEnabled()) return;
            LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
        }

        private void InitPatcher() {
            patcher = new(Metadata.GUID);
            patcher.PatchAll(typeof(PlayerControllerPatch));

            LogPatches();
        }

        public void LogPatches() {
            IEnumerable<MethodBase> patches = patcher.GetPatchedMethods();
            string str = string.Join(", ", patches.ToList());

            Logger.LogInfo("Applied patches to: " + str);
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