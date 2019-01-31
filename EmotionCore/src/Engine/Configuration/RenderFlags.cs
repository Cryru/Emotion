// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Engine.Configuration
{
    /// <summary>
    /// Flags related to rendering.
    /// </summary>
    public class RenderFlags
    {
        /// <summary>
        /// An override for the shader version. Each shader compiled after setting this will override the default version with the
        /// string.
        /// On Mac the shader version is forced to "330 core" and if the "gl_arb_gpu_shader5" OpenGL extension is missing it is
        /// forced to "400".
        /// </summary>
        public string ShaderVersionOverride = "";

        /// <summary>
        /// How detailed drawn circles should be. Updates instantly. Default is 30.
        /// </summary>
        public int CircleDetail = 30;

        /// <summary>
        /// The maximum textures that can be mapped in one MapBuffer. If more than the allowed textures are mapped an exception is
        /// raised.
        /// </summary>
        public int TextureArrayLimit = 16;
    }
}