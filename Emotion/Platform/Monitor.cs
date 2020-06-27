#region Using

using System.Numerics;

#endregion

namespace Emotion.Platform
{
    /// <summary>
    /// Represents the monitor or screen.
    /// </summary>
    public abstract class Monitor
    {
        /// <summary>
        /// The position of the monitor relative to other monitors, in window space.
        /// </summary>
        public Vector2 Position { get; protected set; }

        /// <summary>
        /// The size of the monitor in physical millimeters. Optional.
        /// </summary>
        public int WidthPhysical { get; protected set; }

        /// <summary>
        /// The size of the monitor in physical millimeters. Optional.
        /// </summary>
        public int HeightPhysical { get; protected set; }

        /// <summary>
        /// The X resolution of the monitor.
        /// </summary>
        public int Width { get; protected set; }

        /// <summary>
        /// The Y resolution of the monitor.
        /// </summary>
        public int Height { get; protected set; }

        /// <summary>
        /// The internal name of the monitor.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The name of the graphical device displaying on this monitor.
        /// </summary>
        public string DeviceName { get; protected set; }

        /// <summary>
        /// The name of the graphics adapter for this monitor.
        /// </summary>
        public string AdapterName { get; protected set; }
    }
}