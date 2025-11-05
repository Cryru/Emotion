#nullable enable

using Emotion.Core.Utility.Coroutines;

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
        if (newAsset.ReadyToUse() || !newAsset.IsValid())
        {
            AttachNewAsset(newAsset, true);
            InternalOnChanged();
            return null;
        }

        _currentSetRoutine = Engine.CoroutineManager.StartCoroutine(LoadNewAssetAndAttach(newAsset));
        return _currentSetRoutine;
    }

    private IEnumerator LoadNewAssetAndAttach(AssetObjectReference<TAsset, TObject> newAsset)
    {
        yield return newAsset.Load(this);
        AttachNewAsset(newAsset, false);
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

        Assert(_currentRef.ReadyToUse());
        return _currentRef.GetObject();
    }

    public bool CanBeUsed()
    {
        return _currentRef.ReadyToUse();
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
        TAsset? asset = handle.GetAsset();
        if (asset != null)
            asset.OnLoaded -= AssetHotReloaded;

        handle.RemoveOwnership(this);
    }

    private void AttachNewAsset(AssetObjectReference<TAsset, TObject> handle, bool addOwnership)
    {
        TAsset? asset = handle.GetAsset();
        if (asset != null)
            asset.OnLoaded += AssetHotReloaded;

        if (addOwnership)
            handle.AddOwnership(this);
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
}