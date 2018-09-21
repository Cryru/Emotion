// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Diagnostics;
using Emotion.Debug;
using Emotion.Engine;
using Emotion.Game.Camera;
using Emotion.Game.UI;
using Emotion.Game.UI.Layout;
using Emotion.Graphics.Batching;
using Emotion.Graphics.GLES;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using Emotion.Utils;
using OpenTK.Graphics.ES30;
using Debugger = Emotion.Debug.Debugger;

#endregion

namespace Emotion.Graphics
{
    // todo: Think of a way to prevent having to think about the Z dimension too much. Especially with UI.
    // - Maybe setup a different UIRender call.
    // - Maybe render the UI to a different render target.
    // - Maybe setup a UI buffer.
    // - Maybe setup a separate UI renderer.
    public sealed class Renderer : ContextObject
    {
        /// <summary>
        /// The maximum number of renderable object that can fit in one buffer. This limit is determined by the IBO data type being
        /// ushort.
        /// </summary>
        public static readonly int MaxRenderable = ushort.MaxValue;

        #region Objects

        /// <summary>
        /// The renderer's camera.
        /// </summary>
        public CameraBase Camera;

        /// <summary>
        /// The model matrix stack.
        /// </summary>
        public TransformationStack MatrixStack;

        #endregion

        #region Render State

        private QuadMapBuffer _mainBuffer;
        private LineMapBuffer _mainLineBuffer;

        #endregion

        #region Initialization

        internal Renderer(Context context) : base(context)
        {
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "Loading Emotion OpenTK-GLES Renderer...");
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GL: " + GL.GetString(StringName.Version) + " on " + GL.GetString(StringName.Renderer));
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GLSL: " + GL.GetString(StringName.ShadingLanguageVersion));

            // Create objects.
            Camera = new CameraBase(new Rectangle(0, 0, Context.Settings.RenderWidth, Context.Settings.RenderHeight));
            MatrixStack = new TransformationStack();

            // Create a default program, and use it.
            ShaderProgram defaultProgram = new ShaderProgram((Shader) null, null);
            defaultProgram.Bind();

            // Setup main map buffer.
            _mainBuffer = new QuadMapBuffer(MaxRenderable);
            _mainBuffer.Start();
            _mainLineBuffer = new LineMapBuffer(MaxRenderable);
            _mainLineBuffer.Start();

            // Check if the setup encountered any errors.
            Helpers.CheckError("renderer setup");

            // Setup additional GL arguments.
            GL.Enable(EnableCap.Blend);
            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.DepthTest);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

