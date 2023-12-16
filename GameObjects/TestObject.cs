using System.Collections.Generic;
using System.Reflection;
using BepInEx.Configuration;
using BepInEx.Logging;
using GameNetcodeStuff;
using UnityEngine;

namespace Jmx.LC.KeyboardInventory.GameObjects
{
    public class TestObject : MonoBehaviour
    {
        private static readonly Dictionary<string, MethodInfo> MethodCache = new();
        
        internal static TestObject Instance;
        internal static ManualLogSource _mls;
        
        #region Key Presses
        private KeyboardShortcut _testShortcut;
        internal bool _wasTestKeyDown;

        private KeyboardShortcut _alpha1Shortcut;
        internal bool _wasAlpha1KeyDown;
        
        private KeyboardShortcut _alpha2Shortcut;
        internal bool _wasAlpha2KeyDown;

        private bool _isDrunk;
        #endregion
        
        private static object InvokePrivateMethod(PlayerControllerB instance, string methodName, object[] parameters = null)
        {
            MethodCache.TryGetValue(methodName, out var method);
            method ??= typeof(PlayerControllerB).GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance);

            MethodCache[methodName] = method;
            return method?.Invoke(instance, parameters);
        }

        void Start()
        {

        }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            
            _mls = BepInEx.Logging.Logger.CreateLogSource(nameof(TestObject));
            _mls.LogInfo($"Loaded {nameof(TestObject)}...");

            _testShortcut = new KeyboardShortcut(KeyCode.KeypadMinus);
            _alpha1Shortcut = new KeyboardShortcut(KeyCode.Keypad1);
            _alpha2Shortcut = new KeyboardShortcut(KeyCode.Keypad2);
            _isDrunk = false;
        }

        void Update()
        {
            // Much better than onPressed
            // removes jitter, ensures menu always toggles when key is released
            
            // Switch to inventory 1
            if (_alpha1Shortcut.IsDown() && !_wasAlpha1KeyDown)
                _wasAlpha1KeyDown = true;

            if (_alpha1Shortcut.IsUp() && _wasAlpha1KeyDown)
            {
                _mls.LogInfo("Update - Alpha9 Shortcut Is Up");
                _wasAlpha1KeyDown = false;
            }

            if (_wasAlpha1KeyDown)
            {
                _mls.LogInfo("Update - Alpha9 Shortcut Is Down");
                var switchItemParams = new object[] { 0, null };
                InvokePrivateMethod(KeyboardInventoryModBase._playerRef, "SwitchToItemSlot", switchItemParams);
            }
            
            // Switch to inventory 2
            if (_alpha2Shortcut.IsDown() && !_wasAlpha2KeyDown)
                _wasAlpha2KeyDown = true;

            if (_alpha2Shortcut.IsUp() && _wasAlpha2KeyDown)
            {
                _mls.LogInfo("Update - Alpha0 Shortcut Is Up");
                _wasAlpha2KeyDown = false;
            }

            if (_wasAlpha2KeyDown)
            {
                _mls.LogInfo("Update - Alpha0 Shortcut Is Down");
                var switchItemParams = new object[] { 1, null };
                InvokePrivateMethod(KeyboardInventoryModBase._playerRef, "SwitchToItemSlot", switchItemParams);
            }
            
            // Drunkeness
            if (_testShortcut.IsDown() && !_wasTestKeyDown)
                _wasTestKeyDown = true;

            if (!_testShortcut.IsUp() || !_wasTestKeyDown)
                return;
            
            _mls.LogInfo("Update - Test Shortcut Is Up and Key Was Not Down");
            _wasTestKeyDown = false;
            _isDrunk = !_isDrunk;

            if (_isDrunk)
            {
                KeyboardInventoryModBase._playerRef.drunkness = 100f;
                KeyboardInventoryModBase._mls.LogWarning("YOU ARE FUCKING SHIT FACED");
                return;
            }
            
            KeyboardInventoryModBase._playerRef.drunkness = 0f;
            _mls.LogInfo("You are sober :(");
        }
    }
}