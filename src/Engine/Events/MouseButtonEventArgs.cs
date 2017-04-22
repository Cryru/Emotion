using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using SoulEngine.Enums;

namespace SoulEngine.Events
{
    public class MouseButtonEventArgs : EventArgs
    {
        public MouseButton Button;

        public MouseButtonEventArgs(MouseButton Button)
        {
            this.Button = Button;
        }
    }
}
