// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Game.Camera;
using Emotion.Game.UI;
using Emotion.Game.UI.Layout;
using Emotion.Graphics.Batching;
using Emotion.Graphics.GLES;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using Emotion.System;
using Emotion.Utils;
using OpenTK.Graphics.ES30;
using Soul;
using Debugger = Emotion.Debug.Debugger;

#endregion

namespace Emotion.Graphics
{
    public sealed class Renderer
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
        private bool _viewMatrixEnabled = true;

        #endregion

        #region Initialization

        internal Renderer()
        {
            // Renderer bootstrap.
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "Loading Emotion OpenTK-GLES Renderer...");
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GL: " + GL.GetString(StringName.Version) + " on " + GL.GetString(StringName.Renderer));
            Debugger.Log(MessageType.Info, MessageSource.Renderer, "GLSL: " + GL.GetString(StringName.ShadingLanguageVersion));

            // Flag missing extensions.
            int extCount = GL.GetInteger(GetPName.NumExtensions);
            bool found = false;
            for (int i = 0; i < extCount; i++)
            {
                string extension = GL.GetString(StringNameIndexed.Extensions, i);
                if (extension != "GL_ARB_gpu_shader5") continue;
                found = true;
                break;
            }
            if (!found)
            {
                Debugger.Log(MessageType.Warning, MessageSource.GL, "The extension GL_ARB_GPU_SHADER5 was not found.");
                Shader.Shader5ExtensionMissing = true;
            }

            // Create default shaders. This also sets some shader flags.
            CreateDefaultShaders();

            // Create a default program, and use it.
            ShaderProgram defaultProgram = new ShaderProgram((Shader) null, null);
            defaultProgram.Bind();

            // Create objects.
            Camera = new CameraBase(new Vector3(0, 0, 0), new Vector2(Context.Settings.RenderWidth, Context.Settings.RenderHeight));
            MatrixStack = new TransformationStack();

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
        /// Creates the default shaders.
        /// </summary>
        private void CreateDefaultShaders()
        {
            string defaultVert = Utilities.ReadEmbeddedResource("Emotion.Embedded.Shaders.DefaultVert.glsl");
            string defaultFrag = Utilities.ReadEmbeddedResource("Emotion.Embedded.Shaders.DefaultFrag.glsl");

            try
            {
                ShaderProgram.DefaultVertShader = new Shader(ShaderType.VertexShader, defaultVert);
            }
            catch (Exception ex)
            {
                // Check if one of the expected exceptions.
                if (new Regex("GL_ARB_gpu_shader5").IsMatch(ex.ToString()))
                {
                    Debugger.Log(MessageType.Warning, MessageSource.GL, "The extension GL_ARB_GPU_SHADER5 was found, but is not supported.");
                    Shader.Shader5ExtensionMissing = true;
                    ShaderProgram.DefaultVertShader = new Shader(ShaderType.VertexShader, defaultVert);
                }
                else
                {
                    throw;
                }
            }

            ShaderProgram.DefaultFragShader = new Shader(ShaderType.FragmentShader, defaultFrag);

            Helpers.CheckError("making default shaders");
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

        private BasicTextBg _debugCameraDataText;
        private BasicTextBg _debugFpsCounterDataText;

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
                        ? new CameraBase(new Vector3(Camera.Center.X, Camera.Center.Y, 0), new Vector2(Context.Settings.RenderWidth, Context.Settings.RenderHeight)) {Zoom = Camera.Zoom / 2f}
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

            Font font = Context.AssetLoader.Get<Font>("debugFont.otf");
            _debugCameraDataText = new BasicTextBg(font, 10, "", Color.Yellow, new Color(0, 0, 0, 125), new Vector3(0, 0, 5)) {Padding = new Rectangle(3, 3, 3, 3), Active = false};
            _debugFpsCounterDataText = new BasicTextBg(font, 10, "", Color.Yellow, new Color(0, 0, 0, 125), new Vector3(0, 0, 5)) {Padding = new Rectangle(3, 3, 3, 3), Active = false};

            Debugger.CornerAnchor.AddControl(_debugCameraDataText, AnchorLocation.BottomLeft);
            Debugger.CornerAnchor.AddControl(_debugFpsCounterDataText, AnchorLocation.TopLeft);
        }

