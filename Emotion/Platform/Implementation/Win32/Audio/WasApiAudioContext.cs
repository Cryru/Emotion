﻿#region Using

using Emotion.Audio;
using System.ComponentModel;
using WinApi.ComBaseApi.COM;
using WinApi.Kernel32;

#endregion

#nullable enable

namespace Emotion.Platform.Implementation.Win32.Audio;

public sealed class WasApiAudioContext : AudioContext, IMMNotificationClient
{
    public static WasApiAudioContext? TryCreate(PlatformBase platform)
    {
        try
        {
            IMMDeviceEnumerator enumerator = MMDeviceEnumeratorComObject.Create();
            return new WasApiAudioContext(platform, enumerator);
        }
        catch (Exception ex)
        {
            Engine.Log.Error("Couldn't create WasApi context.", MessageSource.Audio);
            Engine.Log.Error(ex);

            Win32Platform.CheckError("WasApi context creation", false, false);
        }

        return null;
    }

    public WasApiAudioDevice? DefaultDevice { get; private set; }
    public event Action<WasApiAudioDevice>? OnDefaultDeviceChangedInternal;

    private IMMDeviceEnumerator _enumerator;
    private Dictionary<string, WasApiAudioDevice> _devices = new();

    private WasApiAudioContext(PlatformBase platform, IMMDeviceEnumerator enumerator) : base(platform)
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
        DefaultDevice = null!;
        SetDefaultDevice();

        // Register to audio device events.
        _enumerator.RegisterEndpointNotificationCallback(this);
    }

    public override AudioLayer CreatePlatformAudioLayer(string layerName)
    {
        return new WasApiLayer(this, layerName);
    }

    #region Events

    public void OnDeviceStateChanged(string deviceId, DeviceState newState)
    {
    }

    public void OnDeviceAdded(string deviceId)
    {
        ParseDevice(deviceId);
        SetDefaultDevice();
    }

    public void OnDeviceRemoved(string deviceId)
    {
        bool removed = _devices.Remove(deviceId, out WasApiAudioDevice? device);
        if (removed)
        {
            AssertNotNull(device);
            Engine.Log.Trace($"Disconnected audio device - {device.Name}", MessageSource.Win32);

            // If the default device was just removed, find the new default device.
            if (device.Default)
                SetDefaultDevice();
        }
    }

    public void OnDefaultDeviceChanged(DataFlow flow, Role role, string? defaultDeviceId)
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
    private WasApiAudioDevice? ParseDevice(string id)
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
    private WasApiAudioDevice? ParseDevice(IMMDevice device)
    {
        // Try to get the device unique id.
        int error = device.GetId(out string? id);
        if (error != 0 || id == null)
        {
            Win32Platform.CheckError("Couldn't retrieve audio device id.");
            return null;
        }

        var deviceName = $"Unknown Device ({id})";

        // Try to get the device name.
        device.OpenPropertyStore(StorageAccessMode.Read, out IPropertyStore store);
        if (store != null)
        {
            store.GetValue(ref PropertyKey.PKEY_Device_FriendlyName, out PropVariant deviceNameProp);
            object parsed = deviceNameProp.GetValue();
            if (parsed is string deviceNameStr) deviceName = deviceNameStr;
        }

        // Add to the list.
        var dev = new WasApiAudioDevice(id, deviceName, device);
        _devices.TryAdd(id, dev);
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
        int error = endpoint.GetId(out string? defaultId);
        if (error != 0)
            Win32Platform.CheckError("Couldn't retrieve the id of the default audio device.");
        else if (defaultId != null)
            SetDefaultDevice(defaultId);
    }

    /// <summary>
    /// Sets the specified id to be the default device.
    /// </summary>
    /// <param name="id">The id of the device to set as default.</param>
    private void SetDefaultDevice(string? id)
    {
        WasApiAudioDevice? defaultDevice = null;
        if (id != null)
        {
            _devices.TryGetValue(id, out defaultDevice);
            if (defaultDevice == null)
            {
                // Default audio device was not found in the device list - query for it.
                defaultDevice = ParseDevice(id);
                if (defaultDevice == null)
                    Win32Platform.CheckError("Default audio device is not in device list.", true);
            }
        }

        // Unset old default.
        foreach (KeyValuePair<string, WasApiAudioDevice> device in _devices)
        {
            device.Value.Default = false;
        }

        if (defaultDevice == null) return;

        defaultDevice.Default = true;
        DefaultDevice = defaultDevice;
        Engine.Log.Trace($"Default audio device is: {defaultDevice.Name}.", MessageSource.Win32);
        OnDefaultDeviceChangedInternal?.Invoke(defaultDevice);
    }

    #endregion
}