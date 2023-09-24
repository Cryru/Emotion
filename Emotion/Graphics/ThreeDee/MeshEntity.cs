#nullable enable

#region Using

using Emotion.Game.Animation3D;
using Emotion.Graphics.Data;

#endregion

namespace Emotion.Graphics.ThreeDee;

/// <summary>
/// A collection of meshes which make up one visual object.
/// Not all of the meshes are always visible.
/// </summary>
public class MeshEntity
{
	public string? Name { get; set; }
	public float Scale { get; set; } = 1f;
	public Matrix4x4 LocalTransform { get; set; } = Matrix4x4.Identity;
	public Mesh[]? Meshes { get; set; }

	// Animation
	public SkeletalAnimation[]? Animations { get; set; }
	public SkeletonAnimRigRoot? AnimationRig { get; set; }

	// Render settings
	public bool BackFaceCulling = true;

	// Caches
	public Dictionary<string, (Sphere, Cube)>? CachedBounds;

	public void CacheBounds(bool force = false)
	{
		if (force) CachedBounds = null;
		if (CachedBounds != null) return;

		CachedBounds = new();

		CalculateBounds("null", out Sphere sphere, out Cube cube);
		CachedBounds.Add("null", (sphere, cube));

		if (Animations != null)
			for (var i = 0; i < Animations.Length; i++)
			{
				SkeletalAnimation anim = Animations[i];
				CalculateBounds(anim.Name, out Sphere sphereAnim, out Cube cubeAnim);
				CachedBounds.Add(anim.Name, (sphereAnim, cubeAnim));
			}
	}

	public void GetBounds(string? anim, out Sphere sphere, out Cube cube)
	{
		sphere = new Sphere();
		cube = new Cube();

		CacheBounds();
		if (CachedBounds == null) return;

		anim ??= "null";
		if (CachedBounds.TryGetValue(anim, out (Sphere, Cube) bounds))
		{
			sphere = bounds.Item1;
			cube = bounds.Item2;
		}
	}

	protected void CalculateBounds(string anim, out Sphere sphere, out Cube cube)
	{
		sphere = new Sphere();
		cube = new Cube();

		var first = true;
		var min = new Vector3(0);
		var max = new Vector3(0);

		IEnumerator<Vector3> vertices = IterateAllMeshVertices(anim);
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
		sphere = new Sphere(center, radius);

		Vector3 halfExtent = (max - min) / 2f;
		cube = new Cube(center, halfExtent);
	}

	protected IEnumerator<Vector3> IterateAllMeshVertices(string animation)
	{
		Mesh[]? meshes = Meshes;
		if (meshes == null) yield break;

		var boneMatricesPerMesh = new Matrix4x4[meshes.Length][];
		for (var i = 0; i < meshes.Length; i++)
		{
			Mesh mesh = meshes[i];
			var boneCount = 1; // idx 0 is identity
			if (mesh.Bones != null) boneCount += mesh.Bones.Length;
			boneMatricesPerMesh[i] = new Matrix4x4[boneCount];
		}

		for (var i = 0; i < meshes.Length; i++)
		{
			Mesh mesh = meshes[i];
			VertexData[] meshVertices = mesh.Vertices;
			Mesh3DVertexDataBones[]? boneData = mesh.BoneData;

			// Non animated mesh ezpz
			if (boneData == null)
			{
				for (var v = 0; v < meshVertices.Length; v++)
				{
					yield return meshVertices[v].Vertex;
				}

				continue;
			}

			// We will calculate the bone matrices by sampling keyframes and sum
			// up their bounds and get the total animated bound.
			SkeletalAnimation? currentAnimation = null;
			for (var j = 0; j < Animations?.Length; j++)
			{
				SkeletalAnimation anim = Animations[j];
				if (anim.Name == animation)
				{
					currentAnimation = anim;
					break;
				}
			}

			// If there is a current animation go through all key frames.
			SkeletonAnimChannel[]? channels = currentAnimation?.AnimChannels;
			int channelLength = channels?.Length ?? 1;

			for (var j = 0; j < channelLength; j++)
			{
				MeshAnimBoneTranslation[] positionFrames;
				if (channels != null)
				{
					// Going through every single frame is too heavy.
					// SkeletonAnimChannel channel = channels[j];
					// positionFrames = channel.Positions; 

					float animationDuration = currentAnimation!.Duration;
					positionFrames = new[]
					{
						new MeshAnimBoneTranslation
						{
							Timestamp = 0
						},
						new MeshAnimBoneTranslation
						{
							Timestamp = animationDuration * 0.25f
						},
						new MeshAnimBoneTranslation
						{
							Timestamp = animationDuration * 0.5f
						},
						new MeshAnimBoneTranslation
						{
							Timestamp = animationDuration
						}
					};
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
					CalculateBoneMatrices(currentAnimation, boneMatricesPerMesh, positionFrames[k].Timestamp);

					Matrix4x4[] bonesForThisMesh = boneMatricesPerMesh[i];
					for (var v = 0; v < boneData.Length; v++)
					{
						Mesh3DVertexDataBones vertexData = boneData[v];
						Vector3 vertex = meshVertices[v].Vertex;

						Vector3 vertexTransformed = Vector3.Zero;
						for (var w = 0; w < 4; w++)
						{
							float boneId = vertexData.BoneIds[w];
							float weight = vertexData.BoneWeights[w];

							Matrix4x4 boneMat = bonesForThisMesh[(int) boneId];
							Vector3 thisWeightPos = Vector3.Transform(vertex, boneMat);
							vertexTransformed += thisWeightPos * weight;
						}

						yield return vertexTransformed;
					}
				}
			}
		}
	}

	public void CalculateBoneMatrices(SkeletalAnimation? animation, Matrix4x4[][]? matrices, float timeStamp)
	{
		if (matrices == null) return;

		SkeletonAnimRigRoot? animationRig = AnimationRig;
		Mesh[]? meshes = Meshes;

		if (animationRig == null || meshes == null) return;

		// Initialize identity for all meshes matrices.
		for (var i = 0; i < matrices.Length; i++)
		{
			Matrix4x4[] matricesForMesh = matrices[i];
			matricesForMesh[0] = Matrix4x4.Identity;
		}

		CalculateBoneMatricesWalkTree(meshes, matrices, animation, timeStamp, animationRig, Matrix4x4.Identity);
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
}