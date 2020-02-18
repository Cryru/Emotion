﻿#region Using

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Utility;
using Khronos;
using OpenGL;
using WinApi.Core;
using WinApi.Gdi32;
using User32 = WinApi.User32.User32Methods;
using Gdi32 = WinApi.Gdi32.Gdi32Methods;
using DwmApi = WinApi.DwmApi.DwmApiMethods;
using Kernel32 = WinApi.Kernel32.Kernel32Methods;

#endregion

namespace Emotion.Platform.Implementation.Win32.Wgl
{
    public sealed unsafe class WglGraphicsContext : GraphicsContext
    {
        private readonly IntPtr _openGlLibrary;
        private const int FlagNumberPixelFormatsArb = 0x2000;

        private readonly WglFunctions.WglDeleteContext _deleteContext;
        private readonly WglFunctions.WglGetProcAddress _getProcAddress;
        private readonly WglFunctions.WglGetCurrentDc _getCurrentDc;
        private readonly WglFunctions.WglGetCurrentContext _getCurrentContext;
        private readonly WglFunctions.WglMakeCurrent _makeCurrent;
        private readonly WglFunctions.GetExtensionsStringExt _getExtensionsStringExt;
        private readonly WglFunctions.GetExtensionsStringArb _getExtensionsStringArb;
        private readonly WglFunctions.SwapInternalExt _swapIntervalExt;
        private readonly WglFunctions.GetPixelFormatAttributes _getPixelFormatAttribivArb;

        private string[] _wglExtensions;
        private readonly bool _arbMultisample;
        private readonly bool _arbFramebufferSRgb;
        private readonly bool _extFramebufferSRgb;
        private readonly bool _arbPixelFormat;

        private readonly IntPtr _contextHandle;
        private readonly IntPtr _dc;
        private readonly Win32Platform _platform;

        private delegate int RenderDocGetApi(int version, void* api);

        public RenderDocAPI RenderDoc;

