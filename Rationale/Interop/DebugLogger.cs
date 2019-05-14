#region Using

using System.Collections.Generic;
using System.Threading;
using Adfectus.Logging;

#endregion

namespace Rationale.Interop
{
    public class DebugLogger : LoggingProvider
    {
        public List<string> InternalLog { get; private set; } = new List<string>();
        private Communicator _comm;

        public override void Log(Adfectus.Logging.MessageType type, MessageSource source, string message)
        {
            string fullMessage = $"[{source}] [{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId}] {message}";

            // Send either through the comms - if any, or store in an internal log.
            if (_comm != null)
                _comm.SendMessage(new DebugMessage {Type = MessageType.MessageLogged, Data = fullMessage});
            else
                InternalLog.Add(fullMessage);
        }

        public override void Dispose()
        {
            InternalLog.Clear();
        }

        public void AttachCommunicator(Communicator comm)
        {
            _comm = comm;

            // Copy the internal log.
            foreach (string logMsg in InternalLog)
            {
                _comm.SendMessage(new DebugMessage {Type = MessageType.MessageLogged, Data = logMsg});
            }
            InternalLog.Clear();
        }
    }
}