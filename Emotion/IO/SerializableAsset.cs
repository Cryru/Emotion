#nullable enable

namespace Emotion.IO;

public class SerializableAsset<T> where T : Asset, new()
{
    public string? Name { get; set; }

    public T? Get(object? referenceObject = null)
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

    public static implicit operator T?(SerializableAsset<T> serialized)
    {
        return serialized.Get();
    }
}