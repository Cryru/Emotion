#region Using

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.IO;
using Emotion.Standard.Logging;
using WinApi.ComBaseApi.COM;

#endregion

namespace Emotion.Platform.Implementation.Win32.Audio
{
    public class WasApiAudioContext : AudioContext, IMMNotificationClient
    {
        public static WasApiAudioContext TryCreate()
        {
            // WasApi should be present on anything Vista and up.
            if (!Win32Platform.IsWindowsVistaOrGreater) return null;

            // ReSharper disable once SuspiciousTypeConversion.Global
            if (new MMDeviceEnumeratorComObject() is IMMDeviceEnumerator enumerator) return new WasApiAudioContext(enumerator);
            Win32Platform.CheckError("Couldn't create multimedia enumerator.", true);
            return null;
        }

        private IMMDeviceEnumerator _enumerator;
        private Dictionary<string, WasApiAudioDevice> _devices = new Dictionary<string, WasApiAudioDevice>();
        private WasApiAudioDevice _defaultDevice;

        internal WasApiAudioContext(IMMDeviceEnumerator enumerator)
        {
            _enumerator = enumerator;
            int error = _enumerator.EnumAudioEndpoints(DataFlow.Render, DeviceState.Active, out IMMDeviceCollection collection);
            if (error != 0)
            {
                Win32Platform.CheckError("Couldn't detect audio devices.", true);
                return;
            }

            // Get initial devices.
            error = collection.GetCount(out int count);
            if (error != 0)
            {
                Win32Platform.CheckError("Couldn't detect the number of audio devices.", true);
                return;
            }

            for (var i = 0; i < count; i++)
            {
                error = collection.Item(i, out IMMDevice device);
                if (error != 0)
                {
                    Win32Platform.CheckError($"Couldn't retrieve audio device of index {i}.");
                    continue;
                }

                ParseDevice(device);
            }

            // Find the default device.
            SetDefaultDevice();

            // Register to audio device events.
            _enumerator.RegisterEndpointNotificationCallback(this);

            // Start the loop.
            Task.Run(AudioLoop);
        }

        private void AudioLoop()
        {
            if (Thread.CurrentThread.Name == null) Thread.CurrentThread.Name = "WASAPI Audio Thread";

            Engine.Log.Info("Starting audio loop...", MessageSource.Audio);

            while (!Environment.HasShutdownStarted)
            {
                if (!_defaultDevice.Initialized) _defaultDevice.Initialize();

                if (_test == null || _reader == null) continue;

                var frameCount = (int) _defaultDevice.BufferSize;

                // Ensure format.
                if (!_defaultDevice.AudioClientFormat.Equals(_reader.ConvFormat)) _reader.SetFormat(_defaultDevice.AudioClientFormat);

                if (!_defaultDevice.Started)
                {
                    FillBuffer(_defaultDevice.RenderClient, ref _reader, (int) _defaultDevice.BufferSize);
                    _defaultDevice.Start();
                }

                var timeout = (int) (3 * (_defaultDevice.UpdatePeriod / 1000));
                bool success = _defaultDevice.WaitHandle.WaitOne(timeout);
                if (!success)
                {
                    Engine.Log.Warning("Audio device wait timeout.", MessageSource.Audio);
                    continue;
                }

                int error = _defaultDevice.AudioClient.GetCurrentPadding(out int padding);
                if (error != 0) Engine.Log.Warning($"Couldn't get device padding, error {error}.", MessageSource.Audio);

                if (!FillBuffer(_defaultDevice.RenderClient, ref _reader, frameCount - padding)) continue;
                // Wait for the final samples to be read.
                Task.Delay((int) (_defaultDevice.UpdatePeriod / 1000)).Wait();
                _defaultDevice.Stop();
                _test = null;
                _reader = null;
            }

            Engine.Log.Info("Audio loop has ended.", MessageSource.Audio);
        }

        /// <summary>
        /// Fill a render client buffer.
        /// </summary>
        /// <param name="client">The client to fill.</param>
        /// <param name="wasApiSoundReader">The buffer to fill from.</param>
        /// <param name="bufferFrameCount">The number of samples to fill with.</param>
        /// <returns>Whether the buffer has been read to the end.</returns>
        private unsafe bool FillBuffer(IAudioRenderClient client, ref WasApiSoundDataReader wasApiSoundReader, int bufferFrameCount)
        {
            if (bufferFrameCount == 0) return false;

            int error = client.GetBuffer(bufferFrameCount, out IntPtr bufferPtr);
            if (error != 0) Engine.Log.Warning($"Couldn't get device buffer, error {error}.", MessageSource.Audio);
            var buffer = new Span<byte>((void*) bufferPtr, bufferFrameCount * wasApiSoundReader.ConvBytesPerFrame);
            int frames = wasApiSoundReader.Read(buffer, bufferFrameCount);
            error = client.ReleaseBuffer(frames, frames == 0 ? AudioClientBufferFlags.Silent : AudioClientBufferFlags.None);
            if (error != 0) Engine.Log.Warning($"Couldn't release device buffer, error {error}.", MessageSource.Audio);
            return frames == 0;
        }

