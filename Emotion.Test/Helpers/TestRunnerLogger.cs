#region Using

using System;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Test.Helpers
{
    public static class CustomMSource
    {
        public static string TestRunner = "TestRunner";
    }

    public class TestRunnerLogger : DefaultLogger
    {
        private string _linkId;
        private bool _linked;

        public TestRunnerLogger(string id, bool linked, string folder) : base(false, folder)
        {
            _linkId = id;
            _linked = linked;
        }

        public override void Log(MessageType type, string source, string message)
        {
            string formattedMsg = _linked ? $"{_linkId} >> {message}" : $"{message}";

            if (source == CustomMSource.TestRunner) Console.WriteLine(formattedMsg);

            if (type == MessageType.Error) Console.Error.WriteLine(formattedMsg);

            base.Log(type, source, message);
        }

        public override void Dispose()
        {
        }
    }
}