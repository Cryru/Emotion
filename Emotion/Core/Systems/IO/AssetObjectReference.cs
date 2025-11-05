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

public partial class AssetOrObjectReferenceSerialization
{
    [SerializeNonPublicGetSet]
    public virtual string? Name { get; protected set; }
}


public sealed class AssetObjectReference<TAsset, TObject> : AssetOrObjectReferenceSerialization
    where TAsset : Asset, IAssetContainingObject<TObject>, new()
{
    public static AssetObjectReference<TAsset, TObject> Invalid { get; } = new();

    public override string? Name
    {
        get => _assetName;

        // Create via deserialization
        protected set
        {
            _assetName = value;
            _type = AssetOrObjectReferenceType.AssetName;
        }
    }

    private AssetOrObjectReferenceType _type;
    private TAsset? _asset;
    private string? _assetName;
    private TObject? _assetObject;

    // Implicit conversions
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetObjectReference<TAsset, TObject>(TAsset asset)
        => new AssetObjectReference<TAsset, TObject> { _type = AssetOrObjectReferenceType.Asset, _asset = asset };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetObjectReference<TAsset, TObject>(string assetName)
        => new AssetObjectReference<TAsset, TObject> { _type = AssetOrObjectReferenceType.AssetName, _assetName = assetName };

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator AssetObjectReference<TAsset, TObject>(TObject obj)
        => new AssetObjectReference<TAsset, TObject> { _type = AssetOrObjectReferenceType.Object, _assetObject = obj };

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

    public IEnumerator Load(object? addOwner = null, bool loadedAsDependency = false)
    {
        bool addedOwner = false;

        if (_type == AssetOrObjectReferenceType.AssetName)
        {
            _asset = Engine.AssetLoader.ONE_Get<TAsset>(_assetName, addOwner, false, loadedAsDependency);
            _type = AssetOrObjectReferenceType.Asset;
            addedOwner = true;
        }

        if (_type == AssetOrObjectReferenceType.Asset)
        {
            AssertNotNull(_asset);
            if (!_asset.Loaded)
                yield return _asset;
            _assetName = _asset.Name;
            _assetObject = _asset.GetObject();
            _type = AssetOrObjectReferenceType.Object;
        }

        if (!addedOwner && addOwner != null)
            AddOwnership(addOwner);
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

    public TAsset? GetAsset()
    {
        return _type == AssetOrObjectReferenceType.Asset ? _asset : null;
    }

    #region Ownership

    public void AddOwnership(object obj)
    {
        if (!IsValid()) return;
        Assert(ReadyToUse()); // Must be loaded!
        Engine.AssetLoader.AddReferenceToAsset(_asset, obj);
    }

    public void RemoveOwnership<T>(T obj) where T : notnull
    {
        if (!IsValid()) return;
        Assert(ReadyToUse()); // Must be loaded!
        Engine.AssetLoader.RemoveReferenceFromAsset(_asset, obj);
    }

    #endregion

    #region Equality

    // Operators
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(AssetObjectReference<TAsset, TObject>? a, AssetObjectReference<TAsset, TObject>? b)
    {
        bool aIsNull = a is null;
        bool bIsNull = b is null;
        if (aIsNull || bIsNull)
            return aIsNull && bIsNull;

        AssertNotNull(a);
        AssertNotNull(b);

        if (a._assetName != null && b._assetName != null)
            return a._assetName == b._assetName;

        if (a._asset != null && b._asset != null)
            return a._asset == b._asset;

        if (a._assetObject != null && b._assetObject != null)
            return a._assetObject.Equals(b._assetObject);

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(AssetObjectReference<TAsset, TObject>? a, AssetObjectReference<TAsset, TObject>? b)
    {
        return !(a == b);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        if (ReferenceEquals(this, obj))
            return true;

        AssetObjectReference<TAsset, TObject>? objCast = obj as AssetObjectReference<TAsset, TObject>;
        if (objCast != null)
            return objCast == this;

        return false;
    }

    public override int GetHashCode()
    {
        switch (_type)
        {
            case AssetOrObjectReferenceType.AssetName when _assetName != null:
                return _assetName.GetHashCode();
            case AssetOrObjectReferenceType.Asset when _asset != null:
                return _asset.GetHashCode();
            case AssetOrObjectReferenceType.Object when _assetObject != null:
                return _assetObject.GetHashCode();

        }
        return 0;
    }

    #endregion

    public override string? ToString()
    {
        if (_assetName != null) return _assetName;
        if (_assetObject != null) return _assetObject.ToString();
        return "Invalid Asset/Object Reference";
    }
}
