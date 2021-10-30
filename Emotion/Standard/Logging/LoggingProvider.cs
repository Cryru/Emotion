#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;

#endregion

namespace Emotion.Standard.Logging
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
            var redirectErr = new StringWriterExt(true);
            Console.SetError(redirectErr);
            redirectErr.OnUpdate += () => { Error(redirectErr.ToString(), MessageSource.StdErr); };
        }

        #region Error

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The message source.</param>
        public void Error(string message, string source)
        {
            Log(MessageType.Error, source, message);
        }

        /// <summary>
        /// Log an error.
        /// </summary>
        /// <param name="ex">The error's exception.</param>
        public void Error(Exception ex)
        {
            Log(MessageType.Error, "StdException", ex.ToString());
        }

        #endregion

        private HashSet<string> _oncePrint;

        /// <summary>
        /// Log a warning. These are usually workarounds or destabilization of code.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The message source.</param>
        /// <param name="once">Print only once.</param>
        public void Warning(string message, string source, bool once = false)
        {
            if (once)
            {
                _oncePrint ??= new HashSet<string>();
                if (_oncePrint.Contains(message)) return;
                _oncePrint.Add(message);
            }

            Log(MessageType.Warning, source, message);
        }

        /// <summary>
        /// Log an info message. These are general spam.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The message source.</param>
        public void Info(string message, string source)
        {
            Log(MessageType.Info, source, message);
        }

        /// <summary>
        /// Log a trace. These are extreme spam used for debugging.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The message source.</param>
        [Conditional("DEBUG")]
        public void Trace(string message, string source)
        {
            // todo until filtering is introduced.
            if (source == MessageSource.ShaderSource || source == MessageSource.Input) return;

            Log(MessageType.Trace, source, message);
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        /// <param name="type">The type of message to log.</param>
        /// <param name="source">The source of the message.</param>
        /// <param name="message">The message itself.</param>
        public abstract void Log(MessageType type, string source, string message);

        /// <summary>
        /// Stop logging and cleanup.
        /// </summary>
        public abstract void Dispose();
    }
}