#nullable enable

using Emotion.IO;
using PartRuntimeData = (Emotion.Game.TwoDee.SpriteAnimationBodyPart part, int relativeTo, Emotion.Game.TwoDee.SpriteAnimationFrame? currentFrame, System.Numerics.Vector2 anchor);

namespace Emotion.Game.TwoDee;

public class SpriteEntityMetaState
{
    public SpriteEntity Entity { get; init; }

    public SpriteEntityMetaState(SpriteEntity entity)
    {
        Entity = entity;
    }

    private SpriteAnimation? _animation;
    private Dictionary<int, Vector2> _points = new();
    private List<PartRuntimeData> _parts = new();

    public void UpdateAnimation(SpriteAnimation? animation, float timeStamp)
    {
        // Check if changing animation
        if (_animation != animation)
        {
            _animation = animation;
            _parts.Clear();
            _points.Clear();

            if (animation != null)
            {
                // Initialize animation runtime cache
                foreach (SpriteAnimationBodyPart part in animation.ForEachPart())
                {
                    int pointAttachIdx = -1;
                    string pointAttach = part.AttachToPoint;
                    if (pointAttach != "origin")
                        pointAttachIdx = pointAttach.GetStableHashCode();

                    _parts.Add((part, pointAttachIdx, null, Vector2.Zero));
                }
            }
        }
        if (_animation == null)
            return;

        // Assign current frames for all parts
        for (int i = 0; i < _parts.Count; i++)
        {
            PartRuntimeData item = _parts[i];

            SpriteAnimationFrame? frame = GetFrameAtTimestamp(item.part, timeStamp);
            item.currentFrame = frame;

            if (frame != null)
            {
                item.anchor = frame.GetCalculatedOrigin(item.part);
                if (Entity.PixelArt)
                    item.anchor = item.anchor.Round();

                // Check if relative to point
                if (item.relativeTo != -1 && _points.TryGetValue(item.relativeTo, out Vector2 relativeToOffset))
                    item.anchor += relativeToOffset;

                // Set points from this frame
                foreach (SpriteAnimationFramePoint point in frame.Points)
                {
                    int pointId = point.Name.GetStableHashCode();
                    Vector2 pointOffset = point.OriginOffset;
                    if (point.RelativeToPartOrigin)
                        pointOffset += item.anchor;
                    _points[pointId] = pointOffset;
                }
            }
            else
            {
                item.anchor = Vector2.Zero;
            }

            _parts[i] = item; // smh value types
        }
    }

    private SpriteAnimationFrame? GetFrameAtTimestamp(SpriteAnimationBodyPart part, float time)
    {
        if (part.Frames.Count == 0)
            return null;

        float timeBetweenFrames = part.TimeBetweenFrames;
        if (timeBetweenFrames == 0)
            return part.Frames[0];

        float timeInAnim = time % part.Duration;
        float currentTime = 0;
        for (int i = 0; i < part.Frames.Count; i++)
        {
            currentTime += timeBetweenFrames;
            if (currentTime > timeInAnim)
            {
                SpriteAnimationFrame frame = part.Frames[i];
                return frame;
            }
        }

        return null;
    }

    public int GetPartCount()
    {
        return _parts.Count;
    }

    public void GetRenderData(int partIdx, out Texture texture, out Rectangle uv, out Vector2 anchorOffset)
    {
        texture = Texture.EmptyWhiteTexture;
        uv = Rectangle.Empty;
        anchorOffset = Vector2.Zero;

        PartRuntimeData partData = _parts[partIdx];
        if (!partData.part.Visible || partData.currentFrame == null)
            return;

        TextureAsset textureAsset = partData.currentFrame.Texture.Get();
        if(textureAsset.Loaded)
            texture = textureAsset.Texture;

        anchorOffset = partData.anchor;
        uv = partData.currentFrame.UV.IsEmpty ? new Primitives.Rectangle(0, 0,texture.Size) : partData.currentFrame.UV;
    }
}
