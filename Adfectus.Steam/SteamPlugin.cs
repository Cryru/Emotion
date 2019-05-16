#region Using

using System;
using System.IO;
using System.Runtime.InteropServices;
using Adfectus.Common;
using Adfectus.Logging;
using SteamworksSharp;

#endregion

namespace Adfectus.Steam
{
    public class SteamPlugin : Plugin
    {
        #region Properties

        /// <summary>
        /// How often to update Steam callbacks in milliseconds. Default is every 3 seconds.
        /// </summary>
        public float SteamUpdateTime = 3000;

        /// <summary>
        /// The AppId of the steam application.
        /// </summary>
        public static uint Appid { get; private set; }

        #endregion

        private delegate void SteamWarningHook(int severity, string text);
        private SteamWarningHook _steamWarningHook;
        private float _internalTimer;

        public SteamPlugin(uint addId)
        {
            Appid = addId;
        }

        public override void Initialize()
        {
            Engine.Log.Info("Loading Steam...", MessageSource.Other);

            bool debug = Meta.FullVersion.Contains("[DEBUG]");
            if (debug)
            {
                // If in debug mode, create app id file if missing.
                if (!File.Exists("steam_appid.txt")) File.WriteAllText("steam_appid.txt", Appid.ToString());
            }
            else
            {
                // If not in debug mode, delete app id file.
                if (File.Exists("steam_appid.txt")) File.Delete("steam_appid.txt");
            }

            SteamApi.RestartAppIfNecessary(Appid);

            bool steamOpen = SteamApi.IsSteamRunning();

            // If not started through Steam, close the app. Steam will start it.
            if (!steamOpen)
            {
                ErrorHandler.SubmitError(new Exception("Steam is not open."));
                Engine.Quit();
                return;
            }

            // Init the SteamWorks API.
            try
            {
                bool initSuccess = SteamApi.Initialize((int) Appid);
                if (!initSuccess) throw new Exception("Couldn't connect to steam.");
            }
            catch (Exception ex)
            {
                ErrorHandler.SubmitError(new Exception("Failed on SteamApi initialization.", ex));
                Engine.Quit();
                return;
            }

            // Request user stats.
            SteamApi.SteamUserStats.RequestCurrentStats();

            // Setup hooks.
            _steamWarningHook = WarningCallback;
            IntPtr func = Marshal.GetFunctionPointerForDelegate(_steamWarningHook);
            SteamApi.SteamUtils.SetWarningMessageHook(func);

            Engine.Log.Info("Steam loaded!", MessageSource.Other);
        }

        private void WarningCallback(int warning, string error)
        {
            Engine.Log.Warning($"Steam API: {warning} - {error}", MessageSource.Other);
        }

        public override void Update()
        {
            // Add frame time to timer.
            _internalTimer += (float) Engine.RawFrameTime;

            // If enough time has passed update callbacks.
            if (!(_internalTimer > SteamUpdateTime)) return;
            _internalTimer -= SteamUpdateTime;
            SteamApi.RunCallbacks();
        }

        public override void Dispose()
        {

        }

        #region User API

        /// <summary>
        /// Unlocks the achievement on Steam.
        /// </summary>
        /// <param name="achievementName">The name of the achievement.</param>
        public void UnlockAchievement(string achievementName)
        {
            SteamApi.SteamUserStats.SetAchievement(achievementName);
            SteamApi.SteamUserStats.StoreStats();
        }

        #endregion
    }
}