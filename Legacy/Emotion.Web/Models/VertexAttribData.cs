#region Using

using System.Runtime.InteropServices;

#endregion

namespace Emotion.Web.Models
{
    /// <summary>
    /// Used for VertexAttribPointer
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexAttribData
    {
        public uint Index;
        public int Size;
        public int Type;
        public bool Normalized;
        public int Stride;
        public int Offset;
    }
}