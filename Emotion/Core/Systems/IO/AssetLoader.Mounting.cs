#nullable enable

using Emotion.Core.Systems.IO.Sources;
using Emotion.Core.Systems.IO.Storage;
using Emotion.Primitives.DataStructures;
using Emotion.Standard.DataStructures.OptimizedStringReadWrite;
using System.IO;
using System.Reflection;

namespace Emotion.Core.Systems.IO;

public partial class AssetLoader
{
    private NTreeString<AssetFileEntry> _fileSystem = new();

    private static readonly char[] SEPARATORS = ['/', '\\'];
    private List<AssetModificationWatcher>? _watchers;
    private List<IAssetStorage> _storages = new();
    private IAssetStorage? _rootStorage = null;

    public void MountFileSystemFolder(string actualFolderPath, string mountPoint, bool canWriteTo = false)
    {
        if (!Directory.Exists(actualFolderPath))
        {
            Directory.CreateDirectory(actualFolderPath);
            Engine.Log.Info($"Created file system directory - {actualFolderPath}", MessageSource.AssetLoader);
        }

        mountPoint = IsPathEmpty(mountPoint) ? string.Empty : mountPoint;

        NTreeString<AssetFileEntry> mountPointVirtual = IsPathEmpty(mountPoint) ? _fileSystem : _fileSystem.AddGetBranch(mountPoint, true);

        Span<char> actualFolderPathStrip = stackalloc char[actualFolderPath.Length + 1];
        actualFolderPath.CopyTo(actualFolderPathStrip);
        actualFolderPathStrip[^1] = Path.DirectorySeparatorChar;

        int count = 0;
        IEnumerable<string> fileEnum = Directory.EnumerateFiles(actualFolderPath, "*", SearchOption.AllDirectories);
        foreach (string nativeFilePath in fileEnum)
        {
            ReadOnlySpan<char> fileNameVirtual = nativeFilePath;
            int idx = fileNameVirtual.IndexOf(actualFolderPathStrip);
            Assert(idx != -1);
            if (idx != -1)
                fileNameVirtual = fileNameVirtual.Slice(idx + actualFolderPathStrip.Length);

            NTreeString<AssetFileEntry> branch = mountPointVirtual.AddGetBranchFromPath(fileNameVirtual, SEPARATORS, true, out ReadOnlySpan<char> fileName);
            branch.Leaves.Add(new AssetFileEntry<FileSource, string>(fileName, branch, nativeFilePath));
            count++;
        }
        Engine.Log.Info($"Mounted folder '{actualFolderPath}' @ '{mountPoint}' containing {count} assets.", MessageSource.AssetLoader);

        // Start watching this folder for changes to
        // provide a hot-reload experience :)
        if (Engine.Configuration.DebugMode)
        {
            _watchers ??= new List<AssetModificationWatcher>();
            _watchers.Add(new AssetModificationWatcher(this, actualFolderPath, mountPoint));
        }

        if (canWriteTo)
        {
            var store = new FileStorage(this, actualFolderPath, mountPoint);
            if (mountPoint == string.Empty)
            {
                _rootStorage = store;
                Engine.Log.Info($"Mounted root writable location '{actualFolderPath}'", MessageSource.AssetLoader);
            }
            else
            {
                _storages.Add(store);
                Engine.Log.Info($"Mounted writable location '{actualFolderPath}' @ '{mountPoint}'", MessageSource.AssetLoader);
            }
        }
    }

    public void MountFile(string actualFilePath, ReadOnlySpan<char> virtualFilePath)
    {
        NTreeString<AssetFileEntry> branch = _fileSystem.AddGetBranchFromPath(virtualFilePath, SEPARATORS, true, out ReadOnlySpan<char> fileName);
        branch.Leaves.Add(new AssetFileEntry<FileSource, string>(fileName, branch, actualFilePath));
        Engine.Log.Info($"Mounted file '{actualFilePath}' @ '{virtualFilePath}'", MessageSource.AssetLoader);
    }

    public void MountEmbeddedAssets(Assembly assembly, string resourceFolder, string mountPoint)
    {
        NTreeString<AssetFileEntry> mountPointVirtual = IsPathEmpty(mountPoint) ? _fileSystem : _fileSystem.AddGetBranch(mountPoint, true);

        int count = 0;
        string[] resources = assembly.GetManifestResourceNames();
        foreach (string resourcePath in resources)
        {
            ReadOnlySpan<char> fileNameVirtual = resourcePath;
            int idx = fileNameVirtual.IndexOf(resourceFolder);
            Assert(idx != -1);
            if (idx != -1)
                fileNameVirtual = fileNameVirtual.Slice(idx + resourceFolder.Length + 1);

            // We need to cut off the extension as it has the same separator as the resource folder structure - the dot '.'
            int extensionStart = fileNameVirtual.LastIndexOf('.');

            ReadOnlySpan<char> extension = ReadOnlySpan<char>.Empty;
            if (extensionStart != -1)
            {
                extension = fileNameVirtual.Slice(extensionStart);
                fileNameVirtual = fileNameVirtual.Slice(0, extensionStart);
            }

            NTreeString<AssetFileEntry> branch = mountPointVirtual.AddGetBranchFromPath(fileNameVirtual, ['.'], true, out ReadOnlySpan<char> fileName);
            var newEntry = new AssetFileEntry<EmbeddedSource, EmbeddedSource.LoadData>(
                $"{fileName}{extension}", branch, new EmbeddedSource.LoadData(assembly, resourcePath)
            );
            branch.Leaves.Add(newEntry);
            count++;
        }

        Engine.Log.Info($"Mounted '{assembly.GetName().Name}' @ '{mountPoint}' containing {count} assets.", MessageSource.AssetLoader);
    }

