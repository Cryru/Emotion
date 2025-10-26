#nullable enable

using Emotion.Game.Systems.Animation.TwoDee;
using Emotion.Graphics.Assets;
using PartRuntimeData = (Emotion.Game.World.TwoDee.SpriteAnimationBodyPart part, int relativeTo, Emotion.Game.World.TwoDee.SpriteAnimationFrame? currentFrame, System.Numerics.Vector2 anchor);

namespace Emotion.Game.World.TwoDee;

public class SpriteEntityMetaState
{
    public SpriteEntity Entity { get; init; }

    public SpriteEntityMetaState(SpriteEntity entity)
    {
        Entity = entity;
    }

    private SpriteAnimation? _animation;
    private float _totalAnimDuration;
    private float _totalAnimDurationSegment;
    private Dictionary<int, Vector2> _points = new();
    private List<PartRuntimeData> _parts = new();

    public float GetCurrentAnimationTime()
    {
        return _totalAnimDurationSegment;
    }

    public void SetAnimation(string animName, bool forceRecalculate = false)
    {
        SpriteAnimation? animation = Entity.GetAnimation(animName, true);
        if (_animation == animation && !forceRecalculate)
            return;

        _animation = animation;
        _parts.Clear();
        _points.Clear();
        _totalAnimDuration = 1;
        _totalAnimDurationSegment = 1;

        if (_animation != null)
        {
            // Initialize animation runtime cache
            foreach (SpriteAnimationBodyPart part in _animation.ForEachPart())
            {
                int pointAttachIdx = -1;
                string pointAttach = part.AttachToPoint;
                if (pointAttach != "origin")
                    pointAttachIdx = pointAttach.GetStableHashCode();

                _parts.Add((part, pointAttachIdx, null, Vector2.Zero));
            }

            _totalAnimDuration = _animation.TotalDuration;
            _totalAnimDurationSegment = _totalAnimDuration;

            if (_animation.LoopType == AnimationLoopType.NormalThenReverse)
                // -2 frames since we dont want to repeat first and last when switching directions
                _totalAnimDuration = _totalAnimDuration * 2f - _animation.TimeBetweenFrames * 2f;
        }
    }

    public float UpdateAnimation(float currentTime)
    {
        if (_animation == null)
            return currentTime;

        float normalizedTime = currentTime % _totalAnimDuration;
        float currentAnimTime = normalizedTime;

        // NormalThenReverse handling
        if (currentAnimTime >= _totalAnimDurationSegment)
            currentAnimTime = _totalAnimDurationSegment - (currentAnimTime - _totalAnimDurationSegment + _animation.TimeBetweenFrames + 1);

        // Assign current frames for all parts
        for (int i = 0; i < _parts.Count; i++)
        {
            PartRuntimeData item = _parts[i];

            SpriteAnimationFrame? frame = GetFrameAtTimestamp(_animation, item.part, currentAnimTime);
            item.currentFrame = frame;

            if (frame != null)
            {
                item.anchor = frame.GetCalculatedOrigin(item.part);

                //bool flipX = false;
                //if (Entity.PixelArt)
                //    if (flipX)
                //        item.anchor = item.anchor.Ceiling();
                //    else
                //        item.anchor = item.anchor.Floor();
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

        return normalizedTime;
    }

    private SpriteAnimationFrame? GetFrameAtTimestamp(SpriteAnimation animation, SpriteAnimationBodyPart part, float time)
    {
        if (part.Frames.Count == 0)
            return null;

        float timeBetweenFrames = animation.TimeBetweenFrames;
        if (timeBetweenFrames == 0)
            return part.Frames[0];

        int currentFrame = (int)(time / animation.TimeBetweenFrames);
        int frameIdx = currentFrame % part.Frames.Count;
        return part.Frames[frameIdx];
    }

    public int GetPartCount()
    {
        return _parts.Count;
    }

    public bool GetRenderData(int partIdx, out Texture texture, out Rectangle uv, out Vector2 anchorOffset)
    {
        texture = Texture.EmptyWhiteTexture;
        uv = Rectangle.Empty;
        anchorOffset = Vector2.Zero;

        PartRuntimeData partData = _parts[partIdx];
        if (!partData.part.Visible || partData.currentFrame == null)
            return false;

        var partTexture = partData.currentFrame.Texture.GetObject();
        if (partTexture == null)
            return false;

        texture = partTexture;
        anchorOffset = partData.anchor;
        uv = partData.currentFrame.UV.IsEmpty ? new Rectangle(0, 0, texture.Size) : partData.currentFrame.UV;
        return true;
    }
}
