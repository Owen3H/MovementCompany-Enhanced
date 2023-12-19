using HarmonyLib;
using MovementCompanyEnhanced.Core;

namespace MovementCompanyEnhanced.Patches {
    [HarmonyPatch(typeof(GameNetworkManager))]
    internal class NetworkManagerPatch {
        [HarmonyPostfix]
        [HarmonyPatch("StartDisconnect")]
        public static void PlayerLeave() {
            if (!Config.Default.FORCE_HOST_CONFIG)
                return;

            Config.RevertSync();
        }
    }
}
