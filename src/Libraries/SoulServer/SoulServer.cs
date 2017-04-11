using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SoulServer;

namespace SoulEngine
{
    //////////////////////////////////////////////////////////////////////////////
    // SoulEngine - A game engine based on the MonoGame Framework.              //
    // Public Repository: https://github.com/Cryru/SoulEngine                   //
    // SoulServer - A TCP server for use with SoulEngine.                       //
    // Public Repository: https://github.com/Cryru/SoulServer                   //
    //////////////////////////////////////////////////////////////////////////////
    /// <summary>
    /// Networking and Multiplayer support with SoulServer. EXPERIMENTAL 
    /// </summary>
    public static class SoulServer
    {
        #region "Declarations"
        private static Socket Client;
        private static byte[] buffer;
        private static string Hash;
        #endregion

        /// <summary>
        /// Sets up networking.
        /// </summary>
        public static void Setup()
        {
            buffer = new byte[1024];
            Scripting.ScriptEngine.ExposeFunction("connect", (Action<string, int, string, string>)Connect);
        }

        /// <summary>
        /// Connects to the SoulServer on the provided IP and Port.
        /// </summary>
        /// <param name="IP">The IP of the server.</param>
        /// <param name="Port">The port of the server.</param>
        /// <param name="Username">The username to login with.</param>
        /// <param name="Password">The password login with.</param>
        public static void Connect(string IP, int Port, string Username, string Password)
        {
            //Check if already connected.
            if (!isConnected() && Settings.Networking)
            {
                Client = new Socket(SocketType.Stream, ProtocolType.Tcp);
                Hash = Username + "|" + Soul.Encryption.MD5(Username + Password);
                Client.BeginConnect(IP, Port, new AsyncCallback(Connected), null);
            }
        }

        /// <summary>
        /// Processed a message from the server.
        /// </summary>
        /// <param name="Message">The message received.</param>
        private static void ProcessMessage(ServerMessage Message)
        {
            switch(Message.Type)
            {
                //Check if asking for authentification.
                case MType.AUTHENTIFICATION_REQUEST: 
                    Send(new ServerMessage(MType.AUTHENTIFICATION_DATA, Hash));
                    break;
                case MType.AUTHENTIFICATION_ERROR:
                    Client.Disconnect(true);
                    break;
                default:
                    Debugging.Logger.Add(Message.ToString());
                    break;
            }
        }

        #region "Async Callbacks"
        /// <summary>
        /// The callback for when connection is complete.
        /// </summary>
        /// <param name="AR"></param>
        private static void Connected(IAsyncResult AR)
        {
            //Start listening for messages from the server.
            Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedMessage), null);
        }

        /// <summary>
        /// When the server has sent us a message.
        /// </summary>
        private static void ReceivedMessage(IAsyncResult AR)
        {
            //Check if networking is on, we have a client and are connected.
            if (!Settings.Networking || Client == null || !Client.Connected) return;

            try
            {
                //Get the length to cut the message from the buffer.
                int received = Client.EndReceive(AR);
                //Check if empty message.
                if (received == 0) return;
                //Convert the message to a string.
                string message = Encoding.UTF8.GetString(buffer).Substring(0, received).Trim().Replace("\r", "");
                //Process the message.
                ProcessMessage(new ServerMessage(message));
                //Continue receiving messages if connected.
                if (Client.Connected)
                    Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(ReceivedMessage), null);
            }
            catch (Exception e)
            {
                //Log the error.
                Debugging.Logger.Add("Message receive error: " + e.ToString());
            }
        }

        /// <summary>
        /// The callback for when a sending operation is complete.
        /// </summary>
        private static void SendEndCallback(IAsyncResult AR)
        {
            try
            {
                Client.EndSend(AR);
            }
            catch (Exception e)
            {
                Debugging.Logger.Add("Error finishing sending message to network: " + e.Message.ToString());
            }
        }
        #endregion

        #region "Helpers"
        private static bool isConnected()
        {
            return false;
        }
        #endregion

        #region "Publics"
        /// <summary>
        /// Sends data to the connected server.
        /// </summary>
        /// <param name="Message">The message to send.</param>
        public static void Send(ServerMessage Message)
        {
            //Check if networking is on, we have a client and are connected.
            if (!Settings.Networking || Client == null || !Client.Connected) return;

            try
            {
                //Convert the string into bytes.
                byte[] send = Encoding.UTF8.GetBytes(Message.ToString());
                //Start sending, call the sent method.
                Client.BeginSend(send, 0, send.Length, SocketFlags.None, new AsyncCallback(SendEndCallback), Client);
            }
            catch (Exception e)
            {
                Debugging.Logger.Add("Error sending message to network: " + e.Message.ToString());
            }
        }
        #endregion
    }
}
