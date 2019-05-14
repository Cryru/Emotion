#region Using

using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Adfectus.Common;
using Adfectus.Logging;

#endregion

namespace Rationale.Interop
{
    public class Communicator
    {
        private XmlSerializer _serializer = new XmlSerializer(typeof(DebugMessage));
        public Action<DebugMessage> MessageReceiveCallback { get; set; }
        private Process _proc;

        /// <summary>
        /// Wait to receive communication from another process.
        /// </summary>
        public Communicator()
        {
        }

        /// <summary>
        /// Initiate communication with another process.
        /// </summary>
        /// <param name="proc">The process to communicate with.</param>
        public Communicator(Process proc)
        {
            _proc = proc;
            _proc.BeginOutputReadLine();
            _proc.OutputDataReceived += Proc_OutputDataReceived;
        }

        /// <summary>
        /// Send a message to any listening processes.
        /// </summary>
        /// <param name="msg">The message to send.</param>
        public void SendMessage(DebugMessage msg)
        {
            // Check if we know of any processes listening.
            if (_proc == null)
            {
                // Send a message requesting the proc id of any listeners.
                InternalSend(new DebugMessage {Type = MessageType.GetListener});
                string response = Console.ReadLine();
                _proc = Process.GetProcessById(int.Parse(response));
                Task.Run(InternalReceiveSecondary);
            }

            InternalSend(msg);
        }

        private void InternalReceive(string msg)
        {
            try
            {
                using (StringReader reader = new StringReader(msg))
                {
                    DebugMessage parsedMsg = (DebugMessage) _serializer.Deserialize(reader);
                    Engine.Log.Info($"Received debug message of type {parsedMsg.Type}", MessageSource.Debugger);

                    // Handle a GetListener message.
                    if (parsedMsg.Type == MessageType.GetListener) _proc.StandardInput.WriteLine(Process.GetCurrentProcess().Id);

                    MessageReceiveCallback?.Invoke(parsedMsg);
                }
            }
            catch (Exception ex)
            {
                Engine.Log.Warning($"Couldn't parse debug message {msg} - with error {ex}.", MessageSource.Debugger);
            }
        }

        private void InternalReceiveSecondary()
        {
            while (true)
            {
                string input = Console.ReadLine();
                InternalReceive(input);

                if (Engine.IsSetup && !Engine.IsRunning) break;
            }
        }

        private void InternalSend(DebugMessage msg)
        {
            using StringWriter reader = new StringWriter();
            _serializer.Serialize(reader, msg);
            Console.WriteLine(reader.ToString().Replace(Environment.NewLine, ""));
        }

        /// <summary>
        /// Message received from a process.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Proc_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            InternalReceive(e.Data);
        }
    }
}