// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Emotion
{
    public static class Helpers
    {
        /// <summary>
        /// Extracts a string from a pointer.
        /// </summary>
        /// <param name="pointer">The pointer to extract from.</param>
        internal static unsafe string StringFromPointer(IntPtr pointer)
        {
            if (pointer == IntPtr.Zero)
                return "";

            byte* ptr = (byte*) pointer;
            while (*ptr != 0)
                ptr++;

            byte[] bytes = new byte[ptr - (byte*) pointer];
            Marshal.Copy(pointer, bytes, 0, bytes.Length);

            return Encoding.UTF8.GetString(bytes);
        }
    }
}