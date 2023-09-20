#nullable enable

#region Using

using Emotion.Game.Animation3D;

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
}