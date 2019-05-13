#region Using

using System.Diagnostics;

#endregion

namespace Emotion.Platform.DesktopGL.Native
{
    /// <summary>
    /// Functions relating to the Unix family of OS.
    /// </summary>
    public static class UnixNative
    {
        /// <summary>
        /// Execute a bash command on Unix systems.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        public static void ExecuteBashCommand(string command)
        {
            Process bashProcess = new Process
            {
                StartInfo =
                {
                    FileName = "/bin/bash",
                    Arguments = $"-c \"{command.Replace("\"", "\\\"")}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };
            bashProcess.Start();
        }
    }
}