#region Using

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.CompilerServices;
using Emotion.Common;
using Emotion.Common.Threading;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using Khronos;
using OpenGL;

#endregion

namespace Emotion.Graphics
{
    /// <summary>
    /// Handles all drawing APIs, initialization, and state management.
    /// </summary>
    public sealed partial class RenderComposer
    {
        #region Settings

        /// <summary>
        /// Whether v-sync is enabled. On some platforms this is forced on - check using the ForcedVSync flag. On by default.
        /// </summary>
        public bool VSync
        {
            get => ForcedVSync || _vSync;
            set
            {
                _vSync = value;
                GLThread.ExecuteGLThreadAsync(ApplySettings);
            }
        }

        private bool _vSync = true;

        /// <summary>
        /// The positive cut off of the camera.
        /// </summary>
        public float FarZ = 100; // todo: Move this and NearZ to camera.

        /// <summary>
        /// The negative cut off of the camera.
        /// </summary>
        public float NearZ = -100;

        /// <summary>
        /// The maximum number of indices in the default Ibo buffers.
        /// </summary>
        public const uint MAX_INDICES = ushort.MaxValue;

        #endregion

        #region Flags

        /// <summary>
        /// Whether v-sync is forced by the platform or driver.
        /// </summary>
        public bool ForcedVSync { get; internal set; }

        /// <summary>
        /// Whether the renderer is running in compatibility mode, falling back to older features.
        /// </summary>
        public bool CompatibilityMode { get; private set; }

        /// <summary>
        /// Whether direct state access is supported.
        /// https://www.khronos.org/opengl/wiki/Direct_State_Access
        /// </summary>
        public bool Dsa { get; private set; }

        /// <summary>
        /// The maximum textures that can be bound at the same time.
        /// </summary>
        public int TextureArrayLimit { get; private set; } = -1;

        #endregion

        #region Objects

        /// <summary>
        /// The vertex data batch currently active.
        /// </summary>
        public RenderStreamBatch<VertexData> RenderStream { get; private set; }

        /// <summary>
        /// A representation of the screen's frame buffer.
        /// </summary>
        public FrameBuffer ScreenBuffer { get; private set; }

        /// <summary>
        /// The camera active in the scene.
        /// </summary>
        public CameraBase Camera;

#if DEBUG
        /// <summary>
        /// Camera used to debug the current camera.
        /// </summary>
        public DebugCamera DebugCamera;
#endif

        /// <summary>
        /// The stack of frame buffers.
        /// The screen buffer is not a part of this stack.
        /// </summary>
        private Stack<FrameBuffer> _bufferStack = new Stack<FrameBuffer>();

        /// <summary>
        /// The model matrix stack.
        /// </summary>
        private TransformationStack _matrixStack = new TransformationStack();

        #endregion

        #region Intermediary Buffer Functionality

        /// <summary>
        /// The frame buffer rendering is done to, before it is flushed to the screen buffer.
        /// </summary>
        public FrameBuffer DrawBuffer { get; private set; }

        /// <summary>
        /// A render state for merging two buffers.
        /// </summary>
        public RenderState BlitState;

        #endregion

        #region State

        /// <summary>
        /// The current drawing state. Don't modify directly!
        /// </summary>
        public RenderState CurrentState { get; private set; }

        /// <summary>
        /// The current frame buffer.
        /// </summary>
        public FrameBuffer CurrentTarget
        {
            get => _bufferStack.Count == 0 ? null : _bufferStack.Peek();
        }

