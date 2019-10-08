#region Using

using System;

#endregion

// ReSharper disable InheritdocConsiderUsage

namespace OpenGL
{
    /// <summary>
    /// Attribute indicating whether a field shall indicate an OpenGL implementation limit.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    internal class LimitAttribute : Attribute
    {
        /// <summary>
        /// Construct a LimitAttribute.
        /// </summary>
        /// <param name="enum">
        /// A <see cref="Int32" /> that specify the OpenGL enumeration value to used with Gl.Get and Gl.GetString routines.
        /// </param>
        public LimitAttribute(int @enum)
        {
            EnumValue = @enum;
        }

        /// <summary>
        /// The enumeration value representing the limit.
        /// </summary>
        public readonly int EnumValue;

        /// <summary>
        /// In the case the limit is composed by an array, this property specify the array length.
        /// </summary>
        public uint ArrayLength { get; set; }
    }
}