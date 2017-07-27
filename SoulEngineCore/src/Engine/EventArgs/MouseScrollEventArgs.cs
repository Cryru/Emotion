using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;

namespace SoulEngine.Events
{
    public class MouseScrollEventArgs : EventArgs
    {
        public int ScrollAmount;

        public MouseScrollEventArgs(int ScrollAmount)
        {
            this.ScrollAmount = ScrollAmount;
        }
    }
}
