#nullable enable

using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
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

public struct AssetObjectReference<TAsset, TObject> : ICustomReflectorMeta_ExtraMembers
    where TAsset : Asset, IAssetContainingObject<TObject>, new()
{
    public static AssetObjectReference<TAsset, TObject> Invalid { get; } = new();

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
            _asset = Engine.AssetLoader.Get<TAsset>(_assetName, addOwner, false, loadedAsDependency);
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
            TAsset asset = Engine.AssetLoader.Get<TAsset>(_assetName, null, true);
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

        var aVal = a.Value;
        var bVal = b.Value;

        if (aVal._assetName != null && bVal._assetName != null)
            return aVal._assetName == bVal._assetName;

        if (aVal._asset != null && bVal._asset != null)
            return aVal._asset == bVal._asset;

        if (aVal._assetObject != null && bVal._assetObject != null)
            return aVal._assetObject.Equals(bVal._assetObject);

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(AssetObjectReference<TAsset, TObject>? a, AssetObjectReference<TAsset, TObject>? b)
    {
        return !(a == b);
    }

    public override readonly bool Equals(object? obj)
    {
        if (obj == null)
            return false;

        var objCast = obj as AssetObjectReference<TAsset, TObject>?;
        return objCast == this;
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

    #region Reflector

    public static ComplexTypeHandlerMemberBase[] GetExtraReflectorMembers()
    {
        return [
            new ComplexTypeHandlerMember<AssetObjectReference<TAsset, TObject>, string>(
                "AssetName",
                static (ref AssetObjectReference<TAsset, TObject> p, string? v) =>
                {
                    p._assetName = v;
                    p._type = AssetOrObjectReferenceType.AssetName;
                },
                static (p) => p._assetName
            )
        ];
    }

    #endregion
}
