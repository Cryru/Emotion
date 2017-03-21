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
        /// <summary>
        /// A dictionary of tag identifiers and tag objects.
        /// </summary>
        private static Dictionary<string, Type> TagDict;

        /// <summary>
        /// Initializes the TagFactory.
        /// </summary>
        public static void Initialize()
        {
            //Initialize and add systemtic tags.
            TagDict = new Dictionary<string, Type>();
            TagDict.Add("color", typeof(Tags.Color));

            //Add user tags to the dictionary.
            Usertags();
        }

        /// <summary>
        /// Builds a new tag based on the provided data.
        /// </summary>
        /// <param name="Identifier">The tag's identifier.</param>
        /// <param name="Data">The tag's metadata.</param>
        /// <param name="Start">The character position from which the tag is considered active.</param>
        /// <param name="End">The character position at which the tag is no longer considered active.</param>
        /// <param name="Empty">Whether the tag is immediately closed after opened with no chars between.</param>
        /// <returns></returns>
        public static Tag Build(string Identifier, string Data, int Start, int End, bool Empty = false)
        {
            //Check if the dictionary has an entry for the provided identifier.
            if (TagDict.ContainsKey(Identifier))
                return (Tag)Activator.CreateInstance(TagDict[Identifier], new object[] { Data, Start, End, Empty });
            else
                return null;
        }
    }
}