        public WglGraphicsContext(IntPtr windowHandle, Win32Platform platform)
        {
            _platform = platform;

            // Load WGL.
            _openGlLibrary = _platform.LoadLibrary("opengl32.dll");
            if (_openGlLibrary == IntPtr.Zero) throw new Exception("opengl32.dll not found.");

            var createContext = _platform.GetFunctionByName<WglFunctions.WglCreateContext>(_openGlLibrary, "wglCreateContext");
            _deleteContext = _platform.GetFunctionByName<WglFunctions.WglDeleteContext>(_openGlLibrary, "wglDeleteContext");
            _getProcAddress = _platform.GetFunctionByName<WglFunctions.WglGetProcAddress>(_openGlLibrary, "wglGetProcAddress");
            _getCurrentDc = _platform.GetFunctionByName<WglFunctions.WglGetCurrentDc>(_openGlLibrary, "wglGetCurrentDC");
            _getCurrentContext = _platform.GetFunctionByName<WglFunctions.WglGetCurrentContext>(_openGlLibrary, "wglGetCurrentContext");
            _makeCurrent = _platform.GetFunctionByName<WglFunctions.WglMakeCurrent>(_openGlLibrary, "wglMakeCurrent");

            // A dummy context has to be created for opengl32.dll to load the. OpenGL ICD, from which we can then query WGL extensions.
            // This code will accept the Microsoft GDI ICD; accelerated context, creation failure occurs during manual pixel format enumeration
            Debug.Assert(Win32Platform.HelperWindowHandle != IntPtr.Zero);
            IntPtr dc = User32.GetDC(Win32Platform.HelperWindowHandle);
            Debug.Assert(dc != IntPtr.Zero);
            var pfd = new PixelFormatDescriptor();
            pfd.NSize = (ushort) Marshal.SizeOf(pfd);
            pfd.NVersion = 1;
            pfd.DwFlags = PixelFormatFlags.DrawToWindow | PixelFormatFlags.SupportOpenGl | PixelFormatFlags.DoubleBuffer;
            pfd.PixelType = (byte) PixelFormatFlags.RGBA;
            pfd.CColorBits = 24;

            if (!Gdi32.SetPixelFormat(dc, Gdi32.ChoosePixelFormat(dc, ref pfd), ref pfd)) Win32Platform.CheckError("WGL: Could not set pixel format on dummy context.", true);

            // Establish dummy context.
            IntPtr rc = createContext(dc);
            if (rc == IntPtr.Zero) Win32Platform.CheckError("WGL: Could not create dummy context.", true);
            if (!_makeCurrent(dc, rc))
            {
                _deleteContext(rc);
                Win32Platform.CheckError("WGL: Could not make dummy context current.", true);
                return;
            }

            // Check supported version.
            KhronosVersion glGetString = Gl.QueryVersionExternal(GetProcAddress);
            if (glGetString != null)
            {
                if (glGetString.Major < 3)
                {
                    _deleteContext(rc);
                    throw new Exception("WGL: Support is lower than 3.0");
                }
            }
            else
            {
                Engine.Log.Warning("WGL: Couldn't verify context version.", MessageSource.Wgl);
            }

            // Functions must be loaded first as they're needed to retrieve the extension string that tells us whether the functions are supported
            _getExtensionsStringExt = NativeHelpers.GetFunctionByPtr<WglFunctions.GetExtensionsStringExt>(_getProcAddress("wglGetExtensionsStringEXT"));
            _getExtensionsStringArb = NativeHelpers.GetFunctionByPtr<WglFunctions.GetExtensionsStringArb>(_getProcAddress("wglGetExtensionsStringARB"));
            var createContextAttribs = NativeHelpers.GetFunctionByPtr<WglFunctions.CreateContextAttribs>(_getProcAddress("wglCreateContextAttribsARB"));
            _swapIntervalExt = NativeHelpers.GetFunctionByPtr<WglFunctions.SwapInternalExt>(_getProcAddress("wglSwapIntervalEXT"));
            _getPixelFormatAttribivArb = NativeHelpers.GetFunctionByPtr<WglFunctions.GetPixelFormatAttributes>(_getProcAddress("wglGetPixelFormatAttribivARB"));

            WglGetSupportedExtensions();
            _arbMultisample = WglSupportedExtension("WGL_ARB_multisample");
            _arbMultisample = WglSupportedExtension("WGL_ARB_multisample");
            _arbFramebufferSRgb = WglSupportedExtension("WGL_ARB_framebuffer_sRGB");
            _extFramebufferSRgb = WglSupportedExtension("WGL_EXT_framebuffer_sRGB");
            bool arbCreateContext = WglSupportedExtension("WGL_ARB_create_context");
            bool arbCreateContextProfile = WglSupportedExtension("WGL_ARB_create_context_profile");
            _arbPixelFormat = WglSupportedExtension("WGL_ARB_pixel_format");

            Engine.Log.Trace($"ARB Pixel Format: {_arbPixelFormat}", MessageSource.Win32);

            // Dispose of dummy context.
            _deleteContext(rc);

            // Start creating actual context.
            _dc = User32.GetDC(windowHandle);
            if (_dc == IntPtr.Zero) Win32Platform.CheckError("WGL: Could not get window dc.", true);
            int pixelFormatIdx = SupportedPixelFormat(_dc);
            if (pixelFormatIdx == 0) return;

            if (Gdi32.DescribePixelFormat(_dc, pixelFormatIdx, (uint) sizeof(PixelFormatDescriptor), ref pfd) == 0)
            {
                Win32Platform.CheckError("WGL: Failed to retrieve PFD for the selected pixel format.", true);
                return;
            }

            if (!Gdi32.SetPixelFormat(_dc, pixelFormatIdx, ref pfd))
            {
                Win32Platform.CheckError("WGL: Could not set pixel format.", true);
                return;
            }

            if (arbCreateContext)
            {
                WglContextFlags mask = 0;
                WglContextFlags flags = 0;
                var attributes = new List<int>();

                // Check for RenderDoc
                IntPtr renderDocModule = Kernel32.GetModuleHandle("renderdoc.dll");
                if (renderDocModule != IntPtr.Zero)
                {
                    Engine.Log.Warning("Detected render doc. Setting window creation to Core Context 3.3", MessageSource.Win32);

                    attributes.Add((int) WglContextAttributes.MajorVersionArb);
                    attributes.Add(3);

                    attributes.Add((int) WglContextAttributes.MinorVersionArb);
                    attributes.Add(3);

                    // ArbCreateContextProfile is required to set a profile
                    if (arbCreateContextProfile)
                        mask |= WglContextFlags.CoreProfileBitArb;

                    flags |= WglContextFlags.DebugBitArb;

                    // Get a handle to the RenderDoc API
                    IntPtr api = Kernel32.GetProcAddress(renderDocModule, "RENDERDOC_GetAPI");
                    if (api != IntPtr.Zero)
                    {
                        var getApiFunc = Marshal.GetDelegateForFunctionPointer<RenderDocGetApi>(api);
                        void* apiPointers;
                        int ret = getApiFunc(10102, &apiPointers);
                        if (ret == 1)
                        {
                            Debug.Assert(ret == 1);
                            RenderDoc = Marshal.PtrToStructure<RenderDocAPI>((IntPtr) apiPointers);
                        }
                    }
                }

                // Add flags and mask if any. (there mostly isn't)
                // Context creation in Wgl is mostly default and trusts(tm) the other side.
                if (flags != 0)
                {
                    attributes.Add((int) WglContextAttributes.FlagsArb);
                    attributes.Add((int) flags);
                }

                if (mask != 0)
                {
                    attributes.Add((int) WglContextAttributes.ProfileMaskArb);
                    attributes.Add((int) mask);
                }

                attributes.Add(0);

                int[] attArr = attributes.ToArray();
                fixed (int* attPtr = &attArr[0])
                {
                    _contextHandle = createContextAttribs(_dc, IntPtr.Zero, attPtr);
                }

                if (_contextHandle == IntPtr.Zero) Win32Platform.CheckError("Creating WGL context", true);
            }
            else
            {
                _contextHandle = createContext(_dc);
                if (_contextHandle == IntPtr.Zero) Win32Platform.CheckError("Creating WGL legacy context", true);
            }

            Win32Platform.CheckError("Checking if context creation passed.");
            Engine.Log.Trace($"Requested context using pixel format id {pixelFormatIdx}", MessageSource.Win32);

            Valid = true;
        }

