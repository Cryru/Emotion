#nullable enable

namespace Emotion.Game.Systems.Animation.ThreeDee;

public struct SkeletalAnimationSkinJoint
{
    public int RigNodeIdx = -1;
    public Matrix4x4 OffsetMatrix = Matrix4x4.Identity;

    public SkeletalAnimationSkinJoint()
    {

    }
}

public class SkeletalAnimationSkin
{
    public string Name = string.Empty;

    public SkeletalAnimationSkinJoint[] Joints = Array.Empty<SkeletalAnimationSkinJoint>();
}
