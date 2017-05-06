using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulEngine.Events
{
    public class SoulUpdateEventArgs : EventArgs
    {
        public GameTime updateTime;

        public SoulUpdateEventArgs(GameTime updateTime)
        {
            this.updateTime = updateTime;
        }
    }
}
