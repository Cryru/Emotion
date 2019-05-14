#region Using

using System;
using System.IO;
using System.Runtime.InteropServices;
using FreeImageAPI.IO;
using FreeImageAPI.Plugins;

#endregion

namespace FreeImageAPI
{
    public static partial class FreeImage
    {
        public const int FI_RGBA_RED = 2;
        public const int FI_RGBA_GREEN = 1;
        public const int FI_RGBA_BLUE = 0;
        public const int FI_RGBA_ALPHA = 3;
        public const uint FI_RGBA_RED_MASK = 0x00FF0000;
        public const uint FI_RGBA_GREEN_MASK = 0x0000FF00;
        public const uint FI_RGBA_BLUE_MASK = 0x000000FF;
        public const uint FI_RGBA_ALPHA_MASK = 0xFF000000;
        public const int FI_RGBA_RED_SHIFT = 16;
        public const int FI_RGBA_GREEN_SHIFT = 8;
        public const int FI_RGBA_BLUE_SHIFT = 0;
        public const int FI_RGBA_ALPHA_SHIFT = 24;
        public const uint FI_RGBA_RGB_MASK = FI_RGBA_RED_MASK | FI_RGBA_GREEN_MASK | FI_RGBA_BLUE_MASK;
        public const int FI16_555_RED_MASK = 0x7C00;
        public const int FI16_555_GREEN_MASK = 0x03E0;
        public const int FI16_555_BLUE_MASK = 0x001F;
        public const int FI16_555_RED_SHIFT = 10;
        public const int FI16_555_GREEN_SHIFT = 5;
        public const int FI16_555_BLUE_SHIFT = 0;
        public const int FI16_565_RED_MASK = 0xF800;
        public const int FI16_565_GREEN_MASK = 0x07E0;
        public const int FI16_565_BLUE_MASK = 0x001F;
        public const int FI16_565_RED_SHIFT = 11;
        public const int FI16_565_GREEN_SHIFT = 5;
        public const int FI16_565_BLUE_SHIFT = 0;

        #region Internal Delegates

        /// Initialises the library.
        /// <param name="load_local_plugins_only">
        /// When the <paramref name="load_local_plugins_only" /> is true, FreeImage won't make use of external plugins.
        /// </param>
        public delegate void InitialiseInternal(bool load_local_plugins_only);

        public delegate void DeInitialiseInternal();

        private unsafe delegate byte* GetVersion_Internal();

        private unsafe delegate byte* GetCopyrightMessage_Internal();

        /// <param name="fif">Format of the bitmaps.</param>
        /// <param name="message">The error message.</param>
        public delegate void OutputMessageProcInternal(FREE_IMAGE_FORMAT fif, string message);

        /// <remarks>
        /// The function is private because FreeImage can only have a single
        /// callback function. To use the callback use the <see cref="FreeImageEngine.Message" />
        /// event of this class.
        /// </remarks>
        /// <param name="omf">Handler to the callback function.</param>
        public delegate void SetOutputMessageInternal(OutputMessageFunction omf);

        public delegate FIBITMAP AllocateInternal(int width, int height, int bpp,
            uint red_mask, uint green_mask, uint blue_mask);

        public delegate FIBITMAP AllocateTInternal(FREE_IMAGE_TYPE type, int width, int height, int bpp,
            uint red_mask, uint green_mask, uint blue_mask);

        public delegate FIBITMAP AllocateExInternal(int width, int height, int bpp,
            IntPtr color, FREE_IMAGE_COLOR_OPTIONS options, RGBQUAD[] palette,
            uint red_mask, uint green_mask, uint blue_mask);

        public delegate FIBITMAP AllocateExTInternal(FREE_IMAGE_TYPE type, int width, int height, int bpp,
            IntPtr color, FREE_IMAGE_COLOR_OPTIONS options, RGBQUAD[] palette,
            uint red_mask, uint green_mask, uint blue_mask);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP CloneInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        public delegate void UnloadInternal(FIBITMAP dib);

        /// <param name="fif">Type of the bitmap.</param>
        /// <param name="filename">Name of the file to decode.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP LoadAInternal(FREE_IMAGE_FORMAT fif, string filename, FREE_IMAGE_LOAD_FLAGS flags);

        /// <param name="fif">Type of the bitmap.</param>
        /// <param name="filename">Name of the file to decode.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP LoadUInternal(FREE_IMAGE_FORMAT fif, string filename, FREE_IMAGE_LOAD_FLAGS flags);

        /// <param name="fif">Type of the bitmap.</param>
        /// <param name="io">A FreeImageIO structure with functionpointers to handle the source.</param>
        /// <param name="handle">A handle to the source.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP LoadFromHandleInternal(FREE_IMAGE_FORMAT fif, ref FreeImageIO io, fi_handle handle, FREE_IMAGE_LOAD_FLAGS flags);

        /// <param name="fif">Type of the bitmap.</param>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="filename">Name of the file to save to.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SaveAInternal(FREE_IMAGE_FORMAT fif, FIBITMAP dib, string filename, FREE_IMAGE_SAVE_FLAGS flags);

        /// <param name="fif">Type of the bitmap.</param>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="filename">Name of the file to save to.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SaveUInternal(FREE_IMAGE_FORMAT fif, FIBITMAP dib, string filename, FREE_IMAGE_SAVE_FLAGS flags);

        public delegate bool SaveToHandleInternal(FREE_IMAGE_FORMAT fif, FIBITMAP dib, ref FreeImageIO io, fi_handle handle,
            FREE_IMAGE_SAVE_FLAGS flags);

        /// <param name="data">Pointer to the data in memory.</param>
        /// <param name="size_in_bytes">Length of the data in byte.</param>
        /// <returns>Handle to a memory stream.</returns>
        public delegate FIMEMORY OpenMemoryInternal(IntPtr data, uint size_in_bytes);

        public delegate FIMEMORY OpenMemoryExInternal(byte[] data, uint size_in_bytes);

        /// <param name="stream">Handle to a memory stream.</param>
        public delegate void CloseMemoryInternal(FIMEMORY stream);

        /// <param name="fif">Type of the bitmap.</param>
        /// <param name="stream">Handle to a memory stream.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP LoadFromMemoryInternal(FREE_IMAGE_FORMAT fif, FIMEMORY stream, FREE_IMAGE_LOAD_FLAGS flags);

        /// <param name="fif">Type of the bitmap.</param>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="stream">Handle to a memory stream.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SaveToMemoryInternal(FREE_IMAGE_FORMAT fif, FIBITMAP dib, FIMEMORY stream, FREE_IMAGE_SAVE_FLAGS flags);

        /// <param name="stream">Handle to a memory stream.</param>
        /// <returns>The current file position if successful, -1 otherwise.</returns>
        public delegate int TellMemoryInternal(FIMEMORY stream);

        /// <param name="stream">Handle to a memory stream.</param>
        /// <param name="offset">Number of bytes from origin.</param>
        /// <param name="origin">Initial position.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SeekMemoryInternal(FIMEMORY stream, int offset, SeekOrigin origin);

        /// <param name="stream">The target memory stream.</param>
        /// <param name="data">Pointer to the data in memory.</param>
        /// <param name="size_in_bytes">Size of the data in bytes.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool AcquireMemoryInternal(FIMEMORY stream, ref IntPtr data, ref uint size_in_bytes);

        /// <param name="buffer">The buffer to store the data in.</param>
        /// <param name="size">Size in bytes of the items.</param>
        /// <param name="count">Number of items to read.</param>
        /// <param name="stream">
        /// The stream to read from.
        /// The memory pointer associated with stream is increased by the number of bytes actually read.
        /// </param>
        /// <returns>
        /// The number of full items actually read.
        /// May be less than count on error or stream-end.
        /// </returns>
        public delegate uint ReadMemoryInternal(byte[] buffer, uint size, uint count, FIMEMORY stream);

        /// <param name="buffer">The buffer to read the data from.</param>
        /// <param name="size">Size in bytes of the items.</param>
        /// <param name="count">Number of items to write.</param>
        /// <param name="stream">
        /// The stream to write to.
        /// The memory pointer associated with stream is increased by the number of bytes actually written.
        /// </param>
        /// <returns>
        /// The number of full items actually written.
        /// May be less than count on error or stream-end.
        /// </returns>
        public delegate uint WriteMemoryInternal(byte[] buffer, uint size, uint count, FIMEMORY stream);

        /// <param name="fif">Type of the bitmap.</param>
        /// <param name="stream">The stream to decode.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Handle to a FreeImage multi-paged bitmap.</returns>
        public delegate FIMULTIBITMAP LoadMultiBitmapFromMemoryInternal(FREE_IMAGE_FORMAT fif, FIMEMORY stream, FREE_IMAGE_LOAD_FLAGS flags);

        public delegate FREE_IMAGE_FORMAT RegisterLocalPluginInternal(InitProc proc_address,
            string format, string description, string extension, string regexpr);

        public delegate FREE_IMAGE_FORMAT RegisterExternalPluginInternal(string path,
            string format, string description, string extension, string regexpr);

        /// <returns>The number of registered formats.</returns>
        public delegate int GetFIFCountInternal();

        /// <param name="fif">The plugin to enable or disable.</param>
        /// <param name="enable">True: enable the plugin. false: disable the plugin.</param>
        /// <returns>
        /// The previous state of the plugin.
        /// 1 - enabled. 0 - disables. -1 plugin does not exist.
        /// </returns>
        public delegate int SetPluginEnabledInternal(FREE_IMAGE_FORMAT fif, bool enable);

        /// <param name="fif">The plugin to check.</param>
        /// <returns>1 - enabled. 0 - disables. -1 plugin does not exist.</returns>
        public delegate int IsPluginEnabledInternal(FREE_IMAGE_FORMAT fif);

        /// <param name="format">The string that was used to register the plugin.</param>
        /// <returns>A <see cref="FREE_IMAGE_FORMAT" /> identifier from the format.</returns>
        public delegate FREE_IMAGE_FORMAT GetFIFFromFormatInternal(string format);

        /// <param name="mime">A MIME content type.</param>
        /// <returns>A <see cref="FREE_IMAGE_FORMAT" /> identifier from the MIME.</returns>
        public delegate FREE_IMAGE_FORMAT GetFIFFromMimeInternal(string mime);

        private unsafe delegate byte* GetFormatFromFIF_Internal(FREE_IMAGE_FORMAT fif);

        private unsafe delegate byte* GetFIFExtensionList_Internal(FREE_IMAGE_FORMAT fif);

        private unsafe delegate byte* GetFIFDescription_Internal(FREE_IMAGE_FORMAT fif);

        private unsafe delegate byte* GetFIFRegExpr_Internal(FREE_IMAGE_FORMAT fif);

        private unsafe delegate byte* GetFIFMimeType_Internal(FREE_IMAGE_FORMAT fif);

        /// <param name="filename">The filename or -extension.</param>
        /// <returns>The <see cref="FREE_IMAGE_FORMAT" /> of the plugin.</returns>
        public delegate FREE_IMAGE_FORMAT GetFIFFromFilenameAInternal(string filename);

        /// <param name="filename">The filename or -extension.</param>
        /// <returns>The <see cref="FREE_IMAGE_FORMAT" /> of the plugin.</returns>
        public delegate FREE_IMAGE_FORMAT GetFIFFromFilenameUInternal(string filename);

        /// <param name="fif">The <see cref="FREE_IMAGE_FORMAT" /> of the plugin.</param>
        /// <returns>True if the plugin can load bitmaps, else false.</returns>
        public delegate bool FIFSupportsReadingInternal(FREE_IMAGE_FORMAT fif);

        /// <param name="fif">The <see cref="FREE_IMAGE_FORMAT" /> of the plugin.</param>
        /// <returns>True if the plugin can save bitmaps, else false.</returns>
        public delegate bool FIFSupportsWritingInternal(FREE_IMAGE_FORMAT fif);

        /// <param name="fif">The <see cref="FREE_IMAGE_FORMAT" /> of the plugin.</param>
        /// <param name="bpp">The desired bit depth.</param>
        /// <returns>True if the plugin can save bitmaps in the desired bit depth, else false.</returns>
        public delegate bool FIFSupportsExportBPPInternal(FREE_IMAGE_FORMAT fif, int bpp);

        /// <param name="fif">The <see cref="FREE_IMAGE_FORMAT" /> of the plugin.</param>
        /// <param name="type">The desired image type.</param>
        /// <returns>True if the plugin can save bitmaps as the desired type, else false.</returns>
        public delegate bool FIFSupportsExportTypeInternal(FREE_IMAGE_FORMAT fif, FREE_IMAGE_TYPE type);

        /// <param name="fif">The <see cref="FREE_IMAGE_FORMAT" /> of the plugin.</param>
        /// <returns>True if the plugin can load or save an ICC profile, else false.</returns>
        public delegate bool FIFSupportsICCProfilesInternal(FREE_IMAGE_FORMAT fif);

        public delegate FIMULTIBITMAP OpenMultiBitmapInternal(FREE_IMAGE_FORMAT fif, string filename, bool create_new,
            bool read_only, bool keep_cache_in_memory, FREE_IMAGE_LOAD_FLAGS flags);

        public delegate FIMULTIBITMAP OpenMultiBitmapFromHandleInternal(FREE_IMAGE_FORMAT fif, ref FreeImageIO io,
            fi_handle handle, FREE_IMAGE_LOAD_FLAGS flags);

        /// <param name="bitmap">Handle to a FreeImage multi-paged bitmap.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        private delegate bool CloseMultiBitmap_Internal(FIMULTIBITMAP bitmap, FREE_IMAGE_SAVE_FLAGS flags);

        /// <param name="bitmap">Handle to a FreeImage multi-paged bitmap.</param>
        /// <returns>Number of pages.</returns>
        public delegate int GetPageCountInternal(FIMULTIBITMAP bitmap);

        /// <param name="bitmap">Handle to a FreeImage multi-paged bitmap.</param>
        /// <param name="data">Handle to a FreeImage bitmap.</param>
        public delegate void AppendPageInternal(FIMULTIBITMAP bitmap, FIBITMAP data);

