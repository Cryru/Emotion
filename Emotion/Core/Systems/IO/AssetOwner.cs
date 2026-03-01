#nullable enable

using Emotion.Core.Utility.Coroutines;

namespace Emotion.Core.Systems.IO;

[DontSerialize]
public class AssetOwner<TAsset, TObject> where TAsset : Asset, IAssetContainingObject<TObject>, new()
{
    private Action<AssetOwner<TAsset, TObject>, object?>? _onChange = null;
    private object? _onChangeUserData = null;

    private bool _currentlyLoading = false;
    private Coroutine? _currentLoadingRoutine = Coroutine.CompletedRoutine;
    private AssetObjectReference<TAsset, TObject> _loadingRef = AssetObjectReference<TAsset, TObject>.Invalid;

    private bool _deferredSet = false;

    private AssetObjectReference<TAsset, TObject> _currentRef = AssetObjectReference<TAsset, TObject>.Invalid;

    public Coroutine? Set(AssetObjectReference<TAsset, TObject> newAsset, bool deferSetToGetCurrent = false)
    {
        // Dedupe?
        if (_currentlyLoading)
        {
            if (_loadingRef == newAsset)
                return null;
        }
        else
        {
            if (_currentRef == newAsset)
                return null;
        }

        _loadingRef = newAsset;
        _currentlyLoading = true;

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

        if (TryUseRightAway(ref _loadingRef))
        {
            AttachNewAsset(_loadingRef);
            return null;
        }

        _currentLoadingRoutine = Engine.CoroutineManager.StartCoroutine(WaitForNewToLoadRoutine(_loadingRef));
        return _currentLoadingRoutine;
    }

    private bool TryUseRightAway(ref AssetObjectReference<TAsset, TObject> handle)
    {
        // Invalid
        if (handle.Type == AssetOrObjectReferenceType.None) return true;

        // If asset - try to resolve
        if (handle.Type == AssetOrObjectReferenceType.AssetName)
            handle = Engine.AssetLoader.Get<TAsset>(handle.AssetName, this);

        // If asset - check if already loaded
        if (handle.Type == AssetOrObjectReferenceType.Asset)
        {
            AssertNotNull(handle.Asset);
            if (handle.Asset.Loaded)
            {
                TObject? obj = handle.Asset.GetObject();
                if (obj == null)
                    handle = AssetObjectReference<TAsset, TObject>.Invalid;
                else
                    handle = obj;
            }
            else
            {
                return false;
            }
        }

        // If object - we're good to go
        if (handle.Type == AssetOrObjectReferenceType.Object)
        {
            if (handle.AssetObject == null)
                handle = AssetObjectReference<TAsset, TObject>.Invalid;

            return true;
        }

        // Unreachable?
        Assert(false);
        return false;
    }

    private IEnumerator WaitForNewToLoadRoutine(AssetObjectReference<TAsset, TObject> handle)
    {
        Assert(handle.Type != AssetOrObjectReferenceType.AssetName); // Should have been handled by "RightAway" function

        if (handle.Type == AssetOrObjectReferenceType.Asset)
        {
            TAsset? asset = handle.Asset;
            AssertNotNull(asset);
            if (!asset.Loaded)
                yield return asset;

            TObject? obj = asset.GetObject();
            if (obj == null)
                handle = AssetObjectReference<TAsset, TObject>.Invalid;
            else
                handle = obj;
        }
        AttachNewAsset(handle);
    }

    #region Public API

    public Coroutine? GetCurrentLoading()
    {
        if (_deferredSet) return SetInternal();
        return _currentLoadingRoutine;
    }

    public AssetObjectReference<TAsset, TObject> GetCurrentReference()
    {
        return _currentRef;
    }

    public TObject? GetCurrentObject()
    {
        if (!_currentRef.IsValid()) return default;

        Assert(_currentRef.Type == AssetOrObjectReferenceType.Object);
        return _currentRef.AssetObject;
    }

    #endregion

    public void Done()
    {
        _currentLoadingRoutine?.RequestStop();

        _loadingRef = AssetObjectReference<TAsset, TObject>.Invalid;
        _currentlyLoading = false;

        DetachOldAsset(_currentRef);
        _currentRef = AssetObjectReference<TAsset, TObject>.Invalid;

        _onChange = null;
        _onChangeUserData = null;
    }

    #region Changes

    private void AssetHotReloaded(Asset? asset)
    {
        CallOnChange();
    }

    public void SetOnChangeCallback(Action<AssetOwner<TAsset, TObject>, object?> onChangeCallback, object? param1)
    {
        _onChange = onChangeCallback;
        _onChangeUserData = param1;
    }

    protected virtual void CallOnChange()
    {
        _onChange?.Invoke(this, _onChangeUserData);
    }

    #endregion

    #region Attaching

    private void DetachOldAsset(in AssetObjectReference<TAsset, TObject> handle)
    {
        TAsset? asset = handle.Asset;
        asset?.OnLoaded -= AssetHotReloaded;
        RemoveOwnership(handle, this);
    }

    private void AttachNewAsset(in AssetObjectReference<TAsset, TObject> handle)
    {
        AssetObjectReference<TAsset, TObject> old = _currentRef;
        _currentRef = handle;
        DetachOldAsset(old);
        _currentlyLoading = false;

        TAsset? asset = handle.Asset;
        asset?.OnLoaded += AssetHotReloaded;

        AddOwnership(handle, this);
        CallOnChange();
    }

    #endregion

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