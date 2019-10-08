#region Using

using System;

#endregion

namespace Emotion.Standard.Logging
{
    /// <summary>
    /// The logger used, before Engine.Setup is called.
    /// It logs to stdout and the logs don't persist.
    /// </summary>
    public class PreSetupLogger : LoggingProvider
    {
        private bool _warningShown;

        public override void Log(MessageType type, string source, string message)
        {
            if (!_warningShown)
            {
                _warningShown = true;
                Log(MessageType.Warning, "PreSetupLogger", "No logging provider set - logging to stdout.");
            }

            switch (type)
            {
                case MessageType.Error:
                    message = $"{"\x1B[31m"}{message}{"\x1B[0m"}";
                    break;
                case MessageType.Warning:
                    message = $"{"\x1B[33m"}{message}{"\x1B[0m"}";
                    break;
            }

            Console.WriteLine($"[{type}-{source}] {message}");
        }

        public override void Dispose()
        {
        }
    }
}