        /// <param name="bitmap">Handle to a FreeImage multi-paged bitmap.</param>
        /// <param name="page">Page has to be a number smaller than the current number of pages available in the bitmap.</param>
        /// <param name="data">Handle to a FreeImage bitmap.</param>
        public delegate void InsertPageInternal(FIMULTIBITMAP bitmap, int page, FIBITMAP data);

        /// <param name="bitmap">Handle to a FreeImage multi-paged bitmap.</param>
        /// <param name="page">Number of the page to delete.</param>
        public delegate void DeletePageInternal(FIMULTIBITMAP bitmap, int page);

        /// <param name="bitmap">Handle to a FreeImage multi-paged bitmap.</param>
        /// <param name="page">Number of the page to lock.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP LockPageInternal(FIMULTIBITMAP bitmap, int page);

        /// <param name="bitmap">Handle to a FreeImage multi-paged bitmap.</param>
        /// <param name="data">Handle to a FreeImage bitmap.</param>
        /// <param name="changed">If true, the page is applied to the multi-page bitmap.</param>
        public delegate void UnlockPageInternal(FIMULTIBITMAP bitmap, FIBITMAP data, bool changed);

        /// <param name="bitmap">Handle to a FreeImage multi-paged bitmap.</param>
        /// <param name="target">New position of the page.</param>
        /// <param name="source">Old position of the page.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool MovePageInternal(FIMULTIBITMAP bitmap, int target, int source);

        /// <example>
        ///     <code>
        /// int[] lockedPages = null;
        /// int count = 0;
        /// GetLockedPageNumbers(dib, lockedPages, ref count);
        /// lockedPages = new int[count];
        /// GetLockedPageNumbers(dib, lockedPages, ref count);
        /// </code>
        /// </example>
        /// <param name="bitmap">Handle to a FreeImage multi-paged bitmap.</param>
        /// <param name="pages">
        /// The list of locked pages in the multi-pages bitmap.
        /// If set to null, count will contain the number of pages.
        /// </param>
        /// <param name="count">If <paramref name="pages" /> is set to null count will contain the number of locked pages.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool GetLockedPageNumbersInternal(FIMULTIBITMAP bitmap, int[] pages, ref int count);

        /// <param name="filename">Name of the file to analyze.</param>
        /// <param name="size">Reserved parameter - use 0.</param>
        /// <returns>Type of the bitmap.</returns>
        public delegate FREE_IMAGE_FORMAT GetFileTypeAInternal(string filename, int size);

        /// <param name="filename">Name of the file to analyze.</param>
        /// <param name="size">Reserved parameter - use 0.</param>
        /// <returns>Type of the bitmap.</returns>
        public delegate FREE_IMAGE_FORMAT GetFileTypeUInternal(string filename, int size);

        /// <param name="io">A <see cref="FreeImageIO" /> structure with functionpointers to handle the source.</param>
        /// <param name="handle">A handle to the source.</param>
        /// <param name="size">Size in bytes of the source.</param>
        /// <returns>Type of the bitmap.</returns>
        public delegate FREE_IMAGE_FORMAT GetFileTypeFromHandleInternal(ref FreeImageIO io, fi_handle handle, int size);

        /// <param name="stream">Pointer to the stream.</param>
        /// <param name="size">Size in bytes of the source.</param>
        /// <returns>Type of the bitmap.</returns>
        public delegate FREE_IMAGE_FORMAT GetFileTypeFromMemoryInternal(FIMEMORY stream, int size);

        /// <returns>Returns true if the platform is using Litte Endian, else false.</returns>
        public delegate bool IsLittleEndianInternal();

        /// <param name="szColor">Name of the color to convert.</param>
        /// <param name="nRed">Red component.</param>
        /// <param name="nGreen">Green component.</param>
        /// <param name="nBlue">Blue component.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool LookupX11ColorInternal(string szColor, out byte nRed, out byte nGreen, out byte nBlue);

        /// <param name="szColor">Name of the color to convert.</param>
        /// <param name="nRed">Red component.</param>
        /// <param name="nGreen">Green component.</param>
        /// <param name="nBlue">Blue component.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool LookupSVGColorInternal(string szColor, out byte nRed, out byte nGreen, out byte nBlue);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Pointer to the data-bits.</returns>
        public delegate IntPtr GetBitsInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="scanline">Number of the scanline.</param>
        /// <returns>Pointer to the scanline.</returns>
        public delegate IntPtr GetScanLineInternal(FIBITMAP dib, int scanline);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="x">Pixel position in horizontal direction.</param>
        /// <param name="y">Pixel position in vertical direction.</param>
        /// <param name="value">The pixel index.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool GetPixelIndexInternal(FIBITMAP dib, uint x, uint y, out byte value);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="x">Pixel position in horizontal direction.</param>
        /// <param name="y">Pixel position in vertical direction.</param>
        /// <param name="value">The pixel color.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool GetPixelColorInternal(FIBITMAP dib, uint x, uint y, out RGBQUAD value);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="x">Pixel position in horizontal direction.</param>
        /// <param name="y">Pixel position in vertical direction.</param>
        /// <param name="value">The new pixel index.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetPixelIndexInternal(FIBITMAP dib, uint x, uint y, ref byte value);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="x">Pixel position in horizontal direction.</param>
        /// <param name="y">Pixel position in vertical direction.</param>
        /// <param name="value">The new pixel color.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetPixelColorInternal(FIBITMAP dib, uint x, uint y, ref RGBQUAD value);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Type of the bitmap.</returns>
        public delegate FREE_IMAGE_TYPE GetImageTypeInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Palette-size for palletised bitmaps, and 0 for high-colour bitmaps.</returns>
        public delegate uint GetColorsUsedInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Size of one pixel in the bitmap in bits.</returns>
        public delegate uint GetBPPInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>With of the bitmap.</returns>
        public delegate uint GetWidthInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Height of the bitmap.</returns>
        public delegate uint GetHeightInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>With of the bitmap in bytes.</returns>
        public delegate uint GetLineInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>With of the bitmap in bytes.</returns>
        public delegate uint GetPitchInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Size of the DIB-element</returns>
        public delegate uint GetDIBSizeInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Pointer to the bitmap's palette.</returns>
        public delegate IntPtr GetPaletteInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>The horizontal resolution, in pixels-per-meter.</returns>
        public delegate uint GetDotsPerMeterXInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>The vertical resolution, in pixels-per-meter.</returns>
        public delegate uint GetDotsPerMeterYInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="res">The new horizontal resolution.</param>
        public delegate void SetDotsPerMeterXInternal(FIBITMAP dib, uint res);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="res">The new vertical resolution.</param>
        public delegate void SetDotsPerMeterYInternal(FIBITMAP dib, uint res);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Poiter to the header of the bitmap.</returns>
        public delegate IntPtr GetInfoHeaderInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Pointer to the <see cref="BITMAPINFO" /> structure for the bitmap.</returns>
        public delegate IntPtr GetInfoInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>The color type of the bitmap.</returns>
        public delegate FREE_IMAGE_COLOR_TYPE GetColorTypeInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>The bit pattern for RED.</returns>
        public delegate uint GetRedMaskInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>The bit pattern for green.</returns>
        public delegate uint GetGreenMaskInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>The bit pattern for blue.</returns>
        public delegate uint GetBlueMaskInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>The number of transparent colors in a palletised bitmap.</returns>
        public delegate uint GetTransparencyCountInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Pointer to the bitmap's transparency table.</returns>
        public delegate IntPtr GetTransparencyTableInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="enabled">True to enable the transparency, false to disable.</param>
        public delegate void SetTransparentInternal(FIBITMAP dib, bool enabled);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="table">Pointer to the bitmap's new transparency table.</param>
        /// <param name="count">The number of transparent colors in the new transparency table.</param>
        public delegate void SetTransparencyTableInternal(FIBITMAP dib, byte[] table, int count);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>
        /// Returns true when the transparency table is enabled (1-, 4- or 8-bit images)
        /// or when the input dib contains alpha values (32-bit images). Returns false otherwise.
        /// </returns>
        public delegate bool IsTransparentInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Returns true when the image has a file background color, false otherwise.</returns>
        public delegate bool HasBackgroundColorInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="bkcolor">The background color.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool GetBackgroundColorInternal(FIBITMAP dib, out RGBQUAD bkcolor);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="bkcolor">The new background color.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetBackgroundColorInternal(FIBITMAP dib, ref RGBQUAD bkcolor);

        public delegate bool SetBackgroundColorInternalArray(FIBITMAP dib, RGBQUAD[] bkcolor);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="index">The index of the palette entry to be set as transparent color.</param>
        public delegate void SetTransparentIndexInternal(FIBITMAP dib, int index);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>
        /// the index of the palette entry used as transparent color for
        /// the image specified or -1 if there is no transparent color found
        /// (e.g. the image is a high color image).
        /// </returns>
        public delegate int GetTransparentIndexInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Pointer to the <see cref="FIICCPROFILE" /> data of the bitmap.</returns>
        public delegate IntPtr GetICCProfileInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="data">Pointer to the new <see cref="FIICCPROFILE" /> data.</param>
        /// <param name="size">Size of the <see cref="FIICCPROFILE" /> data.</param>
        /// <returns>Pointer to the created <see cref="FIICCPROFILE" /> structure.</returns>
        public delegate IntPtr CreateICCProfileInternal(FIBITMAP dib, byte[] data, int size);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        public delegate void DestroyICCProfileInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ConvertTo4BitsInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ConvertTo8BitsInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ConvertToGreyscaleInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ConvertTo16Bits555Internal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ConvertTo16Bits565Internal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ConvertTo24BitsInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ConvertTo32BitsInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="quantize">Specifies the color reduction algorithm to be used.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ColorQuantizeInternal(FIBITMAP dib, FREE_IMAGE_QUANTIZE quantize);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="quantize">Specifies the color reduction algorithm to be used.</param>
        /// <param name="PaletteSize">Size of the desired output palette.</param>
        /// <param name="ReserveSize">Size of the provided palette of ReservePalette.</param>
        /// <param name="ReservePalette">The provided palette.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ColorQuantizeExInternal(FIBITMAP dib, FREE_IMAGE_QUANTIZE quantize, int PaletteSize, int ReserveSize, RGBQUAD[] ReservePalette);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="t">The threshold.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ThresholdInternal(FIBITMAP dib, byte t);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="algorithm">The dithering algorithm to use.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP DitherInternal(FIBITMAP dib, FREE_IMAGE_DITHER algorithm);

        public delegate FIBITMAP ConvertFromRawBitsInternal(IntPtr bits, int width, int height, int pitch,
            uint bpp, uint red_mask, uint green_mask, uint blue_mask, bool topdown);

        public delegate FIBITMAP ConvertFromRawBitsInternalArray(byte[] bits, int width, int height, int pitch,
            uint bpp, uint red_mask, uint green_mask, uint blue_mask, bool topdown);

        public delegate void ConvertToRawBitsInternal(IntPtr bits, FIBITMAP dib, int pitch, uint bpp,
            uint red_mask, uint green_mask, uint blue_mask, bool topdown);

        public delegate void ConvertToRawBitsInternalArray(byte[] bits, FIBITMAP dib, int pitch, uint bpp,
            uint red_mask, uint green_mask, uint blue_mask, bool topdown);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ConvertToRGBFInternal(FIBITMAP dib);

        /// <param name="src">Handle to a FreeImage bitmap.</param>
        /// <param name="scale_linear">
        /// When true the conversion is done by scaling linearly
        /// each pixel value from [min, max] to an integer value between [0..255],
        /// where min and max are the minimum and maximum pixel values in the image.
        /// When false the conversion is done by rounding each pixel value to an integer between [0..255].
        /// Rounding is done using the following formula:
        /// dst_pixel = (BYTE) MIN(255, MAX(0, q)) where int q = int(src_pixel + 0.5);
        /// </param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ConvertToStandardTypeInternal(FIBITMAP src, bool scale_linear);

        /// <param name="src">Handle to a FreeImage bitmap.</param>
        /// <param name="dst_type">Destination type.</param>
        /// <param name="scale_linear">True to scale linear, else false.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ConvertToTypeInternal(FIBITMAP src, FREE_IMAGE_TYPE dst_type, bool scale_linear);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="tmo">The tone mapping operator to be used.</param>
        /// <param name="first_param">Parmeter depending on the used algorithm</param>
        /// <param name="second_param">Parmeter depending on the used algorithm</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP ToneMappingInternal(FIBITMAP dib, FREE_IMAGE_TMO tmo, double first_param, double second_param);

        /// <param name="src">Handle to a FreeImage bitmap.</param>
        /// <param name="gamma">
        /// A gamma correction that is applied after the tone mapping.
        /// A value of 1 means no correction.
        /// </param>
        /// <param name="exposure">Scale factor allowing to adjust the brightness of the output image.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP TmoDrago03Internal(FIBITMAP src, double gamma, double exposure);

        /// <param name="src">Handle to a FreeImage bitmap.</param>
        /// <param name="intensity">Controls the overall image intensity in the range [-8, 8].</param>
        /// <param name="contrast">Controls the overall image contrast in the range [0.3, 1.0[.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP TmoReinhard05Internal(FIBITMAP src, double intensity, double contrast);

        /// <param name="src">Handle to a FreeImage bitmap.</param>
        /// <param name="color_saturation">Color saturation (s parameter in the paper) in [0.4..0.6]</param>
        /// <param name="attenuation">Atenuation factor (beta parameter in the paper) in [0.8..0.9]</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP TmoFattal02Internal(FIBITMAP src, double color_saturation, double attenuation);

        /// <param name="target">Pointer to the target buffer.</param>
        /// <param name="target_size">
        /// Size of the target buffer.
        /// Must be at least 0.1% larger than source_size plus 12 bytes.
        /// </param>
        /// <param name="source">Pointer to the source buffer.</param>
        /// <param name="source_size">Size of the source buffer.</param>
        /// <returns>The actual size of the compressed buffer, or 0 if an error occurred.</returns>
        public delegate uint ZLibCompressInternal(byte[] target, uint target_size, byte[] source, uint source_size);

