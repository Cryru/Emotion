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
    /// Tag data captured while cleaning tags from the text.
    /// </summary>
    public class TagData
    {
        #region "Declarations"
        /// <summary>
        /// The tag's information.
        /// </summary>
        public string Information;
        /// <summary>
        /// The character position at which the tag is located in the tag free text.
        /// </summary>
        public int StartPosition;
        /// <summary>
        /// The number of characters between this tag, and the previous one.
        /// </summary>
        public int CharactersFromPreviousTag;
        /// <summary>
        /// Whether to skip this data when processing.
        /// </summary>
        public bool Skip = false;
        #endregion

        /// <summary>
        /// Initializes a new tag data instance.
        /// </summary>
        /// <param name="Information">The tag's information..</param>
        /// <param name="StartPosition">The character position at which the tag is located in the tag free text.</param>
        /// <param name="CharactersFromPreviousTag">The number of characters between this tag, and the previous one.</param>
        public TagData(string Information, int StartPosition, int CharactersFromPreviousTag)
        {
            this.Information = Information;
            this.StartPosition = StartPosition;
            this.CharactersFromPreviousTag = CharactersFromPreviousTag;
        }
    }
}
