#region Using

using System;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Standard.Logging;
using OpenAL;

#endregion

namespace Emotion.Platform.Implementation.OpenAL
{
    public sealed class OpenALAudioAdapter : IAudioAdapter
    {
        public IntPtr AudioDevice { get; private set; }
        public IntPtr AudioContext { get; private set; }

        public static OpenALAudioAdapter TryCreate()
        {
            var newCtx = new OpenALAudioAdapter();
            newCtx.AudioDevice = Alc.OpenDevice(null);
            var attr = new int[0];
            newCtx.AudioContext = Alc.CreateContext(newCtx.AudioDevice, attr);
            if (newCtx.AudioDevice == IntPtr.Zero || newCtx.AudioContext == IntPtr.Zero)
            {
                Engine.Log.Error("Couldn't create OpenAL context.", MessageSource.OpenAL);
                return null;
            }

            bool success = Alc.MakeContextCurrent(newCtx.AudioContext);
            if (!success)
            {
                Engine.Log.Error("Couldn't make OpenAL context current.", MessageSource.OpenAL);
                return null;
            }

            return newCtx;
        }

        public AudioLayer CreatePlatformAudioLayer(string layerName)
        {
            return new OpenALAudioLayer(layerName, this);
        }
    }
}