#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
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
        public FrameBuffer WidhDepth(bool bindable = false)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (!bindable)
                DepthStencilAttachment = new FrameBufferTexture(CreateRenderBuffer(InternalFormat.DepthComponent24, FramebufferAttachment.DepthAttachment), Size);
            else
                DepthStencilAttachment = CreateTexture(InternalFormat.DepthComponent24, PixelFormat.DepthComponent, FramebufferAttachment.DepthAttachment);

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
                DepthStencilAttachment = new FrameBufferTexture(CreateRenderBuffer(InternalFormat.Depth24Stencil8, FramebufferAttachment.DepthStencilAttachment), Size);
            else
                DepthStencilAttachment = CreateTexture(InternalFormat.Depth24Stencil8, PixelFormat.DepthStencil, FramebufferAttachment.DepthStencilAttachment);

            return this;
        }

        private static readonly int[] ColorModes = {Gl.COLOR_ATTACHMENT0};

        /// <summary>
        /// Add a color attachment to the framebuffer.
        /// </summary>
        /// <param name="bindable">Whether the attachment will be bindable as a texture.</param>
        /// <returns>This framebuffer - for linking.</returns>
        public FrameBuffer WithColor(bool bindable = true)
        {
            // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression
            if (!bindable)
                ColorAttachment = new FrameBufferTexture(CreateRenderBuffer(InternalFormat.Rgba, FramebufferAttachment.ColorAttachment0), Size);
            else
                ColorAttachment = CreateTexture(InternalFormat.Rgba, PixelFormat.Bgra, FramebufferAttachment.ColorAttachment0);

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
            Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, (int) Size.X, (int) Size.Y);
            Gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachment, RenderbufferTarget.Renderbuffer, buffer);
            return buffer;
        }

        private FrameBufferTexture CreateTexture(InternalFormat internalFormat, PixelFormat pixelFormat, FramebufferAttachment attachment)
        {
            var texture = new FrameBufferTexture(Size, internalFormat, pixelFormat);
            EnsureBound(Pointer);
            Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2d, texture.Pointer, 0);
            return texture;
        }

        #endregion

        /// <summary>
        /// Bind the framebuffer. Also sets the current viewport to the frame buffer's viewport.
        /// </summary>
        public void Bind()
        {
            EnsureBound(Pointer);
            if (ColorAttachment == null && Pointer != 0) Gl.DrawBuffer(DrawBufferMode.None);
            Gl.Viewport((int) Viewport.X, (int) Viewport.Y, (int) Viewport.Width, (int) Viewport.Height);
        }

        /// <summary>
        /// Sample data from the framebuffer.
        /// </summary>
        /// <param name="rect">The rectangle to sample data from in. Top left origin.</param>
        /// <param name="data">The array to fill. You need to allocate one which is long enough to receive the data.</param>
        public unsafe byte[] Sample(Rectangle rect, ref byte[] data)
        {
            if (!Viewport.Contains(rect)) return data;

            rect = new Rectangle(rect.X, Size.Y - (rect.Y + rect.Height), rect.Width, rect.Height);
            Bind();
            fixed (byte* pixelBuffer = &data[0])
            {
                Gl.ReadPixels((int) rect.X, (int) rect.Y, (int) rect.Width, (int) rect.Height, ColorAttachment?.PixelFormat ?? PixelFormat.Bgra, ColorAttachment?.PixelType ?? PixelType.UnsignedByte,
                    (IntPtr) pixelBuffer);
            }

            return data;
        }

        /// <summary>
        /// Sample data from the framebuffer.
        /// </summary>
        /// <param name="rect">The rectangle to sample data from in viewport coordinates. Top left origin.</param>
        public byte[] Sample(Rectangle rect)
        {
            var data = new byte[(int) (rect.Width * rect.Height) * Gl.PixelTypeToByteCount(ColorAttachment?.PixelType ?? PixelType.UnsignedByte) *
                                Gl.PixelTypeToComponentCount(ColorAttachment?.PixelFormat ?? PixelFormat.Bgra)];
            return Sample(rect, ref data);
        }

        /// <summary>
        /// Resize the framebuffer.
        /// If the requested size is smaller than the current texture size, it will be reused.
        /// </summary>
        /// <param name="newSize"></param>
        public void Resize(Vector2 newSize)
        {
            // Quick exit.
            if (newSize == Size) return;

            Size = newSize;

            // Reset size holders.
            AllocatedSize = newSize;
            Viewport = new Rectangle(Viewport.Location, newSize);

            // Re-create textures and framebuffer.
            ColorAttachment?.Upload(newSize, null);
            DepthStencilAttachment?.Upload(newSize, null);
            CheckErrors();
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
                if (!Engine.Configuration.DebugMode) return;

                Gl.GetInteger(GetPName.DrawFramebufferBinding, out int actualBound);
                if (actualBound != pointer) Engine.Log.Error($"Assumed frame buffer was {pointer} but it was {actualBound}.", MessageSource.GL);
                return;
            }

            Gl.BindFramebuffer(FramebufferTarget.Framebuffer, pointer);
            Bound = pointer;
        }

        #region Cleanup

        public void Dispose()
        {
            if (Pointer == 0 || Engine.Host == null) return;
            if (Bound == Pointer) Bound = 0;

            Gl.DeleteFramebuffers(Pointer);
            ColorAttachment?.Dispose();
            ColorAttachment = null;
            DepthStencilAttachment?.Dispose();
            DepthStencilAttachment = null;
        }

        #endregion
    }
}