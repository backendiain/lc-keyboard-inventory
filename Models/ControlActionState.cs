using BepInEx.Configuration;

namespace Jmx.LC.KeyboardInventory.Models
{
    internal struct ControlActionState
    {
        /// <summary>
        ///     The shortcut key associated with the input
        /// </summary>
        public KeyboardShortcut Key { get; internal set; }

        /// <summary>
        ///     Determines whether the key ended down on the last frame
        /// </summary>
        public bool EndedDown { get; internal set; }

        /// <summary>
        ///     
        /// </summary>
        public string Description { get; internal set; }

        /// <summary>
        ///     The inventory slot associated with the key (optional)
        /// </summary>
        public int ItemSlotIndex { get; internal set; }
    }
}
