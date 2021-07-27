#region Using

using System.Threading;
using Emotion.Standard.Audio;
using WinApi.ComBaseApi.COM;

#endregion

namespace Emotion.Platform.Implementation.Win32.Audio
{
    public class WasApiLayerContext
    {
        public WasApiAudioDevice Parent { get; private set; }

        public bool Initialized { get; private set; }
        public bool Started { get; private set; }

        internal IAudioRenderClient RenderClient;
        internal IAudioClient AudioClient;
        internal AudioFormat AudioClientFormat;
        internal uint BufferSize;

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
}