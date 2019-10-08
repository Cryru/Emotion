#region Using

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

#endregion

namespace Emotion.Standard.Logging
{
    /// <summary>
    /// A network logger which logs remotely using UDP. Supports www.papertrailapp.com
    /// </summary>
    public sealed class NetworkLogger : LoggingProvider
    {
        private string _hostName;
        private int _port;
        private string _userId;
        private UdpClient _udpClient;

        /// <summary>
        /// Create a network logger, with the provided remote host.
        /// </summary>
        /// <param name="host">The host to connect to.</param>
        /// <param name="port">The port.</param>
        /// <param name="userId">The unique id of the user.</param>
        public NetworkLogger(string host, int port, string userId)
        {
            _udpClient = new UdpClient();
            _hostName = host;
            _port = port;
            _userId = userId;
        }

        /// <inheritdoc />
        public override void Log(MessageType type, string source, string message)
        {
            Send(type, source, $"[{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId}] {message}");
        }

        /// <inheritdoc />
        public override void Dispose()
        {
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