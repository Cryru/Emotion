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
        private static Dictionary<string, Type> TagDict;

        /// <summary>
        /// 
        /// </summary>
        public static void Initialize()
        {
            TagDict = new Dictionary<string, Type>();
            TagDict.Add("a", typeof(Tags.Bracket));

            //Add user tags to the dictionary.
            Usertags();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="Identifier"></param>
        /// <param name="Data"></param>
        /// <param name="Start"></param>
        /// <param name="End"></param>
        /// <returns></returns>
        public static Tag Build(string Identifier, string Data, int Start, int? End)
        {
            if (TagDict.ContainsKey(Identifier))
                return (Tag)Activator.CreateInstance(TagDict[Identifier], new object[] { Data, Start, End });
            else
                return null;
        }
    }
}
