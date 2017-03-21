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

        public CharData(Color Color = new Color())
        {
            this.Color = Color;
        }
    }
}