        /// <param name="target">Pointer to the target buffer.</param>
        /// <param name="target_size">
        /// Size of the target buffer.
        /// Must have been saved outlide of zlib.
        /// </param>
        /// <param name="source">Pointer to the source buffer.</param>
        /// <param name="source_size">Size of the source buffer.</param>
        /// <returns>The actual size of the uncompressed buffer, or 0 if an error occurred.</returns>
        public delegate uint ZLibUncompressInternal(byte[] target, uint target_size, byte[] source, uint source_size);

        /// <param name="target">Pointer to the target buffer.</param>
        /// <param name="target_size">
        /// Size of the target buffer.
        /// Must be at least 0.1% larger than source_size plus 24 bytes.
        /// </param>
        /// <param name="source">Pointer to the source buffer.</param>
        /// <param name="source_size">Size of the source buffer.</param>
        /// <returns>The actual size of the compressed buffer, or 0 if an error occurred.</returns>
        public delegate uint ZLibGZipInternal(byte[] target, uint target_size, byte[] source, uint source_size);

        /// <param name="target">Pointer to the target buffer.</param>
        /// <param name="target_size">
        /// Size of the target buffer.
        /// Must have been saved outlide of zlib.
        /// </param>
        /// <param name="source">Pointer to the source buffer.</param>
        /// <param name="source_size">Size of the source buffer.</param>
        /// <returns>The actual size of the uncompressed buffer, or 0 if an error occurred.</returns>
        public delegate uint ZLibGUnzipInternal(byte[] target, uint target_size, byte[] source, uint source_size);

        /// <param name="crc">The CRC32 checksum to begin with.</param>
        /// <param name="source">
        /// Pointer to the source buffer.
        /// If the value is 0, the function returns the required initial value for the crc.
        /// </param>
        /// <param name="source_size">Size of the source buffer.</param>
        /// <returns></returns>
        public delegate uint ZLibCRC32Internal(uint crc, byte[] source, uint source_size);

        /// <returns>The new <see cref="FITAG" />.</returns>
        public delegate FITAG CreateTagInternal();

        /// <param name="tag">The <see cref="FITAG" /> to destroy.</param>
        public delegate void DeleteTagInternal(FITAG tag);

        /// <param name="tag">The <see cref="FITAG" /> to clone.</param>
        /// <returns>The new <see cref="FITAG" />.</returns>
        public delegate FITAG CloneTagInternal(FITAG tag);

        private unsafe delegate byte* GetTagKey_Internal(FITAG tag);

        private unsafe delegate byte* GetTagDescription_Internal(FITAG tag);

        /// <param name="tag">The tag field.</param>
        /// <returns>The ID or 0 if unavailable.</returns>
        public delegate ushort GetTagIDInternal(FITAG tag);

        /// <param name="tag">The tag field.</param>
        /// <returns>The tag type.</returns>
        public delegate FREE_IMAGE_MDTYPE GetTagTypeInternal(FITAG tag);

        /// <param name="tag">The tag field.</param>
        /// <returns>The number of components.</returns>
        public delegate uint GetTagCountInternal(FITAG tag);

        /// <param name="tag">The tag field.</param>
        /// <returns>The length of the tag value.</returns>
        public delegate uint GetTagLengthInternal(FITAG tag);

        /// <param name="tag">The tag field.</param>
        /// <returns>Pointer to the value.</returns>
        public delegate IntPtr GetTagValueInternal(FITAG tag);

        /// <param name="tag">The tag field.</param>
        /// <param name="key">The new name.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetTagKeyInternal(FITAG tag, string key);

        /// <param name="tag">The tag field.</param>
        /// <param name="description">The new description.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetTagDescriptionInternal(FITAG tag, string description);

        /// <param name="tag">The tag field.</param>
        /// <param name="id">The new ID.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetTagIDInternal(FITAG tag, ushort id);

        /// <param name="tag">The tag field.</param>
        /// <param name="type">The new type.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetTagTypeInternal(FITAG tag, FREE_IMAGE_MDTYPE type);

        /// <param name="tag">The tag field.</param>
        /// <param name="count">New number of data.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetTagCountInternal(FITAG tag, uint count);

        /// <param name="tag">The tag field.</param>
        /// <param name="length">The new length.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetTagLengthInternal(FITAG tag, uint length);

        /// <param name="tag">The tag field.</param>
        /// <param name="value">Pointer to the new value.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetTagValueInternal(FITAG tag, byte[] value);

        /// <param name="model">The model to match.</param>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="tag">Tag that matches the metadata model.</param>
        /// <returns>
        /// Unique search handle that can be used to call FindNextMetadata or FindCloseMetadata.
        /// Null if the metadata model does not exist.
        /// </returns>
        public delegate FIMETADATA FindFirstMetadataInternal(FREE_IMAGE_MDMODEL model, FIBITMAP dib, out FITAG tag);

        /// <param name="mdhandle">Unique search handle provided by FindFirstMetadata.</param>
        /// <param name="tag">Tag that matches the metadata model.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool FindNextMetadataInternal(FIMETADATA mdhandle, out FITAG tag);

        /// <param name="mdhandle">The handle to close.</param>
        private delegate void FindCloseMetadata_Internal(FIMETADATA mdhandle);

        /// <param name="model">The metadata model to look for.</param>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="key">The metadata field name.</param>
        /// <param name="tag">A FITAG structure returned by the function.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool GetMetadataInternal(FREE_IMAGE_MDMODEL model, FIBITMAP dib, string key, out FITAG tag);

        /// <param name="model">The metadata model used to store the tag.</param>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="key">The tag field name.</param>
        /// <param name="tag">The FreeImage tag to be attached.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetMetadataInternal(FREE_IMAGE_MDMODEL model, FIBITMAP dib, string key, FITAG tag);

        /// <param name="model">The metadata model.</param>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Number of tags contained in the metadata model.</returns>
        public delegate uint GetMetadataCountInternal(FREE_IMAGE_MDMODEL model, FIBITMAP dib);

        /// <param name="dst">The FreeImage bitmap to copy the metadata to.</param>
        /// <param name="src">The FreeImage bitmap to copy the metadata from.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool CloneMetadataInternal(FIBITMAP dst, FIBITMAP src);

        private unsafe delegate byte* TagToString_Internal(FREE_IMAGE_MDMODEL model, FITAG tag, uint Make);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="angle">The angle of rotation.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        [Obsolete("RotateClassic is deprecated (use Rotate instead).")]
        public delegate FIBITMAP RotateClassicInternal(FIBITMAP dib, double angle);

        public delegate FIBITMAP RotateInternal(FIBITMAP dib, double angle, IntPtr backgroundColor);

        public delegate FIBITMAP RotateExInternal(FIBITMAP dib, double angle,
            double x_shift, double y_shift, double x_origin, double y_origin, bool use_mask);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool FlipHorizontalInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool FlipVerticalInternal(FIBITMAP dib);

        public delegate bool JPEGTransformAInternal(string src_file, string dst_file,
            FREE_IMAGE_JPEG_OPERATION operation, bool perfect);

        public delegate bool JPEGTransformUInternal(string src_file, string dst_file,
            FREE_IMAGE_JPEG_OPERATION operation, bool perfect);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="dst_width">Destination width.</param>
        /// <param name="dst_height">Destination height.</param>
        /// <param name="filter">The filter to apply.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP RescaleInternal(FIBITMAP dib, int dst_width, int dst_height, FREE_IMAGE_FILTER filter);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="max_pixel_size">Thumbnail square size.</param>
        /// <param name="convert">When true HDR images are transperantly converted to standard images.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP MakeThumbnailInternal(FIBITMAP dib, int max_pixel_size, bool convert);

        public delegate FIBITMAP EnlargeCanvasInternal(FIBITMAP dib,
            int left, int top, int right, int bottom, IntPtr color, FREE_IMAGE_COLOR_OPTIONS options);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="lookUpTable">
        /// The lookup table.
        /// It's size is assumed to be 256 in length.
        /// </param>
        /// <param name="channel">The color channel to be transformed.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool AdjustCurveInternal(FIBITMAP dib, byte[] lookUpTable, FREE_IMAGE_COLOR_CHANNEL channel);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="gamma">
        /// The parameter represents the gamma value to use (gamma > 0).
        /// A value of 1.0 leaves the image alone, less than one darkens it, and greater than one lightens it.
        /// </param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool AdjustGammaInternal(FIBITMAP dib, double gamma);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="percentage">
        /// A value 0 means no change,
        /// less than 0 will make the image darker and greater than 0 will make the image brighter.
        /// </param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool AdjustBrightnessInternal(FIBITMAP dib, double percentage);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="percentage">
        /// A value 0 means no change,
        /// less than 0 will decrease the contrast and greater than 0 will increase the contrast of the image.
        /// </param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool AdjustContrastInternal(FIBITMAP dib, double percentage);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool InvertInternal(FIBITMAP dib);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="histo">Array of integers with a size of 256.</param>
        /// <param name="channel">Channel to compute from.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool GetHistogramInternal(FIBITMAP dib, int[] histo, FREE_IMAGE_COLOR_CHANNEL channel);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="channel">The color channel to extract.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP GetChannelInternal(FIBITMAP dib, FREE_IMAGE_COLOR_CHANNEL channel);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="dib8">Handle to the bitmap to insert.</param>
        /// <param name="channel">The color channel to replace.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetChannelInternal(FIBITMAP dib, FIBITMAP dib8, FREE_IMAGE_COLOR_CHANNEL channel);

        /// <param name="src">Handle to a FreeImage bitmap.</param>
        /// <param name="channel">The color channel to extract.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP GetComplexChannelInternal(FIBITMAP src, FREE_IMAGE_COLOR_CHANNEL channel);

        /// <param name="dst">Handle to a FreeImage bitmap.</param>
        /// <param name="src">Handle to a FreeImage bitmap.</param>
        /// <param name="channel">The color channel to replace.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool SetComplexChannelInternal(FIBITMAP dst, FIBITMAP src, FREE_IMAGE_COLOR_CHANNEL channel);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="left">Specifies the left position of the cropped rectangle.</param>
        /// <param name="top">Specifies the top position of the cropped rectangle.</param>
        /// <param name="right">Specifies the right position of the cropped rectangle.</param>
        /// <param name="bottom">Specifies the bottom position of the cropped rectangle.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP CopyInternal(FIBITMAP dib, int left, int top, int right, int bottom);

        /// <param name="dst">Handle to a FreeImage bitmap.</param>
        /// <param name="src">Handle to a FreeImage bitmap.</param>
        /// <param name="left">Specifies the left position of the sub image.</param>
        /// <param name="top">Specifies the top position of the sub image.</param>
        /// <param name="alpha">
        /// alpha blend factor.
        /// The source and destination images are alpha blended if alpha=0..255.
        /// If alpha > 255, then the source image is combined to the destination image.
        /// </param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool PasteInternal(FIBITMAP dst, FIBITMAP src, int left, int top, int alpha);

        /// <param name="fg">Handle to a FreeImage bitmap.</param>
        /// <param name="useFileBkg">When true the background of fg is used if it contains one.</param>
        /// <param name="appBkColor">The application background is used if useFileBkg is false.</param>
        /// <param name="bg">
        /// Image used as background when useFileBkg is false or fg has no background
        /// and appBkColor is null.
        /// </param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP CompositeInternal(FIBITMAP fg, bool useFileBkg, ref RGBQUAD appBkColor, FIBITMAP bg);

        public delegate FIBITMAP CompositeInternalArray(FIBITMAP fg, bool useFileBkg, RGBQUAD[] appBkColor, FIBITMAP bg);

        /// <param name="src_file">Source filename.</param>
        /// <param name="dst_file">Destination filename.</param>
        /// <param name="left">Specifies the left position of the cropped rectangle.</param>
        /// <param name="top">Specifies the top position of the cropped rectangle.</param>
        /// <param name="right">Specifies the right position of the cropped rectangle.</param>
        /// <param name="bottom">Specifies the bottom position of the cropped rectangle.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool JPEGCropAInternal(string src_file, string dst_file, int left, int top, int right, int bottom);

        /// <param name="src_file">Source filename.</param>
        /// <param name="dst_file">Destination filename.</param>
        /// <param name="left">Specifies the left position of the cropped rectangle.</param>
        /// <param name="top">Specifies the top position of the cropped rectangle.</param>
        /// <param name="right">Specifies the right position of the cropped rectangle.</param>
        /// <param name="bottom">Specifies the bottom position of the cropped rectangle.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool JPEGCropUInternal(string src_file, string dst_file, int left, int top, int right, int bottom);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public delegate bool PreMultiplyWithAlphaInternal(FIBITMAP dib);

        /// <param name="Laplacian">Handle to a FreeImage bitmap.</param>
        /// <param name="ncycle">Number of cycles in the multigrid algorithm (usually 2 or 3)</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public delegate FIBITMAP MultigridPoissonSolverInternal(FIBITMAP Laplacian, int ncycle);

