#region Using

using System;

#endregion

namespace SharpFont.Cache
{
    /// <summary>
    /// A callback function provided by client applications. It is used by the cache manager to translate a given
    /// FTC_FaceID into a new valid <see cref="Face" /> object, on demand.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///     The third parameter ‘req_data’ is the same as the one passed by the client when
    ///     <see cref="Manager(Library, uint, uint, ulong, FaceRequester, IntPtr)" /> is called.
    ///     </para>
    ///     <para>
    ///     The face requester should not perform funny things on the returned face object, like creating a new
    ///     <see cref="FTSize" /> for it, or setting a transformation through <see cref="Face.SetTransform()" />!
    ///     </para>
    /// </remarks>
    /// <param name="faceId">The face ID to resolve.</param>
    /// <param name="library">A handle to a FreeType library object.</param>
    /// <param name="requestData">Application-provided request data (see note below).</param>
    /// <param name="aface">A new <see cref="Face" /> handle.</param>
    /// <returns>FreeType error code. 0 means success.</returns>
    public delegate Error FaceRequester(IntPtr faceId, IntPtr library, IntPtr requestData, out IntPtr aface);
}