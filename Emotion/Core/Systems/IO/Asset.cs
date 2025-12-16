#nullable enable

#region Using

using Emotion.Core.Systems.Logging;
using Emotion.Core.Utility.Coroutines;
using System.Reflection.Metadata;

#endregion

namespace Emotion.Core.Systems.IO;

/// <summary>
/// A handle to an asset.
/// </summary>
[DontSerialize]
public abstract class Asset : IRoutineWaiter
{
    /// <summary>
    /// The engine path of the asset.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The asset id for this asset within the AssetLoader.
    /// </summary>
    public int UniqueHash { get; set; } = 0;

    /// <summary>
    /// Whether this asset was loaded as a dependency. It could still be a
    /// dependency if it was loaded in another way first.
    /// </summary>
    public bool LoadedAsDependency { get; set; }

    /// <summary>
    /// The byte size of the asset when loaded from the asset source.
    /// Subsequent operations may cause the asset to take more space/less space.
    /// Such as decompression etc.
    /// </summary>
    public int ByteSize { get; set; }

    /// <summary>
    /// Whether the asset is processed by the AssetLoader. If this is false its thread
    /// has either not ran yet or is running currently.
    /// This being true doesn't guarantee the asset is valid, check Loaded for that.
    /// </summary>
    public bool Processed { get; protected set; }

    /// <summary>
    /// Whether the asset is loaded - meaning its in a usable state.
    /// </summary>
    public bool Loaded { get; protected set; }

    /// <summary>
    /// Whether the asset has been freed.
    /// </summary>
    public bool Disposed { get; protected set; }

    /// <summary>
    /// Whether the asset is being loaded inline on the main thread (bad).
    /// </summary>
    public bool LoadingInline { get; protected set; }

    /// <summary>
    /// Fires when the asset is loaded or hot reloaded.
    /// </summary>
    public event Action<Asset>? OnLoaded;

    protected bool _useNewLoading = false;

