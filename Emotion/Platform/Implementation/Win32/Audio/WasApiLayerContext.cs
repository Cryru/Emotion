using Emotion.Standard.Audio;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using WinApi.ComBaseApi.COM;

namespace Emotion.Platform.Implementation.Win32.Audio
{
    public class WasApiLayerContext
    {
        internal WasApiAudioDevice Parent { get; private set; }

        internal bool Initialized { get; private set; }
        internal bool Started { get; private set; }

        internal IAudioClient AudioClient;
        internal AudioFormat AudioClientFormat;
        internal uint BufferSize;

        internal long UpdatePeriod
        {
            get => _updatePeriod;
            set
            {
                _updatePeriod = value;
                TimeoutPeriod = (int)(3 * (value / 1000));
            }
        }
        internal int TimeoutPeriod;

        internal IAudioRenderClient RenderClient;
        internal EventWaitHandle WaitHandle;

        private long _updatePeriod;

        public WasApiLayerContext(WasApiAudioDevice parent)
        {
            Parent = parent;
        }

        public void Start()
        {
            Initialized = true;
            int error = AudioClient.Start();
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't start a layer context of device {Parent.Name}.", true);
            }
            Started = true;
        }

        public void Stop()
        {
            int error = AudioClient.Stop();
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't stop a layer context of device {Parent.Name}.", true);
            }
            Started = false;
        }

        public void Reset()
        {
            int error = AudioClient.Reset();
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't reset a layer context of device {Parent.Name}.", true);
            }
            Initialized = false;
        }
    }
}
