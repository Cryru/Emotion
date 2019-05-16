#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace FreeImageAPI
{
    /// <summary>
    /// The <b>FICOMPLEX</b> structure describes a color consisting of a real and an imaginary part.
    /// Each part is using 4 bytes of data.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct FICOMPLEX : IComparable, IComparable<FICOMPLEX>, IEquatable<FICOMPLEX>
    {
        /// <summary>
        /// Real part of the color.
        /// </summary>
        public double real;

        /// <summary>
        /// Imaginary part of the color.
        /// </summary>
        public double imag;

        /// <summary>
        /// Tests whether two specified <see cref="FICOMPLEX" /> structures are equivalent.
        /// </summary>
        /// <param name="left">The <see cref="FICOMPLEX" /> that is to the left of the equality operator.</param>
        /// <param name="right">The <see cref="FICOMPLEX" /> that is to the right of the equality operator.</param>
        /// <returns>
        /// <b>true</b> if the two <see cref="FICOMPLEX" /> structures are equal; otherwise, <b>false</b>.
        /// </returns>
        public static bool operator ==(FICOMPLEX left, FICOMPLEX right)
        {
            return left.real == right.real && left.imag == right.imag;
        }

        /// <summary>
        /// Tests whether two specified <see cref="FICOMPLEX" /> structures are different.
        /// </summary>
        /// <param name="left">The <see cref="FICOMPLEX" /> that is to the left of the inequality operator.</param>
        /// <param name="right">The <see cref="FICOMPLEX" /> that is to the right of the inequality operator.</param>
        /// <returns>
        /// <b>true</b> if the two <see cref="FICOMPLEX" /> structures are different; otherwise, <b>false</b>.
        /// </returns>
        public static bool operator !=(FICOMPLEX left, FICOMPLEX right)
        {
            return left.real != right.real || left.imag == right.imag;
        }

        /// <summary>
        /// Compares this instance with a specified <see cref="Object" />.
        /// </summary>
        /// <param name="obj">An object to compare with this instance.</param>
        /// <returns>A 32-bit signed integer indicating the lexical relationship between the two comparands.</returns>
        /// <exception cref="ArgumentException"><paramref name="obj" /> is not a <see cref="FICOMPLEX" />.</exception>
        public int CompareTo(object obj)
        {
            if (obj == null) return 1;

            if (!(obj is FICOMPLEX)) throw new ArgumentException("obj");

            return CompareTo((FICOMPLEX) obj);
        }

        /// <summary>
        /// Compares this instance with a specified <see cref="FICOMPLEX" /> object.
        /// </summary>
        /// <param name="other">A <see cref="FICOMPLEX" /> to compare.</param>
        /// <returns>
        /// A signed number indicating the relative values of this instance
        /// and <paramref name="other" />.
        /// </returns>
        public int CompareTo(FICOMPLEX other)
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Tests whether the specified object is a <see cref="FICOMPLEX" /> structure
        /// and is equivalent to this <see cref="FICOMPLEX" /> structure.
        /// </summary>
        /// <param name="obj">The object to test.</param>
        /// <returns>
        /// <b>true</b> if <paramref name="obj" /> is a <see cref="FICOMPLEX" /> structure
        /// equivalent to this <see cref="FICOMPLEX" /> structure; otherwise, <b>false</b>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return obj is FICOMPLEX && this == (FICOMPLEX) obj;
        }

        /// <summary>
        /// Tests whether the specified <see cref="FICOMPLEX" /> structure is equivalent to this <see cref="FICOMPLEX" />
        /// structure.
        /// </summary>
        /// <param name="other">A <see cref="FICOMPLEX" /> structure to compare to this instance.</param>
        /// <returns>
        /// <b>true</b> if <paramref name="obj" /> is a <see cref="FICOMPLEX" /> structure
        /// equivalent to this <see cref="FICOMPLEX" /> structure; otherwise, <b>false</b>.
        /// </returns>
        public bool Equals(FICOMPLEX other)
        {
            return this == other;
        }

        /// <summary>
        /// Returns a hash code for this <see cref="FICOMPLEX" /> structure.
        /// </summary>
        /// <returns>An integer value that specifies the hash code for this <see cref="FICOMPLEX" />.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}