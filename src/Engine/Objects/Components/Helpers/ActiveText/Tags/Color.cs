using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.Helpers.Tags
{
    class Color : Tag
    {
        public Color(string Data) : base(Data)
        {
        }

        public override CharData Effect(CharData c)
        {
            c.Color = new Microsoft.Xna.Framework.Color().fromString(Data);
            return c;
        }
    }
}
