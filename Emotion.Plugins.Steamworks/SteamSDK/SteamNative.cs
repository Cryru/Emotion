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
        #region Common Function Signatures

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool BoolReturning();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void VoidReturning();

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr PointerReturning();

        #endregion

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public delegate bool DelegateRestartAppIfNecessary(uint unOwnAppID);

        [NativeMethod("SteamAPI_RestartAppIfNecessary")]
        public static DelegateRestartAppIfNecessary RestartAppIfNecessary;

        [NativeMethod("SteamAPI_IsSteamRunning")]
        public static BoolReturning IsSteamRunning;

        [NativeMethod("SteamAPI_Init")] public static BoolReturning Init;

        [NativeMethod("SteamAPI_Shutdown")] public static VoidReturning Shutdown;

        [NativeMethod("SteamAPI_RunCallbacks")]
        public static VoidReturning RunCallbacks;

        [NativeMethod("SteamClient")]
        public static PointerReturning GetSteamClient;

        [NativeMethod("SteamAPI_GetHSteamPipe")]
        public static PointerReturning GetSteamPipe;      
        
        [NativeMethod("SteamAPI_GetHSteamUser")]
        public static PointerReturning GetSteamUser;


        #region Steam Stats

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr DelegateGetSteamUserStats(IntPtr steamClientThis, IntPtr steamUser, IntPtr steamPipe, string pchVersion);

        [NativeMethod("SteamAPI_ISteamClient_GetISteamUserStats")]
        public static DelegateGetSteamUserStats GetSteamUserStats;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate bool DelegateRequestStats(IntPtr steamUserStatsThis);

        [NativeMethod("SteamAPI_ISteamUserStats_RequestCurrentStats")]
        public static DelegateRequestStats RequestStats;

        #endregion

        #region Steam Utils

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr DelegateGetSteamUtils(IntPtr steamClientThis, IntPtr hSteamPipe, string pchVersion);

        [NativeMethod("SteamAPI_ISteamClient_GetISteamUtils")]
        public static DelegateGetSteamUtils GetSteamUtils;

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WarningMessageHook(int nSeverity, StringBuilder pchDebugText);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void DelegateSetWarningMessageHook(IntPtr steamUtilsThis, WarningMessageHook pFunction);

        [NativeMethod("SteamAPI_ISteamUtils_SetWarningMessageHook")]
        public static DelegateSetWarningMessageHook SetWarningMessageHook;

        #endregion
    }
}