        #region Helpers

        private void WglGetSupportedExtensions()
        {
            string extensions = null;

            if (_getExtensionsStringArb != null)
                extensions = _getExtensionsStringArb(_getCurrentDc());

            if (extensions == null && _getExtensionsStringExt != null)
                extensions = _getExtensionsStringExt();

            if (extensions == null) return;

            _wglExtensions = extensions.Split(' ');
        }

        private bool WglSupportedExtension(string str)
        {
            return _wglExtensions != null && _wglExtensions.Any(extension => string.Equals(extension, str, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Finds the index of the supported pixel format closest to the requested pixel format.
        /// </summary>
        /// <param name="dc">The window's handle.</param>
        /// <returns>The index of the pixel format to use.</returns>
        private int SupportedPixelFormat(IntPtr dc)
        {
            var nativeCount = 0;
            var usableConfigs = new List<FramebufferConfig>();

            // Check whether we can use the modern API or not.
            if (_arbPixelFormat)
            {
                if (_getPixelFormatAttribivArb == null) throw new Exception("WGL: Unsupported graphics context, getPixelFormatAttribivArb is missing!");

                int nPf = FlagNumberPixelFormatsArb;
                if (!_getPixelFormatAttribivArb(dc, 1, 0, 1, &nPf, &nativeCount))
                {
                    Win32Platform.CheckError("WGL: Failed to retrieve pixel format attribute.", true);
                    return 0;
                }

                var foundAttributes = new List<int>
                {
                    (int) WglAttributes.SupportOpenGLArb,
                    (int) WglAttributes.DrawToWindowArb,
                    (int) WglAttributes.PixelTypeArb,
                    (int) WglAttributes.AccelerationArb,
                    (int) WglAttributes.RedBitsArb,
                    (int) WglAttributes.RedShiftArb,
                    (int) WglAttributes.GreenBitsArb,
                    (int) WglAttributes.GreenShiftArb,
                    (int) WglAttributes.BlueBitsArb,
                    (int) WglAttributes.BlueShiftArb,
                    (int) WglAttributes.AlphaBitsArb,
                    (int) WglAttributes.AlphaShiftArb,
                    (int) WglAttributes.DepthBitsArb,
                    (int) WglAttributes.StencilBitsArb,
                    (int) WglAttributes.AccumBitsArb,
                    (int) WglAttributes.AccumRedBitsArb,
                    (int) WglAttributes.AccumGreenBitsArb,
                    (int) WglAttributes.AccumBlueBitsArb,
                    (int) WglAttributes.AccumAlphaBitsArb,
                    (int) WglAttributes.AuxBuffersArb,
                    (int) WglAttributes.StereoArb,
                    (int) WglAttributes.DoubleBufferArb
                };

                if (_arbMultisample) foundAttributes.Add((int) WglAttributes.SamplesArb);

                if (_arbFramebufferSRgb || _extFramebufferSRgb) foundAttributes.Add((int) WglAttributes.FramebufferSrgbCapableArb);

                int[] attributes = foundAttributes.ToArray();

                for (var i = 0; i < nativeCount; i++)
                {
                    var fbConfig = new FramebufferConfig();

                    // Get pixel format attributes through "modern" extension
                    var values = new int[attributes.Length];

                    fixed (int* vPtr = &values[0])
                    {
                        fixed (int* attrPtr = &attributes[0])
                        {
                            if (!_getPixelFormatAttribivArb(dc, i + 1, 0, (uint) attributes.Length, attrPtr, vPtr))
                            {
                                Win32Platform.CheckError($"WGL: Failed to retrieve pixel format attribute - for config {i}.", true);
                                return 0;
                            }
                        }
                    }

                    if (FindAttributeValue(attributes, values, WglAttributes.SupportOpenGLArb) == 0
                        || FindAttributeValue(attributes, values, WglAttributes.DrawToWindowArb) == 0)
                        continue;

                    if (FindAttributeValue(attributes, values, WglAttributes.PixelTypeArb) != (int) WglAttributeValues.TypeRgbaArb) continue;
                    if (FindAttributeValue(attributes, values, WglAttributes.AccelerationArb) == (int) WglAttributeValues.NoAccelerationArb) continue;

                    fbConfig.RedBits = FindAttributeValue(attributes, values, WglAttributes.RedBitsArb);
                    fbConfig.GreenBits = FindAttributeValue(attributes, values, WglAttributes.GreenBitsArb);
                    fbConfig.BlueBits = FindAttributeValue(attributes, values, WglAttributes.BlueBitsArb);
                    fbConfig.AlphaBits = FindAttributeValue(attributes, values, WglAttributes.AlphaBitsArb);

                    fbConfig.DepthBits = FindAttributeValue(attributes, values, WglAttributes.DepthBitsArb);
                    fbConfig.StencilBits = FindAttributeValue(attributes, values, WglAttributes.StencilBitsArb);

                    fbConfig.AccumRedBits = FindAttributeValue(attributes, values, WglAttributes.AccumRedBitsArb);
                    fbConfig.AccumGreenBits = FindAttributeValue(attributes, values, WglAttributes.AccumGreenBitsArb);
                    fbConfig.AccumBlueBits = FindAttributeValue(attributes, values, WglAttributes.AccumBlueBitsArb);
                    fbConfig.AccumAlphaBits = FindAttributeValue(attributes, values, WglAttributes.AccumAlphaBitsArb);

                    fbConfig.AuxBuffers = FindAttributeValue(attributes, values, WglAttributes.AuxBuffersArb);

                    fbConfig.Stereo = FindAttributeValue(attributes, values, WglAttributes.StereoArb) != 0;
                    fbConfig.Doublebuffer = FindAttributeValue(attributes, values, WglAttributes.DoubleBufferArb) != 0;

                    if (_arbMultisample)
                        fbConfig.Samples = FindAttributeValue(attributes, values, WglAttributes.SamplesArb);

                    if (_arbFramebufferSRgb ||
                        _extFramebufferSRgb)
                        if (FindAttributeValue(attributes, values, WglAttributes.FramebufferSrgbCapableArb) != 0)
                            fbConfig.SRgb = true;

                    fbConfig.Handle = i + 1;
                    usableConfigs.Add(fbConfig);
                }
            }
            else
            {
                nativeCount = Gdi32.DescribePixelFormat(dc, 1, (uint) sizeof(PixelFormatDescriptor), IntPtr.Zero);

                for (var i = 0; i < nativeCount; i++)
                {
                    var fbConfig = new FramebufferConfig();

                    var pfd = new PixelFormatDescriptor();
                    pfd.NSize = (ushort) Marshal.SizeOf(pfd);
                    if (Gdi32.DescribePixelFormat(dc, i + 1, (uint) sizeof(PixelFormatDescriptor), ref pfd) == 0)
                    {
                        Win32Platform.CheckError("WGL: Could not describe pixel format.", true);
                        continue;
                    }

                    // We need both of these to be supported.
                    if ((pfd.DwFlags & PixelFormatFlags.DrawToWindow) == 0 ||
                        (pfd.DwFlags & PixelFormatFlags.SupportOpenGl) == 0)
                        continue;

                    // Should be GenericAccelerated, and mustn't be generic.
                    if ((pfd.DwFlags & PixelFormatFlags.GenericAccelerated) == 0 &&
                        (pfd.DwFlags & PixelFormatFlags.Generic) != 0)
                        continue;

                    if (pfd.PixelType != (byte) PixelFormatFlags.RGBA)
                        continue;

                    fbConfig.RedBits = pfd.CRedBits;
                    fbConfig.GreenBits = pfd.CGreenBits;
                    fbConfig.BlueBits = pfd.CBlueBits;
                    fbConfig.AlphaBits = pfd.CAlphaBits;

                    fbConfig.DepthBits = pfd.CDepthBits;
                    fbConfig.StencilBits = pfd.CStencilBits;

                    fbConfig.AccumRedBits = pfd.CAccumRedBits;
                    fbConfig.AccumGreenBits = pfd.CAccumGreenBits;
                    fbConfig.AccumBlueBits = pfd.CAccumBlueBits;
                    fbConfig.AccumAlphaBits = pfd.CAccumAlphaBits;

                    if ((pfd.DwFlags & PixelFormatFlags.Stereo) != 0) fbConfig.Stereo = true;

                    if ((pfd.DwFlags & PixelFormatFlags.DoubleBuffer) != 0) fbConfig.Doublebuffer = true;

                    fbConfig.Handle = i + 1;
                    usableConfigs.Add(fbConfig);
                }
            }

            if (usableConfigs.Count == 0)
            {
                Engine.Log.Error("The driver doesn't seem to support OpenGL.", MessageSource.Wgl);
                return 0;
            }

            FramebufferConfig closestConfig = ChoosePixelFormat(usableConfigs);
            if (closestConfig != null) return closestConfig.Handle;

            Engine.Log.Error("Couldn't find suitable pixel format.", MessageSource.Wgl);
            return 0;
        }

        #endregion

        #region Helpers

        private static int FindAttributeValue(int[] attributes, int[] values, WglAttributes attribute)
        {
            for (var i = 0; i < attributes.Length; i++)
            {
                if (attributes[i] == (int) attribute) return values[i];
            }

            Engine.Log.Error($"Unknown pixel format attribute requested - {attribute}", MessageSource.Wgl);
            return 0;
        }

        #endregion

        #region Graphic Context API

        protected override void SetSwapIntervalPlatform(int interval)
        {
            if (_platform.Window.DisplayMode != DisplayMode.Fullscreen)
            {
                bool dwmComposition;

                // DWM Composition is always enabled on Win8+
                if (Win32Platform.IsWindows8OrGreater)
                {
                    dwmComposition = true;
                }
                else
                {
                    HResult result = DwmApi.DwmIsCompositionEnabled(out dwmComposition);
                    if (result != HResult.S_OK) dwmComposition = false;
                }

                // Hack: Disable WGL swap interval when desktop composition is enabled to avoid interfering with DWM vsync.
                if (dwmComposition) interval = 0;
            }

            _swapIntervalExt?.Invoke(interval);
        }

        public override void SwapBuffers()
        {
            if (_platform.Window.DisplayMode != DisplayMode.Fullscreen)
            {
                bool dwmComposition;

                // DWM Composition is always enabled on Win8+
                if (Win32Platform.IsWindows8OrGreater)
                {
                    dwmComposition = true;
                }
                else
                {
                    HResult result = DwmApi.DwmIsCompositionEnabled(out dwmComposition);
                    if (result != HResult.S_OK) dwmComposition = false;
                }

                if (dwmComposition)
                    for (var i = 0; i < SwapInterval; i++)
                    {
                        DwmApi.DwmFlush();
                    }
            }

            Gdi32.SwapBuffers(_dc);
        }

        public override void MakeCurrent()
        {
            if (!_makeCurrent(_dc, _contextHandle)) Win32Platform.CheckError("WGL: Couldn't make context current.", true);
        }

        public override IntPtr GetProcAddress(string func)
        {
            IntPtr proc = _getProcAddress(func);
            return proc == IntPtr.Zero ? _platform.GetLibrarySymbolPtr(_openGlLibrary, func) : proc;
        }

        public void Dispose()
        {
            if (_contextHandle == IntPtr.Zero) return;
            _deleteContext(_contextHandle);
        }

        #endregion
    }
}