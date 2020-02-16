﻿#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using OpenGL;

#endregion

namespace Emotion.Graphics.Objects
{
    public sealed class FrameBuffer : IDisposable
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
        /// The OpenGL pointer to this FrameBuffer's render buffer, if any.
        /// It exists, for instance, if you want to bind a depth attachment without a depth texture.
        /// </summary>
        public uint RenderBuffer { get; set; }

        /// <summary>
        /// The size you should treat the framebuffer as. The texture size is the actual size
        /// while the viewport size is the render size.
        /// </summary>
        public Vector2 Size { get; set; }

        /// <summary>
        /// The texture which represents this frame buffer.
        /// </summary>
        public Texture Texture { get; set; }

        /// <summary>
        /// Depth texture of the frame buffer. Optional - depending on constructor.
        /// </summary>
        public Texture DepthTexture { get; set; }

        /// <summary>
        /// The frame buffer's viewport.
        /// </summary>
        public Rectangle Viewport { get; set; }

        /// <summary>
        /// Whether the framebuffer has a stencil attachment.
        /// </summary>
        private bool _stencil;

        /// <summary>
        /// Whether the framebuffer has a depth attachment.
        /// </summary>
        private bool _depth;

        /// <summary>
        /// Default and constant framebuffer color attachments.
        /// </summary>
        private static int[] _modes = {Gl.COLOR_ATTACHMENT0};

        /// <summary>
        /// Create an object representation of a frame buffer from a frame buffer id.
        /// </summary>
        /// <param name="bufferId">The id of the already created buffer.</param>
        /// <param name="size">The size of the frame buffer.</param>
        public FrameBuffer(uint bufferId, Vector2 size)
        {
            Pointer = bufferId;
            Size = size;
            Viewport = new Rectangle(0, 0, size);
        }

        /// <summary>
        /// Create a new frame buffer.
        /// </summary>
        /// <param name="texture">The texture to use for this frame buffer. The frame buffer's size is inferred from it.</param>
        /// <param name="depthTexture">
        /// The depth texture. Should be the same size as the texture. If stencil is attached this is
        /// also the stencil texture. Optional.
        /// </param>
        /// <param name="attachStencil">Whether to attach a stencil attachment.</param>
        /// <param name="attachDepth">Whether to attach a depth attachment. If a depth texture is provided this is overriden. Also required if stencil is requested.</param>
        public FrameBuffer(Texture texture, Texture depthTexture = null, bool attachStencil = false, bool attachDepth = true)
        {
            Texture = texture;
            DepthTexture = depthTexture;
            Size = texture?.Size ?? depthTexture?.Size ?? Vector2.Zero;
            Pointer = Gl.GenFramebuffer();
            Viewport = new Rectangle(0, 0, Size);
            _stencil = attachStencil;
            _depth = attachDepth || DepthTexture != null || _stencil;

            Create();
        }

        public FrameBuffer(Texture texture) : this(texture, null)
        {
        }

        public FrameBuffer(Texture texture, bool attachStencil = false) : this(texture, null, attachStencil)
        {
        }

        /// <summary>
        /// Framebuffer creation.
        /// </summary>
        private void Create()
        {
            EnsureBound(Pointer);

            // Attach the texture to the frame buffer.
            if (Texture != null) Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, Texture.Pointer, 0);

            // Attach depth texture (if any)
            FramebufferAttachment attachment = _stencil ? FramebufferAttachment.DepthStencilAttachment : FramebufferAttachment.DepthAttachment;
            InternalFormat internalFormat = _stencil ? InternalFormat.Depth24Stencil8 : InternalFormat.DepthComponent24;
            if (DepthTexture != null)
            {
                // Use a depth texture to hold the depth attachment.
                DepthTexture.Upload(Size, null, internalFormat, _stencil ? PixelFormat.DepthStencil : PixelFormat.DepthComponent, _stencil ? PixelType.UnsignedInt248 : PixelType.Float);
                Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2d, DepthTexture.Pointer, 0);
            }
            else if(_depth)
            {
                // Create render buffer. This is the object that holds the depth and stencil attachments.
                RenderBuffer = Gl.GenRenderbuffer();
                Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBuffer);
                Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, (int) Size.X, (int) Size.Y);
                Gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachment, RenderbufferTarget.Renderbuffer, RenderBuffer);
            }

            // Attach color components.
            Gl.DrawBuffers(_modes);

            // Check status.
            FramebufferStatus status = Engine.Renderer.Dsa ? Gl.CheckNamedFramebufferStatus(Pointer, FramebufferTarget.Framebuffer) : Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferStatus.FramebufferComplete) Engine.Log.Warning($"FrameBuffer creation failed. Error code {status}.", MessageSource.GL);

            // Clear the target.
            EnsureBound(Pointer);
            Engine.Renderer.ClearFrameBuffer();

            // Restore bindings and so on.
            Engine.Renderer?.EnsureRenderTarget();
        }

        /// <summary>
        /// Bind the framebuffer. Also sets the current viewport to the frame buffer's viewport.
        /// </summary>
        public void Bind()
        {
            EnsureBound(Pointer);
            if (Texture == null && Pointer != 0) Gl.DrawBuffer(DrawBufferMode.None);
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
                Gl.ReadPixels((int) rect.X, (int) rect.Y, (int) rect.Width, (int) rect.Height, Texture?.PixelFormat ?? PixelFormat.Bgra, Texture?.PixelType ?? PixelType.UnsignedByte,
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
            var data = new byte[(int) (rect.Width * rect.Height) * Gl.PixelTypeToByteCount(Texture?.PixelType ?? PixelType.UnsignedByte) *
                                Gl.PixelTypeToComponentCount(Texture?.PixelFormat ?? PixelFormat.Bgra)];
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
            if(newSize == Size) return;

            // Check if re-creation can be skipped by referencing the buffer as a smaller size.
            Texture t = Texture ?? DepthTexture;
            if (t.Size.X >= newSize.X && t.Size.Y >= newSize.Y)
            {
                Size = newSize;
            }
            else
            {
                if(RenderBuffer != 0) Gl.DeleteRenderbuffers(RenderBuffer);

                // Reset size holders.
                Size = newSize;
                Viewport = new Rectangle(Viewport.Location, newSize);

                // Re-create textures and framebuffer.
                Texture?.Upload(newSize, null);
                DepthTexture?.Upload(newSize, null);
                Create();
            }
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

            Texture?.Dispose();
            Gl.DeleteFramebuffers(Pointer);
            if (DepthTexture != null)
            {
                DepthTexture.Dispose();
                DepthTexture = null;
            }

            if (RenderBuffer != 0) Gl.DeleteRenderbuffers(RenderBuffer);
            Texture = null;
            Pointer = 0;
            RenderBuffer = 0;
        }

        #endregion
    }
}