#region Using

using Khronos;

#endregion

namespace OpenGL
{
    public partial class Gl
    {
        /// <summary>
        /// Extension support listing.
        /// </summary>
        public partial class Extensions : ExtensionsCollection
        {
            /// <summary>
            /// Core support for primitive restart.
            /// </summary>
            [CoreExtension(3, 1)] [CoreExtension(3, 0, KhronosVersion.API_GLES2)]
            public bool PrimitiveRestart;

            /// <summary>
            /// Core support for instanced arrays
            /// </summary>
            [CoreExtension(3, 2)] [Extension("GL_ARB_instanced_arrays")]
            public bool InstancedArrays;
        }
    }
}