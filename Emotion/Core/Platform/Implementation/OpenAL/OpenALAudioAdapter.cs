#nullable enable

#region Using

using Emotion.Core.Systems.Audio;
using Emotion.Core.Platform.Implementation.OpenAL;
using OpenAL;

#endregion

namespace Emotion.Core.Platform.Implementation.OpenAL;

// todo: default device swiching
public sealed class OpenALAudioAdapter : AudioContext
{
    public IntPtr AudioDevice { get; private set; }
    public IntPtr AudioContext { get; private set; }

    public OpenALAudioAdapter(PlatformBase platform) : base(platform)
    {
    }

    public static OpenALAudioAdapter TryCreate(PlatformBase platform)
    {
        var newCtx = new OpenALAudioAdapter(platform);
        bool created = newCtx.RecreateAudioContext();
        return !created ? null : newCtx;
    }

    public override AudioLayer CreatePlatformAudioLayer(string layerName)
    {
        return new OpenALAudioLayer(layerName, this);
    }

    /// <summary>
    /// OpenAL doesn't detect device changes easily.
    /// </summary>
    public bool RecreateAudioContext()
    {
        AudioDevice = Alc.OpenDevice(null);
        if (AudioDevice == IntPtr.Zero)
        {
            Engine.Log.Error("Couldn't find an OpenAL device.", MessageSource.OpenAL);
            return false;
        }

        var attr = new int[0];
        AudioContext = Alc.CreateContext(AudioDevice, attr);
        if (AudioContext == IntPtr.Zero)
        {
            Engine.Log.Error("Couldn't create OpenAL context.", MessageSource.OpenAL);
            return false;
        }

        bool success = Alc.MakeContextCurrent(AudioContext);
        if (!success)
        {
            Engine.Log.Error("Couldn't make OpenAL context current.", MessageSource.OpenAL);
            return false;
        }

        if (_layerMapping != null)
            for (var i = 0; i < _layerMapping.Count; i++)
            {
                AudioLayer layer = _layerMapping[i];
                if (layer is OpenALAudioLayer alLayer) // All of them should be.
                    alLayer.RecreateALObjects();
            }

        return true;
    }

    public override void Dispose()
    {
        Alc.CloseDevice(AudioDevice);
        base.Dispose();
    }
}