    public void MountCustomSourceAsset<T, T2>(string fileNameEngine, T2 sourceLoadData)
        where T : IAssetSource<T2>
    {
        NTreeString<AssetFileEntry> branch = _fileSystem.AddGetBranchFromPath(fileNameEngine, SEPARATORS, true, out ReadOnlySpan<char> fileName);
        branch.Leaves.Add(new AssetFileEntry<T, T2>(fileName.ToString(), branch, sourceLoadData));

        Engine.Log.Info($"Mounted custom source asset @ '{fileNameEngine}' with source {typeof(T).Name}.", MessageSource.AssetLoader);
    }

    public void MountAssetAlias(string enginePath, string dstEnginePath)
    {
        NTreeString<AssetFileEntry> branch = _fileSystem.AddGetBranchFromPath(enginePath, SEPARATORS, true, out ReadOnlySpan<char> fileName);
        branch.Leaves.Add(new AliasEntry(fileName.ToString(), branch, dstEnginePath));
    }

    /// <summary>
    /// Unmount a file entry path.
    /// This will not unload any assets loaded from it.
    /// </summary>
    public void UnmountFile(ReadOnlySpan<char> virtualPath)
    {
        NTreeString<AssetFileEntry>? branch = _fileSystem.GetBranchFromPath(virtualPath, SEPARATORS, true, out ReadOnlySpan<char> fileName);
        if (branch == null) return;

        // Remove the entry at that path.
        for (int i = 0; i < branch.Leaves.Count; i++)
        {
            AssetFileEntry leaf = branch.Leaves[i];
            if (fileName.CompareTo(leaf.Name, StringComparison.InvariantCultureIgnoreCase) == 0)
            {
                branch.Leaves.RemoveAt(i);
                break;
            }
        }
    }

    protected IAssetStorage? GetStorageForLocation(ReadOnlySpan<char> virtualPath)
    {
        static bool PathStartsWithMount(ReadOnlySpan<char> path, ReadOnlySpan<char> mount)
        {
            for (int i = 0; i < mount.Length; i++)
            {
                char p = path[i];
                char m = mount[i];
                if (p == '\\') p = '/';
                if (char.ToLowerInvariant(p) != char.ToLowerInvariant(m))
                    return false;
            }

            return true;
        }

        foreach (IAssetStorage storage in _storages)
        {
            string storageAt = storage.MountPoint;
            if (PathStartsWithMount(virtualPath, storageAt))
                return storage;
        }

        return _rootStorage;
    }

    private class AssetModificationWatcher
    {
        private FileSystemWatcher _watcher;
        private AssetLoader _loader;
        private string _folder;
        private string _virtualFolder;

        public AssetModificationWatcher(AssetLoader loader, string folder, string virtualFolder)
        {
            _loader = loader;
            _folder = folder;
            _virtualFolder = virtualFolder;

            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = folder;
            watcher.IncludeSubdirectories = true;
            watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
            watcher.Created += AssetChangeEvent;
            watcher.Deleted += AssetChangeEvent;
            watcher.Changed += AssetChangeEvent;
            watcher.Renamed += AssetChangeEvent;
            watcher.EnableRaisingEvents = true;
            _watcher = watcher;
        }

        public void Dispose()
        {
            _watcher.Dispose();
        }

        private int ActualFilePathToVirtualPath(string filePath, Span<char> memory)
        {
            // Remove the actual file folder from the path
            ReadOnlySpan<char> actualFilePathStripped = filePath.AsSpan();
            int idx = actualFilePathStripped.GetIndexOfFollowedByChar(_folder, Path.DirectorySeparatorChar);
            if (idx != -1)
                actualFilePathStripped = actualFilePathStripped.Slice(idx + _folder.Length + 1);

            ValueStringWriter writer = new ValueStringWriter(memory);
            if (_virtualFolder.Length != 0)
            {
                writer.WriteString(_virtualFolder);
                writer.WriteChar('/');
            }
            writer.WriteString(actualFilePathStripped);

            return writer.CharsWritten;
        }

        protected void AssetChangeEvent(object sender, FileSystemEventArgs e)
        {
            string filePath = e.FullPath;
            if (filePath.EndsWith(".backup") || filePath.EndsWith(".writeTemp")) return;

            Span<char> virtualPath = stackalloc char[1024];
            {
                int written = ActualFilePathToVirtualPath(filePath, virtualPath);
                virtualPath = virtualPath.Slice(0, written);
            }

            if (e.ChangeType == WatcherChangeTypes.Created)
            {
                Engine.Log.Trace($"Asset created - {filePath}!", MessageSource.Debug);
                _loader.MountFile(filePath, virtualPath);
            }
            else if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                Engine.Log.Trace($"Asset deleted - {filePath}!", MessageSource.Debug);
                _loader.UnmountFile(virtualPath);
            }
            else if (e.ChangeType == WatcherChangeTypes.Renamed && e is RenamedEventArgs eRename)
            {
                string oldPath = eRename.OldFullPath;
                if (oldPath.EndsWith(".writeTemp")) return;

                Span<char> oldVirtualPath = stackalloc char[1024];
                {
                    int written = ActualFilePathToVirtualPath(oldPath, oldVirtualPath);
                    oldVirtualPath = oldVirtualPath.Slice(0, written);
                }

                Engine.Log.Trace($"Asset renamed - {oldPath} -> {filePath}!", MessageSource.Debug);

                _loader.UnmountFile(oldVirtualPath);
                _loader.MountFile(filePath, virtualPath);
            }
            else if (e.ChangeType == WatcherChangeTypes.Changed)
            {
                // Engine.Log.Trace($"Detected change in asset - {enginePath}!", MessageSource.Debug);
                _loader.ReloadAsset(virtualPath);
            }
        }
    }
}
