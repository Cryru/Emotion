// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.External;
using Emotion.Utils;

#endregion

namespace Emotion
{
    public static class Starter
    {
        /// <summary>
        /// Creates and returns an Emotion context.
        /// </summary>
        /// <param name="config">A function to apply settings in.</param>
        /// <returns>An Emotion context.</returns>
        public static Context GetEmotionContext(Action<Settings> config = null)
        {
            Debugger.Log(MessageType.Info, MessageSource.Engine, "-------------------------------");
            Debugger.Log(MessageType.Info, MessageSource.Engine, "Executed at: " + Environment.CurrentDirectory);
            Debugger.Log(MessageType.Info, MessageSource.Engine, "64Bit: " + Environment.Is64BitProcess);
            Debugger.Log(MessageType.Info, MessageSource.Engine, "OS: " + CurrentPlatform.OS + " (" + Environment.OSVersion + ")");
            Debugger.Log(MessageType.Info, MessageSource.Engine, "CPU: " + Environment.ProcessorCount);

            // Bootstrap process.
            if (CurrentPlatform.OS == PlatformID.Win32NT) WindowsSetup();

            Debugger.Log(MessageType.Info, MessageSource.Engine, "-------------------------------");

            // Apply settings.
            Settings initial = new Settings();

            // Setup thread manager.
            ThreadManager.BindThread();

            config?.Invoke(initial);

            return new Context(initial);
        }

        private static void WindowsSetup()
        {
            // Set the DLL path on Windows.
            string libraryDirectory = Environment.CurrentDirectory + "\\Libraries\\" + (Environment.Is64BitProcess ? "x64" : "x86");
            Windows.SetDllDirectory(libraryDirectory);
            string path = Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Process);
            Environment.SetEnvironmentVariable("PATH", path + ";" + libraryDirectory, EnvironmentVariableTarget.Process);

            Debugger.Log(MessageType.Info, MessageSource.Engine, "Library Folder: " + libraryDirectory);
        }
    }
}