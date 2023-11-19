using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace MovementCompany.Patches {
    [HarmonyPatch(typeof(PlayerControllerB))]
    class PlayerControllerPatch {
        [HarmonyPatch("PlayerJump")]
        public static IEnumerator Postfix(IEnumerator __result) {
            while (__result.MoveNext()) {
                var cur = __result.Current;
                if (cur is not WaitForSeconds) {
                    yield return cur;
                }
            }
        }
    }
}