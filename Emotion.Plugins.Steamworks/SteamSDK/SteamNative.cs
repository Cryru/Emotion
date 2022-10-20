#region Using

using System;
using System.Runtime.InteropServices;
using System.Text;
using Emotion.Plugins.Steamworks.Helpers;

#endregion

// ReSharper disable UnassignedField.Global
namespace Emotion.Plugins.Steamworks.SteamSDK
{
    public static class SteamNative
    {
        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_RestartAppIfNecessary")]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool RestartAppIfNecessary(uint unOwnAppID);

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_IsSteamRunning")]
        public static extern bool IsSteamRunning();

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_Init")]
        public static extern bool Init();

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_Shutdown")]
        public static extern void Shutdown();

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_RunCallbacks")]
        public static extern void RunCallbacks();

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamClient")]
        public static extern IntPtr GetSteamClient();

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_GetHSteamPipe")]
        public static extern IntPtr GetSteamPipe();

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_GetHSteamUser")]
        public static extern IntPtr GetSteamUser();

        #region Steam Stats

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamClient_GetISteamUserStats")]
        public static extern IntPtr GetSteamUserStats(IntPtr steamClientThis, IntPtr steamUser, IntPtr steamPipe, string pchVersion);

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamUserStats_RequestCurrentStats")]
        public static extern bool RequestStats(IntPtr steamUserStatsThis);

        #endregion

        #region Steam Utils

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamClient_GetISteamUtils")]
        public static extern IntPtr GetSteamUtils(IntPtr steamClientThis, IntPtr hSteamPipe, string pchVersion);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WarningMessageHook(int nSeverity, StringBuilder pchDebugText);

        [DllImport("steamapi_64", CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamUtils_SetWarningMessageHook")]
        public static extern void SetWarningMessageHook(IntPtr steamUtilsThis, WarningMessageHook pFunction);

        #endregion
    }
}