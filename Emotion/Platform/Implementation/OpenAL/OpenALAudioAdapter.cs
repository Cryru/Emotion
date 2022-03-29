#region Using

using System;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Standard.Logging;
using OpenAL;

#endregion

namespace Emotion.Platform.Implementation.OpenAL
{
    public sealed class OpenALAudioAdapter : AudioContext
    {
        public IntPtr AudioDevice { get; private set; }
        public IntPtr AudioContext { get; private set; }

        public static OpenALAudioAdapter TryCreate(PlatformBase _)
        {
            var newCtx = new OpenALAudioAdapter();
            newCtx.AudioDevice = Alc.OpenDevice(null);
            if (newCtx.AudioDevice == IntPtr.Zero)
            {
                Engine.Log.Error("Couldn't find an OpenAL device.", MessageSource.OpenAL);
                return null;
            }

            var attr = new int[0];
            newCtx.AudioContext = Alc.CreateContext(newCtx.AudioDevice, attr);
            if (newCtx.AudioContext == IntPtr.Zero)
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

        public override AudioLayer CreatePlatformAudioLayer(string layerName)
        {
            return new OpenALAudioLayer(layerName, this);
        }

        public override void Dispose()
        {
            Alc.CloseDevice(AudioDevice);
            base.Dispose();
        }
    }
}