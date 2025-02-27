#nullable enable

#region Using

#endregion

namespace Emotion.Game.Animation3D;

public class SkeletonAnimRigNode
{
    public string Name { get; set; } = string.Empty;
    public Matrix4x4 LocalTransform = Matrix4x4.Identity;
    public int ParentIdx = -1;
    //public SkeletonAnimRigNode[] Children = Array.Empty<SkeletonAnimRigNode>();

    // Don't evaluate this node's local transform when applying animation channels.
    public bool DontAnimate;

    // For debugging purposes.
    //private SkeletonAnimRigNode? FindInRig(string name)
    //{
    //    if (Name == name) return this;
    //    if (Children == null) return null;

    //    for (var i = 0; i < Children.Length; i++)
    //    {
    //        SkeletonAnimRigNode child = Children[i];
    //        SkeletonAnimRigNode? found = child.FindInRig(name);
    //        if (found != null) return found;
    //    }

    //    return null;
    //}

    public override string ToString()
    {
        return $"RigBone: {Name}, Parent: {ParentIdx}";
    }
}