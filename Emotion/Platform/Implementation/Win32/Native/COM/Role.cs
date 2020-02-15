// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo

namespace WinApi.ComBaseApi.COM
{
    /// <summary>
    /// The ERole enumeration defines constants that indicate the role
    /// that the system has assigned to an audio endpoint device
    /// </summary>
    public enum Role
    {
        /// <summary>
        /// Games, system notification sounds, and voice commands.
        /// </summary>
        Console,

        /// <summary>
        /// Music, movies, narration, and live music recording
        /// </summary>
        Multimedia,

        /// <summary>
        /// Voice communications (talking to another person).
        /// </summary>
        Communications
    }
}