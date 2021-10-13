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
        /// The framebuffer this texture belongs to.
        /// </summary>
        public FrameBuffer FrameBuffer;

        /// <summary>
        /// FrameBuffer's store RenderBuffers as these objects as well. In that case the texture pointer will be 0
        /// but this RenderBufferPtr will not.
        /// </summary>
        public uint RenderBufferPtr { get; protected set; }

        public FrameBufferTexture(FrameBuffer parent, uint renderBufferPtr, Vector2 size, InternalFormat internalFormat)
        {
            FrameBuffer = parent;
            Pointer = 0;
            RenderBufferPtr = renderBufferPtr;
            Size = size;
            InternalFormat = internalFormat;

#if DEBUG
            AllTextures.Remove(this);
#endif
        }

        public FrameBufferTexture(FrameBuffer parent, Vector2 size, InternalFormat internalFormat, PixelFormat pixelFormat, PixelType pixelType)
            : base(size, pixelFormat, false, internalFormat, pixelType)
        {
            FrameBuffer = parent;
        }

        public override void Upload(Vector2 size, byte[] data, PixelFormat? pixelFormat = null, InternalFormat? internalFormat = null, PixelType? pixelType = null)
        {
            if (RenderBufferPtr != 0)
            {
                Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBufferPtr);
                Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, InternalFormat, (int)size.X, (int)size.Y);
            }

            if (Pointer == 0) return;
            base.Upload(size, data, pixelFormat, internalFormat, pixelType);
        }

        public override void Dispose()
        {
            if (RenderBufferPtr != 0)
                GLThread.ExecuteGLThreadAsync(() => Gl.DeleteRenderbuffers(RenderBufferPtr));

            base.Dispose();
        }
    }
}