#region Using

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Security;
using Emotion.Utility;

#endregion

#pragma warning disable CS1574

// ReSharper disable once CheckNamespace
namespace Emotion.Platform.Implementation.GlfwImplementation.Native
{
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    [SuppressMessage("ReSharper", "CommentTypo")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    public static partial class Glfw
    {
        /// <summary>
        ///     <para>
        ///     This function initializes the GLFW library. Before most GLFW functions can be
        ///     used, GLFW must be initialized, and before an application terminates GLFW should be
        ///     terminated in order to free any resources allocated during or after
        ///     initialization.
        ///     </para>
        ///     <para>
        ///     If this function fails, it calls <see cref="Terminate" /> before returning. If it
        ///     succeeds, you should call <see cref="Terminate" /> before the application exits.
        ///     </para>
        ///     <para>
        ///     Additional calls to this function after successful initialization but before
        ///     termination will return <c>true</c> immediately.
        ///     </para>
        /// </summary>
        /// <returns><c>true</c> if successful, or <c>false</c> if an error occurred.</returns>
        /// <remarks>
        /// <strong>OSX:</strong> This function will change the current directory of the application
        /// to the <c>Contents/Resources</c> subdirectory of the application's bundle, if present.
        /// </remarks>
        /// <seealso cref="Terminate" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwInit")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool Init();

        /// <summary>
        ///     <para>
        ///     This function destroys all remaining windows and cursors, restores any modified
        ///     gamma ramps and frees any other allocated resources. Once this function is called, you
        ///     must again call <see cref="Init" /> successfully before you will be able to use most GLFW
        ///     functions.
        ///     </para>
        ///     <para>
        ///     If GLFW has been successfully initialized, this function should be called before
        ///     the application exits. If initialization fails, there is no need to call this function,
        ///     as it is called by <see cref="Init" /> before it returns failure.
        ///     </para>
        /// </summary>
        /// <remarks>
        /// This function may be called before <see cref="Init" />.
        /// </remarks>
        /// <seealso cref="Init" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwTerminate")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void Terminate();

        /// <summary>
        /// This function retrieves the major, minor and revision numbers of the GLFW library. It is
        /// intended for when you are using GLFW as a shared library and want to ensure that you are
        /// using the minimum required version.
        /// </summary>
        /// <param name="major">Where to store the major version number.</param>
        /// <param name="minor">Where to store the minor version number.</param>
        /// <param name="rev">Where to store the revision number.</param>
        /// <remarks>
        /// This function may be called before <see cref="Init" />.
        /// </remarks>
        /// <seealso cref="GetVersionString" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetVersion")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void GetVersion(out int major, out int minor, out int rev);

        /// <summary>
        ///     <para>
        ///     This function returns the compile-time generated version string of the GLFW
        ///     library binary. It describes the version, platform, compiler and any platform-specific
        ///     compile-time options. It should not be confused with the OpenGL or OpenGL ES version
        ///     string, queried with <c>glGetString</c>.
        ///     </para>
        ///     <para>
        ///     <strong>Do not use the version string</strong> to parse the GLFW library version.
        ///     The <see cref="GetVersion(out int, out int, out int)" /> function provides the version of
        ///     the running library binary in numerical format.
        ///     </para>
        /// </summary>
        /// <returns>The ASCII encoded GLFW version string.</returns>
        /// <remarks>
        /// This function may be called before <see cref="Init" />.
        /// </remarks>
        /// <seealso cref="GetVersion(out int, out int, out int)" />
        public static string GetVersionString()
        {
            return NativeHelpers.StringFromPtr(glfwGetVersionString());
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwGetVersionString();

        /// <summary>
        ///     <para>
        ///     This function sets the error callback, which is called with an error code and a
        ///     human-readable description each time a GLFW error occurs.
        ///     </para>
        ///     <para>
        ///     The error callback is called on the thread where the error occurred. If you are
        ///     using GLFW from multiple threads, your error callback needs to be written
        ///     accordingly.
        ///     </para>
        ///     <para>
        ///     Because the description string may have been generated specifically for that
        ///     error, it is not guaranteed to be valid after the callback has returned. If you wish to
        ///     use it after the callback returns, you need to make a copy.
        ///     </para>
        ///     <para>
        ///     Once set, the error callback remains set even after the library has been
        ///     terminated.
        ///     </para>
        /// </summary>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        /// <remarks>
        /// This function may be called before <see cref="Init" />.
        /// </remarks>
        public static void SetErrorCallback(ErrorFunc callback)
        {
            glfwSetErrorCallback(Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetErrorCallback(IntPtr callback);

        /// <summary>
        /// This function returns an array of handles for all currently connected monitors. The
        /// primary monitor is always first in the returned array. If no monitors were found, this
        /// function returns <c>null</c>.
        /// </summary>
        /// <returns>
        /// An array of monitor handles, or <c>null</c> if no monitors were found or if an
        /// error occurred.
        /// </returns>
        /// <seealso cref="GetPrimaryMonitor" />
        public static unsafe Monitor[] GetMonitors()
        {
            int count;
            IntPtr array = glfwGetMonitors(&count);

            if (count == 0)
                return null;

            var monitors = new Monitor[count];
            int size = Marshal.SizeOf(typeof(IntPtr));

            for (var i = 0; i < count; i++)
            {
                IntPtr ptr = Marshal.ReadIntPtr(array, i * size);
                monitors[i] = new Monitor(ptr);
            }

            return monitors;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe IntPtr glfwGetMonitors(int* count);

        /// <summary>
        /// This function returns the primary monitor. This is usually the monitor where elements
        /// like the task bar or global menu bar are located.
        /// </summary>
        /// <returns>
        /// The primary monitor, or <c>null</c> if no monitors were found or if an error
        /// occurred.
        /// </returns>
        /// <remarks>
        /// The primary monitor is always first in the array returned by <see cref="GetMonitors" />.
        /// </remarks>
        /// <seealso cref="GetMonitors" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetPrimaryMonitor")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern Monitor GetPrimaryMonitor();

        /// <summary>
        /// This function returns the position, in screen coordinates, of the upper-left corner of
        /// the specified monitor.
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <param name="xpos">Where to store the monitor x-coordinate.</param>
        /// <param name="ypos">Where to store the monitor y-coordinate.</param>
        public static unsafe void GetMonitorPos(Monitor monitor, out int xpos, out int ypos)
        {
            int xx, yy;
            glfwGetMonitorPos(monitor.Ptr, &xx, &yy);
            xpos = xx;
            ypos = yy;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe void glfwGetMonitorPos(IntPtr monitor, int* xpos, int* ypos);

        /// <summary>
        ///     <para>
        ///     This function returns the size, in millimetres, of the display area of the
        ///     specified monitor.
        ///     </para>
        ///     <para>
        ///     Some systems do not provide accurate monitor size information, either because the
        ///     monitor <a href="https://en.wikipedia.org/wiki/Extended_display_identification_data">EDID</a>
        ///     data is incorrect or because the driver does not report it accurately.
        ///     </para>
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <param name="widthMM">
        /// Where to store the width, in millimetres, of the monitor's display
        /// area.
        /// </param>
        /// <param name="heightMM">
        /// Where to store the height, in millimetres, of the monitor's
        /// display area.
        /// </param>
        /// <remarks>
        /// <strong>Win32: </strong> Calculates the returned physical size from the current
        /// resolution and system DPI instead of querying the monitor EDID data.
        /// </remarks>
        public static unsafe void GetMonitorPhysicalSize(Monitor monitor, out int widthMM, out int heightMM)
        {
            int ww, hh;
            glfwGetMonitorPhysicalSize(monitor.Ptr, &ww, &hh);
            widthMM = ww;
            heightMM = hh;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe void glfwGetMonitorPhysicalSize(IntPtr monitor, int* widthMM, int* heightMM);

        /// <summary>
        /// This function returns a human-readable name, encoded as UTF-8, of the specified monitor.
        /// The name typically reflects the make and model of the monitor and is not guaranteed to
        /// be unique among the connected monitors.
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>
        /// The UTF-8 encoded name of the monitor, or <c>null</c> if an error
        /// occurred.
        /// </returns>
        public static string GetMonitorName(Monitor monitor)
        {
            return NativeHelpers.StringFromPtr(glfwGetMonitorName(monitor.Ptr));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwGetMonitorName(IntPtr monitor);

        /// <summary>
        /// This function sets the monitor configuration callback, or removes the currently set
        /// callback. This is called when a monitor is connected to or disconnected from the system.
        /// </summary>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetMonitorCallback(MonitorFunc callback)
        {
            glfwSetMonitorCallback(Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetMonitorCallback(IntPtr callback);

        /// <summary>
        /// This function returns an array of all video modes supported by the specified monitor.
        /// The returned array is sorted in ascending order, first by color bit depth (the sum of
        /// all channel depths) and then by resolution area (the product of width and height).
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>An array of video modes, or <c>null</c> if an error occurred.</returns>
        /// <seealso cref="GetVideoMode(Monitor)" />
        public static unsafe VideoMode[] GetVideoModes(Monitor monitor)
        {
            int count;
            VideoMode* array = glfwGetVideoModes(monitor.Ptr, &count);

            if (count == 0)
                return null;

            var result = new VideoMode[count];

            for (var i = 0; i < count; i++)
            {
                result[i] = array[i];
            }

            return result;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe VideoMode* glfwGetVideoModes(IntPtr monitor, int* count);

        /// <summary>
        /// This function returns the current video mode of the specified monitor. If you have
        /// created a full screen window for that monitor, the return value will depend on whether
        /// that window is iconified.
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>The current mode of the monitor, or <c>null</c> if an error occurred.</returns>
        /// <seealso cref="GetVideoModes(Monitor)" />
        public static VideoMode GetVideoMode(Monitor monitor)
        {
            IntPtr ptr = glfwGetVideoMode(monitor.Ptr);
            return (VideoMode) Marshal.PtrToStructure(ptr, typeof(VideoMode));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwGetVideoMode(IntPtr monitor);

        /// <summary>
        /// This function generates a 256-element gamma ramp from the specified exponent and then
        /// calls <see cref="SetGammaRamp(Monitor, GammaRamp)" /> with it. The value must be a finite
        /// number greater than zero.
        /// </summary>
        /// <param name="monitor">The monitor whose gamma ramp to set.</param>
        /// <param name="gamma">The desired exponent.</param>
        public static void SetGamma(Monitor monitor, float gamma)
        {
            glfwSetGamma(monitor.Ptr, gamma);
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void glfwSetGamma(IntPtr monitor, float gamma);

        /// <summary>
        /// This function returns the current gamma ramp of the specified monitor.
        /// </summary>
        /// <param name="monitor">The monitor to query.</param>
        /// <returns>The current gamma ramp, or <c>null</c> if an error occurred.</returns>
        public static unsafe GammaRamp GetGammaRamp(Monitor monitor)
        {
            InternalGammaRamp* internalRamp = glfwGetGammaRamp(monitor.Ptr);

            var ramp = new GammaRamp
            {
                Red = new ushort[internalRamp->Size],
                Green = new ushort[internalRamp->Size],
                Blue = new ushort[internalRamp->Size]
            };

            for (uint i = 0; i < ramp.Size; i++)
            {
                ramp.Red[i] = internalRamp->Red[i];
                ramp.Green[i] = internalRamp->Green[i];
                ramp.Blue[i] = internalRamp->Blue[i];
            }

            return ramp;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe InternalGammaRamp* glfwGetGammaRamp(IntPtr monitor);

        /// <summary>
        /// This function sets the current gamma ramp for the specified monitor. The original gamma
        /// ramp for that monitor is saved by GLFW the first time this function is called and is
        /// restored by <see cref="Terminate" />.
        /// </summary>
        /// <param name="monitor">The monitor whose gamma ramp to set.</param>
        /// <param name="ramp">The gamma ramp to use.</param>
        /// <remarks>
        /// Gamma ramp sizes other than 256 are not supported by all platforms or graphics hardware
        /// (<strong>Win32</strong> requires a 256 gamma ramp size).
        /// </remarks>
        public static unsafe void SetGammaRamp(Monitor monitor, GammaRamp ramp)
        {
            fixed (ushort* rampRed = ramp.Red, rampBlue = ramp.Blue, rampGreen = ramp.Green)
            {
                var internalRamp = new InternalGammaRamp
                {
                    Red = rampRed,
                    Blue = rampBlue,
                    Green = rampGreen,
                    Size = ramp.Size
                };

                glfwSetGammaRamp(monitor.Ptr, internalRamp);
            }
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void glfwSetGammaRamp(IntPtr monitor, InternalGammaRamp ramp);

        /// <summary>
        /// This function resets all window hints to their default values.
        /// </summary>
        /// <seealso cref="WindowHint(Hint, bool)" />
        /// <seealso cref="WindowHint(Hint, Enum)" />
        /// <seealso cref="WindowHint(Hint, int)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwDefaultWindowHints")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void DefaultWindowHints();

        /// <summary>
        ///     <para>
        ///     This function sets hints for the next call to
        ///     <see
        ///         cref="CreateWindow(int,int,string,System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Monitor},System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Window})" />
        ///     . The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or
        ///     <see cref="DefaultWindowHints" />, or until the library is terminated.
        ///     </para>
        ///     <para>This function does not check whether the specified hint values are valid.</para>
        ///     <para>
        ///     If you set hints to invalid values this will instead be reported by the next call
        ///     to
        ///     <see
        ///         cref="CreateWindow(int,int,string,System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Monitor},System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Window})" />
        ///     .
        ///     </para>
        /// </summary>
        /// <param name="hint">The window hint to set.</param>
        /// <param name="value">The new value of the window hint.</param>
        /// <seealso cref="DefaultWindowHints" />
        public static void WindowHint(Hint hint, bool value)
        {
            glfwWindowHint((int) hint, value ? 1 : 0);
        }

        /// <summary>
        ///     <para>
        ///     This function sets hints for the next call to
        ///     <see
        ///         cref="CreateWindow(int,int,string,System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Monitor},System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Window})" />
        ///     . The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or
        ///     <see cref="DefaultWindowHints" />, or until the library is terminated.
        ///     </para>
        ///     <para>This function does not check whether the specified hint values are valid.</para>
        ///     <para>
        ///     If you set hints to invalid values this will instead be reported by the next call
        ///     to
        ///     <see
        ///         cref="CreateWindow(int,int,string,System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Monitor},System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Window})" />
        ///     .
        ///     </para>
        /// </summary>
        /// <param name="hint">The window hint to set.</param>
        /// <param name="value">The new value of the window hint.</param>
        /// <seealso cref="DefaultWindowHints" />
        public static void WindowHint(Hint hint, int value)
        {
            if (value < 0)
                value = DontCare;

            glfwWindowHint((int) hint, value);
        }

        /// <summary>
        ///     <para>
        ///     This function sets hints for the next call to
        ///     <see
        ///         cref="CreateWindow(int,int,string,System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Monitor},System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Window})" />
        ///     . The hints, once set, retain their values
        ///     until changed by a call to <see cref="WindowHint(Hint, int)" /> or
        ///     <see cref="DefaultWindowHints" />, or until the library is terminated.
        ///     </para>
        ///     <para>This function does not check whether the specified hint values are valid.</para>
        ///     <para>
        ///     If you set hints to invalid values this will instead be reported by the next call
        ///     to
        ///     <see
        ///         cref="CreateWindow(int,int,string,System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Monitor},System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Window})" />
        ///     .
        ///     </para>
        /// </summary>
        /// <param name="hint">The window hint to set.</param>
        /// <param name="value">The new value of the window hint.</param>
        /// <seealso cref="DefaultWindowHints" />
        public static void WindowHint(Hint hint, Enum value)
        {
            glfwWindowHint((int) hint, Convert.ToInt32(value));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void glfwWindowHint(int target, int hint);

        /// <summary>
        ///     <para>
        ///     This function creates a window and its associated OpenGL or OpenGL ES context.
        ///     Most of the options controlling how the window and its context should be created are
        ///     specified with window hints.
        ///     </para>
        ///     <para>
        ///     Successful creation does not change which context is current. Before you can use
        ///     the newly created context, you need to make it current.
        ///     </para>
        ///     <para>
        ///     The created window, framebuffer and context may differ from what you requested, as
        ///     not all parameters and hints are hard constraints. This includes the size of the window,
        ///     especially for full screen windows. To query the actual attributes of the created
        ///     window, framebuffer and context see <see cref="GetWindowAttrib(Window, WindowAttrib)" />,
        ///     <see cref="GetWindowSize(Window, out int, out int)" /> and
        ///     <see cref="GetFramebufferSize(Window, out int, out int)" />.
        ///     </para>
        ///     <para>
        ///     To create a full screen window, you need to specify the monitor the window will
        ///     cover. If no monitor is specified, the window will be windowed mode. Unless you have a
        ///     way for the user to choose a specific monitor, it is recommended that you pick the
        ///     primary monitor.
        ///     </para>
        ///     <para>
        ///     For full screen windows, the specified size becomes the resolution of the window's
        ///     <em>desired video mode</em>. As long as a full screen window is not iconified, the
        ///     supported video mode most closely matching the desired video mode is set for the
        ///     specified monitor.
        ///     </para>
        ///     <para>
        ///     By default, newly created windows use the placement recommended by the window
        ///     system. To create the window at a specific position, make it initially invisible using
        ///     the <see cref="Hint.Visible" /> hint, set its position and then show it.
        ///     </para>
        ///     <para>
        ///     As long as at least one full screen window is not iconified, the screensaver is
        ///     prohibited from starting.
        ///     </para>
        ///     <para>
        ///     Window systems put limits on window sizes. Very large or very small window
        ///     dimensions may be overridden by the window system on creation. Check the actual size
        ///     after creation.
        ///     </para>
        ///     <para>
        ///     The swap interval is not set during window creation and the initial value may vary
        ///     depending on driver settings and defaults.
        ///     </para>
        /// </summary>
        /// <param name="width">
        /// The desired width, in screen coordinates, of the window. This must
        /// be greater than zero.
        /// </param>
        /// <param name="height">
        /// The desired height, in screen coordinates, of the window. This must
        /// be greater than zero.
        /// </param>
        /// <param name="title">The initial, UTF-8 encoded window title.</param>
        /// <param name="monitor">
        /// The monitor to use for full screen mode, or <c>null</c> for
        /// windowed mode.
        /// </param>
        /// <param name="share">
        /// The window whose context to share resources with, or <c>null</c> to
        /// not share resources.
        /// </param>
        /// <returns>
        /// The handle of the created window, or <c>null</c> if an error
        /// occurred.
        /// </returns>
        /// <remarks>
        ///     <para>
        ///     <strong>Win32:</strong> Window creation will fail if the Microsoft GDI software
        ///     OpenGL implementation is the only one available.
        ///     </para>
        ///     <para>
        ///     <strong>Win32:</strong> If the executable has an icon resource named
        ///     <c>GLFW_ICON</c>, it will be set as the initial icon for the window. If no such icon is
        ///     present, the <c>IDI_WINLOGO</c> icon will be used instead. To set a different icon, use
        ///     <see cref="SetWindowIcon(Window, Image)" />.
        ///     </para>
        ///     <para>
        ///     <strong>Win32:</strong>The context to share resources with must not be current on
        ///     any other thread.
        ///     </para>
        ///     <para>
        ///     <strong>OSX:</strong> The GLFW window has no icon, as it is not a document window,
        ///     but the dock icon will be the same as the application bundle's icon. For more
        ///     information on bundles, see the
        ///     <a href="https://developer.apple.com/library/mac/documentation/CoreFoundation/Conceptual/CFBundles/">
        ///     Bundle Programming Guide
        ///     </a>
        ///     in the Mac Developer Library.
        ///     </para>
        ///     <para>
        ///     <strong>OSX:</strong> The first time a window is created the menu bar is populated
        ///     with common commands like Hide, Quit and About. The About entry opens a minimal about
        ///     dialog with information from the application's bundle.
        ///     </para>
        ///     <para>
        ///     <strong>OSX:</strong> On OS X 10.10 and later the window frame will not be
        ///     rendered at full resolution on Retina displays unless the <c>NSHighResolutionCapable</c>
        ///     key is enabled in the application bundle's <c>Info.plist</c>. For more information, see
        ///     <a
        ///         href="https://developer.apple.com/library/mac/documentation/GraphicsAnimation/Conceptual/HighResolutionOSX/Explained/Explained.html">
        ///     High Resolution Guidelines for OS X
        ///     </a>
        ///     in the Mac Developer Library.
        ///     </para>
        ///     <para>
        ///     <strong>X11:</strong> Some window managers will not respect the placement of
        ///     initially hidden windows
        ///     </para>
        ///     <para>
        ///     <strong>X11:</strong> Due to the asynchronous nature of X11, it may take a moment
        ///     for a window to reach its requested state.This means you may not be able to query the
        ///     final size, position or other attributes directly after window creation.
        ///     </para>
        /// </remarks>
        /// <seealso cref="DestroyWindow(Window)" />
        public static Window? CreateWindow(int width, int height, string title, Monitor? monitor = null, Window? share = null)
        {
            return glfwCreateWindow(
                width,
                height,
                NativeHelpers.StringToPtr(title),
                monitor.HasValue ? monitor.Value.Ptr : IntPtr.Zero,
                share.HasValue ? share.Value.Ptr : IntPtr.Zero
            );
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Struct)]
        private static extern Window glfwCreateWindow(int width, int height, IntPtr title, IntPtr monitor, IntPtr share);

        /// <summary>
        ///     <para>
        ///     This function destroys the specified window and its context. On calling this
        ///     function, no further callbacks will be called for that window.
        ///     </para>
        ///     <para>
        ///     If the context of the specified window is current on the main thread, it is
        ///     detached before being destroyed.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to destroy.</param>
        /// <seealso
        ///     cref="CreateWindow(int,int,string,System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Monitor},System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Window})" />
        /// <seealso
        ///     cref="CreateWindow(int,int,string,System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Monitor},System.Nullable{Emotion.Platform.Implementation.GlfwImplementation.Native.Glfw.Window})" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwDestroyWindow")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void DestroyWindow([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        /// This function returns the value of the close flag of the specified window.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>The value of the close flag.</returns>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwWindowShouldClose")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool WindowShouldClose([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        /// This function sets the value of the close flag of the specified window. This can be used
        /// to override the user's attempt to close the window, or to signal that it should be
        /// closed.
        /// </summary>
        /// <param name="window">The window whose flag to change.</param>
        /// <param name="value">The new value.</param>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetWindowShouldClose")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SetWindowShouldClose([MarshalAs(UnmanagedType.Struct)] Window window, [MarshalAs(UnmanagedType.Bool)] bool value);

        /// <summary>
        /// This function sets the window title, encoded as UTF-8, of the specified window.
        /// </summary>
        /// <param name="window">The window whose title to change.</param>
        /// <param name="title">The UTF-8 encoded window title.</param>
        /// <remarks>
        /// <strong>OSX:</strong> The window title will not be updated until the next time you
        /// process events.
        /// </remarks>
        public static void SetWindowTitle(Window window, string title)
        {
            glfwSetWindowTitle(window.Ptr, NativeHelpers.StringToPtr(title));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void glfwSetWindowTitle(IntPtr window, IntPtr title);

        /// <summary>
        ///     <para>This function sets the icon of the specified window.</para>
        ///     <para>
        ///     The desired image sizes varies depending on platform and system settings. The
        ///     selected images will be rescaled as needed. Good sizes include 16x16, 32x32 and
        ///     48x48.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose icon to set.</param>
        /// <param name="image">The image to create the icon from.</param>
        /// <remarks>
        /// <strong>OSX:</strong> The GLFW window has no icon, as it is not a document window, so
        /// this function does nothing. The dock icon will be the same as the application bundle's
        /// icon. For more information on bundles, see the
        /// <a href="https://developer.apple.com/library/mac/documentation/CoreFoundation/Conceptual/CFBundles/">
        /// Bundle Programming Guide
        /// </a>
        /// in the Mac Developer Library.
        /// </remarks>
        public static void SetWindowIcon(Window window, Image image)
        {
            SetWindowIcon(window, new[] {image});
        }

        /// <summary>
        ///     <para>
        ///     This function sets the icon of the specified window. If passed an array of
        ///     candidate images, those of or closest to the sizes desired by the system are selected.
        ///     If no images are specified, the window reverts to its default icon.
        ///     </para>
        ///     <para>
        ///     The desired image sizes varies depending on platform and system settings. The
        ///     selected images will be rescaled as needed. Good sizes include 16x16, 32x32 and
        ///     48x48.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose icon to set.</param>
        /// <param name="images">The images to create the icon from.</param>
        /// <remarks>
        /// <strong>OSX:</strong> The GLFW window has no icon, as it is not a document window, so
        /// this function does nothing. The dock icon will be the same as the application bundle's
        /// icon. For more information on bundles, see the
        /// <a href="https://developer.apple.com/library/mac/documentation/CoreFoundation/Conceptual/CFBundles/">
        /// Bundle Programming Guide
        /// </a>
        /// in the Mac Developer Library.
        /// </remarks>
        public static unsafe void SetWindowIcon(Window window, Image[] images)
        {
            if (images == null)
            {
                glfwSetWindowIcon(window.Ptr, 0, IntPtr.Zero);
                return;
            }

            var imgs = new InternalImage[images.Length];

            for (var i = 0; i < imgs.Length; i++)
            {
                int size = images[i].Width * images[i].Width * 4;

                imgs[i] = new InternalImage
                {
                    Width = images[i].Width,
                    Height = images[i].Width,
                    Pixels = Marshal.AllocHGlobal(size)
                };

                Marshal.Copy(images[i].Pixels, 0, imgs[i].Pixels, Math.Min(size, images[i].Pixels.Length));
            }

            IntPtr ptr;
            fixed (InternalImage* array = imgs)
            {
                ptr = new IntPtr(array);
            }

            glfwSetWindowIcon(window.Ptr, images.Length, ptr);

            for (var i = 0; i < imgs.Length; i++)
            {
                Marshal.FreeHGlobal(imgs[i].Pixels);
            }
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void glfwSetWindowIcon(IntPtr window, int count, IntPtr images);

        /// <summary>
        /// This function retrieves the position, in screen coordinates, of the upper-left corner of
        /// the client area of the specified window.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <param name="xpos">
        /// Where to store the x-coordinate of the upper-left corner of the
        /// client area.
        /// </param>
        /// <param name="ypos">
        /// Where to store the y-coordinate of the upper-left corner of the
        /// client area.
        /// </param>
        /// <seealso cref="SetWindowPos(Window, int, int)" />
        public static unsafe void GetWindowPos(Window window, out int xpos, out int ypos)
        {
            int xx, yy;
            glfwGetWindowPos(window.Ptr, &xx, &yy);
            xpos = xx;
            ypos = yy;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe void glfwGetWindowPos(IntPtr window, int* xpos, int* ypos);

        /// <summary>
        ///     <para>
        ///     This function sets the position, in screen coordinates, of the upper-left corner
        ///     of the client area of the specified windowed mode window. If the window is a full screen
        ///     window, this function does nothing.
        ///     </para>
        ///     <para>
        ///     <strong>Do not use this function</strong> to move an already visible window unless
        ///     you have very good reasons for doing so, as it will confuse and annoy the user.
        ///     </para>
        ///     <para>
        ///     The window manager may put limits on what positions are allowed. GLFW cannot and
        ///     should not override these limits.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <param name="xpos">The x-coordinate of the upper-left corner of the client area.</param>
        /// <param name="ypos">The y-coordinate of the upper-left corner of the client area.</param>
        /// <seealso cref="GetWindowPos(Window, out int, out int)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetWindowPos")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SetWindowPos([MarshalAs(UnmanagedType.Struct)] Window window, int xpos, int ypos);

        /// <summary>
        /// This function retrieves the size, in screen coordinates, of the client area of the
        /// specified window.If you wish to retrieve the size of the framebuffer of the window in
        /// pixels, see <see cref="GetFramebufferSize(Window, out int, out int)" />.
        /// </summary>
        /// <param name="window">The window whose size to retrieve.</param>
        /// <param name="width">
        /// Where to store the width, in screen coordinates, of the client
        /// area.
        /// </param>
        /// <param name="height">
        /// Where to store the height, in screen coordinates, of the client
        /// area.
        /// </param>
        /// <seealso cref="SetWindowSize(Window, int, int)" />
        public static unsafe void GetWindowSize(Window window, out int width, out int height)
        {
            int w, h;
            glfwGetWindowSize(window.Ptr, &w, &h);
            width = w;
            height = h;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe void glfwGetWindowSize(IntPtr window, int* width, int* height);

        /// <summary>
        ///     <para>
        ///     This function sets the size limits of the client area of the specified window. If
        ///     the window is full screen, the size limits only take effect once it is made windowed. If
        ///     the window is not resizable, this function does nothing.
        ///     </para>
        ///     <para>
        ///     The size limits are applied immediately to a windowed mode window and may cause it
        ///     to be resized.
        ///     </para>
        ///     <para>
        ///     The maximum dimensions must be greater than or equal to the minimum dimensions and
        ///     all must be greater than or equal to zero.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to set limits for.</param>
        /// <param name="minwidth">
        /// The minimum width, in screen coordinates, of the client area, or
        /// <see cref="DontCare" />.
        /// </param>
        /// <param name="minheight">
        /// The minimum height, in screen coordinates, of the client area,
        /// or <see cref="DontCare" />.
        /// </param>
        /// <param name="maxwidth">
        /// The maximum width, in screen coordinates, of the client area, or
        /// <see cref="DontCare" />.
        /// </param>
        /// <param name="maxheight">
        /// The maximum height, in screen coordinates, of the client area,
        /// or <see cref="DontCare" />.
        /// </param>
        /// <remarks>
        /// If you set size limits and an aspect ratio that conflict, the results are undefined.
        /// </remarks>
        /// <seealso cref="SetWindowAspectRatio(Window, int, int)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetWindowSizeLimits")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SetWindowSizeLimits([MarshalAs(UnmanagedType.Struct)] Window window, int minwidth, int minheight, int maxwidth, int maxheight);

        /// <summary>
        ///     <para>
        ///     This function sets the required aspect ratio of the client area of the specified
        ///     window. If the window is full screen, the aspect ratio only takes effect once it is
        ///     made windowed.  If the window is not resizable, this function does nothing.
        ///     </para>
        ///     <para>
        ///     The aspect ratio is specified as a numerator and a denominator and both values
        ///     must be greater than zero. For example, the common 16:9 aspect ratio is specified as 16
        ///     and 9, respectively.
        ///     </para>
        ///     <para>
        ///     If the numerator and denominator is set to <see cref="DontCare" /> then the aspect
        ///     ratio limit is disabled.
        ///     </para>
        ///     <para>
        ///     The aspect ratio is applied immediately to a windowed mode window and may cause it
        ///     to be resized.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to set limits for.</param>
        /// <param name="numer">
        /// The numerator of the desired aspect ratio, or
        /// <see cref="DontCare" />.
        /// </param>
        /// <param name="denom">
        /// The denominator of the desired aspect ratio, or
        /// <see cref="DontCare" />.
        /// </param>
        /// <remarks>
        /// If you set size limits and an aspect ratio that conflict, the results are undefined.
        /// </remarks>
        /// <seealso cref="SetWindowSizeLimits(Window, int, int, int, int)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetWindowAspectRatio")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SetWindowAspectRatio([MarshalAs(UnmanagedType.Struct)] Window window, int numer, int denom);

        /// <summary>
        ///     <para>
        ///     This function sets the size, in screen coordinates, of the client area of the
        ///     specified window.
        ///     </para>
        ///     <para>
        ///     For full screen windows, this function updates the resolution of its desired video
        ///     mode and switches to the video mode closest to it, without affecting the window's
        ///     context. As the context is unaffected, the bit depths of the framebuffer remain
        ///     unchanged.
        ///     </para>
        ///     <para>
        ///     If you wish to update the refresh rate of the desired video mode in addition to
        ///     its resolution, see @ref glfwSetWindowMonitor.
        ///     </para>
        ///     <para>
        ///     The window manager may put limits on what sizes are allowed.GLFW cannot and should
        ///     not override these limits.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to resize.</param>
        /// <param name="width">
        /// The desired width, in screen coordinates, of the window client
        /// area.
        /// </param>
        /// <param name="height">
        /// The desired height, in screen coordinates, of the window client
        /// area.
        /// </param>
        /// <seealso cref="GetWindowSize(Window, out int, out int)" />
        /// <seealso cref="SetWindowMonitor(Window, Monitor, int, int, int, int, int)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetWindowSize")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SetWindowSize([MarshalAs(UnmanagedType.Struct)] Window window, int width, int height);

        /// <summary>
        /// This function retrieves the size, in pixels, of the framebuffer of the specified window.
        /// If you wish to retrieve the size of the window in screen coordinates, see
        /// <see cref="GetWindowSize(Window, out int, out int)" />.
        /// </summary>
        /// <param name="window">The window whose framebuffer to query.</param>
        /// <param name="width">Where to store the width, in pixels, of the framebuffer.</param>
        /// <param name="height">Where to store the height, in pixels, of the framebuffer.</param>
        /// <seealso cref="SetFramebufferSizeCallback(Window, FramebufferSizeFunc)" />
        public static unsafe void GetFramebufferSize(Window window, out int width, out int height)
        {
            int w, h;
            glfwGetFramebufferSize(window.Ptr, &w, &h);
            width = w;
            height = h;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe void glfwGetFramebufferSize(IntPtr window, int* width, int* height);

        /// <summary>
        ///     <para>
        ///     This function retrieves the size, in screen coordinates, of each edge of the frame
        ///     of the specified window. This size includes the title bar, if the window has one. The
        ///     size of the frame may vary depending on the window - related hints used to create
        ///     it.
        ///     </para>
        ///     <para>
        ///     Because this function retrieves the size of each window frame edge and not the
        ///     offset along a particular coordinate axis, the retrieved values will always be zero or
        ///     positive.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose frame size to query.</param>
        /// <param name="left">
        /// Where to store the size, in screen coordinates, of the left edge of
        /// the window frame.
        /// </param>
        /// <param name="top">
        /// Where to store the size, in screen coordinates, of the top edge of the
        /// window frame.
        /// </param>
        /// <param name="right">
        /// Where to store the size, in screen coordinates, of the right edge of
        /// the window frame.
        /// </param>
        /// <param name="bottom">
        /// Where to store the size, in screen coordinates, of the bottom edge
        /// of the window frame.
        /// </param>
        public static unsafe void GetWindowFrameSize(Window window, out int left, out int top, out int right, out int bottom)
        {
            int l, t, r, b;
            glfwGetWindowFrameSize(window.Ptr, &l, &t, &r, &b);
            left = l;
            top = t;
            right = r;
            bottom = b;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe void glfwGetWindowFrameSize(IntPtr window, int* left, int* top, int* right, int* bottom);

        /// <summary>
        ///     <para>
        ///     This function iconifies (minimizes) the specified window if it was previously
        ///     restored. If the window is already iconified, this function does nothing.
        ///     </para>
        ///     <para>
        ///     If the specified window is a full screen window, the original monitor resolution
        ///     is restored until the window is restored.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to iconify.</param>
        /// <seealso cref="RestoreWindow(Window)" />
        /// <seealso cref="MaximizeWindow(Window)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwIconifyWindow")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void IconifyWindow([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        ///     <para>
        ///     This function restores the specified window if it was previously iconified
        ///     (minimized) or maximized.If the window is already restored, this function does
        ///     nothing.
        ///     </para>
        ///     <para>
        ///     If the specified window is a full screen window, the resolution chosen for the
        ///     window is restored on the selected monitor.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to restore.</param>
        /// <seealso cref="IconifyWindow(Window)" />
        /// <seealso cref="MaximizeWindow(Window)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwRestoreWindow")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void RestoreWindow([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        ///     <para>
        ///     This function maximizes the specified window if it was previously not maximized.
        ///     If the window is already maximized, this function does nothing.
        ///     </para>
        ///     <para>
        ///     If the specified window is a full screen window, this function does
        ///     nothing.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to maximize.</param>
        /// <seealso cref="IconifyWindow(Window)" />
        /// <seealso cref="RestoreWindow(Window)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwMaximizeWindow")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void MaximizeWindow([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        /// This function makes the specified window visible if it was previously hidden. If the
        /// window is already visible or is in full screen mode, this function does nothing.
        /// </summary>
        /// <param name="window">The window to make visible.</param>
        /// <seealso cref="HideWindow(Window)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwShowWindow")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void ShowWindow([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        /// This function hides the specified window if it was previously visible. If the window is
        /// already hidden or is in full screen mode, this function does nothing.
        /// </summary>
        /// <param name="window">The window to hide.</param>
        /// <seealso cref="ShowWindow(Window)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwHideWindow")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void HideWindow([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        ///     <para>
        ///     This function brings the specified window to front and sets input focus. The
        ///     window should already be visible and not iconified.
        ///     </para>
        ///     <para>
        ///     By default, both windowed and full screen mode windows are focused when initially
        ///     created. Set the <see cref="Hint.Focused" /> hint to disable this behavior.
        ///     </para>
        ///     <para>
        ///     <strong>Do not use this function</strong> to steal focus from other applications
        ///     unless you are certain that is what the user wants. Focus stealing can be extremely
        ///     disruptive.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to give input focus.</param>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwFocusWindow")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void FocusWindow([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        /// This function returns the handle of the monitor that the specified window is in full
        /// screen on.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <returns>
        /// The monitor, or <see cref="Monitor.None" /> if the window is in windowed mode or
        /// an error occurred.
        /// </returns>
        /// <seealso cref="SetWindowMonitor(Window, Monitor, int, int, int, int, int)" />
        public static Monitor GetWindowMonitor(Window window)
        {
            IntPtr ptr = glfwGetWindowMonitor(window.Ptr);
            return new Monitor(ptr);
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwGetWindowMonitor(IntPtr window);

        /// <summary>
        ///     <para>
        ///     This function sets the monitor that the window uses for full screen mode or, if
        ///     the monitor is <see cref="Monitor.None" />, makes it windowed mode.
        ///     </para>
        ///     <para>
        ///     When setting a monitor, this function updates the width, height and refresh rate
        ///     of the desired video mode and switches to the video mode closest to it. The window
        ///     position is ignored when setting a monitor.
        ///     </para>
        ///     <para>
        ///     When the monitor is <see cref="Monitor.None" />, the position, width and height
        ///     are used to place the window client area. The refresh rate is ignored when no monitor is
        ///     specified.
        ///     </para>
        ///     <para>
        ///     If you only wish to update the resolution of a full screen window or the size of a
        ///     windowed mode window, see <see cref="SetWindowSize(Window, int, int)" />"/>.
        ///     </para>
        ///     <para>
        ///     When a window transitions from full screen to windowed mode, this function
        ///     restores any previous window settings such as whether it is decorated, floating,
        ///     resizable, has size or aspect ratio limits, etc...
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose monitor, size or video mode to set.</param>
        /// <param name="monitor">
        /// The desired monitor, or <see cref="Monitor.None" /> to set windowed
        /// mode.
        /// </param>
        /// <param name="xpos">
        /// The desired x-coordinate of the upper-left corner of the client
        /// area.
        /// </param>
        /// <param name="ypos">
        /// The desired y-coordinate of the upper-left corner of the client
        /// area.
        /// </param>
        /// <param name="width">
        /// The desired with, in screen coordinates, of the client area or video
        /// mode.
        /// </param>
        /// <param name="height">
        /// The desired height, in screen coordinates, of the client area or
        /// video mode.
        /// </param>
        /// <param name="refreshRate">
        /// The desired refresh rate, in Hz, of the video mode, or
        /// <see cref="DontCare" />.
        /// </param>
        /// <seealso cref="GetWindowMonitor(Window)" />
        /// <seealso cref="SetWindowSize(Window, int, int)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetWindowMonitor")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SetWindowMonitor([MarshalAs(UnmanagedType.Struct)] Window window, [MarshalAs(UnmanagedType.Struct)] Monitor monitor, int xpos, int ypos, int width, int height,
            int refreshRate);

        /// <summary>
        /// This function returns the value of an attribute of the specified window or its OpenGL or
        /// OpenGL ES context.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <param name="attrib">The window attribute whose value to return.</param>
        /// <returns>The value of the attribute, or <c>false</c> if an error occurred.</returns>
        /// <remarks>
        /// Framebuffer related hints are not window attributes.
        /// </remarks>
        public static bool GetWindowAttrib(Window window, WindowAttrib attrib)
        {
            return glfwGetWindowAttrib(window.Ptr, Convert.ToInt32(attrib)) != 0;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern int glfwGetWindowAttrib(IntPtr window, int attrib);

        /// <summary>
        /// This function sets the user-defined pointer of the specified window. The current value
        /// is retained until the window is destroyed. The initial value is <c>IntPtr.Zero</c>.
        /// </summary>
        /// <param name="window">The window whose pointer to set.</param>
        /// <param name="ptr">The new value.</param>
        /// <seealso cref="GetWindowUserPointer(Window)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetWindowUserPointer")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SetWindowUserPointer([MarshalAs(UnmanagedType.Struct)] Window window, IntPtr ptr);

        /// <summary>
        /// This function sets the user-defined pointer of the specified window. The initial value
        /// is <c>IntPtr.Zero</c>.
        /// </summary>
        /// <param name="window">The window whose pointer to return.</param>
        /// <returns>The user-defined pointer of the specified window.</returns>
        /// <seealso cref="SetWindowUserPointer(Window, IntPtr)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetWindowUserPointer")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetWindowUserPointer([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        /// This function sets the position callback of the specified window, which is called when
        /// the window is moved. The callback is provided with the screen position of the upper-left
        /// corner of the client area of the window.
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetWindowPosCallback(Window window, WindowPosFunc callback)
        {
            glfwSetWindowPosCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetWindowPosCallback(IntPtr window, IntPtr callback);

        /// <summary>
        /// This function sets the size callback of the specified window, which is called when the
        /// window is resized. The callback is provided with the size, in screen coordinates, of the
        /// client area of the window.
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetWindowSizeCallback(Window window, WindowSizeFunc callback)
        {
            glfwSetWindowSizeCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetWindowSizeCallback(IntPtr window, IntPtr callback);

        /// <summary>
        ///     <para>
        ///     This function sets the close callback of the specified window, which is called
        ///     when the user attempts to close the window, for example by clicking the close widget in
        ///     the title bar.
        ///     </para>
        ///     <para>
        ///     The close flag is set before this callback is called, but you can modify it at any
        ///     time with <see cref="SetWindowShouldClose(Window, bool)" />.
        ///     </para>
        ///     <para>The close callback is not triggered by <see cref="DestroyWindow(Window)" />.</para>
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        /// <remarks>
        /// <strong>OSX:</strong> Selecting Quit from the application menu will trigger the close
        /// callback for all windows.
        /// </remarks>
        public static void SetWindowCloseCallback(Window window, WindowCloseFunc callback)
        {
            glfwSetWindowCloseCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetWindowCloseCallback(IntPtr window, IntPtr callback);

        /// <summary>
        ///     <para>
        ///     This function sets the refresh callback of the specified window, which is called
        ///     when the client area of the window needs to be redrawn, for example if the window has
        ///     been exposed after having been covered by another window.
        ///     </para>
        ///     <para>
        ///     On compositing window systems such as Aero, Compiz or Aqua, where the window
        ///     contents are saved off-screen, this callback may be called only very infrequently or
        ///     never at all.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetWindowRefreshCallback(Window window, WindowRefreshFunc callback)
        {
            glfwSetWindowRefreshCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetWindowRefreshCallback(IntPtr window, IntPtr callback);

        /// <summary>
        ///     <para>
        ///     This function sets the focus callback of the specified window, which is called
        ///     when the window gains or loses input focus.
        ///     </para>
        ///     <para>
        ///     After the focus callback is called for a window that lost input focus, synthetic
        ///     key and mouse button release events will be generated for all such that had been
        ///     pressed. For more information, see <see cref="SetKeyCallback(Window, KeyFunc)" /> and
        ///     <see cref="SetMouseButtonCallback(Window, MouseButtonFunc)" />.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetWindowFocusCallback(Window window, WindowFocusFunc callback)
        {
            glfwSetWindowFocusCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetWindowFocusCallback(IntPtr window, IntPtr callback);

        /// <summary>
        /// This function sets the iconification callback of the specified window, which is called
        /// when the window is iconified or restored.
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetWindowIconifyCallback(Window window, WindowIconifyFunc callback)
        {
            glfwSetWindowIconifyCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetWindowIconifyCallback(IntPtr window, IntPtr callback);

        /// <summary>
        /// This function sets the framebuffer resize callback of the specified window, which is
        /// called when the framebuffer of the specified window is resized.
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetFramebufferSizeCallback(Window window, FramebufferSizeFunc callback)
        {
            glfwSetFramebufferSizeCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetFramebufferSizeCallback(IntPtr window, IntPtr callback);

        /// <summary>
        ///     <para>
        ///     This function processes only those events that are already in the event queue and
        ///     then returns immediately. Processing events will cause the window and input callbacks
        ///     associated with those events to be called.
        ///     </para>
        ///     <para>
        ///     On some platforms, a window move, resize or menu operation will cause event
        ///     processing to block. This is due to how event processing is designed on those platforms.
        ///     You can use the window refresh callback to redraw the contents of your window when
        ///     necessary during such operations.
        ///     </para>
        ///     <para>
        ///     On some platforms, certain events are sent directly to the application without
        ///     going through the event queue, causing callbacks to be called outside of a call to one
        ///     of the event processing functions.
        ///     </para>
        ///     <para>Event processing is not required for joystick input to work.</para>
        /// </summary>
        /// <seealso cref="WaitEvents" />
        /// <seealso cref="WaitEventsTimeout(double)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwPollEvents")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void PollEvents();

        /// <summary>
        ///     <para>
        ///     This function puts the calling thread to sleep until at least one event is
        ///     available in the event queue. Once one or more events are available, it behaves exactly
        ///     like <see cref="PollEvents" />, i.e. the events in the queue are processed and the
        ///     function then returns immediately. Processing events will cause the window and input
        ///     callbacks associated with those events to be called.
        ///     </para>
        ///     <para>
        ///     Since not all events are associated with callbacks, this function may return
        ///     without a callback having been called even if you are monitoring all callbacks.
        ///     </para>
        ///     <para>
        ///     On some platforms, a window move, resize or menu operation will cause event
        ///     processing to block. This is due to how event processing is designed on those platforms.
        ///     You can use the window refresh callback to redraw the contents of your window when
        ///     necessary during such operations.
        ///     </para>
        ///     <para>
        ///     On some platforms, certain callbacks may be called outside of a call to one of the
        ///     event processing functions.
        ///     </para>
        ///     <para>
        ///     If no windows exist, this function returns immediately. For synchronization of
        ///     threads in applications that do not create windows, use your threading library of
        ///     choice.
        ///     </para>
        ///     <para>Event processing is not required for joystick input to work.</para>
        /// </summary>
        /// <seealso cref="PollEvents" />
        /// <seealso cref="WaitEventsTimeout(double)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwWaitEvents")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void WaitEvents();

        /// <summary>
        ///     <para>
        ///     This function puts the calling thread to sleep until at least one event is
        ///     available in the event queue, or until the specified timeout is reached. Once one or
        ///     more events are available, it behaves exactly like <see cref="PollEvents" />, i.e. the
        ///     events in the queue are processed and the function then returns immediately. Processing
        ///     events will cause the window and input callbacks associated with those events to be
        ///     called.
        ///     </para>
        ///     <para>
        ///     Since not all events are associated with callbacks, this function may return
        ///     without a callback having been called even if you are monitoring all callbacks.
        ///     </para>
        ///     <para>
        ///     On some platforms, a window move, resize or menu operation will cause event
        ///     processing to block. This is due to how event processing is designed on those platforms.
        ///     You can use the window refresh callback to redraw the contents of your window when
        ///     necessary during such operations.
        ///     </para>
        ///     <para>
        ///     On some platforms, certain callbacks may be called outside of a call to one of the
        ///     event processing functions.
        ///     </para>
        ///     <para>
        ///     If no windows exist, this function returns immediately. For synchronization of
        ///     threads in applications that do not create windows, use your threading library of
        ///     choice.
        ///     </para>
        ///     <para>Event processing is not required for joystick input to work.</para>
        /// </summary>
        /// <param name="timeout">The maximum amount of time, in seconds, to wait.</param>
        /// <seealso cref="PollEvents" />
        /// <seealso cref="WaitEvents" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwWaitEventsTimeout")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void WaitEventsTimeout(double timeout);

        /// <summary>
        ///     <para>
        ///     This function posts an empty event from the current thread to the event queue,
        ///     causing <see cref="WaitEvents" /> or <see cref="WaitEventsTimeout(double)" /> to
        ///     return.
        ///     </para>
        ///     <para>
        ///     If no windows exist, this function returns immediately. For synchronization of
        ///     threads in applications that do not create windows, use your threading library of
        ///     choice.
        ///     </para>
        /// </summary>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwPostEmptyEvent")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void PostEmptyEvent();

        /// <summary>
        /// This function returns the value of an input option for the specified window.
        /// </summary>
        /// <param name="window">The window to query.</param>
        /// <param name="mode">One of <see cref="InputMode" />.</param>
        /// <returns>The value of an input option for the specified window.</returns>
        public static int GetInputMode(Window window, InputMode mode)
        {
            return glfwGetInputMode(window.Ptr, Convert.ToInt32(mode));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern int glfwGetInputMode(IntPtr window, int mode);

        /// <summary>
        ///     <para>This function sets an input mode option for the specified window.</para>
        ///     <para>
        ///     If the mode is <see cref="InputMode.Cursor" />, the value must be one of the
        ///     following cursor modes:
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///         <see cref="CursorMode.Normal" /> makes the cursor visible and behaving
        ///         normally.
        ///         </item>
        ///         <item>
        ///         <see cref="CursorMode.Hidden" /> makes the cursor invisible when it is over the
        ///         client area of the window but does not restrict the cursor from leaving.
        ///         </item>
        ///         <item>
        ///         <see cref="CursorMode.Disabled" /> hides and grabs the cursor, providing
        ///         virtual and unlimited cursor movement.This is useful for implementing for example 3D
        ///         camera controls.
        ///         </item>
        ///     </list>
        ///     <para>
        ///     If the mode is <see cref="InputMode.StickyKeys" />, the value must be either
        ///     <c>true</c> to enable sticky keys, or <c>false</c> to disable it. If sticky keys are
        ///     enabled, a key press will ensure that <see cref="GetKey(Window, int)" /> returns
        ///     <see cref="InputState.Press" /> the next time it is called even if the key had been
        ///     released before the call. This is useful when you are only interested in whether keys
        ///     have been pressed but not when or in which order.
        ///     </para>
        ///     <para>
        ///     If the mode is <see cref="InputMode.StickyMouseButton" />, the value must be either
        ///     <c>true</c> to enable sticky mouse buttons, or <c>false</c> to disable it. If sticky
        ///     mouse buttons are enabled, a mouse button press will ensure that
        ///     <see cref="GetMouseButton(Window,MouseButton)" /> returns <see cref="InputState.Press" /> the
        ///     next time it is called even if the mouse button had been released before the call. This
        ///     is useful when you are only interested in whether mouse buttons have been pressed but
        ///     not when or in which order.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose input mode to set.</param>
        /// <param name="mode">One of <see cref="InputMode" />.</param>
        /// <param name="value">The new value of the specified input mode.</param>
        /// <seealso cref="GetInputMode(Window, InputMode)" />
        public static void SetInputMode(Window window, InputMode mode, CursorMode value)
        {
            glfwSetInputMode(window.Ptr, Convert.ToInt32(mode), Convert.ToInt32(value));
        }

        /// <summary>
        ///     <para>This function sets an input mode option for the specified window.</para>
        ///     <para>
        ///     If the mode is <see cref="InputMode.Cursor" />, the value must be one of the
        ///     following cursor modes:
        ///     </para>
        ///     <list type="bullet">
        ///         <item>
        ///         <see cref="CursorMode.Normal" /> makes the cursor visible and behaving
        ///         normally.
        ///         </item>
        ///         <item>
        ///         <see cref="CursorMode.Hidden" /> makes the cursor invisible when it is over the
        ///         client area of the window but does not restrict the cursor from leaving.
        ///         </item>
        ///         <item>
        ///         <see cref="CursorMode.Disabled" /> hides and grabs the cursor, providing
        ///         virtual and unlimited cursor movement.This is useful for implementing for example 3D
        ///         camera controls.
        ///         </item>
        ///     </list>
        ///     <para>
        ///     If the mode is <see cref="InputMode.StickyKeys" />, the value must be either
        ///     <c>true</c> to enable sticky keys, or <c>false</c> to disable it. If sticky keys are
        ///     enabled, a key press will ensure that <see cref="GetKey(Window, int)" /> returns
        ///     <see cref="InputState.Press" /> the next time it is called even if the key had been
        ///     released before the call. This is useful when you are only interested in whether keys
        ///     have been pressed but not when or in which order.
        ///     </para>
        ///     <para>
        ///     If the mode is <see cref="InputMode.StickyMouseButton" />, the value must be either
        ///     <c>true</c> to enable sticky mouse buttons, or <c>false</c> to disable it. If sticky
        ///     mouse buttons are enabled, a mouse button press will ensure that
        ///     <see cref="GetMouseButton(Window,MouseButton)" /> returns <see cref="InputState.Press" /> the
        ///     next time it is called even if the mouse button had been released before the call. This
        ///     is useful when you are only interested in whether mouse buttons have been pressed but
        ///     not when or in which order.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose input mode to set.</param>
        /// <param name="mode">One of <see cref="InputMode" />.</param>
        /// <param name="value">The new value of the specified input mode.</param>
        /// <seealso cref="GetInputMode(Window, InputMode)" />
        public static void SetInputMode(Window window, InputMode mode, bool value)
        {
            glfwSetInputMode(window.Ptr, Convert.ToInt32(mode), value ? 1 : 0);
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void glfwSetInputMode(IntPtr window, int mode, int value);

        /// <summary>
        ///     <para>
        ///     This function returns the localized name of the specified printable key. This is
        ///     intended for displaying key bindings to the user.
        ///     </para>
        ///     <para>
        ///     If the key is <see cref="KeyCode.Unknown" />, the scancode is used instead,
        ///     otherwise the scancode is ignored. If a non-printable key or (if the key is
        ///     <see cref="KeyCode.Unknown" />) a scancode that maps to a non-printable key is specified,
        ///     this function returns <c>null</c>.
        ///     </para>
        ///     <para>This behavior allows you to pass in the arguments passed to the key callback without modification.</para>
        ///     <para>The printable keys are:</para>
        ///     <list type="bullet">
        ///         <item>
        ///             <see cref="KeyCode.Apostrophe" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.Comma" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.Minus" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.Period" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.Slash" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.SemiColon" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.Equal" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.LeftBracket" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.RightBracket" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.Backslash" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.World1" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.World2" />
        ///         </item>
        ///         <item><see cref="KeyCode.Alpha0" /> to <see cref="KeyCode.Alpha9" /></item>
        ///         <item><see cref="KeyCode.A" /> to <see cref="KeyCode.Z" /></item>
        ///         <item><see cref="KeyCode.Numpad0" /> to <see cref="KeyCode.Numpad9" /></item>
        ///         <item>
        ///             <see cref="KeyCode.NumpadDecimal" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.NumpadDivide" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.NumpadMultiply" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.NumpadSubtract" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.NumpadAdd" />
        ///         </item>
        ///         <item>
        ///             <see cref="KeyCode.NumpadEqual" />
        ///         </item>
        ///     </list>
        /// </summary>
        /// <param name="key">The key to query, or <see cref="KeyCode.Unknown" />.</param>
        /// <param name="scancode">The scancode of the key to query.</param>
        /// <returns>The localized name of the key, or <c>null</c>.</returns>
        public static string GetKeyName(KeyCode key, int scancode)
        {
            return Marshal.PtrToStringAnsi(glfwGetKeyName(Convert.ToInt32(key), scancode));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwGetKeyName(int key, int scancode);

        /// <summary>
        ///     <para>
        ///     This function returns the last state reported for the specified key to the
        ///     specified window. The returned state is one of <see cref="InputState.Press" /> or
        ///     <see cref="InputState.Release" />. The higher-level action
        ///     <see cref="InputState.Repeat" /> is only reported to the key callback.
        ///     </para>
        ///     <para>
        ///     If the <see cref="InputMode.StickyKeys" /> input mode is enabled, this function
        ///     returns <see cref="InputState.Press" /> the first time you call it for a key that was
        ///     pressed, even if that key has already been released.
        ///     </para>
        ///     <para>
        ///     The key functions deal with physical keys, with key tokens named after their use
        ///     on the standard US keyboard layout. If you want to input text, use the Unicode character
        ///     callback instead.
        ///     </para>
        ///     <para>
        ///     The modifier key bit masks are not key tokens and cannot be used with this
        ///     function.
        ///     </para>
        ///     <para><strong>Do not use this function</strong> to implement text input.</para>
        /// </summary>
        /// <param name="window">The desired window.</param>
        /// <param name="key">
        /// The desired keyboard key. <see cref="KeyCode.Unknown" /> is not a valid
        /// key for this function.
        /// </param>
        /// <returns>
        /// One of <see cref="InputState.Press" /> or
        /// <see cref="InputState.Release" />.
        /// </returns>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetKey")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetKey([MarshalAs(UnmanagedType.Struct)] Window window, int key);

        /// <summary>
        ///     <para>
        ///     This function returns the last state reported for the specified mouse button to
        ///     the specified window. The returned state is one of <see cref="InputState.Press" /> or
        ///     <see cref="InputState.Release" />.
        ///     </para>
        ///     <para>
        ///     If the <see cref="InputMode.StickyKeys" /> input mode is enabled, this function
        ///     <see cref="InputState.Press" /> the first time you call it for a mouse button that was
        ///     pressed, even if that mouse button has already been released.
        ///     </para>
        /// </summary>
        /// <param name="window">The desired window.</param>
        /// <param name="button">The desired mouse button.</param>
        /// <returns><c>true</c> if the button was pressed, <c>false</c> otherwise.</returns>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetMouseButton")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetMouseButton([MarshalAs(UnmanagedType.Struct)] Window window, MouseButton button);

        /// <summary>
        ///     <para>
        ///     This function returns the position of the cursor, in screen coordinates, relative
        ///     to the upper-left corner of the client area of the specified window.
        ///     </para>
        ///     <para>
        ///     If the cursor is disabled (with <see cref="CursorMode.Disabled" />) then the cursor
        ///     position is unbounded and limited only by the minimum and maximum values of a
        ///     <c>double</c>.
        ///     </para>
        ///     <para>
        ///     The coordinate can be converted to their integer equivalents with the <c>floor</c>
        ///     function. Casting directly to an integer type works for positive coordinates, but fails
        ///     for negative ones.
        ///     </para>
        /// </summary>
        /// <param name="window">The desired window.</param>
        /// <param name="xpos">
        /// Where to store the cursor x-coordinate, relative to the left edge of
        /// the client area.
        /// </param>
        /// <param name="ypos">
        /// Where to store the cursor y-coordinate, relative to the to top edge
        /// of the client area.
        /// </param>
        /// <seealso cref="SetCursorPos(Window, double, double)" />
        public static unsafe void GetCursorPos(Window window, out double xpos, out double ypos)
        {
            double xx, yy;
            glfwGetCursorPos(window.Ptr, &xx, &yy);
            xpos = xx;
            ypos = yy;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe void glfwGetCursorPos(IntPtr window, double* xpos, double* ypos);

        /// <summary>
        ///     <para>
        ///     This function sets the position, in screen coordinates, of the cursor relative to
        ///     the upper-left corner of the client area of the specified window. The window must have
        ///     input focus. If the window does not have input focus when this function is called, it
        ///     fails silently.
        ///     </para>
        ///     <para>
        ///     <strong>Do not use this function</strong> to implement things like camera
        ///     controls. GLFW already provides the <see cref="CursorMode.Disabled" /> cursor mode
        ///     that hides the cursor, transparently re-centers it and provides unconstrained cursor
        ///     motion. See <see cref="SetInputMode(Window, InputMode, CursorMode)" /> for more
        ///     information.
        ///     </para>
        ///     <para>
        ///     If the cursor mode is <see cref="CursorMode.Disabled" /> then the cursor
        ///     position is unconstrained and limited only by the minimum and maximum values of a
        ///     <c>double</c>.
        ///     </para>
        /// </summary>
        /// <param name="window">The desired window.</param>
        /// <param name="xpos">
        /// The desired x-coordinate, relative to the left edge of the client
        /// area.
        /// </param>
        /// <param name="ypos">
        /// The desired y-coordinate, relative to the top edge of the client
        /// area.
        /// </param>
        /// <seealso cref="GetCursorPos(Window, out double, out double)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetCursorPos")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SetCursorPos([MarshalAs(UnmanagedType.Struct)] Window window, double xpos, double ypos);

        /// <summary>
        ///     <para>
        ///     Creates a new custom cursor image that can be set for a window with
        ///     <see cref="SetCursor(Window, Cursor)" />. The cursor can be destroyed with
        ///     <see cref="DestroyCursor(Cursor)" />. Any remaining cursors are destroyed by
        ///     <see cref="Terminate" />.
        ///     </para>
        ///     <para>
        ///     The pixels are 32-bit, little-endian, non-premultiplied RGBA, i.e. eight bits per
        ///     channel.  They are arranged canonically as packed sequential rows, starting from the
        ///     top-left corner.
        ///     </para>
        ///     <para>
        ///     The cursor hotspot is specified in pixels, relative to the upper-left corner of
        ///     the cursor image. Like all other coordinate systems in GLFW, the X-axis points to the
        ///     right and the Y-axis points down.
        ///     </para>
        /// </summary>
        /// <param name="image">The desired cursor image.</param>
        /// <param name="xhot">The desired x-coordinate, in pixels, of the cursor hotspot.</param>
        /// <param name="yhot">The desired y-coordinate, in pixels, of the cursor hotspot.</param>
        /// <returns>The handle of the created cursor.</returns>
        /// <seealso cref="DestroyCursor(Cursor)" />
        /// <seealso cref="CreateStandardCursor(CursorType)" />
        public static unsafe Cursor CreateCursor(Image image, int xhot, int yhot)
        {
            int size = image.Width * image.Width * 4;

            var img = new InternalImage
            {
                Width = image.Width,
                Height = image.Width,
                Pixels = Marshal.AllocHGlobal(size)
            };

            Marshal.Copy(image.Pixels, 0, img.Pixels, Math.Min(size, image.Pixels.Length));

            var ptr = new IntPtr(&img);
            ptr = glfwCreateCursor(ptr, xhot, yhot);

            Marshal.FreeHGlobal(img.Pixels);

            return new Cursor(ptr);
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwCreateCursor(IntPtr image, int xhot, int yhot);

        /// <summary>
        /// Returns a cursor with a standard shape, that can be set for a window with
        /// <see cref="CreateCursor(Image, int, int)" />.
        /// </summary>
        /// <param name="cursor">One of the standard shapes.</param>
        /// <returns>A new cursor ready to use.</returns>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwCreateStandardCursor")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern Cursor CreateStandardCursor(CursorType cursor);

        /// <summary>
        /// This function destroys a cursor previously created with
        /// <see cref="CreateCursor(Image, int, int)" />. Any remaining cursors will be destroyed by
        /// <see cref="Terminate" />.
        /// </summary>
        /// <param name="cursor">The cursor object to destroy.</param>
        /// <seealso cref="CreateCursor(Image, int, int)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwDestroyCursor")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void DestroyCursor([MarshalAs(UnmanagedType.Struct)] Cursor cursor);

        /// <summary>
        ///     <para>
        ///     This function sets the cursor image to be used when the cursor is over the client
        ///     area of the specified window.The set cursor will only be visible when the cursor mode
        ///     of the window is <see cref="CursorMode.Normal" />.
        ///     </para>
        ///     <para>
        ///     On some platforms, the set cursor may not be visible unless the window also has
        ///     input focus.
        ///     </para>
        /// </summary>
        /// <param name="window">The window to set the cursor for.</param>
        /// <param name="cursor">The cursor to set.</param>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetCursor")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SetCursor([MarshalAs(UnmanagedType.Struct)] Window window, [MarshalAs(UnmanagedType.Struct)] Cursor cursor);

        /// <summary>
        ///     <para>
        ///     This function sets the key callback of the specified window, which is called when
        ///     a key is pressed, repeated or released.
        ///     </para>
        ///     <para>
        ///     The key functions deal with physical keys, with layout independent key tokens
        ///     named after their values in the standard US keyboard layout. If you want to input text,
        ///     use the <see cref="SetCharCallback(Window, CharFunc)" /> instead.
        ///     </para>
        ///     <para>
        ///     When a window loses input focus, it will generate synthetic key release events for
        ///     all pressed keys. You can tell these events from user-generated events by the fact that
        ///     the synthetic ones are generated after the focus loss event has been processed, i.e.
        ///     after the <see cref="SetWindowFocusCallback(Window, WindowFocusFunc)" /> has been
        ///     called.
        ///     </para>
        ///     <para>
        ///     The scancode of a key is specific to that platform or sometimes even to that
        ///     machine. Scancodes are intended to allow users to bind keys that don't have a GLFW key
        ///     token. Such keys have <c>key</c> set to <see cref="KeyCode.Unknown" />, their state is
        ///     not saved and so it cannot be queried with <see cref="GetKey(Window, int)" />.
        ///     </para>
        ///     <para>
        ///     Sometimes GLFW needs to generate synthetic key events, in which case the scancode
        ///     may be zero.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetKeyCallback(Window window, KeyFunc callback)
        {
            glfwSetKeyCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetKeyCallback(IntPtr window, IntPtr callback);

        /// <summary>
        ///     <para>
        ///     This function sets the character callback of the specified window, which is called
        ///     when a Unicode character is input.
        ///     </para>
        ///     <para>
        ///     The character callback is intended for Unicode text input. As it deals with
        ///     characters, it is keyboard layout dependent, whereas
        ///     <see cref="SetKeyCallback(Window, KeyFunc)" /> is not. Characters do not map 1:1 to
        ///     physical keys, as a key may produce zero, one or more characters. If you want to know
        ///     whether a specific physical key was pressed or released, see the key callback
        ///     instead.
        ///     </para>
        ///     <para>
        ///     The character callback behaves as system text input normally does and will not be
        ///     called if modifier keys are held down that would prevent normal text input on that
        ///     platform, for example a Super (Command) key on OS X or Alt key on Windows. There is
        ///     <see cref="SetCharModsCallback(Window, CharModsFunc)" /> that receives these
        ///     events.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetCharCallback(Window window, CharFunc callback)
        {
            glfwSetCharCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetCharCallback(IntPtr window, IntPtr callback);

        /// <summary>
        ///     <para>
        ///     This function sets the character with modifiers callback of the specified window,
        ///     which is called when a Unicode character is input regardless of what modifier keys are
        ///     used.
        ///     </para>
        ///     <para>
        ///     The character with modifiers callback is intended for implementing custom Unicode
        ///     character input. For regular Unicode text input, use
        ///     <see cref="SetCharCallback(Window, CharFunc)" />. Like the character callback, the
        ///     character with modifiers callback deals with characters and is keyboard layout
        ///     dependent. Characters do not map 1:1 to physical keys, as a key may produce zero, one or
        ///     more characters.If you want to know whether a specific physical key was pressed or
        ///     released, see <see cref="SetKeyCallback(Window, KeyFunc)" /> instead.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetCharModsCallback(Window window, CharModsFunc callback)
        {
            glfwSetCharModsCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetCharModsCallback(IntPtr window, IntPtr callback);

        /// <summary>
        ///     <para>
        ///     This function sets the mouse button callback of the specified window, which is
        ///     called when a mouse button is pressed or released.
        ///     </para>
        ///     <para>
        ///     When a window loses input focus, it will generate synthetic mouse button release
        ///     events for all pressed mouse buttons. You can tell these events from user-generated
        ///     events by the fact that the synthetic ones are generated after the focus loss event has
        ///     been processed, i.e. after <see cref="SetWindowFocusCallback(Window, WindowFocusFunc)" />
        ///     has been called.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetMouseButtonCallback(Window window, MouseButtonFunc callback)
        {
            glfwSetMouseButtonCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetMouseButtonCallback(IntPtr window, IntPtr callback);

        /// <summary>
        /// This function sets the cursor position callback of the specified window, which is called
        /// when the cursor is moved.The callback is provided with the position, in screen
        /// coordinates, relative to the upper-left corner of the client area of the window.
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetCursorPosCallback(Window window, CursorPosFunc callback)
        {
            glfwSetCursorPosCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetCursorPosCallback(IntPtr window, IntPtr callback);

        /// <summary>
        /// This function sets the cursor boundary crossing callback of the specified window, which
        /// is called when the cursor enters or leaves the client area of the window.
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetCursorEnterCallback(Window window, CursorEnterFunc callback)
        {
            glfwSetCursorEnterCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetCursorEnterCallback(IntPtr window, IntPtr callback);

        /// <summary>
        ///     <para>
        ///     This function sets the scroll callback of the specified window, which is called
        ///     when a scrolling device is used, such as a mouse wheel or scrolling area of a
        ///     touchpad.
        ///     </para>
        ///     <para>
        ///     The scroll callback receives all scrolling input, like that from a mouse wheel or
        ///     a touchpad scrolling area.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetScrollCallback(Window window, CursorPosFunc callback)
        {
            glfwSetScrollCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetScrollCallback(IntPtr window, IntPtr callback);

        /// <summary>
        /// This function sets the file drop callback of the specified window, which is called when
        /// one or more dragged files are dropped on the window.
        /// </summary>
        /// <param name="window">The window whose callback to set.</param>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetDropCallback(Window window, DropFunc callback)
        {
            glfwSetDropCallback(window.Ptr, Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetDropCallback(IntPtr window, IntPtr callback);

        /// <summary>
        /// This function returns whether the specified joystick is present.
        /// </summary>
        /// <param name="joy">The joystick to query.</param>
        /// <returns><c>true</c> if the joystick is present, or <c>false</c> otherwise.</returns>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwJoystickPresent")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool JoystickPresent(Joystick joy);

        /// <summary>
        ///     <para>
        ///     This function returns the values of all axes of the specified joystick. Each
        ///     element in the array is a value between -1.0 and 1.0.
        ///     </para>
        ///     <para>
        ///     Querying a joystick slot with no device present is not an error, but will cause
        ///     this function to return <c>null</c>. Call <see cref="JoystickPresent(Joystick)" /> to
        ///     check device presence.
        ///     </para>
        /// </summary>
        /// <param name="joy">The joystick to query.</param>
        /// <returns>
        /// An array of axis values, or <c>null</c> if the joystick is not
        /// present.
        /// </returns>
        public static unsafe float[] GetJoystickAxes(Joystick joy)
        {
            int n;
            IntPtr array = glfwGetJoystickAxes((int) joy, &n);

            if (n == 0 || array == IntPtr.Zero)
                return null;

            var axes = new float[n];
            Marshal.Copy(array, axes, 0, n);
            return axes;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe IntPtr glfwGetJoystickAxes(int joy, int* count);

        /// <summary>
        ///     <para>This function returns the state of all buttons of the specified joystick.</para>
        ///     <para>
        ///     Querying a joystick slot with no device present is not an error, but will cause
        ///     this function to return <c>null</c>. Call <see cref="JoystickPresent(Joystick)" /> to
        ///     check device presence.
        ///     </para>
        /// </summary>
        /// <param name="joy">The joystick to query.</param>
        /// <returns>
        /// An array of button states, or <c>null</c> if the joystick is not
        /// present.
        /// </returns>
        public static unsafe bool[] GetJoystickButtons(Joystick joy)
        {
            int n;
            IntPtr array = glfwGetJoystickButtons((int) joy, &n);

            if (n == 0 || array == IntPtr.Zero)
                return null;

            var b = new byte[n];
            Marshal.Copy(array, b, 0, n);

            var buttons = new bool[n];
            for (var i = 0; i < n; i++)
            {
                buttons[i] = b[i] > 0;
            }

            return buttons;
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern unsafe IntPtr glfwGetJoystickButtons(int joy, int* count);

        /// <summary>
        ///     <para>
        ///     This function returns the name, encoded as UTF-8, of the specified
        ///     joystick.
        ///     </para>
        ///     <para>
        ///     Querying a joystick slot with no device present is not an error, but will cause
        ///     this function to return <c>null</c>. Call <see cref="JoystickPresent(Joystick)" /> to
        ///     check device presence.
        ///     </para>
        /// </summary>
        /// <param name="joy">The joystick to query.</param>
        /// <returns>
        /// The UTF-8 encoded name of the joystick, or <c>null</c> if the joystick is not
        /// present.
        /// </returns>
        public static string GetJoystickName(Joystick joy)
        {
            return NativeHelpers.StringFromPtr(glfwGetJoystickName((int) joy));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwGetJoystickName(int joy);

        /// <summary>
        /// This function sets the joystick configuration callback, or removes the currently set
        /// callback. This is called when a joystick is connected to or disconnected from the
        /// system.
        /// </summary>
        /// <param name="callback">
        /// The new callback, or <c>null</c> to remove the currently set
        /// callback.
        /// </param>
        public static void SetJoystickCallback(JoystickFunc callback)
        {
            glfwSetJoystickCallback(Marshal.GetFunctionPointerForDelegate(callback));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwSetJoystickCallback(IntPtr callback);

        /// <summary>
        /// This function sets the system clipboard to the specified, UTF-8 encoded string.
        /// </summary>
        /// <param name="window">The window that will own the clipboard contents.</param>
        /// <param name="value">A UTF-8 encoded string.</param>
        /// <seealso cref="GetClipboardString(Window)" />
        public static void SetClipboardString(Window window, string value)
        {
            glfwSetClipboardString(window.Ptr, NativeHelpers.StringToPtr(value));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern void glfwSetClipboardString(IntPtr window, IntPtr value);

        /// <summary>
        /// This function returns the contents of the system clipboard, if it contains or is
        /// convertible to a UTF-8 encoded string. If the clipboard is empty or if its contents
        /// cannot be converted, <c>null</c> is returned and a
        /// <see cref="ErrorCode.FormatUnavailable" /> error is generated.
        /// </summary>
        /// <param name="window">The window that will request the clipboard contents.</param>
        /// <returns>
        /// The contents of the clipboard as a UTF-8 encoded string, or <c>null</c> if an
        /// error occurred.
        /// </returns>
        /// <seealso cref="SetClipboardString(Window, string)" />
        public static string GetClipboardString(Window window)
        {
            return NativeHelpers.StringFromPtr(glfwGetClipboardString(window.Ptr));
        }

        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl)]
        [SuppressUnmanagedCodeSecurity]
        private static extern IntPtr glfwGetClipboardString(IntPtr window);

        /// <summary>
        ///     <para>
        ///     This function returns the value of the GLFW timer. Unless the timer has been set
        ///     using <see cref="SetTime(double)" />, the timer measures time elapsed since GLFW was
        ///     initialized.
        ///     </para>
        ///     <para>
        ///     The resolution of the timer is system dependent, but is usually on the order of a
        ///     few micro- or nanoseconds. It uses the highest-resolution monotonic time source on each
        ///     supported platform.
        ///     </para>
        /// </summary>
        /// <returns>The current value, in seconds, or zero if an error occurred.</returns>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetTime")]
        [SuppressUnmanagedCodeSecurity]
        public static extern double GetTime();

        /// <summary>
        /// This function sets the value of the GLFW timer. It then continues to count up from that
        /// value. The value must be a positive finite number less than or equal to 18446744073.0
        /// which is approximately 584.5 years.
        /// </summary>
        /// <param name="time">The new value, in seconds.</param>
        /// <remarks>
        /// The upper limit of the timer is calculated as <c>floor((2^64 - 1) / 10^9)</c> and is due
        /// to implementations storing nanoseconds in 64 bits. The limit may be increased in the
        /// future.
        /// </remarks>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSetTime")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SetTime(double time);

        /// <summary>
        /// This function returns the current value of the raw timer, measured in <c>1/frequency</c>
        /// seconds. To get the frequency, call <see cref="GetTimerFrequency" />.
        /// </summary>
        /// <returns>The value of the timer, or zero if an error occurred.</returns>
        /// <seealso cref="GetTimerFrequency" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetTimerValue")]
        [SuppressUnmanagedCodeSecurity]
        public static extern ulong GetTimerValue();

        /// <summary>
        /// This function returns the frequency, in Hz, of the raw timer.
        /// </summary>
        /// <returns>The frequency of the timer, in Hz, or zero if an error occurred</returns>
        /// <seealso cref="GetTimerValue" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetTimerFrequency")]
        [SuppressUnmanagedCodeSecurity]
        public static extern ulong GetTimerFrequency();

        /// <summary>
        ///     <para>
        ///     This function makes the OpenGL or OpenGL ES context of the specified window
        ///     current on the calling thread. A context can only be made current on a single thread at
        ///     a time and each thread can have only a single current context at a time.
        ///     </para>
        ///     <para>
        ///     By default, making a context non-current implicitly forces a pipeline flush. On
        ///     machines that support <c>GL_KHR_context_flush_control</c>, you can control whether a
        ///     context performs this flush by setting the <see cref="Hint.ContextReleaseBehavior" />
        ///     window hint.
        ///     </para>
        ///     <para>
        ///     The specified window must have an OpenGL or OpenGL ES context. Specifying a window
        ///     without a context will generate a <see cref="ErrorCode.NoWindowContext" /> error.
        ///     </para>
        /// </summary>
        /// <param name="window">
        /// The window whose context to make current, or
        /// <see cref="Window.None" /> to detach the current context.
        /// </param>
        /// <seealso cref="GetCurrentContext" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwMakeContextCurrent")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void MakeContextCurrent([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        /// This function returns the window whose OpenGL or OpenGL ES context is current on the
        /// calling thread.
        /// </summary>
        /// <returns>
        /// The window whose context is current, or <see cref="Window.None" /> if no
        /// window's context is current.
        /// </returns>
        /// <seealso cref="MakeContextCurrent(Window)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwGetCurrentContext")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Struct)]
        public static extern Window GetCurrentContext();

        /// <summary>
        ///     <para>
        ///     This function swaps the front and back buffers of the specified window when
        ///     rendering with OpenGL or OpenGL ES. If the swap interval is greater than zero, the GPU
        ///     driver waits the specified number of screen updates before swapping the buffers.
        ///     </para>
        ///     <para>
        ///     The specified window must have an OpenGL or OpenGL ES context. Specifying a window
        ///     without a context will generate a <see cref="ErrorCode.NoWindowContext" /> error.
        ///     </para>
        /// </summary>
        /// <param name="window">The window whose buffers to swap.</param>
        /// <remarks>
        /// <strong>EGL:</strong> The context of the specified window must be current on the
        /// calling thread.
        /// </remarks>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSwapBuffers")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SwapBuffers([MarshalAs(UnmanagedType.Struct)] Window window);

        /// <summary>
        ///     <para>
        ///     This function sets the swap interval for the current OpenGL or OpenGL ES context,
        ///     i.e. the number of screen updates to wait from the time
        ///     <see cref="SwapBuffers(Window)" /> was called before swapping the buffers and returning.
        ///     This is sometimes called <em>vertical synchronization</em>,
        ///     <em>
        ///     vertical retrace
        ///     synchronization
        ///     </em>
        ///     or just <em>vsync</em>.
        ///     </para>
        ///     <para>
        ///     Contexts that support either of the <c>WGL_EXT_swap_control_tear</c> and
        ///     <c>GLX_EXT_swap_control_tear</c> extensions also accept negative swap intervals, which
        ///     allow the driver to swap even if a frame arrives a little bit late.
        ///     </para>
        ///     <para>
        ///     You can check for the presence of these extensions using
        ///     <see cref="ExtensionSupported(string)" />. For more information about swap tearing, see
        ///     the extension specifications.
        ///     </para>
        ///     <para>
        ///     A context must be current on the calling thread. Calling this function without a
        ///     current context will cause a <see cref="ErrorCode.NoCurrentContext" /> error.
        ///     </para>
        /// </summary>
        /// <param name="interval">
        /// The minimum number of screen updates to wait for until the
        /// buffers are swapped by <see cref="SwapBuffers(Window)" />.
        /// </param>
        /// <remarks>
        ///     <para>
        ///     This function is not called during context creation, leaving the swap interval set
        ///     to whatever is the default on that platform. This is done because some swap interval
        ///     extensions used by GLFW do not allow the swap interval to be reset to zero once it has
        ///     been set to a non-zero value.
        ///     </para>
        ///     <para>
        ///     Some GPU drivers do not honor the requested swap interval, either because of a
        ///     user setting that overrides the application's request or due to bugs in the
        ///     driver.
        ///     </para>
        /// </remarks>
        /// <seealso cref="SwapBuffers(Window)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwSwapInterval")]
        [SuppressUnmanagedCodeSecurity]
        public static extern void SwapInterval(int interval);

        /// <summary>
        ///     <para>
        ///     This function returns whether the specified API extension is supported by the
        ///     current OpenGL or OpenGL ES context. It searches both for client API extension and
        ///     context creation API extensions.
        ///     </para>
        ///     <para>
        ///     A context must be current on the calling thread. Calling this function without a
        ///     current context will cause a <see cref="ErrorCode.NoCurrentContext" /> error.
        ///     </para>
        ///     <para>
        ///     As this functions retrieves and searches one or more extension strings each call,
        ///     it is recommended that you cache its results if it is going to be used frequently. The
        ///     extension strings will not change during the lifetime of a context, so there is no
        ///     danger in doing this.
        ///     </para>
        /// </summary>
        /// <param name="extension">The ASCII encoded name of the extension.</param>
        /// <returns><c>true</c> if the extension is available, or <c>false</c> otherwise.</returns>
        /// <seealso cref="GetProcAddress(string)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "glfwExtensionSupported")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool ExtensionSupported(string extension);

        /// <summary>
        ///     <para>
        ///     This function returns the address of the specified OpenGL or OpenGL ES core or
        ///     extension function, if it is supported by the current context.
        ///     </para>
        ///     <para>
        ///     A context must be current on the calling thread. Calling this function without a
        ///     current context will cause a <see cref="ErrorCode.NoCurrentContext" /> error.
        ///     </para>
        /// </summary>
        /// <param name="procname">The ASCII encoded name of the function.</param>
        /// <returns>The address of the function, or <c>null</c> if an error occurred.</returns>
        /// <remarks>
        ///     <para>
        ///     The address of a given function is not guaranteed to be the same between
        ///     contexts.
        ///     </para>
        ///     <para>
        ///     This function may return a non-<c>null</c> address despite the associated version
        ///     or extension not being available. Always check the context version or extension string
        ///     first.
        ///     </para>
        /// </remarks>
        /// <seealso cref="ExtensionSupported(string)" />
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, EntryPoint = "glfwGetProcAddress")]
        [SuppressUnmanagedCodeSecurity]
        public static extern IntPtr GetProcAddress(string procname);

        /// <summary>
        /// This function returns whether the Vulkan loader has been found. This check is performed
        /// by <see cref="Init" />.
        /// </summary>
        /// <returns><c>true</c> if Vulkan is available, or <c>false</c> otherwise.</returns>
        [DllImport(LIBRARY_NAME, CallingConvention = CallingConvention.Cdecl, EntryPoint = "glfwVulkanSupported")]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool VulkanSupported();
    }
}