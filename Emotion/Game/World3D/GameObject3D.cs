#nullable enable

#region Using

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Game.Animation3D;
using Emotion.Game.ThreeDee;
using Emotion.Game.World;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.Shading;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using OpenGL;

#endregion

namespace Emotion.Game.World3D;

public class GameObject3D : BaseGameObject
{
	/// <summary>
	/// The current visual of this object.
	/// </summary>
	[DontSerialize]
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

	/// <summary>
	/// Sphere that encompasses the whole object.
	/// If the mesh is animated the sphere encompasses all keyframes of the animation.
	/// </summary>
	public Sphere BoundingSphere
	{
		get => _bSphereBase.Transform(GetModelMatrix());
	}

	/// <summary>
	/// Axis aligned cube that encompasses the whole object.
	/// If the mesh is animated the AABB encompasses all keyframes of the animation.
	/// </summary>
	public Cube Bounds3D
	{
		get => _bCubeBase.Transform(GetModelMatrix());
	}

	protected Sphere _bSphereBase;
	protected Cube _bCubeBase;

	private SkeletalAnimation? _currentAnimation;
	private float _time;
	private Matrix4x4[][]? _boneMatricesPerMesh;

	private static ShaderAsset? _skeletalShader;
	private const int MAX_BONES = 126; // Must match number in SkeletalAnim.vert

	public GameObject3D(string name) : base(name)
	{
		_width = 1;
		_depth = 1;
		_height = 1;
		Resized();
		Moved();
		Rotated();
	}

	// Serialization constructor.
	protected GameObject3D()
	{
		_width = 1;
		_depth = 1;
		_height = 1;
		Resized();
		Moved();
		Rotated();
	}

	public override async Task LoadAssetsAsync()
	{
		_skeletalShader ??= await Engine.AssetLoader.GetAsync<ShaderAsset>("Shaders/SkeletalAnim.xml");
	}

	protected override void UpdateInternal(float dt)
	{
		_time += dt;
		CalculateBoneMatrices(_boneMatricesPerMesh, _time % _currentAnimation?.Duration ?? 0);
		base.UpdateInternal(dt);
	}

