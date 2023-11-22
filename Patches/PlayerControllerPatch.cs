using GameNetcodeStuff;
using HarmonyLib;
using MovementCompany.Component;
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

        [HarmonyPostfix]
        [HarmonyPatch("SpawnPlayerAnimation")]
        public static void SpawnPlayerPatch() {
            PlayerControllerB[] Players = Object.FindObjectsOfType<PlayerControllerB>();
            int PlayersAmt = Players.Length;

            for (int i = 0; i < PlayersAmt; i++) {
                PlayerControllerB player = Players[i];

                if (player == null) return;
                if (player.GetComponentInChildren<MovementScript>() != null) return;

                if (player.IsOwner && player.isPlayerControlled) {
                    player.gameObject.AddComponent<MovementScript>().player = player;
                    Plugin.Logger.LogMessage($"Gave {player.playerUsername} the movement script.");
                }
            }
        }
    }
}