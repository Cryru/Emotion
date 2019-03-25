#region Using

using System;

#endregion

namespace Adfectus.Common
{
    /// <summary>
    /// An Adfectus plugin.
    /// </summary>
    public abstract class Plugin : IDisposable
    {
        /// <summary>
        /// Initialize the plugin. Is run after all modules are setup, and before the loop starts.
        /// </summary>
        public abstract void Initialize();

        /// <summary>
        /// Updates the plugin. Is run at the end of the update loop.
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Perform plugin cleanup.
        /// </summary>
        public abstract void Dispose();
    }
}