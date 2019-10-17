using System;
using System.Runtime.InteropServices;
using System.Threading;
using WinApi.ComBaseApi.COM;

namespace Emotion.Platform.Implementation.Win32.Audio
{
    public class WasApiAudioDevice : AudioDevice
    {
        public static Guid IdAudioMeterInformation = new Guid("C02216F6-8C67-4B5B-9D00-D008E73E0064");
        public static Guid IdAudioEndpointVolume = new Guid("5CDF2C82-841E-4546-9722-0CF74078229A");
        public static Guid IdAudioClient = new Guid("1CB9AD4C-DBFA-4c32-B178-C2F568A703B2");
        public static Guid IdAudioSessionManager = new Guid("BFA971F1-4D5E-40BB-935E-967039BFBEE4");
        public static Guid IdDeviceTopology = new Guid("2A07407E-6497-4A18-9787-32F79BD0D98F");
        public static Guid IdAudioRenderClient = new Guid("F294ACFC-3146-4483-A7BF-ADDCA7C260E2");

        internal string Id { get; }

        internal bool Initialized { get; private set; }
        internal bool Started { get; private set; }

        internal IMMDevice ComHandle;
        internal IAudioClient AudioClient;
        internal WaveFormat AudioClientFormat;
        internal uint BufferSize;
        internal long UpdatePeriod;
        internal IAudioRenderClient RenderClient;
        internal IAudioEndpointVolume VolumeController;
        internal EventWaitHandle WaitHandle;

        internal WasApiAudioDevice(string id, string name, IMMDevice handle)
        {
            Id = id;
            Name = name;
            ComHandle = handle;
        }

        /// <summary>
        /// Late initialization for the WasApi backend's representation of this device.
        /// </summary>
        public void Initialize()
        {
            if(Initialized) return;

            // Activate the device.
            int error = ComHandle.Activate(ref IdAudioClient, ClsCtx.ALL, IntPtr.Zero, out object audioDevice);
            AudioClient = (IAudioClient) audioDevice;
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't activate audio device of name {Name}.", true);
            }

            // todo: use the service from the audio client
            error = ComHandle.Activate(ref IdAudioEndpointVolume, ClsCtx.ALL, IntPtr.Zero, out object volumeControl);
            VolumeController = (IAudioEndpointVolume) volumeControl;
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't activate volume controller of name {Name}.", true);
            }

            // Get device format.
            error = AudioClient.GetMixFormat(out IntPtr deviceFormat);
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't detect the mix format of the audio client of {Name}.", true);
            }

            AudioClientFormat = Marshal.PtrToStructure<WaveFormat>(deviceFormat);
            if (AudioClientFormat.ExtraSize >= 22) AudioClientFormat = Marshal.PtrToStructure<WaveFormatExtensible>(deviceFormat);

            error = AudioClient.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.EventCallback, 0, 0, deviceFormat, Guid.Empty);
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't initialize the audio client of device {Name}. Mix format is of the {AudioClientFormat.Tag} type.", true);
            }

            // Get data.
            error = AudioClient.GetDevicePeriod(out long _, out long minPeriod);
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't get device {Name} period.", true);
            }
            UpdatePeriod = minPeriod;

            error = AudioClient.GetBufferSize(out BufferSize);
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't get device {Name} buffer size.", true);
            }

            // Set wait handle for when the client is ready to process a buffer.
            WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            error = AudioClient.SetEventHandle(WaitHandle.SafeWaitHandle.DangerousGetHandle());
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't set audio wait handle for device {Name}.", true);
            }

            error = AudioClient.GetService(IdAudioRenderClient, out object audioRenderClient);
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't get the audio render client for device {Name}.", true);
            }

            RenderClient = (IAudioRenderClient) audioRenderClient;

            Initialized = true;
        }

        public void Start()
        {
            int error = AudioClient.Start();
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't start the audio client of device {Name}.", true);
            }
            Started = true;
        }

        public void Stop()
        {
            int error = AudioClient.Stop();
            if (error != 0)
            {
                Win32Platform.CheckError($"Couldn't stop the audio client of device {Name}.", true);
            }
            Started = false;
            Initialized = false;
        }
    }
}