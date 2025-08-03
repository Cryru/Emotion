#nullable enable

namespace Emotion.Core;

/// <summary>
/// Data about the engine.
/// Should be filled automatically by CI at some point? Idk
/// </summary>
public static class MetaData
{
    /// <summary>
    /// The engine version.
    /// </summary>
    public static string Version = "0.1.000+";

    /// <summary>
    /// The hash of the git commit, if built from one.
    /// </summary>
    public static string GitHash = "";

    /// <summary>
    /// Which build configuration this is.
    /// </summary>
#if DEBUG
    public static string BuildConfig = "DEBUG";
#elif RELEASE
    public static string BuildConfig = "RELEASE";
#else
    public static string BuildConfig = "UNKNOWN";
#endif
}