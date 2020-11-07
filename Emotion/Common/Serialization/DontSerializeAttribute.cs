#region Using

using System;

#endregion

namespace Emotion.Common.Serialization
{
    /// <summary>
    /// Marker for a field/property which should not be serialized.
    /// </summary>
    public class DontSerializeAttribute : Attribute
    {
    }
}