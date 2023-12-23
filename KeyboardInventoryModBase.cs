using System;
using BackendIain.LC.KeyboardInventory.Patches;
using BackendIain.LC.KeyboardInventory.UI;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;
using UnityEngine;

namespace BackendIain.LC.KeyboardInventory
{
    [BepInPlugin(ModGuid, ModName, ModVersion)]
    public class KeyboardInventoryModBase : BaseUnityPlugin
    {
        internal const string ModGuid = "BackendIain.LC.KeyboardInventory";
        internal const string ModName = "Keyboard Inventory";
        internal const string ModVersion = "0.1.0";
    
        private readonly Harmony _harmony = new Harmony(ModGuid);
        internal static KeyboardInventoryModBase Instance;
        internal static ManualLogSource _mls;
        
        // Host
        internal static bool _isHost;
        
        // Settings
        private static ConfigEntry<bool> _enableNumKeyInventoryBindings;
        
        // GUI
        internal static GUILoader _myGUI;
        private static bool _hasGUISynced = false;

        // Player
        internal static PlayerControllerB _playerRef;

        /// <summary>
        ///     Determines whether the module custom inventory keyboard bindings are active or not
        /// </summary>
        internal static ConfigEntry<bool> EnableNumKeyInventoryBindings => _enableNumKeyInventoryBindings;

        #region Lifecycle Private
        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            _mls = BepInEx.Logging.Logger.CreateLogSource(ModGuid);
            _mls.LogInfo($"Loaded {ModName}. Patching...");
            _harmony.PatchAll(typeof(KeyboardInventoryModBase));
            _harmony.PatchAll(typeof(PlayerControllerBPatch));

            var gameObj = new GameObject(nameof(GUILoader));
            DontDestroyOnLoad(gameObj);
            gameObj.hideFlags = HideFlags.HideAndDontSave;
            gameObj.AddComponent<GUILoader>();
            
            _myGUI = gameObj.GetComponent<GUILoader>();
            SetBindings();
            SetGUIVars();
            SetupKeyBindings();
        }
        
        private void Update()
        {
            // Nothing to handle on update frame at the moment
        }
        
        private void SetBindings()
        {
            _enableNumKeyInventoryBindings = Config.Bind("General Settings", "Enable num key inventory bindings?", false, "Enable num key inventory bindings?");
        }
        
        private void SetGUIVars()
        {
            // load from config on start

            // bools
            _myGUI.GuiEnableNumKeyInventoryBindings = _enableNumKeyInventoryBindings.Value;
        }

        internal void UpdateCFGVarsFromGUI()
        {
            if(!_hasGUISynced)
                SetGUIVars();
            
            // bools
            _enableNumKeyInventoryBindings.Value = _myGUI.GuiEnableNumKeyInventoryBindings;
        }
        #endregion
        
        #region Key Bindings
        /// <summary>
        ///     Setups key binding events
        /// </summary>
        private void SetupKeyBindings()
        {
            var root = GetComponent<GUILoader>();
        }
        #endregion

        #region Patches
        [HarmonyPatch(typeof(RoundManager), "Start")]
        [HarmonyPrefix]
        private static void SetIsHost()
        {
            _mls.LogInfo("Host Status: " + RoundManager.Instance.NetworkManager.IsHost);
            _isHost = RoundManager.Instance.NetworkManager.IsHost;
        }
        #endregion
    }
}
