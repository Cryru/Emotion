#region Using

#endregion

namespace Emotion.Graphics.Data
{
    public class VertexAttributeAttribute : Attribute
    {
        /// <summary>
        /// The number of data components the attribute has.
        /// </summary>
        public int ComponentCount { get; set; } = 1;

        /// <summary>
        /// Whether the value is normalized.
        /// </summary>
        public bool Normalized { get; set; }

        /// <summary>
        /// Force the attribute to be interpreted as this type.
        /// Otherwise the type of the field is taken.
        /// </summary>
        public Type TypeOverride { get; set; }

        public VertexAttributeAttribute(int componentCount, bool normalized, Type typeOverride = null)
        {
            ComponentCount = componentCount;
            Normalized = normalized;
            TypeOverride = typeOverride;
        }
    }
}