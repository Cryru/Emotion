#region Using

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Threading;
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
        // ***************************************************************************************************************
        // All paths the AssetLoader works with are "engine paths"
        // They conform to the following rules.
        // - The only legal directory separator character is "/"
        // - Relative paths (../) are legal but are not supported by most functions. Use GetNonRelativePath to convert.
        // - Files without extensions are illegal. They are considered directories.
        // ***************************************************************************************************************

        /// <summary>
        /// List of all assets in all sources.
        /// </summary>
        public string[] AllAssets
        {
            get => _manifest.Keys.ToArray();
        }

        /// <summary>
        /// Currently loaded assets.
        /// </summary>
        protected ConcurrentDictionary<string, Asset> _loadedAssets = new ConcurrentDictionary<string, Asset>();

        /// <summary>
        /// A list of asset paths and which asset source can serve them.
        /// </summary>
        protected ConcurrentDictionary<string, AssetSource> _manifest = new ConcurrentDictionary<string, AssetSource>();

        /// <summary>
        /// A list of asset paths and which asset store handles them.
        /// </summary>
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
            IAssetStore store = GetStore(name);
            if (store == null)
            {
                // If root path and in debug mode, save to the project assets.
                if (!Engine.Configuration.DebugMode || !_storage.TryGetValue("../../../assets", out store))
                {
                    store = _storage.First().Value;
                    Engine.Log.Warning($"Tried to store asset {name} but there's no store to service its folder. Saving to default store {store.Folder}.", MessageSource.AssetLoader);
                    if(store == null) return false;
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
        /// Get the asset store an asset with this filename would be stored in.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        /// <returns>The store which this asset would end up in, or null if none.</returns>
        public IAssetStore GetStore(string name)
        {
            string folder = GetDirectoryName(name);
            bool found = _storage.TryGetValue(folder, out IAssetStore store);
            return found ? store : null;
        }

        /// <summary>
        /// Get a loaded asset by its name or load it asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of asset.</typeparam>
        /// <param name="name">The name of the asset within any loaded source.</param>
        /// <returns>The loaded or cached asset.</returns>
        public Task<T> GetAsync<T>(string name) where T : Asset, new()
        {
            return Task.Run(() =>
            {
                Thread.CurrentThread.Name ??= $"AssetLoading Thread {name}";
                return Get<T>(name);
            });
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

        /// <summary>
        /// Get the name of a directory in an asset path.
        /// </summary>
        public static string GetDirectoryName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            if (name[^1] == '/') return name;

            int lastSlash = name.LastIndexOf("/", StringComparison.Ordinal);
            return lastSlash == -1 ? name : name.Substring(0, lastSlash);
        }

        /// <summary>
        /// Get an asset path relative to another path.
        /// </summary>
        public static string GetRelativePath(string relativeTo, string path)
        {
            int lastBack = path.LastIndexOf("../", StringComparison.Ordinal);
            if (lastBack == -1) return path;

            path = path.Substring(0, lastBack);
            string directory = GetDirectoryName(relativeTo);
            return directory + "/" + path;
        }

        /// <summary>
        /// Get the non-relative path from a path relative to another.
        /// [Folder/OneFile.ext] + [../File.ext] = File.ext
        /// [Folder/OneFile.ext] + [File.ext] = Folder/File.ext
        /// </summary>
        public static string GetNonRelativePath(string relativeTo, string path)
        {
            int lastBack = path.LastIndexOf("../", StringComparison.Ordinal);
            if (lastBack == -1)
            {
                string folderName = GetDirectoryName(relativeTo);
                return JoinPath(folderName, path);
            }

            string[] folders = relativeTo.Split("/");
            var relativeIdx = 0;
            var times = 0;
            while (true)
            {
                int nextRelative = path.IndexOf("../", relativeIdx, StringComparison.Ordinal);
                if (nextRelative == -1) break;
                relativeIdx = nextRelative + "../".Length;
                times++;
            }

            if (times > folders.Length) return path;
            var construct = "";
            for (var i = 0; i < folders.Length - times; i++)
            {
                construct = JoinPath(construct, folders[i]);
            }

            construct = JoinPath(construct, path.Substring(relativeIdx));
            return construct;
        }

        /// <summary>
        /// Join to asset paths.
        /// </summary>
        public static string JoinPath(string left, string right)
        {
            if (string.IsNullOrEmpty(left)) return right;
            if (string.IsNullOrEmpty(right)) return left;
            return left + "/" + right;
        }
    }
}