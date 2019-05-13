#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Adfectus.Common;
using Adfectus.Logging;

#endregion

namespace Adfectus.IO
{
    /// <summary>
    /// Manages loading and unloading of assets.
    /// </summary>
    public class AssetLoader
    {
        #region Properties

        /// <summary>
        /// An array of the currently loaded assets.
        /// </summary>
        public Asset[] LoadedAssets
        {
            get => _loadedAssets.Values.ToArray();
        }

        /// <summary>
        /// An array of all assets in the manifest.
        /// </summary>
        public string[] AllAssets
        {
            get => _assetManifest.Keys.ToArray();
        }

        #endregion

        #region Objects

        /// <summary>
        /// Loaded assets.
        /// </summary>
        private ConcurrentDictionary<string, Asset> _loadedAssets;

        /// <summary>
        /// Manifest of all assets.
        /// </summary>
        private ConcurrentDictionary<string, AssetSource> _assetManifest;

        #endregion

        /// <summary>
        /// Create a new asset loader.
        /// </summary>
        /// <param name="sources">The asset sources for the loader.</param>
        public AssetLoader(IEnumerable<AssetSource> sources)
        {
            // Populate manifest.
            _assetManifest = new ConcurrentDictionary<string, AssetSource>(StringComparer.OrdinalIgnoreCase);
            foreach (AssetSource source in sources)
            {
                AddSource(source);
            }

            _loadedAssets = new ConcurrentDictionary<string, Asset>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Add a source to the asset loader.
        /// </summary>
        /// <param name="source">The source to add.</param>
        public void AddSource(AssetSource source)
        {
            // Get the manifest of the source.
            string[] manifest = source.GetManifest();

            // Add all files from it to the total manifest, and link it to this source.
            manifest.AsParallel().ForAll(x => _assetManifest.TryAdd(x, source));

            Engine.Log.Trace($"Added source {source} to asset loader, with a manifest size of {manifest.Length}.", MessageSource.AssetLoader);
        }

        /// <summary>
        /// Returns an asset, if not loaded loads it.
        /// </summary>
        /// <typeparam name="T">The type of asset.</typeparam>
        /// <param name="path">A file path to the asset with the RootDirectory as the parent. Will be converted to an engine path.</param>
        /// <returns>A loaded asset.</returns>
        [SuppressMessage("ReSharper", "ConvertIfStatementToReturnStatement")]
        public T Get<T>(string path) where T : Asset
        {
            if (string.IsNullOrEmpty(path)) return null;

            // Convert the path to an engine path.
            string enginePath = PathToEnginePath(path);

            // Check if the asset is already loaded, in which case return it.
            bool loaded = _loadedAssets.TryGetValue(enginePath, out Asset loadedAsset);
            if (loaded) return (T) loadedAsset;

            // Check if the asset exists in the manifest.
            bool contains = _assetManifest.TryGetValue(enginePath, out AssetSource source);
            if (!contains)
            {
                Engine.Log.Error($"Asset [{enginePath}] not found in manifest.", MessageSource.AssetLoader);
                return null;
            }

            // Get the asset.
            byte[] fileContents = source.GetAsset(enginePath);

            // Create an instance of the asset and add it.
            DebugMessageWrap("Creating", enginePath, typeof(T), MessageType.Trace);
            T temp = (T) Activator.CreateInstance(typeof(T));
            temp.Name = enginePath;
            temp.CreateAsset(fileContents);
            DebugMessageWrap("Created", enginePath, typeof(T), MessageType.Info);

            // Add it to the loaded assets.
            _loadedAssets.TryAdd(enginePath, temp);

            // Return.
            return temp;
        }

        /// <summary>
        /// Destroy an asset, freeing memory.
        /// </summary>
        /// <param name="path">A path to the asset. Will be converted to an engine path.</param>
        public void Destroy(string path)
        {
            if (string.IsNullOrEmpty(path)) return;

            // Convert the path to an engine path.
            string enginePath = PathToEnginePath(path);

            // Check if the asset is already loaded, if not do nothing. Also remove it from the list.
            bool loaded = _loadedAssets.TryRemove(enginePath, out Asset asset);
            if (!loaded) return;

            // Dispose of asset.
            DebugMessageWrap("Destroying", enginePath, asset.GetType(), MessageType.Info);
            asset.DestroyAsset();
            DebugMessageWrap("Destroyed", enginePath, null, MessageType.Trace);
        }

        #region Helpers

        /// <summary>
        /// Returns whether the specified asset exists.
        /// </summary>
        /// <param name="path">The path to the asset.</param>
        /// <returns>True if it exists, false otherwise.</returns>
        public bool Exists(string path)
        {
            return _assetManifest.ContainsKey(PathToEnginePath(path));
        }

        /// <summary>
        /// Converts the provided path to an engine universal format.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>The converted path.</returns>
        public static string PathToEnginePath(string path)
        {
            return path.Replace("//", "/").Replace('/', '$').Replace('\\', '$').Replace('$', '/');
        }

        #endregion

        #region Debugging

        /// <summary>
        /// Post a formatted asset loader debug message.
        /// </summary>
        /// <param name="operation">The operation being performed.</param>
        /// <param name="path">An engine path to the file.</param>
        /// <param name="type">The type of asset.</param>
        /// <param name="messageType">The type of message to log.</param>
        private void DebugMessageWrap(string operation, string path, Type type, MessageType messageType)
        {
            Engine.Log.Log(messageType, MessageSource.AssetLoader, $"{operation} asset [{path}]{(type != null ? " of type " + type : "")}");
        }

        #endregion
    }
}