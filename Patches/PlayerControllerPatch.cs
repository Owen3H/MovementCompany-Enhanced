using GameNetcodeStuff;
using HarmonyLib;
using MovementCompanyEnhanced.Component;
using MovementCompanyEnhanced.Core;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace MovementCompanyEnhanced.Patches {
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerPatch {
        [HarmonyTranspiler]
        [HarmonyPatch("PlayerJump", MethodType.Enumerator)]
        public static IEnumerable<CodeInstruction> RemoveJumpDelay(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> patchedInstructions = instructions.ToList();

            for (int i = 0; i < patchedInstructions.Count; i++) {
                CodeInstruction curInstruction = patchedInstructions[i];
                if (curInstruction.opcode != OpCodes.Newobj) 
                    continue;

                #region Replace `new WaitForSeconds(float32)` with `null`
                var op = curInstruction.operand as ConstructorInfo;
                if (op?.DeclaringType == typeof(WaitForSeconds)) {
                    // Equivalent to `yield return null`
                    patchedInstructions[i] = new CodeInstruction(OpCodes.Ldnull);
                    patchedInstructions.RemoveAt(i-1);
                    i--;
                }
                #endregion
            }

            return patchedInstructions;
        }

        [HarmonyPostfix]
        [HarmonyPatch("SpawnPlayerAnimation")]
        public static void GiveMovementScript(PlayerControllerB __instance) {
            // Shouldn't ever happen here but just in-case.
            if (__instance == null)
                return;

            if (__instance.GetComponentInChildren<CustomMovement>() != null) 
                return;

            if (__instance.IsOwner && __instance.isPlayerControlled) {
                __instance.gameObject.AddComponent<CustomMovement>().player = __instance;
                Plugin.Logger.LogInfo("Client player was given the movement script.");
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch("DamagePlayer")]
        public static bool OverrideFallDamage(ref int damageNumber, ref bool fallDamage) {
            if (Plugin.Config.FALL_DAMAGE_ENABLED) {
                fallDamage = false;
                damageNumber = Mathf.Clamp(Mathf.RoundToInt(Plugin.Config.FALL_DAMAGE), 0, 100);

                return true;
            }

            return !fallDamage;
        }
    }
}