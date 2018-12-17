// Emotion - https://github.com/Cryru/Emotion

namespace Emotion.Engine.Configuration
{
    /// <summary>
    /// Flags related to rendering.
    /// </summary>
    public class RenderFlags
    {
        /// <summary>
        /// The major version of the OpenGL context.
        /// Recommended is 3 and is that by default.
        /// </summary>
        public int OpenGLMajorVersion { get; private set; } = 3;

        /// <summary>
        /// The minor version of the OpenGL context. Some tools like RenderDoc won't work on versions under 3.3.
        /// 3.2 is required for the "gl_arb_gpu_shader5" extension.
        /// For some reason the Linux I test on would prefer 3.0 though.
        /// </summary>
        public int OpenGLMinorVersion { get; set; } = 3;

        /// <summary>
        /// Whether the "gl_arb_gpu_shader5" OpenGL extension is missing. In which case the shaders must be patched.
        /// Will be forcefully switched by the Renderer when checking extensions and loading shaders.
        /// </summary>
        public bool Shader5ExtensionMissing { get; set; }

        /// <summary>
        /// How detailed drawn circles should be. Updates instantly. Default is 30.
        /// </summary>
        public int CircleDetail = 30;
    }
}