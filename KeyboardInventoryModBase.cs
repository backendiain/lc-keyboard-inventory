using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using HarmonyLib;

namespace Jmx.LC.KeyboardInventory
{
    [BepInPlugin(modGuid, modName, modVersion)]
    public class KeyboardInventoryModBase : BaseUnityPlugin
    {
        private const string modGuid = "Jmx.LC.KeyboardInventory";
        private const string modName = "Tweaks keybinds so that the number keys will change the current item held by the player.";
        private const string modVersion = "0.1.0";
    
        private readonly Harmony _harmony = new Harmony(modGuid);
        private static KeyboardInventoryModBase Instance;
        internal ManualLogSource _mls;
        
        // Host
        internal static bool _isHost;
        
        // Settings
        private static ConfigEntry<bool> _enableNumKeyInventoryBindings;
        
        // GUI
        internal static GUILoader _myGUI;
        private static bool _hasGUISynced = false;

        // Player
        internal static PlayerControllerB _playerRef;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            _mls = BepInEx.Logging.Logger.CreateLogSource(modGuid);
            _mls.LogInfo($"Testing the {modName} mod has awakened with a manual log source!");
            _harmony.PatchAll(typeof(KeyboardInventoryModBase));

            // Plugin startup logic
            Logger.LogInfo($"Plugin {modGuid} is loaded!");
        }
    }
}
