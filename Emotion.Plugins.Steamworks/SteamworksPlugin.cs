#region Using

using System;
using System.Collections;
using System.IO;
using System.Text;
using Emotion.Common;
using Emotion.Game.Time;
using Emotion.Plugins.Steamworks.SteamSDK;

#endregion

namespace Emotion.Plugins.Steamworks
{
    public class SteamworksPlugin : IPlugin
    {
        /// <summary>
        /// The app id of "Space War" the application defined in the Steamworks examples for testing the API.
        /// </summary>
        public const uint DEBUG_APP_ID = 480;

        private const string LOG_SOURCE = "SteamPlugin";

        /// <summary>
        /// The app id of the currently loaded instance.
        /// </summary>
        public static uint AppId { get; private set; }

        /// <summary>
        /// Whether an instance exists.
        /// </summary>
        private bool _init;

        /// <summary>
        /// Runs steam callbacks at a specified frequency.
        /// </summary>
        private Every _callbackRunner;

        private IntPtr _steamClient;
        private IntPtr _steamPipe;
        private IntPtr _steamUser;
        private IntPtr _steamUtils;
        private IntPtr _steamUserStats;

        public SteamworksPlugin(uint appId, int callbackFrequencyMs = 2000)
        {
            if (_init) throw new Exception("Only one Steamworks Plugin can be active.");

            _init = true;
            AppId = appId;

            _callbackRunner = new Every(callbackFrequencyMs, SteamNative.RunCallbacks);
        }

        public void Initialize()
        {
            Engine.Host.AssociateAssemblyWithNativeLibrary(typeof(SteamworksPlugin).Assembly, "Steam", "steam_api64");

            if (Engine.Configuration.DebugMode)
            {
                // If in debug mode, create app id file if missing.
                if (!File.Exists("steam_appid.txt")) File.WriteAllText("steam_appid.txt", AppId.ToString());
            }
            else
            {
                // If not in debug mode, delete app id file.
                if (File.Exists("steam_appid.txt")) File.Delete("steam_appid.txt");
            }

            Engine.Log.Info($"Initializing Steam plugin - app id is {AppId}", LOG_SOURCE);

            // Check if we should restart.
            bool necessary = SteamNative.RestartAppIfNecessary(AppId);
            if (necessary)
            {
                Engine.Log.Warning("Steam API said we should restart.", LOG_SOURCE);
                Engine.Quit();
            }

            bool steamOpen = SteamNative.IsSteamRunning();
            if (!steamOpen)
            {
                Engine.Log.Warning("Steam is not running.", LOG_SOURCE);
                Engine.Quit();
            }

            try
            {
                bool initialized = SteamNative.Init();
                if (!initialized)
                {
                    Engine.Log.Warning("Steam didn't initialize.", LOG_SOURCE);
                    Engine.Quit();
                }
            }
            catch (Exception ex)
            {
                Engine.Log.Error($"Error while initializing Steam - {ex}.", LOG_SOURCE);
                Engine.Quit();
            }

            // Initialize Steam modules.
            _steamClient = SteamNative.GetSteamClient();
            _steamPipe = SteamNative.GetSteamPipe();
            _steamUser = SteamNative.GetSteamUser();
            _steamUtils = SteamNative.GetSteamUtils(_steamClient, _steamPipe, Constants.STEAMUTILS_INTERFACE_VERSION);
            _steamUserStats = SteamNative.GetSteamUserStats(_steamClient, _steamUser, _steamPipe, Constants.STEAMUSERSTATS_INTERFACE_VERSION);

            // Attach warning callback.
            SteamNative.SetWarningMessageHook(_steamUtils, _warningHook);

            bool statsReceived = SteamNative.RequestStats(_steamUserStats);
            if (!statsReceived) Engine.Log.Warning("User is not logged in to Steam.", LOG_SOURCE);

            SteamNative.RunCallbacks();
            Engine.CoroutineManager.StartCoroutine(UpdateRoutine());
        }

        private static SteamNative.WarningMessageHook _warningHook = WarningCallback;

        private static void WarningCallback(int severity, StringBuilder msg)
        {
            Engine.Log.Warning(msg.ToString(), $"SteamSDK-{severity}");
        }

        public IEnumerator UpdateRoutine()
        {
            while (Engine.Status != EngineStatus.Stopped)
            {
                _callbackRunner.Update(Engine.DeltaTime);
                yield return null;
            }
        }

        public void Dispose()
        {
            SteamNative.Shutdown();
        }
    }
}