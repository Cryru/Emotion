#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World3D.Objects;

public sealed class InfiniteGrid : Quad3D
{
	public float TileSize = 100;
	public Vector2 Offset;
	private Vector2 _infiniteGridSize = new Vector2(10_000, 10_000);

	public InfiniteGrid()
	{
		Size3D = _infiniteGridSize.ToVec3(1);
		ObjectFlags |= ObjectFlags.Map3DDontReceiveShadow;
		ObjectFlags |= ObjectFlags.Map3DDontThrowShadow;
		ObjectFlags |= ObjectFlags.Map3DDontReceiveAmbient;
	}

	public override async Task LoadAssetsAsync()
	{
		await base.LoadAssetsAsync();
		await EntityMetaState!.SetShader("Shaders/3DGrid.xml");
	}

	protected override void RenderInternal(RenderComposer c)
	{
		Vector2 cameraPos = c.Camera.Position.ToVec2();
		Position = cameraPos.ToVec3(Z); // Set position to camera position without the Z

		EntityMetaState!.SetShaderParam("squareSize", new Vector2(TileSize));
		EntityMetaState.SetShaderParam("cameraPos", (cameraPos + Offset) / _infiniteGridSize);
		EntityMetaState.SetShaderParam("totalSize", _infiniteGridSize);

		base.RenderInternal(c);
	}

	public override bool IsTransparent()
	{
		return true;
	}
}