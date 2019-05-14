#region Using

using System;
using System.Runtime.InteropServices;
using SharpFont.Bdf.Internal;

#endregion

namespace SharpFont.Bdf
{
    /// <summary>
    /// This structure models a given BDF/PCF property.
    /// </summary>
    public class Property
    {
        #region Fields

        private IntPtr reference;
        private PropertyRec rec;

        #endregion

        #region Constructors

        internal Property(IntPtr reference)
        {
            Reference = reference;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the property type.
        /// </summary>
        public PropertyType Type
        {
            get => rec.type;
        }

        /// <summary>
        /// Gets the atom string, if type is <see cref="PropertyType.Atom" />.
        /// </summary>
        public string Atom
        {
            get
            {
                // only this property throws an exception because the pointer could be to unmanaged memory not owned by
                // the process.
                if (rec.type != PropertyType.Atom)
                    throw new InvalidOperationException("The property type is not Atom.");

                return Marshal.PtrToStringAnsi(rec.atom);
            }
        }

        /// <summary>
        /// Gets a signed integer, if type is <see cref="PropertyType.Integer" />.
        /// </summary>
        public int Integer
        {
            get => rec.integer;
        }

        /// <summary>
        /// Gets an unsigned integer, if type is <see cref="PropertyType.Cardinal" />.
        /// </summary>

        public uint Cardinal
        {
            get => rec.cardinal;
        }

        internal IntPtr Reference
        {
            get => reference;

            set
            {
                reference = value;
                rec = PInvokeHelper.PtrToStructure<PropertyRec>(reference);
            }
        }

        #endregion
    }
}