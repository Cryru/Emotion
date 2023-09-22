#nullable enable

#region Using

using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Game.ThreeDee;

/// <summary>
/// Holds state information referring to a mesh entity.
/// </summary>
public class MeshEntityMetaState
{
	/// <summary>
	/// Whether the mesh index should be rendered.
	/// </summary>
	public bool[] RenderMesh = Array.Empty<bool>();

	/// <summary>
	/// A color to tint all vertices of the entity in.
	/// Is multiplied by the material color.
	/// </summary>
	public Color Tint = Color.White;

	public MeshEntityMetaState(MeshEntity? entity)
	{
		if (entity == null) return;

		RenderMesh = new bool[entity.Meshes.Length];
		for (var i = 0; i < RenderMesh.Length; i++)
		{
			RenderMesh[i] = true;
		}
	}
}