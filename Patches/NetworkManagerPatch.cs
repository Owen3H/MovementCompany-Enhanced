using HarmonyLib;
using MovementCompanyEnhanced.Core;

namespace MovementCompanyEnhanced.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class NetworkManagerPatch {
    [HarmonyPostfix]
    [HarmonyPatch("StartDisconnect")]
    public static void PlayerLeave() {
        if (!Config.Default.SYNC_TO_CLIENTS.Value)
            return;

        Config.RevertSync();
    }
}