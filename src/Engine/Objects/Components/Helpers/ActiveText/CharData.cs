using Microsoft.Xna.Framework;
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

        public CharData(string Content, Color Color)
        {
            this.Content = Content;
            this.Color = Color;
        }
    }
}
