using Emotion.IO;

namespace Emotion.Game.TwoDee;

public class SpriteAnimation
{
    public string Name = "Unnamed Animation";

    public int TimeBetweenFrames = 500;
    public List<SerializableAsset<TextureAsset>> Textures = new List<SerializableAsset<TextureAsset>>();
    public List<SpriteAnimationFrame> Frames = new List<SpriteAnimationFrame>();

    public override string ToString()
    {
        return Name;
    }
}