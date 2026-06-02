#nullable enable

using Emotion.Core.Platform.Implementation.Null;
using Emotion.Core.Platform.Implementation.Web.Razor;
using Microsoft.JSInterop;

namespace Emotion.Core.Platform.Implementation.Web;

[DontSerialize]
public sealed class WebPlatform : PlatformBase
{
    public static WebPlatform ActiveHost { get; set; } = null!;

    private readonly RenderCanvas _canvasElement;
    private readonly DotNetObjectReference<WebPlatform> _hostReference;
    private Vector2 _size;
    private Action? _onTick;
    private Action? _onFrame;

    public IJSInProcessRuntime JsRuntime { get; }

    public WebPlatform(RenderCanvas canvasElement)
    {
        ActiveHost = this;

        DisplayMode = DisplayMode.Windowed;
        NamedThreads = false;

        _canvasElement = canvasElement;
        JsRuntime = canvasElement.JsRuntime;
        _hostReference = DotNetObjectReference.Create(this);
        JsRuntime.InvokeVoid("InitJavascript", _hostReference);

        Context = new WebGLContext(JsRuntime);
        Audio = new NullAudioContext(this);
    }

    public void InitLoop(Action tick, Action draw)
    {
        _onTick = tick;
        _onFrame = draw;
    }

    protected override void SetupInternal(Configurator config)
    {
        config.LoopFactory = InitLoop;
        config.Logger ??= new WebLogger();
    }

    public override void DisplayMessageBox(string message)
    {
        Console.WriteLine(message);
    }

    protected override bool UpdatePlatform()
    {
        return true;
    }

    public override WindowState WindowState { get; set; } = WindowState.Normal;

    protected override Vector2 GetPosition()
    {
        return Vector2.Zero;
    }

    protected override void SetPosition(Vector2 position)
    {
    }

    protected override Vector2 GetSize()
    {
        return _size;
    }

    protected override void SetSize(Vector2 size)
    {
        _size = size;
        Resized(size);
    }

    public override nint LoadLibrary(string path)
    {
        return nint.Zero;
    }

    public override nint GetLibrarySymbolPtr(nint library, string symbolName)
    {
        return nint.Zero;
    }

    protected override void UpdateDisplayMode()
    {
    }

    public override void Close()
    {
        base.Close();
        if (ActiveHost == this) ActiveHost = null;
        _hostReference.Dispose();
    }

    [JSInvokable]
    public void SetSizeJs(int width, int height)
    {
        SetSize(new Vector2(width, height));
    }

    [JSInvokable]
    public void RunLoop(float mouseX, float mouseY)
    {
        if (Engine.Status != EngineState.Running) return;

        try
        {
            Engine.Input.ReportMouseMove(new Vector2(mouseX, mouseY));
            _onTick?.Invoke();
            _onFrame?.Invoke();
        }
        catch (Exception ex)
        {
            Engine.CriticalError(ex);
            throw;
        }
    }

    [JSInvokable]
    public void KeyDown(int keyCode)
    {
        UpdateKeyStatus((Key)keyCode, true);
    }

    [JSInvokable]
    public void KeyUp(int keyCode)
    {
        UpdateKeyStatus((Key)keyCode, false);
    }

    [JSInvokable]
    public void MouseKeyDown(int keyCode)
    {
        UpdateKeyStatus((Key)(keyCode + (int)Key.MouseKeyStart), true);
    }

    [JSInvokable]
    public void MouseKeyUp(int keyCode)
    {
        UpdateKeyStatus((Key)(keyCode + (int)Key.MouseKeyStart), false);
    }
}