        /// <param name="lookUpTable">
        /// Output lookup table to be used with <see cref="AdjustCurve" />.
        /// The size of 'lookUpTable' is assumed to be 256.
        /// </param>
        /// <param name="brightness">
        /// Percentage brightness value where -100 &lt;= brightness &lt;= 100.
        /// <para>
        /// A value of 0 means no change, less than 0 will make the image darker and greater
        /// than 0 will make the image brighter.
        /// </para>
        /// </param>
        /// <param name="contrast">
        /// Percentage contrast value where -100 &lt;= contrast &lt;= 100.
        /// <para>
        /// A value of 0 means no change, less than 0 will decrease the contrast
        /// and greater than 0 will increase the contrast of the image.
        /// </para>
        /// </param>
        /// <param name="gamma">
        /// Gamma value to be used for gamma correction.
        /// <para>
        /// A value of 1.0 leaves the image alone, less than one darkens it,
        /// and greater than one lightens it.
        /// </para>
        /// </param>
        /// <param name="invert">If set to true, the image will be inverted.</param>
        /// <returns>
        /// The number of adjustments applied to the resulting lookup table
        /// compared to a blind lookup table.
        /// </returns>
        /// <remarks>
        /// This function creates a lookup table to be used with <see cref="AdjustCurve" /> which may adjust
        /// brightness and contrast, correct gamma and invert the image with a single call to
        /// <see cref="AdjustCurve" />. If more than one of these image display properties need to be adjusted,
        /// using a combined lookup table should be preferred over calling each adjustment function
        /// separately. That's particularly true for huge images or if performance is an issue. Then,
        /// the expensive process of iterating over all pixels of an image is performed only once and
        /// not up to four times.
        /// <para />
        /// Furthermore, the lookup table created does not depend on the order, in which each single
        /// adjustment operation is performed. Due to rounding and byte casting issues, it actually
        /// matters in which order individual adjustment operations are performed. Both of the following
        /// snippets most likely produce different results:
        /// <para />
        /// <code>
        /// // snippet 1: contrast, brightness
        /// AdjustContrast(dib, 15.0);
        /// AdjustBrightness(dib, 50.0);
        /// </code>
        /// <para />
        /// <code>
        /// // snippet 2: brightness, contrast
        /// AdjustBrightness(dib, 50.0);
        /// AdjustContrast(dib, 15.0);
        /// </code>
        /// <para />
        /// Better and even faster would be snippet 3:
        /// <para />
        /// <code>
        /// // snippet 3:
        /// byte[] lut = new byte[256];
        /// GetAdjustColorsLookupTable(lut, 50.0, 15.0, 1.0, false);
        /// AdjustCurve(dib, lut, FREE_IMAGE_COLOR_CHANNEL.FICC_RGB);
        /// </code>
        /// <para />
        /// This function is also used internally by <see cref="AdjustColors" />, which does not return the
        /// lookup table, but uses it to call <see cref="AdjustCurve" /> on the passed image.
        /// </remarks>
        public delegate int GetAdjustColorsLookupTableInternal(byte[] lookUpTable, double brightness, double contrast, double gamma, bool invert);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="brightness">
        /// Percentage brightness value where -100 &lt;= brightness &lt;= 100.
        /// <para>
        /// A value of 0 means no change, less than 0 will make the image darker and greater
        /// than 0 will make the image brighter.
        /// </para>
        /// </param>
        /// <param name="contrast">
        /// Percentage contrast value where -100 &lt;= contrast &lt;= 100.
        /// <para>
        /// A value of 0 means no change, less than 0 will decrease the contrast
        /// and greater than 0 will increase the contrast of the image.
        /// </para>
        /// </param>
        /// <param name="gamma">
        /// Gamma value to be used for gamma correction.
        /// <para>
        /// A value of 1.0 leaves the image alone, less than one darkens it,
        /// and greater than one lightens it.
        /// </para>
        /// This parameter must not be zero or smaller than zero.
        /// If so, it will be ignored and no gamma correction will be performed on the image.
        /// </param>
        /// <param name="invert">If set to true, the image will be inverted.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        /// <remarks>
        /// This function adjusts an image's brightness, contrast and gamma as well as it
        /// may optionally invert the image within a single operation. If more than one of
        /// these image display properties need to be adjusted, using this function should
        /// be preferred over calling each adjustment function separately. That's particularly
        /// true for huge images or if performance is an issue.
        /// <para />
        /// This function relies on <see cref="GetAdjustColorsLookupTable" />,
        /// which creates a single lookup table, that combines all adjustment operations requested.
        /// <para />
        /// Furthermore, the lookup table created by <see cref="GetAdjustColorsLookupTable" /> does
        /// not depend on the order, in which each single adjustment operation is performed.
        /// Due to rounding and byte casting issues, it actually matters in which order individual
        /// adjustment operations are performed. Both of the following snippets most likely produce
        /// different results:
        /// <para />
        /// <code>
        /// // snippet 1: contrast, brightness
        /// AdjustContrast(dib, 15.0);
        /// AdjustBrightness(dib, 50.0);
        /// </code>
        /// <para />
        /// <code>
        /// // snippet 2: brightness, contrast
        /// AdjustBrightness(dib, 50.0);
        /// AdjustContrast(dib, 15.0);
        /// </code>
        /// <para />
        /// Better and even faster would be snippet 3:
        /// <para />
        /// <code>
        /// // snippet 3:
        /// AdjustColors(dib, 50.0, 15.0, 1.0, false);
        /// </code>
        /// </remarks>
        public delegate bool AdjustColorsInternal(FIBITMAP dib, double brightness, double contrast, double gamma, bool invert);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="srccolors">Array of colors to be used as the mapping source.</param>
        /// <param name="dstcolors">Array of colors to be used as the mapping destination.</param>
        /// <param name="count">
        /// The number of colors to be mapped. This is the size of both
        /// srccolors and dstcolors.
        /// </param>
        /// <param name="ignore_alpha">If true, 32-bit images and colors are treated as 24-bit.</param>
        /// <param name="swap">
        /// If true, source and destination colors are swapped, that is,
        /// each destination color is also mapped to the corresponding source color.
        /// </param>
        /// <returns>The total number of pixels changed.</returns>
        /// <remarks>
        /// This function maps up to <paramref name="count" /> colors specified in
        /// <paramref name="srccolors" /> to these specified in <paramref name="dstcolors" />.
        /// Thereby, color <i>srccolors[N]</i>, if found in the image, will be replaced by color
        /// <i>dstcolors[N]</i>. If <paramref name="swap" /> is <b>true</b>, additionally all colors
        /// specified in <paramref name="dstcolors" /> are also mapped to these specified
        /// in <paramref name="srccolors" />. For high color images, the actual image data will be
        /// modified whereas, for palletized images only the palette will be changed.
        /// <para />
        /// The function returns the number of pixels changed or zero, if no pixels were changed.
        /// <para />
        /// Both arrays <paramref name="srccolors" /> and <paramref name="dstcolors" /> are assumed
        /// not to hold less than <paramref name="count" /> colors.
        /// <para />
        /// For 16-bit images, all colors specified are transparently converted to their
        /// proper 16-bit representation (either in RGB555 or RGB565 format, which is determined
        /// by the image's red- green- and blue-mask).
        /// <para />
        /// <b>
        /// Note, that this behaviour is different from what <see cref="ApplyPaletteIndexMapping" /> does,
        /// which modifies the actual image data on palletized images.
        /// </b>
        /// </remarks>
        public delegate uint ApplyColorMappingInternal(FIBITMAP dib, RGBQUAD[] srccolors, RGBQUAD[] dstcolors, uint count, bool ignore_alpha, bool swap);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="color_a">One of the two colors to be swapped.</param>
        /// <param name="color_b">The other of the two colors to be swapped.</param>
        /// <param name="ignore_alpha">If true, 32-bit images and colors are treated as 24-bit.</param>
        /// <returns>The total number of pixels changed.</returns>
        /// <remarks>
        /// This function swaps the two specified colors <paramref name="color_a" /> and
        /// <paramref name="color_b" /> on a palletized or high color image.
        /// For high color images, the actual image data will be modified whereas, for palletized
        /// images only the palette will be changed.
        /// <para />
        /// <b>
        /// Note, that this behaviour is different from what <see cref="SwapPaletteIndices" /> does,
        /// which modifies the actual image data on palletized images.
        /// </b>
        /// <para />
        /// This is just a thin wrapper for <see cref="ApplyColorMapping" /> and resolves to:
        /// <para />
        /// <code>
        /// return ApplyColorMapping(dib, color_a, color_b, 1, ignore_alpha, true);
        /// </code>
        /// </remarks>
        public delegate uint SwapColorsInternal(FIBITMAP dib, ref RGBQUAD color_a, ref RGBQUAD color_b, bool ignore_alpha);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="srcindices">Array of palette indices to be used as the mapping source.</param>
        /// <param name="dstindices">Array of palette indices to be used as the mapping destination.</param>
        /// <param name="count">
        /// The number of palette indices to be mapped. This is the size of both
        /// srcindices and dstindices
        /// </param>
        /// <param name="swap">
        /// If true, source and destination palette indices are swapped, that is,
        /// each destination index is also mapped to the corresponding source index.
        /// </param>
        /// <returns>The total number of pixels changed.</returns>
        /// <remarks>
        /// This function maps up to <paramref name="count" /> palette indices specified in
        /// <paramref name="srcindices" /> to these specified in <paramref name="dstindices" />.
        /// Thereby, index <i>srcindices[N]</i>, if present in the image, will be replaced by index
        /// <i>dstindices[N]</i>. If <paramref name="swap" /> is <b>true</b>, additionally all indices
        /// specified in <paramref name="dstindices" /> are also mapped to these specified in
        /// <paramref name="srcindices" />.
        /// <para />
        /// The function returns the number of pixels changed or zero, if no pixels were changed.
        /// Both arrays <paramref name="srcindices" /> and <paramref name="dstindices" /> are assumed not to
        /// hold less than <paramref name="count" /> indices.
        /// <para />
        /// <b>
        /// Note, that this behaviour is different from what <see cref="ApplyColorMapping" /> does, which
        /// modifies the actual image data on palletized images.
        /// </b>
        /// </remarks>
        public delegate uint ApplyPaletteIndexMappingInternal(FIBITMAP dib, byte[] srcindices, byte[] dstindices, uint count, bool swap);

        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="index_a">One of the two palette indices to be swapped.</param>
        /// <param name="index_b">The other of the two palette indices to be swapped.</param>
        /// <returns>The total number of pixels changed.</returns>
        /// <remarks>
        /// This function swaps the two specified palette indices <i>index_a</i> and
        /// <i>index_b</i> on a palletized image. Therefore, not the palette, but the
        /// actual image data will be modified.
        /// <para />
        /// <b>
        /// Note, that this behaviour is different from what <see cref="SwapColors" /> does on palletized images,
        /// which only swaps the colors in the palette.
        /// </b>
        /// <para />
        /// This is just a thin wrapper for <see cref="ApplyColorMapping" /> and resolves to:
        /// <para />
        /// <code>
        /// return ApplyPaletteIndexMapping(dib, index_a, index_b, 1, true);
        /// </code>
        /// </remarks>
        public delegate uint SwapPaletteIndicesInternal(FIBITMAP dib, ref byte index_a, ref byte index_b);

        public delegate bool FillBackgroundInternal(FIBITMAP dib, IntPtr color, FREE_IMAGE_COLOR_OPTIONS options);

        #endregion

        #region Function Handles

        /// <summary>
        /// Initialises the library.
        /// </summary>
        public static InitialiseInternal Initialise;

        /// <summary>
        /// Deinitialises the library.
        /// </summary>
        public static DeInitialiseInternal DeInitialise;

        private static GetVersion_Internal GetVersion_;
        private static GetCopyrightMessage_Internal GetCopyrightMessage_;

        /// <summary>
        /// Calls the set error message function in FreeImage.
        /// </summary>
        public static OutputMessageProcInternal OutputMessageProc;

        /// <summary>
        /// You use the function FreeImage_SetOutputMessage to capture the log string
        /// so that you can show it to the user of the program.
        /// The callback is implemented in the <see cref="FreeImageEngine.Message" /> event of this class.
        /// </summary>
        public static SetOutputMessageInternal SetOutputMessage;

        public static AllocateInternal Allocate_;
        public static AllocateTInternal AllocateT_;
        public static AllocateExInternal AllocateEx_;
        public static AllocateExTInternal AllocateExT_;

        /// <summary>
        /// Makes an exact reproduction of an existing bitmap, including metadata and attached profile if any.
        /// </summary>
        public static CloneInternal Clone;

        /// <summary>
        /// Deletes a previously loaded FIBITMAP from memory.
        /// </summary>
        public static UnloadInternal Unload;

        /// <summary>
        /// Decodes a bitmap, allocates memory for it and returns it as a FIBITMAP.
        /// </summary>
        public static LoadAInternal LoadA;

        /// <summary>
        /// Decodes a bitmap, allocates memory for it and returns it as a FIBITMAP.
        /// Supports UNICODE filenames.
        /// </summary>
        public static LoadUInternal LoadU;

        /// <summary>
        /// Loads a bitmap from an arbitrary source.
        /// </summary>
        public static LoadFromHandleInternal LoadFromHandle;

        /// <summary>
        /// Saves a previosly loaded FIBITMAP to a file.
        /// </summary>
        public static SaveAInternal SaveA;

        /// <summary>
        /// Saves a previosly loaded FIBITMAP to a file.
        /// Supports UNICODE filenames.
        /// </summary>
        public static SaveUInternal SaveU;

        public static SaveToHandleInternal SaveToHandle;

        /// <summary>
        /// Open a memory stream.
        /// </summary>
        public static OpenMemoryInternal OpenMemory;

        public static OpenMemoryExInternal OpenMemoryEx;

        /// <summary>
        /// Close and free a memory stream.
        /// </summary>
        public static CloseMemoryInternal CloseMemory;

        /// <summary>
        /// Decodes a bitmap from a stream, allocates memory for it and returns it as a FIBITMAP.
        /// </summary>
        public static LoadFromMemoryInternal LoadFromMemory;

        /// <summary>
        /// Saves a previosly loaded FIBITMAP to a stream.
        /// </summary>
        public static SaveToMemoryInternal SaveToMemory;

        /// <summary>
        /// Gets the current position of a memory handle.
        /// </summary>
        public static TellMemoryInternal TellMemory;

        /// <summary>
        /// Moves the memory handle to a specified location.
        /// </summary>
        public static SeekMemoryInternal SeekMemory;

        /// <summary>
        /// Provides a direct buffer access to a memory stream.
        /// </summary>
        public static AcquireMemoryInternal AcquireMemory;

        /// <summary>
        /// Reads data from a memory stream.
        /// </summary>
        public static ReadMemoryInternal ReadMemory;

