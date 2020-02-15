#region Using

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

#endregion

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
// ReSharper disable IdentifierTypo
// ReSharper disable CommentTypo
namespace WinApi.ComBaseApi.COM
{
    /// <summary>
    /// from Propidl.h.
    /// http://msdn.microsoft.com/en-us/library/aa380072(VS.85).aspx
    /// contains a union so we have to do an explicit layout
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct PropVariant
    {
        /// <summary>
        /// Value type tag.
        /// </summary>
        [FieldOffset(0)] public short vt;

        /// <summary>
        /// Reserved1.
        /// </summary>
        [FieldOffset(2)] public short wReserved1;

        /// <summary>
        /// Reserved2.
        /// </summary>
        [FieldOffset(4)] public short wReserved2;

        /// <summary>
        /// Reserved3.
        /// </summary>
        [FieldOffset(6)] public short wReserved3;

        /// <summary>
        /// cVal.
        /// </summary>
        [FieldOffset(8)] public sbyte cVal;

        /// <summary>
        /// bVal.
        /// </summary>
        [FieldOffset(8)] public byte bVal;

        /// <summary>
        /// iVal.
        /// </summary>
        [FieldOffset(8)] public short iVal;

        /// <summary>
        /// uiVal.
        /// </summary>
        [FieldOffset(8)] public ushort uiVal;

        /// <summary>
        /// lVal.
        /// </summary>
        [FieldOffset(8)] public int lVal;

        /// <summary>
        /// ulVal.
        /// </summary>
        [FieldOffset(8)] public uint ulVal;

        /// <summary>
        /// intVal.
        /// </summary>
        [FieldOffset(8)] public int intVal;

        /// <summary>
        /// uintVal.
        /// </summary>
        [FieldOffset(8)] public uint uintVal;

        /// <summary>
        /// hVal.
        /// </summary>
        [FieldOffset(8)] public long hVal;

        /// <summary>
        /// uhVal.
        /// </summary>
        [FieldOffset(8)] public long uhVal;

        /// <summary>
        /// fltVal.
        /// </summary>
        [FieldOffset(8)] public float fltVal;

        /// <summary>
        /// dblVal.
        /// </summary>
        [FieldOffset(8)] public double dblVal;

        //VARIANT_BOOL boolVal;
        /// <summary>
        /// boolVal.
        /// </summary>
        [FieldOffset(8)] public short boolVal;

        /// <summary>
        /// scode.
        /// </summary>
        [FieldOffset(8)] public int scode;

        //CY cyVal;
        //[FieldOffset(8)] private DateTime date; - can cause issues with invalid value
        /// <summary>
        /// Date time.
        /// </summary>
        [FieldOffset(8)] public FILETIME filetime;

        //CLSID* puuid;
        //CLIPDATA* pclipdata;
        //BSTR bstrVal;
        //BSTRBLOB bstrblobVal;
        /// <summary>
        /// Binary large object.
        /// </summary>
        [FieldOffset(8)] public IntPtr blobVal;

        //LPSTR pszVal;
        /// <summary>
        /// Pointer value.
        /// </summary>
        [FieldOffset(8)] public IntPtr pointerValue; //LPWSTR 
        //IUnknown* punkVal;
        /*IDispatch* pdispVal;
        IStream* pStream;
        IStorage* pStorage;
        LPVERSIONEDSTREAM pVersionedStream;
        LPSAFEARRAY parray;
        CAC cac;
        CAUB caub;
        CAI cai;
        CAUI caui;
        CAL cal;
        CAUL caul;
        CAH cah;
        CAUH cauh;
        CAFLT caflt;
        CADBL cadbl;
        CABOOL cabool;
        CASCODE cascode;
        CACY cacy;
        CADATE cadate;
        CAFILETIME cafiletime;
        CACLSID cauuid;
        CACLIPDATA caclipdata;
        CABSTR cabstr;
        CABSTRBLOB cabstrblob;
        CALPSTR calpstr;
        CALPWSTR calpwstr;
        CAPROPVARIANT capropvar;
        CHAR* pcVal;
        UCHAR* pbVal;
        SHORT* piVal;
        USHORT* puiVal;
        LONG* plVal;
        ULONG* pulVal;
        INT* pintVal;
        UINT* puintVal;
        FLOAT* pfltVal;
        DOUBLE* pdblVal;
        VARIANT_BOOL* pboolVal;
        DECIMAL* pdecVal;
        SCODE* pscode;
        CY* pcyVal;
        DATE* pdate;
        BSTR* pbstrVal;
        IUnknown** ppunkVal;
        IDispatch** ppdispVal;
        LPSAFEARRAY* pparray;
        PROPVARIANT* pvarVal;
        */

        /// <summary>
        /// Property value
        /// </summary>
        public object Value
        {
            get
            {
                short ve = DataType;
                switch (ve)
                {
                    case 16: // VarEnum.VT_I1 8bit integer
                        return bVal;
                    case 22:
                    case 2: // VarEnum.VT_I2 16bit integer
                        return iVal;
                    case 3: // VarEnum.VT_I4 32bit integer
                        return lVal;
                    case 20: // VarEnum.VT_I8 64bit integer
                        return hVal;
                    case 19: // VarEnum.VT_UI4 32bit unsigned integer
                        return ulVal;
                    case 21: // VarEnum.VT_UI8 64bit unsigned integer
                        return uhVal;
                    case 31: // VarEnum.VT_LPWSTR Wide null terminated string
                        return Marshal.PtrToStringUni(pointerValue);
                    case 65: // VarEnum.VT_BLOB Length prefixed byte array
                    case 4096 | 17: // VarEnum.VT_VECTOR | VarEnum.VT_UI1 Array of unsigned bytes
                        return blobVal;
                    case 72: // VarEnum.VT_CLSID COM class
                        return Marshal.PtrToStructure<Guid>(pointerValue);
                    case 11: // VarEnum.VT_BOOL boolean
                        switch (boolVal)
                        {
                            case -1:
                                return true;
                            case 0:
                                return false;
                            default:
                                throw new NotSupportedException("PropVariant VT_BOOL must be either -1 or 0");
                        }
                    case 64: // VarEnum.VT_FILETIME WIN32 Filetime
                        return DateTime.FromFileTime(((long) filetime.dwHighDateTime << 32) + filetime.dwLowDateTime);
                }

                throw new NotImplementedException("PropVariant " + ve);
            }
        }

        /// <summary>
        /// Gets the type of data in this PropVariant
        /// </summary>
        public short DataType
        {
            get => vt;
        }
    }
}