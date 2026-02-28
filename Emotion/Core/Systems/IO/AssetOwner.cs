#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Core.Systems.IO;

[DontSerialize]
public class AssetOwner<TAsset, TObject> where TAsset : Asset, IAssetContainingObject<TObject>, new()
{
    private Coroutine? _currentSetRoutine = Coroutine.CompletedRoutine;
    private Action<AssetOwner<TAsset, TObject>, object?>? _onChange = null;
    private object? _onChangeUserData = null;
    private bool _deferredSet = false;

    private AssetObjectReference<TAsset, TObject> _currentRef = AssetObjectReference<TAsset, TObject>.Invalid;
    private bool _currentOwnershipEstablished = false;

    private bool _loadingOwnershipEstablished = false;
    private AssetObjectReference<TAsset, TObject> _loadingRef = AssetObjectReference<TAsset, TObject>.Invalid;

    public Coroutine? Set(AssetObjectReference<TAsset, TObject> newAsset, bool deferSetToGetCurrent = false)
    {
        // Dedupe?
        if (_currentRef == newAsset || _loadingRef == newAsset)
            return null;

        _loadingRef = newAsset;
        _loadingOwnershipEstablished = false;

        if (deferSetToGetCurrent)
        {
            _loadingRef = newAsset;
            _deferredSet = true;
            return null;
        }

        return SetInternal();
    }

    private Coroutine? SetInternal()
    {
        _deferredSet = false;
        AssetObjectReference<TAsset, TObject> newAsset = _loadingRef;

        // New object that is already loaded, or invalid
        if (newAsset.Type == AssetOrObjectReferenceType.Object || !newAsset.IsValid())
        {
            Assert(!_loadingOwnershipEstablished);
            AttachNewAsset(newAsset);
            InternalOnChanged();
            return null;
        }

        _currentSetRoutine = Engine.CoroutineManager.StartCoroutine(LoadNewAssetAndAttach(newAsset));
        return _currentSetRoutine;
    }

    private IEnumerator LoadNewAssetAndAttach(AssetObjectReference<TAsset, TObject> handle)
    {
        Assert(!_loadingOwnershipEstablished);

        if (handle.Type == AssetOrObjectReferenceType.AssetName)
        {
            TAsset asset = Engine.AssetLoader.Get<TAsset>(handle.AssetName, this, false, false);
            handle = asset;

            // Ownership established through loading the asset itself which also
            // enables ownership tracking for the asset.
            _loadingOwnershipEstablished = true;
        }

        if (handle.Type == AssetOrObjectReferenceType.Asset)
        {
            TAsset? asset = handle.Asset;
            AssertNotNull(asset);
            if (!asset.Loaded)
                yield return asset;

            TObject? obj = asset.GetObject();
            if (obj != null)
                handle = obj;
        }
        _currentRef = handle;

        AttachNewAsset(handle);
        InternalOnChanged();
    }

    public Coroutine? GetCurrentLoading()
    {
        if (_deferredSet)
            return SetInternal();

        if (CanBeUsed()) return null;
        return _currentSetRoutine;
    }

    public AssetObjectReference<TAsset, TObject> GetCurrentReference()
    {
        return _currentRef;
    }

    public TObject? GetCurrentObject()
    {
        if (_currentRef == null || !_currentRef.IsValid()) return default;

        Assert(_currentRef.Type == AssetOrObjectReferenceType.Object);
        return _currentRef.AssetObject;
    }

    public bool CanBeUsed()
    {
        return _currentRef.Type == AssetOrObjectReferenceType.Object;
    }

    public void Done()
    {
        DetachOldAsset(_currentRef);
        _currentRef = AssetObjectReference<TAsset, TObject>.Invalid;
        _currentSetRoutine?.RequestStop();
        _onChange = null;
        _onChangeUserData = null;
    }

    private void DetachOldAsset(in AssetObjectReference<TAsset, TObject> handle)
    {
        TAsset? asset = handle.Asset;
        asset?.OnLoaded -= AssetHotReloaded;

        if (_currentOwnershipEstablished)
        {
            RemoveOwnership(handle, this);
            _currentOwnershipEstablished = false;
        }
    }

    private void AttachNewAsset(in AssetObjectReference<TAsset, TObject> handle)
    {
        AssetObjectReference<TAsset, TObject> old = _currentRef;
        _currentRef = handle;
        DetachOldAsset(old);

        TAsset? asset = handle.Asset;
        asset?.OnLoaded += AssetHotReloaded;

        if (!_currentOwnershipEstablished)
        {
            AddOwnership(handle, this);
            _currentOwnershipEstablished = true;
        }
    }

    private void AssetHotReloaded(Asset? asset)
    {
        InternalOnChanged();
    }

    public void SetOnChangeCallback(Action<AssetOwner<TAsset, TObject>, object?> onChangeCallback, object? param1)
    {
        _onChange = onChangeCallback;
        _onChangeUserData = param1;
    }

    protected virtual void InternalOnChanged()
    {
        _onChange?.Invoke(this, _onChangeUserData);
    }

    #region Ownership API

    private static void AddOwnership(AssetObjectReference<TAsset, TObject> handle, object owner)
    {
        if (!handle.IsValid()) return;
        Assert(handle.Type == AssetOrObjectReferenceType.Object); // Must be loaded!
        Engine.AssetLoader.AddReferenceToAsset(handle.Asset, owner);
    }

    private static void RemoveOwnership<T>(AssetObjectReference<TAsset, TObject> handle, T obj) where T : notnull
    {
        if (!handle.IsValid()) return;
        Assert(handle.Type == AssetOrObjectReferenceType.Object); // Must be loaded!
        Engine.AssetLoader.RemoveReferenceFromAsset(handle.Asset, obj);
    }


    #endregion
}