    /// <summary>
    /// Called by the asset loader on the asset loading thread(s),
    /// which performs the IO and asset creation and/or hot reloading if already loaded.
    /// </summary>
    public IEnumerator AssetLoader_LoadAsset(AssetLoader assetLoader, bool loadingInline = false)
    {
        Stopwatch timer = new Stopwatch();
        timer.Start();

        LoadingInline = loadingInline;

        AssetFileEntry? entry = assetLoader.TryGetFileEntry(Name);
        if (entry == null)
        {
            Processed = true;
            yield break;
        }
        ReadOnlyMemory<byte> data = ReadOnlyMemory<byte>.Empty;

        // Due to sharing violations we should try to hot reload this in a try-catch.
        //Engine.SuppressLogExceptions(true);
        int attempts = 0;
        while (attempts < 10)
        {
            FileReadRoutineResult fileRead = entry.GetAssetData();
            yield return fileRead;

            if (fileRead.Finished && !fileRead.Errored)
            {
                data = fileRead.FileBytes;
                break;
            }

            //try
            //{
            //    data = source.GetAsset(Name);
            //    break;
            //}
            //catch (Exception)
            //{
            //    // Error, try again :(
            //}
            //finally
            //{
            //    Engine.SuppressLogExceptions(false);
            //}

            attempts++;
        }

        ByteSize = data.Length;

        if (_useNewLoading)
        {
            yield return Internal_LoadAssetRoutine(data);
            if (!LoadedAsDependency)
            {
                if (Loaded)
                    Engine.Log.Info($"Reloaded asset '{Name}'{(_dependencies == null ? "" : $" ({_dependencies.Count} Dependencies)")} in {timer.ElapsedMilliseconds}ms", MessageSource.AssetLoader);
                else
                    Engine.Log.Info($"Loaded asset '{Name}'{(_dependencies == null ? "" : $" ({_dependencies.Count} Dependencies)")} in {timer.ElapsedMilliseconds}ms", MessageSource.AssetLoader);
            }
            Loaded = true;
            if (OnLoaded != null)
                Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine());
        }
        else
        {
            // Hot reload
            if (Loaded)
            {
                ReloadInternal(data);
                if (!LoadedAsDependency) Engine.Log.Info($"Reloaded asset '{Name}'", MessageSource.AssetLoader);
                if (OnLoaded != null)
                    Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine());
            }
            else
            {
                CreateInternal(data);
                if (!LoadedAsDependency) Engine.Log.Info($"Loaded asset '{Name}'", MessageSource.AssetLoader);
                Loaded = true;
                if (OnLoaded != null)
                    Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine());
            }
        }

        Processed = true;
    }

    public void AssetLoader_CreateLegacy(ReadOnlyMemory<byte> data)
    {
        Loaded = true;
        Processed = true;
        CreateInternal(data);
        Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine());
    }

    // Loaded event is executed as a coroutine as assets are loaded in another thread
    // and we don't want threading issues.
    private IEnumerator ExecuteAssetLoadedEventsRoutine()
    {
        OnLoaded?.Invoke(this);
        yield break;
    }

    protected abstract void CreateInternal(ReadOnlyMemory<byte> data);

    protected virtual void ReloadInternal(ReadOnlyMemory<byte> data)
    {

    }

    protected virtual IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        yield break;
    }

    /// <summary>
    /// Cleans up the asset resources and sets it into the disposed state.
    /// DO NOT CALL MANUALLY! Use AssetLoader.DisposeOf
    /// </summary>
    internal void _DoneViaAssetLoader()
    {
        if (Disposed) return;
        DisposeInternal();
        Disposed = true;
        Loaded = false;
    }

    protected abstract void DisposeInternal();

    /// <summary>
    /// The hashcode of the asset. Derived from the name.
    /// </summary>
    /// <returns>The hashcode of the asset.</returns>
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    #region File Extension Support

    public static string[] GetFileExtensionsSupported<T>() where T : Asset
    {
        var typ = typeof(T);
        if (_assetTypeToExtensions.TryGetValue(typ, out string[]? extensions))
            return extensions;

        return Array.Empty<string>();
    }

    protected static Dictionary<Type, string[]> _assetTypeToExtensions = new();

    protected static void RegisterFileExtensionSupport<T>(string[] extensions) where T : Asset
    {
        _assetTypeToExtensions.Add(typeof(T), extensions);
    }

    #endregion

    #region Routine Waiter

    public bool Finished => Processed;

    public void Update()
    {
        // nop
    }

    #endregion

    #region Dependencies

    private List<Asset>? _dependencies;

    protected void LoadAssetDependency<T, TObject>(AssetObjectReference<T, TObject> assetOrObjectReference)
        where T : Asset, IAssetContainingObject<TObject>, new()
    {
        T? asset = null;
        if (assetOrObjectReference.Type == AssetOrObjectReferenceType.AssetName)
        {
            asset = Engine.AssetLoader.Get<T>(assetOrObjectReference.AssetName, this, LoadingInline, true);
        }
        else if (assetOrObjectReference.Type == AssetOrObjectReferenceType.Asset)
        {
            asset = assetOrObjectReference.Asset;
            if (asset != null)
                Engine.AssetLoader.AddReferenceToAsset(asset, this);
        }
        if (asset == null) return;

        _dependencies ??= new List<Asset>();
        _dependencies.Add(asset);
    }

    protected T LoadAssetDependency<T>(string? name, bool cachedLoad = true) where T : Asset, new()
    {
        T dependantAsset = Engine.AssetLoader.Get<T>(name, this, LoadingInline, true, !cachedLoad);

        _dependencies ??= new List<Asset>();
        _dependencies.Add(dependantAsset);

        return dependantAsset;
    }

    protected IEnumerator WaitAllDependenciesToLoad()
    {
        if (_dependencies == null) yield break;

        while (true)
        {
            bool anyLoading = false;
            for (int i = 0; i < _dependencies.Count; i++)
            {
                Asset dependent = _dependencies[i];
                if (!dependent.Loaded)
                {
                    anyLoading = true;
                    yield return null;
                }
            }

            if (!anyLoading)
                break;
        }
    }

    #endregion
}