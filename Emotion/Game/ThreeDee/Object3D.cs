#region Using

using System;
using System.Numerics;
using Emotion.Common;
using Emotion.Game.Animation3D;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.ThreeDee;
using Emotion.Primitives;
using OpenGL;

#endregion

namespace Emotion.Game.ThreeDee
{
    public class Object3D : Transform3D, IRenderable
    {
        public MeshEntity Entity
        {
            get => _entity;
            set
            {
                _entity = value;
                _totalNumberOfBones = -1;
                SetAnimation(null);
            }
        }

        private MeshEntity _entity;

        public string CurrentAnimation
        {
            get => _currentAnimation?.Name;
        }

        private SkeletalAnimation _currentAnimation;
        private float _time;
        private int _totalNumberOfBones = -1;
        private Matrix4x4[] _boneMatrices;

        public void SetAnimation(string name)
        {
            SkeletalAnimation animInstance = null;
            if (Entity.Animations != null)
            {
                for (var i = 0; i < Entity.Animations.Length; i++)
                {
                    SkeletalAnimation anim = Entity.Animations[i];
                    if (anim.Name == name) animInstance = anim;
                }
            }
            _currentAnimation = animInstance;
            _time = 0;

            // Cache some info.
            if (_totalNumberOfBones == -1)
            {
                for (var i = 0; i < Entity.Meshes.Length; i++)
                {
                    Mesh mesh = Entity.Meshes[i];
                    if (mesh.Bones != null) _totalNumberOfBones += mesh.Bones.Length;
                }

                _totalNumberOfBones++; // Zero index contains identity.
                _totalNumberOfBones++; // Convert to zero indexed.
                if (_boneMatrices == null)
                    _boneMatrices = new Matrix4x4[_totalNumberOfBones];
                else if (_totalNumberOfBones > _boneMatrices.Length) Array.Resize(ref _boneMatrices, _totalNumberOfBones);

                // Must be the same equal or less than constant in the SkeletalAnim.vert
                if (_totalNumberOfBones > 126) Engine.Log.Error($"Entity {Entity.Name} has more bones in all its meshes combined than allowed.", "3D");
            }

            Matrix4x4 defaultMatrix = Matrix4x4.Identity;
            if (Entity.AnimationRig != null) defaultMatrix = Entity.AnimationRig.LocalTransform;
            for (var i = 0; i < _boneMatrices.Length; i++)
            {
                _boneMatrices[i] = defaultMatrix;
            }
        }

        public void Update(float dt)
        {
            _time += dt;
            _currentAnimation?.ApplyBoneMatrices(Entity, _boneMatrices, _time % _currentAnimation.Duration);
        }

        public void Render(RenderComposer c)
        {
            // Render using the render stream.
            // todo: larger meshes should create their own data buffers.
            // todo: culling state.
            if (Entity?.Meshes == null) return;

            c.FlushRenderStream();

            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(CullFaceMode.Back);
            Gl.FrontFace(FrontFaceDirection.Ccw);
            
            c.PushModelMatrix(_scaleMatrix * _rotationMatrix * _translationMatrix);

            Mesh[] meshes = Entity.Meshes;

            // Assume that if the first mesh is boned, all are.
            if (meshes.Length > 0 && meshes[0].VerticesWithBones != null)
                RenderBonesVerticesCompat(c, meshes);
            else
                RenderMesh(c, meshes);

            c.PopModelMatrix();

            c.FlushRenderStream();
            Gl.Disable(EnableCap.CullFace);
        }

        /// <summary>
        /// Render the mesh using the default render stream.
        /// </summary>
        private void RenderMesh(RenderComposer c, Mesh[] meshes)
        {
            for (var i = 0; i < meshes.Length; i++)
            {
                Mesh obj = meshes[i];
                VertexData[] vertData = obj.Vertices;
                ushort[] indices = obj.Indices;
                Texture texture = null;
                if (obj.Material.DiffuseTexture != null) texture = obj.Material.DiffuseTexture;
                RenderStreamBatch<VertexData>.StreamData memory = c.RenderStream.GetStreamMemory((uint)vertData!.Length, (uint)indices.Length, BatchMode.SequentialTriangles, texture);

                vertData.CopyTo(memory.VerticesData);
                indices.CopyTo(memory.IndicesData);

                ushort structOffset = memory.StructIndex;
                for (var j = 0; j < memory.IndicesData.Length; j++)
                {
                    memory.IndicesData[j] = (ushort)(memory.IndicesData[j] + structOffset);
                }
            }
        }

