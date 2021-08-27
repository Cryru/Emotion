#region Using

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using WinApi.User32;

#endregion

namespace Emotion.Platform.Implementation.CommonDesktop
{
    public abstract class DesktopPlatform : PlatformBase
    {
        protected string _platformIdentifier;
        protected string _platformExtension;

        protected override void Setup(Configurator config)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _platformIdentifier = "win";
                if (RuntimeInformation.OSArchitecture == Architecture.X64 && RuntimeInformation.ProcessArchitecture == Architecture.X64)
                    _platformIdentifier += "64";
                else
                    _platformIdentifier += "32";
                _platformExtension = ".dll";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _platformIdentifier = "linux";
                _platformExtension = ".so";
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _platformIdentifier = "macos";
                _platformExtension = ".dylib";
            }

            base.Setup(config);

            if (Engine.AssetLoader == null) return;
            Engine.Log.Trace("Adding default desktop asset sources.", MessageSource.Platform);
            Engine.AssetLoader.AddSource(new FileAssetSource("Assets"));
            Engine.AssetLoader.AddStore(new FileAssetStore("Player"));
            if (Engine.Configuration.DebugMode && Directory.Exists(DebugAssetStore.AssetDevPath)) Engine.AssetLoader.AddStore(new DebugAssetStore());
        }

        /// <summary>
        /// List of connected monitors.
        /// </summary>
        public List<Monitor> Monitors = new List<Monitor>();

        /// <summary>
        /// Returns the monitor the window is on, or the primary monitor if undetermined.
        /// </summary>
        /// <returns>The monitor the window is on.</returns>
        internal virtual Monitor GetMonitorOfWindow()
        {
            if (Monitors.Count == 0) return null;

            var rect = new Rectangle(Position, Size);
            // ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
            foreach (Monitor monitor in Monitors)
            {
                var monitorRect = new Rectangle(monitor.Position.X, monitor.Position.Y, monitor.Width, monitor.Height);
                if (monitorRect.Contains(rect) || monitorRect.Intersects(rect)) return monitor;
            }

            return Monitors[0];
        }

        protected void UpdateMonitor(Monitor monitor, bool connected, bool first)
        {
            if (connected)
            {
                Engine.Log.Info($"Detected monitor - {monitor.Name} ({monitor.Width}x{monitor.Height}){(first ? " Primary" : "")}", MessageSource.Platform);

                if (first)
                {
                    Monitors.Insert(0, monitor);

                    // Re-initiate fullscreen mode.
                    if (DisplayMode != DisplayMode.Fullscreen) return;
                    DisplayMode = DisplayMode.Windowed;
                    DisplayMode = DisplayMode.Fullscreen;
                }
                else
                {
                    Monitors.Add(monitor);
                }
            }
            else
            {
                Engine.Log.Info($"Disconnected monitor - {monitor.Name} ({monitor.Width}x{monitor.Height}){(first ? " Primary" : "")}", MessageSource.Platform);

                Monitors.Remove(monitor);
                UpdateDisplayMode();
            }
        }

        /// <summary>
        /// The folder in which native libraries are stored by the desktop platform.
        /// </summary>
        public const string DESKTOP_NATIVE_LIB_FOLDER = "AssetsNativeLibs";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string AppendPlatformIdentifierAndExtension(string libFolder, string libName)
        {
            return Path.Join(DESKTOP_NATIVE_LIB_FOLDER, libFolder, _platformIdentifier, $"{libName}{_platformExtension}");
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IntPtr LoadLibrary(string path)
        {
            switch (path)
            {
                case "glfw":
                    path = AppendPlatformIdentifierAndExtension("GLFW", "glfw");
                    break;
                case "libEGL":
                    path = AppendPlatformIdentifierAndExtension("ANGLE", "libEGL");
                    break;
                case "libGLESv2":
                    path = AppendPlatformIdentifierAndExtension("ANGLE", "libGLESv2");
                    break;
                case "mesa":
                    path = AppendPlatformIdentifierAndExtension("Mesa", "opengl32");
                    break;
                case "OpenAL":
                {
                    path = Path.Join(DESKTOP_NATIVE_LIB_FOLDER, "OpenAL", _platformIdentifier);

                    // Linux only dependency, located in the same folder.
                    if (_platformIdentifier == "linux") LoadLibrary(Path.Join(path, "libsndio.so.6.1"));

                    path = Path.Join(path, $"openal32{_platformExtension}");
                    break;
                }
            }

            bool loaded = NativeLibrary.TryLoad(path, out IntPtr ptr);
            if (loaded) return ptr;

            Engine.Log.Info($"Couldn't load library {path}", MessageSource.Engine);
            if (!File.Exists(path)) Engine.Log.Info($"File doesn't exist at {path}", MessageSource.Engine);
            return IntPtr.Zero;
        }

        /// <inheritdoc />
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName)
        {
            return !NativeLibrary.TryGetExport(library, symbolName, out IntPtr ptr) ? IntPtr.Zero : ptr;
        }

        /// <inheritdoc />
        public override void DisplayMessageBox(string message)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    User32Methods.MessageBox(IntPtr.Zero, message, "Something went wrong!", (uint) 0x00000010L);
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    UnixNative.ExecuteBashCommand($"osascript -e 'tell app \"System Events\" to display dialog \"{message}\" buttons {{\"OK\"}} with icon caution'");
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    try
                    {
                        // Display a message box using Zenity.
                        UnixNative.ExecuteBashCommand($"zenity --error --text=\"{message}\" --title=\"Something went wrong!\" 2>/dev/null");
                    }
                    catch (Exception)
                    {
                        // Fallback to xmessage.
                        UnixNative.ExecuteBashCommand($"xmessage \"{message}\"");
                    }
            }
            catch (Exception e)
            {
                Engine.Log.Error($"Couldn't display error message box - {message}. {e}", MessageSource.Platform);
            }
        }
    }
}