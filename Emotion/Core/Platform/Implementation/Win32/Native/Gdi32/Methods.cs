#nullable enable

#region Using

using Emotion;
using Emotion.Core.Platform.Implementation.Win32.Native;
using Emotion.Core.Platform.Implementation.Win32.Native.User32;
using System.Runtime.InteropServices;

#endregion

#pragma warning disable 1591 // Documentation for this file is found at msdn

namespace Emotion.Core.Platform.Implementation.Win32.Native.Gdi32;

public static class Gdi32
{
    public const string LibraryName = "gdi32";

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint GetStockObject(int fnObject);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern uint GetPixel(nint hdc, int nXPos, int nYPos);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool DeleteObject(nint hObject);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern uint SetPixel(nint hdc, int x, int y, uint crColor);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int GetPixelFormat(nint hdc);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int SetBkMode(nint hdc, int iBkMode);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateDIBSection(nint hdc, nint pbmi,
        DibBmiColorUsageFlag iUsage, out nint ppvBits, nint hSection, uint dwOffset);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateCompatibleDC(nint hdc);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern int GetObject(nint hgdiobj, int cbBuffer, nint lpvObject);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateCompatibleBitmap(nint hdc, int nWidth, int nHeight);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateBitmap(int nWidth, int nHeight, uint cPlanes, uint cBitsPerPel, nint lpvBits);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateBitmapIndirect([In] ref Bitmap lpbm);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateDIBitmap(nint hdc, [In] ref BitmapInfoHeader
            lpbmih, uint fdwInit, byte[] lpbInit, nint lpbmi,
        DibBmiColorUsageFlag fuUsage);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateDIBitmap(nint hdc, [In] ref BitmapInfoHeader
            lpbmih, uint fdwInit, nint lpbInit, nint lpbmi,
        DibBmiColorUsageFlag fuUsage);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int SetDIBitsToDevice(nint hdc, int xDest, int yDest, uint
            dwWidth, uint dwHeight, int xSrc, int ySrc, uint uStartScan, uint cScanLines,
        byte[] lpvBits, nint lpbmi, DibBmiColorUsageFlag fuColorUse);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int SetDIBitsToDevice(nint hdc, int xDest, int yDest, uint
            dwWidth, uint dwHeight, int xSrc, int ySrc, uint uStartScan, uint cScanLines,
        nint lpvBits, nint lpbmi, DibBmiColorUsageFlag fuColorUse);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool DeleteDC(nint hdc);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int SetBitmapBits(nint hbmp, uint cBytes, [In] byte[] lpBits);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int GetBitmapBits(nint hbmp, int cbBuffer, [Out] byte[] lpvBits);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int SetBitmapBits(nint hbmp, uint cBytes, nint lpBits);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int GetBitmapBits(nint hbmp, int cbBuffer, nint lpvBits);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateRectRgnIndirect([In] ref Rect lprc);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateEllipticRgn(int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateEllipticRgnIndirect([In] ref Rect lprc);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateRoundRectRgn(int x1, int y1, int x2, int y2,
        int cx, int cy);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int CombineRgn(nint hrgnDest, nint hrgnSrc1,
        nint hrgnSrc2, RegionModeFlags fnCombineMode);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool OffsetViewportOrgEx(nint hdc, int nXOffset, int nYOffset, out Point lpPoint);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool SetViewportOrgEx(nint hdc, int x, int y, out Point lpPoint);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int SetMapMode(nint hdc, int fnMapMode);


    /// <summary>
    /// The GetClipBox function retrieves the dimensions of the tightest bounding rectangle that can be drawn around the
    /// current visible area on the device. The visible area is defined by the current clipping region or clip path, as
    /// well as any overlapping windows.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <param name="lprc">A pointer to a RECT structure that is to receive the rectangle dimensions, in logical units.</param>
    /// <returns>If the function succeeds, the return value specifies the clipping box's complexity.</returns>
    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern RegionType GetClipBox(nint hdc, out Rect lprc);

    /// <summary>
    /// The GetClipRgn function retrieves a handle identifying the current application-defined clipping region for the
    /// specified device context.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <param name="hrgn">
    /// A handle to an existing region before the function is called. After the function returns, this
    /// parameter is a handle to a copy of the current clipping region.
    /// </param>
    /// <returns>
    /// f the function succeeds and there is no clipping region for the given device context, the return value is
    /// zero. If the function succeeds and there is a clipping region for the given device context, the return value is 1.
    /// If an error occurs, the return value is -1.
    /// </returns>
    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int GetClipRgn(nint hdc, nint hrgn);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern RegionType SelectClipRgn(nint hdc, nint hrgn);

    /// <summary>
    /// The ExtSelectClipRgn function combines the specified region with the current clipping region using the specified
    /// mode.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <param name="hrgn">
    /// A handle to the region to be selected. This handle must not be NULL unless the RGN_COPY mode is
    /// specified.
    /// </param>
    /// <param name="fnMode">The operation to be performed</param>
    /// <returns>The return value specifies the new clipping region's complexity.</returns>
    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern RegionType ExtSelectClipRgn(nint hdc, nint hrgn, RegionModeFlags fnMode);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern RegionType IntersectClipRect(nint hdc,
        int nLeftRect,
        int nTopRect,
        int nRightRect,
        int nBottomRect
    );

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern RegionType ExcludeClipRect(nint hdc,
        int nLeftRect,
        int nTopRect,
        int nRightRect,
        int nBottomRect
    );

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern RegionType OffsetClipRgn(nint hdc,
        int nXOffset,
        int nYOffset
    );

    /// <summary>
    /// The GetMetaRgn function retrieves the current metaregion for the specified device context.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <param name="hrgn">
    /// A handle to an existing region before the function is called. After the function returns, this
    /// parameter is a handle to a copy of the current metaregion.
    /// </param>
    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool GetMetaRgn(nint hdc, nint hrgn);

    /// <summary>
    /// The SetMetaRgn function intersects the current clipping region for the specified device context with the current
    /// metaregion and saves the combined region as the new metaregion for the specified device context. The clipping
    /// region is reset to a null region.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <returns>The return value specifies the new clipping region's complexity.</returns>
    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern RegionType SetMetaRgn(nint hdc);

    /// <summary>
    /// The LPtoDP function converts logical coordinates into device coordinates. The conversion depends on the mapping
    /// mode of the device context, the settings of the origins and extents for the window and viewport, and the world
    /// transformation.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <param name="lpPoints">
    /// A pointer to an array of POINT structures. The x-coordinates and y-coordinates contained in each
    /// of the POINT structures will be transformed.
    /// </param>
    /// <param name="nCount">The number of points in the array.</param>
    /// <remarks>
    /// The LPtoDP function fails if the logical coordinates exceed 32 bits, or if the converted device coordinates exceed
    /// 27 bits. In the case of such an overflow, the results for all the points are undefined.
    /// LPtoDP calculates complex floating-point arithmetic, and it has a caching system for efficiency.Therefore, the
    /// conversion result of an initial call to LPtoDP might not exactly match the conversion result of a later call to
    /// LPtoDP.We recommend not to write code that relies on the exact match of the conversion results from multiple calls
    /// to LPtoDP even if the parameters that are passed to each call are identical.
    /// </remarks>
    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool LPtoDP(nint hdc, [In] [Out] ref Point lpPoints, int nCount);

    /// <summary>
    /// The DPtoLP function converts device coordinates into logical coordinates. The conversion depends on the mapping
    /// mode of the device context, the settings of the origins and extents for the window and viewport, and the world
    /// transformation.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <param name="lpPoints">
    /// A pointer to an array of POINT structures. The x-coordinates and y-coordinates contained in each
    /// of the POINT structures will be transformed.
    /// </param>
    /// <param name="nCount">The number of points in the array.</param>
    /// <remarks>
    /// The DPtoLP function fails if the device coordinates exceed 27 bits, or if the converted logical coordinates exceed
    /// 32 bits. In the case of such an overflow, the results for all the points are undefined.
    /// </remarks>
    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool DPtoLP(nint hdc, [In] [Out] ref Point lpPoints, int nCount);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool SelectClipPath(nint hdc, RegionModeFlags iMode);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool FillRgn(nint hdc, nint hrgn, nint hbr);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreateSolidBrush(uint crColor);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool FrameRgn(nint hdc, nint hrgn, nint hbr, int nWidth,
        int nHeight);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool PaintRgn(nint hdc, nint hrgn);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool InvertRgn(nint hdc, nint hrgn);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool LineTo(nint hdc, int nXEnd, int nYEnd);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool MoveToEx(nint hdc, int x, int y, out Point lpPoint);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool RoundRect(nint hdc, int nLeftRect, int nTopRect,
        int nRightRect, int nBottomRect, int nWidth, int nHeight);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint SelectObject(nint hdc, nint hgdiobj);

    [DllImport(LibraryName, CharSet = CharSet.Unicode)]
    public static extern bool TextOut(nint hdc, int nXStart, int nYStart,
        string lpString, int cbString);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool BitBlt(nint hdc, int nXDest, int nYDest, int nWidth, int nHeight,
        nint hdcSrc,
        int nXSrc, int nYSrc, BitBltFlags dwRop);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool StretchBlt(nint hdcDest, int nXOriginDest, int nYOriginDest,
        int nWidthDest, int nHeightDest,
        nint hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
        BitBltFlags dwRop);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int SaveDC(nint hdc);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern bool RestoreDC(nint hdc, int nSavedDc);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint PathToRegion(nint hdc);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreatePolygonRgn(
        Point[] lppt,
        int cPoints,
        int fnPolyFillMode);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern nint CreatePolyPolygonRgn(Point[] lppt,
        int[] lpPolyCounts,
        int nCount, int fnPolyFillMode);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int GetDeviceCaps(nint hdc, DeviceCap nIndex);

    [DllImport(LibraryName, ExactSpelling = true, EntryPoint = "GdiAlphaBlend")]
    public static extern bool AlphaBlend(nint hdcDest, int nXOriginDest, int nYOriginDest,
        int nWidthDest, int nHeightDest,
        nint hdcSrc, int nXOriginSrc, int nYOriginSrc, int nWidthSrc, int nHeightSrc,
        BlendFunction blendFunction);

    /// <summary>
    /// The GetRandomRgn function copies the system clipping region of a specified device context to a specific region.
    /// </summary>
    /// <param name="hdc">A handle to the device context.</param>
    /// <param name="hrgn">
    /// A handle to a region. Before the function is called, this identifies an existing region. After the
    /// function returns, this identifies a copy of the current system region. The old region identified by hrgn is
    /// overwritten.
    /// </param>
    /// <param name="iNum">This parameter must be SYSRGN.</param>
    /// <returns>
    /// If the function succeeds, the return value is 1. If the function fails, the return value is -1. If the region
    /// to be retrieved is NULL, the return value is 0. If the function fails or the region to be retrieved is NULL, hrgn
    /// is not initialized.
    /// </returns>
    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern int GetRandomRgn(nint hdc, nint hrgn, DcRegionType iNum);

    /// <summary>
    /// The OffsetRgn function moves a region by the specified offsets.
    /// </summary>
    /// <param name="hrgn">Handle to the region to be moved.</param>
    /// <param name="nXOffset">Specifies the number of logical units to move left or right.</param>
    /// <param name="nYOffset">Specifies the number of logical units to move up or down.</param>
    /// <returns>The return value specifies the new region's complexity. </returns>
    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern RegionType OffsetRgn(nint hrgn, int nXOffset, int nYOffset);

    [DllImport(LibraryName, ExactSpelling = true)]
    public static extern RegionType GetRgnBox(nint hWnd, out Rect lprc);

    #region Pixel Format

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-choosepixelformat
    /// </summary>
    [DllImport(LibraryName)]
    public static extern int ChoosePixelFormat(nint hdc, ref PixelFormatDescriptor pfd);

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-setpixelformat
    /// </summary>
    [DllImport(LibraryName)]
    public static extern bool SetPixelFormat(nint hdc, int format, ref PixelFormatDescriptor pfd);

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-describepixelformat
    /// </summary>
    [DllImport(LibraryName)]
    public static extern int DescribePixelFormat(
        nint hdc,
        int iPixelFormat,
        uint nBytes,
        ref PixelFormatDescriptor pfd
    );

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/win32/api/wingdi/nf-wingdi-describepixelformat
    /// </summary>
    [DllImport(LibraryName)]
    public static extern int DescribePixelFormat(
        nint hdc,
        int iPixelFormat,
        uint nBytes,
        nint pfd
    );

    #endregion

    #region DC

    /// <summary>
    /// https://docs.microsoft.com/en-us/windows/desktop/api/wingdi/nf-wingdi-createdcw
    /// </summary>
    [DllImport(LibraryName, CharSet = CharSet.Auto)]
    public static extern nint CreateDCW(
        [MarshalAs(UnmanagedType.LPWStr)] string pwszDriver,
        [MarshalAs(UnmanagedType.LPWStr)] string pwszDevice,
        [MarshalAs(UnmanagedType.LPWStr)] string pszPort,
        nint pdm
    );

    #endregion

    [DllImport(LibraryName)]
    public static extern bool SwapBuffers(nint hdc);
}