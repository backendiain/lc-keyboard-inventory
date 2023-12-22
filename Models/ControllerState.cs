using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jmx.LC.KeyboardInventory.Models
{
    /// <summary>
    ///     A state of the control scheme used for player actions
    /// </summary>
    internal struct ControllerState
    {
        /// <summary>
        ///     The state for each bind of the control scheme
        /// </summary>
        public IEnumerable<ControlActionState> Inputs { get; set; }
    }
}
