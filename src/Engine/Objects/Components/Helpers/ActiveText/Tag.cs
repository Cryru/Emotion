using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.Helpers
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// An active text markup tag.
    /// </summary>
    public abstract class Tag
    {
        #region "Declarations"
        /// <summary>
        /// The tag's metadata.
        /// </summary>
        public string Data
        {
            get
            {
                return data;
            }
        }
        private string data;
        #endregion

        #region "Functions"
        /// <summary>
        /// The character data mutation that will be applied on each character while the tag is considered active.
        /// </summary>
        public abstract CharData Effect(CharData c);
        #endregion

        /// <summary>
        /// Initializes a new tag.
        /// </summary>
        /// <param name="Data">The tag's metadata.</param>
        public Tag(string Data)
        {
            data = Data;
        }
    }
}
