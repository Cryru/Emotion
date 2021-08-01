#region Using

using System;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Standard.Logging;
using OpenAL;

#endregion

namespace Emotion.Platform.Implementation.OpenAL
{
    public sealed class OpenALAudioAdapter : ThreadedAudioAdapter
    {
        public IntPtr AudioDevice { get; private set; }
        public IntPtr AudioContext { get; private set; }

        public static OpenALAudioAdapter TryCreate(PlatformBase platform)
        {
            var newCtx = new OpenALAudioAdapter(platform);
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

        public OpenALAudioAdapter(PlatformBase platform) : base(platform)
        {
        }

        protected override AudioLayer CreatePlatformAudioLayerInternal(string layerName)
        {
            return new OpenALAudioLayer(layerName, this);
        }

        protected override void UpdateLayer(AudioLayer layer)
        {
            var alLayer = layer as OpenALAudioLayer;
            alLayer?.ProcUpdate();
        }
    }
}