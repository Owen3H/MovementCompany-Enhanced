using GameNetcodeStuff;
using HarmonyLib;
using MovementCompany.Core;
using System.Collections;
using UnityEngine;

namespace MovementCompany.Patches {
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerPatch {
        [HarmonyPostfix]
        [HarmonyPatch("PlayerJump")]
        public static IEnumerator PlayerJumpPatch(IEnumerator __result) {
            while (__result.MoveNext()) {
                var cur = __result.Current;
                if (cur is not WaitForSeconds) {
                    yield return cur;
                }
            }
        }

        //[HarmonyPostfix]
        //[HarmonyPatch("SpawnPlayerAnimation")]
        //public static void SpawnPlayerPatch() {

        //}
    }
}