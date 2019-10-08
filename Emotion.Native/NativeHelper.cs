#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Native
{
    /// <summary>
    /// Helps with certain native functionality.
    /// </summary>
    public static class NativeHelper
    {
        /// <summary>
        /// Display an error message natively.
        /// Usually this means a popup will show up.
        /// </summary>
        /// <param name="message">The message to display.</param>
        public static void DisplayErrorMessage(string message)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                WindowsNative.MessageBox(IntPtr.Zero, message, "Something went wrong!", (uint) (0x00000000L | 0x00000010L));
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                UnixNative.ExecuteBashCommand($"osascript -e 'tell app \"System Events\" to display dialog \"{message}\" buttons {{\"OK\"}} with icon caution'");
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                try
                {
                    // Display a message box using Zenity.
                    UnixNative.ExecuteBashCommand($"zenity --error --text=\"{message}\" --title=\"Something went wrong!\" 2>/dev/null");
                }
                catch (Exception)
                {
                    // Fallback to xmessage.
                    UnixNative.ExecuteBashCommand($"xmessage \"{message}\"");
                }
        }
    }
}