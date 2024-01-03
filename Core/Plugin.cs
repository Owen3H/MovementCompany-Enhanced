using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MovementCompanyEnhanced.Core {
    [BepInPlugin(Metadata.GUID, Metadata.NAME, Metadata.VERSION)]
    public class Plugin : BaseUnityPlugin {
        private Harmony patcher;

        internal static new ManualLogSource Logger { get; private set; }

        public static new Config Config { get; private set; }

        private void Awake() {
            Logger = base.Logger;
            Config = new(base.Config);

            if (!PluginEnabled(logIfDisabled: true)) return;

            Config.InitBindings();

            try {
                InitPatcher();
                Logger.LogInfo("Plugin loaded.");
            } catch(Exception e) {
                Logger.LogError(e);
            }
        }

        public void OnDestroy() {
            if (!PluginEnabled()) return;
            LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
        }

        public void InitPatcher() {
            patcher = new(Metadata.GUID);
            patcher.PatchAll();

            //LogPatches();
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