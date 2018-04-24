// Emotion - https://github.com/Cryru/Emotion

#if GLES

#region Using

using System;
using Emotion.Game.Objects.Camera;
using Emotion.Platform.Base;
using Emotion.Platform.Base.Assets;
using Emotion.Platform.Base.Objects;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Platform.GLES
{
    /// <inheritdoc cref="Renderer" />
    public class GLRenderer : Renderer
    {
        #region Properties

        public override CameraBase Camera { get; set; }

        /// <summary>
        /// The context this object belongs to.
        /// </summary>
        internal GLContext EmotionContext { get; set; }

        #endregion

        #region Primary Functions
        
        internal GLRenderer(GLContext context)
        {
            EmotionContext = context;
        }

        public override void Clear(Color color)
        {
            GL.ClearColor(color.R, color.G, color.B, color.A);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public override void Present()
        {
            EmotionContext.Window.SwapBuffers();
        }

        internal override void Destroy()
        {
        }

        #endregion

        public override void DrawTexture(Texture texture, Rectangle location, Rectangle source, float opacity, bool camera = true)
        {
            throw new NotImplementedException();
        }

        public override void DrawTexture(Texture texture, Rectangle location, Rectangle source, bool camera = true)
        {
            throw new NotImplementedException();
        }

        public override void DrawTexture(Texture texture, Rectangle location, bool camera = true)
        {
            throw new NotImplementedException();
        }

        public override void DrawRectangleOutline(Rectangle rect, Color color, bool camera = true)
        {
            throw new NotImplementedException();
        }

        public override void DrawRectangle(Rectangle rect, Color color, bool camera = true)
        {
            throw new NotImplementedException();
        }

        public override void DrawLine(Vector2 start, Vector2 end, Color color, bool camera = true)
        {
            throw new NotImplementedException();
        }

        public override TextDrawingSession StartTextSession(Font font, int fontSize, int width, int height)
        {
            throw new NotImplementedException();
        }

        public override void DrawText(Font font, int size, string text, Color color, Vector2 location, bool camera = true)
        {
            throw new NotImplementedException();
        }

        public override void DrawText(Font font, int size, string[] text, Color color, Vector2 location, bool camera = true)
        {
            throw new NotImplementedException();
        }
    }
}

#endif