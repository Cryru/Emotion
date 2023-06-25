#region Using

using Emotion.Graphics.ThreeDee;

#endregion

#nullable enable

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