#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Adfectus.Common;
using Adfectus.Logging;

#endregion

namespace Rationale.Interop
{
    public class Communicator
    {
        /// <summary>
        /// A callback to call when a message is received.
        /// </summary>
        public Action<DebugMessage> MessageReceiveCallback { get; set; }

        private XmlSerializer _serializer;
        private Socket _me;
        private Socket _other;

        private byte[] _buffer;
        private bool _isServer;

        /// <summary>
        /// Host a debug communicator.
        /// </summary>
        public Communicator(int port)
        {
            CreateSerializer();

            _me = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _me.Bind(new IPEndPoint(IPAddress.Any, port));
            _me.Listen(1);
            _isServer = true;

            Task.Run(() =>
            {
                _other = _me.Accept();
                int bufferSize = _other.SendBufferSize;
                _buffer = new byte[bufferSize];
                StartReceive();
            });
        }

        /// <summary>
        /// Connect to a debug communicator.
        /// </summary>
        /// <param name="ip">The ip to connect to.</param>
        /// <param name="port">The port to connect on.</param>
        public Communicator(string ip = "127.0.0.1", int port = 9991)
        {
            CreateSerializer();

            _me = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _me.Connect(new IPEndPoint(IPAddress.Parse(ip), port));
            SendMessage(new DebugMessage {Type = MessageType.GetListener});

            Task.Run(() =>
            {
                int bufferSize = _me.ReceiveBufferSize;
                _buffer = new byte[bufferSize];
                StartReceive();
            });
        }

        /// <summary>
        /// Send a message to any listening processes.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        public void SendMessage(DebugMessage msg)
        {
            StringWriter writer = new StringWriter();
            _serializer.Serialize(writer, msg);
            byte[] bytes = Encoding.Default.GetBytes(writer.ToString());

            if (_isServer)
                _other.SendAsync(bytes, SocketFlags.None);
            else
                _me.SendAsync(bytes, SocketFlags.None);
        }

        private void StartReceive()
        {
            try
            {
                if (_isServer)
                    _other.ReceiveAsync(_buffer, SocketFlags.None).ContinueWith(_ => InternalReceive(_.Result));
                else
                    _me.ReceiveAsync(_buffer, SocketFlags.None).ContinueWith(_ => InternalReceive(_.Result));
            }
            catch (Exception ex)
            {
                Engine.Log.Warning($"Error occured while trying to listen for messages. {ex}", MessageSource.Debugger);
            }
        }

        private void InternalReceive(int length)
        {
            using (MemoryStream ms = new MemoryStream(_buffer, 0, length))
            using (StreamReader reader = new StreamReader(ms))
            {
                try
                {
                    DebugMessage parsedMsg = (DebugMessage) _serializer.Deserialize(reader);
                    MessageReceiveCallback?.Invoke(parsedMsg);
                }
                catch (Exception ex)
                {
                    Engine.Log.Warning($"Couldn't parse debug message with error {ex}.", MessageSource.Debugger);
                }
            }

            StartReceive();
        }

        private void CreateSerializer()
        {
            //List<Type> knownTypes = new List<Type>();
            //Assembly[] sourceAssemblies =
            //{
            //    Assembly.GetCallingAssembly(),
            //    Assembly.GetExecutingAssembly(),
            //    Assembly.GetEntryAssembly(),
            //    Engine.Platform.GetType().Assembly
            //};
            //sourceAssemblies = sourceAssemblies.Distinct().Where(x => x != null).ToArray();

            //// Asset type - used by the asset debugger.
            //knownTypes.AddRange(FindAllDerivedTypes<Asset>(sourceAssemblies));

            _serializer = new XmlSerializer(typeof(DebugMessage));
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly[] assemblies)
        {
            List<Type> combined = new List<Type>();

            foreach (Assembly asm in assemblies)
            {
                List<Type> types = FindAllDerivedTypes<T>(asm);
                combined.AddRange(types);
            }

            return combined;
        }

        public static List<Type> FindAllDerivedTypes<T>(Assembly assembly)
        {
            Type derivedType = typeof(T);
            return assembly
                .GetTypes()
                .Where(t =>
                    t != derivedType &&
                    derivedType.IsAssignableFrom(t)
                ).ToList();
        }
    }
}