// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Engine;

#endregion

namespace Emotion.Debug.Logging
{
    /// <summary>
    /// A network logger which logs remotely. Supports www.papertrailapp.com
    /// </summary>
    public sealed class NetworkLogger : LoggingProvider
    {
        private ConcurrentQueue<Tuple<MessageType, MessageSource, string>> _loggingQueue;
        private Thread _loggingThread;

        private string _hostName;
        private int _port;
        private string _userId;
        private UdpClient _udpClient;

        /// <summary>
        /// Create a network logger.
        /// </summary>
        public NetworkLogger()
        {
            // Setup logging queue.
            _loggingQueue = new ConcurrentQueue<Tuple<MessageType, MessageSource, string>>();

            // Start the logging thread.
            _loggingThread = new Thread(LoggingThread) {Name = "Logging Thread"};
            _loggingThread.Start();
            while (!_loggingThread.IsAlive)
            {
            }
        }

        /// <summary>
        /// Connect to the remote host.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port.</param>
        /// <param name="userId">The unique id of the user.</param>
        public void Connect(string host, int port, string userId)
        {
            _udpClient = new UdpClient();
            _hostName = host;
            _port = port;
            _userId = userId;
        }

        private void LoggingThread()
        {
            try
            {
                while (!Environment.HasShutdownStarted)
                {
                    // Sleep.
                    Task.Delay(1).Wait();

                    // Perform loop.
                    LogThreadLoop();
                }

                FlushRemainingLogs();
            }
            catch (Exception ex)
            {
                if (ex.Message != "Thread was being aborted." && !(ex is ThreadAbortException) || Context.IsRunning) Error("Logging thread has crashed.", ex, MessageSource.Debugger);
            }
        }

        private void LogThreadLoop()
        {
            // Read from the logging queue.
            bool readLine = _loggingQueue.TryPeek(out Tuple<MessageType, MessageSource, string> nextLog);
            if (!readLine) return;

            MessageType type = nextLog.Item1;
            MessageSource source = nextLog.Item2;
            string message = nextLog.Item3;

            // Send to remote host.
            bool success = Send(type, source.ToString(), message);

            // Remove from queue if sent.
            if (success) _loggingQueue.TryDequeue(out _);
        }

        private void FlushRemainingLogs()
        {
            // Flush logs.
            while (!_loggingQueue.IsEmpty) LogThreadLoop();
        }

        /// <inheritdoc />
        public override void Log(MessageType type, MessageSource source, string message)
        {
            _loggingQueue.Enqueue(new Tuple<MessageType, MessageSource, string>(type, source, $"[{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId}] {message}"));
        }

        /// <inheritdoc />
        public override void Dispose()
        {
            // Stop the logging thread.
            _loggingThread.Abort();
            while (_loggingThread.IsAlive)
            {
            }

            FlushRemainingLogs();

            // Destroy UDP socket.
            _udpClient.Dispose();
        }

        #region Networking

        /// <summary>
        /// Send the log to the remote host.
        /// </summary>
        /// <param name="logLevel">The log level.</param>
        /// <param name="sender">The sender of the message.</param>
        /// <param name="message">The message itself.</param>
        /// <return>Whether the message was sent. (Doesn't mean it was received.)</return>
        private bool Send(MessageType logLevel, string sender, string message)
        {
            // Check if no host or port are configured.
            if (string.IsNullOrWhiteSpace(_hostName) || _port == 0)
                return false;


            string dateTime = DateTime.UtcNow.ToString("s");
            string hostName = _userId;
            message = $"<{16 * 8 + (int) logLevel}>{dateTime} {hostName} {sender} {message}";
            byte[] bytes = Encoding.UTF8.GetBytes(message);
            _udpClient.SendAsync(bytes, bytes.Length, _hostName, _port);

            return true;
        }

        #endregion
    }
}