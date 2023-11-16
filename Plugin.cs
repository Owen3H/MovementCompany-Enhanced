using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MovementCompany.Component;
using UnityEngine;

namespace MovementCompany {
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        public static new ManualLogSource Logger { get; private set; }
        private Harmony _harmony;

        private void Awake() {
            _harmony = new(MyPluginInfo.PLUGIN_GUID);

            Logger.LogInfo($"Loaded plugin: {MyPluginInfo.PLUGIN_GUID}");
            _harmony.PatchAll();
        }

        public void OnDestroy() {
            GameObject gameObject = new("MovementAdder");
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<MovementAdder>();

            LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
        }
    }
}