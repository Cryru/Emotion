#nullable enable

namespace Emotion.Game.Systems.Animation.TwoDee;

/// <summary>
/// Contains the information needed to render and animate a sprite.
/// This class is initialized and managed by the animation editor.
/// </summary>
public class AnimatedSprite
{
    public string AssetFile;
    public SpriteAnimationFrameSource FrameSource;
    public Dictionary<string, SpriteAnimationData> Animations;

    public AnimatedSprite(string textureAssetName, SpriteAnimationFrameSource frameSource)
    {
        AssetFile = textureAssetName;
        FrameSource = frameSource;
    }

    // serialization
    protected AnimatedSprite()
    {

    }
}