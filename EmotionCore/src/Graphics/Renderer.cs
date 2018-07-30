// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Graphics.GLES;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;

#endregion

namespace Emotion.Graphics
{
    public sealed unsafe class Renderer : ContextObject
    {
        #region Render State

        public TransformationStack TransformationStack { get; private set; }

        private MapBuffer _mainBuffer;
        private MapBuffer _outlineBuffer;

        #endregion

        #region Limits

        public static readonly int MaxRenderable = 10922;

        #endregion

        #region Initialization

        internal Renderer(Context context) : base(context)
        {
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "Loading Emotion OpenTK-GLES Renderer...");
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GL: " + GL.GetString(StringName.Version) + " on " + GL.GetString(StringName.Renderer));
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GLSL: " + GL.GetString(StringName.ShadingLanguageVersion));

            // Setup render state.
            TransformationStack = new TransformationStack();

            // Create a default program, and use it.
            ShaderProgram defaultProgram = new ShaderProgram(null, null);
            defaultProgram.Bind();

            // Setup main map buffer.
            _mainBuffer = new MapBuffer(MaxRenderable, this);
            _outlineBuffer = new MapBuffer(MaxRenderable, this);
            _mainBuffer.Start();
            _outlineBuffer.Start();

            // Check if the setup encountered any errors.
            Helpers.CheckError("renderer setup");

            // Setup additional GL arguments.
            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        internal void Destroy()
        {
            _mainBuffer.Delete();
            _outlineBuffer.Delete();
        }

        #endregion

        #region System API

        /// <summary>
        /// Clear the screen.
        /// </summary>
        public void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Helpers.CheckError("clear");
        }

        /// <summary>
        /// Flush the system buffers and restart their mapping.
        /// </summary>
        public void End()
        {
            if (_mainBuffer.Mapping && _mainBuffer.AnythingMapped) RenderFlush();

            if (_outlineBuffer.Mapping && _outlineBuffer.AnythingMapped) RenderOutlineFlush();
        }

        #endregion

        #region New Drawing API

        public void RenderFlush()
        {
            _mainBuffer.Draw();
            _mainBuffer.Start();
        }

        public void RenderOutlineFlush()
        {
            _outlineBuffer.Draw(PrimitiveType.LineLoop);
            _outlineBuffer.Start();
        }

        public void Render(Renderable2D renderable)
        {
            // Add the model matrix to the stack.
            TransformationStack.Push(renderable.ModelMatrix);

            // Pass the renderable.
            _mainBuffer.Add(Vector3.Zero, renderable.Size, renderable.Color, renderable.Texture, renderable.TextureArea, TransformationStack.CurrentMatrix);

            // Remove the model matrix from the stack.
            TransformationStack.Pop();
        }

        public void Render(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null, Matrix4? vertMatrix = null)
        {
            _mainBuffer.Add(location, size, color, texture, textureArea, vertMatrix);
        }

        public void RenderOutline(Renderable2D renderable)
        {
            // Add the model matrix to the stack.
            TransformationStack.Push(renderable.ModelMatrix);

            // Pass the renderable.
            _outlineBuffer.Add(Vector3.Zero, renderable.Size, renderable.Color, renderable.Texture, renderable.TextureArea, TransformationStack.CurrentMatrix);

            // Remove the model matrix from the stack.
            TransformationStack.Pop();
        }

        public void RenderOutline(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null, Matrix4? vertMatrix = null)
        {
            _outlineBuffer.Add(location, size, color, texture, textureArea, vertMatrix);
        }

        #endregion

        #region Old Drawing APIs

        #region Primitive Drawing

