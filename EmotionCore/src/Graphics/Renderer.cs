// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Diagnostics;
using System.Numerics;
using Emotion.Engine;
using Emotion.Game.Camera;
using Emotion.Game.UI;
using Emotion.Game.UI.Layout;
using Emotion.Graphics.Batching;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Text;
using Emotion.Primitives;
using OpenTK.Graphics.ES30;
using Buffer = Emotion.Graphics.Objects.Buffer;
using Debugger = Emotion.Debug.Debugger;

#endregion

namespace Emotion.Graphics
{
    /// <summary>
    /// The object which takes care of rendering and managing GL.
    /// </summary>
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
        public CameraBase Camera
        {
            get => _camera;
            set
            {
                _camera = value;
                _camera.Update();
            }
        }

        #endregion

        #region Render State

        /// <summary>
        /// Private camera tracker.
        /// </summary>
        private CameraBase _camera;

        /// <summary>
        /// The main drawing buffer.
        /// </summary>
        private QuadMapBuffer _mainBuffer;

        /// <summary>
        /// The model matrix stack.
        /// </summary>
        private TransformationStack _modelMatrix;

        #endregion

        #region Initialization

        /// <summary>
        /// Creates a new renderer. Is called by the Context when initializing modules.
        /// </summary>
        internal Renderer()
        {
            // Create objects.
            Camera = new CameraBase(new Vector3(0, 0, 0), new Vector2(Context.Settings.RenderSettings.Width, Context.Settings.RenderSettings.Height));
            _modelMatrix = new TransformationStack();

            // Setup main map buffer.
            _mainBuffer = new QuadMapBuffer(MaxRenderable);

            // Setup debug.
            SetupDebug();
        }

        /// <summary>
        /// Destroy the renderer.
        /// </summary>
        internal void Destroy()
        {
            _mainBuffer.Delete();
        }

        #endregion

        #region Debugging API

        private BasicTextBg _debugCameraDataText;
        private BasicTextBg _debugFpsCounterDataText;

        private CameraBase _debugCamera;
        private bool _fpsCounter;
        private bool _drawMouse;

