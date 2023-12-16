using GameNetcodeStuff;
using HarmonyLib;
using Jmx.LC.KeyboardInventory.GameObjects;
using UnityEngine;
using UnityEngine.Events;

namespace Jmx.LC.KeyboardInventory.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class PlayerControllerBPatch
    {
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        static void PatchStart(ref PlayerControllerB __instance)
        {
            //Add a listener to the new Event. Calls MyAction method when invoked
            if(__instance.isHostPlayerObject)
                KeyboardInventoryModBase._playerRef = __instance;
            // KeyboardInventoryModBase._playerRef.gameObject.AddComponent<TestObject>();
        }
        
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        static void PatchUpdate()
        {
            KeyboardInventoryModBase._myGUI.GuiIsHost = KeyboardInventoryModBase._isHost;
            KeyboardInventoryModBase.Instance.UpdateCFGVarsFromGUI();

            // if (Input.anyKeyDown)
            //     KeyboardInventoryModBase._mls.LogInfo($"{nameof(PlayerControllerBPatch)} Foo!");
            //
            // if (Input.anyKeyDown && Event.current.isKey)
            // {
            //     KeyboardInventoryModBase._mls.LogInfo($"{nameof(PlayerControllerBPatch)} A key was hit! KeyCode: {Event.current.keyCode}");
            // }
            //
            // if (Input.GetKeyDown(KeyCode.KeypadMinus))
            // {
            //     KeyboardInventoryModBase._mls.LogInfo($"{nameof(PlayerControllerBPatch)} KeypadMinus was hit!");
            // }
        }
    }
}