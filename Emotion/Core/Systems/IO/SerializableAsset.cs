#nullable enable

namespace Emotion.Core.Systems.IO;

public class SerializableAsset
{
    public string? Name { get; protected set; }
}

public class SerializableAsset<T> : SerializableAsset where T : Asset, new()
{
    public T Get(object? referenceObject = null)
    {
        return Engine.AssetLoader.ONE_Get<T>(Name, referenceObject);
    }

    public static implicit operator SerializableAsset<T>(string? name)
    {
        return new SerializableAsset<T> { Name = name };
    }

    public static implicit operator string?(SerializableAsset<T> asset)
    {
        return asset.Name;
    }

    public static implicit operator SerializableAsset<T>?(T? asset)
    {
        if (asset == null) return null;
        return new SerializableAsset<T> { Name = asset.Name };
    }

    public static implicit operator T(SerializableAsset<T> serialized)
    {
        return serialized.Get();
    }

    public override string ToString()
    {
        return Name ?? "Invalid Asset";
    }
}