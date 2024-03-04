#region Using

using Emotion.Utility;

#endregion

namespace Emotion.Game.Animation3D
{
    public struct MeshAnimBoneTranslation
    {
        public float Timestamp;
        public Vector3 Position;
    }

    public struct MeshAnimBoneRotation
    {
        public float Timestamp;
        public Quaternion Rotation;
    }

    public struct MeshAnimBoneScale
    {
        public float Timestamp;
        public Vector3 Scale;
    }

    public class SkeletonAnimChannel
    {
        public string Name { get; set; }
        public MeshAnimBoneTranslation[] Positions;
        public MeshAnimBoneRotation[] Rotations;
        public MeshAnimBoneScale[] Scales;

        public Matrix4x4 GetMatrixAtTimestamp(float timestamp)
        {
            Matrix4x4 translate = GetInterpolatedPosition(timestamp);
            Matrix4x4 rotate = GetInterpolatedRotation(timestamp);
            Matrix4x4 scale = GetInterpolatedScale(timestamp);
            return scale * rotate * translate;
        }

        private int GetPositionAt(ref float timestamp)
        {
            for (var i = 0; i < Positions.Length; i++)
            {
                ref MeshAnimBoneTranslation key = ref Positions[i];
                if (key.Timestamp >= timestamp) return i == 0 ? 0 : i - 1;
            }

            ref MeshAnimBoneTranslation lastKey = ref Positions[^1];
            timestamp = lastKey.Timestamp;
            return Positions.Length - 2;
        }

        private int GetRotationAt(ref float timestamp)
        {
            for (var i = 0; i < Rotations.Length; i++)
            {
                ref MeshAnimBoneRotation key = ref Rotations[i];
                if (key.Timestamp >= timestamp) return i == 0 ? 0 : i - 1;
            }

            ref MeshAnimBoneRotation lastKey = ref Rotations[^1];
            timestamp = lastKey.Timestamp;
            return Rotations.Length - 2;
        }

        private int GetScaleAt(ref float timestamp)
        {
            for (var i = 0; i < Scales.Length; i++)
            {
                ref MeshAnimBoneScale key = ref Scales[i];
                if (key.Timestamp >= timestamp) return i == 0 ? 0 : i - 1;
            }

            ref MeshAnimBoneScale lastKey = ref Scales[^1];
            timestamp = lastKey.Timestamp;
            return Rotations.Length - 2;
        }

        private Matrix4x4 GetInterpolatedPosition(float timestamp)
        {
            if (Positions.Length == 0) return Matrix4x4.Identity;
            if (Positions.Length == 1) return Matrix4x4.CreateTranslation(Positions[0].Position);

            int currentIndex = GetPositionAt(ref timestamp);
            int nextIndex = currentIndex + 1;
            float t = Maths.FastInverseLerp(Positions[currentIndex].Timestamp, Positions[nextIndex].Timestamp, timestamp);
            Assert(t is >= 0.0f and <= 1.0f);
            Vector3 pos = Vector3.Lerp(Positions[currentIndex].Position, Positions[nextIndex].Position, t);
            return Matrix4x4.CreateTranslation(pos);
        }

        private Matrix4x4 GetInterpolatedRotation(float timestamp)
        {
            if (Rotations.Length == 0) return Matrix4x4.Identity;
            if (Rotations.Length == 1) return Matrix4x4.CreateFromQuaternion(Quaternion.Normalize(Rotations[0].Rotation));

            int currentIndex = GetRotationAt(ref timestamp);
            int nextIndex = currentIndex + 1;
            float t = Maths.FastInverseLerp(Rotations[currentIndex].Timestamp, Rotations[nextIndex].Timestamp, timestamp);
            Assert(t is >= 0.0f and <= 1.0f);
            Quaternion rotation = Quaternion.Slerp(Rotations[currentIndex].Rotation, Rotations[nextIndex].Rotation, t);
            rotation = Quaternion.Normalize(rotation);
            return Matrix4x4.CreateFromQuaternion(rotation);
        }

        private Matrix4x4 GetInterpolatedScale(float timestamp)
        {
            if (Scales.Length == 0) return Matrix4x4.Identity;
            if (Scales.Length == 1) return Matrix4x4.CreateScale(Scales[0].Scale);

            int currentIndex = GetScaleAt(ref timestamp);
            int nextIndex = currentIndex + 1;
            float t = Maths.FastInverseLerp(Scales[currentIndex].Timestamp, Scales[nextIndex].Timestamp, timestamp);
            Assert(t is >= 0.0f and <= 1.0f);
            Vector3 scale = Vector3.Lerp(Scales[currentIndex].Scale, Scales[nextIndex].Scale, t);
            return Matrix4x4.CreateScale(scale);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}