        /// <summary>
        /// Renders a mesh with bone vertices as a non-animated mesh. Allows the mesh to
        /// be drawn using the default render stream.
        /// </summary>
        private void RenderBonesVerticesCompat(RenderComposer c, Mesh[] meshes)
        {
            for (var i = 0; i < meshes.Length; i++)
            {
                Mesh obj = meshes[i];
                VertexDataWithBones[] vertData = obj.VerticesWithBones;
                ushort[] indices = obj.Indices;
                Texture texture = null;
                if (obj.Material.DiffuseTexture != null) texture = obj.Material.DiffuseTexture;
                RenderStreamBatch<VertexData>.StreamData memory = c.RenderStream.GetStreamMemory((uint) vertData!.Length, (uint) indices.Length, BatchMode.SequentialTriangles, texture);

                // Copy the part of the vertices that dont contain bone data.
                for (var j = 0; j < vertData.Length; j++)
                {
                    ref VertexDataWithBones vertSrc = ref vertData[j];
                    ref VertexData vertDst = ref memory.VerticesData[j];

                    vertDst.Vertex = vertSrc.Vertex;
                    vertDst.UV = vertSrc.UV;
                    vertDst.Color = Color.White.ToUint();
                }

                indices.CopyTo(memory.IndicesData);

                ushort structOffset = memory.StructIndex;
                for (var j = 0; j < memory.IndicesData.Length; j++)
                {
                    memory.IndicesData[j] = (ushort) (memory.IndicesData[j] + structOffset);
                }
            }
        }

        /// <summary>
        /// Render the mesh animated.
        /// The normal render stream cannot be used to do so, so one must be passed in.
        /// Also requires the SkeletanAnim shader or one that supports skinned meshes.
        /// </summary>
        public void RenderAnimated(RenderComposer c, RenderStreamBatch<VertexDataWithBones> bonedStream)
        {
            if (Entity?.Meshes == null) return;
            if (_boneMatrices == null) SetAnimation(null);

            c.FlushRenderStream();

            Gl.Enable(EnableCap.CullFace);
            Gl.CullFace(CullFaceMode.Back);
            Gl.FrontFace(FrontFaceDirection.Ccw);

            c.PushModelMatrix(_scaleMatrix * _rotationMatrix * _translationMatrix);
            c.CurrentState.Shader.SetUniformMatrix4("finalBonesMatrices", _boneMatrices, _boneMatrices.Length);

            Mesh[] meshes = Entity.Meshes;

            for (var i = 0; i < meshes.Length; i++)
            {
                Mesh obj = meshes[i];
                VertexDataWithBones[] vertData = obj.VerticesWithBones;
                ushort[] indices = obj.Indices;
                Texture texture = null;
                if (obj.Material.DiffuseTexture != null) texture = obj.Material.DiffuseTexture;
                RenderStreamBatch<VertexDataWithBones>.StreamData memory = bonedStream.GetStreamMemory((uint) vertData!.Length, (uint) indices.Length, BatchMode.SequentialTriangles, texture);

                // Didn't manage to get enough memory.
                if (memory.VerticesData.Length == 0) continue;

                vertData.CopyTo(memory.VerticesData);
                indices.CopyTo(memory.IndicesData);

                ushort structOffset = memory.StructIndex;
                for (var j = 0; j < memory.IndicesData.Length; j++)
                {
                    memory.IndicesData[j] = (ushort) (memory.IndicesData[j] + structOffset);
                }
            }

            bonedStream.FlushRender();
            c.PopModelMatrix();
            Gl.Disable(EnableCap.CullFace);
        }
    }
}