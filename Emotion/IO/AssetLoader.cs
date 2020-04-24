#region Using

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Standard.Logging;

#endregion

namespace Emotion.IO
{
    /// <summary>
    /// Module used to load assets from various sources.
    /// Implemented by Platform.
    /// </summary>
    public class AssetLoader
    {
        /// <summary>
        /// List of all assets in all sources.
        /// </summary>
        public string[] AllAssets
        {
            get => _manifest.Keys.ToArray();
        }

        protected ConcurrentDictionary<string, Asset> _loadedAssets = new ConcurrentDictionary<string, Asset>();
        protected ConcurrentDictionary<string, AssetSource> _manifest = new ConcurrentDictionary<string, AssetSource>();
        protected ConcurrentDictionary<string, IAssetStore> _storage = new ConcurrentDictionary<string, IAssetStore>();

        /// <summary>
        /// List of all loaded assets from all sources.
        /// </summary>
        public Asset[] LoadedAssets
        {
            get => _loadedAssets.Values.ToArray();
        }

        /// <summary>
        /// Create an asset loader from a set of sources.
        /// </summary>
        /// <param name="sources"></param>
        public AssetLoader(AssetSource[] sources) : this()
        {
            foreach (AssetSource source in sources)
            {
                AddSource(source);
            }
        }

        /// <summary>
        /// Create an asset loader.
        /// </summary>
        public AssetLoader()
        {
        }

        /// <summary>
        /// Add a source to the asset loader.
        /// Conflicting asset names are overwritten by whichever was added first.
        /// </summary>
        /// <param name="source">A new source to load assets from.</param>
        public void AddSource(AssetSource source)
        {
            string[] sourceManifest = source.GetManifest();
            foreach (string asset in sourceManifest)
            {
                _manifest.TryAdd(NameToEngineName(asset), source);

                // If duplicated names should overwrite assets in the sources then replace with this line:
                // Note: Old versions of Emotion didn't overwrite assets.
                //_manifest.AddOrUpdate(NameToEngineName(asset), source, (_,__) => source);
            }

            Engine.Log.Info($"Mounted asset source '{source}' containing {sourceManifest.Length} assets.", MessageSource.AssetLoader);
        }

        /// <summary>
        /// Add a store to the asset loader.
        /// Conflicting asset names are overwritten by whichever was added first.
        /// </summary>
        /// <param name="store">A new store to load and save assets from.</param>
        public void AddStore(AssetSource store)
        {
            if (!(store is IAssetStore storeCast))
            {
                Engine.Log.Warning($"Tried to mount an asset store which isn't a store - {store}.", MessageSource.AssetLoader);
                return;
            }

            // Stores are also sources.
            AddSource(store);

            _storage.TryAdd(NameToEngineName(storeCast.Folder), storeCast);
            Engine.Log.Info($"Mounted asset store '{store}' at {storeCast.Folder}.", MessageSource.AssetLoader);
        }

        #region Assets

        /// <summary>
        /// Whether an asset with the provided name exists in any source.
        /// </summary>
        /// <param name="name">The name of the asset to check.</param>
        /// <returns>Whether an asset with the provided name exists in any source.</returns>
        public bool Exists(string name)
        {
            return _manifest.ContainsKey(NameToEngineName(name));
        }

        /// <summary>
        /// Whether an asset with the provided name is loaded.
        /// </summary>
        /// <param name="name">The name of the asset to check.</param>
        /// <returns>Whether an asset with a provided name is loaded.</returns>
        public bool Loaded(string name)
        {
            return _loadedAssets.ContainsKey(NameToEngineName(name));
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

            // Check if loaded.
            bool loaded = _loadedAssets.TryGetValue(name, out Asset asset);
            // If loaded and not disposed - return it.
            if (loaded && !asset.Disposed)
            {
                Debug.Assert(asset is T, "Asset was requested twice as different types.");
                return (T) asset;
            }

            // Get the source which contains it, if any.
            AssetSource source = GetSource(name);

            // Check if the asset was found in any source.
            if (source == null)
            {
                Engine.Log.Warning($"Tried to load asset {name} which doesn't exist in any loaded source.", MessageSource.AssetLoader);
                return default;
            }

            PerfProfiler.ProfilerEventStart($"Loading {name}", "Loading");
            PerfProfiler.ProfilerEventStart($"SourceLoading {name}", "Loading");

            // Load it from the source.
            byte[] data = source.GetAsset(name);

            PerfProfiler.ProfilerEventEnd($"SourceLoading {name}", "Loading");
            PerfProfiler.ProfilerEventStart($"InternalLoading {name}", "Loading");

            // Load the asset.
            asset = new T {Name = name};
            asset.Create(data);
            _loadedAssets.AddOrUpdate(name, asset, (_, ___) => asset);

            PerfProfiler.ProfilerEventEnd($"InternalLoading {name}", "Loading");
            PerfProfiler.ProfilerEventEnd($"Loading {name}", "Loading");

            return (T) asset;
        }

        /// <summary>
        /// Store an asset.
        /// </summary>
        /// <param name="asset">The asset data to store.</param>
        /// <param name="name">The engine name to store it under.</param>
        /// <param name="backup">Whether to backup the old file if any.</param>
        /// <returns>Whether the file was saved.</returns>
        public bool Save(byte[] asset, string name, bool backup = true)
        {
            if (string.IsNullOrEmpty(name)) return false;

            // Convert to engine name.
            name = NameToEngineName(name);

            // Find a store which matches the name folder.
            IAssetStore store;
            int folderIndex = name.IndexOf('/');
            if (folderIndex == -1)
            {
                store = _storage.First().Value;
                name = $"{store.Folder}/{name}";
            }
            else
            {
                string folder = name.Substring(0, folderIndex);
                bool found = _storage.TryGetValue(folder, out store);
                if (!found)
                {
                    Engine.Log.Warning($"Tried to store asset {name} but there's no store to service folder {folder}.", MessageSource.AssetLoader);
                    return false;
                }
            }

            // Store the asset.
            try
            {
                store.SaveAsset(asset, name, backup);

                // If it didn't exist until now - add it to the internal manifest.
                if (!Exists(name)) _manifest.TryAdd(name, (AssetSource) store);
                return true;
            }
            catch (Exception ex)
            {
                Engine.Log.Error(new Exception($"Couldn't store asset - {name}", ex));
                return false;
            }
        }

        /// <summary>
        /// Get the source which contains the specified asset.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        /// <returns>The source which can load the asset, or null if none.</returns>
        public AssetSource GetSource(string name)
        {
            bool assetFound = _manifest.TryGetValue(name, out AssetSource source);
            return assetFound ? source : null;
        }

        /// <summary>
        /// Get a loaded asset by its name or load it asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of asset.</typeparam>
        /// <param name="name">The name of the asset within any loaded source.</param>
        /// <returns>The loaded or cached asset.</returns>
        public Task<T> GetAsync<T>(string name) where T : Asset, new()
        {
            return Task.Run(() => Get<T>(name));
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

        #endregion

        /// <summary>
        /// Converts the provided asset name to an engine name
        /// </summary>
        /// <param name="name">The name to convert.</param>
        /// <returns>The converted name.</returns>
        public static string NameToEngineName(string name)
        {
            return name.Replace("//", "/").Replace('/', '$').Replace('\\', '$').Replace('$', '/').ToLower();
        }
    }
}