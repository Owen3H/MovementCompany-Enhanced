using GameNetcodeStuff;
using HarmonyLib;
using MovementCompanyEnhanced.Component;
using MovementCompanyEnhanced.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MovementCompanyEnhanced.Patches {
    [HarmonyPatch(typeof(PlayerControllerB))]
    internal class PlayerControllerPatch {
        internal static CustomMovement movementScript;

        static bool removeFirstDelay => Config.Instance.REMOVE_FIRST_JUMP_DELAY;
        static bool removeSecondDelay => Config.Instance.REMOVE_SECOND_JUMP_DELAY;

        [HarmonyPostfix]
        [HarmonyPatch("ConnectClientToPlayerObject")]
        public static void InitializeLocalPlayer() {
            if (Config.IsHost) {
                Config.MessageManager.RegisterNamedMessageHandler("MCE_OnRequestConfigSync", Config.OnRequestSync);
                Config.Synced = true;

                return;
            }

            Config.Synced = false;
            Config.MessageManager.RegisterNamedMessageHandler("MCE_OnReceiveConfigSync", Config.OnReceiveSync);
            Config.RequestSync();
        }

        [HarmonyPostfix]
        [HarmonyPatch("SpawnPlayerAnimation")]
        public static void GiveMovementScript(PlayerControllerB __instance) {
            if (!__instance) return;
            if (__instance.GetComponentInChildren<CustomMovement>() != null) {
                return;
            }

            if (__instance.IsOwner && __instance.isPlayerControlled) {
                movementScript = __instance.gameObject.AddComponent<CustomMovement>();
                movementScript.player = __instance;

                Plugin.Logger.LogInfo("Client player was given the movement script.");
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch("PlayerJump", MethodType.Enumerator)]
        public static IEnumerable<CodeInstruction> RemoveJumpDelay(IEnumerable<CodeInstruction> instructions) {
            List<CodeInstruction> patchedInstructions = instructions.ToList();

            if (!removeFirstDelay && !removeSecondDelay) {
                return patchedInstructions;
            }

            int wfsCount = 0;

            for (int i = 0; i < patchedInstructions.Count; i++) {
                CodeInstruction curInstruction = patchedInstructions[i];
                if (curInstruction.opcode != OpCodes.Newobj) 
                    continue;

                #region Replace `new WaitForSeconds(float32)` with `null`
                var op = curInstruction.operand as ConstructorInfo;
                if (op?.DeclaringType == typeof(WaitForSeconds)) {
                    if (wfsCount == 0 && !removeFirstDelay)
                        continue;

                    if (wfsCount == 1 && !removeSecondDelay)
                        continue;

                    // Equivalent to `yield return null`
                    patchedInstructions[i] = new CodeInstruction(OpCodes.Ldnull);
                    patchedInstructions.RemoveAt(i-1);
                    i--;

                    wfsCount++;
                }
                #endregion
            }

            return patchedInstructions;
        }

        [HarmonyPrefix]
        [HarmonyPatch("DamagePlayer")]
        public static bool OverrideFallDamage(ref int damageNumber, ref bool fallDamage) {
            if (Config.Instance.FALL_DAMAGE_ENABLED) {
                damageNumber = Mathf.Clamp(Mathf.RoundToInt(Config.Instance.FALL_DAMAGE), 0, 100);
                return true;
            }

            return !fallDamage;
        }

        [HarmonyPrefix]
        [HarmonyPatch("Crouch_performed")]
        public static bool DisableCrouchToggle() {
            if (Config.Default.HOLD_TO_CROUCH) {
                movementScript.player.Crouch(true);
                return false;
            }

            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("OnEnable")]
        public static void CrouchHold() {
            if (!Config.Default.HOLD_TO_CROUCH) return;

            try {
                InputActionAsset actions = IngamePlayerSettings.Instance.playerInput.actions;
                actions.FindAction("Crouch", false).canceled += CrouchCanceled;
            } catch(Exception e) {
                Plugin.Logger.LogError($"An error occurred patching crouch hold!\n{e}");
            }
        }

        static void CrouchCanceled(InputAction.CallbackContext _) {
            movementScript.player.Crouch(false);
        }
    }
}