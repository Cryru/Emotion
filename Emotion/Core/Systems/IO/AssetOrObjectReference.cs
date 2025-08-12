#nullable enable

using System.Runtime.CompilerServices;

namespace Emotion.Core.Systems.IO;

public enum AssetOrObjectReferenceType
{
    None = 0,
    NoneDeleted,
    Asset,
    AssetName,
    Object
}
public interface IAssetContainingObject<TObject>
{
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
        return _type != AssetOrObjectReferenceType.None && _type != AssetOrObjectReferenceType.NoneDeleted;
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

            _type = AssetOrObjectReferenceType.NoneDeleted;
        }
    }

    public IEnumerator PerformLoading(object? owningObject, Action<object> onChanged)
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
        return _assetObject;
    }
}
