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
    /// 
    /// </summary>
    public abstract class Tag
    {
        #region "Declarations"
        /// <summary>
        /// Whether the tag is considered active.
        /// </summary>
        public bool Active;
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
        /// <summary>
        /// The character position from which the tag is considered active.
        /// </summary>
        public int Start
        {
            get
            {
                return start;
            }
        }
        private int start;
        /// <summary>
        /// The character position at which the tag is no longer considered active.
        /// </summary>
        public int End
        {
            get
            {
                return end;
            }
        }
        private int end;
        /// <summary>
        /// Whether the tag is immediately closed after opened with no chars between.
        /// </summary>
        public bool Empty
        {
            get
            {
                return empty;
            }
        }
        private bool empty;
        #endregion

        #region "Functions"
        /// <summary>
        /// The character data mutation that will be applied once the tag is considered active.
        /// </summary>
        public abstract CharData onStart(CharData c);
        /// <summary>
        /// The character data mutation that will be applied on each character while the tag is considered active.
        /// </summary>
        public abstract CharData onDuration(CharData c);
        /// <summary>
        /// The character data mutation that wll be applied on the last character of which the tag is considered active.
        /// </summary>
        public abstract CharData onEnd(CharData c);
        #endregion

        /// <summary>
        /// Initializes a new tag.
        /// </summary>
        /// <param name="Data">The tag's metadata.</param>
        /// <param name="Start">The character position from which the tag is considered active.</param>
        /// <param name="End">The character position at which the tag is no longer considered active.</param>
        /// <param name="Empty">Whether the tag is immediately closed after opened with no chars between.</param>
        public Tag(string Data, int Start, int End, bool Empty = false)
        {
            start = Start;
            end = End;
            data = Data;
            empty = Empty;
        }
    }
}
