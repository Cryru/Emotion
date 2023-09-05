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
		/// <summary>
		/// The current visual of this object.
		/// </summary>
		public MeshEntity? Entity
		{
			get => _entity;
			set
			{
				lock (this)
				{
					_entity = value;
					EntityMetaState = new MeshEntityMetaState(value);
					OnSetEntity();
				}
			}
		}

		private MeshEntity? _entity;

		/// <summary>
		/// Entity related state for this particular object.
		/// </summary>
		public MeshEntityMetaState? EntityMetaState { get; private set; }

		/// <summary>
		/// The name of the current animation playing (if any).
		/// </summary>
		public string CurrentAnimation
		{
			get => _currentAnimation?.Name ?? "None";
		}

		private SkeletalAnimation? _currentAnimation;
		private float _time;
		private Matrix4x4[][]? _boneMatricesPerMesh;

		// Caches


		private static ShaderAsset? _skeletalShader;
		private const int MAX_BONES = 126; // Must match number in SkeletalAnim.vert

		// temp
		public enum RenderLike
		{
			RenderStream,
			RenderStreamAnimated
		}

		public RenderLike RenderMode = RenderLike.RenderStream;

		private void OnSetEntity()
		{
			RenderMode = RenderLike.RenderStream;
			if (_entity?.Meshes == null) return;

			// Create bone matrices for the meshes (if they have bones)
			_boneMatricesPerMesh = new Matrix4x4[_entity.Meshes.Length][];

			for (var i = 0; i < _entity.Meshes.Length; i++)
			{
				Mesh mesh = _entity.Meshes[i];
				var boneCount = 1; // idx 0 is identity
				if (mesh.Bones != null)
				{
					boneCount += mesh.Bones.Length;
					if (boneCount > MAX_BONES)
					{
						Engine.Log.Error($"Entity {_entity.Name}'s mesh {mesh.Name} has too many bones ({boneCount} > {MAX_BONES}).", "3D");
						boneCount = MAX_BONES;
					}
				}

				_boneMatricesPerMesh[i] = new Matrix4x4[boneCount];
				mesh.BuildRuntimeBoneCache();
			}

			var renderMode = RenderLike.RenderStream;
			SkeletonAnimRigRoot? skeleton = _entity.AnimationRig;
			if (skeleton != null) renderMode = RenderLike.RenderStreamAnimated;
			RenderMode = renderMode;

			// Reset the animation.
			// This will also set the default bone matrices.
			SetAnimation(null);
		}

		public void SetAnimation(string? name)
		{
			if (_entity?.Meshes == null)
			{
				_currentAnimation = null;
				_time = 0;
				return;
			}

			// Try to find the animation instance.
			SkeletalAnimation? animInstance = null;
			if (_entity.Animations != null)
				for (var i = 0; i < _entity.Animations.Length; i++)
				{
					SkeletalAnimation anim = _entity.Animations[i];
					if (anim.Name == name) animInstance = anim;
				}

			_currentAnimation = animInstance;
			_time = 0;
			
			// Initialize bones
			ApplyBoneMatrices();
		}

		public virtual void Update(float dt)
		{
			_time += dt;
			ApplyBoneMatrices();
		}

		private void ApplyBoneMatrices()
		{
			lock (this)
			{
				if (_entity?.Meshes == null) return;

				AssertNotNull(_boneMatricesPerMesh);
				for (var i = 0; i < _boneMatricesPerMesh.Length; i++)
				{
					Matrix4x4[] matricesForMesh = _boneMatricesPerMesh[i];
					matricesForMesh[0] = Matrix4x4.Identity;
				}

				SkeletonAnimRigRoot? animationRig = _entity.AnimationRig;
				if (animationRig == null) return;

				ApplyBoneMatricesWalkTree(_time % _currentAnimation?.Duration ?? 0, animationRig, Matrix4x4.Identity);
			}
		}

		private void ApplyBoneMatricesWalkTree(float timeStamp, SkeletonAnimRigNode node, Matrix4x4 parentMatrix)
		{
			string nodeName = node.Name ?? "Unknown";

			Matrix4x4 currentMatrix = node.LocalTransform;
			if (_currentAnimation != null)
			{
				if (node.DontAnimate)
				{
					currentMatrix = Matrix4x4.Identity;
				}
				else
				{
					SkeletonAnimChannel? channel = _currentAnimation.GetMeshAnimBone(nodeName);
					if (channel != null)
						currentMatrix = channel.GetMatrixAtTimestamp(timeStamp);
				}
			}

			Matrix4x4 myMatrix = currentMatrix * parentMatrix;

			AssertNotNull(_entity);
			AssertNotNull(_entity.Meshes);
			for (var i = 0; i < _entity.Meshes.Length; i++)
			{
				Mesh mesh = _entity.Meshes[i];
				if (mesh.Bones == null) continue;

				AssertNotNull(_boneMatricesPerMesh);
				Matrix4x4[] myMatrices = _boneMatricesPerMesh[i];

				AssertNotNull(mesh.BoneNameCache);
				if (mesh.BoneNameCache.TryGetValue(nodeName, out MeshBone? meshBone)) myMatrices[meshBone.BoneIndex] = meshBone.OffsetMatrix * myMatrix;
			}

			if (node.Children == null) return;
			for (var i = 0; i < node.Children.Length; i++)
			{
				SkeletonAnimRigNode child = node.Children[i];
				ApplyBoneMatricesWalkTree(timeStamp, child, myMatrix);
			}
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

			c.PushModelMatrix(GetModelMatrix());

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
			AssertNotNull(_entity);
			AssertNotNull(_entity.Meshes);

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
			AssertNotNull(_entity);
			AssertNotNull(_entity.Meshes);

			Mesh[] meshes = _entity.Meshes;
			MeshEntityMetaState? metaState = EntityMetaState;

			for (var i = 0; i < meshes.Length; i++)
			{
				if (metaState != null && !metaState.RenderMesh[i]) continue;

				Mesh obj = meshes[i];
				AssertNotNull(obj.VerticesWithBones);

				VertexDataWithBones[] vertData = obj.VerticesWithBones;
				ushort[] indices = obj.Indices;
				Texture? texture = null;
				if (obj.Material.DiffuseTexture != null) texture = obj.Material.DiffuseTexture;
				StreamData<VertexData> memory = c.RenderStream.GetStreamMemory<VertexData>((uint) vertData!.Length, (uint) indices.Length, BatchMode.SequentialTriangles, texture);

				if (memory.VerticesData.Length == 0)
				{
					Engine.Log.Warning($"Couldn't render mesh {obj.Name} due to it requiring more indices than available in the render stream.", "3D", true);
					continue;
				}

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
		public void RenderAnimated(RenderComposer c)
		{
			_skeletalShader ??= Engine.AssetLoader.Get<ShaderAsset>("Shaders/SkeletalAnim.xml");
			if (_skeletalShader == null) return;
			if (_entity == null) return;

			Mesh[]? meshes = _entity.Meshes;
			MeshEntityMetaState? metaState = EntityMetaState;

			if (meshes == null) return;

			c.FlushRenderStream();

			if (_entity.BackFaceCulling)
			{
				Gl.Enable(EnableCap.CullFace);
				Gl.CullFace(CullFaceMode.Back);
				Gl.FrontFace(FrontFaceDirection.Ccw);
			}

			c.PushModelMatrix(GetModelMatrix());
			c.SetShader(_skeletalShader.Shader);

			Matrix4x4[][]? boneMatricesPerMesh = _boneMatricesPerMesh;
			AssertNotNull(boneMatricesPerMesh);

			for (var i = 0; i < meshes.Length; i++)
			{
				Mesh obj = meshes[i];
				if (metaState != null && !metaState.RenderMesh[i]) continue;
				_skeletalShader.Shader.SetUniformColor("diffuseColor", obj.Material.DiffuseColor);

				Matrix4x4[] boneMats = boneMatricesPerMesh[i];
				_skeletalShader.Shader.SetUniformMatrix4("finalBonesMatrices", boneMats, boneMats.Length);

				AssertNotNull(obj.VerticesWithBones);
				VertexDataWithBones[] vertData = obj.VerticesWithBones;
				ushort[] indices = obj.Indices;
				Texture? texture = null;
				if (obj.Material.DiffuseTexture != null) texture = obj.Material.DiffuseTexture;
				var memory = c.RenderStream.GetStreamMemory<VertexDataWithBones>((uint) vertData!.Length, (uint) indices.Length, BatchMode.SequentialTriangles, texture);

				// Didn't manage to get enough memory.
				if (memory.VerticesData.Length == 0)
				{
					Engine.Log.Warning($"Couldn't render mesh {obj.Name} due to it requiring more indices than available in the render stream.", "3D", true);
					continue;
				}

				vertData.CopyTo(memory.VerticesData);
				indices.CopyTo(memory.IndicesData);

				ushort structOffset = memory.StructIndex;
				for (var j = 0; j < memory.IndicesData.Length; j++)
				{
					memory.IndicesData[j] = (ushort) (memory.IndicesData[j] + structOffset);
				}

				c.FlushRenderStream();
            }

			c.FlushRenderStream();
            c.SetShader();
			c.PopModelMatrix();

			if (_entity.BackFaceCulling)
				Gl.Disable(EnableCap.CullFace);
		}

		public Matrix4x4 GetModelMatrix()
		{
			return _scaleMatrix * _rotationMatrix * _translationMatrix;
		}

		public virtual void Dispose()
		{
		}

		#region Debug

		public void DebugDrawSkeleton(RenderComposer c)
		{
			SkeletonAnimRigRoot? rig = _entity?.AnimationRig;
			if (rig == null) return;

			var coneMeshGenerator = new CylinderMeshGenerator
			{
				RadiusTop = 0,
				RadiusBottom = 1.25f,
				Sides = 4
			};
			var visualizationMeshes = new List<Mesh>();

			void DrawSkeleton(SkeletonAnimRigNode node, Matrix4x4 parentMatrix, Vector3 parentPos)
			{
				Matrix4x4 currentMatrix = node.LocalTransform;
				if (_currentAnimation != null)
				{
					if (node.DontAnimate)
					{
						currentMatrix = Matrix4x4.Identity;
					}
					else
					{
						SkeletonAnimChannel? channel = _currentAnimation.GetMeshAnimBone(node.Name);
						if (channel != null)
							currentMatrix = channel.GetMatrixAtTimestamp(_time % _currentAnimation.Duration);
					}
				}

				Matrix4x4 matrix = currentMatrix * parentMatrix;
				Vector3 bonePos = Vector3.Transform(Vector3.Zero, matrix);

				if (parentPos != Vector3.Zero)
				{
					float height = Vector3.Distance(parentPos, bonePos);
					coneMeshGenerator.Height = height * Height;

					// Look at params
					Vector3 conePos = parentPos;
					Vector3 lookTowards = bonePos;
					Vector3 meshDefaultLook = Vector3.UnitZ;

					// Look at
					Vector3 dir = Vector3.Normalize(lookTowards - conePos);
					Vector3 rotationAxis = Vector3.Cross(meshDefaultLook, dir);
					float rotationAngle = MathF.Acos(Vector3.Dot(meshDefaultLook, dir) / meshDefaultLook.Length() / dir.Length());
					var rotationMatrix = Matrix4x4.CreateFromAxisAngle(rotationAxis, rotationAngle);

					Mesh coneMesh = coneMeshGenerator.GenerateMesh().TransformMeshVertices(
						_scaleMatrix.Inverted() *
						rotationMatrix *
						Matrix4x4.CreateTranslation(conePos)
					).ColorMeshVertices(Color.PrettyPink);
					visualizationMeshes.Add(coneMesh);
				}

				SkeletonAnimRigNode[]? children = node.Children;
				if (children == null) return;
				for (var i = 0; i < children.Length; i++)
				{
					SkeletonAnimRigNode child = children[i];
					DrawSkeleton(child, matrix, bonePos);
				}
			}

			DrawSkeleton(rig, Matrix4x4.Identity, Vector3.Zero);

			c.PushModelMatrix(GetModelMatrix());
			for (var i = 0; i < visualizationMeshes.Count; i++)
			{
				Mesh mesh = visualizationMeshes[i];
				mesh.Render(c);
			}

			c.PopModelMatrix();
		}

		#endregion
	}
}