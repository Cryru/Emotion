#nullable enable

using Emotion.IO;

namespace Emotion.Game.TwoDee;

public class SpriteEntityMetaState
{
    public SpriteEntity Entity { get; init; }

    public SpriteEntityMetaState(SpriteEntity entity)
    {
        Entity = entity;

        // todo entity should load textures as dependant assets and should have some sort of map so
        // we don't have to go through the AssetLoader all the time?
    }

    private SpriteAnimation? _animation;
    private float _animationTimestamp;
    private int _currentFrameIdx;

    private TextureAsset? _currentFrameTexture;
    private SpriteAnimationFrame? _currentFrameInstance;

    public void UpdateAnimation(SpriteAnimation? animation, float timeStamp)
    {
        _animation = animation;
        _animationTimestamp = timeStamp;
        _currentFrameIdx = -1;
        _currentFrameTexture = null;

        if (animation == null) return;

        float timeBetweenFrames = animation.TimeBetweenFrames;
        float currentTime = 0;
        for (int i = 0; i < animation.Frames.Count; i++)
        {
            currentTime += timeBetweenFrames;
            if (currentTime > _animationTimestamp)
            {
                _currentFrameIdx = i;

                SpriteAnimationFrame frame = animation.Frames[i];
                int currentTextureId = frame.TextureId;
                if (currentTextureId < animation.Textures.Count)
                {
                    _currentFrameTexture = animation.Textures[currentTextureId].Get();
                    _currentFrameInstance = frame;
                }
                break;
            }
        }
    }

    public void GetRenderData(out Texture texture, out Rectangle uv, out Vector2 anchorOffset)
    {
        texture = Texture.EmptyWhiteTexture;
        uv = Rectangle.Empty;
        anchorOffset = Vector2.Zero;
        if (_currentFrameTexture != null && _currentFrameTexture.Loaded)
        {
            AssertNotNull(_currentFrameInstance);

            texture = _currentFrameTexture.Texture;
            anchorOffset = -_currentFrameInstance.GetCalculatedAnchor(texture, out uv);
            if (Entity.PixelArt)
                anchorOffset = anchorOffset.Round();
        }
    }
}
