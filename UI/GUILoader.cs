using BepInEx.Configuration;
using BepInEx.Logging;
using UnityEngine;

namespace Jmx.LC.KeyboardInventory.UI
{
    internal class GUILoader : MonoBehaviour
    {
        #region Key Presses
        private KeyboardShortcut _openCloseMenu;
        internal bool _wasKeyDown;
        #endregion
        
        #region UI/Menu
        private bool _isMenuOpen;

        private int _toolbarInt = 0;
        private string[] _toolbarStrings = { "General Settings" };

        private int MENUWIDTH = 600;
        private int MENUHEIGHT = 800;
        private int MENUX;
        private int MENUY;
        private int ITEMWIDTH = 300;
        private int CENTERX;
        
        private GUIStyle _menuStyle;
        private GUIStyle _buttonStyle;
        private GUIStyle _labelStyle;
        private GUIStyle _toggleStyle;
        private GUIStyle _hScrollStyle;
        #endregion
        
        internal ManualLogSource _mls;

        #region Public Values
        // all vars I'd like my "backend" to access
        public bool GuiEnableNumKeyInventoryBindings;
        public bool GuiIsHost;
        #endregion

        // Private Methods
        #region Lifecycle Methods Private
        /// <summary>
        ///     Fires whenever the class instance is awakened
        /// </summary>
        private void Awake()
        {
            _mls = BepInEx.Logging.Logger.CreateLogSource(KeyboardInventoryModBase.ModGuid);
            _mls.LogInfo("GUILoader loaded.");      
            _openCloseMenu = new KeyboardShortcut(KeyCode.KeypadMultiply);
            _isMenuOpen = false;
            // this isn't pygame.. only need the screenwidth and height
            MENUX = (Screen.width / 2); //- (MENUWIDTH / 2);
            MENUY = (Screen.height / 2); // - (MENUHEIGHT / 2);
            CENTERX = MENUX + ((MENUWIDTH / 2) - (ITEMWIDTH / 2));

            _mls.LogInfo($"MENUX: {MENUX}, MENUY: {MENUY}, CENTERX: {CENTERX}, ITEMWIDTH: {ITEMWIDTH}");   
        }
        #endregion

        #region Menu
        /// <summary>
        ///     Inits the menu
        /// </summary>
        private void IntitializeMenu()
        {
            if (_menuStyle != null) 
                return;
            
            _mls.LogInfo("Initializing menu...");
            _menuStyle = new GUIStyle(GUI.skin.box);
            _buttonStyle = new GUIStyle(GUI.skin.button);
            _labelStyle = new GUIStyle(GUI.skin.label);
            _toggleStyle = new GUIStyle(GUI.skin.toggle);
            _hScrollStyle = new GUIStyle(GUI.skin.horizontalSlider);

            _menuStyle.normal.textColor = Color.white;
            _menuStyle.normal.background = MakeTex(2, 2, new Color(0.01f, 0.01f, 0.1f, .9f));
            _menuStyle.fontSize = 18;
            _menuStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

            _buttonStyle.normal.textColor = Color.white;
            _buttonStyle.fontSize = 18;

            _labelStyle.normal.textColor = Color.white;
            _labelStyle.normal.background = MakeTex(2, 2, new Color(0.01f, 0.01f, 0.1f, .9f));
            _labelStyle.fontSize = 18;
            _labelStyle.alignment = TextAnchor.MiddleCenter;
            _labelStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;

            _toggleStyle.normal.textColor = Color.white;
            _toggleStyle.fontSize = 18;

            _hScrollStyle.normal.textColor = Color.white;
            _hScrollStyle.normal.background = MakeTex(2, 2, new Color(0.0f, 0.0f, 0.2f, .9f));
            _hScrollStyle.normal.background.hideFlags = HideFlags.HideAndDontSave;
        }
        #endregion
        
        #region Utils
        /// <summary>
        ///     Makes a texture of the given dimensions and colour
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        private Texture2D MakeTex(int width, int height, Color col)
        {
            Color[] pix = new Color[width * height];
            for (int i = 0; i < pix.Length; ++i)
            {
                pix[i] = col;
            }
            Texture2D result = new Texture2D(width, height);
            result.SetPixels(pix);
            result.Apply();
            return result;
        }
        #endregion

        // Public Methods
        #region Lifecycle Methods Public
        public void OnDestroy()
        {
            _mls.LogInfo("The GUILoader was destroyed :(");
        }

        public void Update()
        {
            // Much better than onPressed
            // removes jitter, ensures menu always toggles when key is released
            if (_openCloseMenu.IsDown() && !_wasKeyDown)
                _wasKeyDown = true;

            if (!_openCloseMenu.IsUp() || !_wasKeyDown)
                return;

            _mls.LogInfo("Update - Menu Is Up and Key Was Not Down");

            _wasKeyDown = false;
            _isMenuOpen = !_isMenuOpen;
                
            if (_isMenuOpen)
            {
                _mls.LogInfo("Menu was open");

                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.Confined;
                return;
            }
            
            _mls.LogInfo("Menu was not open");
                
            Cursor.visible = false;
            //Cursor.lockState = CursorLockMode.Locked;
        }
        #endregion

        #region GUI
        public void OnGUI()
        {
            _mls.LogInfo($"OnGui(), GuiIsHost: {GuiIsHost}, MenuStyle: {_menuStyle != null}, isMenuOpen: {_isMenuOpen}");
            
            // if (!GuiIsHost)
            //     return;
            
            if (_menuStyle == null)
                IntitializeMenu();

            if (!_isMenuOpen)
                return;

            GUI.Box(new Rect(MENUX, MENUY, MENUWIDTH, MENUHEIGHT), "Inventory Keyboard Bindings", _menuStyle);
            _toolbarInt = GUI.Toolbar(new Rect(MENUX, MENUY - 30, MENUWIDTH, 30), _toolbarInt, _toolbarStrings, _buttonStyle);

            switch(_toolbarInt)
            {
                case 0:
                    // General Settings
                    /*
                     * List of needs:
                     * Determines whether the mod is enabled
                     */
                    // appears I'll need a label for these
                    // spring speed
                    GuiEnableNumKeyInventoryBindings = GUI.Toggle(new Rect(CENTERX, MENUY + 440, ITEMWIDTH, 30), GuiEnableNumKeyInventoryBindings, "Enable num key inventory bindings?", _toggleStyle);
                    break;
            }
        }
        #endregion
    }
}