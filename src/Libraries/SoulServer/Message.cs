using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SoulServer
{
    public class ServerMessage
    {
        public string Type;
        public string Data;

        public ServerMessage(string Message)
        {
            Type = Message.Split('\0')[0];
            Data = Message.Split('\0')[1];
        }
        public ServerMessage(string Type, string Data)
        {
            this.Type = Type;
            this.Data = Data;
        }

        public override string ToString()
        {
            return Type + "\0" + Data;
        }
    }

    public class MType
    {
        #region "Authentification"
        public const string AUTHENTIFICATION_REQUEST = "AUTHENTIFICATION_REQUEST";
        public const string AUTHENTIFICATION_DATA = "AUTHENTIFICATION_DATA";
        public const string AUTHENTIFICATION_CORRECT = "AUTHENTIFICATION_CORRECT";
        public const string AUTHENTIFICATION_ERROR = "AUTHENTIFICATION_ERROR";
        #endregion
    }
}
