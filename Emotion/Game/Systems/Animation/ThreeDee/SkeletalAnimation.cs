#nullable enable

namespace Emotion.Game.Systems.Animation.ThreeDee;

public class SkeletalAnimation
{
    /// <summary>
    /// The animation's name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Duration in milliseconds
    /// </summary>
    public float Duration { get; set; }

    /// <summary>
    /// Animation bone transformation that make up the animation.
    /// </summary>
    public SkeletonAnimChannel?[] AnimChannels { get; set; } = Array.Empty<SkeletonAnimChannel>();

    // todo: cache this
    public SkeletonAnimChannel? GetAnimChannelForRigNode(int nodeIdx)
    {
        if (AnimChannels.Length == 0) return null;
        Assert(nodeIdx < AnimChannels.Length);
        return AnimChannels[nodeIdx];
    }

    public override string ToString()
    {
        return $"Animation {Name} [{Duration}ms]";
    }
}