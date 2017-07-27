using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.Helpers.Tags
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Colors the character in the color specified in the data.
    /// </summary>
    class onClick : Tag
    {
        public onClick(string Data) : base(Data)
        {

        }

        public override CharData Effect(CharData c, DrawData d)
        {
            c.clickEvent = true;
            c.clickData = Data;
            return c;
        }
    }
}