	/// <inheritdoc />
	protected override void RenderInternal(RenderComposer c)
	{
		// todo: larger entities should create their own data buffers.
		// todo: culling state.
		MeshEntity? entity = _entity;
		if (entity?.Meshes == null) return;

		CalculateBounds(out _bSphereBase, out _bCubeBase);

		Mesh[] meshes = entity.Meshes;
		MeshEntityMetaState? metaState = EntityMetaState;

		c.FlushRenderStream();

		if (entity.BackFaceCulling)
		{
			Gl.Enable(EnableCap.CullFace); // todo: render stream state
			Gl.CullFace(CullFaceMode.Back);
			Gl.FrontFace(FrontFaceDirection.Ccw);
		}

		c.PushModelMatrix(entity.LocalTransform * GetModelMatrix());

		if (entity.AnimationRig == null)
		{
			// todo: apply Tint property -> mesh shader
			for (var i = 0; i < meshes.Length; i++)
			{
				if (metaState != null && !metaState.RenderMesh[i]) continue;

				Mesh obj = meshes[i];
				obj.Render(c);
			}
		}
		else if (_skeletalShader != null)
		{
			ShaderProgram? shader = _skeletalShader.Shader;
			c.SetShader(shader);

			Matrix4x4[][]? boneMatricesPerMesh = _boneMatricesPerMesh;
			AssertNotNull(boneMatricesPerMesh);

			for (var i = 0; i < meshes.Length; i++)
			{
				Mesh obj = meshes[i];
				if (metaState != null && !metaState.RenderMesh[i]) continue;
				_skeletalShader.Shader.SetUniformColor("diffuseColor", obj.Material.DiffuseColor);
				_skeletalShader.Shader.SetUniformColor("objectTint", Tint);

				if (boneMatricesPerMesh != null)
				{
					Matrix4x4[] boneMats = boneMatricesPerMesh[i];
					shader.SetUniformMatrix4("finalBonesMatrices", boneMats, boneMats.Length);
				}

				AssertNotNull(obj.VerticesWithBones);
				VertexDataWithBones[] vertData = obj.VerticesWithBones;
				ushort[] indices = obj.Indices;
				Texture? texture = null;
				if (obj.Material.DiffuseTexture != null) texture = obj.Material.DiffuseTexture;
				StreamData<VertexDataWithBones> memory = c.RenderStream.GetStreamMemory<VertexDataWithBones>((uint) vertData!.Length, (uint) indices.Length, BatchMode.SequentialTriangles, texture);

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

			c.SetShader();
		}

		c.PopModelMatrix();
		if (entity.BackFaceCulling) Gl.Disable(EnableCap.CullFace);

		var meshGen = new SphereMeshGenerator();
		Sphere sphere = BoundingSphere;
		Mesh mesh = meshGen.GenerateMesh().TransformMeshVertices(
			Matrix4x4.CreateScale(sphere.Radius) * Matrix4x4.CreateTranslation(sphere.Origin)
		).ColorMeshVertices(Color.Green * 0.5f);
		mesh.Render(c);

		//var cube = Bounds3D;
		//cube.RenderOutline(c);
	}

	#region Transform 3D

	protected Matrix4x4 _rotationMatrix;
	protected Matrix4x4 _scaleMatrix;
	protected Matrix4x4 _translationMatrix;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override void Resized()
	{
		base.Resized();

		float entityScale = Entity?.Scale ?? 1f;

		Assert(!float.IsNaN(_width));
		Assert(!float.IsNaN(_depth));
		Assert(!float.IsNaN(_height));
		_scaleMatrix = Matrix4x4.CreateScale(_width * entityScale, _depth * entityScale, _height * entityScale);
	}

	protected override void Moved()
	{
		base.Moved();

		_translationMatrix = Matrix4x4.CreateTranslation(_x, _y, _z);
	}

	protected override void Rotated()
	{
		base.Rotated();

		_rotationMatrix = Matrix4x4.CreateFromYawPitchRoll(_rotation.Y, _rotation.X, _rotation.Z);

		// ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
		Map?.InvalidateObjectBounds(this);
	}

	public Matrix4x4 GetModelMatrix()
	{
		return _scaleMatrix * _rotationMatrix * _translationMatrix;
	}

	#endregion

	private void OnSetEntity()
	{
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

		// Reset the animation.
		// This will also set the default bone matrices.
		SetAnimation(null);

		// Update unit scale.
		Resized();

		CacheVerticesForCollision(false);
		CalculateBounds(out _bSphereBase, out _bCubeBase);
	}

	public void SetAnimation(string? name)
	{
		MeshEntity? entity = _entity;
		if (entity?.Meshes == null)
		{
			_currentAnimation = null;
			_time = 0;
			return;
		}

		// Try to find the animation instance.
		SkeletalAnimation? animInstance = null;
		if (entity.Animations != null)
			for (var i = 0; i < entity.Animations.Length; i++)
			{
				SkeletalAnimation anim = entity.Animations[i];
				if (anim.Name == name) animInstance = anim;
			}

		_currentAnimation = animInstance;
		_time = 0;

		// Initialize bones
		CalculateBoneMatrices(_boneMatricesPerMesh, 0);

		CacheVerticesForCollision();
		CalculateBounds(out _bSphereBase, out _bCubeBase);
	}

	private void CalculateBoneMatrices(Matrix4x4[][]? matrices, float timeStamp)
	{
		if (matrices == null) return;

		SkeletonAnimRigRoot? animationRig;
		Mesh[]? meshes;
		SkeletalAnimation? currentAnimation;

		lock (this)
		{
			meshes = _entity?.Meshes;
			animationRig = _entity?.AnimationRig;
			currentAnimation = _currentAnimation;
		}

		if (animationRig == null || meshes == null) return;

		// Initialize identity for all meshes matrices.
		for (var i = 0; i < matrices.Length; i++)
		{
			Matrix4x4[] matricesForMesh = matrices[i];
			matricesForMesh[0] = Matrix4x4.Identity;
		}

		CalculateBoneMatricesWalkTree(meshes, matrices, currentAnimation, timeStamp, animationRig, Matrix4x4.Identity);
	}

	private void CalculateBoneMatricesWalkTree(
		Mesh[] meshes,
		Matrix4x4[][] matrices,
		SkeletalAnimation? currentAnimation,
		float timeStamp,
		SkeletonAnimRigNode node,
		Matrix4x4 parentMatrix)
	{
		string nodeName = node.Name ?? "Unknown";

		Matrix4x4 currentMatrix = node.LocalTransform;
		if (currentAnimation != null)
		{
			if (node.DontAnimate)
			{
				currentMatrix = Matrix4x4.Identity;
			}
			else
			{
				SkeletonAnimChannel? channel = currentAnimation.GetMeshAnimBone(nodeName);
				if (channel != null)
					currentMatrix = channel.GetMatrixAtTimestamp(timeStamp);
			}
		}

		Matrix4x4 myMatrix = currentMatrix * parentMatrix;
		for (var i = 0; i < meshes.Length; i++)
		{
			Mesh mesh = meshes[i];
			if (mesh.Bones == null) continue;

			Matrix4x4[] myMatrices = matrices[i];

			AssertNotNull(mesh.BoneNameCache);
			if (mesh.BoneNameCache.TryGetValue(nodeName, out MeshBone? meshBone)) myMatrices[meshBone.BoneIndex] = meshBone.OffsetMatrix * myMatrix;
		}

		if (node.Children == null) return;
		for (var i = 0; i < node.Children.Length; i++)
		{
			SkeletonAnimRigNode child = node.Children[i];
			CalculateBoneMatricesWalkTree(meshes, matrices, currentAnimation, timeStamp, child, myMatrix);
		}
	}

	#region Bounds and Collision

	protected IEnumerator<Vector3> ForEachVertexInEveryKeyFrameForBounds()
	{
		Mesh[]? meshes = _entity?.Meshes;
		if (meshes == null) yield break;

		AssertNotNull(_entity);
		Matrix4x4 entityTransform = _entity.LocalTransform;

		Matrix4x4[][]? boneMatricesPerMesh = null;

		for (var i = 0; i < meshes.Length; i++)
		{
			Mesh mesh = meshes[i];
			VertexData[]? meshVertices = mesh.Vertices;
			VertexDataWithBones[]? verticesWithBones = mesh.VerticesWithBones;
			if (meshVertices == null && verticesWithBones == null) continue;

			// Non animated mesh ezpz
			if (meshVertices != null)
			{
				for (var v = 0; v < meshVertices.Length; v++)
				{
					yield return Vector3.Transform(meshVertices[v].Vertex, entityTransform);
				}

				continue;
			}

			AssertNotNull(verticesWithBones);
			AssertNotNull(_boneMatricesPerMesh);

			// Prepare bones if boned mesh
			if (boneMatricesPerMesh == null)
			{
				boneMatricesPerMesh = new Matrix4x4[_boneMatricesPerMesh.Length][];
				for (var j = 0; j < _boneMatricesPerMesh.Length; j++)
				{
					boneMatricesPerMesh[j] = new Matrix4x4[_boneMatricesPerMesh[j].Length];
				}
			}

			Matrix4x4[] bonesForThisMesh = boneMatricesPerMesh[i];

			// Check if there is a current animation, if not we still need to apply the node transforms.
			SkeletalAnimation? currentAnimation = _currentAnimation;

			// If there is a current animation go through all key frames.
			SkeletonAnimChannel[]? channels = currentAnimation?.AnimChannels;
			int channelLength = channels?.Length ?? 1;
			for (var j = 0; j < channelLength; j++)
			{
				MeshAnimBoneTranslation[] positionFrames;
				if (channels != null)
				{
					SkeletonAnimChannel channel = channels[j];
					positionFrames = channel.Positions;
				}
				else
				{
					positionFrames = new[]
					{
						new MeshAnimBoneTranslation
						{
							Timestamp = 0
						}
					};
				}

				for (var k = 0; k < positionFrames.Length; k++)
				{
					CalculateBoneMatrices(boneMatricesPerMesh, positionFrames[k].Timestamp);

					for (var v = 0; v < verticesWithBones.Length; v++)
					{
						VertexDataWithBones vertexData = verticesWithBones[v];

						Matrix4x4 boneMatrix = Matrix4x4.Identity;
						for (var w = 0; w < 4; w++)
						{
							float boneId = vertexData.BoneIds[w];
							float weight = vertexData.BoneWeights[w];
							if (boneId == 0 || weight == 0) continue;

							Matrix4x4 boneMat = bonesForThisMesh[(int) boneId];
							boneMatrix *= boneMat * weight;
						}

						yield return Vector3.Transform(vertexData.Vertex, entityTransform * boneMatrix);
					}
				}
			}
		}
	}

	protected void CalculateBounds(out Sphere boundingSphere, out Cube boundingCube)
	{
		var first = true;
		var min = new Vector3(0);
		var max = new Vector3(0);

		IEnumerator<Vector3> vertices = ForEachVertexInEveryKeyFrameForBounds();
		while (vertices.MoveNext())
		{
			Vector3 vertex = vertices.Current;
			if (first)
			{
				min = vertex;
				max = vertex;
				first = false;
			}
			else
			{
				// Find the minimum and maximum extents of the vertices
				min = Vector3.Min(min, vertex);
				max = Vector3.Max(max, vertex);
			}
		}

		Vector3 center = (min + max) / 2f;

		float radius = Vector3.Distance(center, max);
		boundingSphere = new Sphere(center, radius);

		Vector3 halfExtent = (max - min) / 2f;
		boundingCube = new Cube(center, halfExtent);
	}

	private Vector3[]?[]? _verticesCacheCollision;

	public void CacheVerticesForCollision(bool reuseMeshData = true)
	{
		Mesh[]? meshes = _entity?.Meshes;
		if (meshes == null) return;

		AssertNotNull(_entity);
		Matrix4x4 entityTransform = _entity.LocalTransform;

		if (!reuseMeshData) _verticesCacheCollision = null;
		_verticesCacheCollision ??= new Vector3[meshes.Length][];
		for (var m = 0; m < meshes.Length; m++)
		{
			Mesh mesh = meshes[m];
			VertexDataWithBones[]? verticesWithBones = mesh.VerticesWithBones;
			VertexData[]? vertices = mesh.Vertices;
			if (verticesWithBones == null && vertices == null) continue;

			int vertexCount = verticesWithBones?.Length ?? vertices.Length;
			Vector3[] thisMesh;
			if (_verticesCacheCollision[m] != null)
			{
				thisMesh = _verticesCacheCollision[m]!;
			}
			else
			{
				thisMesh = new Vector3[vertexCount];
				_verticesCacheCollision[m] = thisMesh;
			}

			if (verticesWithBones != null)
			{
				Matrix4x4[] bonesForThisMesh = _boneMatricesPerMesh![m];

				for (var vertexIdx = 0; vertexIdx < verticesWithBones.Length; vertexIdx++)
				{
					ref VertexDataWithBones vertexDataBoned = ref verticesWithBones![vertexIdx];
					Vector3 vertex = vertexDataBoned.Vertex;

					Matrix4x4 boneMatrix = Matrix4x4.Identity;
					for (var w = 0; w < 4; w++)
					{
						float boneId = vertexDataBoned.BoneIds[w];
						float weight = vertexDataBoned.BoneWeights[w];
						if (weight == 0) continue;

						Matrix4x4 boneMat = bonesForThisMesh[(int) boneId];
						boneMatrix *= boneMat * weight;
					}

					thisMesh[vertexIdx] = Vector3.Transform(vertex, entityTransform * boneMatrix);
				}
			}
			else if (vertices != null)
			{
				for (var vertexIdx = 0; vertexIdx < vertices.Length; vertexIdx++)
				{
					thisMesh[vertexIdx] = Vector3.Transform(vertices[vertexIdx].Vertex, entityTransform);
				}
			}
		}
	}

	public void GetMeshTriangleForCollision(int meshIdx, int v1, int v2, int v3, out Vector3 vert1, out Vector3 vert2, out Vector3 vert3)
	{
		vert1 = Vector3.Zero;
		vert2 = Vector3.Zero;
		vert3 = Vector3.Zero;

		Mesh[]? meshes = _entity?.Meshes;
		if (meshes == null) return;

		Mesh mesh = meshes[meshIdx];
		VertexData[]? meshVertices = mesh.Vertices;
		VertexDataWithBones[]? verticesWithBones = mesh.VerticesWithBones;
		if (meshVertices == null && verticesWithBones == null) return;
		if (_verticesCacheCollision == null) return;

		Vector3[] meshData = _verticesCacheCollision[meshIdx];
		vert1 = meshData[v1];
		vert2 = meshData[v2];
		vert3 = meshData[v3];
	}

	#endregion

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