#nullable enable

#region Using

using Emotion.Core.Platform.Implementation.Win32.Native.WasAPI;
using Emotion.Core.Systems.Audio;
using Emotion.Platform.Implementation.Win32;
#endregion

namespace Emotion.Core.Platform.Implementation.Win32.Audio;

public class WasApiLayerContext
{
    public WasApiAudioDevice Parent { get; private set; }

    public bool Initialized { get; private set; }
    public bool Started { get; private set; }

    internal IAudioRenderClient RenderClient;
    internal IAudioClient AudioClient;
    internal AudioFormat AudioClientFormat;

    public WasApiLayerContext(WasApiAudioDevice parent)
    {
        Parent = parent;
    }

    public void Start()
    {
        Initialized = true;
        int error = AudioClient.Start();
        if (error != 0) Win32Platform.CheckError($"Couldn't start a layer context of device {Parent.Name}.", true);
        Started = true;
    }

    public void Stop()
    {
        int error = AudioClient.Stop();
        if (error != 0) Win32Platform.CheckError($"Couldn't stop a layer context of device {Parent.Name}.", true);
        Started = false;
    }

    public void Reset()
    {
        int error = AudioClient.Reset();
        if (error != 0) Win32Platform.CheckError($"Couldn't reset a layer context of device {Parent.Name}.", true);
        Initialized = false;
    }
}