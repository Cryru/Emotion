#region Using

using System.IO;

#endregion

namespace Emotion.Testing
{
    public class TestLogger : NetIOAsyncLogger
    {
        public TestLogger(string folder) : base(false, folder)
        {
        }

        protected override string GenerateLogName()
        {
            return $"{_logFolder}{Path.DirectorySeparatorChar}TestLog.log";
        }

        public override void Log(MessageType type, string source, string message)
        {
            if (source == MessageSource.Test || type == MessageType.Error || type == MessageType.Warning)
            {
                if (type == MessageType.Error)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(message);
                    Console.ResetColor();
                }
                else if (type == MessageType.Warning)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.WriteLine(message);
                    Console.ResetColor();
                }
                else
                {
                    Console.WriteLine(message);
                }
            }

            base.Log(type, source, message);
        }
    }
}