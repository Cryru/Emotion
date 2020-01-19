#region Using

using System;
using System.Diagnostics;
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
        /// The OpenGL pointer to this FrameBuffer's render buffer.
        /// </summary>
        public uint RenderBuffer { get; set; }

        /// <summary>
        /// The size of the framebuffer.
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
        /// <param name="depthTexture">The depth texture. Should be the same size as the texture. If stencil is attached this is also the stencil texture. Optional.</param>
        /// <param name="attachStencil">Whether to attach a stencil attachment.</param>
        public FrameBuffer(Texture texture, Texture depthTexture = null, bool attachStencil = false)
        {
            Texture = texture;
            Size = texture?.Size ?? depthTexture?.Size ?? Vector2.Zero;
            Pointer = Gl.GenFramebuffer();
            Viewport = new Rectangle(0, 0, Size);

            EnsureBound(Pointer);

            // Attach the texture to the frame buffer.
            if(texture != null) Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2d, texture.Pointer, 0);

            // Attach depth texture (if any)
            FramebufferAttachment attachment = attachStencil ? FramebufferAttachment.DepthStencilAttachment : FramebufferAttachment.DepthAttachment;
            InternalFormat internalFormat = attachStencil ? InternalFormat.Depth24Stencil8 : InternalFormat.DepthComponent24;
            if (depthTexture != null)
            {
                // Use a depth texture to hold the depth attachment.
                depthTexture.Upload(Size, null, internalFormat, attachStencil ? PixelFormat.DepthStencil : PixelFormat.DepthComponent, attachStencil ? PixelType.UnsignedInt248 : PixelType.Float);
                DepthTexture = depthTexture;
                Gl.FramebufferTexture2D(FramebufferTarget.Framebuffer, attachment, TextureTarget.Texture2d, depthTexture.Pointer, 0);
            }
            else
            {
                // Create render buffer. This is the object that holds the depth and stencil attachments.
                RenderBuffer = Gl.GenRenderbuffer();
                Gl.BindRenderbuffer(RenderbufferTarget.Renderbuffer, RenderBuffer);
                Gl.RenderbufferStorage(RenderbufferTarget.Renderbuffer, internalFormat, (int)Size.X, (int)Size.Y);
                Gl.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, attachment, RenderbufferTarget.Renderbuffer, RenderBuffer);
            }

            // Attach color components.
            Gl.DrawBuffers(_modes);

            // Check status.
            FramebufferStatus status = Engine.Renderer.Dsa ? Gl.CheckNamedFramebufferStatus(Pointer, FramebufferTarget.Framebuffer) : Gl.CheckFramebufferStatus(FramebufferTarget.Framebuffer);
            if (status != FramebufferStatus.FramebufferComplete) Engine.Log.Warning($"FrameBuffer creation failed. Error code {status}.", MessageSource.GL);

            // Clear the target.
            EnsureBound(Pointer);
            Engine.Renderer.Clear();

            // Restore bindings and so on.
            Engine.Renderer?.EnsureRenderTarget();
        }

        public FrameBuffer(Texture texture) : this(texture, null)
        {

        }

        public FrameBuffer(Texture texture, bool attachStencil = false) : this(texture, null, attachStencil)
        {
        }

        public void Bind()
        {
            EnsureBound(Pointer);
            if (Texture == null && Pointer != 0) Gl.DrawBuffer(DrawBufferMode.None);
            Gl.Viewport((int) Viewport.X, (int) Viewport.Y, (int) Viewport.Width, (int) Viewport.Height);
        }

        /// <summary>
        /// Ensures the provided pointer is the currently bound framebuffer.
        /// </summary>
        /// <param name="pointer">The pointer to ensure is bound.</param>
        public static void EnsureBound(uint pointer)
        {
            // Check if it is already bound.
            if (Bound == pointer && pointer != 0)
            {
                // If in debug mode, verify this with OpenGL.
                if (!Engine.Configuration.DebugMode) return;

                Gl.GetInteger(GetPName.DrawFramebufferBinding, out uint actualBound);
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
            if(RenderBuffer != 0) Gl.DeleteRenderbuffers(RenderBuffer);
            Texture = null;
            Pointer = 0;
            RenderBuffer = 0;
        }

        #endregion
    }
}