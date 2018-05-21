// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Utils;
using OpenTK;
using OpenTK.Graphics.ES30;
using Vector2 = Emotion.Primitives.Vector2;

#endregion

namespace Emotion.GLES
{
    /// <summary>
    /// A texture you cna draw on.
    /// </summary>
    /// <inheritdoc cref="ITexture" />
    public sealed class RenderTarget : ITexture
    {
        #region Properties

        public Vector2 Size
        {
            get => new Vector2(Width, Height);
        }

        public int Width { get; private set; }

        public int Height { get; private set; }

        public Matrix4 TextureMatrix { get; private set; }

        #endregion

        private int _pointer { get; set; }
        private Framebuffer _buffer;

        public RenderTarget(int width, int height)
        {
            Width = width;
            Height = height;

            // Create texture.
            _pointer = GL.GenTexture();
            TextureMatrix = Matrix4.CreateOrthographicOffCenter(0, Width * 2, Height * 2, 0, 0, 1);

            // Upload an empty texture.
            Use();

            // Set scaling to pixel perfect.
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (float) All.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (float) All.Nearest);

            GL.TexImage2D(TextureTarget2d.Texture2D, 0, TextureComponentCount.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            Helpers.CheckError("creating render target texture");

            // Create framebuffer.
            _buffer = new Framebuffer();
            _buffer.Use();

            // Link the framebuffer and the texture.
            GL.FramebufferTexture2D(FramebufferTarget.DrawFramebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget2d.Texture2D, _pointer, 0);
            Helpers.CheckError("linking framebuffer to texture");

            // Create and set dbe.
            DrawBufferMode[] dbe = {DrawBufferMode.ColorAttachment0};
            GL.DrawBuffers(1, dbe);

            // Check the buffer.
            if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
                throw new Exception("Couldn't load framebuffer.");
            Helpers.CheckError("creating render target");

            _buffer.StopUsing();
        }

        public void UseBuffer()
        {
            _buffer.Use();
        }

        #region Texture API

        public void Use()
        {
            GL.BindTexture(TextureTarget.Texture2D, _pointer);
        }

        public void Cleanup()
        {
            GL.DeleteTexture(_pointer);
            _buffer.Destroy();
        }

        #endregion
    }
}