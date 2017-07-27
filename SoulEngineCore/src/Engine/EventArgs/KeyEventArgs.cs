using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;

namespace SoulEngine.Events
{
    public class KeyEventArgs : EventArgs
    {
        public Keys Key;

        public KeyEventArgs(Keys Key)
        {
            this.Key = Key;
        }
    }
}
