using BepInEx.Configuration;
using GameNetcodeStuff;
using HarmonyLib;
using Jmx.LC.KeyboardInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Jmx.LC.KeyboardInventory.Patches
{
    [HarmonyPatch(typeof(PlayerControllerB))]
    public class PlayerControllerBPatch
    {
        private static readonly IDictionary<string, MethodInfo> _methodCache = new Dictionary<string, MethodInfo>();
        private static readonly object[] _backwardsParam = { false };
        private static readonly object[] _forwardsParam = { true };

        internal static ControlActionState _itemSlot1Action;
        internal static ControlActionState _itemSlot2Action;
        internal static ControlActionState _itemSlot3Action;
        internal static ControlActionState _itemSlot4Action;

        internal static ControlActionState _emote1Action;
        internal static ControlActionState _emote2Action;

        internal static ControllerState _allActions;
        internal static ControllerState _itemSlotActions;
        internal static ControllerState _emoteActions;

        #region Patches
        [HarmonyPatch("Start")]
        [HarmonyPrefix]
        public static void PatchStart(ref PlayerControllerB __instance)
        {
            if(__instance.isHostPlayerObject)
                KeyboardInventoryModBase._playerRef = __instance;

            // Inventory switch key binds
            _itemSlot1Action = new ControlActionState { Key = new KeyboardShortcut(KeyCode.Alpha1), Description = "Item slot 1", ItemSlotIndex = 0 };
            _itemSlot2Action = new ControlActionState { Key = new KeyboardShortcut(KeyCode.Alpha2), Description = "Item slot 2", ItemSlotIndex = 1 };
            _itemSlot3Action = new ControlActionState { Key = new KeyboardShortcut(KeyCode.Alpha3), Description = "Item slot 3", ItemSlotIndex = 2 };
            _itemSlot4Action = new ControlActionState { Key = new KeyboardShortcut(KeyCode.Alpha4), Description = "Item slot 4", ItemSlotIndex = 3 };

            _itemSlotActions = new ControllerState
            {
                Inputs = new[] {_itemSlot1Action, _itemSlot2Action, _itemSlot3Action, _itemSlot4Action},
            };
            
            _emote1Action = new ControlActionState { Key = new KeyboardShortcut(KeyCode.Minus), Description = "Emote 1", ItemSlotIndex = 4 };
            _emote2Action = new ControlActionState { Key = new KeyboardShortcut(KeyCode.Equals), Description = "Emote 2", ItemSlotIndex = 5 };

            _emoteActions = new ControllerState
            {
                Inputs = new[] {_emote1Action, _emote2Action},
            };
            
            _allActions = new ControllerState
            {
                Inputs = _itemSlotActions.Inputs.Concat(_emoteActions.Inputs),
            };
        }
        
        [HarmonyPatch("Update")]
        [HarmonyPrefix]
        public static void PatchUpdate(
            ref PlayerControllerB __instance,
            ref float ___timeSinceSwitchingSlots,
            ref bool ___throwingObject)
        {
            KeyboardInventoryModBase._myGUI.GuiIsHost = KeyboardInventoryModBase._isHost;
            KeyboardInventoryModBase.Instance.UpdateCFGVarsFromGUI();
            
            if (!KeyboardInventoryModBase.EnableNumKeyInventoryBindings.Value)
                return;

            if ((!__instance.IsOwner || !__instance.isPlayerControlled ||
                __instance.IsServer && !__instance.isHostPlayerObject) && !__instance.isTestingPlayer)
                return;

            // todo: It is probably better to use Unity's new input handler
            // but I noticed that the DLL used by LC end of 2023 was the legacy module
            // and I don't know much about Unity so this felt a fun way to compare and learn old/new methods
            
            // No point continuing if nothing has been pressed
            // You'll just end up scrolling back and forth between inventory slots if you continued
            if (_allActions.Inputs == null || !_allActions.Inputs.Any(x => x.Key.IsDown())) 
                return;
            
            foreach(var action in _allActions.Inputs)
                HandleActionInput(
                    action, 
                    ref __instance, 
                    ref ___timeSinceSwitchingSlots, 
                    ref ___throwingObject);
        }
        #endregion

        #region Item Slot Switching
        /// <summary>
        ///     Handles the input for a given action
        /// </summary>
        /// <param name="action"></param>
        /// <param name="__instance"></param>
        /// <param name="___timeSinceSwitchingSlots"></param>
        /// <param name="___throwingObject"></param>
        /// <returns></returns>
        private static ControlActionState HandleActionInput(
            ControlActionState action,
            ref PlayerControllerB __instance,
            ref float __timeSinceSwitchingSlots,
            ref bool __throwingObject)
        {
            if (action.Key.IsDown() && !action.EndedDown)
                action.EndedDown = true;

            if (action.Key.IsUp() && action.EndedDown)
                action.EndedDown = false;

            // Emotes
            if (action.EndedDown && _emoteActions.Inputs.Any(x => x.Key.MainKey == action.Key.MainKey))
            {
                PerformEmote(ref __instance, action.Key.MainKey == KeyCode.Minus ? 1 : 2);
                return action;
            }

            // Inventory Switching
            if (action.EndedDown && CanSwitchItem(__instance, __timeSinceSwitchingSlots, __throwingObject))
            {
                StopAllEmotes(ref __instance, __timeSinceSwitchingSlots, __throwingObject);
                SwitchItemSlot(__instance, action.ItemSlotIndex, __timeSinceSwitchingSlots, __throwingObject);
                __timeSinceSwitchingSlots = 0;
            }

            return action;
        }

        /// <summary>
        ///     Attempts to switch to the given item slot index
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="slotIndex"></param>
        /// <param name="timeSinceSwitchingSlots"></param>
        /// <param name="isThrowingObject"></param>
        /// <returns></returns>
        private static bool SwitchItemSlot(
            PlayerControllerB instance, 
            int slotIndex, 
            float timeSinceSwitchingSlots,
            bool isThrowingObject)
        {
            // Can we switch?
            if (!CanSwitchItem(instance, timeSinceSwitchingSlots, isThrowingObject) || instance.currentItemSlot == slotIndex)
                return false;

            // Netcode? Presumably this updates the clients for other party members
            var distanceToSlot = instance.currentItemSlot - slotIndex;
            var isNewSlotBeforeCurrent = distanceToSlot > 0;

            // Check whether we'd be on the same slot as before
            if(Math.Abs(distanceToSlot) == instance.ItemSlots.Length - 1)
            {
                // The flag evaluates to the value to SwitchItemSlotsServerRpc's forward param
                // vasanex mentions: "we can just skip one slot forwards/backwards here and save RPC calls."
                // They'll be correct, I just don't fully understand how one slot backwards/forwards doesn't require an RPC call
                // var parameters = isNewSlotBeforeCurrent ? _forwardsParam : _backwardsParam;
                // InvokePrivateMethod(instance, "SwitchItemSlotsServerRpc", parameters);
            }
            else
            {
                var parameters = isNewSlotBeforeCurrent ? _backwardsParam : _forwardsParam;

                do
                {
                    InvokePrivateMethod(instance, "SwitchItemSlotsServerRpc", parameters);
                    distanceToSlot += isNewSlotBeforeCurrent ? -1 : 1;
                } while (distanceToSlot != 0);
            }

            // Hop out of build mode
            ShipBuildModeManager.Instance.CancelBuildMode();
            instance.playerBodyAnimator.SetBool("GrabValidated", false);

            // Make the item switch
            InvokePrivateMethod(
                instance,
                "SwitchToItemSlot",
                new object[] { slotIndex, null }
            );

            // More netcode?
            // Let the player listen to the song of our people (panicked fumbling of items)
            if (instance.currentlyHeldObjectServer != null)
                instance.currentlyHeldObjectServer.gameObject.GetComponent<AudioSource>()
                    .PlayOneShot(instance.currentlyHeldObjectServer.itemProperties.grabSFX, 0.6f);

            return true;
        }

        /// <summary>
        ///     Determines whether an item slot switch is possible
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="timeSinceSwitchingSlots"></param>
        /// <param name="isThrowingObject"></param>
        /// <returns></returns>
        private static bool CanSwitchItem(
            PlayerControllerB instance, 
            float timeSinceSwitchingSlots,
            bool isThrowingObject)
        {
            return !(timeSinceSwitchingSlots < 0.01 || instance.inTerminalMenu || instance.isGrabbingObjectAnimation ||
                     instance.inSpecialInteractAnimation || isThrowingObject || instance.isTypingChat ||
                     instance.twoHanded || instance.activatingItem || instance.jetpackControls ||
                     instance.disablingJetpackControls);
        }

        /// <summary>
        ///     Performs the beginning emote for the provided player instance matching the given emote ID
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="emoteId"></param>
        private static void PerformEmote(
            ref PlayerControllerB __instance,
            int emoteId)
        {
            __instance.timeSinceStartingEmote = 0;
            __instance.performingEmote = true;
            __instance.playerBodyAnimator.SetInteger("emoteNumber", emoteId);
            __instance.StartPerformingEmoteServerRpc();
        }

        /// <summary>
        ///     Stops all emotes being performed by the provided instance
        /// </summary>
        /// <param name="__instance"></param>
        /// <param name="timeSinceSwitchingSlots"></param>
        /// <param name="isThrowingObject"></param>
        private static void StopAllEmotes(
            ref PlayerControllerB __instance, 
            float timeSinceSwitchingSlots, 
            bool isThrowingObject)
        {
            __instance.performingEmote = false;
            __instance.timeSinceStartingEmote = 0;
            __instance.StopPerformingEmoteServerRpc();
        }
        #endregion

        #region Utils
        /// <summary>
        ///     Invokes a private method on the PlayerControllerB instance
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="methodName"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        private static object InvokePrivateMethod(PlayerControllerB instance, string methodName, object[] parameters = null)
        {
            _methodCache.TryGetValue(methodName, out var method);
            method ??= typeof(PlayerControllerB).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            _methodCache[methodName] = method;
            return method?.Invoke(instance, parameters);
        }
        #endregion
    }
}