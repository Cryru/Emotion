using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soul.Engine.Legacy
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
        public static Dictionary<string, Type> TagDict;

        /// <summary>
        /// Initializes the TagFactory.
        /// </summary>
        public static void Initialize()
        {
            //Initialize and add systemic tags.
            TagDict = new Dictionary<string, Type>();
            TagDict.Add("color", typeof(Tags.ColorText));
            TagDict.Add("border", typeof(Tags.Border));
            TagDict.Add("click", typeof(Tags.onClick));
        }

        /// <summary>
        /// Builds a new tag based on the provided data.
        /// </summary>
        /// <param name="Identifier">The tag's identifier.</param>
        /// <param name="Data">The tag's metadata.</param>
        /// <returns></returns>
        public static Tag Build(string Identifier, string Data)
        {
            //Check if the dictionary has an entry for the provided identifier.
            if (TagDict.ContainsKey(Identifier))
                return (Tag)Activator.CreateInstance(TagDict[Identifier], new object[] { Data });
            else
                return null;
        }
    }
}
