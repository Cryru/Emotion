#region Using

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Emotion.Common;
using Emotion.Primitives;
using Emotion.Standard.Logging;
using WinApi.User32;

#endregion

namespace Emotion.Platform.Implementation.CommonDesktop
{
    public abstract class DesktopPlatform : PlatformBase
    {
        public override void Setup(Configurator config)
        {
            base.Setup(config);

            if (Engine.AssetLoader == null) return;
            Engine.AssetLoader.AddSource(new FileAssetSource("Assets"));
            Engine.AssetLoader.AddStore(new FileAssetStore("Player"));
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

        /// <inheritdoc />
        public override IntPtr LoadLibrary(string path)
        {
            return NativeLibrary.Load(path);
        }

        /// <inheritdoc />
        public override IntPtr GetLibrarySymbolPtr(IntPtr library, string symbolName)
        {
            return NativeLibrary.GetExport(library, symbolName);
        }

        /// <inheritdoc />
        public override void DisplayMessageBox(string message)
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    User32Methods.MessageBox(IntPtr.Zero, message, "Something went wrong!", (uint) (0x00000000L | 0x00000010L));
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