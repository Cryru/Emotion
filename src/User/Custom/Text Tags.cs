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
    /// Creates tag objects from identifiers.
    /// </summary>
    public partial class TagFactory
    {
        /*
         * Here is where you add your text tag objects to the association dictionary.
         * To add your object it must implement the "Tag" (SoulEngine.Objects.Components.Helpers)
         * abstract class. Then you simply write TagDict.Add({keyword}, typeof({object}));
         * in the Usertags() function below, replacing {keyword} with the tag's word (ex. <a> would be "a")
         * and {object} with your object (ex. Tags.Color).
         */

        /// <summary>
        /// Adds user defined tags to the tag dictionary.
        /// </summary>
        private static void Usertags()
        {

        }
    }
}
