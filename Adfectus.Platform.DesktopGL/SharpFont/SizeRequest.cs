#region Using

using System;
using System.Runtime.InteropServices;
using FT_Long = System.IntPtr;
using FT_ULong = System.UIntPtr;

#endregion

namespace SharpFont
{
    /// <summary>
    /// A structure used to model a size request.
    /// </summary>
    /// <remarks>
    /// If <see cref="Width" /> is zero, then the horizontal scaling value is set equal to the vertical scaling value,
    /// and vice versa.
    /// </remarks>
    [StructLayout(LayoutKind.Sequential)]
    public struct SizeRequest : IEquatable<SizeRequest>
    {
        #region Fields

        private FT_Long width;
        private FT_Long height;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the type of request. See <see cref="SizeRequestType" />.
        /// </summary>
        public SizeRequestType RequestType { get; set; }

        /// <summary>
        /// Gets or sets the desired width.
        /// </summary>
        public int Width
        {
            get => (int) width;

            set => width = (FT_Long) value;
        }

        /// <summary>
        /// Gets or sets the desired height.
        /// </summary>
        public int Height
        {
            get => (int) height;

            set => height = (FT_Long) value;
        }

        /// <summary>
        /// Gets or sets the horizontal resolution. If set to zero, <see cref="Width" /> is treated as a 26.6 fractional pixel
        /// value.
        /// </summary>

        public uint HorizontalResolution { get; set; }

        /// <summary>
        /// Gets or sets the horizontal resolution. If set to zero, <see cref="Height" /> is treated as a 26.6 fractional pixel
        /// value.
        /// </summary>

        public uint VerticalResolution { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Compares two <see cref="SizeRequest" />s for equality.
        /// </summary>
        /// <param name="left">A <see cref="SizeRequest" />.</param>
        /// <param name="right">Another <see cref="SizeRequest" />.</param>
        /// <returns>A value indicating equality.</returns>
        public static bool operator ==(SizeRequest left, SizeRequest right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Compares two <see cref="SizeRequest" />s for inequality.
        /// </summary>
        /// <param name="left">A <see cref="SizeRequest" />.</param>
        /// <param name="right">Another <see cref="SizeRequest" />.</param>
        /// <returns>A value indicating inequality.</returns>
        public static bool operator !=(SizeRequest left, SizeRequest right)
        {
            return !left.Equals(right);
        }

        /// <summary>
        /// Compares this instance of <see cref="SizeRequest" /> to another for equality.
        /// </summary>
        /// <param name="other">A <see cref="SizeRequest" />.</param>
        /// <returns>A value indicating equality.</returns>
        public bool Equals(SizeRequest other)
        {
            return RequestType == other.RequestType &&
                   width == other.width &&
                   height == other.height &&
                   HorizontalResolution == other.HorizontalResolution &&
                   VerticalResolution == other.VerticalResolution;
        }

        /// <summary>
        /// Compares this instance of <see cref="SizeRequest" /> to another object for equality.
        /// </summary>
        /// <param name="obj">An object.</param>
        /// <returns>A value indicating equality.</returns>
        public override bool Equals(object obj)
        {
            if (obj is SizeRequest)
                return Equals((SizeRequest) obj);
            return false;
        }

        /// <summary>
        /// Gets a unique hash code for this instance.
        /// </summary>
        /// <returns>A unique hash code.</returns>
        public override int GetHashCode()
        {
            return RequestType.GetHashCode() ^ width.GetHashCode() ^ height.GetHashCode() ^ HorizontalResolution.GetHashCode() ^ VerticalResolution.GetHashCode();
        }

        #endregion
    }
}