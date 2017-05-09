using Jint;
using Jint.Runtime.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SoulEngine.Scripting;

namespace SoulEngine.Debugging
{
    public static class DebugSocket
    {
        /// <summary>
        /// The socket to be used for broadcasting.
        /// </summary>
        public static UdpClient Socket;

        /// <summary>
        /// The list of attached debuggers.
        /// </summary>
        private static List<IPEndPoint> AttachedDebuggers;

        /// <summary>
        /// Initiate broadcasting debug information.
        /// </summary>
        public static void Setup()
        {
            //Check if debugging is enabled.
            if (Settings.Debug == false) return;

            //Initiate the socket.
            Socket = new UdpClient(new IPEndPoint(IPAddress.Any, Settings.DebugSocketPort));

            //Create a list for attached debuggers.
            AttachedDebuggers = new List<IPEndPoint>();

            //Open script access.
            ScriptEngine.Interpreter = new Engine(cfg => cfg.AllowClr());
            ScriptEngine.Interpreter.Execute("var SoulEngine = importNamespace(\"SoulEngine\")");

            //Listen for messages.
            Socket.BeginReceive(ReceivedMessage, null);
        }

        /// <summary>
        /// When a message is received.
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceivedMessage(IAsyncResult AR)
        {
            IPEndPoint receivedIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            AttachedDebuggers.Add(receivedIpEndPoint);
            var receivedBytes = Socket.EndReceive(AR, ref receivedIpEndPoint);
            string message = Encoding.UTF8.GetString(receivedBytes);

            //Execute through the script engine.
            Broadcast(ScriptEngine.ExecuteScript(message).ToString());

            //Resume listening.
            Socket.BeginReceive(ReceivedMessage, null);
        }

        /// <summary>
        /// Send a message to all connected debuggers.
        /// </summary>
        /// <param name="Message"></param>
        public static void Broadcast(string Message)
        {
            for (int i = AttachedDebuggers.Count - 1; i >= 0 ; i--)
            {
                var send = Encoding.UTF8.GetBytes(Message);
                try { Socket.BeginSend(send, send.Length, AttachedDebuggers[i], new AsyncCallback(SendEndCallback), AttachedDebuggers[i]); } catch { AttachedDebuggers.RemoveAt(i); }               
            }
        }

        private static void SendEndCallback(IAsyncResult AR)
        {
            try { Socket.EndSend(AR); } catch { AttachedDebuggers.Remove((IPEndPoint)AR.AsyncState); }
        }
    }
}
