#nullable enable

#region Using

using Emotion.Core.Systems.IO.Sources;
using Emotion.Core.Systems.IO.Storage;
using Emotion.Core.Utility.Coroutines;
using Emotion.Primitives.DataStructures;
using System.Collections.Concurrent;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

#endregion

namespace Emotion.Core.Systems.IO;

/// <summary>
/// Module used to load assets from various sources.
/// </summary>
public partial class AssetLoader
{
    // ***************************************************************************************************************
    // All paths the AssetLoader works with are "engine paths"
    // They conform to the following rules.
    // - The only legal directory separator character is "/"
    // - Relative paths (../) are legal but are not supported by most functions. Use GetNonRelativePath to convert.
    // - Files without extensions are illegal. They are considered directories.
    // ***************************************************************************************************************

    #region Init

    public static string GameDirectory { get; private set; } = string.Empty;

    internal static void SetupGameDirectory() // We want to get this as soon as possible
    {
        if (RuntimeInformation.OSDescription == "Browser") return;

        Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
        GameDirectory = Directory.GetCurrentDirectory();
    }

    /// <summary>
    /// Create the default asset loader which loads the engine assembly, the game assembly, the setup calling assembly
    /// and any embedded assets in them.
    /// </summary>
    internal static AssetLoader CreateDefaultAssetLoader()
    {
        var loader = new AssetLoader();
        foreach (Assembly ass in Helpers.AssociatedAssemblies)
        {
            loader.MountEmbeddedAssets(ass, "Assets", string.Empty);
        }

        InitDebug();

        return loader;
    }

    internal void LateInit() // We kind of need the default asset source mounted (via the host) already.
    {
        if (Exists("AssetRemap.xml"))
        {
            // We need this loaded now, before stuff starts requesting assets
            XMLAsset<Dictionary<string, string>> assetRemapAsset = ONE_Get<XMLAsset<Dictionary<string, string>>>("AssetRemap.xml", null, true);
            if (assetRemapAsset.HasContent())
            {
                Dictionary<string, string>? content = assetRemapAsset.Content;
                AssertNotNull(content);
                foreach ((string from, string to) in content)
                {
                    MountAssetAlias(from, to);
                }
            }
        }
    }

    #endregion

    public void Update()
    {
        // Clear finished routines
        _loadingAssetRoutines.RemoveAll(x => x.Finished);

        // Add reloads
        while (_assetsToReload.TryDequeue(out AssetFileEntry? assetToReload))
        {
            // Dedupe if next is same.
            if (_assetsToReload.TryPeek(out AssetFileEntry? nextAssetToReload))
            {
                if (nextAssetToReload == assetToReload) continue;
            }

            AssertNotNull(assetToReload);

            // Reload loaded assets with the name.
            // Since this is editor/debug code we don't care that this would
            // be slow and copy the list (ConcurrentDictionary enum).
            foreach ((int _, Asset asset) in _createdAssets)
            {
                // We could also end up enqueuing multiple asset instances that
                // represent the same entry loaded as different types.
                if (asset.Name == assetToReload.FullName)
                {
                    IRoutineWaiter loadRoutine = Engine.Jobs.Add(asset.AssetLoader_LoadAsset(this));
                    _loadingAssetRoutines.Add(loadRoutine);
                }
            }
        }

        // Add new assets
        while (_assetsToLoad.TryDequeue(out Asset? asset))
        {
            if (asset.Disposed) continue;

            IRoutineWaiter loadRoutine = Engine.Jobs.Add(asset.AssetLoader_LoadAsset(this));
            _loadingAssetRoutines.Add(loadRoutine);
        }
    }

    #region Asset Lifecycle

    /// <summary>
    /// Get a loaded asset by its name or load it.
    /// This is a legacy API!
    /// </summary>
    public T? Get<T>(string name, bool cache = true) where T : Asset, new()
    {
        return ONE_Get<T>(name, null, true, false, !cache);
    }

