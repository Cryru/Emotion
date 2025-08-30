#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Core.Systems.IO;

public enum AssetOrObjectReferenceType
{
    None = 0,
    Deleted,
    Asset,
    AssetName,
    Object
}
public interface IAssetContainingObject<TObject>
{
    public bool Finished { get; }

    public TObject? GetObject();
}

public class AssetOrObjectReference<TAsset, TObject>
    where TAsset : Asset, IAssetContainingObject<TObject>, new()
{
    public static AssetOrObjectReference<TAsset, TObject> Invalid = new();

    private AssetOrObjectReferenceType _type;
    private TAsset? _asset;
    private string? _assetName;
    private TObject? _assetObject;

    private object? _owningObject;
    private Action<object>? _onAssetChanged;

    // Implicit conversions
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetOrObjectReference<TAsset, TObject>(TAsset asset)
        => new AssetOrObjectReference<TAsset, TObject> { _type = AssetOrObjectReferenceType.Asset, _asset = asset };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetOrObjectReference<TAsset, TObject>(string assetName)
        => new AssetOrObjectReference<TAsset, TObject> { _type = AssetOrObjectReferenceType.AssetName, _assetName = assetName };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetOrObjectReference<TAsset, TObject>(TObject obj)
        => new AssetOrObjectReference<TAsset, TObject> { _type = AssetOrObjectReferenceType.Object, _assetObject = obj };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsValid()
    {
        return _type != AssetOrObjectReferenceType.None && _type != AssetOrObjectReferenceType.Deleted;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ReadyToUse()
    {
        return _type == AssetOrObjectReferenceType.Object;
    }

    public void Cleanup()
    {
        if (_owningObject != null)
        {
            AssertNotNull(_asset);
            _asset.OnLoaded -= AssetReloaded;

            Engine.AssetLoader.RemoveReferenceFromAsset(_asset, _owningObject);
            _owningObject = null;
            _onAssetChanged = null;

            _type = AssetOrObjectReferenceType.Deleted;
        }
    }

    public IEnumerator PerformLoading(object? owningObject, Action<object>? onChanged)
    {
        if (_type == AssetOrObjectReferenceType.AssetName)
        {
            _asset = Engine.AssetLoader.ONE_Get<TAsset>(_assetName, owningObject);
            _type = AssetOrObjectReferenceType.Asset;
            _owningObject = owningObject;
        }

        if (_type == AssetOrObjectReferenceType.Asset)
        {
            if (_owningObject == null && owningObject != null)
            {
                Engine.AssetLoader.AddReferenceToAsset(_asset, owningObject);
                _owningObject = owningObject;
            }

            AssertNotNull(_asset);
            yield return _asset;

            _onAssetChanged = onChanged;
            _asset.OnLoaded += AssetReloaded;
            _assetName = _asset.Name;
            _assetObject = _asset.GetObject();
            _type = AssetOrObjectReferenceType.Object;
        }
    }

    private void AssetReloaded(Asset obj)
    {
        TAsset assetAsMeshAsset = (TAsset)obj;
        _assetObject = assetAsMeshAsset.GetObject();

        if (_owningObject != null && _onAssetChanged != null)
            _onAssetChanged.Invoke(_owningObject);
    }

    public TObject? GetObject()
    {
        AssertNotNull(_type != AssetOrObjectReferenceType.Deleted);

        if (_type == AssetOrObjectReferenceType.AssetName)
        {
            TAsset asset = Engine.AssetLoader.ONE_Get<TAsset>(_assetName, null, true);
            if (asset.Loaded)
                return asset.GetObject();

        }
        else if (_type == AssetOrObjectReferenceType.Asset)
        {
            AssertNotNull(_asset);
            if (_asset.Loaded)
                return _asset.GetObject();
        }

        // _type == Object
        return _assetObject;
    }

    #region Equality

    // Operators
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(AssetOrObjectReference<TAsset, TObject>? a, AssetOrObjectReference<TAsset, TObject>? b)
    {
        bool aIsNull = a is null;
        bool bIsNull = b is null;
        if (aIsNull || bIsNull)
            return aIsNull && bIsNull;

        if (a._assetName != null && b._assetName != null)
            return a._assetName == b._assetName;

        if (a._asset != null && b._asset != null)
            return a._asset == b._asset;

        if (a._assetObject != null && b._assetObject != null)
            return a._assetObject.Equals(b._assetObject);

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(AssetOrObjectReference<TAsset, TObject>? a, AssetOrObjectReference<TAsset, TObject>? b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        AssetOrObjectReference<TAsset, TObject>? objCast = obj as AssetOrObjectReference<TAsset, TObject>;
        if (objCast != null)
            return objCast == this;

        return false;
    }

    #endregion
}