        /// <summary>
        /// Writes data to a memory stream.
        /// </summary>
        public static WriteMemoryInternal WriteMemory;

        /// <summary>
        /// Open a multi-page bitmap from a memory stream.
        /// </summary>
        public static LoadMultiBitmapFromMemoryInternal LoadMultiBitmapFromMemory;

        public static RegisterLocalPluginInternal RegisterLocalPlugin;
        public static RegisterExternalPluginInternal RegisterExternalPlugin;

        /// <summary>
        /// Retrieves the number of FREE_IMAGE_FORMAT identifiers being currently registered.
        /// </summary>
        public static GetFIFCountInternal GetFIFCount;

        /// <summary>
        /// Enables or disables a plugin.
        /// </summary>
        public static SetPluginEnabledInternal SetPluginEnabled;

        /// <summary>
        /// Retrieves the state of a plugin.
        /// </summary>
        public static IsPluginEnabledInternal IsPluginEnabled;

        /// <summary>
        /// Returns a <see cref="FREE_IMAGE_FORMAT" /> identifier from the format string that was used to register the FIF.
        /// </summary>
        public static GetFIFFromFormatInternal GetFIFFromFormat;

        /// <summary>
        /// Returns a <see cref="FREE_IMAGE_FORMAT" /> identifier from a MIME content type string
        /// (MIME stands for Multipurpose Internet Mail Extension).
        /// </summary>
        public static GetFIFFromMimeInternal GetFIFFromMime;

        private static GetFormatFromFIF_Internal GetFormatFromFIF_;
        private static GetFIFExtensionList_Internal GetFIFExtensionList_;
        private static GetFIFDescription_Internal GetFIFDescription_;
        private static GetFIFRegExpr_Internal GetFIFRegExpr_;
        private static GetFIFMimeType_Internal GetFIFMimeType_;

        /// <summary>
        /// This function takes a filename or a file-extension and returns the plugin that can
        /// read/write files with that extension in the form of a <see cref="FREE_IMAGE_FORMAT" /> identifier.
        /// </summary>
        public static GetFIFFromFilenameAInternal GetFIFFromFilenameA;

        /// <summary>
        /// This function takes a filename or a file-extension and returns the plugin that can
        /// read/write files with that extension in the form of a <see cref="FREE_IMAGE_FORMAT" /> identifier.
        /// Supports UNICODE filenames.
        /// </summary>
        public static GetFIFFromFilenameUInternal GetFIFFromFilenameU;

        /// <summary>
        /// Checks if a plugin can load bitmaps.
        /// </summary>
        public static FIFSupportsReadingInternal FIFSupportsReading;

        /// <summary>
        /// Checks if a plugin can save bitmaps.
        /// </summary>
        public static FIFSupportsWritingInternal FIFSupportsWriting;

        /// <summary>
        /// Checks if a plugin can save bitmaps in the desired bit depth.
        /// </summary>
        public static FIFSupportsExportBPPInternal FIFSupportsExportBPP;

        /// <summary>
        /// Checks if a plugin can save a bitmap in the desired data type.
        /// </summary>
        public static FIFSupportsExportTypeInternal FIFSupportsExportType;

        /// <summary>
        /// Checks if a plugin can load or save an ICC profile.
        /// </summary>
        public static FIFSupportsICCProfilesInternal FIFSupportsICCProfiles;

        public static OpenMultiBitmapInternal OpenMultiBitmap;
        public static OpenMultiBitmapFromHandleInternal OpenMultiBitmapFromHandle;

        /// <summary>
        /// Closes a previously opened multi-page bitmap and, when the bitmap was not opened read-only, applies any changes made to
        /// it.
        /// </summary>
        private static CloseMultiBitmap_Internal CloseMultiBitmap_;

        /// <summary>
        /// Returns the number of pages currently available in the multi-paged bitmap.
        /// </summary>
        public static GetPageCountInternal GetPageCount;

        /// <summary>
        /// Appends a new page to the end of the bitmap.
        /// </summary>
        public static AppendPageInternal AppendPage;

        /// <summary>
        /// Inserts a new page before the given position in the bitmap.
        /// </summary>
        public static InsertPageInternal InsertPage;

        /// <summary>
        /// Deletes the page on the given position.
        /// </summary>
        public static DeletePageInternal DeletePage;

        /// <summary>
        /// Locks a page in memory for editing.
        /// </summary>
        public static LockPageInternal LockPage;

        /// <summary>
        /// Unlocks a previously locked page and gives it back to the multi-page engine.
        /// </summary>
        public static UnlockPageInternal UnlockPage;

        /// <summary>
        /// Moves the source page to the position of the target page.
        /// </summary>
        public static MovePageInternal MovePage;

        /// <summary>
        /// Returns an array of page-numbers that are currently locked in memory.
        /// When the pages parameter is null, the size of the array is returned in the count variable.
        /// </summary>
        public static GetLockedPageNumbersInternal GetLockedPageNumbers;

        /// <summary>
        /// Orders FreeImage to analyze the bitmap signature.
        /// </summary>
        public static GetFileTypeAInternal GetFileTypeA;

        /// <summary>
        /// Orders FreeImage to analyze the bitmap signature.
        /// Supports UNICODE filenames.
        /// </summary>
        public static GetFileTypeUInternal GetFileTypeU;

        /// <summary>
        /// Uses the <see cref="FreeImageIO" /> structure as described in the topic bitmap management functions
        /// to identify a bitmap type.
        /// </summary>
        public static GetFileTypeFromHandleInternal GetFileTypeFromHandle;

        /// <summary>
        /// Uses a memory handle to identify a bitmap type.
        /// </summary>
        public static GetFileTypeFromMemoryInternal GetFileTypeFromMemory;

        /// <summary>
        /// Returns whether the platform is using Little Endian.
        /// </summary>
        public static IsLittleEndianInternal IsLittleEndian;

        /// <summary>
        /// Converts a X11 color name into a corresponding RGB value.
        /// </summary>
        public static LookupX11ColorInternal LookupX11Color;

        /// <summary>
        /// Converts a SVG color name into a corresponding RGB value.
        /// </summary>
        public static LookupSVGColorInternal LookupSVGColor;

        /// <summary>
        /// Returns a pointer to the data-bits of the bitmap.
        /// </summary>
        public static GetBitsInternal GetBits;

        /// <summary>
        /// Returns a pointer to the start of the given scanline in the bitmap's data-bits.
        /// </summary>
        public static GetScanLineInternal GetScanLine;

        /// <summary>
        /// Get the pixel index of a palettized image at position (x, y), including range check (slow access).
        /// </summary>
        public static GetPixelIndexInternal GetPixelIndex;

        /// <summary>
        /// Get the pixel color of a 16-, 24- or 32-bit image at position (x, y), including range check (slow access).
        /// </summary>
        public static GetPixelColorInternal GetPixelColor;

        /// <summary>
        /// Set the pixel index of a palettized image at position (x, y), including range check (slow access).
        /// </summary>
        public static SetPixelIndexInternal SetPixelIndex;

        /// <summary>
        /// Set the pixel color of a 16-, 24- or 32-bit image at position (x, y), including range check (slow access).
        /// </summary>
        public static SetPixelColorInternal SetPixelColor;

        /// <summary>
        /// Retrieves the type of the bitmap.
        /// </summary>
        public static GetImageTypeInternal GetImageType;

        /// <summary>
        /// Returns the number of colors used in a bitmap.
        /// </summary>
        public static GetColorsUsedInternal GetColorsUsed;

        /// <summary>
        /// Returns the size of one pixel in the bitmap in bits.
        /// </summary>
        public static GetBPPInternal GetBPP;

        /// <summary>
        /// Returns the width of the bitmap in pixel units.
        /// </summary>
        public static GetWidthInternal GetWidth;

        /// <summary>
        /// Returns the height of the bitmap in pixel units.
        /// </summary>
        public static GetHeightInternal GetHeight;

        /// <summary>
        /// Returns the width of the bitmap in bytes.
        /// </summary>
        public static GetLineInternal GetLine;

        /// <summary>
        /// Returns the width of the bitmap in bytes, rounded to the next 32-bit boundary,
        /// also known as pitch or stride or scan width.
        /// </summary>
        public static GetPitchInternal GetPitch;

        /// <summary>
        /// Returns the size of the DIB-element of a FIBITMAP in memory.
        /// </summary>
        public static GetDIBSizeInternal GetDIBSize;

        /// <summary>
        /// Returns a pointer to the bitmap's palette.
        /// </summary>
        public static GetPaletteInternal GetPalette;

        /// <summary>
        /// Returns the horizontal resolution, in pixels-per-meter, of the target device for the bitmap.
        /// </summary>
        public static GetDotsPerMeterXInternal GetDotsPerMeterX;

        /// <summary>
        /// Returns the vertical resolution, in pixels-per-meter, of the target device for the bitmap.
        /// </summary>
        public static GetDotsPerMeterYInternal GetDotsPerMeterY;

        /// <summary>
        /// Set the horizontal resolution, in pixels-per-meter, of the target device for the bitmap.
        /// </summary>
        public static SetDotsPerMeterXInternal SetDotsPerMeterX;

        /// <summary>
        /// Set the vertical resolution, in pixels-per-meter, of the target device for the bitmap.
        /// </summary>
        public static SetDotsPerMeterYInternal SetDotsPerMeterY;

        /// <summary>
        /// Returns a pointer to the <see cref="BITMAPINFOHEADER" /> of the DIB-element in a FIBITMAP.
        /// </summary>
        public static GetInfoHeaderInternal GetInfoHeader;

        /// <summary>
        /// Alias for FreeImage_GetInfoHeader that returns a pointer to a <see cref="BITMAPINFO" />
        /// rather than to a <see cref="BITMAPINFOHEADER" />.
        /// </summary>
        public static GetInfoInternal GetInfo;

        /// <summary>
        /// Investigates the color type of the bitmap by reading the bitmap's pixel bits and analysing them.
        /// </summary>
        public static GetColorTypeInternal GetColorType;

        /// <summary>
        /// Returns a bit pattern describing the red color component of a pixel in a FreeImage bitmap.
        /// </summary>
        public static GetRedMaskInternal GetRedMask;

        /// <summary>
        /// Returns a bit pattern describing the green color component of a pixel in a FreeImage bitmap.
        /// </summary>
        public static GetGreenMaskInternal GetGreenMask;

        /// <summary>
        /// Returns a bit pattern describing the blue color component of a pixel in a FreeImage bitmap.
        /// </summary>
        public static GetBlueMaskInternal GetBlueMask;

        /// <summary>
        /// Returns the number of transparent colors in a palletised bitmap.
        /// </summary>
        public static GetTransparencyCountInternal GetTransparencyCount;

        /// <summary>
        /// Returns a pointer to the bitmap's transparency table.
        /// </summary>
        public static GetTransparencyTableInternal GetTransparencyTable;

        /// <summary>
        /// Tells FreeImage if it should make use of the transparency table
        /// or the alpha channel that may accompany a bitmap.
        /// </summary>
        public static SetTransparentInternal SetTransparent;

        /// <summary>
        /// Set the bitmap's transparency table. Only affects palletised bitmaps.
        /// </summary>
        public static SetTransparencyTableInternal SetTransparencyTable_;

        /// <summary>
        /// Returns whether the transparency table is enabled.
        /// </summary>
        public static IsTransparentInternal IsTransparent;

        /// <summary>
        /// Returns whether the bitmap has a file background color.
        /// </summary>
        public static HasBackgroundColorInternal HasBackgroundColor;

        /// <summary>
        /// Returns the file background color of an image.
        /// For 8-bit images, the color index in the palette is returned in the
        /// rgbReserved member of the bkcolor parameter.
        /// </summary>
        public static GetBackgroundColorInternal GetBackgroundColor;

        /// <summary>
        /// Set the file background color of an image.
        /// When saving an image to PNG, this background color is transparently saved to the PNG file.
        /// </summary>
        public static SetBackgroundColorInternalArray SetBackgroundColorArray;

        /// <summary>
        /// Set the file background color of an image.
        /// When saving an image to PNG, this background color is transparently saved to the PNG file.
        /// </summary>
        public static SetBackgroundColorInternal SetBackgroundColor;

        /// <summary>
        /// Sets the index of the palette entry to be used as transparent color
        /// for the image specified. Does nothing on high color images.
        /// </summary>
        public static SetTransparentIndexInternal SetTransparentIndex;

        /// <summary>
        /// Returns the palette entry used as transparent color for the image specified.
        /// Works for palletised images only and returns -1 for high color
        /// images or if the image has no color set to be transparent.
        /// </summary>
        public static GetTransparentIndexInternal GetTransparentIndex;

        /// <summary>
        /// Retrieves a pointer to the <see cref="FIICCPROFILE" /> data of the bitmap.
        /// This function can also be called safely, when the original format does not support profiles.
        /// </summary>
        public static GetICCProfileInternal GetICCProfile;

        /// <summary>
        /// Creates a new <see cref="FIICCPROFILE" /> block from ICC profile data previously read from a file
        /// or built by a color management system. The profile data is attached to the bitmap.
        /// </summary>
        public static CreateICCProfileInternal CreateICCProfile;

        /// <summary>
        /// This function destroys an <see cref="FIICCPROFILE" /> previously created by
        /// <see cref="CreateICCProfile(FIBITMAP,byte[],int)" />.
        /// After this call the bitmap will contain no profile information.
        /// This function should be called to ensure that a stored bitmap will not contain any profile information.
        /// </summary>
        public static DestroyICCProfileInternal DestroyICCProfile;

        /// <summary>
        /// Converts a bitmap to 4 bits.
        /// If the bitmap was a high-color bitmap (16, 24 or 32-bit) or if it was a
        /// monochrome or greyscale bitmap (1 or 8-bit), the end result will be a
        /// greyscale bitmap, otherwise (1-bit palletised bitmaps) it will be a palletised bitmap.
        /// </summary>
        public static ConvertTo4BitsInternal ConvertTo4Bits;

