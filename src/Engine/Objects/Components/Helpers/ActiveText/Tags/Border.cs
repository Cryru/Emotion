using Microsoft.Xna.Framework;
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
    /// Renders a border around the text in the color specified by data.
    /// </summary>
    class Border : Tag
    {
        public Border(string Data) : base(Data)
        {
        }

        public override CharData Effect(CharData c, DrawData d)
        {
            Microsoft.Xna.Framework.Color borderColor = new Microsoft.Xna.Framework.Color().fromString(Data);

            Context.ink.DrawString(c.Font, c.Content, new Vector2(d.X + 1, d.Y), borderColor);
            Context.ink.DrawString(c.Font, c.Content, new Vector2(d.X, d.Y + 1), borderColor);
            Context.ink.DrawString(c.Font, c.Content, new Vector2(d.X - 1, d.Y), borderColor);
            Context.ink.DrawString(c.Font, c.Content, new Vector2(d.X, d.Y - 1), borderColor);
            return c;
        }
    }
}
