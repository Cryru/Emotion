#region Using

using System;
using System.Runtime.InteropServices;

#endregion

namespace FreeImageAPI
{
    /// <summary>
    /// This Structure contains ICC-Profile data.
    /// </summary>
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct FIICCPROFILE
    {
        /// <summary>
        /// Creates a new ICC-Profile for <paramref name="dib" />.
        /// </summary>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="data">The ICC-Profile data.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dib" /> is null.
        /// </exception>
        public FIICCPROFILE(FIBITMAP dib, byte[] data)
            : this(dib, data, data.Length)
        {
        }

        /// <summary>
        /// Creates a new ICC-Profile for <paramref name="dib" />.
        /// </summary>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="data">The ICC-Profile data.</param>
        /// <param name="size">Number of bytes to use from data.</param>
        /// <exception cref="ArgumentNullException">
        /// <paramref name="dib" /> is null.
        /// </exception>
        public unsafe FIICCPROFILE(FIBITMAP dib, byte[] data, int size)
        {
            if (dib.IsNull) throw new ArgumentNullException("dib");

            FIICCPROFILE prof;
            size = Math.Min(size, data.Length);
            prof = *(FIICCPROFILE*) FreeImage.CreateICCProfile(dib, data, size);
            Flags = prof.Flags;
            Size = prof.Size;
            DataPointer = prof.DataPointer;
        }

        /// <summary>
        /// Info flag of the profile.
        /// </summary>
        public ICC_FLAGS Flags { get; }

        /// <summary>
        /// Profile's size measured in bytes.
        /// </summary>
        public uint Size { get; }

        /// <summary>
        /// Points to a block of contiguous memory containing the profile.
        /// </summary>
        public IntPtr DataPointer { get; }

        /// <summary>
        /// Copy of the ICC-Profiles data.
        /// </summary>
        public unsafe byte[] Data
        {
            get
            {
                byte[] result;
                FreeImage.CopyMemory(result = new byte[Size], DataPointer.ToPointer(), Size);
                return result;
            }
        }

        /// <summary>
        /// Indicates whether the profile is CMYK.
        /// </summary>
        public bool IsCMYK
        {
            get => (Flags & ICC_FLAGS.FIICC_COLOR_IS_CMYK) != 0;
        }
    }
}