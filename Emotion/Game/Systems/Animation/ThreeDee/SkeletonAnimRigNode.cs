#nullable enable

namespace Emotion.Game.Systems.Animation.ThreeDee;

public class SkeletonAnimRigNode
{
    public string Name { get; set; } = string.Empty;
    public Matrix4x4 LocalTransform = Matrix4x4.Identity;
    public int ParentIdx = -1;

    // Don't evaluate this node's local transform when applying animation channels.
    public bool DontAnimate;

    public override string ToString()
    {
        return $"RigBone: {Name}, Parent: {ParentIdx}";
    }
}