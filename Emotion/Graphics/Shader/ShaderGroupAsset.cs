#nullable enable

using Emotion.Core.Systems.IO;

namespace Emotion.Graphics.Shader;

public class ShaderGroupAsset : TextAsset, IAssetContainingObject<ShaderGroup>
{
    public ShaderGroup? ShaderGroup = null;

    public ShaderGroupAsset()
    {
        _useNewLoading = true;
    }

    public ShaderGroup? GetObject()
    {
        return ShaderGroup;
    }

    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        base.CreateInternal(data);

        ShaderGroup?.Dispose(); // Hot reloading
        ShaderGroup = new ShaderGroup(Name, base.Content);
        yield return ShaderGroup.Init();
    }
}