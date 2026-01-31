#nullable enable

#region Using

using System.Runtime.CompilerServices;
using System.Text.Unicode;
using Emotion.Core.Systems.Input;
using Emotion.Core.Systems.Logging;
using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Profiling;
using Emotion.Core.Utility.Threading;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Batches3D;
using Emotion.Graphics.Camera;
using Emotion.Graphics.Memory;
using Emotion.Graphics.Shading;
using Emotion.Graphics.Text;
using OpenGL;
using OpenGL.Khronos;

#endregion

namespace Emotion.Graphics;

/// <summary>
/// Handles all drawing APIs, initialization, and state management.
/// </summary>
[DontSerialize]
public sealed partial class Renderer
{
    // Emotion uses a:
    // Z-Up Left Handed Coordinate System
    // --------------------------------------
    // This is the same system used by Unreal

    public static Vector3 Up { get; } = new Vector3(0, 0, 1);

    public static Vector3 Up2D { get; } = new Vector3(0, -1, 0);

    public static Vector3 Right { get; } = new Vector3(0, 1, 0);

    public static Vector3 Forward { get; } = new Vector3(1, 0, 0);

    public static Vector3 XAxis = new Vector3(1, 0, 0);
    public static Vector3 YAxis = new Vector3(0, 1, 0);
    public static Vector3 ZAxis = new Vector3(0, 0, 1);

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
            GLThread.ExecuteOnGLThreadAsync(static () => Engine.Renderer.ApplySettings());
        }
    }

    private bool _vSync = true;

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
    public int TextureBindLimit { get; private set; } = -1;

    #endregion

    #region Objects

    /// <summary>
    /// An object that handles batched rendering through streaming vertices every frame.
    /// </summary>
    public RenderStreamBatch RenderStream { get; private set; }

    /// <summary>
    /// A specialized renderer for mesh entities.
    /// The backbone of the 3D renderer.
    /// </summary>
    public MeshEntityBatchRenderer MeshEntityRenderer { get; private set; }

    /// <summary>
    /// A representation of the screen's frame buffer.
    /// </summary>
    public FrameBuffer ScreenBuffer { get; private set; } = null!;

    /// <summary>
    /// The camera active in the scene.
    /// </summary>
    public CameraBase Camera
    {
        get => _virtualCamera != null ? _virtualCamera : _camera;
        set
        {
            // Can be set in the renderer or during scene loading.
            // Kind of problematic, but a problem for another day.
            //Assert(GLThread.IsGLThread());

            if (_camera != null) _camera.Detach();

            _camera = value;
            _camera.Attach();
            _camera.RecreateProjectionMatrix();
        }
    }

    private CameraBase _camera;

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
    public FrameBuffer DrawBuffer { get; private set; } = null!;

    /// <summary>
    /// A render state for merging two buffers.
    /// </summary>
    public RenderState BlitState;

    /// <summary>
    /// A render state for merging two buffers, and applying premult alpha.
    /// </summary>
    public RenderState BlitStatePremult;

    #endregion

    #region State

    /// <summary>
    /// Whether the renderer is currently between StartFrame and EndFrame.
    /// </summary>
    public bool InFrame { get; private set; }

    /// <summary>
    /// The current drawing state. Don't modify directly!
    /// </summary>
    public RenderState CurrentState { get; private set; }

    /// <summary>
    /// The current shader program (note that CurrentState.Shader is a reference to this).
    /// </summary>
    public ShaderProgram CurrentShader { get; private set; } = null!;

    /// <summary>
    /// The current frame buffer.
    /// </summary>
    public FrameBuffer CurrentTarget
    {
        get => _bufferStack.Count == 0 ? DrawBuffer : _bufferStack.Peek();
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

    #region Initialization

    internal void Initialize()
    {
        GLThread.BindThread();

        Engine.Log.Info($"OpenGL Context {Gl.CurrentVersion}", MessageSource.Renderer);
        Engine.Log.Info($" Renderer: {Gl.CurrentRenderer}", MessageSource.Renderer);
        Engine.Log.Info($" Vendor: {Gl.CurrentVendor}", MessageSource.Renderer);
        Engine.Log.Info($" Shader: {Gl.CurrentShadingVersion}", MessageSource.Renderer);

        // Set flags.
        CompatibilityMode = Gl.SoftwareRenderer || Engine.Configuration.RendererCompatMode;
        Dsa = !CompatibilityMode && Gl.CurrentVersion.Major >= 4 && Gl.CurrentVersion.Minor >= 5;
        TextureBindLimit = Gl.SoftwareRenderer || Gl.CurrentVersion.Profile == KhronosVersion.PROFILE_WEBGL ? 16 : Gl.CurrentLimits.MaxTextureImageUnits;

        Engine.Log.Info($" Flags: " +
                        $"{(CompatibilityMode ? "Compat, " : "")}" +
                        $"{(Dsa ? "Dsa, " : "")}" +
                        $"Textures[{TextureBindLimit}]", MessageSource.Renderer);

        // Attach callback if debug mode is enabled.
        bool hasDebugSupport = Gl.CurrentExtensions.DebugOutput_ARB || (Gl.CurrentVersion.Major >= 4 && Gl.CurrentVersion.Minor >= 3);
        if (Engine.Configuration.GlDebugMode && !CompatibilityMode && hasDebugSupport)
        {
            Gl.Enable(EnableCap.DebugOuput);
            Gl.Enable(EnableCap.DebugOutputSynchronous);
            Gl.DebugMessageCallback(_glDebugCallback, IntPtr.Zero);
            Engine.Log.Trace("Attached OpenGL debug callback.", MessageSource.Renderer);
        }
        // In release mode GL errors are not checked after every call.
        // In that case a error catching callback is attached so that GL errors are logged.
        else if (hasDebugSupport)
        {
            Gl.Enable(EnableCap.DebugOuput);
            Gl.DebugMessageCallback(_glErrorCatchCallback, IntPtr.Zero);
            Engine.Log.Info("Attached OpenGL error catching callback.", MessageSource.Renderer);
        }

        // Create a representation of the screen buffer, and the buffer which will be drawn to.
        Vector2 windowSize = Engine.Host.Size;
        ScreenBuffer = new FrameBuffer(0, windowSize);
        DrawBuffer = !Engine.Configuration.UseIntermediaryBuffer ? new FrameBuffer(0, windowSize) : new FrameBuffer(windowSize).WithColor().WithDepthStencil();
        _bufferStack.Push(DrawBuffer);

        // Decide on scaling mode.
        if (Engine.Configuration.ScaleBlackBars)
        {
            Assert(Engine.Configuration.UseIntermediaryBuffer, "Scale black bars requires an intermediary buffer.");
            Engine.Host.OnResize += HostResizedBlackBars;
            HostResizedBlackBars(windowSize);
        }
        else
        {
            Engine.Host.OnResize += HostResized;
            HostResized(windowSize);
        }

        // Create default camera. RenderState applying requires one to be set.
        Camera = new Camera2D(Vector3.Zero, 1, KeyListenerType.None);

        // Setup GL state.
        Vector4 c = Engine.Configuration.ClearColor.ToVec4();
        Gl.ClearColor((int)c.X, (int)c.Y, (int)c.Z, (int)c.W);

        // Create bound dictionary for all slots and texture types
        TextureTarget[] possibleTypes = Enum.GetValues<TextureTarget>();
        foreach (TextureTarget type in possibleTypes)
        {
            TextureObjectBase.Bound[type] = new uint[Engine.Renderer.TextureBindLimit];
        }

        // Initialize misc graphics objects.
        Texture.InitializeEmptyTexture();

        // Load base assets.
        IRoutineWaiter loadingRoutine = Engine.Jobs.Add(InitializeSystemAssetsRoutineAsync());

        // Create default render states (depends on shaders)
        CurrentState = new RenderState();

        BlitState = RenderState.Default;
        BlitState.AlphaBlending = false;
        BlitState.DepthTest = false;
        BlitState.ViewMatrix = false;
        BlitState.Shader = "Shaders/Blit.xml";

        BlitStatePremult = BlitState;
        BlitStatePremult.Shader = "Shaders/BlitPremultAlpha.xml";

        // Create render objects
        RenderStream = new RenderStreamBatch(); // This is used for IM-like rendering.
        MeshEntityRenderer = new MeshEntityBatchRenderer();
        TextRenderer.Init();

        // Apply display settings (this is the initial application) and attach the camera updating coroutine.
        ApplySettings();
        UpdateCamera();

        _systemAssetsLoading = loadingRoutine;
    }

    #endregion

    #region System Loading

    /// <summary>
    /// Whether the renderer is fully initialized.
    /// </summary>
    public bool ReadyToRender { get => _systemAssetsLoading != null && _systemAssetsLoading.Finished; }

    private IRoutineWaiter? _systemAssetsLoading;

    private IEnumerator InitializeSystemAssetsRoutineAsync()
    {
        yield return ShaderFactory.LoadDefaultShadersRoutineAsync();
    }

    #endregion

    #region Event Handles and Sizing

    private static Gl.DebugProc _glErrorCatchCallback = GlErrorCatchCallback;

    private static unsafe void GlErrorCatchCallback(DebugSource source, DebugType msgType, uint id, DebugSeverity severity, int length, IntPtr message, IntPtr userParam)
    {
        if (msgType != DebugType.DebugTypeError) return;

        var stringMessage = new string((sbyte*) message, 0, length);
        Engine.Log.Warning(stringMessage, $"GL_{source}", true);
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
        Camera?.RecreateViewMatrix();
        Camera?.RecreateProjectionMatrix();
        ApplySettings();
    }

    /// <summary>
    /// Recalculate the draw buffer when the host is resized, using black bars.
    /// </summary>
    private void HostResizedBlackBars(Vector2 size)
    {
        // Calculate borderbox / pillarbox.
        float targetAspectRatio = Engine.Configuration.RenderSize.X / Engine.Configuration.RenderSize.Y;
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

            DrawBuffer.Resize(Engine.Configuration.RenderSize, true);
        }
        else
        {
            Vector2 sizeInsideBars = new Vector2(width, height);
            Vector2 baseRes = Engine.Configuration.RenderSize;
            Vector2 ratio = sizeInsideBars / baseRes;

            Scale = MathF.Min(ratio.X, ratio.Y);
            IntScale = (int)MathF.Floor(MathF.Min(size.X, size.Y) / MathF.Min(baseRes.X, baseRes.Y));

            DrawBuffer.Resize(sizeInsideBars, true);
            Engine.Log.Info($"Resized host to {size} - scale is {Scale} and int scale is {IntScale}", MessageSource.Renderer);
            Engine.Log.Info($"Drawbuffer size is {sizeInsideBars}", MessageSource.Renderer);
        }

        var vpX = (int) (size.X / 2 - width / 2);
        var vpY = (int) (size.Y / 2 - height / 2);

        // Set viewport.
        ScreenBuffer.Resize(size);
        ScreenBuffer.Viewport = new Rectangle(vpX, vpY, width, height);
       
        Camera?.RecreateViewMatrix();
        Camera?.RecreateProjectionMatrix();
        ApplySettings();
    }

    #endregion

    /// <summary>
    /// Called at the start of the frame, after GL tasks are executed.
    /// Resets the render state and clears old states from the previous frame and tasks.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Renderer StartFrame()
    {
        // Check if running on the GL Thread.
        Assert(GLThread.IsGLThread());
        InFrame = true;

        _matrixStack.Clear();

        // Run GLThread tasks.
        // Needs to be at the start, to ensure assets from others threads are uploaded etc.
        PerfProfiler.FrameEventStart("GLThread.Run");
        GLThread.Run();
        PerfProfiler.FrameEventEnd("GLThread.Run");

        // Reset to the default state
        // In compatibility mode dont carry over any state from the last frame as some drivers dont like it
        SetState(RenderState.Default, true);

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
        {
            Engine.Log.Warning($"A framebuffer was left bound from the last frame!", MessageSource.Renderer, true);
            while (_bufferStack.Count > 1) _bufferStack.Pop();
        }

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
        Assert(GLThread.IsGLThread());

        RenderDebugObjects();

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
        MeshEntityRenderer.DoTasks();
        TextRenderer.DoTasks();
        GPUMemoryAllocator.ProcessFreed();
        InFrame = false;
    }

    public void UpdateCamera()
    {
        Camera.Update();
    }

    #region Framebuffer, Shader, and Model Matrix Syncronization and State

    /// <summary>
    /// Synchronizes uniform properties with the currently bound shader.
    /// </summary>
    public void SyncShader()
    {
        ShaderProgram currentShader = CurrentShader;
        if (currentShader == null) return; // Before default shader is created
        PerfProfiler.FrameEventStart("ShaderSync");

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
        CurrentShader.SetUniformMatrix4("modelMatrix", ModelMatrix);
    }

    /// <summary>
    /// Synchronizes the view matrix to the current shader.
    /// Moved to a separate function so that it can be updated without updating all uniforms.
    /// </summary>
    private void SyncViewMatrix()
    {
        bool viewMatrixEnabled = Engine.Renderer.CurrentState.ViewMatrix;

        Matrix4x4 projectionMatrix;
        switch (Engine.Renderer.CurrentState.ProjectionBehavior)
        {
            default:
            case ProjectionBehavior.AlwaysCameraProjection:
            case ProjectionBehavior.AutoCamera when viewMatrixEnabled:
                projectionMatrix = _camera.ProjectionMatrix;
                break;
            case ProjectionBehavior.AlwaysDefault2D:
            case ProjectionBehavior.AutoCamera: // when !viewMatrixEnabled:
                projectionMatrix = CameraBase.GetDefault2DProjection();
                break;
        }
        CurrentShader.SetUniformMatrix4("projectionMatrix", projectionMatrix);

        // Check if the view matrix is off.
        Matrix4x4 viewMatrix = _camera.ViewMatrix;
        if (!Engine.Renderer.CurrentState.ViewMatrix)
        {
            // Same as in Camera2D as the default "no view" view is a 2D view.
            // Keep that code in sync :)
            viewMatrix =
                Matrix4x4.CreateScale(new Vector3(1, -1, 1)) *
                Matrix4x4.CreateLookAtLeftHanded(Vector3.Zero, new Vector3(0, 0, -1), Up2D);
        }

        CurrentShader.SetUniformMatrix4("viewMatrix", viewMatrix);
    }

    /// <summary>
    /// Ensures the current render target is current, and bound.
    /// </summary>
    public void EnsureRenderTarget()
    {
        // Happens on initialization.
        if (CurrentTarget == null) return;

        CurrentTarget.Bind();

        // Camera matrices depend on the current target.
        Camera.RecreateViewMatrix();
        Camera.RecreateProjectionMatrix();

        // Functions such as CacheGlyphs and other graphical tasks can run outside of the frame cycle.
        // The ExecuteOnGLThread will inline them despite !InFrame if executed on the GL Thread, and
        // since the Update loop also occurs on the GLThread this happens from time to time.
        // To ensure that the state is synced (usually the camera does it) we manually sync here in that case.
        if (!InFrame) SyncShader();
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
            Engine.Log.Warning("Cannot pop off the Drawbuffer.", MessageSource.Renderer, true);
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
        ReadOnlySpan<byte> utf8Str = new ReadOnlySpan<byte>((byte*) message, length);
        Span<char> stringAsUtf16 = stackalloc char[length];
        Utf8.ToUtf16(utf8Str, stringAsUtf16, out int _, out int charsWritten);
        stringAsUtf16 = stringAsUtf16.Slice(0, charsWritten);

        // NVidia drivers love to spam the debug log with how your buffers will be mapped in the system heap.
        if (msgType == DebugType.DebugTypeOther && stringAsUtf16.IndexOf("SYSTEM HEAP") != -1) return;
        if (msgType == DebugType.DebugTypeOther && stringAsUtf16.IndexOf("will use VIDEO memory") != -1) return;
        if (msgType == DebugType.DebugTypeOther && stringAsUtf16.IndexOf("Based on the usage hint and actual usage") != -1) return;

        switch (severity)
        {
            case DebugSeverity.DebugSeverityHigh:
                Engine.Log.ONE_Warning($"GL_{msgType}_{source}", stringAsUtf16);
                break;
            case DebugSeverity.DebugSeverityMedium:
                Engine.Log.ONE_Info($"GL_{msgType}_{source}", stringAsUtf16);
                break;
            default:
                Engine.Log.ONE_Trace($"GL_{msgType}_{source}", stringAsUtf16);
                break;
        }
    }

    private CameraBase? _virtualCamera;

    /// <summary>
    /// Set a camera to be returned by Engine.Renderer.Camera
    /// For all of the game code this camera will be considered the current camera (culling etc)
    /// but for internal rendering uses (such as the view matrix) the camera will be the actual current camera.
    /// This is used for debugging the camera.
    /// </summary>
    public void DebugSetVirtualCamera(CameraBase? camera)
    {
        _virtualCamera = camera;
        camera?.RecreateViewMatrix();
        camera?.RecreateProjectionMatrix();
    }

    #endregion
}