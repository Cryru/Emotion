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
    class Color : Tag
    {
        public Color(string Data) : base(Data)
        {
        }

        public override CharData Effect(CharData c, DrawData d)
        {
            c.Color = new Microsoft.Xna.Framework.Color().fromString(Data);
            return c;
        }
    }
}
