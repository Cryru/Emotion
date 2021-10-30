#region Using

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Emotion.Common;
using Emotion.Standard.Logging;
using Emotion.Utility;

#endregion

#nullable enable

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
        protected ConcurrentDictionary<string, Asset> _loadedAssets = new();

        /// <summary>
        /// A list of asset paths and which asset source can serve them.
        /// </summary>
        protected ConcurrentDictionary<string, AssetSource> _manifest = new();

        /// <summary>
        /// A list of asset paths and which asset store handles them.
        /// </summary>
        protected ConcurrentDictionary<string, IAssetStore> _storage = new();

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
        /// <param name="cache">If set to false the asset will neither be fetched from cache, neither cached.</param>
        /// <returns>The loaded or cached asset.</returns>
        public T? Get<T>(string name, bool cache = true) where T : Asset, new()
        {
            if (string.IsNullOrEmpty(name)) return default;

            // Convert to engine name.
            name = NameToEngineName(name);

            // Check if loaded.
            Asset? asset;
            if (cache)
                // If loaded and not disposed - return it.
                if (_loadedAssets.TryGetValue(name, out asset) && !asset!.Disposed)
                {
                    Debug.Assert(asset is T, "Asset was requested twice as different types.");
                    return (T) asset;
                }

            // Get the source which contains it, if any.
            AssetSource? source = GetSource(name);

            // Check if the asset was found in any source.
            if (source == null)
            {
                Engine.Log.Warning($"Tried to load asset {name} which doesn't exist in any loaded source.", MessageSource.AssetLoader, true);
                return default;
            }

            PerfProfiler.ProfilerEventStart($"Loading {name}", "Loading");

            // Load it from the source.
            ReadOnlyMemory<byte> data = source.GetAsset(name);
            if (data.IsEmpty) return default;

            // Load the asset.
            asset = new T {Name = name};
            asset.Create(data);
            if (cache) _loadedAssets.AddOrUpdate(name, asset, (_, ___) => asset);

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
            IAssetStore? store = GetStore(name);
            if (store == null)
                // If root path and in debug mode, save to the project assets.
                if (!Engine.Configuration.DebugMode || !_storage.TryGetValue(NameToEngineName(DebugAssetStore.AssetDevPath), out store))
                {
                    if (_storage.Count == 0)
                    {
                        Engine.Log.Warning($"Couldn't find asset store for {name} and debug store isn't loaded.", MessageSource.AssetLoader);
                        return false;
                    }

                    store = _storage.FirstOrDefault().Value;
                    Engine.Log.Warning($"Tried to store asset {name} but there's no store that services that folder. Saving to debug store \"{store.Folder}\".", MessageSource.AssetLoader);
                }

            // Store the asset.
            try
            {
                store.SaveAsset(asset, name, backup);
                Engine.Log.Info($"Saved asset {name} via {store}", MessageSource.AssetLoader);

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
        public AssetSource? GetSource(string name)
        {
            bool found = _manifest.TryGetValue(name, out AssetSource? source);
            return found ? source : null;
        }

        /// <summary>
        /// Get the asset store an asset with this filename would be stored in.
        /// </summary>
        /// <param name="name">The name of the asset.</param>
        /// <returns>The store which this asset would end up in, or null if none.</returns>
        public IAssetStore? GetStore(string name)
        {
            string folder = GetFirstDirectoryName(name);
            bool found = _storage.TryGetValue(folder, out IAssetStore? store);
            return found ? store : null;
        }

        private Dictionary<string, Task> _asyncLoadingTasks = new();

        /// <summary>
        /// Get a loaded asset by its name or load it asynchronously.
        /// </summary>
        /// <typeparam name="T">The type of asset.</typeparam>
        /// <param name="name">The name of the asset within any loaded source.</param>
        /// <returns>The loaded or cached asset.</returns>
        public Task<T?> GetAsync<T>(string? name) where T : Asset, new()
        {
            if (name == null) return Task.FromResult((T?) null);

            Task? task;
            lock (_asyncLoadingTasks)
            {
                if (_asyncLoadingTasks.TryGetValue(name, out task))
                    return (Task<T?>) task;

                task = Task.Run(() => Get<T>(name));
                _asyncLoadingTasks.Add(name, task);
            }

            return (Task<T?>) task;
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
            bool loaded = _loadedAssets.TryRemove(name, out Asset? asset);
            if (!loaded) return;

            // Remove async loading shortcut for this asset, as it will contain the destroyed asset.
            _asyncLoadingTasks.Remove(name);

            // Dispose of asset.
            asset!.Dispose();
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
        /// Converts a path to the platform equivalent on the currently running platform.
        /// </summary>
        /// <param name="path">The path to convert.</param>
        /// <returns>A cross platform path.</returns>
        public static string CrossPlatformPath(string path)
        {
            return path.Replace('/', '$').Replace('\\', '$').Replace('$', Path.DirectorySeparatorChar);
        }

        /// <summary>
        /// Converts the string to one which is safe for use in the file system.
        /// </summary>
        /// <param name="str">The string to convert.</param>
        /// <returns>A string safe to use in the file system.</returns>
        public static string MakeStringPathSafe(string str)
        {
            return Path.GetInvalidPathChars().Aggregate(str, (current, c) => current.Replace(c, ' '));
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
        /// Get the path to a file without the extension.
        /// </summary>
        /// <param name="assetName">The path to the file.</param>
        public static string GetFilePathNoExtension(string assetName)
        {
            int dotIdx = assetName.IndexOf('.');
            if (dotIdx == -1) return assetName;
            return assetName[..dotIdx];
        }

        /// <summary>
        /// Get the name of the first directory in the asset path.
        /// </summary>
        public static string GetFirstDirectoryName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;
            int lastSlash = name.IndexOf("/", StringComparison.Ordinal);
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
            if (left[^1] == '/') left = left[..^1];
            if (right[0] == '/') right = right[1..];
            return left + "/" + right;
        }

        /// <summary>
        /// Create the default asset loader which loads the engine assembly, the game assembly, the setup calling assembly, and the
        /// file system.
        /// Duplicate assemblies are not loaded.
        /// </summary>
        /// <returns>The default asset loader.</returns>
        public static AssetLoader CreateDefaultAssetLoader()
        {
            var loader = new AssetLoader();

            // Create sources.
            for (var i = 0; i < Helpers.AssociatedAssemblies.Length; i++)
            {
                Assembly assembly = Helpers.AssociatedAssemblies[i];
                loader.AddSource(new EmbeddedAssetSource(assembly, "Assets"));
            }

            return loader;
        }
    }
}