        /// <summary>
        /// The current model matrix.
        /// </summary>
        public Matrix4x4 ModelMatrix
        {
            get => _matrixStack.CurrentMatrix;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The ratio between the current DrawBuffer resolution and RenderSize.
        /// Cannot fall below 1 as the RenderSize is the minimum size.
        /// If not in FullScale mode then this is always one.
        /// </summary>
        public float Scale { get; private set; } = 1;

        /// <summary>
        /// The closest integer scale to the current scale - used to make sure pixel art looks consistent.
        /// Cannot fall below 1 as the RenderSize is the minimum size.
        /// </summary>
        public int IntScale { get; private set; } = 1;

        #endregion

        /// <summary>
        /// Is run by the engine when the main renderer is created.
        /// </summary>
        internal void Setup()
        {
            // Check if running on the GL Thread.
            Debug.Assert(GLThread.IsGLThread());

            Engine.Log.Info($"Created OpenGL Context {Gl.CurrentVersion}", MessageSource.Renderer);
            Engine.Log.Info($" Renderer: {Gl.CurrentRenderer}", MessageSource.Renderer);
            Engine.Log.Info($" Vendor: {Gl.CurrentVendor}", MessageSource.Renderer);
            Engine.Log.Info($" Shader: {Gl.CurrentShadingVersion}", MessageSource.Renderer);

            // Set flags.
            CompatibilityMode = Gl.SoftwareRenderer || Engine.Configuration.RendererCompatMode;
            Dsa = !CompatibilityMode && Gl.CurrentVersion.Major >= 4 && Gl.CurrentVersion.Minor >= 5;
            TextureArrayLimit = Gl.SoftwareRenderer || Gl.CurrentVersion.Profile == KhronosVersion.PROFILE_WEBGL ? 16 : Gl.CurrentLimits.MaxTextureImageUnits;

            Engine.Log.Info($" Flags: {(CompatibilityMode ? "Compat, " : "")}{(Dsa ? "Dsa, " : "")}Textures[{TextureArrayLimit}]", MessageSource.Renderer);

            // Attach callback if debug mode is enabled.
            bool hasDebugSupport = Gl.CurrentExtensions.DebugOutput_ARB || Gl.CurrentVersion.Major >= 4 && Gl.CurrentVersion.Minor >= 3;
            if (Engine.Configuration.GlDebugMode && !CompatibilityMode && hasDebugSupport)
            {
                Gl.Enable(EnableCap.DebugOuput);
                Gl.DebugMessageCallback(_glDebugCallback, IntPtr.Zero);
                Engine.Log.Trace("Attached OpenGL debug callback.", MessageSource.Renderer);
            }
            // In release mode GL errors are not checked after every call.
            // In that case a error catching callback is attached so that GL errors are logged.
            else if(hasDebugSupport)
            {
                Gl.Enable(EnableCap.DebugOuput);
                Gl.DebugMessageCallback(_glErrorCatchCallback, IntPtr.Zero);
                Engine.Log.Info("Attached OpenGL error catching callback.", MessageSource.Renderer);
            }

            // Create a representation of the screen buffer, and the buffer which will be drawn to.
            Vector2 windowSize = Engine.Host.Size;
            ScreenBuffer = new FrameBuffer(0, windowSize);
            DrawBuffer = !Engine.Configuration.UseIntermediaryBuffer ? new FrameBuffer(0, windowSize) : new FrameBuffer(windowSize).WithColor().WithDepth();
            _bufferStack.Push(DrawBuffer);

            // Decide on scaling mode.
            if (Engine.Configuration.ScaleBlackBars)
            {
                Debug.Assert(Engine.Configuration.UseIntermediaryBuffer, "Scale black bars requires an intermediary buffer.");
                Engine.Host.OnResize += HostResizedBlackBars;
                HostResizedBlackBars(windowSize);
            }
            else
            {
                Engine.Host.OnResize += HostResized;
                HostResized(windowSize);
            }

            // Create default camera. RenderState applying requires one to be set.
            Camera = new PixelArtCamera(Vector3.Zero);

            // Create default render objects.
            Vector4 c = Engine.Configuration.ClearColor.ToVec4();
            Gl.ClearColor((int) c.X, (int) c.Y, (int) c.Z, (int) c.W);

            ShaderProgram defaultProgram = ShaderFactory.CreateDefaultShader();
            if (defaultProgram == null)
            {
                Engine.CriticalError(new Exception("Couldn't create default shaders."));
                return;
            }

            Texture.InitializeEmptyTexture();

            // Create default render states.
            CurrentState = new RenderState();
            SetState(RenderState.Default);

            BlitState = RenderState.Default.Clone();
            BlitState.AlphaBlending = false;
            BlitState.DepthTest = false;
            BlitState.ViewMatrix = false;
            BlitState.Shader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/Blit.xml").Shader;

            // Create render stream. This is used for IM-like rendering.
            RenderStream = new RenderStreamBatch<VertexData>();

            // Apply display settings (this is the initial application) and attach the camera updating coroutine.
            ApplySettings();
            Engine.CoroutineManager.StartCoroutine(UpdateCoroutine());

#if DEBUG
            Engine.Host.OnKey.AddListener(DebugFunctionalityKeyInput);
#endif
        }

        #region Event Handles and Sizing

        private static Gl.DebugProc _glErrorCatchCallback = GlErrorCatchCallback;
        private static unsafe void GlErrorCatchCallback(DebugSource source, DebugType msgType, uint id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            if(msgType != DebugType.DebugTypeError) return;

            var stringMessage = new string((sbyte*) message, 0, length);
            Engine.Log.Error(stringMessage, $"GL_{source}");
        }

        /// <summary>
        /// Apply rendering settings.
        /// </summary>
        public void ApplySettings()
        {
            Engine.Host.Context.SwapInterval = _vSync ? 1 : 0;
        }

        /// <summary>
        /// Recreate the drawbuffer when the host is resized.
        /// </summary>
        private void HostResized(Vector2 size)
        {
            // Recalculate scale.
            Vector2 baseRes = Engine.Configuration.RenderSize;
            if (size.X < baseRes.X || size.Y < baseRes.Y)
            {
                Engine.Log.Info($"Tried to resize host to {size} which is below minimum size (render size) {baseRes}", MessageSource.Renderer);
                return;
            }
            
            Vector2 ratio = size / baseRes;
            Scale = MathF.Min(ratio.X, ratio.Y);
            IntScale = (int) MathF.Floor(MathF.Min(size.X, size.Y) / MathF.Min(baseRes.X, baseRes.Y));

            Vector2 drawBufferSize = size;
            if (Engine.Configuration.IntScaleDrawBuffer)
            {
                Scale -= IntScale - 1;
                drawBufferSize /= IntScale;
                drawBufferSize.IntCastRound();
                IntScale = 1;
            }

            Engine.Log.Info($"Resized host to {size} - scale is {Scale} and int scale is {IntScale}", MessageSource.Renderer);

            ScreenBuffer.Resize(size);
            DrawBuffer.Resize(drawBufferSize, true);
            Camera?.RecreateMatrix();
            ApplySettings();
        }

        /// <summary>
        /// Recalculate the draw buffer when the host is resized, using black bars.
        /// </summary>
        private void HostResizedBlackBars(Vector2 size)
        {
            // Calculate borderbox / pillarbox.
            float targetAspectRatio = DrawBuffer.Size.X / DrawBuffer.Size.Y;
            float width = size.X;
            float height = (int) (width / targetAspectRatio + 0.5f);

            // If the height is bigger then the black bars will appear on the top and bottom, otherwise they will be on the left and right.
            if (height > size.Y)
            {
                height = size.Y;
                width = (int) (height * targetAspectRatio + 0.5f);
            }

            if (Engine.Configuration.IntScaleDrawBuffer)
            {
                var xIntScale = (float) Math.Floor(width / DrawBuffer.Size.X);
                var yIntScale = (float) Math.Floor(height / DrawBuffer.Size.Y);
                width = DrawBuffer.Size.X * xIntScale;
                height = DrawBuffer.Size.Y * yIntScale;
            }

            var vpX = (int) (size.X / 2 - width / 2);
            var vpY = (int) (size.Y / 2 - height / 2);

            // Set viewport.
            ScreenBuffer.Resize(size);
            ScreenBuffer.Viewport = new Rectangle(vpX, vpY, width, height);
            DrawBuffer.Resize(Engine.Configuration.RenderSize, true);

            ApplySettings();
        }

        #endregion

        /// <summary>
        /// Called at the start of the frame.
        /// Prepares the renderer for rendering.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RenderComposer StartFrame()
        {
            // Check if running on the GL Thread.
            Debug.Assert(GLThread.IsGLThread());

            // Reset to the default state.
            PerfProfiler.FrameEventStart("DefaultStateSet");
            SetState(RenderState.Default, true);
            PerfProfiler.FrameEventEnd("DefaultStateSet");

            // Clear the screen.
            PerfProfiler.FrameEventStart("Clear");
            ScreenBuffer.Bind();
            ClearFrameBuffer();
            PerfProfiler.FrameEventEnd("Clear");

            if (Engine.Configuration.UseIntermediaryBuffer)
            {
                // Clear the draw buffer.
                // No need to call EnsureRenderTarget as the DrawBuffer should be alone in this stack.
                DrawBuffer.Bind();
                ClearFrameBuffer();
            }

            // Check if a render target was forgotten.
            if (_bufferStack.Count > 1)
                Debug.Assert(false);

            return this;
        }

        /// <summary>
        /// Called at the end of the frame.
        /// Flushes everything and performs the actual rendering.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EndFrame()
        {
            // Check if running on the GL Thread.
            Debug.Assert(GLThread.IsGLThread());

#if DEBUG

            if (DebugCamera != null)
            {
                SetUseViewMatrix(true);
                Rectangle actualCameraBounds = Camera.GetWorldBoundingRect();
                RenderOutline(actualCameraBounds, Color.Red);
                RenderCircle(Camera.Position, 5, Color.Red, true);
                RenderLine(Camera.Position, actualCameraBounds.TopLeft.ToVec3(), Color.Red);
                RenderLine(Camera.Position, actualCameraBounds.TopRight.ToVec3(), Color.Red);
                RenderLine(Camera.Position, actualCameraBounds.BottomLeft.ToVec3(), Color.Red);
                RenderLine(Camera.Position, actualCameraBounds.BottomRight.ToVec3(), Color.Red);
            }

#endif
            if (Engine.Configuration.UseIntermediaryBuffer)
            {
                // Push a blit from the draw buffer to the screen buffer.
                SetState(BlitState);
                RenderTo(ScreenBuffer);
                RenderFrameBuffer(DrawBuffer, ScreenBuffer.Size);
                RenderTo(null);
            }
            else
            {
                // If no intermediary buffer there is nothing to cause the final flush.
                FlushRenderStream();
            }

            RenderStream.DoTasks(this);
        }

        private IEnumerator UpdateCoroutine()
        {
            while (Engine.Status == EngineStatus.Running)
            {
                Camera.Update();
#if DEBUG
                DebugCamera?.Update();
#endif
                yield return null;
            }
        }

        #region Framebuffer, Shader, and Model Matrix Syncronization and State

        /// <summary>
        /// Synchronizes uniform properties with the currently bound shader.
        /// </summary>
        public void SyncShader()
        {
            ShaderProgram currentShader = CurrentState.Shader;
            if (CurrentState.Shader == null) return;
            PerfProfiler.FrameEventStart("ShaderSync");

            currentShader.SetUniformMatrix4("projectionMatrix", Matrix4x4.CreateOrthographicOffCenter(0, CurrentTarget.Size.X, CurrentTarget.Size.Y, 0, NearZ, FarZ));

            SyncModelMatrix();
            SyncViewMatrix();

            currentShader.SetUniformFloat("iTime", Engine.TotalTime / 1000f);
            currentShader.SetUniformVector3("iResolution", new Vector3(CurrentTarget.Size.X, CurrentTarget.Size.Y, 0));

            PerfProfiler.FrameEventEnd("ShaderSync");
        }

        /// <summary>
        /// Synchronizes the model matrix to the current shader.
        /// Moved to a separate function so that it can be updated without updating all uniforms.
        /// </summary>
        private void SyncModelMatrix()
        {
            CurrentState.Shader.SetUniformMatrix4("modelMatrix", ModelMatrix);
        }

        /// <summary>
        /// Synchronizes the view matrix to the current shader.
        /// Moved to a separate function so that it can be updated without updating all uniforms.
        /// </summary>
        private void SyncViewMatrix()
        {
            // Check if the view matrix is off.
            if (!Engine.Renderer.CurrentState.ViewMatrix.GetValueOrDefault())
            {
                CurrentState.Shader.SetUniformMatrix4("viewMatrix", Matrix4x4.Identity);
                return;
            }

#if DEBUG
            if (DebugCamera != null)
            {
                CurrentState.Shader.SetUniformMatrix4("viewMatrix", DebugCamera.ViewMatrix);
                return;
            }
#endif

            CurrentState.Shader.SetUniformMatrix4("viewMatrix", Camera.ViewMatrix);
        }

        /// <summary>
        /// Ensures the current render target is current, and bound.
        /// </summary>
        public void EnsureRenderTarget()
        {
            // Happens on initialization.
            if (CurrentTarget == null) return;

            CurrentTarget.Bind();
            SyncShader();
        }

        /// <summary>
        /// Push a framebuffer on top of the target stack, meaning it will be used for subsequent drawing.
        /// </summary>
        /// <param name="target">The target to push.</param>
        internal void PushFramebuffer(FrameBuffer target)
        {
            _bufferStack.Push(target);
            EnsureRenderTarget();
        }

        /// <summary>
        /// Pop off a render target off of the top of the target stack, meaning the one before it will be used for subsequent
        /// drawing.
        /// </summary>
        internal void PopFramebuffer(bool rebindPrevious = true)
        {
            if (_bufferStack.Count == 1)
            {
                Engine.Log.Error("Cannot pop off the Drawbuffer.", MessageSource.Renderer);
                return;
            }

            _bufferStack.Pop();
            if (rebindPrevious) EnsureRenderTarget();
        }

        #endregion

        #region Debug Functionality

        private static Gl.DebugProc _glDebugCallback = GlDebugCallback;
        private static unsafe void GlDebugCallback(DebugSource source, DebugType msgType, uint id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
        {
            var stringMessage = new string((sbyte*) message, 0, length);

            // NVidia drivers love to spam the debug log with how your buffers will be mapped in the system heap.
            if (msgType == DebugType.DebugTypeOther && stringMessage.Contains("SYSTEM HEAP")) return;

            switch (severity)
            {
                case DebugSeverity.DebugSeverityHigh:
                    Engine.Log.Warning(stringMessage, $"GL_{msgType}_{source}");
                    break;
                case DebugSeverity.DebugSeverityMedium:
                    Engine.Log.Info(stringMessage, $"GL_{msgType}_{source}");
                    break;
                default:
                    Engine.Log.Trace(stringMessage, $"GL_{msgType}_{source}");
                    break;
            }
        }

#if DEBUG

        private bool DebugFunctionalityKeyInput(Key key, KeyStatus state)
        {
            if (state != KeyStatus.Down) return true;

            bool ctrl = Engine.Host.IsCtrlModifierHeld();
            if (key == Key.F1 && !ctrl) ToggleDebugCamera();
            return true;
        }

        private void ToggleDebugCamera()
        {
            if (DebugCamera != null)
            {
                Engine.Log.Info("Debug camera turned off.", MessageSource.Debug);
                DebugCamera.Dispose();
                DebugCamera = null;
                return;
            }

            DebugCamera = new DebugCamera(Camera.Position, Camera.Zoom);
            Engine.Log.Info("Debug camera turned on. Use the numpad keys 8462 to move and mouse scroll to zoom.", MessageSource.Debug);
        }

#endif

        #endregion
    }
}