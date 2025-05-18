using Emotion.Common.Serialization;
using Emotion.Game.Animation3D;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

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

