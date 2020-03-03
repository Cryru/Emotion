#region Using

using System;

#endregion

namespace Emotion.Platform.Implementation.Win32.Wgl
{
    public static unsafe class WglFunctions
    {
        public delegate IntPtr WglCreateContext(IntPtr hdc);

        public delegate bool WglDeleteContext(IntPtr context);

        public delegate IntPtr WglGetProcAddress(string proc);

        public delegate IntPtr WglGetCurrentDc();

        public delegate IntPtr WglGetCurrentContext();

        public delegate bool WglMakeCurrent(IntPtr hdc, IntPtr context);

        public delegate char* GetExtensionsStringExt();

        public delegate char* GetExtensionsStringArb(IntPtr hdc);

        public delegate IntPtr CreateContextAttribs(IntPtr hdc, IntPtr context, int* a);

        public delegate void SwapInternalExt(int interval);

        public delegate bool GetPixelFormatAttributes(IntPtr hdc, int a, int b, uint c, int* d, int* e);
    }
}