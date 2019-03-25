#region Using

using System;
using System.Numerics;
using Adfectus.Common.Configuration;
using Adfectus.Graphics;
using Adfectus.Input;

#endregion

namespace Adfectus.Common.Hosting
{
    /// <summary>
    /// Provides the GraphicsManager, InputManager, and window.
    /// </summary>
    public interface IHost : IDisposable
    {
        /// <summary>
        /// Whether the host is focused.
        /// </summary>
        bool Focused { get; }

        /// <summary>
        /// The size of the host.
        /// </summary>
        Vector2 Size { get; set; }

        /// <summary>
        /// The host's window mode.
        /// </summary>
        WindowMode WindowMode { get; set; }

        /// <summary>
        /// Whether the host is open.
        /// </summary>
        bool Open { get; }

        /// <summary>
        /// The graphics manager of the host.
        /// </summary>
        GraphicsManager GraphicsManager { get; }

        /// <summary>
        /// The input manager of the host.
        /// </summary>
        IInputManager InputManager { get; }

        /// <summary>
        /// Updates the host and its events.
        /// </summary>
        void Update();

        /// <summary>
        /// Swap between the back buffer and the front buffer.
        /// </summary>
        void SwapBuffers();

        /// <summary>
        /// Get the size of the screen.
        /// </summary>
        Vector2 GetScreenSize();
    }
}