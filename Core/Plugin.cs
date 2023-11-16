using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using MovementCompany.Component;
using UnityEngine;

namespace MovementCompany.Core {
    [BepInPlugin(Metadata.GUID, Metadata.NAME, Metadata.VERSION)]
    public class MovementCompany : BaseUnityPlugin {
        public static new ManualLogSource Logger { get; private set; }
        private Harmony _harmony;

        private void Awake() {
            _harmony = new(Metadata.GUID);
            _harmony.PatchAll();

            Logger = base.Logger;
            Logger.LogInfo($"Loaded plugin: {Metadata.GUID}");
        }

        public void OnDestroy() {
            GameObject gameObject = new("MovementAdder");
            DontDestroyOnLoad(gameObject);
            gameObject.AddComponent<MovementAdder>();

            LC_API.ServerAPI.ModdedServer.SetServerModdedOnly();
        }
    }
}