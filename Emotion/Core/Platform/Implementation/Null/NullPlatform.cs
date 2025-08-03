#nullable enable

using Emotion;
using Emotion.Core;
using Emotion.Core.Platform;
using Emotion.Core.Platform;

namespace Emotion.Core.Platform.Implementation.Null;

public class NullPlatform : PlatformBase
{
    public override void DisplayMessageBox(string message)
    {
        Console.WriteLine($"NullPlatform MessageBox: {message}");
    }

    protected override bool UpdatePlatform()
    {
        return true;
    }

    protected override void SetupInternal(Configurator config)
    {
        Audio = new NullAudioContext(this);
    }

    public override nint LoadLibrary(string path)
    {
        return nint.Zero;
    }

    public override nint GetLibrarySymbolPtr(nint library, string symbolName)
    {
        return nint.Zero;
    }

    public override WindowState WindowState { get; set; }
    private Vector2 _position { get; set; }
    private Vector2 _size { get; set; }

    protected override void SetPosition(Vector2 position)
    {
        _position = position;
    }

    protected override Vector2 GetPosition()
    {
        return _position;
    }

    protected override void SetSize(Vector2 size)
    {
        _size = size;
    }

    protected override Vector2 GetSize()
    {
        return _size;
    }

    protected override void UpdateDisplayMode()
    {
    }
}