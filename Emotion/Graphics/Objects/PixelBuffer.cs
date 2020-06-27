#region Using

using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    public class PixelBuffer : DataBuffer
    {
        /// <summary>
        /// The bound pixel buffer.
        /// </summary>
        public new static uint Bound
        {
            get => DataBuffer.Bound[BufferTarget.PixelPackBuffer];
            set => DataBuffer.Bound[BufferTarget.PixelPackBuffer] = value;
        }

        /// <summary>
        /// The OpenGL pointer to the fence this PBO will use.
        /// </summary>
        public uint Fence { get; set; }

        public PixelBuffer(uint byteSize = 0, BufferUsage usage = BufferUsage.StaticDraw) : base(BufferTarget.PixelPackBuffer, byteSize, BufferUsage.StreamCopy)
        {
            EnsureBound(0);
        }

        /// <summary>
        /// Ensures the provided pointer is the currently bound pixel buffer.
        /// </summary>
        /// <param name="pointer">The pointer to ensure is bound.</param>
        public static void EnsureBound(uint pointer)
        {
            EnsureBound(pointer, BufferTarget.PixelPackBuffer);
        }

        public void Sample()
        {
        }

        public void Check()
        {
        }
    }
}