    /// <summary>
    /// Free an asset, removing it from the cache and unloading it.
    /// </summary>
    public void DisposeOf(Asset? asset)
    {
        if (asset == null) return;

        if (_createdAssets.TryRemove(asset.UniqueHash, out Asset? removed))
        {
            Assert(asset == removed);
            removed._DoneViaAssetLoader();
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Whether an asset with the provided name exists in any source.
    /// </summary>
    public bool Exists(ReadOnlySpan<char> name)
    {
        return TryGetFileEntry(name) != null;
    }

    public IEnumerator WaitForAllAssetsToLoadRoutine()
    {
        while (!_assetsToLoad.IsEmpty || _loadingAssetRoutines.Count > 0)
        {
            yield return null;
        }
    }

    /// <summary>
    /// Try to get a file entry from the asset file system.
    /// </summary>
    public AssetFileEntry? TryGetFileEntry(ReadOnlySpan<char> name)
    {
        NTreeString<AssetFileEntry>? branch = _fileSystem.GetBranchFromPath(name, SEPARATORS, true, out ReadOnlySpan<char> fileName);
        if (branch == null) return null;

        foreach (AssetFileEntry leaf in branch.Leaves)
        {
            if (fileName.CompareTo(leaf.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                if (leaf is AliasEntry aliasEntry)
                    return TryGetFileEntry(aliasEntry.AssetSourceLoadData);
                else
                    return leaf;
            }
        }

        return null;
    }

    /// <summary>
    /// Get a list of all assets in the specified folder.
    /// </summary>
    public IEnumerable<string> ForEachAssetInFolder(string folder, bool fullName = true)
    {
        ReadOnlySpan<char> pathData = folder;
        NTreeString<AssetFileEntry>? currentFolder = _fileSystem.GetBranchFromPath(pathData, SEPARATORS, false, out _);
        if (currentFolder == null) yield break;

        foreach (AssetFileEntry leaf in currentFolder.Leaves)
        {
            if (fullName)
                yield return leaf.FullName;
            else
                yield return leaf.Name;
        }
    }

    public IEnumerable<string> ForEachAssetWithExtension(string extension, bool fullName = true)
    {
        foreach (AssetFileEntry leaf in _fileSystem.ForEachLeaf())
        {
            if (leaf is AliasEntry) continue;
            if (leaf.Name.EndsWith(extension, StringComparison.InvariantCultureIgnoreCase))
            {
                if (fullName)
                    yield return leaf.FullName;
                else
                    yield return leaf.Name;
            }
        }
        yield break;
    }

    #endregion

    #region Static Path Helpers

    private readonly static ConcurrentDictionary<string, string> _cachedNameToEngineNameConversions = new(); // Reduce allocations

    /// <summary>
    /// Converts the provided file system name to an engine name
    /// </summary>
    public static string NameToEngineName(string name)
    {
        if (_cachedNameToEngineNameConversions.TryGetValue(name, out string? cachedConversion)) return cachedConversion;

        string engineName = name.Replace("//", "/").Replace('\\', '/').ToLowerInvariant();
        _cachedNameToEngineNameConversions.TryAdd(name, engineName);
        return engineName;
    }

    /// <summary>
    /// Get the name of a directory in an asset path.
    /// </summary>
    public static string GetDirectoryName(ReadOnlySpan<char> name)
    {
        if (name.Length == 0) return string.Empty;
        if (name[^1] == '/') return string.Empty;

        int lastSlash = name.LastIndexOf(['/'], StringComparison.InvariantCulture);
        return lastSlash == -1 ? string.Empty : name.Slice(0, lastSlash).ToString();
    }

    /// <summary>
    /// Get the name of the file without directories.
    /// </summary>
    public static string GetFileName(string name)
    {
        if (string.IsNullOrEmpty(name)) return string.Empty;

        int lastSlash = name.LastIndexOf("/", StringComparison.InvariantCulture);
        return lastSlash == -1 || lastSlash == name.Length - 1 ? name : name.Substring(lastSlash + 1);
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
    /// Join two asset paths.
    /// </summary>
    public static string JoinPath(string left, string right)
    {
        if (string.IsNullOrEmpty(left)) return right;
        if (string.IsNullOrEmpty(right)) return left;
        if (left[^1] == '/') left = left[..^1];
        if (right[0] == '/') right = right[1..];
        return $"{left}/{right}";
    }

    private static bool IsPathEmpty(string path)
    {
        return path.Length == 0 || path == ".";
    }

    #endregion

    #region Storage

    public bool Save(string name, StringBuilder sb)
    {
        ReadOnlySpan<char> nameSpan = name.AsSpan();
        IAssetStorage? storage = GetStorageForLocation(nameSpan);
        if (storage == null) return false;

        AssetStorageOperation op = storage.StartSave(nameSpan);
        if (!op.IsValid) return false;

        Stream stream = op.Stream;
        using (StreamWriter streamWriter = new StreamWriter(stream, Encoding.UTF8, leaveOpen: true))
        {
            foreach (ReadOnlyMemory<char> chunk in sb.GetChunks())
            {
                streamWriter.Write(chunk);
            }
        }

        return storage.EndSave(op);
    }

    public bool Save(string name, Span<byte> data)
    {
        ReadOnlySpan<char> nameSpan = name.AsSpan();
        IAssetStorage? storage = GetStorageForLocation(name);
        if (storage == null) return false;

        AssetStorageOperation op = storage.StartSave(nameSpan);
        if (!op.IsValid) return false;

        Stream stream = op.Stream;
        stream.Write(data);

        return storage.EndSave(op);
    }

    public bool Save(string name, byte[] data)
    {
        return Save(name, data.AsSpan());
    }

    public bool Save(string name, string str)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
        return Save(name, bytes);
    }

    #endregion

    #region ONE

    private readonly ConcurrentDictionary<int, Asset> _createdAssets = new();
    private readonly ConcurrentQueue<Asset> _assetsToLoad = new();
    private readonly ConcurrentQueue<AssetFileEntry?> _assetsToReload = new();
    private readonly List<IRoutineWaiter> _loadingAssetRoutines = new(16);

    public T GetInstant<T>(string? name, object? addReferenceToObject = null, bool noCache = false)
        where T : Asset, new()
    {
        return ONE_Get<T>(name, addReferenceToObject, true, false, noCache);
    }

    public T ONE_Get<T>(string? name, object? addRefenceToObject = null, bool loadInline = false, bool loadedAsDependency = false, bool noCache = false)
        where T : Asset, new()
    {
        string enginePath = string.Empty;

        if (!string.IsNullOrEmpty(name))
        {
            AssetFileEntry? entry = TryGetFileEntry(name);
            if (entry != null)
                enginePath = entry.FullName;
            else // We allow loading of assets that don't exist.
                enginePath = AssetLoader.NameToEngineName(name);
        }

        // If the asset already exists, get it.
        int assetCacheHash = Maths.GetCantorPair(enginePath.GetStableHashCodeASCII(), typeof(T).GetHashCode());
        if (!noCache && _createdAssets.TryGetValue(assetCacheHash, out Asset? loadedAsset))
        {
            if (addRefenceToObject != null)
                AddReferenceToAsset(loadedAsset, addRefenceToObject);

            return (T)loadedAsset;
        }

        // Create a new asset and register it.
        T newAsset = new T
        {
            Name = enginePath,
            UniqueHash = assetCacheHash,
            LoadedAsDependency = loadedAsDependency
        };
        if (!noCache)
            _createdAssets.TryAdd(assetCacheHash, newAsset);

        if (addRefenceToObject != null)
            AddReferenceToAsset(newAsset, addRefenceToObject);

        if (loadInline)
        {
            Coroutine.RunInline(newAsset.AssetLoader_LoadAsset(this));
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
    public void ONE_ReloadAsset(ReadOnlySpan<char> name)
    {
        AssetFileEntry? entry = TryGetFileEntry(name);
        if (entry == null) return;
        _assetsToReload.Enqueue(entry);
    }

    #endregion

    #region Debug

    public IEnumerable<Asset> ForEachLoadedAsset()
    {
        return _createdAssets.Values;
    }

    public static bool CanWriteAssets { get; private set; } = false;

    public static string DevModeProjectFolder { get; private set; } = string.Empty;

    public static string DevModeAssetFolder { get; private set; } = string.Empty;

    private static void InitDebug()
    {
        if (Engine.Configuration.DebugMode)
        {
            Engine.Log.Trace("Attempting to add developer mode desktop asset sources.", MessageSource.Debug);

            string? projectFolder = null;
            try
            {
                projectFolder = DetermineProjectFolder();
                Engine.Log.Info($"Found project folder: {projectFolder}", MessageSource.Debug);
            }
            catch (Exception)
            {

            }
            if (projectFolder != null)
            {
                CanWriteAssets = true;
                DevModeProjectFolder = projectFolder;
                DevModeAssetFolder = Path.Join(projectFolder, "Assets");
            }
        }
    }

    private static string DetermineProjectFolder()
    {
        string currentDirectory = AssetLoader.GameDirectory;
        DirectoryInfo? parentDir = Directory.GetParent(currentDirectory);
        int levelsBack = 1;
        while (parentDir != null)
        {
            bool found = false;
            foreach (DirectoryInfo dir in parentDir.GetDirectories())
            {
                if (dir.Name == "Assets")
                {
                    found = true;
                    break;
                }
            }

            if (found)
            {
                found = false;
                foreach (FileInfo file in parentDir.EnumerateFiles())
                {
                    if (file.Extension == ".csproj" || file.Extension == ".sln" || file.Extension == ".slnx")
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (found)
            {
                // Convert to relative path.
                StringBuilder s = new StringBuilder();
                for (int i = 0; i < levelsBack; i++)
                {
                    s.Append("..");
                    if (i != levelsBack - 1) s.Append("\\");
                }

                return s.ToString();
            }

            parentDir = Directory.GetParent(parentDir.FullName);
            levelsBack++;
        }

        return string.Empty;
    }

    #endregion
}