#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Graphics;
using Emotion.IO;

#endregion

namespace Emotion.Game.World3D.Objects;

public sealed class InfiniteGrid : Quad3D
{
	public float TileSize = 100;
	public Vector2 Offset;

	private static ShaderAsset? _shader;
	private Vector2 _infiniteGridSize = new Vector2(10_000, 10_000);

	public InfiniteGrid()
	{
		Size3D = _infiniteGridSize.ToVec3(1);
	}

    public override async Task LoadAssetsAsync()
    {
        _shader ??= await Engine.AssetLoader.GetAsync<ShaderAsset>("Shaders/3DGrid.xml");

        await base.LoadAssetsAsync();
    }

    protected override void RenderInternal(RenderComposer c)
	{
		if (_shader == null) return;

		Vector2 cameraPos = c.Camera.Position.ToVec2();
		Position = cameraPos.ToVec3(Z); // Set position to camera position without the Z
		c.SetShader(_shader.Shader);
		_shader.Shader.SetUniformVector2("squareSize", new Vector2(TileSize));
		_shader.Shader.SetUniformVector2("cameraPos", (cameraPos + Offset) / _infiniteGridSize); // Camera position in UV space.
		_shader.Shader.SetUniformVector2("totalSize", _infiniteGridSize);
		_shader.Shader.SetUniformColor("color", Tint);
		base.RenderInternal(c);
		c.SetShader();
	}
}