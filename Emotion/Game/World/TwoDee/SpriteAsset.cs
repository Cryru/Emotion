using Emotion.Core.Systems.IO;

namespace Emotion.Game.World.TwoDee;

#nullable enable

public class SpriteAsset : XMLAsset<SpriteEntity>
{
    public SpriteEntity? Entity => Content;

    protected override IEnumerator Internal_LoadAssetRoutine(ReadOnlyMemory<byte> data)
    {
        yield return base.Internal_LoadAssetRoutine(data);
        if (Content == null) yield break;

        List<SpriteAnimation> anims = Content.Animations;
        foreach (SpriteAnimation anim in anims)
        {
            foreach (SpriteAnimationBodyPart part in anim.Parts)
            {
                foreach (SpriteAnimationFrame frame in part.Frames)
                {
                    LoadAssetDependency(frame.Texture);
                }
            }
        }

        yield return WaitAllDependenciesToLoad();
    }
}
