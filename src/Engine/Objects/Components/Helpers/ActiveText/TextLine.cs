using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
    /// A cached list of CharData.
    /// </summary>
    public class TextLine
    {
        public List<CharData> Chars;
        public bool Manual;
        public int SpaceOnLine;

        public TextLine(List<CharData> Chars, int SpaceOnLine, bool Manual = false)
        {
            this.Chars = Chars;
            this.Manual = Manual;
            this.SpaceOnLine = SpaceOnLine;
        }

        public override string ToString()
        {
            string temp = "";

            for (int i = 0; i < Chars.Count; i++)
            {
                temp += Chars[i].Content;
            }

            return temp;
        }
    }
}
