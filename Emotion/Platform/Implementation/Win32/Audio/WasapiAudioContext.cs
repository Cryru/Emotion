﻿#region Using

using System.Collections.Generic;
using System.Linq;
using Emotion.Audio;
using Emotion.Common;
using Emotion.Standard.Audio;
using Emotion.Standard.Logging;
using WinApi.ComBaseApi.COM;

#endregion

namespace Emotion.Platform.Implementation.Win32.Audio
{
    public sealed class WasApiAudioContext : AudioContext, IMMNotificationClient
    {
        public static WasApiAudioContext TryCreate()
        {
            // ReSharper disable once SuspiciousTypeConversion.Global
            if (new MMDeviceEnumeratorComObject() is IMMDeviceEnumerator enumerator) return new WasApiAudioContext(enumerator);
            Win32Platform.CheckError("Couldn't create multimedia enumerator.", true);
            return null;
        }

        public WasApiAudioDevice DefaultDevice { get; private set; }

        private IMMDeviceEnumerator _enumerator;
        private Dictionary<string, WasApiAudioDevice> _devices = new Dictionary<string, WasApiAudioDevice>();

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
        }

        private AudioFormat AudioFormatFromWasApiFormat(WaveFormat fmt)
        {
            return new AudioFormat(fmt.BitsPerSample, fmt.IsFloat(), fmt.Channels, fmt.SampleRate);
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
        private WasApiAudioDevice ParseDevice(string id)
        {
            int error = _enumerator.GetDevice(id, out IMMDevice device);
            if (error != 0) Win32Platform.CheckError($"Couldn't get device of id {id}.", true);

            return ParseDevice(device);
        }

        /// <summary>
        /// Parse and activate a device.
        /// If successful it is added to the devices list.
        /// </summary>
        /// <param name="device">The device to parse.</param>
        private WasApiAudioDevice ParseDevice(IMMDevice device)
        {
            // Try to get the device unique id.
            int error = device.GetId(out string id);
            if (error != 0)
            {
                Win32Platform.CheckError("Couldn't retrieve audio device id.");
                return null;
            }

            return ParseDevice(device, id);
        }

        /// <summary>
        /// Parse and activate a device.
        /// If successful it is added to the devices list.
        /// </summary>
        /// <param name="device">The device to parse.</param>
        /// <param name="id">The unique id of the device.</param>
        private WasApiAudioDevice ParseDevice(IMMDevice device, string id)
        {
            if (device == null) return null;

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
            var dev = new WasApiAudioDevice(id, deviceName, device);
            _devices.Add(id, dev);
            Engine.Log.Trace($"Detected audio device - {deviceName}", MessageSource.Win32);

            return dev;
        }

        /// <summary>
        /// Finds the default device and sets it as such.
        /// </summary>
        private void SetDefaultDevice()
        {
            _enumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Console, out IMMDevice endpoint);
            if (endpoint == null) return;
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
            if (defaultDevice == null)
            {
                // Default audio device was not found in the device list - query for it.
                defaultDevice = ParseDevice(id);
                if (defaultDevice == null)
                    Win32Platform.CheckError("Default audio device is not in device list.", true);
            }

            // Unset old default.
            foreach (KeyValuePair<string, WasApiAudioDevice> device in _devices)
            {
                device.Value.Default = false;
            }

            defaultDevice.Default = true;
            DefaultDevice = defaultDevice;
            Engine.Log.Trace($"Default audio device is: {defaultDevice.Name}.", MessageSource.Win32);

            // Tell all layers about this change.
            lock (_layers)
            {
                foreach (WasApiLayer layer in _layers)
                {
                    layer.DefaultDeviceChanged(defaultDevice);
                }
            }
        }

        #endregion

        public override string[] GetLayers()
        {
            string[] names;
            lock (_layers)
            {
                names = new string[_layers.Count];
                for (int i = 0; i < _layers.Count; i++)
                {
                    names[i] = _layers[i].Name;
                }
            }

            return names;
        }

        private List<WasApiLayer> _layers = new List<WasApiLayer>();

        public override AudioLayer CreateLayer(string layerName, float layerVolume = 1)
        {
            var newLayer = new WasApiLayer(layerName, this) {Volume = layerVolume};
            lock (_layers)
            {
                _layers.Add(newLayer);
            }

            return newLayer;
        }

        public override void RemoveLayer(string layerName)
        {
            var layer = (WasApiLayer) GetLayer(layerName);
            if (layer == null) return;

            layer.Stop();
            layer.Dispose();
            lock (_layers)
            {
                _layers.Remove(layer);
            }
        }

        public override AudioLayer GetLayer(string layerName)
        {
            lock (_layers)
            {
                return _layers.FirstOrDefault(layer => layer.Name == layerName);
            }
        }
    }
}