        /// <summary>
        /// Converts a bitmap to 8 bits. If the bitmap was a high-color bitmap (16, 24 or 32-bit)
        /// or if it was a monochrome or greyscale bitmap (1 or 4-bit), the end result will be a
        /// greyscale bitmap, otherwise (1 or 4-bit palletised bitmaps) it will be a palletised bitmap.
        /// </summary>
        public static ConvertTo8BitsInternal ConvertTo8Bits;

        /// <summary>
        /// Converts a bitmap to a 8-bit greyscale image with a linear ramp.
        /// </summary>
        public static ConvertToGreyscaleInternal ConvertToGreyscale;

        /// <summary>
        /// Converts a bitmap to 16 bits, where each pixel has a color pattern of
        /// 5 bits red, 5 bits green and 5 bits blue. One bit in each pixel is unused.
        /// </summary>
        public static ConvertTo16Bits555Internal ConvertTo16Bits555;

        /// <summary>
        /// Converts a bitmap to 16 bits, where each pixel has a color pattern of
        /// 5 bits red, 6 bits green and 5 bits blue.
        /// </summary>
        public static ConvertTo16Bits565Internal ConvertTo16Bits565;

        /// <summary>
        /// Converts a bitmap to 24 bits. A clone of the input bitmap is returned for 24-bit bitmaps.
        /// </summary>
        public static ConvertTo24BitsInternal ConvertTo24Bits;

        /// <summary>
        /// Converts a bitmap to 32 bits. A clone of the input bitmap is returned for 32-bit bitmaps.
        /// </summary>
        public static ConvertTo32BitsInternal ConvertTo32Bits;

        /// <summary>
        /// Quantizes a high-color 24-bit bitmap to an 8-bit palette color bitmap.
        /// </summary>
        public static ColorQuantizeInternal ColorQuantize;

        /// <summary>
        /// ColorQuantizeEx is an extension to the <see cref="ColorQuantize(FIBITMAP, FREE_IMAGE_QUANTIZE)" /> method that
        /// provides additional options used to quantize a 24-bit image to any
        /// number of colors (up to 256), as well as quantize a 24-bit image using a
        /// partial or full provided palette.
        /// </summary>
        public static ColorQuantizeExInternal ColorQuantizeEx_;

        /// <summary>
        /// Converts a bitmap to 1-bit monochrome bitmap using a threshold T between [0..255].
        /// The function first converts the bitmap to a 8-bit greyscale bitmap.
        /// Then, any brightness level that is less than T is set to zero, otherwise to 1.
        /// For 1-bit input bitmaps, the function clones the input bitmap and builds a monochrome palette.
        /// </summary>
        public static ThresholdInternal Threshold;

        /// <summary>
        /// Converts a bitmap to 1-bit monochrome bitmap using a dithering algorithm.
        /// For 1-bit input bitmaps, the function clones the input bitmap and builds a monochrome palette.
        /// </summary>
        public static DitherInternal Dither;

        public static ConvertFromRawBitsInternal ConvertFromRawBits_;
        public static ConvertFromRawBitsInternalArray ConvertFromRawBitsArray;
        public static ConvertToRawBitsInternal ConvertToRawBits;
        public static ConvertToRawBitsInternalArray ConvertToRawBitsArray;

        /// <summary>
        /// Converts a 24- or 32-bit RGB(A) standard image or a 48-bit RGB image to a FIT_RGBF type image.
        /// </summary>
        public static ConvertToRGBFInternal ConvertToRGBF;

        /// <summary>
        /// Converts a non standard image whose color type is FIC_MINISBLACK
        /// to a standard 8-bit greyscale image.
        /// </summary>
        public static ConvertToStandardTypeInternal ConvertToStandardType;

        /// <summary>
        /// Converts an image of any type to type dst_type.
        /// </summary>
        public static ConvertToTypeInternal ConvertToType;

        /// <summary>
        /// Converts a High Dynamic Range image (48-bit RGB or 96-bit RGBF) to a 24-bit RGB image, suitable for display.
        /// </summary>
        public static ToneMappingInternal ToneMapping;

        /// <summary>
        /// Converts a High Dynamic Range image to a 24-bit RGB image using a global
        /// operator based on logarithmic compression of luminance values, imitating the human response to light.
        /// </summary>
        public static TmoDrago03Internal TmoDrago03;

        /// <summary>
        /// Converts a High Dynamic Range image to a 24-bit RGB image using a global operator inspired
        /// by photoreceptor physiology of the human visual system.
        /// </summary>
        public static TmoReinhard05Internal TmoReinhard05;

        /// <summary>
        /// Apply the Gradient Domain High Dynamic Range Compression to a RGBF image and convert to 24-bit RGB.
        /// </summary>
        public static TmoFattal02Internal TmoFattal02;

        /// <summary>
        /// Compresses a source buffer into a target buffer, using the ZLib library.
        /// </summary>
        public static ZLibCompressInternal ZLibCompress;

        /// <summary>
        /// Decompresses a source buffer into a target buffer, using the ZLib library.
        /// </summary>
        public static ZLibUncompressInternal ZLibUncompress;

        /// <summary>
        /// Compresses a source buffer into a target buffer, using the ZLib library.
        /// </summary>
        public static ZLibGZipInternal ZLibGZip;

        /// <summary>
        /// Decompresses a source buffer into a target buffer, using the ZLib library.
        /// </summary>
        public static ZLibGUnzipInternal ZLibGUnzip;

        /// <summary>
        /// Generates a CRC32 checksum.
        /// </summary>
        public static ZLibCRC32Internal ZLibCRC32;

        /// <summary>
        /// Allocates a new <see cref="FITAG" /> object.
        /// This object must be destroyed with a call to
        /// <see cref="FreeImageAPI.FreeImage.DeleteTag(FITAG)" /> when no longer in use.
        /// </summary>
        public static CreateTagInternal CreateTag;

        /// <summary>
        /// Delete a previously allocated <see cref="FITAG" /> object.
        /// </summary>
        public static DeleteTagInternal DeleteTag;

        /// <summary>
        /// Creates and returns a copy of a <see cref="FITAG" /> object.
        /// </summary>
        public static CloneTagInternal CloneTag;

        private static GetTagKey_Internal GetTagKey_;
        private static GetTagDescription_Internal GetTagDescription_;

        /// <summary>
        /// Returns the tag ID.
        /// </summary>
        public static GetTagIDInternal GetTagID;

        /// <summary>
        /// Returns the tag data type.
        /// </summary>
        public static GetTagTypeInternal GetTagType;

        /// <summary>
        /// Returns the number of components in the tag (in tag type units).
        /// </summary>
        public static GetTagCountInternal GetTagCount;

        /// <summary>
        /// Returns the length of the tag value in bytes.
        /// </summary>
        public static GetTagLengthInternal GetTagLength;

        /// <summary>
        /// Returns the tag value.
        /// It is up to the programmer to interpret the returned pointer correctly,
        /// according to the results of GetTagType and GetTagCount.
        /// </summary>
        public static GetTagValueInternal GetTagValue;

        /// <summary>
        /// Sets the tag field name.
        /// </summary>
        public static SetTagKeyInternal SetTagKey;

        /// <summary>
        /// Sets the tag description.
        /// </summary>
        public static SetTagDescriptionInternal SetTagDescription;

        /// <summary>
        /// Sets the tag ID.
        /// </summary>
        public static SetTagIDInternal SetTagID;

        /// <summary>
        /// Sets the tag data type.
        /// </summary>
        public static SetTagTypeInternal SetTagType;

        /// <summary>
        /// Sets the number of data in the tag.
        /// </summary>
        public static SetTagCountInternal SetTagCount;

        /// <summary>
        /// Sets the length of the tag value in bytes.
        /// </summary>
        public static SetTagLengthInternal SetTagLength;

        /// <summary>
        /// Sets the tag value.
        /// </summary>
        public static SetTagValueInternal SetTagValue;

        /// <summary>
        /// Provides information about the first instance of a tag that matches the metadata model.
        /// </summary>
        public static FindFirstMetadataInternal FindFirstMetadata_;

        /// <summary>
        /// Find the next tag, if any, that matches the metadata model argument in a previous call
        /// to FindFirstMetadata, and then alters the tag object contents accordingly.
        /// </summary>
        public static FindNextMetadataInternal FindNextMetadata_;

        /// <summary>
        /// Closes the specified metadata search handle and releases associated resources.
        /// </summary>
        private static FindCloseMetadata_Internal FindCloseMetadata_;

        /// <summary>
        /// Retrieve a metadata attached to a dib.
        /// </summary>
        public static GetMetadataInternal GetMetadata_;

        /// <summary>
        /// Attach a new FreeImage tag to a dib.
        /// </summary>
        public static SetMetadataInternal SetMetadata_;

        /// <summary>
        /// Returns the number of tags contained in the model metadata model attached to the input dib.
        /// </summary>
        public static GetMetadataCountInternal GetMetadataCount;

        /// <summary>
        /// Copies the metadata of FreeImage bitmap to another.
        /// </summary>
        public static CloneMetadataInternal CloneMetadata;

        private static TagToString_Internal TagToString_;

        public static RotateInternal Rotate_;
        public static RotateExInternal RotateEx;

        /// <summary>
        /// Flip the input dib horizontally along the vertical axis.
        /// </summary>
        public static FlipHorizontalInternal FlipHorizontal;

        /// <summary>
        /// Flip the input dib vertically along the horizontal axis.
        /// </summary>
        public static FlipVerticalInternal FlipVertical;

        public static JPEGTransformAInternal JPEGTransformA;
        public static JPEGTransformUInternal JPEGTransformU;

        /// <summary>
        /// Performs resampling (or scaling, zooming) of a greyscale or RGB(A) image
        /// to the desired destination width and height.
        /// </summary>
        public static RescaleInternal Rescale;

        /// <summary>
        /// Creates a thumbnail from a greyscale or RGB(A) image, keeping aspect ratio.
        /// </summary>
        public static MakeThumbnailInternal MakeThumbnail;

        public static EnlargeCanvasInternal EnlargeCanvas_;

        /// <summary>
        /// Perfoms an histogram transformation on a 8-, 24- or 32-bit image.
        /// </summary>
        public static AdjustCurveInternal AdjustCurve;

        /// <summary>
        /// Performs gamma correction on a 8-, 24- or 32-bit image.
        /// </summary>
        public static AdjustGammaInternal AdjustGamma;

        /// <summary>
        /// Adjusts the brightness of a 8-, 24- or 32-bit image by a certain amount.
        /// </summary>
        public static AdjustBrightnessInternal AdjustBrightness;

        /// <summary>
        /// Adjusts the contrast of a 8-, 24- or 32-bit image by a certain amount.
        /// </summary>
        public static AdjustContrastInternal AdjustContrast;

        /// <summary>
        /// Inverts each pixel data.
        /// </summary>
        public static InvertInternal Invert;

        /// <summary>
        /// Computes the image histogram.
        /// </summary>
        public static GetHistogramInternal GetHistogram;

        /// <summary>
        /// Retrieves the red, green, blue or alpha channel of a 24- or 32-bit image.
        /// </summary>
        public static GetChannelInternal GetChannel;

        /// <summary>
        /// Insert a 8-bit dib into a 24- or 32-bit image.
        /// Both images must have to same width and height.
        /// </summary>
        public static SetChannelInternal SetChannel;

        /// <summary>
        /// Retrieves the real part, imaginary part, magnitude or phase of a complex image.
        /// </summary>
        public static GetComplexChannelInternal GetComplexChannel;

        /// <summary>
        /// Set the real or imaginary part of a complex image.
        /// Both images must have to same width and height.
        /// </summary>
        public static SetComplexChannelInternal SetComplexChannel;

        /// <summary>
        /// Copy a sub part of the current dib image.
        /// </summary>
        public static CopyInternal Copy;

        /// <summary>
        /// Alpha blend or combine a sub part image with the current dib image.
        /// The bit depth of the dst bitmap must be greater than or equal to the bit depth of the src.
        /// </summary>
        public static PasteInternal Paste;

        /// <summary>
        /// This function composite a transparent foreground image against a single background color or
        /// against a background image.
        /// </summary>
        public static CompositeInternal Composite;

        /// <summary>
        /// This function composite a transparent foreground image against a single background color or
        /// against a background image.
        /// </summary>
        public static CompositeInternalArray CompositeArray;

        /// <summary>
        /// Performs a lossless crop on a JPEG file.
        /// </summary>
        public static JPEGCropAInternal JPEGCropA;

        /// <summary>
        /// Performs a lossless crop on a JPEG file.
        /// Supports UNICODE filenames.
        /// </summary>
        public static JPEGCropUInternal JPEGCropU;

        /// <summary>
        /// Applies the alpha value of each pixel to its color components.
        /// The aplha value stays unchanged.
        /// Only works with 32-bits color depth.
        /// </summary>
        public static PreMultiplyWithAlphaInternal PreMultiplyWithAlpha;

        /// <summary>
        /// Solves a Poisson equation, remap result pixels to [0..1] and returns the solution.
        /// </summary>
        public static MultigridPoissonSolverInternal MultigridPoissonSolver;

        /// <summary>
        /// Creates a lookup table to be used with <see cref="AdjustCurve" /> which may adjusts brightness and
        /// contrast, correct gamma and invert the image with a single call to <see cref="AdjustCurve" />.
        /// </summary>
        public static GetAdjustColorsLookupTableInternal GetAdjustColorsLookupTable;

        /// <summary>
        /// Adjusts an image's brightness, contrast and gamma as well as it may
        /// optionally invert the image within a single operation.
        /// </summary>
        public static AdjustColorsInternal AdjustColors;

        /// <summary>
        /// Applies color mapping for one or several colors on a 1-, 4- or 8-bit
        /// palletized or a 16-, 24- or 32-bit high color image.
        /// </summary>
        public static ApplyColorMappingInternal ApplyColorMapping;

        /// <summary>
        /// Swaps two specified colors on a 1-, 4- or 8-bit palletized
        /// or a 16-, 24- or 32-bit high color image.
        /// </summary>
        public static SwapColorsInternal SwapColors;

