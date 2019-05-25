#region Using

using Adfectus.Primitives;

#endregion

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
        /// The color to clear the window with.
        /// </summary>
        public Color ClearColor { get; set; } = Color.Black;

        /// <summary>
        /// Whether to scale at integer scales only. False by default.
        /// </summary>
        public bool IntegerScale { get; set; } = false;
    }
}