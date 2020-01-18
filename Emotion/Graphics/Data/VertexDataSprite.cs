#region Using

using System.Runtime.InteropServices;

#endregion

namespace Emotion.Graphics.Data
{
    /// <summary>
    /// The representative of a single sprite made out of four VertexData.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexDataSprite
    {
        public VertexData FirstVertex;
        public VertexData SecondVertex;
        public VertexData ThirdVertex;
        public VertexData FourthVertex;
    }
}