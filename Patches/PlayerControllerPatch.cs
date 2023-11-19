using GameNetcodeStuff;
using HarmonyLib;
using System.Collections;
using UnityEngine;

namespace MovementCompany.Patches {
    [HarmonyPatch(typeof(PlayerControllerB))]
    class PlayerControllerPatch {
        [HarmonyPrefix]
        [HarmonyPatch("PlayerJump")]
        public static IEnumerator OverridePlayerJump(IEnumerable result) {
            foreach(var e in result) {
                if (e is not WaitForSeconds) {
                    yield return e;
                }
            }
        }
    }
}