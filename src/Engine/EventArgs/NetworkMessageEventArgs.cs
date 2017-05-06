using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Input;
using SoulEngine.Enums;

namespace SoulEngine.Events
{
    public class NetworkMessageEventArgs : EventArgs
    {
        public string Message;
        public string MessageType;

        public NetworkMessageEventArgs(string Message, string MessageType)
        {
            this.Message = Message;
            this.MessageType = MessageType;
        }
    }
}
