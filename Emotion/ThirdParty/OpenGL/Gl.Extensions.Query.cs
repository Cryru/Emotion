#nullable enable

using OpenGL.Khronos;

namespace OpenGL;

public partial class Gl
{
    /// <summary>
    /// Extension support listing.
    /// </summary>
    public partial class Extensions
    {
        /// <summary>
        /// Query the extensions supported by current platform.
        /// </summary>
        /// <remarks>
        /// An OpenGL context must be current on the calling thread.
        /// </remarks>
        public void Query()
        {
            string glVersionString = GetString(StringName.Version);
            if (glVersionString == null)
                throw new InvalidOperationException("unable to determine OpenGL version");

            KhronosVersion glVersion = KhronosVersion.Parse(glVersionString);
            bool indexedExtensions = glVersion.Major >= 3 && Delegates.pglGetStringi != null;

            if (indexedExtensions)
            {
                Get(GetPName.NumExtensions, out int extensionCount);

                var extensions = new List<string>();
                for (uint i = 0; i < (uint) extensionCount; i++)
                {
                    extensions.Add(GetString(StringName.Extensions, i));
                }

                Query(glVersion, extensions.ToArray());
            }
            else
            {
                Query(glVersion, GetString(StringName.Extensions));
            }
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        /// <returns>
        /// It returns a deep copy of this Extension.
        /// </returns>
        public Extensions Clone()
        {
            return (Extensions) MemberwiseClone();
        }
    }
}