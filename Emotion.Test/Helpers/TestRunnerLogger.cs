#region Using

using System;
using System.IO;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Test.Helpers
{
    public class TestRunnerLogger : NetIOAsyncLogger
    {
        public static string TestRunnerSrc = "TestRunner";

        private bool _linked;

        public TestRunnerLogger(bool linked, string folder) : base(false, folder)
        {
            _linked = linked;
        }

        protected override string GenerateLogName()
        {
            var runnerId = Runner.RunnerId.ToString();
            return $"{_logFolder}{Path.DirectorySeparatorChar}ID{runnerId} {DateTime.Now:MM-dd-yyyy_HH-mm-ss}.log";
        }

        public override void Log(MessageType type, string source, string message)
        {
            var runnerId = Runner.RunnerId.ToString();
            string formattedMsg = _linked ? $"{runnerId} >> {message}" : $"{message}";

            if (source == TestRunnerSrc)
            {
                if (type == MessageType.Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(formattedMsg);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(formattedMsg);
                }
            }

            base.Log(type, source, message);
        }
    }
}