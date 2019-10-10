#region Using

using System;
using WinApi;

#endregion

namespace Emotion.Platform.Implementation.Win32.Audio
{
    public class WasApiAudioContext : AudioContext
    {
        public static WasApiAudioContext TryCreate()
        {
            IntPtr notifyEvent = SynchApi.CreateEventW(IntPtr.Zero, false, false, null);
            if (notifyEvent == IntPtr.Zero)
            {
                Win32Platform.CheckError("Couldn't register notify event.", true);
                return null;
            }

            return null;
        }
    }
}