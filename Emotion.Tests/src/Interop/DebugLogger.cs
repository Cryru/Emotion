// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Threading;
using Emotion.Debug;
using Emotion.Debug.Logging;

#endregion

namespace Emotion.Tests.Interop
{
    public sealed class DebugLogger : LoggingProvider
    {
        /// <inheritdoc />
        public override void Log(MessageType type, MessageSource source, string message)
        {
            Console.WriteLine($"[{type}] [{source}] [{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId}] {message}");
        }

        /// <inheritdoc />
        public override void Dispose()
        {
        }
    }
}