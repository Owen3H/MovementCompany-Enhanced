using HarmonyLib;
using MovementCompanyEnhanced.Core;

namespace MovementCompanyEnhanced.Patches;

[HarmonyPatch(typeof(GameNetworkManager))]
internal class NetworkManagerPatch {
    [HarmonyPostfix]
    [HarmonyPatch("StartDisconnect")]
    public static void PlayerLeave() {
        if (!MCEConfig.Default.SYNC_TO_CLIENTS)
            return;

       // MCEConfig.RevertSync();
    }
}