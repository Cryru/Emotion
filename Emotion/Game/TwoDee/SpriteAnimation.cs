using Emotion.IO;

namespace Emotion.Game.TwoDee;

#nullable enable

public class SpriteAnimationBodyPart
{
    public int TimeBetweenFrames = 500;
    public List<SpriteAnimationFrame> Frames = new List<SpriteAnimationFrame>();

    public float Duration
    {
        get => Frames.Count * TimeBetweenFrames;
    }
}

public class SpriteAnimation : SpriteAnimationBodyPart
{
    public string Name = "Unnamed Animation";

    public List<SerializableAsset<TextureAsset>> Textures = new List<SerializableAsset<TextureAsset>>();
    public List<SpriteAnimationBodyPart> OtherParts = new List<SpriteAnimationBodyPart>();

    public override string ToString()
    {
        return Name;
    }
}