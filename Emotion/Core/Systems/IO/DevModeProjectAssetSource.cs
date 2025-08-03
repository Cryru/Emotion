#nullable enable

#region Using

using System.IO;
using Emotion.Core.Platform.Implementation.CommonDesktop;
using Emotion.Core.Systems.Logging;

#endregion

namespace Emotion.Core.Systems.IO;

public class DevModeProjectAssetSource : FileAssetStore
{
    public override string StoreFolder => "assets";

    private FileSystemWatcher _watcher;

    public DevModeProjectAssetSource(string assetFolder) : base(assetFolder, false)
    {
        FileSystemWatcher watcher = new FileSystemWatcher();
        watcher.Path = _folderFs;
        watcher.IncludeSubdirectories = true;
        watcher.NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName;
        watcher.Created += AssetChangeEvent;
        watcher.Deleted += AssetChangeEvent;
        watcher.Changed += AssetChangeEvent;
        watcher.Renamed += AssetChangeEvent;
        watcher.EnableRaisingEvents = true;
        _watcher = watcher;
    }

    protected void AssetChangeEvent(object sender, FileSystemEventArgs e)
    {
        string filePath = e.FullPath;
        string enginePath = FilePathToEnginePath(filePath, FolderInPath);
        if (e.ChangeType == WatcherChangeTypes.Created)
        {
            Engine.Log.Trace($"Detected new asset - {enginePath}!", MessageSource.Debug);
            InternalManifest.TryAdd(enginePath, filePath);
        }
        else if (e.ChangeType == WatcherChangeTypes.Deleted)
        {
            Engine.Log.Trace($"Detected deletion of asset - {enginePath}!", MessageSource.Debug);
            InternalManifest.Remove(enginePath, out string? _);
        }
        else if (e.ChangeType == WatcherChangeTypes.Renamed && e is RenamedEventArgs eRename)
        {
            // todo: we could probably just hot reload the asset as the new name, right?
            // case: someone is holding a handle to an invalid asset name, a valid asset is renamed to that name
            // the handle should now be valid, and the old asset...should be invalid?
            // Must decide how to handle deletion first.

            string oldPath = eRename.OldFullPath;
            string oldEnginePath = FilePathToEnginePath(oldPath, FolderInPath);
            InternalManifest.Remove(oldEnginePath, out string? _);
            InternalManifest.TryAdd(enginePath, filePath);
            Engine.AssetLoader.ONE_ReloadAsset(enginePath);
        }
        else if (e.ChangeType == WatcherChangeTypes.Changed)
        {
            // Engine.Log.Trace($"Detected change in asset - {enginePath}!", MessageSource.Debug);
            Engine.AssetLoader.ONE_ReloadAsset(enginePath);
        }
    }

    public override bool HasAsset(string enginePath)
    {
        return InternalManifest.ContainsKey(enginePath);
    }

    public override string ToString()
    {
        return $"Developer Mode Source; Project Asset Folder @ {Folder}";
    }
}
