#region Using

using System;

#endregion

namespace Emotion.Web.Helpers
{
    public struct UnmanagedMemory
    {
        public int Size;
        public IntPtr Memory;
    }
}