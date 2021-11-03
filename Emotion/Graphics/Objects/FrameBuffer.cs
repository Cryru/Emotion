#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Game.Time.Routines;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    /// <summary>
    /// Represents a FrameBuffer, also known as a "render target", "drawable texture" and others.
    /// This class is also used to represent the primary frame buffer (screen buffer) in which case the GL Pointer is 0.
    /// </summary>
    public class FrameBuffer : IDisposable
    {
        /// <summary>
        /// The currently bound frame buffer.
        /// </summary>
        public static uint Bound { get; set; }

        /// <summary>
        /// The OpenGL pointer to this FrameBuffer.
        /// </summary>
        public uint Pointer { get; set; }

        /// <summary>
        /// The size allocated for the framebuffer. This is the actual size of the textures.
        /// </summary>
        public Vector2 AllocatedSize { get; set; }

        /// <summary>
        /// The size you should treat the framebuffer as. The texture size is the actual size
        /// while the viewport size is the render size.
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// The frame buffer's viewport.
        /// </summary>
        public Rectangle Viewport { get; set; }

        /// <summary>
        /// The color attachment of the FrameBuffer if any.
        /// </summary>
        public FrameBufferTexture ColorAttachment { get; protected set; }

        /// <summary>
        /// The depth, or depth and stencil, attachment of the FrameBuffer if any.
        /// </summary>
        public FrameBufferTexture DepthStencilAttachment { get; protected set; }

        #region Legacy

        /// <summary>
        /// The texture which represents this frame buffer.
        /// This is a legacy getter and redirects to ColorAttachment.
        /// </summary>
        public Texture Texture
        {
            get => ColorAttachment;
        }

        /// <summary>
        /// Depth texture of the frame buffer.
        /// This is a legacy parameter redirecting to DepthStencilAttachment.
        /// </summary>
        public Texture DepthTexture
        {
            get => DepthStencilAttachment;
        }

        #endregion

        #region Unsynch Sampling

        /// <summary>
        /// PBO to be used for async sampling.
        /// </summary>
        private PixelBuffer _pbo;

        /// <summary>
        /// The current request if any. Only one can be active.
        /// </summary>
        private FrameBufferSampleRequest _sampleRequest;

        #endregion

        /// <summary>
        /// Create an object representation of a frame buffer from a frame buffer id.
        /// </summary>
        /// <param name="bufferId">The id of the already created buffer.</param>
        /// <param name="size">The size of the frame buffer.</param>
        public FrameBuffer(uint bufferId, Vector2 size)
        {
            Pointer = bufferId;
            Size = size;
            AllocatedSize = size;
            Viewport = new Rectangle(0, 0, size);
        }

        /// <summary>
        /// Create a new frame buffer of the given size.
        /// </summary>
        /// <param name="size"></param>
        public FrameBuffer(Vector2 size)
        {
            Pointer = Gl.GenFramebuffer();
            Size = size;
            AllocatedSize = size;
            Viewport = new Rectangle(0, 0, Size);
        }

        #region Factory

        /// <summary>
        /// Add a depth attachment to the framebuffer.
        /// </summary>
        /// <param name="bindable">Whether the attachment will be bindable as a texture.</param>
        /// <returns>This framebuffer - for linking.</returns>
        public FrameBuffer WithDepth(bool bindable = false)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (!bindable)
                DepthStencilAttachment = new FrameBufferTexture(this, CreateRenderBuffer(InternalFormat.DepthComponent24, FramebufferAttachment.DepthAttachment), Size,
                    InternalFormat.DepthComponent24);
            else
                DepthStencilAttachment = CreateTexture(InternalFormat.DepthComponent24, PixelFormat.DepthComponent, PixelType.UnsignedInt, FramebufferAttachment.DepthAttachment);

            return this;
        }

        /// <summary>
        /// Add a depth and stencil attachments to the framebuffer.
        /// </summary>
        /// <param name="bindable">Whether the attachment will be bindable as a texture.</param>
        /// <returns>This framebuffer - for linking.</returns>
        public FrameBuffer WithDepthStencil(bool bindable = false)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (!bindable)
                DepthStencilAttachment = new FrameBufferTexture(this, CreateRenderBuffer(InternalFormat.Depth24Stencil8, FramebufferAttachment.DepthStencilAttachment), Size,
                    InternalFormat.Depth24Stencil8);
            else
                DepthStencilAttachment = CreateTexture(InternalFormat.Depth24Stencil8, PixelFormat.DepthStencil, PixelType.UnsignedInt248, FramebufferAttachment.DepthStencilAttachment);

            return this;
        }

        private static readonly int[] ColorModes = { Gl.COLOR_ATTACHMENT0 };

        /// <summary>
        /// Add a color attachment to the framebuffer.
        /// </summary>
        /// <param name="bindable">Whether the attachment will be bindable as a texture.</param>
        /// <returns>This framebuffer - for linking.</returns>
        public FrameBuffer WithColor(bool bindable = true)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (!bindable)
                ColorAttachment = new FrameBufferTexture(this, CreateRenderBuffer(InternalFormat.Rgba, FramebufferAttachment.ColorAttachment0), Size, InternalFormat.Rgba);
            else
                ColorAttachment = CreateTexture(InternalFormat.Rgba, PixelFormat.Bgra, PixelType.UnsignedByte, FramebufferAttachment.ColorAttachment0);

            Gl.DrawBuffers(ColorModes);

            return this;
        }

        /// <summary>
        /// Check if any errors occured in the creation of the frame buffer.
        /// </summary>
        public void CheckErrors()
        {
            FramebufferStatus status = Engine.Renderer.Dsa ? Gl.CheckNamedFramebufferStatus(Pointer, FramebufferTarget.Framebuffer) : Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferStatus.FramebufferComplete) Engine.Log.Warning($"FrameBuffer creation failed. Error code {status}.", MessageSource.GL);
        }

        private uint CreateRenderBuffer(InternalFormat internalFormat, FramebufferAttachment attachment)
        {
            EnsureBound(Pointer);
            uint buffer = Gl.GenRenderbuffer();
            Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, buffer);
            Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, (int)Size.X, (int)Size.Y);
            Gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachment, RenderbufferTarget.Renderbuffer, buffer);
            return buffer;
        }

        private FrameBufferTexture CreateTexture(InternalFormat internalFormat, PixelFormat pixelFormat, PixelType pixelType, FramebufferAttachment attachment)
        {
            var texture = new FrameBufferTexture(this, Size, internalFormat, pixelFormat, pixelType)
            {
                FlipY = true // Framebuffer textures are always flipped.
            };
            EnsureBound(Pointer);
            Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2d, texture.Pointer, 0);
            return texture;
        }

        #endregion

        /// <summary>
        /// Bind the framebuffer. Unlike EnsureBound this sets the current viewport to the frame buffer's viewport.
        /// </summary>
        public void Bind()
        {
            EnsureBound(Pointer);
            if (ColorAttachment == null && Pointer != 0) Gl.DrawBuffers((int)DrawBufferMode.None);
            Gl.Viewport((int)Viewport.X, (int)Viewport.Y, (int)Viewport.Width, (int)Viewport.Height);
        }

        /// <summary>
        /// Sample data from the framebuffer.
        /// </summary>
        /// <param name="rect">The rectangle to sample data from in. Top left origin.</param>
        /// <param name="data">The array to fill. You need to allocate one which is long enough to receive the data.</param>
        /// <param name="format">The pixel format to return the pixels in.</param>
        public unsafe bool Sample(Rectangle rect, byte[] data, PixelFormat format)
        {
            rect = rect.ClampTo(Viewport);
            rect = new Rectangle(rect.X, Size.Y - (rect.Y + rect.Height), rect.Width, rect.Height);

            FrameBuffer previouslyBound = Engine.Renderer.CurrentTarget;
            Bind();
            if (data != null)
                fixed (byte* pixelBuffer = &data[0])
                {
                    Gl.ReadPixels((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, format, ColorAttachment?.PixelType ?? PixelType.UnsignedByte,
                        (IntPtr)pixelBuffer);
                }
            else
                Gl.ReadPixels((int)rect.X, (int)rect.Y, (int)rect.Width, (int)rect.Height, format, ColorAttachment?.PixelType ?? PixelType.UnsignedByte,
                    IntPtr.Zero);

            previouslyBound.Bind();
            return true;
        }

        /// <summary>
        /// Sample data from the framebuffer.
        /// </summary>
        /// <param name="rect">The rectangle to sample data from in viewport coordinates. Top left origin.</param>
        /// <param name="format">The pixel format to return the pixels in.</param>
        public byte[] Sample(Rectangle rect, PixelFormat format)
        {
            var data = new byte[(int)(rect.Width * rect.Height) * Gl.PixelTypeToByteCount(ColorAttachment?.PixelType ?? PixelType.UnsignedByte) * Gl.PixelFormatToComponentCount(format)];
            Sample(rect, data, format);
            return data;
        }

        /// <summary>
        /// Sample the buffer non-synchronously. Only one sample request can be active at a time, requesting another one before
        /// the current one is finished will return the current one irregardless if the requested rect is the same.
        /// </summary>
        /// <param name="rect">The rectangle to sample data from in viewport coordinates. Top left origin.</param>
        /// <param name="format">The pixel format to return the pixels in.</param>
        /// <param name="data">Optional data pointer to fill with the data.</param>
        /// <returns>null if invalid request, or a IRoutineWaiter framebuffer sample request otherwise</returns>
        public FrameBufferSampleRequest SampleUnsynch(Rectangle rect, PixelFormat format, byte[] data = null)
        {
            if (_sampleRequest != null) return _sampleRequest;
            var sampleRequest = new FrameBufferSampleRequest();
            _sampleRequest = sampleRequest;

            uint byteSize = (uint)(rect.Width * rect.Height) *
                            Gl.PixelTypeToByteCount(ColorAttachment?.PixelType ?? PixelType.UnsignedByte) *
                            Gl.PixelFormatToComponentCount(format);

            // Allocate data if needed.
            if (data == null || data.Length < byteSize) data = new byte[byteSize];

            // Either make a new pbo or resize if needed.
            if (_pbo == null)
                _pbo = new PixelBuffer(byteSize);
            else if (_pbo.Size < byteSize)
                _pbo.Upload(IntPtr.Zero, byteSize, BufferUsage.StreamCopy);

            // Create a request. This is basically a normal sample with a bound PBO
            PixelBuffer.EnsureBound(_pbo.Pointer);
            if (!Sample(rect, null, format))
            {
                _sampleRequest = null;
                PixelBuffer.EnsureBound(0);
                return null;
            }

            var newFence = new Fence();
            PixelBuffer.EnsureBound(0);

            // Poll the request until the fence has been signaled.
            void UpdateRequest()
            {
                if (!newFence.IsSignaled())
                {
                    // If not ready, reinsert back into the GLThread queue.
                    GLThread.ExecuteGLThreadAsync(UpdateRequest);
                    return;
                }

                // Read the data from the PBO.
                PixelBuffer.EnsureBound(_pbo.Pointer);
                Span<byte> mapper = _pbo.CreateMapper<byte>(0, (int)byteSize);
                mapper.CopyTo(new Span<byte>(data));
                _pbo.FinishMapping();
                PixelBuffer.EnsureBound(0);
                _sampleRequest.Data = data;
                _sampleRequest = null;
            }

            UpdateRequest();
            return sampleRequest;
        }

        /// <summary>
        /// Resize the framebuffer.
        /// If the requested size is smaller than the current texture size, it will be reused.
        /// </summary>
        /// <param name="newSize"></param>
        /// <param name="reuseAttachments">Whether to reuse attachments if they are bigger than the new size.</param>
        public void Resize(Vector2 newSize, bool reuseAttachments = false)
        {
            newSize = newSize.IntCastRound();

            // Quick exit.
            if (newSize == Size) return;

            Size = newSize;
            Viewport = new Rectangle(0, 0, newSize);

            if (reuseAttachments && AllocatedSize.X >= newSize.X && AllocatedSize.Y >= newSize.Y && Pointer != 0) return;

            // Re-create textures and framebuffer.
            AllocatedSize = newSize;
            ColorAttachment?.Upload(newSize, null);
            DepthStencilAttachment?.Upload(newSize, null);
            if (Pointer != 0) CheckErrors();
        }

        /// <summary>
        /// Ensures the provided pointer is the currently bound framebuffer.
        /// </summary>
        /// <param name="pointer">The pointer to ensure is bound.</param>
        private static void EnsureBound(uint pointer)
        {
            // Check if it is already bound.
            if (Bound == pointer && pointer != 0)
            {
                // If in debug mode, verify this with OpenGL.
                if (!Engine.Configuration.GlDebugMode) return;

                Gl.GetInteger(GetPName.DrawFramebufferBinding, out int actualBound);
                if (actualBound != pointer) Engine.Log.Error($"Assumed frame buffer was {pointer} but it was {actualBound}.", MessageSource.GL);
                return;
            }

            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, pointer);
            Bound = pointer;

            // Some drivers invalidate bound textures if they are framebuffer textures once the framebuffer changes.
            Texture.Bound[0] = 0;
        }

        /// <summary>
        /// Cleanup used resources.
        /// </summary>
        public void Dispose()
        {
            if (Pointer == 0 || Engine.Host == null) return;
            if (Bound == Pointer) Bound = 0;

            Gl.DeleteFramebuffers(Pointer);
            ColorAttachment?.Dispose();
            ColorAttachment = null;
            DepthStencilAttachment?.Dispose();
            DepthStencilAttachment = null;
            _pbo?.Dispose();
            _pbo = null;
        }
    }

    /// <summary>
    /// Dummy used by FrameBuffer.SampleUnsynch
    /// </summary>
    public class FrameBufferSampleRequest : IRoutineWaiter
    {
        /// <inheritdoc />
        public bool Finished
        {
            get => Data != null;
        }

        /// <summary>
        /// Sampled data. If null the sampling isn't done.
        /// </summary>
        public byte[] Data { get; set; }

        /// <inheritdoc />
        public void Update()
        {
        }
    }
}