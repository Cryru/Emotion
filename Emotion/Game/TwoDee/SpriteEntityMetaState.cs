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
    private int _currentFrameIdx;

    private TextureAsset? _currentFrameTexture;
    private SpriteAnimationFrame? _currentFrameInstance;
    private (int, TextureAsset?, SpriteAnimationFrame?)[]? _otherPartsRenderData;
    private int _otherPartsUsed = 0;

    public void UpdateAnimation(SpriteAnimation? animation, float timeStamp)
    {
        _animation = animation;
        _currentFrameIdx = -1;
        _currentFrameTexture = null;
        _otherPartsUsed = 0;

        if (animation == null) return;

        (int, TextureAsset?, SpriteAnimationFrame?) mainAnim = GetDataForPart(animation, timeStamp, animation);
        _currentFrameIdx = mainAnim.Item1;
        if (_currentFrameIdx == -1) return; // huh?

        _currentFrameTexture = mainAnim.Item2;
        _currentFrameInstance = mainAnim.Item3;

        // Make sure we have space for the other parts.
        int otherPartsSpace = _otherPartsRenderData == null ? 0 : _otherPartsRenderData.Length;
        if (otherPartsSpace < animation.OtherParts.Count)
        {
            if (_otherPartsRenderData == null)
                _otherPartsRenderData = new (int, TextureAsset?, SpriteAnimationFrame?)[animation.OtherParts.Count];
            else
                Array.Resize(ref _otherPartsRenderData, animation.OtherParts.Count);
        }
        _otherPartsUsed = animation.OtherParts.Count;

        // Cache data for other parts.
        if (_otherPartsUsed > 0)
        {
            AssertNotNull(_otherPartsRenderData);
            for (int i = 0; i < animation.OtherParts.Count; i++)
            {
                SpriteAnimationBodyPart otherPart = animation.OtherParts[i];
                _otherPartsRenderData[i] = GetDataForPart(animation, timeStamp, otherPart);
            }
        }
    }

    private static (int, TextureAsset?, SpriteAnimationFrame?) GetDataForPart(SpriteAnimation anim, float time, SpriteAnimationBodyPart part)
    {
        float timeInAnim = time % part.Duration;
        float timeBetweenFrames = part.TimeBetweenFrames;

        float currentTime = 0;
        for (int i = 0; i < part.Frames.Count; i++)
        {
            currentTime += timeBetweenFrames;
            if (currentTime > timeInAnim)
            {
                SpriteAnimationFrame frame = part.Frames[i];
                int currentTextureId = frame.TextureId;
                return (i, anim.Textures.SafelyGet(currentTextureId)?.Get(), frame);
            }
        }

        return (-1, null, null);
    }

    public int GetPartCount()
    {
        if (_currentFrameIdx == -1)
            return 0;

        return 1 + _otherPartsUsed;
    }

    public void GetRenderData(int part, out Texture texture, out Rectangle uv, out Vector2 anchorOffset)
    {
        texture = Texture.EmptyWhiteTexture;
        uv = Rectangle.Empty;
        anchorOffset = Vector2.Zero;

        if (_currentFrameTexture != null && _currentFrameTexture.Loaded)
        {
            AssertNotNull(_currentFrameInstance);

            texture = _currentFrameTexture.Texture;
            anchorOffset = -_currentFrameInstance.GetCalculatedAnchor(texture, out uv);
        }

        if (part == 0)
        {
            return;
        }
        else
        {
            AssertNotNull(_otherPartsRenderData);
            (int, TextureAsset?, SpriteAnimationFrame?) otherPartData = _otherPartsRenderData[part - 1];
            TextureAsset? asset = otherPartData.Item2;
            if (otherPartData.Item1 != -1 && asset != null && asset.Loaded)
            {
                AssertNotNull(otherPartData.Item3);

                texture = asset.Texture;
                anchorOffset = anchorOffset - otherPartData.Item3.GetCalculatedAnchor(texture, out uv);
            }
        }

        if (Entity.PixelArt)
            anchorOffset = anchorOffset.Round();
    }
}