            // Setup debug.
            SetupDebug();
        }

        /// <summary>
        /// Destroy the renderer.
        /// </summary>
        internal void Destroy()
        {
            _mainBuffer.Delete();
            _mainLineBuffer.Delete();
        }

        #endregion

        #region Debugging API

        private Controller _debugUIController;
        private BasicTextBg _debugCameraDataText;
        private BasicTextBg _debugFpsCounterDataText;
        private CornerAnchor _cornerAnchor;

        private CameraBase _debugCamera;
        private bool _fpsCounter;
        private bool _frameByFrame;
        private bool _frameByFrameAdvance;
        private bool _drawMouse;

        [Conditional("DEBUG")]
        private void SetupDebug()
        {
            Context.ScriptingEngine.Expose("debugCamera",
                (Func<string>) (() =>
                {
                    _debugCamera = _debugCamera == null
                        ? new CameraBase(new Rectangle(Camera.Center.X, Camera.Center.Y, Context.Settings.RenderWidth, Context.Settings.RenderHeight)) {Zoom = Camera.Zoom / 2f}
                        : null;
                    _debugCameraDataText.Active = !_debugCameraDataText.Active;

                    return "Debug camera " + (_debugCamera == null ? "disabled." : "enabled.");
                }),
                "Enables the debug camera. Move it with the arrow keys. Invoke again to cancel.");

            Context.ScriptingEngine.Expose("fps",
                (Func<string>) (() =>
                {
                    _fpsCounter = !_fpsCounter;
                    _debugFpsCounterDataText.Active = !_debugFpsCounterDataText.Active;

                    return "Fps counter " + (_fpsCounter ? "enabled." : "disabled.");
                }),
                "Enables the fps counter. Invoke again to cancel.");

            Context.ScriptingEngine.Expose("fbf",
                (Func<string>) (() =>
                {
                    _frameByFrame = !_frameByFrame;
                    _frameByFrameAdvance = false;

                    return "Frame by frame mode " + (_frameByFrame ? "enabled." : "disabled.");
                }),
                "Enables the frame by frame mode. Press F11 to advance or hold F12 to fast forward. Invoke again to cancel.");

            Context.ScriptingEngine.Expose("debugMouse",
                (Func<string>) (() =>
                {
                    _drawMouse = !_drawMouse;

                    return "Mouse square drawing is " + (_drawMouse ? "enabled." : "disabled.");
                }),
                "Enables drawing a square around the mouse cursor. Invoke again to cancel.");

            // Define UI components for debugging.
            _debugUIController = new Controller(Context);

            Font font = Context.AssetLoader.Get<Font>("debugFont.otf");
            _debugCameraDataText = new BasicTextBg(font, 10, "", Color.Yellow, new Color(0, 0, 0, 125), new Vector2(0, 0), 5) {Padding = new Rectangle(3, 3, 3, 3), Active = false};
            _debugFpsCounterDataText = new BasicTextBg(font, 10, "", Color.Yellow, new Color(0, 0, 0, 125), new Vector2(0, 0), 5) {Padding = new Rectangle(3, 3, 3, 3), Active = false};

            _cornerAnchor = new CornerAnchor();
            _cornerAnchor.AddControl(_debugCameraDataText, AnchorLocation.BottomLeft);
            _cornerAnchor.AddControl(_debugFpsCounterDataText, AnchorLocation.TopLeft);

            _debugUIController.Add(_cornerAnchor);
            _debugUIController.Add(_debugCameraDataText);
            _debugUIController.Add(_debugFpsCounterDataText);
        }

        [Conditional("DEBUG")]
        private void UpdateDebug()
        {
            _debugUIController.Update();

            // Update debugging camera.
            if (_debugCamera != null)
            {
                if (Context.Input.IsKeyHeld("Down")) _debugCamera.Y += 10 + 0.1f * Context.FrameTime;
                if (Context.Input.IsKeyHeld("Right")) _debugCamera.X += 10 + 0.1f * Context.FrameTime;
                if (Context.Input.IsKeyHeld("Up")) _debugCamera.Y -= 10 + 0.1f * Context.FrameTime;
                if (Context.Input.IsKeyHeld("Left")) _debugCamera.X -= 10 + 0.1f * Context.FrameTime;

                float scrollPos = Context.Input.GetMouseScrollRelative();
                if (scrollPos < 0) _debugCamera.Zoom += 0.005f * Context.FrameTime;
                if (scrollPos > 0) _debugCamera.Zoom -= 0.005f * Context.FrameTime;

                _debugCamera.Update(null);
            }

            // Update frame by frame mode.
            if (_frameByFrame)
            {
                _frameByFrameAdvance = false;
                if (Context.Input.IsKeyDown("F11")) _frameByFrameAdvance = true;
                if (Context.Input.IsKeyHeld("F12")) _frameByFrameAdvance = true;
            }
        }

        [Conditional("DEBUG")]
        private void DrawDebug()
        {
            _debugUIController.Draw();

            if (_debugCamera != null)
            {
                // Draw bounds.
                RenderOutline(new Vector3(Camera.X, Camera.Y, Camera.Z), Camera.Size, Color.Yellow);

                // Draw center.
                RenderOutline(new Vector3(Camera.X + Camera.Width / 2 - 5f, Camera.Y + Camera.Height / 2 - 5f, Camera.Z), new Vector2(10, 10), Color.Yellow);

                _debugCameraDataText.Text = $"Debug Zoom: {_debugCamera.Zoom}\n" +
                                            $"Debug Location: {_debugCamera.Bounds}\n" +
                                            $"Camera Location: {Camera.Bounds}";
                _cornerAnchor.Update();
            }

            if (_fpsCounter)
            {
                _debugFpsCounterDataText.Text = $"FPS: {1000 / Context.FrameTime:N0}";
                _cornerAnchor.Update();
            }

            if (_drawMouse)
            {
                Vector2 mouseLocation = Context.Input.GetMousePosition();
                mouseLocation.X -= 5;
                mouseLocation.Y -= 5;

                DisableViewMatrix();
                RenderOutline(new Vector3(mouseLocation.X, mouseLocation.Y, 100), new Vector2(10, 10), Color.Pink);
                EnableViewMatrix();
            }
        }

        #endregion

        #region System API

        /// <summary>
        /// Whether to render the next frame.
        /// </summary>
        /// <returns>Whether to render the next frame.</returns>
        internal bool RenderFrame()
        {
            return !_frameByFrame || _frameByFrameAdvance;
        }

        /// <summary>
        /// Clear the screen.
        /// </summary>
        internal void Clear()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Helpers.CheckError("clear");

            // Sync the current shader.
            SyncShader();
        }

        /// <summary>
        /// Flush the main mapping buffer.
        /// </summary>
        internal void End()
        {
            // Draw any debugging if needed.
            DrawDebug();

            // Flush unflushed buffers.
            if (_mainBuffer.AnythingMapped || _mainLineBuffer.AnythingMapped)
            {
                Debugger.Log(MessageType.Warning, MessageSource.Renderer, "Unflushed render queues at the end of a frame.");
                RenderFlush();
                RenderOutlineFlush();
            }
        }

        /// <summary>
        /// Updates the renderer.
        /// </summary>
        /// <param name="_">The time passed from the previous update to now. Unused.</param>
        internal void Update(float _)
        {
            // Update debugging features.
            UpdateDebug();

            // Update the current camera.
            Camera.Update(Context);
        }

        #endregion

        #region Other APIs

        /// <summary>
        /// Synchronize the shader properties with the actual ones. This doesn't set the model matrix.
        /// </summary>
        /// <param name="shader">The shader to synchronize.</param>
        /// <param name="full">Whether to perform a full synchronization. Some properties are not expected to change often.</param>
        public void SyncShader(ShaderProgram shader, bool full = true)
        {
            shader.SetUniformFloat("time", Context.Time);
            if (full) shader.SetUniformMatrix4("projectionMatrix", Matrix4.CreateOrthographicOffCenter(0, Context.Settings.RenderWidth, Context.Settings.RenderHeight, 0, -100, 100));
            EnableViewMatrix();
        }

        /// <summary>
        /// Synchronize the current shader's properties with the actual ones. This doesn't set the model matrix.
        /// </summary>
        /// <param name="full">Whether to perform a full synchronization. Some properties are not expected to change often.</param>
        public void SyncShader(bool full = true)
        {
            ShaderProgram.Current.SetUniformFloat("time", Context.Time);
            if (full) ShaderProgram.Current.SetUniformMatrix4("projectionMatrix", Matrix4.CreateOrthographicOffCenter(0, Context.Settings.RenderWidth, Context.Settings.RenderHeight, 0, -100, 100));
            EnableViewMatrix();
        }

        /// <summary>
        /// Set the current model matrix for the current shader.
        /// </summary>
        public void SetModelMatrix()
        {
            ShaderProgram.Current.SetUniformMatrix4("modelMatrix", MatrixStack.CurrentMatrix);
        }

        public Vector2 ScreenToWorld(Vector2 position)
        {
            return Vector2.TransformPosition(position, (_debugCamera ?? Camera).ViewMatrix.Inverted());
        }

        /// <summary>
        /// Disables the view matrix until the shader is resynchronized or it is re-enabled.
        /// </summary>
        public void DisableViewMatrix()
        {
            ShaderProgram.Current.SetUniformMatrix4("viewMatrix", Matrix4.Identity);
        }

        /// <summary>
        /// Enables the view matrix.
        /// </summary>
        public void EnableViewMatrix()
        {
            ShaderProgram.Current.SetUniformMatrix4("viewMatrix", (_debugCamera ?? Camera).ViewMatrix);
        }

        /// <summary>
        /// Enables the view matrix for the specified shader.
        /// </summary>
        public void EnableViewMatrix(ShaderProgram shader)
        {
            shader.SetUniformMatrix4("viewMatrix", (_debugCamera ?? Camera).ViewMatrix);
        }

        #endregion

        #region Batching Render API

        /// <summary>
        /// Queue a render on the main map buffer.
        /// </summary>
        /// <param name="location">The location of the buffer.</param>
        /// <param name="size">The size of the buffer.</param>
        /// <param name="color">The color of the vertices.</param>
        /// <param name="texture">The texture to use.</param>
        /// <param name="textureArea">The texture area to render.</param>
        public void RenderQueue(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            _mainBuffer.Add(location, size, color, texture, textureArea);
        }

        /// <summary>
        /// Queue a render of a rectangle outline.
        /// </summary>
        /// <param name="location">The location of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="color">The color of the lines.</param>
        public void RenderQueueOutline(Vector3 location, Vector2 size, Color color)
        {
            _mainLineBuffer.Add(location, size, color);
        }

        /// <summary>
        /// Flushes the main map buffer, and restarts its mapping.
        /// </summary>
        public void RenderFlush()
        {
            // Check if anything has been mapped.
            if (!_mainBuffer.AnythingMapped) return;
            // If still mapping, finish.
            if (_mainBuffer.Mapping) _mainBuffer.FinishMapping();
            Render(_mainBuffer);
            _mainBuffer.Start(true);
        }

        /// <summary>
        /// Flushed the main outline buffer, and restarts its mapping.
        /// </summary>
        public void RenderOutlineFlush()
        {
            // Check if anything has been mapped.
            if (!_mainLineBuffer.AnythingMapped) return;
            // If still mapping, finish.
            if (_mainLineBuffer.Mapping) _mainLineBuffer.FinishMapping();
            Render(_mainLineBuffer);
            _mainLineBuffer.Start();
        }

        #endregion

        #region Instant Render

        /// <summary>
        /// Instantly render a quad to the screen.
        /// </summary>
        /// <param name="location">The location of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad, if any.</param>
        /// <param name="textureArea">The texture area of the quad's texture, if any.</param>
        public void Render(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            RenderQueue(location, size, color, texture, textureArea);
            RenderFlush();
        }

        /// <summary>
        /// Instantly render a rectangle outline.
        /// </summary>
        /// <param name="location">The location of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="color">The color of the lines.</param>
        public void RenderOutline(Vector3 location, Vector2 size, Color color)
        {
            _mainLineBuffer.Add(location, size, color);
            RenderOutlineFlush();
        }

        /// <summary>
        /// Instantly render a line.
        /// </summary>
        /// <param name="pointOne">The first point.</param>
        /// <param name="pointTwo">The second point.</param>
        /// <param name="color">The color of the line.</param>
        public void RenderLine(Vector3 pointOne, Vector3 pointTwo, Color color)
        {
            _mainLineBuffer.Add(pointOne, pointTwo, color);
            RenderOutlineFlush();
        }

        /// <summary>
        /// Renders a string to the screen.
        /// </summary>
        /// <param name="font">The font to render using.</param>
        /// <param name="textSize">The size to render in.</param>
        /// <param name="text">The text to render.</param>
        /// <param name="position">The position to render to.</param>
        /// <param name="color">The color to render in.</param>
        public void RenderString(Font font, uint textSize, string text, Vector3 position, Color color)
        {
            // Flush the buffer.
            RenderFlush();

            // Add the string's model matrix.
            MatrixStack.Push(Matrix4.CreateTranslation(position));

            // Queue letters.
            Rectangle[] uvs = new Rectangle[text.Length];
            Atlas atlas = font.GetFontAtlas(textSize);

            float penX = 0;
            float penY = 0;

            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '\n')
                {
                    penX = 0;
                    penY += atlas.LineSpacing;
                    continue;
                }

                if (i > 0) penX += atlas.GetKerning(text[i - 1], text[i]);

                Glyph g = atlas.Glyphs[text[i]];

                Vector3 renderPos = new Vector3(g.MinX + penX, penY + g.YBearing, position.Z);
                uvs[i] = new Rectangle(g.X, g.Y, g.Width, g.Height);
                RenderQueue(renderPos, uvs[i].Size, color, atlas.Texture, uvs[i]);
                penX += g.Advance;
            }

            // Render the whole string.
            RenderFlush();

            // Remove the model matrix.
            MatrixStack.Pop();
        }

        /// <summary>
        /// Queue a renderable to be rendered at the end of the frame.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        /// <param name="skipModelMatrix">Whether to skip applying the model matrix. Off by default.</param>
        public void Render(Renderable renderable, bool skipModelMatrix = false)
        {
            if (!skipModelMatrix) SetModelMatrix();
            renderable.Render(this);
        }

        /// <summary>
        /// Queue a renderable to be rendered at the end of the frame.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        /// <param name="modelMatrix">The renderable's model matrix.</param>
        public void Render(Renderable renderable, Matrix4 modelMatrix)
        {
            MatrixStack.Push(modelMatrix);
            SetModelMatrix();
            renderable.Render(this);
            MatrixStack.Pop();
        }

        /// <summary>
        /// Queue a renderable to be rendered at the end of the frame.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        public void Render(TransformRenderable renderable)
        {
            MatrixStack.Push(renderable.ModelMatrix);
            SetModelMatrix();
            renderable.Render(this);
            MatrixStack.Pop();
        }

        #endregion
    }
}