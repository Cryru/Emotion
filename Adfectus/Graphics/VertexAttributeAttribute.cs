#region Using

using System;

#endregion

namespace Adfectus.Graphics
{
    public class VertexAttributeAttribute : Attribute
    {
        public int ComponentCount { get; set; } = 1;

        public bool Normalized { get; set; }

        public Type TypeOverride { get; set; }

        public VertexAttributeAttribute(int componentCount, bool normalized, Type typeOverride = null)
        {
            ComponentCount = componentCount;
            Normalized = normalized;
            TypeOverride = typeOverride;
        }
    }
}