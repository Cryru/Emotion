using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Adfectus.Common;

namespace Adfectus.IO
{
    /// <summary>
    /// Module used to load assets from various sources.
    /// Implemented by Platform.
    /// </summary>
    public abstract class AssetLoader
    {
        /// <summary>
        /// List of all assets in all sources.
        /// </summary>
        public string[] AllAssets
        {
            get => _manifest.Keys.ToArray();
        }

        /// <summary>
        /// List of all loaded assets from all sources.
        /// </summary>
        public Asset[] LoadedAssets
        {
            get => _loadedAssets.Values.ToArray();
        }

        protected ConcurrentDictionary<string, Asset> _loadedAssets = new ConcurrentDictionary<string, Asset>();
        protected ConcurrentDictionary<string, AssetSource> _manifest = new ConcurrentDictionary<string, AssetSource>();
        protected Dictionary<Type, Func<Asset>> _customLoaders = new Dictionary<Type, Func<Asset>>();

        #region Sources

        /// <summary>
        /// Add a source to the asset loader.
        /// Conflicting asset names are overwritten by whichever was added last.
        /// </summary>
        /// <param name="source">A new source to load assets from.</param>
        public void AddSource(AssetSource source)
        {
            string[] sourceManifest = source.GetManifest();
            foreach (string asset in sourceManifest)
            {
                _manifest.AddOrUpdate(NameToEngineName(asset), source, (_,__) => source);
            }
        }

        #endregion

        #region Assets

        /// <summary>
        /// Whether an asset with the provided name exists in any source.
        /// </summary>
        /// <param name="name">The name of the asset to check.</param>
        /// <returns>Whether an asset with the provided name exists in any source.</returns>
        public bool AssetExists(string name)
        {
            return _manifest.ContainsKey(name);
        }

        /// <summary>
        /// Whether an asset with the provided name is loaded.
        /// </summary>
        /// <param name="name">The name of the asset to check.</param>
        /// <returns>Whether an asset with a provided name is loaded.</returns>
        public bool AssetLoaded(string name)
        {
            return _loadedAssets.ContainsKey(name);
        }

        /// <summary>
        /// Get a loaded asset by its name or load it.
        /// </summary>
        /// <typeparam name="T">The type of asset.</typeparam>
        /// <param name="name">The name of the asset within any loaded source.</param>
        /// <returns>The loaded or cached asset.</returns>
        public T Get<T>(string name) where T : Asset, new()
        {
            if (string.IsNullOrEmpty(name)) return default;

            // Convert to engine name.
            name = NameToEngineName(name);

            // Check if cached.
            bool cached = _loadedAssets.TryGetValue(name, out Asset asset);
            // If cached and not disposed - return it.
            if (cached && !asset.Disposed)
            {
                return (T) asset;
            }

            // Check if the asset exists in any of the sources.
            bool assetFound = _manifest.TryGetValue(name, out AssetSource source);
            if (!assetFound)
            {
                Engine.Log.Error($"Tried to load asset {name} which doesn't exist in any loaded source.", Logging.MessageSource.AssetLoader);
                return default;
            }

            // Load the from the source.
            byte[] data = source.GetAsset(name);

            // Load and cache the asset.
            asset = Load<T>(data);
            asset.Name = name;
            _loadedAssets.AddOrUpdate(name, asset, (_, ___) => asset);

            return (T) asset;
        }

        /// <summary>
        /// Destroy an asset, freeing memory.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        public void Destroy(string name)
        {
            if (string.IsNullOrEmpty(name)) return;

            // Convert to engine name.
            name = NameToEngineName(name);

            // Check if the asset is already loaded, if not do nothing. Also remove it from the list.
            bool loaded = _loadedAssets.TryRemove(name, out Asset asset);
            if (!loaded) return;

            // Dispose of asset.
            asset.Dispose();
        }

        /// <summary>
        /// Load an asset from bytes.
        /// </summary>
        /// <typeparam name="T">The type of asset to load.</typeparam>
        /// <param name="data">A byte array to load the asset from.</param>
        /// <returns>A loaded asset from the byte array.</returns>
        public T Load<T>(byte[] data) where T : Asset, new()
        {
            // Check if a custom loader exists for this type.
            bool customLoader = _customLoaders.TryGetValue(typeof(T), out Func<Asset> loaderFunc);
            T asset = customLoader ? (T) loaderFunc() : new T();
            asset.Create(data);

            return asset;
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Converts the provided asset name to an engine name
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <returns>The converted name.</returns>
        public static string NameToEngineName(string name)
        {
            return name.Replace("//", "/").Replace('/', '$').Replace('\\', '$').Replace('$', '/');
        }

        #endregion
    }
}
