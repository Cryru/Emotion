using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.Helpers
{
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
