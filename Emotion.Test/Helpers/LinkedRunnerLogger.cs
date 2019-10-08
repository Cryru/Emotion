using System;
using System.Diagnostics;
using Emotion.Standard.Logging;

namespace Emotion.Test.Helpers
{
    public static class CustomMSource
    {
        public static string TestRunner = "TestRunner";
    }

    public class LinkedRunnerLogger : LoggingProvider
    {
        private string _linkId;

        public LinkedRunnerLogger()
        {
            _linkId = $"LR{Process.GetCurrentProcess().Id}";
        }

        public override void Log(MessageType type, string source, string message)
        {
            string formattedMsg = $"{_linkId} >> [{type}-{source}] {message}";

            if (source == CustomMSource.TestRunner || type == MessageType.Warning)
            {
                Console.WriteLine(formattedMsg);
                return;
            }

            if (type == MessageType.Error)
            {
                Console.Error.WriteLine(formattedMsg);
            }
        }

        public override void Dispose()
        {
           
        }
    }
}