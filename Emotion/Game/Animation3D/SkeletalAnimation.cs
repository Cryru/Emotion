#region Using

using System.Numerics;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Game.Animation3D
{
    public class SkeletalAnimation
    {
        /// <summary>
        /// The animation's name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Duration in milliseconds
        /// </summary>
        public float Duration { get; set; }

        /// <summary>
        /// Animation bone transformation that make up the animation.
        /// </summary>
        public SkeletonAnimChannel[] AnimChannels { get; set; }

        /// <summary>
        /// Apply the transformations of this animation to the bone matrices of a particular entity.
        /// </summary>
        /// <param name="entity">The entity the bone matrices are to be filled for.</param>
        /// <param name="boneMatrices">Array of matrices. Indexed relative to bone indices in the entity.</param>
        /// <param name="timeStamp">The current animation time 0-Duration</param>
        public void ApplyBoneMatrices(MeshEntity entity, Matrix4x4[] boneMatrices, float timeStamp)
        {
            ApplyBoneMatricesInternal(entity.AnimationRig, entity, boneMatrices, timeStamp, Matrix4x4.Identity);
        }

        private void ApplyBoneMatricesInternal(SkeletonAnimRigNode node, MeshEntity entity, Matrix4x4[] boneMatrices, float timeStamp, Matrix4x4 parentTransform)
        {
            string name = node.Name;
            Matrix4x4 nodeTransform = node.LocalTransform;

            SkeletonAnimChannel animChannel = GetMeshAnimBone(name);
            if (animChannel != null) nodeTransform = animChannel.GetMatrixAtTimestamp(timeStamp);

            Matrix4x4 globalTransform = nodeTransform * parentTransform;

            MeshBone bone = BoneIndexFromEntity(entity, name);
            if (bone != null) boneMatrices[bone.BoneIndex] = bone.OffsetMatrix * globalTransform;

            if (node.Children == null) return;
            for (var i = 0; i < node.Children.Length; i++)
            {
                SkeletonAnimRigNode child = node.Children[i];
                ApplyBoneMatricesInternal(child, entity, boneMatrices, timeStamp, globalTransform);
            }
        }

        private SkeletonAnimChannel GetMeshAnimBone(string name)
        {
            for (var i = 0; i < AnimChannels.Length; i++)
            {
                SkeletonAnimChannel channels = AnimChannels[i];
                if (channels.Name == name) return channels;
            }

            return null;
        }

        private MeshBone BoneIndexFromEntity(MeshEntity entity, string boneName)
        {
            for (var i = 0; i < entity.Meshes.Length; i++)
            {
                Mesh mesh = entity.Meshes[i];
                if (mesh.Bones == null) continue;
                for (var j = 0; j < mesh.Bones.Length; j++)
                {
                    MeshBone bone = mesh.Bones[j];
                    if (bone.Name == boneName) return bone;
                }
            }

            return null;
        }

        // For debugging purposes.
        private SkeletonAnimRigNode FindInRig(SkeletonAnimRigNode node, string name)
        {
            if (node.Name == name) return node;

            for (var i = 0; i < node.Children.Length; i++)
            {
                SkeletonAnimRigNode child = node.Children[i];
                SkeletonAnimRigNode found = FindInRig(child, name);
                if (found != null) return found;
            }

            return null;
        }
    }
}