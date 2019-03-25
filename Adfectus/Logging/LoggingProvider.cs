#region Using

using System;
using System.Diagnostics;

#endregion

namespace Adfectus.Logging
{
    /// <summary>
    /// Provides logging of actions.
    /// </summary>
    public abstract class LoggingProvider : IDisposable
    {
        /// <summary>
        /// Create a new logging provider.
        /// </summary>
        protected LoggingProvider()
        {
            // Attach to default std err.
            StringWriterExt redirectErr = new StringWriterExt(true);
            Console.SetError(redirectErr);
            redirectErr.OnUpdate += () => { Error(redirectErr.ToString(), MessageSource.StdErr); };
        }

        #region Error

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The message source.</param>
        public void Error(string message, MessageSource source)
        {
            Log(MessageType.Error, source, message);
        }

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="ex">The error's exception.</param>
        public void Error(Exception ex)
        {
            Log(MessageType.Error, MessageSource.StdErr, ex.ToString());
        }

        #endregion

        /// <summary>
        /// Log a warning. These are usually workarounds or destabilization of code.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The message source.</param>
        public void Warning(string message, MessageSource source)
        {
            Log(MessageType.Warning, source, message);
        }

        /// <summary>
        /// Log an info message. These are general spam.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The message source.</param>
        public void Info(string message, MessageSource source)
        {
            Log(MessageType.Info, source, message);
        }

        /// <summary>
        /// Log a trace. These are extreme spam used for debugging.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The message source.</param>
        [Conditional("DEBUG")]
        public void Trace(string message, MessageSource source)
        {
            Log(MessageType.Trace, source, message);
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="type">The type of message to log.</param>
        /// <param name="source">The source of the message.</param>
        /// <param name="message">The message itself.</param>
        public abstract void Log(MessageType type, MessageSource source, string message);

        /// <summary>
        /// Stop logging and cleanup.
        /// </summary>
        public abstract void Dispose();
    }
}