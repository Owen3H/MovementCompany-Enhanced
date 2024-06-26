using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;

using MovementCompanyEnhanced.Component;
using MovementCompanyEnhanced.Core;

using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

namespace MovementCompanyEnhanced.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal class PlayerControllerPatch {
    internal static CustomMovement movementScript;

    //static bool crouchHeld = false;

    static bool removeFirstDelay => MCEConfig.Instance.REMOVE_FIRST_JUMP_DELAY;
    static bool removeSecondDelay => MCEConfig.Instance.REMOVE_SECOND_JUMP_DELAY;

    static ConfigEntry<bool> HoldToCrouch => Plugin.Config.HOLD_TO_CROUCH;
    static InputActionAsset Actions => IngamePlayerSettings.Instance.playerInput.actions;

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
        if (MCEConfig.Default.FALL_DAMAGE_ENABLED) {
            damageNumber = Mathf.Clamp(Mathf.RoundToInt(MCEConfig.Instance.FALL_DAMAGE), 0, 100);
            return true;
        }

        return !fallDamage;
    }

    //[HarmonyPrefix]
    //[HarmonyPatch("Update")]
    //public static void FixSprintToCrouch(PlayerControllerB __instance, ref bool ___isWalking) {

    //}

    [HarmonyPrefix]
    [HarmonyPatch("OnEnable")]
    public static void CrouchHold() {
        if (HoldToCrouch == null) return;

        try {
            InputAction action = Actions.FindAction("Crouch", true);
            RegisterActionCancel(action, CrouchCancelled);
        } catch (Exception e) {
            Plugin.Logger.LogError(e);
        }
    }

    public static void RegisterActionCancel(InputAction action, Action<CallbackContext> callback, bool unregister = false) {
        if (unregister) {
            action.canceled -= callback;
            return;
        }

        action.canceled += callback;
    }

    static void CrouchCancelled(CallbackContext _) {
        movementScript.player.Crouch(false);
        //crouchHeld = false;
    }

    //// Custom crouch condition
    //static void CrouchPerformed() {
    //    if (!HoldToCrouch.Value || movementScript.player.isCrouching) return;

    //    movementScript.player.Crouch(true);
    //    Plugin.Logger.LogDebug("HoldToCrouch: Crouch performed");
    //}

    //[HarmonyPrefix]
    //[HarmonyPatch("Crouch_performed")]
    //public static bool OnCrouch() {
    //    CrouchPerformed();

    //    // Disable crouch toggle
    //    return false;
    //}

    //[HarmonyTranspiler]
    //[HarmonyPatch("Crouch_performed")]
    //public static IEnumerable<CodeInstruction> ReplaceCrouchToggle(IEnumerable<CodeInstruction> instructions) {
    //    List<CodeInstruction> patchedInstructions = instructions.ToList();

    //    for (int i = 0; i < patchedInstructions.Count; i++)  {
    //        var cur = patchedInstructions[i];

    //        Plugin.Logger.LogDebug($"Code: {cur.opcode}\nOperand: {cur.operand}");
    //    }

    //    return patchedInstructions;
    //}
}