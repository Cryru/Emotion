#region Using

using Emotion.Game.Animation3D;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using OpenGL;

#endregion

#nullable enable

namespace Emotion.Game.ThreeDee
{
	public class Object3D : Transform3D, IRenderable
	{
		public MeshEntity? Entity
		{
			get => _entity;
			set
			{
				_entity = value;
				_totalNumberOfBones = -1;
				EntityMetaState = new MeshEntityMetaState(value);
				SetAnimation(null); // Reset animation
			}
		}

		private MeshEntity? _entity;

		public MeshEntityMetaState? EntityMetaState { get; private set; }

		public string CurrentAnimation
		{
			get => _currentAnimation?.Name ?? "None";
		}

		private SkeletalAnimation? _currentAnimation;
		private float _time;
		private int _totalNumberOfBones = -1;
		private Matrix4x4[]? _boneMatrices;

		private static ShaderAsset? _skeletalShader;

		public void SetAnimation(string? name)
		{
			if (_entity == null)
			{
				_currentAnimation = null;
				_time = 0;
				return;
			}

			SkeletalAnimation? animInstance = null;
			if (_entity.Animations != null)
				for (var i = 0; i < _entity.Animations.Length; i++)
				{
					SkeletalAnimation anim = _entity.Animations[i];
					if (anim.Name == name) animInstance = anim;
				}

			_currentAnimation = animInstance;
			_time = 0;

			// Cache some info.
			if (_totalNumberOfBones == -1)
			{
				for (var i = 0; i < _entity.Meshes.Length; i++)
				{
					Mesh mesh = _entity.Meshes[i];
					if (mesh.Bones != null) _totalNumberOfBones += mesh.Bones.Length;
				}

				_totalNumberOfBones++; // Zero index contains identity.
				_totalNumberOfBones++; // Convert to zero indexed.
				if (_boneMatrices == null)
					_boneMatrices = new Matrix4x4[_totalNumberOfBones];
				else if (_totalNumberOfBones > _boneMatrices.Length) Array.Resize(ref _boneMatrices, _totalNumberOfBones);

				// Must be the same equal or less than constant in the SkeletalAnim.vert
				// todo: make engine variable that is overwritten in shader.
				// todo: make into ubo
				if (_totalNumberOfBones > 126) Engine.Log.Error($"Entity {_entity.Name} has more bones ({_totalNumberOfBones} > 126) in all its meshes combined than allowed.", "3D");
			}

			Debug.Assert(_boneMatrices != null);

			Matrix4x4 defaultMatrix = Matrix4x4.Identity;
			if (_entity.AnimationRig != null) defaultMatrix = _entity.AnimationRig.LocalTransform;
			for (var i = 0; i < _boneMatrices.Length; i++)
			{
				_boneMatrices[i] = defaultMatrix;
			}
		}

		public virtual void Update(float dt)
		{
			_time += dt;
			_currentAnimation?.ApplyBoneMatrices(Entity, _boneMatrices, _time % _currentAnimation.Duration);
		}

		public void Render(RenderComposer c)
		{
			// Render using the render stream.
			// todo: larger entities should create their own data buffers.
			// todo: culling state.
			// todo: better differentiation between animated and non-animated meshes.
			if (_entity?.Meshes == null) return;

			c.FlushRenderStream();

			if (_entity.BackFaceCulling)
			{
				Gl.Enable(EnableCap.CullFace); // todo: render stream state
				Gl.CullFace(CullFaceMode.Back);
				Gl.FrontFace(FrontFaceDirection.Ccw);
			}

			c.PushModelMatrix(_scaleMatrix * _rotationMatrix * _translationMatrix);

			Mesh[] meshes = _entity.Meshes;

			// Assume that if the first mesh is boned, all are.
			if (meshes.Length > 0 && meshes[0].VerticesWithBones != null)
				RenderAnimatedEntityMeshesAsStatic(c);
			else
				RenderEntityMeshes(c);

			c.PopModelMatrix();

			c.FlushRenderStream();

			if (_entity.BackFaceCulling) Gl.Disable(EnableCap.CullFace);
		}

		/// <summary>
		/// Render the mesh using the default render stream.
		/// </summary>
		private void RenderEntityMeshes(RenderComposer c)
		{
			Debug.Assert(_entity != null);
			Debug.Assert(_entity.Meshes != null);

			Mesh[] meshes = _entity.Meshes;
			MeshEntityMetaState? metaState = EntityMetaState;

			for (var i = 0; i < meshes.Length; i++)
			{
				if (metaState != null && !metaState.RenderMesh[i]) continue;

				Mesh obj = meshes[i];
				obj.Render(c);
			}
		}

		/// <summary>
		/// Renders a mesh with bone vertices as a non-animated mesh.
		/// Allows animated mesh to be drawn using the default render stream (which is a non animated vertex format).
		/// </summary>
		private void RenderAnimatedEntityMeshesAsStatic(RenderComposer c)
		{
			Debug.Assert(_entity != null);
			Debug.Assert(_entity.Meshes != null);

			Mesh[] meshes = _entity.Meshes;
			MeshEntityMetaState? metaState = EntityMetaState;

			for (var i = 0; i < meshes.Length; i++)
			{
				if (metaState != null && !metaState.RenderMesh[i]) continue;

				Mesh obj = meshes[i];
				Debug.Assert(obj.VerticesWithBones != null);

				VertexDataWithBones[] vertData = obj.VerticesWithBones;
				ushort[] indices = obj.Indices;
				Texture? texture = null;
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
		/// Also requires the SkeletalAnim shader or one that supports skinned meshes.
		/// </summary>
		public void RenderAnimated(RenderComposer c, RenderStreamBatch<VertexDataWithBones> bonedStream)
		{
			_skeletalShader ??= Engine.AssetLoader.Get<ShaderAsset>("Shaders/SkeletalAnim.xml");
			if (_skeletalShader == null) return;

			Mesh[]? meshes = Entity?.Meshes;
			MeshEntityMetaState? metaState = EntityMetaState;

			if (Entity == null || meshes == null) return;
			if (_boneMatrices == null) SetAnimation(null);
			Debug.Assert(_boneMatrices != null);

			c.FlushRenderStream();

            if (Entity.BackFaceCulling)
			{
                Gl.Enable(EnableCap.CullFace);
                Gl.CullFace(CullFaceMode.Back);
                Gl.FrontFace(FrontFaceDirection.Ccw);
            }

			c.PushModelMatrix(_scaleMatrix * _rotationMatrix * _translationMatrix);
			c.SetShader(_skeletalShader.Shader);
			_skeletalShader.Shader.SetUniformMatrix4("finalBonesMatrices", _boneMatrices, _boneMatrices.Length);

			for (var i = 0; i < meshes.Length; i++)
			{
				Mesh obj = meshes[i];
				if (metaState != null && !metaState.RenderMesh[i]) continue;

				Debug.Assert(obj.VerticesWithBones != null);
				VertexDataWithBones[] vertData = obj.VerticesWithBones;
				ushort[] indices = obj.Indices;
				Texture? texture = null;
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

			if (bonedStream.AnythingMapped) bonedStream.FlushRender();
			c.SetShader();
			c.PopModelMatrix();

            if (Entity.BackFaceCulling)
                Gl.Disable(EnableCap.CullFace);
		}

		public Matrix4x4 GetModelMatrix()
		{
			return _scaleMatrix * _rotationMatrix * _translationMatrix;
		}

		public virtual void Dispose()
		{
		}
	}
}