        /// <summary>
        /// Applies palette index mapping for one or several indices
        /// on a 1-, 4- or 8-bit palletized image.
        /// </summary>
        public static ApplyPaletteIndexMappingInternal ApplyPaletteIndexMapping;

        /// <summary>
        /// Swaps two specified palette indices on a 1-, 4- or 8-bit palletized image.
        /// </summary>
        public static SwapPaletteIndicesInternal SwapPaletteIndices;

        public static FillBackgroundInternal FillBackground_;

        #endregion

        /// <summary>
        /// Initialize the library.
        /// </summary>
        /// <param name="lib">A pointer to the loaded library using the NativeLibrary class.</param>
        /// <returns>Whether initialization was successful.</returns>
        public static int Init(IntPtr lib)
        {
            GetFuncPointer(lib, "FreeImage_Initialise", ref Initialise);
            GetFuncPointer(lib, "FreeImage_DeInitialise", ref DeInitialise);
            GetFuncPointer(lib, "FreeImage_GetVersion", ref GetVersion_);
            GetFuncPointer(lib, "FreeImage_GetCopyrightMessage", ref GetCopyrightMessage_);
            GetFuncPointer(lib, "FreeImage_OutputMessageProc", ref OutputMessageProc);
            GetFuncPointer(lib, "FreeImage_SetOutputMessage", ref SetOutputMessage);
            GetFuncPointer(lib, "FreeImage_Allocate", ref Allocate_);
            GetFuncPointer(lib, "FreeImage_AllocateT", ref AllocateT_);
            GetFuncPointer(lib, "FreeImage_AllocateEx", ref AllocateEx_);
            GetFuncPointer(lib, "FreeImage_AllocateExT", ref AllocateExT_);
            GetFuncPointer(lib, "FreeImage_Clone", ref Clone);
            GetFuncPointer(lib, "FreeImage_Unload", ref Unload);
            GetFuncPointer(lib, "FreeImage_Load", ref LoadA);
            GetFuncPointer(lib, "FreeImage_LoadU", ref LoadU);
            GetFuncPointer(lib, "FreeImage_LoadFromHandle", ref LoadFromHandle);
            GetFuncPointer(lib, "FreeImage_Save", ref SaveA);
            GetFuncPointer(lib, "FreeImage_SaveU", ref SaveU);
            GetFuncPointer(lib, "FreeImage_SaveToHandle", ref SaveToHandle);
            GetFuncPointer(lib, "FreeImage_OpenMemory", ref OpenMemory);
            GetFuncPointer(lib, "FreeImage_OpenMemory", ref OpenMemoryEx);
            GetFuncPointer(lib, "FreeImage_CloseMemory", ref CloseMemory);
            GetFuncPointer(lib, "FreeImage_LoadFromMemory", ref LoadFromMemory);
            GetFuncPointer(lib, "FreeImage_SaveToMemory", ref SaveToMemory);
            GetFuncPointer(lib, "FreeImage_TellMemory", ref TellMemory);
            GetFuncPointer(lib, "FreeImage_SeekMemory", ref SeekMemory);
            GetFuncPointer(lib, "FreeImage_AcquireMemory", ref AcquireMemory);
            GetFuncPointer(lib, "FreeImage_ReadMemory", ref ReadMemory);
            GetFuncPointer(lib, "FreeImage_WriteMemory", ref WriteMemory);
            GetFuncPointer(lib, "FreeImage_LoadMultiBitmapFromMemory", ref LoadMultiBitmapFromMemory);
            GetFuncPointer(lib, "FreeImage_RegisterLocalPlugin", ref RegisterLocalPlugin);
            GetFuncPointer(lib, "FreeImage_RegisterExternalPlugin", ref RegisterExternalPlugin);
            GetFuncPointer(lib, "FreeImage_GetFIFCount", ref GetFIFCount);
            GetFuncPointer(lib, "FreeImage_SetPluginEnabled", ref SetPluginEnabled);
            GetFuncPointer(lib, "FreeImage_IsPluginEnabled", ref IsPluginEnabled);
            GetFuncPointer(lib, "FreeImage_GetFIFFromFormat", ref GetFIFFromFormat);
            GetFuncPointer(lib, "FreeImage_GetFIFFromMime", ref GetFIFFromMime);
            GetFuncPointer(lib, "FreeImage_GetFormatFromFIF", ref GetFormatFromFIF_);
            GetFuncPointer(lib, "FreeImage_GetFIFExtensionList", ref GetFIFExtensionList_);
            GetFuncPointer(lib, "FreeImage_GetFIFDescription", ref GetFIFDescription_);
            GetFuncPointer(lib, "FreeImage_GetFIFRegExpr", ref GetFIFRegExpr_);
            GetFuncPointer(lib, "FreeImage_GetFIFMimeType", ref GetFIFMimeType_);
            GetFuncPointer(lib, "FreeImage_GetFIFFromFilename", ref GetFIFFromFilenameA);
            GetFuncPointer(lib, "FreeImage_GetFIFFromFilenameU", ref GetFIFFromFilenameU);
            GetFuncPointer(lib, "FreeImage_FIFSupportsReading", ref FIFSupportsReading);
            GetFuncPointer(lib, "FreeImage_FIFSupportsWriting", ref FIFSupportsWriting);
            GetFuncPointer(lib, "FreeImage_FIFSupportsExportBPP", ref FIFSupportsExportBPP);
            GetFuncPointer(lib, "FreeImage_FIFSupportsExportType", ref FIFSupportsExportType);
            GetFuncPointer(lib, "FreeImage_FIFSupportsICCProfiles", ref FIFSupportsICCProfiles);
            GetFuncPointer(lib, "FreeImage_OpenMultiBitmap", ref OpenMultiBitmap);
            GetFuncPointer(lib, "FreeImage_OpenMultiBitmapFromHandle", ref OpenMultiBitmapFromHandle);
            GetFuncPointer(lib, "FreeImage_CloseMultiBitmap", ref CloseMultiBitmap_);
            GetFuncPointer(lib, "FreeImage_GetPageCount", ref GetPageCount);
            GetFuncPointer(lib, "FreeImage_AppendPage", ref AppendPage);
            GetFuncPointer(lib, "FreeImage_InsertPage", ref InsertPage);
            GetFuncPointer(lib, "FreeImage_DeletePage", ref DeletePage);
            GetFuncPointer(lib, "FreeImage_LockPage", ref LockPage);
            GetFuncPointer(lib, "FreeImage_UnlockPage", ref UnlockPage);
            GetFuncPointer(lib, "FreeImage_MovePage", ref MovePage);
            GetFuncPointer(lib, "FreeImage_GetLockedPageNumbers", ref GetLockedPageNumbers);
            GetFuncPointer(lib, "FreeImage_GetFileType", ref GetFileTypeA);
            GetFuncPointer(lib, "FreeImage_GetFileTypeU", ref GetFileTypeU);
            GetFuncPointer(lib, "FreeImage_GetFileTypeFromHandle", ref GetFileTypeFromHandle);
            GetFuncPointer(lib, "FreeImage_GetFileTypeFromMemory", ref GetFileTypeFromMemory);
            GetFuncPointer(lib, "FreeImage_IsLittleEndian", ref IsLittleEndian);
            GetFuncPointer(lib, "FreeImage_LookupX11Color", ref LookupX11Color);
            GetFuncPointer(lib, "FreeImage_LookupSVGColor", ref LookupSVGColor);
            GetFuncPointer(lib, "FreeImage_GetBits", ref GetBits);
            GetFuncPointer(lib, "FreeImage_GetScanLine", ref GetScanLine);
            GetFuncPointer(lib, "FreeImage_GetPixelIndex", ref GetPixelIndex);
            GetFuncPointer(lib, "FreeImage_GetPixelColor", ref GetPixelColor);
            GetFuncPointer(lib, "FreeImage_SetPixelIndex", ref SetPixelIndex);
            GetFuncPointer(lib, "FreeImage_SetPixelColor", ref SetPixelColor);
            GetFuncPointer(lib, "FreeImage_GetImageType", ref GetImageType);
            GetFuncPointer(lib, "FreeImage_GetColorsUsed", ref GetColorsUsed);
            GetFuncPointer(lib, "FreeImage_GetBPP", ref GetBPP);
            GetFuncPointer(lib, "FreeImage_GetWidth", ref GetWidth);
            GetFuncPointer(lib, "FreeImage_GetHeight", ref GetHeight);
            GetFuncPointer(lib, "FreeImage_GetLine", ref GetLine);
            GetFuncPointer(lib, "FreeImage_GetPitch", ref GetPitch);
            GetFuncPointer(lib, "FreeImage_GetDIBSize", ref GetDIBSize);
            GetFuncPointer(lib, "FreeImage_GetPalette", ref GetPalette);
            GetFuncPointer(lib, "FreeImage_GetDotsPerMeterX", ref GetDotsPerMeterX);
            GetFuncPointer(lib, "FreeImage_GetDotsPerMeterY", ref GetDotsPerMeterY);
            GetFuncPointer(lib, "FreeImage_SetDotsPerMeterX", ref SetDotsPerMeterX);
            GetFuncPointer(lib, "FreeImage_SetDotsPerMeterY", ref SetDotsPerMeterY);
            GetFuncPointer(lib, "FreeImage_GetInfoHeader", ref GetInfoHeader);
            GetFuncPointer(lib, "FreeImage_GetInfo", ref GetInfo);
            GetFuncPointer(lib, "FreeImage_GetColorType", ref GetColorType);
            GetFuncPointer(lib, "FreeImage_GetRedMask", ref GetRedMask);
            GetFuncPointer(lib, "FreeImage_GetGreenMask", ref GetGreenMask);
            GetFuncPointer(lib, "FreeImage_GetBlueMask", ref GetBlueMask);
            GetFuncPointer(lib, "FreeImage_GetTransparencyCount", ref GetTransparencyCount);
            GetFuncPointer(lib, "FreeImage_GetTransparencyTable", ref GetTransparencyTable);
            GetFuncPointer(lib, "FreeImage_SetTransparent", ref SetTransparent);
            GetFuncPointer(lib, "FreeImage_SetTransparencyTable", ref SetTransparencyTable_);
            GetFuncPointer(lib, "FreeImage_IsTransparent", ref IsTransparent);
            GetFuncPointer(lib, "FreeImage_HasBackgroundColor", ref HasBackgroundColor);
            GetFuncPointer(lib, "FreeImage_GetBackgroundColor", ref GetBackgroundColor);
            GetFuncPointer(lib, "FreeImage_SetBackgroundColor", ref SetBackgroundColor);
            GetFuncPointer(lib, "FreeImage_SetBackgroundColor", ref SetBackgroundColorArray);
            GetFuncPointer(lib, "FreeImage_SetTransparentIndex", ref SetTransparentIndex);
            GetFuncPointer(lib, "FreeImage_GetTransparentIndex", ref GetTransparentIndex);
            GetFuncPointer(lib, "FreeImage_GetICCProfile", ref GetICCProfile);
            GetFuncPointer(lib, "FreeImage_CreateICCProfile", ref CreateICCProfile);
            GetFuncPointer(lib, "FreeImage_DestroyICCProfile", ref DestroyICCProfile);
            GetFuncPointer(lib, "FreeImage_ConvertTo4Bits", ref ConvertTo4Bits);
            GetFuncPointer(lib, "FreeImage_ConvertTo8Bits", ref ConvertTo8Bits);
            GetFuncPointer(lib, "FreeImage_ConvertToGreyscale", ref ConvertToGreyscale);
            GetFuncPointer(lib, "FreeImage_ConvertTo16Bits555", ref ConvertTo16Bits555);
            GetFuncPointer(lib, "FreeImage_ConvertTo16Bits565", ref ConvertTo16Bits565);
            GetFuncPointer(lib, "FreeImage_ConvertTo24Bits", ref ConvertTo24Bits);
            GetFuncPointer(lib, "FreeImage_ConvertTo32Bits", ref ConvertTo32Bits);
            GetFuncPointer(lib, "FreeImage_ColorQuantize", ref ColorQuantize);
            GetFuncPointer(lib, "FreeImage_ColorQuantizeEx", ref ColorQuantizeEx_);
            GetFuncPointer(lib, "FreeImage_Threshold", ref Threshold);
            GetFuncPointer(lib, "FreeImage_Dither", ref Dither);
            GetFuncPointer(lib, "FreeImage_ConvertFromRawBits", ref ConvertFromRawBits_);
            GetFuncPointer(lib, "FreeImage_ConvertFromRawBits", ref ConvertFromRawBitsArray);
            GetFuncPointer(lib, "FreeImage_ConvertToRawBits", ref ConvertToRawBits);
            GetFuncPointer(lib, "FreeImage_ConvertToRawBits", ref ConvertToRawBitsArray);
            GetFuncPointer(lib, "FreeImage_ConvertToRGBF", ref ConvertToRGBF);
            GetFuncPointer(lib, "FreeImage_ConvertToStandardType", ref ConvertToStandardType);
            GetFuncPointer(lib, "FreeImage_ConvertToType", ref ConvertToType);
            GetFuncPointer(lib, "FreeImage_ToneMapping", ref ToneMapping);
            GetFuncPointer(lib, "FreeImage_TmoDrago03", ref TmoDrago03);
            GetFuncPointer(lib, "FreeImage_TmoReinhard05", ref TmoReinhard05);
            GetFuncPointer(lib, "FreeImage_TmoFattal02", ref TmoFattal02);
            GetFuncPointer(lib, "FreeImage_ZLibCompress", ref ZLibCompress);
            GetFuncPointer(lib, "FreeImage_ZLibUncompress", ref ZLibUncompress);
            GetFuncPointer(lib, "FreeImage_ZLibGZip", ref ZLibGZip);
            GetFuncPointer(lib, "FreeImage_ZLibGUnzip", ref ZLibGUnzip);
            GetFuncPointer(lib, "FreeImage_ZLibCRC32", ref ZLibCRC32);
            GetFuncPointer(lib, "FreeImage_CreateTag", ref CreateTag);
            GetFuncPointer(lib, "FreeImage_DeleteTag", ref DeleteTag);
            GetFuncPointer(lib, "FreeImage_CloneTag", ref CloneTag);
            GetFuncPointer(lib, "FreeImage_GetTagKey", ref GetTagKey_);
            GetFuncPointer(lib, "FreeImage_GetTagDescription", ref GetTagDescription_);
            GetFuncPointer(lib, "FreeImage_GetTagID", ref GetTagID);
            GetFuncPointer(lib, "FreeImage_GetTagType", ref GetTagType);
            GetFuncPointer(lib, "FreeImage_GetTagCount", ref GetTagCount);
            GetFuncPointer(lib, "FreeImage_GetTagLength", ref GetTagLength);
            GetFuncPointer(lib, "FreeImage_GetTagValue", ref GetTagValue);
            GetFuncPointer(lib, "FreeImage_SetTagKey", ref SetTagKey);
            GetFuncPointer(lib, "FreeImage_SetTagDescription", ref SetTagDescription);
            GetFuncPointer(lib, "FreeImage_SetTagID", ref SetTagID);
            GetFuncPointer(lib, "FreeImage_SetTagType", ref SetTagType);
            GetFuncPointer(lib, "FreeImage_SetTagCount", ref SetTagCount);
            GetFuncPointer(lib, "FreeImage_SetTagLength", ref SetTagLength);
            GetFuncPointer(lib, "FreeImage_SetTagValue", ref SetTagValue);
            GetFuncPointer(lib, "FreeImage_FindFirstMetadata", ref FindFirstMetadata_);
            GetFuncPointer(lib, "FreeImage_FindNextMetadata", ref FindNextMetadata_);
            GetFuncPointer(lib, "FreeImage_FindCloseMetadata", ref FindCloseMetadata_);
            GetFuncPointer(lib, "FreeImage_GetMetadata", ref GetMetadata_);
            GetFuncPointer(lib, "FreeImage_SetMetadata", ref SetMetadata_);
            GetFuncPointer(lib, "FreeImage_GetMetadataCount", ref GetMetadataCount);
            GetFuncPointer(lib, "FreeImage_CloneMetadata", ref CloneMetadata);
            GetFuncPointer(lib, "FreeImage_TagToString", ref TagToString_);
            GetFuncPointer(lib, "FreeImage_Rotate", ref Rotate_);
            GetFuncPointer(lib, "FreeImage_RotateEx", ref RotateEx);
            GetFuncPointer(lib, "FreeImage_FlipHorizontal", ref FlipHorizontal);
            GetFuncPointer(lib, "FreeImage_FlipVertical", ref FlipVertical);
            GetFuncPointer(lib, "FreeImage_JPEGTransform", ref JPEGTransformA);
            GetFuncPointer(lib, "FreeImage_JPEGTransformU", ref JPEGTransformU);
            GetFuncPointer(lib, "FreeImage_Rescale", ref Rescale);
            GetFuncPointer(lib, "FreeImage_MakeThumbnail", ref MakeThumbnail);
            GetFuncPointer(lib, "FreeImage_EnlargeCanvas", ref EnlargeCanvas_);
            GetFuncPointer(lib, "FreeImage_AdjustCurve", ref AdjustCurve);
            GetFuncPointer(lib, "FreeImage_AdjustGamma", ref AdjustGamma);
            GetFuncPointer(lib, "FreeImage_AdjustBrightness", ref AdjustBrightness);
            GetFuncPointer(lib, "FreeImage_AdjustContrast", ref AdjustContrast);
            GetFuncPointer(lib, "FreeImage_Invert", ref Invert);
            GetFuncPointer(lib, "FreeImage_GetHistogram", ref GetHistogram);
            GetFuncPointer(lib, "FreeImage_GetChannel", ref GetChannel);
            GetFuncPointer(lib, "FreeImage_SetChannel", ref SetChannel);
            GetFuncPointer(lib, "FreeImage_GetComplexChannel", ref GetComplexChannel);
            GetFuncPointer(lib, "FreeImage_SetComplexChannel", ref SetComplexChannel);
            GetFuncPointer(lib, "FreeImage_Copy", ref Copy);
            GetFuncPointer(lib, "FreeImage_Paste", ref Paste);
            GetFuncPointer(lib, "FreeImage_Composite", ref Composite);
            GetFuncPointer(lib, "FreeImage_Composite", ref CompositeArray);
            GetFuncPointer(lib, "FreeImage_JPEGCrop", ref JPEGCropA);
            GetFuncPointer(lib, "FreeImage_JPEGCropU", ref JPEGCropU);
            GetFuncPointer(lib, "FreeImage_PreMultiplyWithAlpha", ref PreMultiplyWithAlpha);
            GetFuncPointer(lib, "FreeImage_MultigridPoissonSolver", ref MultigridPoissonSolver);
            GetFuncPointer(lib, "FreeImage_GetAdjustColorsLookupTable", ref GetAdjustColorsLookupTable);
            GetFuncPointer(lib, "FreeImage_AdjustColors", ref AdjustColors);
            GetFuncPointer(lib, "FreeImage_ApplyColorMapping", ref ApplyColorMapping);
            GetFuncPointer(lib, "FreeImage_SwapColors", ref SwapColors);
            GetFuncPointer(lib, "FreeImage_ApplyPaletteIndexMapping", ref ApplyPaletteIndexMapping);
            GetFuncPointer(lib, "FreeImage_SwapPaletteIndices", ref SwapPaletteIndices);
            GetFuncPointer(lib, "FreeImage_FillBackground", ref FillBackground_);


            return 1;
        }

