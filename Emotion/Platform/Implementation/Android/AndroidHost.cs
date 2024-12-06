#region Using

using System.Runtime.InteropServices;
using Android.Content.Res;
using Android.Graphics;
using Android.Opengl;
using Android.Util;
using Android.Views;
using Emotion.Platform.Implementation.CommonDesktop;
using Emotion.Platform.Input;
using Activity = Android.App.Activity;

#endregion

namespace Emotion.Platform.Implementation.Android;

public class AndroidHost : PlatformBase
{
    #region Android API

    public void SurfaceChanged()
    {
        // todo: gotta recreate everything yikes
        Resized(Size);
    }

    public void SurfaceCreated()
    {
        // Run main on the GL thread.
        EmotionActivity.MainActivity.Main();
        FocusChanged(true);
    }

    public void DrawFrame()
    {
        // This is the main loop.
        _onTick?.Invoke();
        _onFrame?.Invoke();
    }

    #endregion

    private Action _onTick;
    private Action _onFrame;
    private Activity _activity;

    public AndroidGraphicsContext AndroidContext;

    public AndroidHost(Activity activity)
    {
        _activity = activity;

        var surface = new OpenGLSurface(activity, OnTouchEvent);
        activity.SetContentView(surface);

        var renderer = new AndroidGLRenderer(this);
        surface.SetRenderer(renderer);

        activity.SetContentView(surface);

        // Will flush only when there is something to flush.
        surface.RenderMode = Rendermode.Continuously;
        surface.PreserveEGLContextOnPause = true; // Dont kill our context on minimize pls

        AndroidContext = new AndroidGraphicsContext(this, surface, renderer);
        Context = AndroidContext;

        Audio = new AndroidAudio(this);
    }

    #region Platform API

    protected override void SetupInternal(Configurator config)
    {
        config.Logger = new AndroidLogger();

        // On Android the loop is managed by the built in renderer.
        config.LoopFactory = (onTick, onFrame) =>
        {
            _onTick = onTick;
            _onFrame = onFrame;
        };

        Engine.AssetLoader.AddSource(new AndroidAssetSource(_activity));
        Engine.AssetLoader.AddStore(new FileAssetStore("Player"));
    }

    public override void DisplayMessageBox(string message)
    {
    }

    protected override bool UpdatePlatform()
    {
        return true;
    }

    public override WindowState WindowState { get; set; } = WindowState.Normal;

    protected override void UpdateDisplayMode()
    {
        // unsupported - always fullscreen.
        DisplayMode = DisplayMode.Fullscreen;
    }

    protected override Vector2 GetPosition()
    {
        return Vector2.Zero;
    }

    protected override void SetPosition(Vector2 position)
    {
        // unsupported
    }

    protected override Vector2 GetSize()
    {
        var drawableRect = new Rect();
        AndroidContext.Surface.GetDrawingRect(drawableRect);
        return new Vector2(drawableRect.Width(), drawableRect.Height());
    }

    public override Vector2 GetDPI()
    {
        DisplayMetricsDensity? dpiEnum = Resources.System?.DisplayMetrics?.DensityDpi;
        int dpiNum = 0;
        if (dpiEnum != null)
            dpiNum = (int)dpiEnum;
        else
            dpiNum = 96;

        return new Vector2(dpiNum);
    }

    protected override void SetSize(Vector2 size)
    {
        // unsupported
    }

    public override nint LoadLibrary(string path)
    {
        return NativeLibrary.Load(path);
    }

    public override nint GetLibrarySymbolPtr(nint library, string symbolName)
    {
        return !NativeLibrary.TryGetExport(library, symbolName, out nint ptr) ? nint.Zero : ptr;
    }

    public override bool IsTouchScreen()
    {
        return true;
    }

    #endregion

    private Vector2 _prevTouch;

    private void OnTouchEvent(MotionEvent e)
    {
        Vector2 pos = new Vector2(e.GetX(), e.GetY());
        if (e.Action == MotionEventActions.Move ||
            e.Action == MotionEventActions.Down)
            UpdateMousePosition(pos);

        if (e.Action == MotionEventActions.Down)
        {
            UpdateKeyStatus(Key.MouseKeyLeft, true);
        }

        if (e.Action == MotionEventActions.Up)
        {
            UpdateKeyStatus(Key.MouseKeyLeft, false);
            UpdateMousePosition(new Vector2(-1));
        }

        _prevTouch = pos;
        if (e.Action == MotionEventActions.Move)
        {
            // todo: pinch to mouse wheel
        }
    }
}