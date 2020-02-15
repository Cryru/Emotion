﻿#region Using

using System;
using System.Runtime.InteropServices;
using System.Threading;
using WinApi.ComBaseApi.COM;

#endregion

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
        internal IMMDevice ComHandle;

        internal WasApiAudioDevice(string id, string name, IMMDevice handle)
        {
            Id = id;
            Name = name;
            ComHandle = handle;
        }

        /// <summary>
        /// Late initialization for the WasApi backend's representation of this device.
        /// </summary>
        public WasApiLayerContext CreateLayerContext()
        {
            var context = new WasApiLayerContext(this);

            // Activate the device.
            int error = ComHandle.Activate(ref IdAudioClient, ClsCtx.ALL, IntPtr.Zero, out object audioDevice);
            var audioClient = (IAudioClient) audioDevice;
            if (error != 0) Win32Platform.CheckError($"Couldn't activate audio device of name {Name}.", true);

            context.AudioClient = audioClient;

            // Get device format.
            error = audioClient.GetMixFormat(out IntPtr deviceFormat);
            if (error != 0) Win32Platform.CheckError($"Couldn't detect the mix format of the audio client of {Name}.", true);

            var audioClientFormat = Marshal.PtrToStructure<WaveFormat>(deviceFormat);
            if (audioClientFormat.ExtraSize >= 22) audioClientFormat = Marshal.PtrToStructure<WaveFormatExtensible>(deviceFormat);
            context.AudioClientFormat = audioClientFormat.ToEmotionFormat();

            error = audioClient.Initialize(AudioClientShareMode.Shared, AudioClientStreamFlags.EventCallback, 0, 0, deviceFormat, Guid.Empty);
            if (error != 0) Win32Platform.CheckError($"Couldn't initialize the audio client of device {Name}. Mix format is of the {audioClientFormat.Tag} type.", true);

            // Get data.
            error = audioClient.GetDevicePeriod(out long _, out long minPeriod);
            if (error != 0) Win32Platform.CheckError($"Couldn't get device {Name} period.", true);
            context.UpdatePeriod = minPeriod;

            error = audioClient.GetBufferSize(out context.BufferSize);
            if (error != 0) Win32Platform.CheckError($"Couldn't get device {Name} buffer size.", true);

            // Set wait handle for when the client is ready to process a buffer.
            context.WaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
            error = audioClient.SetEventHandle(context.WaitHandle.SafeWaitHandle.DangerousGetHandle());
            if (error != 0) Win32Platform.CheckError($"Couldn't set audio wait handle for device {Name}.", true);

            error = audioClient.GetService(IdAudioRenderClient, out object audioRenderClient);
            if (error != 0) Win32Platform.CheckError($"Couldn't get the audio render client for device {Name}.", true);

            context.RenderClient = (IAudioRenderClient) audioRenderClient;

            return context;
        }
    }
}