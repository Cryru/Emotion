// SoulEngine - https://github.com/Cryru/SoulEngine

namespace Soul.Engine.Enums
{
    /// <summary>
    /// The text style to use when rendering.
    /// </summary>
    public enum TextStyle
    {
        /// <summary>
        /// Default. The text is aligned to the left of the box.
        /// </summary>
        Left,

        /// <summary>
        /// Each line is centered.
        /// </summary>
        Center,

        /// <summary>
        /// The text is aligned to the right of the box.
        /// </summary>
        Right,

        /// <summary>
        /// Each line of text is stretched to be somewhat the same width creating a box effect.
        /// </summary>
        Justified,

        /// <summary>
        /// Each line of text is stretched to be somewhat the same width creating a box effect, and the text is centered inside the
        /// box.
        /// </summary>
        JustifiedCenter
    }
}