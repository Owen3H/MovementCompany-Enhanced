using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MovementCompanyEnhanced.Core;

[BepInPlugin(Metadata.GUID, Metadata.NAME, Metadata.VERSION)]
[BepInDependency("io.github.CSync", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("LC_API", BepInDependency.DependencyFlags.HardDependency)]
public class Plugin : BaseUnityPlugin {
    internal static new ManualLogSource Logger { get; private set; }
    public static new Config Config { get; private set; }

    private Harmony patcher;

    private void Awake() {
        Logger = base.Logger;
        Config = new(base.Config);

        if (!PluginEnabled(logIfDisabled: true)) return;

        Config.InitBindings();

        try {
            patcher = new(Metadata.GUID);
            patcher.PatchAll();

            Logger.LogInfo("Plugin loaded.");
        } catch(Exception e) {
            Logger.LogError(e);
        }
    }

    public void OnDestroy() {
        if (!PluginEnabled()) return;
        LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
    }

    public bool PluginEnabled(bool logIfDisabled = false) {
        bool enabled = Config.PLUGIN_ENABLED.Value;
        if (!enabled && logIfDisabled) {
            Logger.LogInfo("MovementCompanyEnhanced disabled globally.");
        }

        return enabled;
    }

    //public void LogPatches() {
    //    IEnumerable<MethodBase> patches = patcher.GetPatchedMethods();
    //    string str = string.Join(", ", patches.ToList());

    //    Logger.LogInfo("Applied patches to: " + str);
    //}
}