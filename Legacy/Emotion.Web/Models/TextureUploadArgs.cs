#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Web.Models
{
    [StructLayout(LayoutKind.Sequential)]
    public struct TextureUploadArgs
    {
        public int Target;
        public int Level;
        public int InternalFormat;
        public int Width;
        public int Height;
        public int Border;
        public int Format;
        public int Type;
        public IntPtr PixelsPointer;
        public int PixelsByteSize;
    }
}