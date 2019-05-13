#region Using

using System;
using System.IO;
using System.Runtime.InteropServices;
using FreeImageAPI.IO;

#endregion

namespace FreeImageAPI
{
    // Delegates used by the FreeImageIO structure

    /// <summary>
    /// Delegate for capturing FreeImage error messages.
    /// </summary>
    /// <param name="fif">The format of the image.</param>
    /// <param name="message">The errormessage.</param>
    // DLL_API is missing in the definition of the callbackfuntion.
    [UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi, ThrowOnUnmappableChar = false)]
    public delegate void OutputMessageFunction(FREE_IMAGE_FORMAT fif, string message);
}

namespace FreeImageAPI.IO
{
    /// <summary>
    /// Delegate to the C++ function <b>fread</b>.
    /// </summary>
    /// <param name="buffer">Pointer to read from.</param>
    /// <param name="size">Item size in bytes.</param>
    /// <param name="count">Maximum number of items to be read.</param>
    /// <param name="handle">Handle/stream to read from.</param>
    /// <returns>
    /// Number of full items actually read,
    /// which may be less than count if an error occurs or
    /// if the end of the file is encountered before reaching count.
    /// </returns>
    public delegate uint ReadProc(IntPtr buffer, uint size, uint count, fi_handle handle);

    /// <summary>
    /// Delegate to the C++ function <b>fwrite</b>.
    /// </summary>
    /// <param name="buffer">Pointer to data to be written.</param>
    /// <param name="size">Item size in bytes.</param>
    /// <param name="count">Maximum number of items to be written.</param>
    /// <param name="handle">Handle/stream to write to.</param>
    /// <returns>
    /// Number of full items actually written,
    /// which may be less than count if an error occurs.
    /// Also, if an error occurs, the file-position indicator cannot be determined.
    /// </returns>
    public delegate uint WriteProc(IntPtr buffer, uint size, uint count, fi_handle handle);

    /// <summary>
    /// Delegate to the C++ function <b>fseek</b>.
    /// </summary>
    /// <param name="handle">Handle/stream to seek in.</param>
    /// <param name="offset">Number of bytes from origin.</param>
    /// <param name="origin">Initial position.</param>
    /// <returns>If successful 0 is returned; otherwise a nonzero value. </returns>
    public delegate int SeekProc(fi_handle handle, int offset, SeekOrigin origin);

    /// <summary>
    /// Delegate to the C++ function <b>ftell</b>.
    /// </summary>
    /// <param name="handle">Handle/stream to retrieve its currents position from.</param>
    /// <returns>The current position.</returns>
    public delegate int TellProc(fi_handle handle);

    // Delegates used by 'Plugin' structure
}

namespace FreeImageAPI.Plugins
{
    /// <summary>
    /// Delegate to a function that returns a string which describes
    /// the plugins format.
    /// </summary>
    public delegate string FormatProc();

    /// <summary>
    /// Delegate to a function that returns a string which contains
    /// a more detailed description.
    /// </summary>
    public delegate string DescriptionProc();

    /// <summary>
    /// Delegate to a function that returns a comma seperated list
    /// of file extensions the plugin can read or write.
    /// </summary>
    public delegate string ExtensionListProc();

    /// <summary>
    /// Delegate to a function that returns a regular expression that
    /// can be used to idientify whether a file can be handled by the plugin.
    /// </summary>
    public delegate string RegExprProc();

    /// <summary>
    /// Delegate to a function that opens a file.
    /// </summary>
    public delegate IntPtr OpenProc(ref FreeImageIO io, fi_handle handle, bool read);

    /// <summary>
    /// Delegate to a function that closes a previosly opened file.
    /// </summary>
    public delegate void CloseProc(ref FreeImageIO io, fi_handle handle, IntPtr data);

    /// <summary>
    /// Delegate to a function that returns the number of pages of a multipage
    /// bitmap if the plugin is capable of handling multipage bitmaps.
    /// </summary>
    public delegate int PageCountProc(ref FreeImageIO io, fi_handle handle, IntPtr data);

    /// <summary>
    /// UNKNOWN
    /// </summary>
    public delegate int PageCapabilityProc(ref FreeImageIO io, fi_handle handle, IntPtr data);

    /// <summary>
    /// Delegate to a function that loads and decodes a bitmap into memory.
    /// </summary>
    public delegate FIBITMAP LoadProc(ref FreeImageIO io, fi_handle handle, int page, int flags, IntPtr data);

    /// <summary>
    /// Delegate to a function that saves a bitmap.
    /// </summary>
    public delegate bool SaveProc(ref FreeImageIO io, FIBITMAP dib, fi_handle handle, int page, int flags, IntPtr data);

    /// <summary>
    /// Delegate to a function that determines whether the source defined
    /// by
    /// <param name="io" />
    /// and
    /// <param name="handle" />
    /// is a valid image.
    /// </summary>
    public delegate bool ValidateProc(ref FreeImageIO io, fi_handle handle);

    /// <summary>
    /// Delegate to a function that returns a string which contains
    /// the plugin's mime type.
    /// </summary>
    public delegate string MimeProc();

    /// <summary>
    /// Delegate to a function that returns whether the plugin can handle the
    /// specified color depth.
    /// </summary>
    public delegate bool SupportsExportBPPProc(int bpp);

    /// <summary>
    /// Delegate to a function that returns whether the plugin can handle the
    /// specified image type.
    /// </summary>
    public delegate bool SupportsExportTypeProc(FREE_IMAGE_TYPE type);

    /// <summary>
    /// Delegate to a function that returns whether the plugin can handle
    /// ICC-Profiles.
    /// </summary>
    public delegate bool SupportsICCProfilesProc();

    /// <summary>
    /// Callback function used by FreeImage to register plugins.
    /// </summary>
    public delegate void InitProc(ref Plugin plugin, int format_id);
}