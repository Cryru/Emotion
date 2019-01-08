// Emotion - https://github.com/Cryru/Emotion

#region Using

using System;
using System.Collections.Concurrent;
using System.Linq;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// A source of assets.
    /// </summary>
    public abstract class AssetSource
    {
        /// <summary>
        /// The internal manifest.
        /// </summary>
        public ConcurrentDictionary<string, string> InternalManifest { get; private set; }

        /// <summary>
        /// Create a new asset source.
        /// </summary>
        protected AssetSource()
        {
            InternalManifest = new ConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Returns an asset by engine path.
        /// </summary>
        /// <param name="enginePath">The engine path to the asset.</param>
        /// <returns>A byte array of the asset.</returns>
        public abstract byte[] GetAsset(string enginePath);

        /// <summary>
        /// Returns all assets this manifest manages.
        /// </summary>
        /// <returns>A list of engine paths to assets this source can provide.</returns>
        public string[] GetManifest()
        {
            // Return manifest.
            return InternalManifest.Keys.ToArray();
        }
    }
}