#nullable enable

using Emotion.Game.World.TwoDee;
using Emotion.Graphics.Assets;

namespace Emotion.Core.Systems.IO;

public class SpriteEntityAsset : XMLAssetBase<SpriteEntity, SpriteEntityAsset>
{
    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        yield return base.Internal_LoadAssetRoutine(data);
        if (Content == null) yield break;

        foreach (SpriteAnimation anim in Content.Animations)
        {
            foreach (SpriteAnimationBodyPart part in anim.ForEachPart())
            {
                foreach (SpriteAnimationFrame frame in part.Frames)
                {
                    LoadAssetDependency<TextureAsset, Texture>(frame.Texture);
                }
            }
        }
        yield return WaitAllDependenciesToLoad();
    }
}
