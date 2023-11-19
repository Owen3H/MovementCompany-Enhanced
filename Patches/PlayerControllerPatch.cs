using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace MovementCompany.Patches {
    [HarmonyPatch(typeof(PlayerControllerB))]
    class PlayerControllerPatch {
        [HarmonyPatch("PlayerJump")]
        public static IEnumerator Postfix(IEnumerator __result) {
            foreach(var e in __result) {
                if (e is not WaitForSeconds) {
                    yield return e;
                }
            }
        }
    }
}