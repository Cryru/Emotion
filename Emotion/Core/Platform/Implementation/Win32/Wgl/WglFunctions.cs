#region Using

#endregion

namespace Emotion.Core.Platform.Implementation.Win32.Wgl
{
    public static unsafe class WglFunctions
    {
        public delegate nint WglCreateContext(nint hdc);

        public delegate bool WglDeleteContext(nint context);

        public delegate nint WglGetProcAddress(string proc);

        public delegate nint WglGetCurrentDc();

        public delegate nint WglGetCurrentContext();

        public delegate bool WglMakeCurrent(nint hdc, nint context);

        public delegate nint GetExtensionsStringExt();

        public delegate nint GetExtensionsStringArb(nint hdc);

        public delegate nint CreateContextAttribs(nint hdc, nint context, int* a);

        public delegate void SwapInternalExt(int interval);

        public delegate bool GetPixelFormatAttributes(nint hdc, int a, int b, uint c, int* d, int* e);
    }
}