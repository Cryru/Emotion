using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.Helpers.Tags
{
    class Color : Tag
    {
        public Color(string Data, int Start, int End, bool Empty = false) : base(Data, Start, End, Empty)
        {
        }

        public override CharData onDuration(CharData c)
        {
            c.Color = new Microsoft.Xna.Framework.Color().fromString(Data);
            return c;
        }

        public override CharData onEnd(CharData c)
        {
            return c;
        }

        public override CharData onStart(CharData c)
        {
            return c;
        }
    }
}
