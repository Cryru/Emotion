using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Components
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// The base for object components.
    /// </summary>
    public class Component
    {
        #region "Variables"
        /// <summary>
        /// Higher priority components are updated first.
        /// </summary>
        public int Priority = 0;

        /// <summary>
        /// The object this component belongs to.
        /// </summary>
        GameObject Object;
        #endregion

        /// <summary>
        /// Checks if dependant components have been attached.
        /// </summary>
        /// <returns>True if yes, false if no.</returns>
        public virtual bool CheckDependencies()
        {
            return true;
        }
        /// <summary>
        /// Is run every tick.
        /// </summary>
        public virtual int Update()
        {
            return 0;
        }
        /// <summary>
        /// Is run every frame.
        /// </summary>
        public virtual int Draw()
        {
            return 0;
        }
    }
}
