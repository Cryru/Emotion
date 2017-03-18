using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.Helpers.Tags
{
    class Bracket : Tag
    {
        public Bracket(string Data, int Start, int End, bool Empty = false) : base(Data, Start, End, Empty)
        {
        }

        public override CharData onDuration(CharData c)
        {
            return c;
        }

        public override CharData onEnd(CharData c)
        {
            c.Content += Data;
            return c;
        }

        public override CharData onStart(CharData c)
        {
            c.Content = Data + c.Content;
            return c;
        }
    }
}
