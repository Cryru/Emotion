#region Using

using System;

#endregion

namespace Emotion.Common.Serialization
{
    /// <summary>
    /// By default only properties with a public get and set are serialized.
    /// This attribute cases the property to be serialized regardless.
    /// Note that the property should still have both a get and a set and be itself "public".
    /// </summary>
    public class SerializeNonPublicGetSetAttribute : Attribute
    {
    }
}