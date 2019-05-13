#region Using

using System;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A list of bit-field constants used within the ‘flags’ field of the <see cref="OpenArgs" /> structure.
    /// </summary>
    /// <remarks>
    /// The <see cref="OpenFlags.Memory" />, <see cref="OpenFlags.Stream" />, and <see cref="OpenFlags.PathName" /> flags
    /// are mutually exclusive.
    /// </remarks>
    [Flags]
    public enum OpenFlags
    {
        /// <summary>This is a memory-based stream.</summary>
        Memory = 0x01,

        /// <summary>Copy the stream from the ‘stream’ field.</summary>
        Stream = 0x02,

        /// <summary>Create a new input stream from a C path name.</summary>
        PathName = 0x04,

        /// <summary>Use the ‘driver’ field.</summary>
        Driver = 0x08,

        /// <summary>Use the ‘num_params’ and ‘params’ fields.</summary>
        Params = 0x10
    }
}