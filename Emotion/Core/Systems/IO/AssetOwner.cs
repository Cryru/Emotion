#nullable enable

using Emotion.Core.Utility.Coroutines;
using System.Reflection.Metadata;

namespace Emotion.Core.Systems.IO;

[DontSerialize]
public class AssetOwner<TAsset, TObject> where TAsset : Asset, IAssetContainingObject<TObject>, new()
{
    private AssetObjectReference<TAsset, TObject> _currentRef = AssetObjectReference<TAsset, TObject>.Invalid;
    private Coroutine? _currentSetRoutine = Coroutine.CompletedRoutine;
    private Action<AssetOwner<TAsset, TObject>, object?>? _onChange = null;
    private object? _onChangeUserData = null;
    private bool _deferredSet = false;

    public Coroutine? Set(AssetObjectReference<TAsset, TObject> newAsset, bool deferSetToGetCurrent = false)
    {
        // Dedupe?
        if (_currentRef == newAsset)
            return null;

        DetachOldAsset(_currentRef);
        _currentRef = newAsset;

        if (deferSetToGetCurrent)
        {
            _deferredSet = true;
            return null;
        }

        return SetInternal();
    }

    private Coroutine? SetInternal()
    {
        _deferredSet = false;
        AssetObjectReference<TAsset, TObject> newAsset = _currentRef;

        // New object that is already loaded, or invalid
        if (newAsset.Type == AssetOrObjectReferenceType.Object || !newAsset.IsValid())
        {
            AttachNewAsset(newAsset, true);
            InternalOnChanged();
            return null;
        }

        _currentSetRoutine = Engine.CoroutineManager.StartCoroutine(LoadNewAssetAndAttach(newAsset));
        return _currentSetRoutine;
    }

    private IEnumerator LoadNewAssetAndAttach(AssetObjectReference<TAsset, TObject> handle)
    {
        bool addedOwner = false;
        if (handle.Type == AssetOrObjectReferenceType.AssetName)
        {
            TAsset asset = Engine.AssetLoader.Get<TAsset>(handle.AssetName, this, false, false);
            handle = asset;
            addedOwner = true;
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

        AttachNewAsset(handle, !addedOwner);
        InternalOnChanged();
    }

    public Coroutine? GetCurrentLoading()
    {
        if (_deferredSet) SetInternal();

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

    private void DetachOldAsset(AssetObjectReference<TAsset, TObject> handle)
    {
        TAsset? asset = handle.Asset;
        asset?.OnLoaded -= AssetHotReloaded;
        RemoveOwnership(handle, this);
    }

    private void AttachNewAsset(AssetObjectReference<TAsset, TObject> handle, bool addOwnership)
    {
        TAsset? asset = handle.Asset;
        asset?.OnLoaded += AssetHotReloaded;
        if (addOwnership)
            AddOwnership(handle, this);
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