#region Using

using System;

#endregion

namespace Emotion.Standard.XML
{
    /// <summary>
    /// Marker for a field/property which should not be serialized.
    /// </summary>
    public class DontSerializeAttribute : Attribute
    {
    }
}