namespace Emotion.Game.Animation3D
{
    public class MeshBone
    {
        public string Name { get; set; }
        public int BoneIndex;
        public Matrix4x4 OffsetMatrix;

        public override string ToString()
        {
            return $"{BoneIndex} {Name}";
        }
    }
}