        [Conditional("DEBUG")]
        private void UpdateDebug()
        {
            // Update debugging camera.
            if (_debugCamera != null)
            {
                if (Context.InputManager.IsKeyHeld("Down")) _debugCamera.Y += 10 + 0.1f * Context.FrameTime;
                if (Context.InputManager.IsKeyHeld("Right")) _debugCamera.X += 10 + 0.1f * Context.FrameTime;
                if (Context.InputManager.IsKeyHeld("Up")) _debugCamera.Y -= 10 + 0.1f * Context.FrameTime;
                if (Context.InputManager.IsKeyHeld("Left")) _debugCamera.X -= 10 + 0.1f * Context.FrameTime;

                float scrollPos = Context.InputManager.GetMouseScrollRelative();
                if (scrollPos < 0) _debugCamera.Zoom += 0.005f * Context.FrameTime;
                if (scrollPos > 0) _debugCamera.Zoom -= 0.005f * Context.FrameTime;

                _debugCamera.Update();
            }

            // Update frame by frame mode.
            if (_frameByFrame)
            {
                _frameByFrameAdvance = false;
                if (Context.InputManager.IsKeyDown("F11")) _frameByFrameAdvance = true;
                if (Context.InputManager.IsKeyHeld("F12")) _frameByFrameAdvance = true;
            }
        }

        [Conditional("DEBUG")]
        private void DrawDebug()
        {
            if (_debugCamera != null)
            {
                // Draw bounds.
                RenderOutline(new Vector3(Camera.X, Camera.Y, Camera.Z), Camera.Size, Color.Yellow);

                // Draw center.
                RenderOutline(new Vector3(Camera.X + Camera.Width / 2 - 5f, Camera.Y + Camera.Height / 2 - 5f, Camera.Z), new Vector2(10, 10), Color.Yellow);

                _debugCameraDataText.Text = $"Debug Zoom: {_debugCamera.Zoom}\n" +
                                            $"Debug Location: {_debugCamera}\n" +
                                            $"Camera Location: {Camera}";
            }

            if (_fpsCounter) _debugFpsCounterDataText.Text = $"FPS: {1000 / Context.FrameTime:N0}";

            if (_drawMouse)
            {
                Vector2 mouseLocation = Context.InputManager.GetMousePosition();
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
            // Restore bound state. Some drivers unbind objects when swapping buffers.
            ShaderProgram.Current.Bind();
            GLES.Buffer.BoundPointer = 0;
            IndexBuffer.BoundPointer = 0;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            Helpers.CheckError("clear");

            // Sync the current shader.
            SyncCurrentShader();
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
            Camera.Update();
        }

        #endregion

        #region Other APIs

        /// <summary>
        /// Synchronize the current shader's properties with the actual ones. This doesn't set the model matrix.
        /// </summary>
        /// <param name="full">Whether to perform a full synchronization. Some properties are not expected to change often.</param>
        public void SyncCurrentShader(bool full = true)
        {
            if (full)
            {
                SetModelMatrix();
                ShaderProgram.Current.SetUniformMatrix4("projectionMatrix", Matrix4.CreateOrthographicOffCenter(0, Context.Settings.RenderWidth, Context.Settings.RenderHeight, 0, -100, 100));
            }
            ShaderProgram.Current.SetUniformMatrix4("viewMatrix", _viewMatrixEnabled ? (_debugCamera ?? Camera).ViewMatrix : Matrix4.Identity);
            ShaderProgram.Current.SetUniformFloat("time", Context.TotalTime);

            Helpers.CheckError("Syncing shader");
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
            _viewMatrixEnabled = false;
            ShaderProgram.Current.SetUniformMatrix4("viewMatrix", Matrix4.Identity);
        }

        /// <summary>
        /// Enables the view matrix.
        /// </summary>
        public void EnableViewMatrix()
        {
            _viewMatrixEnabled = true;
            ShaderProgram.Current.SetUniformMatrix4("viewMatrix", (_debugCamera ?? Camera).ViewMatrix);
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

        #endregion

        #region Object Rendering

        /// <summary>
        /// Queue a renderable to be rendered at the end of the frame.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        /// <param name="skipModelMatrix">
        /// Whether to skip applying the transformation stack matrix as model matrix. You want to do
        /// this when your renderable is rendering other renderables. Off by default.
        /// </param>
        public void Render(IRenderable renderable, bool skipModelMatrix = false)
        {
            if (!skipModelMatrix) SetModelMatrix();
            renderable.Render(this);
        }

        /// <summary>
        /// Queue a renderable to be rendered at the end of the frame.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        /// <param name="modelMatrix">The renderable's model matrix.</param>
        public void Render(IRenderable renderable, Matrix4 modelMatrix)
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