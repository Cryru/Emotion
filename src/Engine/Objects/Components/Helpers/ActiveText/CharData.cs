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
    /// Render data about a single character.
    /// </summary>
    public class CharData
    {
        public Color Color;
        public string Content;
        public float offsetX;
        public float offsetY;
        public SpriteFont Font;

        public CharData(string Content, Color Color, float offsetX, float offsetY, SpriteFont Font)
        {
            this.Content = Content;
            this.Color = Color;
            this.offsetX = offsetX;
            this.offsetY = offsetY;
            this.Font = Font;
        }
    }
}
