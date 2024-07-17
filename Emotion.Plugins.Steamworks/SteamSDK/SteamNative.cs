#region Using

using System;
using System.Runtime.InteropServices;
using System.Text;

#endregion

// ReSharper disable UnassignedField.Global
namespace Emotion.Plugins.Steamworks.SteamSDK;

public static class SteamNative
{
    public const string NativeLibraryName = "steamapi_64";

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_RestartAppIfNecessary")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool RestartAppIfNecessary(uint unOwnAppID);

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_IsSteamRunning")]
    public static extern bool IsSteamRunning();

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_Init")]
    public static extern bool Init();

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_Shutdown")]
    public static extern void Shutdown();

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_RunCallbacks")]
    public static extern void RunCallbacks();

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamClient")]
    public static extern IntPtr GetSteamClient();

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_GetHSteamPipe")]
    public static extern IntPtr GetSteamPipe();

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_GetHSteamUser")]
    public static extern IntPtr GetHSteamUser();

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamClient_GetISteamUser")]
    public static extern IntPtr GetISteamUser(IntPtr steamClientThis, IntPtr hSteamUser, IntPtr steamPipe, string pchVersion);

    #region Steam Stats

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamClient_GetISteamUserStats")]
    public static extern IntPtr GetSteamUserStats(IntPtr steamClientThis, IntPtr hSteamUser, IntPtr steamPipe, string pchVersion);

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamUserStats_RequestCurrentStats")]
    public static extern bool RequestStats(IntPtr steamUserStatsThis);

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamUserStats_SetAchievement")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool SetAchievement(IntPtr steamUserStatsThis, [MarshalAs(UnmanagedType.LPStr)] string pchName);

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamUserStats_StoreStats")]
    [return: MarshalAs(UnmanagedType.I1)]
    public static extern bool StoreStats(IntPtr steamUserStatsThis);

    #endregion

    #region Steam Utils

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamClient_GetISteamUtils")]
    public static extern IntPtr GetSteamUtils(IntPtr steamClientThis, IntPtr hSteamPipe, string pchVersion);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void WarningMessageHook(int nSeverity, StringBuilder pchDebugText);

    [DllImport(NativeLibraryName, CallingConvention = CallingConvention.Cdecl, EntryPoint = "SteamAPI_ISteamUtils_SetWarningMessageHook")]
    public static extern void SetWarningMessageHook(IntPtr steamUtilsThis, WarningMessageHook pFunction);

    #endregion

    #region Steam User

    [DllImport(NativeLibraryName, EntryPoint = "SteamAPI_ISteamUser_GetSteamID", CallingConvention = CallingConvention.Cdecl)]
    public static extern ulong GetSteamID(IntPtr iSteamUserThis);

    #endregion
}