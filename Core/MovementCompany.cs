using BepInEx;
using HarmonyLib;
using MovementCompany.Component;
using UnityEngine;

namespace MovementCompany.Core {
    [BepInPlugin(Metadata.GUID, Metadata.NAME, Metadata.VERSION)]
    public class MovementCompany : BaseUnityPlugin {
        private Harmony _harmony;
        //public static ManualLogSource Logger { get; private set; }

        private void Awake() {
            _harmony = new(Metadata.GUID);
            _harmony.PatchAll();

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