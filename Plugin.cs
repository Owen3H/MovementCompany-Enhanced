using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MovementCompany.Component;
using UnityEngine;

namespace MovementCompany
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin {
        Harmony _harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
        public static ManualLogSource Log;
        
        private void Awake() {
            Log = Logger;
            // Plugin startup logic
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");
            _harmony.PatchAll();
        }

        public void OnDestroy() {
            GameObject gameObject = new GameObject("MovementAdder");
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<MovementAdder>();
            LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
        }
    }
}