        #region Events

        public void OnDeviceStateChanged(string deviceId, DeviceState newState)
        {
        }

        public void OnDeviceAdded(string pwstrDeviceId)
        {
            ParseDevice(pwstrDeviceId);
            SetDefaultDevice();
        }

        public void OnDeviceRemoved(string deviceId)
        {
            bool removed = _devices.Remove(deviceId, out WasApiAudioDevice device);
            if (removed)
                Engine.Log.Trace($"Disconnected audio device - {device.Name}", MessageSource.Win32);
            // If the default device was just removed, find the new default device.
            if (removed && device.Default)
                SetDefaultDevice();
        }

        public void OnDefaultDeviceChanged(DataFlow flow, Role role, string defaultDeviceId)
        {
            // Update default device.
            if (flow != DataFlow.Render || role != Role.Console) return;

            SetDefaultDevice(defaultDeviceId);
        }

        public void OnPropertyValueChanged(string pwstrDeviceId, PropertyKey key)
        {
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Parse and activate a device.
        /// If successful it is added to the devices list.
        /// </summary>
        /// <param name="id">The unique id of the device.</param>
        private void ParseDevice(string id)
        {
            int error = _enumerator.GetDevice(id, out IMMDevice device);
            if (error != 0) Win32Platform.CheckError($"Couldn't get device of id {id}.", true);

            ParseDevice(device);
        }

        /// <summary>
        /// Parse and activate a device.
        /// If successful it is added to the devices list.
        /// </summary>
        /// <param name="device">The device to parse.</param>
        private void ParseDevice(IMMDevice device)
        {
            // Try to get the device unique id.
            int error = device.GetId(out string id);
            if (error != 0)
            {
                Win32Platform.CheckError("Couldn't retrieve audio device id.");
                return;
            }

            ParseDevice(device, id);
        }

        /// <summary>
        /// Parse and activate a device.
        /// If successful it is added to the devices list.
        /// </summary>
        /// <param name="device">The device to parse.</param>
        /// <param name="id">The unique id of the device.</param>
        private void ParseDevice(IMMDevice device, string id)
        {
            if (device == null) return;

            string deviceName = $"Unknown Device ({id})";

            // Try to get the device name.
            device.OpenPropertyStore(StorageAccessMode.Read, out IPropertyStore store);
            if (store != null)
            {
                store.GetValue(ref PropertyKey.PKEY_Device_FriendlyName, out PropVariant deviceNameProp);
                object parsed = deviceNameProp.Value;
                if (parsed is string deviceNameStr) deviceName = deviceNameStr;
            }

            // Add to the list.
            _devices.Add(id, new WasApiAudioDevice(id, deviceName, device));
            Engine.Log.Trace($"Detected audio device - {deviceName}", MessageSource.Win32);
        }

        /// <summary>
        /// Finds the default device and sets it as such.
        /// </summary>
        private void SetDefaultDevice()
        {
            _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console, out IMMDevice endpoint);
            int error = endpoint.GetId(out string defaultId);
            if (error != 0)
                Win32Platform.CheckError("Couldn't retrieve the id of the default audio device.");
            else
                SetDefaultDevice(defaultId);
        }

        /// <summary>
        /// Sets the specified id to be the default device.
        /// </summary>
        /// <param name="id">The id of the device to set as default.</param>
        private void SetDefaultDevice(string id)
        {
            _devices.TryGetValue(id, out WasApiAudioDevice defaultDevice);
            if (defaultDevice != null)
            {
                // Unset old default.
                foreach (KeyValuePair<string, WasApiAudioDevice> device in _devices)
                {
                    device.Value.Default = false;
                }

                defaultDevice.Default = true;
                _defaultDevice = defaultDevice;
                Engine.Log.Trace($"Default audio device is: {defaultDevice.Name}.", MessageSource.Win32);
            }
            else
            {
                Win32Platform.CheckError("Default audio device is not in device list.");
            }
        }

        #endregion

        private WaveSoundAsset _test;
        private WasApiSoundDataReader _reader;

        public override void PlayAudioTest(WaveSoundAsset wav)
        {
            _test = wav;
            _reader = new WasApiSoundDataReader(_test);
        }
    }
}