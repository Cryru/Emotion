#region Using

using System;
using System.Runtime.InteropServices;
using SharpFont.Internal;

#endregion

namespace SharpFont
{
    /// <summary>
    /// Describe a function used to destroy the ‘client’ data of any FreeType object. See the description of the
    /// <see cref="Generic" /> type for details of usage.
    /// </summary>
    /// <param name="object">
    /// The address of the FreeType object which is under finalization. Its client data is accessed through its
    /// ‘generic’ field.
    /// </param>
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void GenericFinalizer(IntPtr @object);

    /// <summary>
    ///     <para>
    ///     Client applications often need to associate their own data to a variety of FreeType core objects. For example,
    ///     a text layout API might want to associate a glyph cache to a given size object.
    ///     </para>
    ///     <para>
    ///     Most FreeType object contains a ‘generic’ field, of type <see cref="Generic" />, which usage is left to client
    ///     applications and font servers.
    ///     </para>
    ///     <para>
    ///     It can be used to store a pointer to client-specific data, as well as the address of a ‘finalizer’ function,
    ///     which will be called by FreeType when the object is destroyed (for example, the previous client example would
    ///     put the address of the glyph cache destructor in the ‘finalizer’ field).
    ///     </para>
    /// </summary>
    [Obsolete("Use the Tag property and Disposed event.")]
    public class Generic
    {
        #region Fields

        private GenericRec rec;

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="Generic" /> class.
        /// </summary>
        /// <param name="data">
        /// A typeless pointer to some client data. The data it cointains must stay fixed until finalizer is called.
        /// </param>
        /// <param name="finalizer">A delegate that gets called when the contained object gets finalized.</param>
        public Generic(IntPtr data, GenericFinalizer finalizer)
        {
            rec.data = data;
            //rec.finalizer = finalizer;
        }

        internal Generic(GenericRec genInternal)
        {
            rec = genInternal;
        }

        internal Generic(IntPtr reference)
        {
            rec = PInvokeHelper.PtrToStructure<GenericRec>(reference);
        }

        internal Generic(IntPtr reference, int offset)
            : this(new IntPtr(reference.ToInt64() + offset))
        {
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the size of a <see cref="Generic" />, in bytes.
        /// </summary>
        public static int SizeInBytes
        {
            get => Marshal.SizeOf(typeof(GenericRec));
        }

        /// <summary>
        /// Gets or sets a typeless pointer to any client-specified data. This field is completely ignored by the
        /// FreeType library.
        /// </summary>
        public IntPtr Data
        {
            get => rec.data;

            set => rec.data = value;
        }

        /// <summary>
        /// Gets or sets a pointer to a <see cref="GenericFinalizer" /> function, which will be called when the object
        /// is destroyed. If this field is set to NULL, no code will be called.
        /// </summary>
        /*public GenericFinalizer Finalizer
        {
            get
            {
                return rec.finalizer;
            }

            set
            {
                rec.finalizer = value;
            }
        }*/

        #endregion

        #region Methods

        //TODO make this private and build it into the setters if the reference isn't IntPtr.Zero.
        internal void WriteToUnmanagedMemory(IntPtr location)
        {
            Marshal.WriteIntPtr(location, rec.data);
            //Marshal.WriteIntPtr(location, IntPtr.Size, Marshal.GetFunctionPointerForDelegate(rec.finalizer));
            Marshal.WriteIntPtr(location, IntPtr.Size, rec.finalizer);
        }

        #endregion
    }
}