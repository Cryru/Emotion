#region Using

using System;
using System.Runtime.InteropServices;
using SharpFont.TrueType.Internal;

#endregion

namespace SharpFont.TrueType
{
    /// <summary>
    /// A structure used to model an SFNT ‘name’ table entry.
    /// </summary>
    /// <remarks>
    /// Possible values for ‘platform_id’, ‘encoding_id’, ‘language_id’, and ‘name_id’ are given in the file
    /// ‘ttnameid.h’. For details please refer to the TrueType or OpenType specification.
    /// </remarks>
    /// <see cref="PlatformId" />
    /// <see cref="AppleEncodingId" />
    /// <see cref="MacEncodingId" />
    /// <see cref="MicrosoftEncodingId" />
    public class SfntName
    {
        #region Fields

        private SfntNameRec rec;

        #endregion

        #region Constructors

        internal SfntName(SfntNameRec rec)
        {
            this.rec = rec;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the platform ID for ‘string’.
        /// </summary>

        public PlatformId PlatformId
        {
            get => rec.platform_id;
        }

        /// <summary>
        /// Gets the encoding ID for ‘string’.
        /// </summary>

        public ushort EncodingId
        {
            get => rec.encoding_id;
        }

        /// <summary>
        /// Gets the language ID for ‘string’.
        /// </summary>

        public ushort LanguageId
        {
            get => rec.language_id;
        }

        /// <summary>
        /// Gets an identifier for ‘string’.
        /// </summary>

        public ushort NameId
        {
            get => rec.name_id;
        }

        /// <summary>
        /// This property returns <see cref="StringPtr" /> interpreted as UTF-16.
        /// </summary>
        public string String
        {
            get => Marshal.PtrToStringUni(rec.@string, (int) rec.string_len);
        }

        /// <summary>
        /// This property returns <see cref="StringPtr" /> interpreted as ANSI.
        /// </summary>
        public string StringAnsi
        {
            get => Marshal.PtrToStringAnsi(rec.@string, (int) rec.string_len);
        }

        /// <summary>
        ///     <para>
        ///     Gets the ‘name’ string. Note that its format differs depending on the (platform,encoding) pair. It can be a
        ///     Pascal String, a UTF-16 one, etc.
        ///     </para>
        ///     <para>
        ///     Generally speaking, the string is not zero-terminated. Please refer to the TrueType specification for
        ///     details.
        ///     </para>
        /// </summary>
        public IntPtr StringPtr
        {
            get => rec.@string;
        }

        #endregion
    }
}