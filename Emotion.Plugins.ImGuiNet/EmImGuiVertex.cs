#region Using

using System.Numerics;
using System.Runtime.InteropServices;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Plugins.ImGuiNet
{
    /// <summary>
    /// Represents an ImGuiVertex as required by Emotion's VAO constructor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct EmImGuiVertex
    {
        [VertexAttribute(2, false)] public Vector2 Pos;
        [VertexAttribute(2, false)] public Vector2 UV;
        [VertexAttribute(4, true, typeof(byte))]
        public uint Col;
    }
}