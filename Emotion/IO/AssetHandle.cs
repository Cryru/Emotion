using Emotion.Common.Serialization;
using Emotion.Game.Time.Routines;

#nullable enable

namespace Emotion.IO;

public abstract class AssetHandleBase : IRoutineWaiter
{
    public string Name { get; init; }

    public bool AssetLoaded;

    protected AssetHandleBase(string name)
    {
        Name = name;
    }

    public abstract bool LoadAsset(AssetSource source);

    #region Routine Waiter

    public bool Finished => AssetLoaded || !Engine.AssetLoader.IsAssetHandleQueued(this); // if not queued we consider it non-existing at this point

    public void Update()
    {
        // nop
    }

    #endregion
}

[DontSerialize]
public class AssetHandle<T> : AssetHandleBase where T : Asset, new()
{
    /// <summary>
    /// An empty invalid asset of this specified type.
    /// The Asset property will be null.
    /// </summary>
    public static AssetHandle<T> Empty = new(string.Empty);

    /// <summary>
    /// The asset type itself.
    /// </summary>
    public T? Asset;

    /// <summary>
    /// Fires when the asset is loaded or hot reloaded.
    /// </summary>
    public event Action<T>? OnAssetLoaded;

    public AssetHandle(string name) : base(name)
    {
    }

    /// <summary>
    /// Called by the asset loader on the asset loading thread(s),
    /// which performs the IO and asset creation and/or hot reloading if
    /// already loaded.
    /// </summary>
    public override bool LoadAsset(AssetSource source)
    {
        if (source == null) return true; // ???

        ReadOnlyMemory<byte> data;

        // Due to sharing violations we should try to hot reload this in a try-catch.
        Engine.SuppressLogExceptions(true);
        try
        {
            data = source.GetAsset(Name);
        }
        catch (Exception)
        {
            return false;
        }
        finally
        {
            Engine.SuppressLogExceptions(false);
        }

        // Hot reload
        if (Asset != null)
        {
            if (Asset is not IHotReloadableAsset reloadableAsset) return true;

            reloadableAsset.Reload(data);
            Engine.Log.Info($"Reloaded asset '{Name}'", MessageSource.AssetLoader);
            Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine(Asset));
            return true;
        }

        Asset = new T { Name = Name };
        Asset.Create(data);
        AssetLoaded = true;
        Engine.CoroutineManager.StartCoroutine(ExecuteAssetLoadedEventsRoutine(Asset));
        return true;
    }

    // Loaded event is executed as a coroutine as assets are loaded in another thread
    // and we don't want threading issues.
    private IEnumerator ExecuteAssetLoadedEventsRoutine(T asset)
    {
        OnAssetLoaded?.Invoke(asset);
        yield break;
    }
}

public class SerializableAssetHandle<T> where T : Asset, new()
{
    public string? Name { get; set; }

    public AssetHandle<T> GetAssetHandle(object? referenceObject = null)
    {
        if (string.IsNullOrEmpty(Name)) return AssetHandle<T>.Empty;
        return Engine.AssetLoader.ONE_Get<T>(Name, referenceObject);
    }

    public static implicit operator SerializableAssetHandle<T>(string? name)
    {
        return new SerializableAssetHandle<T> { Name = name };
    }

    public static implicit operator string?(SerializableAssetHandle<T> handle)
    {
        return handle.Name;
    }

    public static implicit operator SerializableAssetHandle<T>(AssetHandle<T> handle)
    {
        return new SerializableAssetHandle<T> { Name = handle.Name };
    }

    public static implicit operator AssetHandle<T>(SerializableAssetHandle<T> handle)
    {
        return handle.GetAssetHandle();
    }
}