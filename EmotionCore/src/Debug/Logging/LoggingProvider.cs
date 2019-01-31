// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Diagnostics;
using Emotion.Engine;

#endregion

namespace Emotion.Debug.Logging
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
            // Attach to default std.
            StringWriterExt redirectErr = new StringWriterExt(true);
            Console.SetError(redirectErr);
            redirectErr.OnUpdate += () => { Error(redirectErr.ToString(), MessageSource.StdErr); };

            // Attach to unhandled exceptions.
            AppDomain.CurrentDomain.UnhandledException += (e, a) => { Error($"Unhandled exception: {e}.", MessageSource.StdErr); };

            // Ensure log dispose is called when app domain closes.
            AppDomain.CurrentDomain.ProcessExit += (e, a) => { Dispose(); };
        }

        /// <summary>
        /// Log an error. These messages are fatal to code execution.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="source">The message source.</param>
        public void Error(string message, MessageSource source)
        {
            Log(MessageType.Error, source, message);
        }

        /// <summary>
        /// Log an error. These messages are fatal to code execution. If in debug mode the exception will be thrown, otherwise the
        /// Context.Quit() method will be invoked.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="ex">The exception if any.</param>
        /// <param name="source">The message source.</param>
        public void Error(string message, Exception ex, MessageSource source)
        {
            Log(MessageType.Error, source, message + "\n" + ex);

            if (System.Diagnostics.Debugger.IsAttached) System.Diagnostics.Debugger.Break();

            if (Context.Flags.CrashOnError) Context.Crash();
        }

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