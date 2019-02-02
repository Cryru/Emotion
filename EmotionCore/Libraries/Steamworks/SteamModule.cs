// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.IO;
using System.Threading.Tasks;
using Emotion.Debug;
using Emotion.Engine;
using Steamworks;

#endregion

namespace Emotion.Libraries.Steamworks
{
    /// <summary>
    /// An Emotion plugin module for integrating with Steam.
    /// </summary>
    public sealed class SteamModule
    {
        #region Properties

        /// <summary>
        /// How often to update Steam callbacks in milliseconds. Default is every 3 seconds.
        /// </summary>
        public float SteamUpdateTime = 3000;

        #endregion

        /// <summary>
        /// Whether the module is setup.
        /// </summary>
        private bool _isSetup;

        /// <summary>
        /// Internal steam update timer.
        /// </summary>
        private float _internalTimer;

        /// <summary>
        /// Create a steam module. Only one can be active.
        /// </summary>
        internal SteamModule(uint appId)
        {
                Context.Log.Info("Loading Steam...", MessageSource.Other);

                // Check whether package is correct.
                bool correctPackage = Packsize.Test();
                if (!correctPackage) Context.Log.Warning("SteamWorks.Net package is incorrect. Use Mono dll on Linux and MacOS.", MessageSource.Other);

                // Check whether the native SteamWorks dll is correct.
                bool correctNative = DllCheck.Test();
                if (!correctNative) Context.Log.Warning("SteamWorks native DLL is not the correct version.", MessageSource.Other);

                if (Debugger.DebugMode)
                {
                    // If in debug mode, create app id file if missing.
                    if (!File.Exists("steam_appid.txt")) File.WriteAllText("steam_appid.txt", appId.ToString());
                }
                else
                {
                    // If not in debug mode, delete app id file.
                    if (File.Exists("steam_appid.txt")) File.Delete("steam_appid.txt");
                }

                AppId_t idStruct = new AppId_t(appId);
                bool needToRestart = SteamAPI.RestartAppIfNecessary(idStruct);

                // If not started through Steam, close the app. Steam will start it.
                if (needToRestart)
                {
                    Context.Log.Warning("Game not started through Steam. Restarting...", MessageSource.Other);
                    Context.Crash();
                    return;
                }

                // Init the SteamWorks API.
                try
                {
                    bool initSuccess = SteamAPI.Init();
                    if (!initSuccess) throw new Exception("Couldn't connect to Steam.");
                }
                catch (Exception ex)
                {
                    Context.Log.Error("Failed on SteamAPI init.", ex, MessageSource.Other);
                    Context.Crash();
                    return;
                }

                _isSetup = true;

                // Request user stats.
                SteamUserStats.RequestCurrentStats();

                // Setup hooks.
                SteamClient.SetWarningMessageHook((severity, warning) => { Context.Log.Warning($"Steam API: {warning}", MessageSource.Other); });

                Context.Log.Info("Steam loaded!", MessageSource.Other);
        }

        /// <summary>
        /// Updates the Steam callbacks. Is run every tick.
        /// </summary>
        internal void Update()
        {
            // Check if setup.
            if (!_isSetup) return;

            // Add frame time to timer.
            _internalTimer += Context.RawFrameTime;

            // If enough time has passed update callbacks.
            if (_internalTimer > SteamUpdateTime)
            {
                _internalTimer -= SteamUpdateTime;
                SteamAPI.RunCallbacks();
            }
        }

        /// <summary>
        /// Disposes of the module.
        /// </summary>
        internal void Dispose()
        {
            // Check if setup.
            if (!_isSetup) return;

            SteamAPI.Shutdown();
        }

        #region User API

        /// <summary>
        /// Unlocks the achievement on Steam.
        /// </summary>
        /// <param name="achievementName">The name of the achievement.</param>
        public void UnlockAchievement(string achievementName)
        {
            // Check if setup.
            if (!_isSetup) return;

            SteamUserStats.SetAchievement(achievementName);
            SteamUserStats.StoreStats();
        }

        #endregion
    }
}