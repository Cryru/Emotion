using Emotion.IO;

namespace Emotion.Game.TwoDee;

#nullable enable

public class SpriteAsset : XMLAsset<SpriteEntity>
{
    public SpriteEntity? Entity => Content;
}
