#region Using

using System.Numerics;

#endregion

namespace Emotion.Game.Animation3D
{
    public class SkeletonAnimRigNode
    {
        public string Name { get; set; }
        public Matrix4x4 LocalTransform;
        public SkeletonAnimRigNode[] Children;

        public override string ToString()
        {
            return $"Bone: {Name}, Children: {Children?.Length}";
        }
    }
}