        /// <summary>
        /// Load a function from the library.
        /// </summary>
        /// <typeparam name="TDelegate">The delegate type to load the function as.</typeparam>
        /// <param name="lib">Pointer to the library.</param>
        /// <param name="name">The name of the function.</param>
        /// <param name="funcDel">The delegate variable which to load the function into.</param>
        internal static void GetFuncPointer<TDelegate>(IntPtr lib, string name, ref TDelegate funcDel)
        {
            bool success = NativeLibrary.TryGetExport(lib, name, out IntPtr func);
            if (success) funcDel = Marshal.GetDelegateForFunctionPointer<TDelegate>(func);
        }

        /// <summary>
        /// Returns a string containing the current version of the library.
        /// </summary>
        /// <returns>The current version of the library.</returns>
        public static unsafe string GetVersion()
        {
            return PtrToStr(GetVersion_());
        }

        /// <summary>
        /// Returns a string containing a standard copyright message.
        /// </summary>
        /// <returns>A standard copyright message.</returns>
        public static unsafe string GetCopyrightMessage()
        {
            return PtrToStr(GetCopyrightMessage_());
        }

        /// <summary>
        /// Decodes a bitmap, allocates memory for it and returns it as a FIBITMAP.
        /// </summary>
        /// <param name="fif">Type of the bitmap.</param>
        /// <param name="filename">Name of the file to decode.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Handle to a FreeImage bitmap.</returns>
        public static FIBITMAP Load(FREE_IMAGE_FORMAT fif, string filename, FREE_IMAGE_LOAD_FLAGS flags)
        {
            return LoadA(fif, filename, flags);
        }

        /// <summary>
        /// Saves a previosly loaded FIBITMAP to a file.
        /// </summary>
        /// <param name="fif">Type of the bitmap.</param>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <param name="filename">Name of the file to save to.</param>
        /// <param name="flags">Flags to enable or disable plugin-features.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public static bool Save(FREE_IMAGE_FORMAT fif, FIBITMAP dib, string filename, FREE_IMAGE_SAVE_FLAGS flags)
        {
            return SaveA(fif, dib, filename, flags);
        }

        /// <summary>
        /// Returns the string that was used to register a plugin from the system assigned <see cref="FREE_IMAGE_FORMAT" />.
        /// </summary>
        /// <param name="fif">The assigned <see cref="FREE_IMAGE_FORMAT" />.</param>
        /// <returns>The string that was used to register the plugin.</returns>
        public static unsafe string GetFormatFromFIF(FREE_IMAGE_FORMAT fif)
        {
            return PtrToStr(GetFormatFromFIF_(fif));
        }

        /// <summary>
        /// Returns a comma-delimited file extension list describing the bitmap formats the given plugin can read and/or write.
        /// </summary>
        /// <param name="fif">The desired <see cref="FREE_IMAGE_FORMAT" />.</param>
        /// <returns>A comma-delimited file extension list.</returns>
        public static unsafe string GetFIFExtensionList(FREE_IMAGE_FORMAT fif)
        {
            return PtrToStr(GetFIFExtensionList_(fif));
        }

        /// <summary>
        /// Returns a descriptive string that describes the bitmap formats the given plugin can read and/or write.
        /// </summary>
        /// <param name="fif">The desired <see cref="FREE_IMAGE_FORMAT" />.</param>
        /// <returns>A descriptive string that describes the bitmap formats.</returns>
        public static unsafe string GetFIFDescription(FREE_IMAGE_FORMAT fif)
        {
            return PtrToStr(GetFIFDescription_(fif));
        }

        /// <summary>
        /// Returns a regular expression string that can be used by a regular expression engine to identify the bitmap.
        /// FreeImageQt makes use of this function.
        /// </summary>
        /// <param name="fif">The desired <see cref="FREE_IMAGE_FORMAT" />.</param>
        /// <returns>A regular expression string.</returns>
        public static unsafe string GetFIFRegExpr(FREE_IMAGE_FORMAT fif)
        {
            return PtrToStr(GetFIFRegExpr_(fif));
        }

        /// <summary>
        /// Given a <see cref="FREE_IMAGE_FORMAT" /> identifier, returns a MIME content type string (MIME stands for Multipurpose
        /// Internet Mail Extension).
        /// </summary>
        /// <param name="fif">The desired <see cref="FREE_IMAGE_FORMAT" />.</param>
        /// <returns>A MIME content type string.</returns>
        public static unsafe string GetFIFMimeType(FREE_IMAGE_FORMAT fif)
        {
            return PtrToStr(GetFIFMimeType_(fif));
        }

        /// <summary>
        /// This function takes a filename or a file-extension and returns the plugin that can
        /// read/write files with that extension in the form of a <see cref="FREE_IMAGE_FORMAT" /> identifier.
        /// </summary>
        /// <param name="filename">The filename or -extension.</param>
        /// <returns>The <see cref="FREE_IMAGE_FORMAT" /> of the plugin.</returns>
        public static FREE_IMAGE_FORMAT GetFIFFromFilename(string filename)
        {
            return GetFIFFromFilenameA(filename);
        }

        /// <summary>
        /// Orders FreeImage to analyze the bitmap signature.
        /// </summary>
        /// <param name="filename">Name of the file to analyze.</param>
        /// <param name="size">Reserved parameter - use 0.</param>
        /// <returns>Type of the bitmap.</returns>
        public static FREE_IMAGE_FORMAT GetFileType(string filename, int size)
        {;
            return GetFileTypeA(filename, size);
        }

        /// <summary>
        /// Retrieves the <see cref="FIICCPROFILE" /> data of the bitmap.
        /// This function can also be called safely, when the original format does not support profiles.
        /// </summary>
        /// <param name="dib">Handle to a FreeImage bitmap.</param>
        /// <returns>The <see cref="FIICCPROFILE" /> data of the bitmap.</returns>
        public static FIICCPROFILE GetICCProfileEx(FIBITMAP dib)
        {
            unsafe
            {
                return *(FIICCPROFILE*) GetICCProfile(dib);
            }
        }

        /// <summary>
        /// Returns the tag field name (unique inside a metadata model).
        /// </summary>
        /// <param name="tag">The tag field.</param>
        /// <returns>The field name.</returns>
        public static unsafe string GetTagKey(FITAG tag)
        {
            return PtrToStr(GetTagKey_(tag));
        }

        /// <summary>
        /// Returns the tag description.
        /// </summary>
        /// <param name="tag">The tag field.</param>
        /// <returns>The description or NULL if unavailable.</returns>
        public static unsafe string GetTagDescription(FITAG tag)
        {
            return PtrToStr(GetTagDescription_(tag));
        }

        /// <summary>
        /// Converts a FreeImage tag structure to a string that represents the interpreted tag value.
        /// The function is not thread safe.
        /// </summary>
        /// <param name="model">The metadata model.</param>
        /// <param name="tag">The interpreted tag value.</param>
        /// <param name="Make">Reserved.</param>
        /// <returns>The representing string.</returns>
        public static unsafe string TagToString(FREE_IMAGE_MDMODEL model, FITAG tag, uint Make)
        {
            return PtrToStr(TagToString_(model, tag, Make));
        }

        /// <summary>
        /// Performs a lossless rotation or flipping on a JPEG file.
        /// </summary>
        /// <param name="src_file">Source file.</param>
        /// <param name="dst_file">Destination file; can be the source file; will be overwritten.</param>
        /// <param name="operation">The operation to apply.</param>
        /// <param name="perfect">To avoid lossy transformation, you can set the perfect parameter to true.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public static bool JPEGTransform(string src_file, string dst_file,
            FREE_IMAGE_JPEG_OPERATION operation, bool perfect)
        {
            return JPEGTransformA(src_file, dst_file, operation, perfect);
        }

        /// <summary>
        /// Performs a lossless crop on a JPEG file.
        /// </summary>
        /// <param name="src_file">Source filename.</param>
        /// <param name="dst_file">Destination filename.</param>
        /// <param name="left">Specifies the left position of the cropped rectangle.</param>
        /// <param name="top">Specifies the top position of the cropped rectangle.</param>
        /// <param name="right">Specifies the right position of the cropped rectangle.</param>
        /// <param name="bottom">Specifies the bottom position of the cropped rectangle.</param>
        /// <returns>Returns true on success, false on failure.</returns>
        public static bool JPEGCrop(string src_file, string dst_file, int left, int top, int right, int bottom)
        {
            return JPEGCropA(src_file, dst_file, left, top, right, bottom);
        }
    }
}