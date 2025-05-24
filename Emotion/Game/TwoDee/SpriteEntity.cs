#nullable enable

namespace Emotion.Game.TwoDee;

public class SpriteEntity
{
    public static SpriteEntity MissingEntity = new SpriteEntity()
    {
        Name = "Missing Entity"
    };

    public string Name = "Unnamed Entity";
    public bool PixelArt;

    //public List<SerializableAsset<TextureAsset>> Textures = new();
    public List<SpriteAnimation> Animations = new();
}

