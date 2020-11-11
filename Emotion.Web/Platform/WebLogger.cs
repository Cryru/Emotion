#region Using

using System;
using System.Threading;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Web.Platform
{
    public class WebLogger : LoggingProvider
    {
        public override void Log(MessageType type, string source, string message)
        {
            Console.WriteLine($"{Engine.TotalTime:0} [{source}] [{Thread.CurrentThread.Name}/{Thread.CurrentThread.ManagedThreadId}] {message}");
        }

        public override void Dispose()
        {
        }
    }
}