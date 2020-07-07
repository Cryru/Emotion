#region Using

using System.Collections.Generic;
using Emotion.Common;
using Emotion.Primitives;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.Platform.Implementation.CommonDesktop
{
    public abstract class DesktopPlatform : PlatformBase
    {
        internal override void Setup(Configurator config)
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
    }
}