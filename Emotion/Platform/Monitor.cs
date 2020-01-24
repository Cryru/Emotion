#region Using

using System.Numerics;

#endregion

namespace Emotion.Platform
{
    public abstract class Monitor
    {
        /// <summary>
        /// The position of the monitor.
        /// </summary>
        public Vector2 Position { get; protected set; }

        /// <summary>
        /// The size of the monitor in physical millimeters.
        /// </summary>
        public int WidthPhysical { get; protected set; }

        public int HeightPhysical { get; protected set; }

        /// <summary>
        /// The resolution of the monitor.
        /// </summary>
        public int Width { get; protected set; }

        public int Height { get; protected set; }
        public string Name { get; protected set; }
        public string DeviceName { get; protected set; }
        public string AdapterName { get; protected set; }
    }
}