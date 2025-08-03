#nullable enable

namespace Emotion.Core.Systems.IO.AssetPack;

public class AssetBlob
{
    public string Name;
    public int Index;
    public Dictionary<string, BlobFile> BlobMeta = new Dictionary<string, BlobFile>();
}