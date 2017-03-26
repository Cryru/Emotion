using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Objects.Components.Helpers.Tags
{
    class Border : Tag
    {
        public Border(string Data) : base(Data)
        {
        }

        public override CharData Effect(CharData c)
        {
            Microsoft.Xna.Framework.Color borderColor = new Microsoft.Xna.Framework.Color().fromString(Data);

            Context.ink.DrawString(c.Font, c.Content, new Vector2(c.offsetX + 1, c.offsetY), borderColor);
            Context.ink.DrawString(c.Font, c.Content, new Vector2(c.offsetX, c.offsetY + 1), borderColor);
            Context.ink.DrawString(c.Font, c.Content, new Vector2(c.offsetX - 1, c.offsetY), borderColor);
            Context.ink.DrawString(c.Font, c.Content, new Vector2(c.offsetX, c.offsetY - 1), borderColor);
            return c;
        }
    }
}
