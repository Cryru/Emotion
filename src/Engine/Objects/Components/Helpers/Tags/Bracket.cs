using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.Helpers.Tags
{
    class Bracket : Tag
    {
        public Bracket(string Data, int Start, int? End) : base(Data, Start, End)
        {
        }

        public override CharData onDuration(CharData c)
        {
            return c;
        }

        public override CharData onEnd(CharData c)
        {
            c.Content += "]";
            return c;
        }

        public override CharData onStart(CharData c)
        {
            c.Content = "[" + c.Content;
            return c;
        }
    }
}
