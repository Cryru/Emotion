#nullable enable


namespace Emotion.Core.Systems.IO;

/// <summary>
/// A generic asset of an unspecified type, accessed as raw data.
/// </summary>
public class OtherAsset : Asset
{
    public ReadOnlyMemory<byte> Content { get; private set; }

    public OtherAsset()
    {
        _useNewLoading = true;
    }

    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        Content = data;
        return base.Internal_LoadAssetRoutine(data);
    }

    protected override void CreateInternal(ReadOnlyMemory<byte> data)
    {
        Content = data;
    }

    protected override void DisposeInternal()
    {
        Content = null;
    }
}