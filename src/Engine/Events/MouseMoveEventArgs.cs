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
        public Vector2 LastPosition;

        public MouseMoveEventArgs(Vector2 LastPosition)
        {
            this.LastPosition = LastPosition;
        }
    }
}