        /// <summary>
        /// Draws a rectangle outline on the current render target, which by default is the window.
        /// </summary>
        /// <param name="rect">The rectangle to outline.</param>
        /// <param name="color">The color of the outline.</param>
        /// <param name="camera">
        /// Whether the rectangle location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        [Obsolete("v0 function, deprecated. Use RenderOuline.")]
        public void DrawRectangleOutline(Rectangle rect, Color color, bool camera = true)
        {
            if (!camera) TransformationStack.Push(Matrix4.Identity);
            _outlineBuffer.Add(new Vector3(rect.X, rect.Y, 0), rect.Size, color);
            if (!camera) TransformationStack.Pop();
        }

        /// <summary>
        /// Draws a filled rectangle on the screen.
        /// </summary>
        /// <param name="rect">The rectangle to draw.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="camera">
        /// Whether the rectangle location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        [Obsolete("v0 function, deprecated. Use Render.")]
        public void DrawRectangle(Rectangle rect, Color color, bool camera = true)
        {
            if (!camera) TransformationStack.Push(Matrix4.Identity);
            _mainBuffer.Add(new Vector3(rect.X, rect.Y, 0), rect.Size, color);
            if (!camera) TransformationStack.Pop();
        }

        /// <summary>
        /// Draws a line on the current render target, which by default is the window.
        /// </summary>
        /// <param name="start">The line's starting point.</param>
        /// <param name="end">The line's ending point.</param>
        /// <param name="color">The line's color.</param>
        /// <param name="camera">
        /// Whether the line's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        [Obsolete("v0 function, deprecated. Create a map buffer of the line type and use that to render lines.")]
        public void DrawLine(Vector2 start, Vector2 end, Color color, bool camera = true)
        {
            Debugger.Log(MessageType.Error, MessageSource.Renderer, "[DEPRECATED] Renderer.DrawLine was called.");
        }

        #endregion

        #region Texture Drawing

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        [Obsolete("v0 function, deprecated. Use Render.")]
        public void DrawTexture(Texture texture, Rectangle location, bool camera = true)
        {
            if (!camera) TransformationStack.Push(Matrix4.Identity);
            _mainBuffer.Add(new Vector3(location.X, location.Y, 0), location.Size, Color.White, texture);
            if (!camera) TransformationStack.Pop();
        }

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="source">Which part of the texture to draw.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        [Obsolete("v0 function, deprecated. Use Render.")]
        public void DrawTexture(Texture texture, Rectangle location, Rectangle source, bool camera = true)
        {
            if (!camera) TransformationStack.Push(Matrix4.Identity);
            _mainBuffer.Add(new Vector3(location.X, location.Y, 0), location.Size, Color.White, texture, source);
            if (!camera) TransformationStack.Pop();
        }

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="color">Tints the texture in the provided color. Opacity can be controlled through the color alpha.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        [Obsolete("v0 function, deprecated. Use Render.")]
        public void DrawTexture(Texture texture, Rectangle location, Color color, bool camera = true)
        {
            if (!camera) TransformationStack.Push(Matrix4.Identity);
            _mainBuffer.Add(new Vector3(location.X, location.Y, 0), location.Size, color, texture);
            if (!camera) TransformationStack.Pop();
        }

        /// <summary>
        /// Draw a texture on the current render target, which by default is the window.
        /// </summary>
        /// <param name="texture">The texture to draw.</param>
        /// <param name="location">Where to draw the texture.</param>
        /// <param name="source">Which part of the texture to draw.</param>
        /// <param name="color">Tints the texture in the provided color. Opacity can be controlled through the color alpha.</param>
        /// <param name="camera">
        /// Whether the texture's location should be in world coordinates (camera), or pixel coordinates
        /// (screen).
        /// </param>
        [Obsolete("v0 function, deprecated. Use Render.")]
        public void DrawTexture(Texture texture, Rectangle location, Rectangle source, Color color, bool camera = true)
        {
            if (!camera) TransformationStack.Push(Matrix4.Identity);
            _mainBuffer.Add(new Vector3(location.X, location.Y, 0), location.Size, color, texture, source);
            if (!camera) TransformationStack.Pop();
        }

        #endregion

        #endregion
    }
}