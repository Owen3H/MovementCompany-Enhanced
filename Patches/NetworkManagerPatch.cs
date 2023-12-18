using HarmonyLib;
using MovementCompanyEnhanced.Core;

namespace MovementCompanyEnhanced.Patches {
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class NetworkManagerPatch {
        [HarmonyPostfix]
        [HarmonyPatch("StartDisconnect")]
        public static void PlayerLeave() {
            Plugin.Logger.LogInfo($"Config sync disabled. Reverted to client config.");
            Config.RevertSync();
        }
    }
}
