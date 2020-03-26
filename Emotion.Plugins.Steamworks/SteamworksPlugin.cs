#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Emotion.Common;
using Emotion.Game.Time;
using Emotion.Plugins.Steamworks.Helpers;
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
        /// A pointer to the loaded native steam library.
        /// </summary>
        public static IntPtr LibraryPointer { get; private set; }

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

            _callbackRunner = new Every(callbackFrequencyMs, () => { SteamNative.RunCallbacks(); });

            LoadNative();
        }

        public void Initialize()
        {
            LoadFunctions();

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
            if (!statsReceived)
            {
                Engine.Log.Warning("User is not logged in to Steam.", LOG_SOURCE);
            }

            SteamNative.RunCallbacks();
        }

        private static SteamNative.WarningMessageHook _warningHook = WarningCallback;

        private static void WarningCallback(int severity, StringBuilder msg)
        {
            Engine.Log.Warning(msg.ToString(), $"SteamSDK-{severity}");
        }

        public void Update()
        {
            _callbackRunner.Update(Engine.DeltaTime);
        }

        public void Dispose()
        {
            SteamNative.Shutdown();
        }

        #region Native Loader

        private static void LoadNative()
        {
            if (!Directory.Exists("steam")) throw new Exception("No \"steam\" directory found!");

            string folder = null;
            string libName = null;

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                folder = Environment.Is64BitProcess ? $"{Environment.CurrentDirectory}\\steam\\win64" : $"{Environment.CurrentDirectory}\\steam";
                libName =  Environment.Is64BitProcess ? "steam_api64.dll" : "steam_api.dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                folder = Environment.Is64BitProcess ? $"{Environment.CurrentDirectory}/steam/osx" : null;
                libName = "libsteam_api.dylib";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                folder = Environment.Is64BitProcess ? $"{Environment.CurrentDirectory}/steam/linux64" : $"{Environment.CurrentDirectory}/steam/linux32";
                libName = "libsteam_api.so";
            }

            string fullPath = Path.Join(folder, libName);
            if (string.IsNullOrEmpty(folder) || !File.Exists(fullPath)) throw new Exception($"Couldn't find Steam library in {fullPath}.");

            bool loaded = NativeLibrary.TryLoad(fullPath, out IntPtr libPtr);
            if (!loaded) throw new Exception("Couldn't load Steam library.");

            LibraryPointer = libPtr;
        }

        public static void LoadFunctions()
        {
            IEnumerable<FieldInfo> methods = typeof(SteamNative).GetTypeInfo().DeclaredFields;
            foreach (FieldInfo method in methods)
            {
                var nativeMethodAttribute = (NativeMethodAttribute) method.GetCustomAttribute(typeof(NativeMethodAttribute));
                if (nativeMethodAttribute == null) continue;

                bool success = NativeLibrary.TryGetExport(LibraryPointer, nativeMethodAttribute.Name, out IntPtr funcAddress);
                if (!success)
                {
                    Engine.Log.Warning($"Couldn't load function - {nativeMethodAttribute.Name}", LOG_SOURCE);
                    continue;
                }

                Delegate val = Marshal.GetDelegateForFunctionPointer(funcAddress, method.FieldType);
                method.SetValue(null, val);
            }
        }

        #endregion
    }
}