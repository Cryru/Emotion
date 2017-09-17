using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace SoulEngine.Events
{
    public class MouseMoveEventArgs : EventArgs
    {
        public Vector2 From;
        public Vector2 To;

        public MouseMoveEventArgs(Vector2 From, Vector2 To)
        {
            this.From = From;
            this.To = To;
        }
    }
}
