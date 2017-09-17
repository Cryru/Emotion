using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using SoulEngine.Enums;

namespace SoulEngine.Events
{
    public class NetworkEventArgs : EventArgs
    {
        public NetworkStatus Status;

        public NetworkEventArgs(NetworkStatus Status)
        {
            this.Status = Status;
        }
    }
}
