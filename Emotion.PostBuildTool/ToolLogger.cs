#region Using

using System;
using Emotion.Standard.Logging;

#endregion

#nullable enable

namespace Emotion.PostBuildTool
{
    public class ToolLogger : NetIOAsyncLogger
    {
        public static string Help = "Help";
        public static string Tool = "Tool";

        public ToolLogger(bool stdOut, string? logFolder = null) : base(stdOut, logFolder)
        {
        }

        public override void Log(MessageType type, string source, string message)
        {
            if (source == Help || source == Tool)
            {
                if (type == MessageType.Error)
                    Console.Error.WriteLine(message);
                else
                    Console.WriteLine("\x1b[38;5;0015m " + message);

                return;
            }

            base.Log(type, source, message);
        }
    }
}