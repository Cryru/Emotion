using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Soul.Engine.Enums
{
    /// <summary>
    /// The matrix to draw on.
    /// </summary>
    public enum DrawLocation
    {
        /// <summary>
        /// Not currently drawing.
        /// </summary>
        None,
        /// <summary>
        /// Can be used to draw outside out of the screen space, like the black bars.
        /// </summary>
        Terminus,
        /// <summary>
        /// UI and other stuff goes on this channel.
        /// </summary>
        Screen,
        /// <summary>
        /// Game objects and such are here, this space is viewed through a camera.
        /// </summary>
        World
    }
}
