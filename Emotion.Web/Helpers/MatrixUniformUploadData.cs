#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Web.Helpers
{
    /// <summary>
    /// Used for uploading matrix array uniforms and float array uniforms with
    /// a component count of 2 or up.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MatrixUniformUploadData
    {
        public int ComponentCount;
        public int ArrayLength;
        public IntPtr Data;
        public bool Transpose;
    }
}