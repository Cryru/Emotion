using Adfectus.Primitives;

namespace Adfectus.Common.Configuration
{
    /// <summary>
    /// Flags related to rendering.
    /// </summary>
    public class RenderFlags
    {
        /// <summary>
        /// How detailed drawn circles should be. Updates instantly. Default is 30.
        /// </summary>
        public int CircleDetail { get; set; } = 30;

        /// <summary>
        /// The maximum textures that can be mapped in one MapBuffer. If more than the allowed textures are mapped an exception is
        /// raised.
        /// </summary>
        public int TextureArrayLimit { get; set; } = 16;

        /// <summary>
        /// The maximum number of renderable vertices by a single stream buffer. Readonly, is limited by indices.
        /// </summary>
        public uint MaxRenderable { get; internal set; } = ushort.MaxValue;

        /// <summary>
        /// Whether to explicitly set the vertex attribute locations before linking the shader.
        /// Used for GL 2.0 support.
        /// </summary>
        public bool SetVertexAttribLocations { get; set; }

        /// <summary>
        /// Whether the use an internal framebuffer when rendering before flushing to the window's framebuffer. This makes scaling better on different displays.
        /// True by default.
        /// </summary>
        public bool UseFramebuffer { get; set; } = true;

        /// <summary>
        /// Whether to use Vertex Array Objects. True by default. 
        /// </summary>
        public bool UseVao { get; set; } = true;

        /// <summary>
        /// Whether to generate texture mip maps and apply a swizzle mask. True by default, turned off for compatibility.
        /// When turned off the texture loading will assume RGBA instead of the standard FreeImage BGRA
        /// </summary>
        public bool TextureLoadStandard { get; set; } = true;

        /// <summary>
        /// The color to clear the window with.
        /// </summary>
        public Color ClearColor { get; set; } = Color.Black;

        /// <summary>
        /// Whether to scale at integer scales only. False by default.
        /// </summary>
        public bool IntegerScale { get; set; } = false;
    }
}