using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace SoulEngine.Events
{
    public class ScrollbarEventArgs : EventArgs
    {
        public int Value;

        public ScrollbarEventArgs(int Value)
        {
            this.Value = Value;
        }
    }
}