        [Conditional("DEBUG")]
        private void SetupDebug()
        {
            Context.ScriptingEngine.Expose("debugCamera",
                (Func<string>) (() =>
                {
                    _debugCamera = _debugCamera == null
                        ? new CameraBase(new Vector3(Camera.Center.X, Camera.Center.Y, 0), new Vector2(Context.Settings.RenderSettings.Width, Context.Settings.RenderSettings.Height))
                        {
                            Zoom = Camera.Zoom / 2f
                        }
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

            Debugger.CornerAnchor.AddChild(_debugCameraDataText, AnchorLocation.BottomLeft);
            Debugger.CornerAnchor.AddChild(_debugFpsCounterDataText, AnchorLocation.TopLeft);
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

            if (_fpsCounter) _debugFpsCounterDataText.Text = $"FPS: {1000 / Context.RawFrameTime:N0}";

            if (!_drawMouse) return;
            Vector2 mouseLocation = Context.InputManager.GetMousePosition();
            mouseLocation.X -= 5;
            mouseLocation.Y -= 5;

            Camera.Enabled = false;
            RenderOutline(new Vector3(mouseLocation.X, mouseLocation.Y, 100), new Vector2(10, 10), Color.Pink);
            Camera.Enabled = true;
        }

        #endregion

        #region System API

        /// <summary>
        /// Clear the screen.
        /// </summary>
        internal void Clear()
        {
            // Restore bound state. Some drivers unbind objects when swapping buffers.
            ShaderProgram.Current.Bind();
            Buffer.BoundPointer = 0;
            IndexBuffer.BoundPointer = 0;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GLThread.CheckError("clear");

            // Update the current camera.
            Camera.Update();

            // Sync the current shader.
            SystemSyncCurrentShader();
        }

        /// <summary>
        /// Flush the main mapping buffer.
        /// </summary>
        internal void End()
        {
            // Draw any debugging if needed.
            DrawDebug();

            // Submit remaining rendered.
            Submit();
        }

        /// <summary>
        /// Updates the renderer.
        /// </summary>
        internal void Update()
        {
            // Update debugging features.
            UpdateDebug();
        }

        #endregion

        #region Other APIs

        /// <summary>
        /// Transforms a point through the viewMatrix converting it from screen space to world space.
        /// </summary>
        /// <param name="position">The point to transform.</param>
        /// <returns>The provided point in the world.</returns>
        public Vector2 ScreenToWorld(Vector2 position)
        {
            return Vector2.Transform(position, (_debugCamera ?? Camera).ViewMatrix.Inverted());
        }

        #endregion

        #region Rendering

        /// <summary>
        /// Render a quad to the screen.
        /// </summary>
        /// <param name="location">The location of the quad.</param>
        /// <param name="size">The size of the quad.</param>
        /// <param name="color">The color of the quad.</param>
        /// <param name="texture">The texture of the quad, if any.</param>
        /// <param name="textureArea">The texture area of the quad's texture, if any.</param>
        public void Render(Vector3 location, Vector2 size, Color color, Texture texture = null, Rectangle? textureArea = null)
        {
            _mainBuffer.MapNextQuad(location, size, color, texture, textureArea);
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
            // Add the string's model matrix.
            PushToModelMatrix(Matrix4x4.CreateTranslation(position));

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
                Render(renderPos, uvs[i].Size, color, atlas.Texture, uvs[i]);
                penX += g.Advance;
            }

            // Remove the model matrix.
            PopModelMatrix();
        }

        /// <summary>
        /// Render a line.
        /// </summary>
        /// <param name="pointOne">The first point.</param>
        /// <param name="pointTwo">The second point.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public void RenderLine(Vector3 pointOne, Vector3 pointTwo, Color color, float thickness = 1)
        {
            _mainBuffer.MapNextLine(pointOne, pointTwo, color, thickness);
        }

        /// <summary>
        /// Render a rectangle outline.
        /// </summary>
        /// <param name="location">The location of the rectangle.</param>
        /// <param name="size">The size of the rectangle.</param>
        /// <param name="color">The color of the lines.</param>
        /// <param name="thickness">How thick the line should be.</param>
        public void RenderOutline(Vector3 location, Vector2 size, Color color, float thickness = 1)
        {
            RenderLine(location, new Vector3(location.X + size.X, location.Y, location.Z), color, thickness);
            RenderLine(new Vector3(location.X + size.X, location.Y, location.Z), new Vector3(location.X + size.X, location.Y + size.Y, location.Z), color, thickness);
            RenderLine(new Vector3(location.X + size.X, location.Y + size.Y, location.Z), new Vector3(location.X, location.Y + size.Y, location.Z), color, thickness);
            RenderLine(new Vector3(location.X, location.Y + size.Y, location.Z), location, color, thickness);
        }

        /// <summary>
        /// Render a circle outline.
        /// </summary>
        /// <param name="position">
        /// The top right position of the imaginary rectangle which encompasses the circle. Can be modified
        /// with "useCenter"
        /// </param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="color">The circle color.</param>
        /// <param name="useCenter">Whether the position should instead be the center of the circle.</param>
        public void RenderCircleOutline(Vector3 position, float radius, Color color, bool useCenter = false)
        {
            // Add the circle's model matrix.
            PushToModelMatrix(useCenter ? Matrix4x4.CreateTranslation(position.X - radius, position.Y - radius, position.Z) : Matrix4x4.CreateTranslation(position));

            float fX = 0;
            float fY = 0;
            float pX = 0;
            float pY = 0;

            // Generate points.
            for (uint i = 0; i < Context.Flags.RenderFlags.CircleDetail; i++)
            {
                float angle = (float) (i * 2 * Math.PI / Context.Flags.RenderFlags.CircleDetail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius;
                float y = (float) Math.Sin(angle) * radius;

                if (i == 0)
                {
                    RenderLine(new Vector3(radius + x, radius + y, 0), new Vector3(radius + x, radius + y, 0), color);
                    fX = x;
                    fY = y;
                }
                else if (i == Context.Flags.RenderFlags.CircleDetail - 1)
                {
                    RenderLine(new Vector3(radius + pX, radius + pY, 0), new Vector3(radius + x, radius + y, 0), color);
                    RenderLine(new Vector3(radius + x, radius + y, 0), new Vector3(radius + fX, radius + fY, 0), color);
                }
                else
                {
                    RenderLine(new Vector3(radius + pX, radius + pY, 0), new Vector3(radius + x, radius + y, 0), color);
                }

                pX = x;
                pY = y;
            }

            // Remove the model matrix.
            PopModelMatrix();
        }

        /// <summary>
        /// Render a circle.
        /// </summary>
        /// <param name="position">
        /// The top right position of the imaginary rectangle which encompasses the circle. Can be modified
        /// with "useCenter"
        /// </param>
        /// <param name="radius">The circle radius.</param>
        /// <param name="color">The circle color.</param>
        /// <param name="useCenter">Whether the position should instead be the center of the circle.</param>
        public void RenderCircle(Vector3 position, float radius, Color color, bool useCenter = false)
        {
            // Add the circle's model matrix.
            PushToModelMatrix(useCenter ? Matrix4x4.CreateTranslation(position.X - radius, position.Y - radius, position.Z) : Matrix4x4.CreateTranslation(position));

            float pX = 0;
            float pY = 0;
            float fX = 0;
            float fY = 0;

            // Generate points.
            for (uint i = 0; i < Context.Flags.RenderFlags.CircleDetail; i++)
            {
                float angle = (float) (i * 2 * Math.PI / Context.Flags.RenderFlags.CircleDetail - Math.PI / 2);
                float x = (float) Math.Cos(angle) * radius;
                float y = (float) Math.Sin(angle) * radius;

                _mainBuffer.MapNextVertex(new Vector3(radius + pX, radius + pY, 0), color);
                _mainBuffer.MapNextVertex(new Vector3(radius + x, radius + y, 0), color);
                _mainBuffer.MapNextVertex(new Vector3(radius, radius, 0), color);
                _mainBuffer.MapNextVertex(new Vector3(radius, radius, 0), color);

                if (i == 0)
                {
                    fX = x;
                    fY = y;
                }
                else if (i == Context.Flags.RenderFlags.CircleDetail - 1)
                {
                    _mainBuffer.MapNextVertex(new Vector3(radius + pX, radius + pY, 0), color);
                    _mainBuffer.MapNextVertex(new Vector3(radius + fX, radius + fY, 0), color);
                    _mainBuffer.MapNextVertex(new Vector3(radius, radius, 0), color);
                    _mainBuffer.MapNextVertex(new Vector3(radius, radius, 0), color);
                }

                pX = x;
                pY = y;
            }

            // Remove the model matrix.
            PopModelMatrix();
        }

        /// <summary>
        /// Submit all render commands so far.
        /// </summary>
        public void Submit()
        {
            // Check if anything was mapped at all.
            if (!_mainBuffer.Mapping || !_mainBuffer.AnythingMapped) return;

            GLThread.ExecuteGLThread(() =>
            {
                _mainBuffer.Render();
                _mainBuffer.Reset();
            });
        }

        #endregion

        #region State Modification

        /// <summary>
        /// Sets the current shader to the one specified. If null sets the shader to the default one.
        /// </summary>
        /// <param name="shader">The shader to set.</param>
        public void SetShader(ShaderProgram shader = null)
        {
            GLThread.ExecuteGLThread(() =>
            {
                // Check if setting to the same shader.
                if (shader == ShaderProgram.Current) return;

                // Flush the draw buffer.
                Submit();

                // Default or provided switch.
                if (shader == null)
                    ShaderProgram.Default.Bind();
                else
                    shader.Bind();

                // Sync shader uniforms.
                SystemSyncCurrentShader();
            });
        }

        /// <summary>
        /// Push a matrix on top of the model matrix stack.
        /// </summary>
        /// <param name="matrix">The matrix to add.</param>
        /// <param name="multiply">Whether to multiply the new matrix by the previous matrix.</param>
        public void PushToModelMatrix(Matrix4x4 matrix, bool multiply = true)
        {
            // Flush the draw buffer.
            Submit();

            // Push into stack and update shader.
            _modelMatrix.Push(matrix, multiply);
            GLThread.ExecuteGLThread(() => { ShaderProgram.Current.SetUniformMatrix4("modelMatrix", _modelMatrix.CurrentMatrix); });
        }

        /// <summary>
        /// Remove the top matrix from the model matrix stack.
        /// </summary>
        public void PopModelMatrix()
        {
            // Flush the draw buffer.
            Submit();

            // Pop out of stack and update shader.
            _modelMatrix.Pop();
            GLThread.ExecuteGLThread(() => { ShaderProgram.Current.SetUniformMatrix4("modelMatrix", _modelMatrix.CurrentMatrix); });
        }

        #endregion

        #region Renderable

        /// <summary>
        /// Render a renderable object.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        public void Render(IRenderable renderable)
        {
            // Render the draw buffer.
            Submit();

            // Render the renderable.
            GLThread.ExecuteGLThread(renderable.Render);
        }

        /// <summary>
        /// Render a renderable object with a model matrix.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        /// <param name="modelMatrix">The renderable's model matrix.</param>
        /// <param name="multiplyMatrix">Whether to multiply the new matrix by the previous matrix.</param>
        public void Render(IRenderable renderable, Matrix4x4 modelMatrix, bool multiplyMatrix = true)
        {
            // Push the model matrix.
            PushToModelMatrix(modelMatrix, multiplyMatrix);

            // Render the renderable.
            GLThread.ExecuteGLThread(renderable.Render);

            // Pop model matrix.
            PopModelMatrix();
        }

        /// <summary>
        /// Queue a renderable to be rendered at the end of the frame.
        /// </summary>
        /// <param name="renderable">The renderable to render.</param>
        /// <param name="multiplyMatrix">Whether to multiply the new matrix by the previous matrix.</param>
        public void Render(TransformRenderable renderable, bool multiplyMatrix = true)
        {
            // Push the model matrix.
            PushToModelMatrix(renderable.ModelMatrix, multiplyMatrix);

            // Render the renderable.
            GLThread.ExecuteGLThread(renderable.Render);

            // Pop model matrix.
            PopModelMatrix();
        }

        #endregion

        #region System Functions

        /// <summary>
        /// Updated the camera's matrix.
        /// System function.
        /// </summary>
        public void UpdateCameraMatrix()
        {
            // Flush the draw buffer.
            Submit();

            // Upload to the shader.
            GLThread.ExecuteGLThread(() => { ShaderProgram.Current.SetUniformMatrix4("viewMatrix", (_debugCamera ?? Camera).ViewMatrix); });
        }

        /// <summary>
        /// Synchronize the current shader's uniform properties with the actual ones.
        /// System function.
        /// </summary>
        /// <param name="full">Whether to perform a full synchronization. Some properties are not expected to change often.</param>
        public void SystemSyncCurrentShader(bool full = true)
        {
            GLThread.ExecuteGLThread(() =>
            {
                if (full)
                    ShaderProgram.Current.SetUniformMatrix4("projectionMatrix",
                        Matrix4x4.CreateOrthographicOffCenter(0, Context.Settings.RenderSettings.Width, Context.Settings.RenderSettings.Height, 0, -100, 100));

                ShaderProgram.Current.SetUniformMatrix4("modelMatrix", _modelMatrix.CurrentMatrix);
                ShaderProgram.Current.SetUniformMatrix4("viewMatrix", (_debugCamera ?? Camera).ViewMatrix);
                ShaderProgram.Current.SetUniformFloat("time", Context.TotalTime);

                GLThread.CheckError("Syncing shader");
            });
        }

        #endregion
    }
}