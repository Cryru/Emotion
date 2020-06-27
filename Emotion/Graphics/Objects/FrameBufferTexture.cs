#region Using

using System.Numerics;
using Emotion.Common.Threading;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    /// <summary>
    /// Texture overload used by FrameBuffer to store its attachments.
    /// </summary>
    public class FrameBufferTexture : Texture
    {
        /// <summary>
        /// FrameBuffer's store RenderBuffers as these objects as well. In that case the texture pointer will be 0
        /// but this RenderBufferPtr will not.
        /// </summary>
        public uint RenderBufferPtr { get; protected set; }

        public FrameBufferTexture(uint renderBufferPtr, Vector2 size, InternalFormat internalFormat)
        {
            Pointer = 0;
            RenderBufferPtr = renderBufferPtr;
            Size = size;
            InternalFormat = internalFormat;
        }

        public FrameBufferTexture(Vector2 size, InternalFormat internalFormat, PixelFormat pixelFormat) : base(size, false, internalFormat, pixelFormat)
        {
        }

        public override void Upload(Vector2 size, byte[] data, InternalFormat? internalFormat = null, PixelFormat? pixelFormat = null, PixelType? pixelType = null)
        {
            if (RenderBufferPtr != 0)
            {
                Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBufferPtr);
                Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat, (int) size.X, (int) size.Y);
            }

            if (Pointer == 0) return;
            base.Upload(size, data, internalFormat, pixelFormat, pixelType);
        }

        public override void Dispose()
        {
            if (RenderBufferPtr != 0)
                GLThread.ExecuteGLThreadAsync(() => Gl.DeleteRenderbuffers(RenderBufferPtr));

            base.Dispose();
        }
    }
}