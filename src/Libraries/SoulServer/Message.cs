using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulServer
{
    /// <summary>
    /// A message to be sent to the server.
    /// </summary>
    public class ServerMessage
    {
        public string Type;
        public string Data;
        public long Timestamp;

        public ServerMessage(string Message)
        {
            //Check if the full message is present.
            if (Message.Split('\0').Length != 3) return;

            Type = Message.Split('\0')[0];
            Data = Message.Split('\0')[1];
            if (!long.TryParse(Message.Split('\0')[2], out Timestamp)) return;
        }

        public ServerMessage(string Type, string Data)
        {
            this.Type = Type;
            this.Data = Data;
            Timestamp = DateTime.Now.Ticks;
        }


        public ServerMessage(string Type, string Data, long Timestamp)
        {
            this.Type = Type;
            this.Data = Data;
            this.Timestamp = Timestamp;
        }

        public override string ToString()
        {
            return Type + "\0" + Data + "\0" + Timestamp;
        }
    }

    /// <summary>
    /// Types of messages.
    /// </summary>
    public partial class MType
    {
        #region "Authentification"
        public const string AUTHENTIFICATION = "0";
        #endregion
    }
}
