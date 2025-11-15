#nullable enable

#region Using

using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Emotion.Core.Systems.Logging;
using Emotion.Core.Utility.Coroutines;
using Emotion.Core.Utility.Profiling;
using Emotion.Primitives.DataStructures;

#endregion

namespace Emotion.Core.Systems.IO;

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

    private Dictionary<string, string>? _assetRemap = null;

    private List<AssetSource> _assetSources = new(4);

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
    public void AddSource(AssetSource source, bool logAdding = true)
    {
        string[] sourceManifest = source.GetManifest();
        foreach (string asset in sourceManifest)
        {
            _manifest.TryAdd(NameToEngineName(asset), source);

            // If duplicated names should overwrite assets in the sources then replace with this line:
            // Note: Old versions of Emotion didn't overwrite assets.
            //_manifest.AddOrUpdate(NameToEngineName(asset), source, (_,__) => source);
        }

        if (logAdding)
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

        _storage.TryAdd(NameToEngineName(storeCast.StoreFolder), storeCast);
        Engine.Log.Info($"Mounted asset store '{store}' at {storeCast.StoreFolder}.", MessageSource.AssetLoader);
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
    /// Get a list of all assets in the specified folder.
    /// </summary>
    public string[] GetAssetsInFolder(string name)
    {
        name = NameToEngineName(name);

        var matches = new List<string>();
        foreach (string manifestKey in _manifest.Keys)
        {
            if (manifestKey.StartsWith(name)) matches.Add(manifestKey);
        }

        for (int i = 0; i < _assetSources.Count; i++)
        {
            AssetSource assSrc = _assetSources[i];
            bool hasIt = assSrc.HasAsset(name);
            var manifest = assSrc.GetManifest();
            for (int m = 0; m < manifest.Length; m++)
            {
                string manifestKey = manifest[m];
                if (manifestKey.StartsWith(name)) matches.Add(manifestKey);
            }
        }

        return matches.ToArray();
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
        {
#if !WEB
            // If loading async, wait for it instead of loading again.
            if (_asyncLoadingTasks.TryGetValue(name, out Task? task) && !task.IsCompleted && task.Id != Task.CurrentId) task.Wait();
#endif

            // If loaded and not disposed - return it.
            if (_loadedAssets.TryGetValue(name, out asset) && !asset.Disposed)
            {
                Assert(asset is T, "Asset was requested twice as different types.");
                return (T)asset;
            }
        }

        string nameGetAs = name;

        // Get the source which contains it, if any.
        AssetSource? source = GetSource(name);

        if (source == null && _assetRemap != null)
        {
            if (_assetRemap.TryGetValue(name, out string? remappedName))
            {
                nameGetAs = remappedName;
                source = GetSource(nameGetAs);
                Engine.Log.Info($"Remapped {name} -> {remappedName}", MessageSource.AssetLoader);
            }
        }

        // Check if the asset was found in any source.
        if (source == null)
        {
            Engine.Log.Warning($"Tried to load asset {name} which doesn't exist in any loaded source.", MessageSource.AssetLoader, true);
            return default;
        }

        PerfProfiler.ProfilerEventStart($"Loading {name}", "Loading");

        // Load it from the source.
        ReadOnlyMemory<byte> data = source.GetAsset(nameGetAs);
        if (data.IsEmpty) return default;

        // Load the asset.
        asset = new T { Name = name };
        asset.AssetLoader_CreateLegacy(data);
        if (cache) _loadedAssets.AddOrUpdate(name, asset, (_, ___) => asset);

        PerfProfiler.ProfilerEventEnd($"Loading {name}", "Loading");

        return (T)asset;
    }

    public bool SaveDevMode(string content, string name, bool backup = true)
    {
        if (!Engine.Configuration.DebugMode) return false;
        return Save(content, "Assets/" + name, backup);
    }

    public bool Save(string content, string name, bool backup = true)
    {
        byte[] bytes = System.Text.Encoding.Default.GetBytes(content);
        return Save(bytes, name, backup);
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
        {
            Engine.Log.Warning($"Couldn't find asset store for {name}.", MessageSource.AssetLoader);
            return false;
        }

        // Store the asset.
        try
        {
            store.SaveAsset(asset, name, backup);
            Engine.Log.Info($"Saved asset {name} via {store}", MessageSource.AssetLoader);

            // If it didn't exist until now - add it to the internal manifest.
            if (!Exists(name)) _manifest.TryAdd(name, (AssetSource)store);
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
        for (int i = 0; i < _assetSources.Count; i++)
        {
            AssetSource assSrc = _assetSources[i];
            bool hasIt = assSrc.HasAsset(name);
            if (hasIt)
                return assSrc;
        }

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
        //playe
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
    /// <param name="cache">Whether to cache this asset in memory for faster subsequent retrieval.</param>
    /// <returns>The loaded or cached asset.</returns>
    public Task<T?> GetAsync<T>(string? name, bool cache = true) where T : Asset, new()
    {
        if (name == null) return Task.FromResult((T?)null);
        name = NameToEngineName(name);

        Task? task;
        lock (_asyncLoadingTasks)
        {
            if (_asyncLoadingTasks.TryGetValue(name, out task)) // Check if already async loading this asset.
                return (Task<T?>)task;

            // Avoid lambda allocation
            task = Task.Factory.StartNew(
                static (state) =>
                {
                    var stateTuple = (ValueTuple<string, bool>?)state;
                    if (stateTuple == null || !stateTuple.HasValue) return null;
                    (string, bool) taskParam = stateTuple.Value;
                    return Engine.AssetLoader.Get<T>(taskParam.Item1, taskParam.Item2);
                },
                (name, cache)
            );
            _asyncLoadingTasks.Add(name, task);
        }

        return (Task<T?>)task;
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

    public void LateInit()
    {
        if (Exists("AssetRemap.xml"))
        {
            XMLAsset<Dictionary<string, string>>? assetRemapAsset = Get<XMLAsset<Dictionary<string, string>>>("AssetRemap.xml");
            if (assetRemapAsset != null)
                _assetRemap = assetRemapAsset.Content;
        }
    }

    #endregion

    private static ConcurrentDictionary<string, string> _cachedNameToEngineNameConversions = new(); // Reduce allocations

    /// <summary>
    /// Converts the provided asset name to an engine name
    /// </summary>
    /// <param name="name">The name to convert.</param>
    /// <returns>The converted name.</returns>
    public static string NameToEngineName(string name)
    {
        if (_cachedNameToEngineNameConversions.TryGetValue(name, out string? cachedConversion)) return cachedConversion;

        string engineName = name.Replace("//", "/").Replace('/', '$').Replace('\\', '$').Replace('$', '/').ToLower();
        _cachedNameToEngineNameConversions.TryAdd(name, engineName);
        return engineName;
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
        if (string.IsNullOrEmpty(name)) return "";
        if (name[^1] == '/') return "";

        int lastSlash = name.LastIndexOf("/", StringComparison.Ordinal);
        return lastSlash == -1 ? "" : name.Substring(0, lastSlash);
    }

    /// <summary>
    /// Get the name of the file without directories.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static string GetFileName(string name)
    {
        if (string.IsNullOrEmpty(name)) return "";

        int lastSlash = name.LastIndexOf("/", StringComparison.Ordinal);
        return lastSlash == -1 || lastSlash == name.Length - 1 ? name : name.Substring(lastSlash + 1);
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
    /// Remove the relative part of a relative path and return it relative to a directory.
    /// [Folder/OtherFile.ext] + [../../../File.ext] = Folder/File.ext
    /// [Folder/] + [../../../File.ext] = Folder/File.ext
    /// [Folder/OtherFile.ext] + [File.ext] = Folder/File.ext
    /// [] + [../File.ext] = File.ext
    /// </summary>
    public static string TrimRelativePath(string relativeTo, string path)
    {
        int lastBack = path.LastIndexOf("../", StringComparison.Ordinal);
        if (lastBack != -1) path = path[..lastBack];

        string directory = GetDirectoryName(relativeTo);
        return JoinPath(directory, path);
    }

    /// <summary>
    /// Get a non-relative path from a path relative to a specific directory.
    /// [Folder] + [../File.ext] = File.ext
    /// [Folder] + [File.ext] = Folder/File.ext
    /// [Folder] + [OtherFolder/File.ext] = Folder/OtherFolder/File.ext
    /// [Folder] + [./File.ext] = Folder/File.ext
    /// [Folder] + [OtherFolder/File.ext] + [joinNonRelative == false] = OtherFolder/File.ext
    /// [Folder] + [./File.ext] + [joinNonRelative == false] = File.ext
    /// </summary>
    public static string GetNonRelativePath(string relativeToDirectory, string path, bool joinNonRelative = true)
    {
        path = NameToEngineName(path);

        if (string.IsNullOrWhiteSpace(path)) return joinNonRelative ? relativeToDirectory : "";

        if (path.Length > 2 && path[0] == '.' && path[1] == '/') path = path[2..];

        int lastBack = path.LastIndexOf("../", StringComparison.Ordinal);
        if (lastBack == -1) return joinNonRelative ? JoinPath(relativeToDirectory, path) : path;

        string[] folders = relativeToDirectory.Split("/");
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

    public NTree<string, string> GetAssetFileTree()
    {
        NTree<string, string> tree = new NTree<string, string>();

        for (int i = 0; i < _assetSources.Count; i++)
        {
            AssetSource source = _assetSources[i];

            string[] assets = source.GetManifest();
            Array.Sort(assets);

            foreach (string assetPath in assets)
            {
                if (assetPath.Contains('/'))
                {
                    string[] folderPath = assetPath.Split('/')[..^1];
                    tree.Add(folderPath, assetPath);
                }
                else
                {
                    tree.Leaves.Add(assetPath);
                }
            }
        }

        return tree;
    }

    #region ONE

    public void ONE_AddAssetSource(AssetSource source)
    {
        _assetSources.Add(source);
        Engine.Log.Info($"Mounted asset source '{source}'", MessageSource.AssetLoader);
    }

    private ConcurrentDictionary<int, Asset> _createdAssets = new ConcurrentDictionary<int, Asset>();
    private ConcurrentQueue<Asset> _assetsToLoad = new ConcurrentQueue<Asset>();
    private ConcurrentQueue<string> _assetsToReload = new ConcurrentQueue<string>();
    private List<IRoutineWaiter> _loadingAssetRoutines = new(16);

    public T ONE_Get<T>(string? name, object? addRefenceToObject = null, bool loadInline = false, bool loadedAsDependency = false, bool noCache = false) where T : Asset, new()
    {
        if (string.IsNullOrEmpty(name))
            name = string.Empty;
        else
            name = NameToEngineNameRemapped(name);

        // If the asset already exists, get it.
        int assetCacheHash = NameToAssetCacheHash(name, typeof(T));
        if (!noCache && _createdAssets.TryGetValue(assetCacheHash, out Asset? loadedAsset))
        {
            if (addRefenceToObject != null)
                AddReferenceToAsset(loadedAsset, addRefenceToObject);

            return (T) loadedAsset;
        }

        // Create a new asset and register it.
        T newAsset = new T { Name = name, LoadedAsDependency = loadedAsDependency };
        if (!noCache)
            _createdAssets.TryAdd(assetCacheHash, newAsset);

        if (addRefenceToObject != null)
            AddReferenceToAsset(newAsset, addRefenceToObject);

        if (loadInline)
        {
            Coroutine.RunInline(newAsset.AssetLoader_LoadAsset());
        }
        else
        {
            // Queue the asset to be loaded
            _assetsToLoad.Enqueue(newAsset);
        }

        return newAsset;
    }

    public void AddReferenceToAsset(Asset? asset, object referencingObject)
    {
        if (asset == null) return;
        // todo
    }

    public void RemoveReferenceFromAsset(Asset? asset, object referencingObject, bool deleteIfNoReferences = true)
    {
        if (asset == null) return;
        // todo
    }

    /// <summary>
    /// Reloads any loaded assets with the provided name.
    /// This is called to hot reload assets by their managing sources.
    /// 
    /// The reload sequence will eventually converge into the same code as the load sequence,
    /// however it dedupes reload requests as usually managing sources spam reload requests (at least on windows).
    /// </summary>
    public void ONE_ReloadAsset(string name)
    {
        name = NameToEngineNameRemapped(name);
        _assetsToReload.Enqueue(name);
    }

    public void Update()
    {
        // Clear finished routines
        _loadingAssetRoutines.RemoveAll(x => x.Finished);

        // Add reloads
        while (_assetsToReload.TryDequeue(out string? assetNameToReload))
        {
            // Dedupe if next is same.
            if (_assetsToReload.TryPeek(out string? nextAssetToReload))
            {
                if (nextAssetToReload == assetNameToReload) continue;
            }

            // Reload loaded assets with the name.
            // Since this is editor/debug code we don't care that this would
            // be slow and copy the list (ConcurrentDictionary enum).
            foreach ((int _, Asset asset) in _createdAssets)
            {
                if (asset.Name == assetNameToReload)
                {
                    IRoutineWaiter loadRoutine = Engine.Jobs.Add(asset.AssetLoader_LoadAsset());
                    _loadingAssetRoutines.Add(loadRoutine);
                }
            }
        }

        // Add new assets
        while (_assetsToLoad.TryDequeue(out Asset? asset))
        {
            IRoutineWaiter loadRoutine = Engine.Jobs.Add(asset.AssetLoader_LoadAsset());
            _loadingAssetRoutines.Add(loadRoutine);
        }
    }

    public IEnumerator WaitForAllAssetsToLoadRoutine()
    {
        while (true)
        {
            if (_assetsToLoad.Count > 0 || _loadingAssetRoutines.Count > 0)
            {
                yield return null;
                continue;
            }

            break;
        }
    }

    private string NameToEngineNameRemapped(string name)
    {
        string engineName = NameToEngineName(name);

        if (_assetRemap != null && _assetRemap.TryGetValue(engineName, out string? remappedName))
            return remappedName;
        return engineName;
    }

    private int NameToAssetCacheHash(string name, Type type)
    {
        int hash1 = name.GetStableHashCodeASCII();
        int hash2 = type.GetHashCode();
        return Maths.GetCantorPair(hash1, hash2);
    }

    #endregion

    #region Static Setup

    public static string GameDirectory = string.Empty;

    public static void SetupGameDirectory()
    {
        if (RuntimeInformation.OSDescription != "Browser")
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            GameDirectory = Directory.GetCurrentDirectory();
            Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
        }
    }

    #endregion
}