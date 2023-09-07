#nullable enable

#region Using

using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Emotion.Common.Serialization;
using Emotion.Editor;
using Emotion.Game.Animation3D;
using Emotion.Game.ThreeDee;
using Emotion.Game.World;
using Emotion.Graphics;
using Emotion.Graphics.Batches;
using Emotion.Graphics.Data;
using Emotion.Graphics.Objects;
using Emotion.Graphics.ThreeDee;
using Emotion.IO;
using OpenGL;

#endregion

namespace Emotion.Game.World3D;

public class GameObject3D : BaseGameObject
{
	[AssetFileName<MeshAsset>] public string? EntityPath;

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
		if (_skeletalShader == null) _skeletalShader = await Engine.AssetLoader.GetAsync<ShaderAsset>("");

		if (string.IsNullOrEmpty(EntityPath)) return;

		var asset = await Engine.AssetLoader.GetAsync<MeshAsset>(EntityPath);
		Entity = asset?.Entity;
	}

	protected override void UpdateInternal(float dt)
	{
		_time += dt;
		ApplyBoneMatrices();
		base.UpdateInternal(dt);
	}

	/// <inheritdoc />
	protected override void RenderInternal(RenderComposer c)
	{
		// todo: larger entities should create their own data buffers.
		// todo: culling state.
		if (_entity?.Meshes == null) return;

		Mesh[] meshes = _entity.Meshes;
		MeshEntityMetaState? metaState = EntityMetaState;

		c.FlushRenderStream();

		if (_entity.BackFaceCulling)
		{
			Gl.Enable(EnableCap.CullFace); // todo: render stream state
			Gl.CullFace(CullFaceMode.Back);
			Gl.FrontFace(FrontFaceDirection.Ccw);
		}

		c.PushModelMatrix(GetModelMatrix());

		if (_entity.AnimationRig == null)
		{
			for (var i = 0; i < meshes.Length; i++)
			{
				if (metaState != null && !metaState.RenderMesh[i]) continue;

				Mesh obj = meshes[i];
				obj.Render(c);
			}
		}
		else if (_skeletalShader != null)
		{
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

			c.SetShader();
		}

		c.PopModelMatrix();
		if (_entity.BackFaceCulling) Gl.Disable(EnableCap.CullFace);
	}

	#region Transform 3D

	protected Matrix4x4 _rotationMatrix;
	protected Matrix4x4 _scaleMatrix;
	protected Matrix4x4 _translationMatrix;

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected override void Resized()
	{
		base.Resized();

		Assert(!float.IsNaN(_width));
		Assert(!float.IsNaN(_depth));
		Assert(!float.IsNaN(_height));
		_scaleMatrix = Matrix4x4.CreateScale(_width, _depth, _height);
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