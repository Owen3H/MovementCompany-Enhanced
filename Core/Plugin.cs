using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace MovementCompanyEnhanced.Core;

[BepInPlugin(GUID, NAME, VERSION)]
[BepInDependency("io.github.CSync", BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BaseUnityPlugin {
    public const string GUID = MyPluginInfo.PLUGIN_GUID;
    public const string NAME = MyPluginInfo.PLUGIN_NAME;
    public const string VERSION = MyPluginInfo.PLUGIN_VERSION;

    internal static new ManualLogSource Logger { get; private set; }
    public static new MCEConfig Config { get; private set; }

    private Harmony Patcher;

    private void Awake() {
        Logger = base.Logger;
        Config = new(base.Config);

        if (!PluginEnabled(logIfDisabled: true)) return;

        Config.InitBindings();

        try {
            Patcher = new(GUID);
            Patcher.PatchAll();

            Logger.LogInfo("Plugin loaded.");
        } catch(Exception e) {
            Logger.LogError(e);
        }
    }

    public void OnDestroy() {
        if (!PluginEnabled()) return;
        //ModdedServer.SetServerModdedOnly();
    }

    public bool PluginEnabled(bool logIfDisabled = false) {
        bool enabled = Config.PLUGIN_ENABLED.Value;
        if (!enabled && logIfDisabled) {
            Logger.LogInfo("MovementCompanyEnhanced disabled globally.");
        }

        return enabled;
    }
}