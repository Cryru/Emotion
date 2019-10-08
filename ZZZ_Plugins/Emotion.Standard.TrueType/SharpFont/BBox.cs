#region Using

using System;
using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A structure used to hold an outline's bounding box, i.e., the
    /// coordinates of its extrema in the horizontal and vertical directions.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BBox : IEquatable<BBox>
    {
        #region Fields

        private FT_Long xMin, yMin;
        private FT_Long xMax, yMax;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="BBox" /> struct.
        /// </summary>
        /// <param name="left">The left bound.</param>
        /// <param name="bottom">The bottom bound.</param>
        /// <param name="right">The right bound.</param>
        /// <param name="top">The upper bound.</param>
        public BBox(int left, int bottom, int right, int top)
        {
            xMin = (IntPtr) left;
            yMin = (IntPtr) bottom;
            xMax = (IntPtr) right;
            yMax = (IntPtr) top;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the horizontal minimum (left-most).
        /// </summary>
        public int Left
        {
            get => (int) xMin;
        }

        /// <summary>
        /// Gets the vertical minimum (bottom-most).
        /// </summary>
        public int Bottom
        {
            get => (int) yMin;
        }

        /// <summary>
        /// Gets the horizontal maximum (right-most).
        /// </summary>
        public int Right
        {
            get => (int) xMax;
        }

        /// <summary>
        /// Gets the vertical maximum (top-most).
        /// </summary>
        public int Top
        {
            get => (int) yMax;
        }

        #endregion

        #region Operators

        /// <summary>
        /// Compares two instances of <see cref="BBox" /> for equality.
        /// </summary>
        /// <param name="left">A <see cref="BBox" />.</param>
        /// <param name="right">Another <see cref="BBox" />.</param>
        /// <returns>A value indicating equality.</returns>
        public static bool operator ==(BBox left, BBox right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two instances of <see cref="BBox" /> for inequality.
        /// </summary>
        /// <param name="left">A <see cref="BBox" />.</param>
        /// <param name="right">Another <see cref="BBox" />.</param>
        /// <returns>A value indicating inequality.</returns>
        public static bool operator !=(BBox left, BBox right)
        {
            return !left.Equals(right);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Compares this instance of <see cref="BBox" /> to another for equality.
        /// </summary>
        /// <param name="other">A <see cref="BBox" />.</param>
        /// <returns>A value indicating equality.</returns>
        public bool Equals(BBox other)
        {
            return
                xMin == other.xMin &&
                yMin == other.yMin &&
                xMax == other.xMax &&
                yMax == other.yMax;
        }

        /// <summary>
        /// Compares this instance of <see cref="BBox" /> to an object for equality.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>A value indicating equality.</returns>
        public override bool Equals(object obj)
        {
            if (obj is BBox)
                return Equals((BBox) obj);

            return false;
        }

        /// <summary>
        /// Gets a unique hash code for this instance.
        /// </summary>
        /// <returns>A hash code.</returns>
        public override int GetHashCode()
        {
            //TODO better hash algo
            return xMin.GetHashCode() ^ yMin.GetHashCode() ^ xMax.GetHashCode() ^ yMax.GetHashCode();
        }

        /// <summary>
        /// Gets a string that represents this instance.
        /// </summary>
        /// <returns>A string representation of this instance.</returns>
        public override string ToString()
        {
            return "Min: (" + (int) xMin + ", " + (int) yMin + "), Max: (" + (int) xMax + ", " + (int) yMax + ")";
        }

        #endregion
    }
}