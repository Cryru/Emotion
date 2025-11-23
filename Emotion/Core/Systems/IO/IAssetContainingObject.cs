#nullable enable

namespace Emotion.Core.Systems.IO;

public interface IAssetContainingObject<TObject>
{
    public bool Finished { get; }

    public TObject? GetObject();
}