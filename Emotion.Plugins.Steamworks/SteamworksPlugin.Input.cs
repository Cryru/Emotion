using Emotion.Common;
using Emotion.Plugins.Steamworks.SteamSDK;
using System;

namespace Emotion.Plugins.Steamworks;

public partial class SteamworksPlugin
{
    private IntPtr _steamInput;

    public void InitInput()
    {
        _steamInput = SteamNative.GetSteamInput(_steamClient, _hSteamUser, _steamPipe, Constants.STEAMINPUT_INTERFACE_VERSION);
        bool initialized = SteamNative.ISteamInput_Init(_steamInput, false);
        if (!initialized)
        {
            Engine.Log.Warning("Steam Input didn't initialize.", LOG_SOURCE);
            return;
        }
    }
}
