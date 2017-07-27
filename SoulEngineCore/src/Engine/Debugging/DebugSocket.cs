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
using SoulEngine.Modules;

namespace SoulEngine.Debugging
{
    public static class DebugSocket
    {
        /// <summary>
        /// The socket to be used for broadcasting.
        /// </summary>
        public static Socket Socket;

        /// <summary>
        /// The attached debugger.
        /// </summary>
        private static Socket AttachedDebugger;

        /// <summary>
        /// The message sending and receiving buffer;
        /// </summary>
        private static byte[] buffer;

        /// <summary>
        /// Initiate broadcasting debug information.
        /// </summary>
        public static void Setup()
        {
            //Check if debugging is enabled.
            if (Settings.Debug == false) return;

            //Initiate the socket.
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.Bind(new IPEndPoint(IPAddress.Any, Settings.DebugSocketPort));

            //Define the message buffer.
            buffer = new byte[1024];

            //Listen for messages.
            Socket.Listen(1);
            Socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        private static void AcceptCallback(IAsyncResult AR)
        {
            //Accept the current connection.
            try { AttachedDebugger = Socket.EndAccept(AR); } catch { return; }

            AttachedDebugger.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedMessage), null);

            Socket.BeginAccept(new AsyncCallback(AcceptCallback), null);
        }

        /// <summary>
        /// When a message is received.
        /// </summary>
        /// <param name="ar"></param>
        private static void ReceivedMessage(IAsyncResult AR)
        {
            int received;
            try { received = Socket.EndReceive(AR); } catch { return; }
            //Check if empty message.
            if (received == 0) return;
            //Convert the message to a string.
            string message = Encoding.UTF8.GetString(buffer).Substring(0, received);

            //Execute through the script engine if not a dummy message.
            if(message.Substring(0, 1) != "0") Broadcast("script", Context.Core.Module<ScriptEngine>().ExecuteScript(message).ToString());

            //Resume listening.
            AttachedDebugger.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedMessage), null);
        }

        /// <summary>
        /// Send a message to all connected debuggers.
        /// </summary>
        /// <param name="Type">The type identifier of the message.</param>
        /// <param name="Message">The message data.</param>
        public static void Broadcast(string Type, string Message)
        {
            //Check if the type is 'fatalerror' in which case crash.
            if(Type == "fatalerror")
            {
                throw new Exception(Message);
                Context.Core.Exit();
            }

            //Check if any debugger is attached.
            if (AttachedDebugger == null) return;

            //Convert to JSON format.
            string JSON = "{\"type\": \"" + Type + "\", \"data\": \"" + Message + "\"}";

            //Convert the string into bytes.
            byte[] send = Encoding.UTF8.GetBytes(JSON);
            //Start sending, call the sent method.
            try { AttachedDebugger.BeginSend(send, 0, send.Length, SocketFlags.None, new AsyncCallback(SendEndCallback), null); } catch { AttachedDebugger = null; }
        }

        private static void SendEndCallback(IAsyncResult AR)
        {
            try { AttachedDebugger.EndSend(AR); } catch { AttachedDebugger = null; }
        }
    }
}
