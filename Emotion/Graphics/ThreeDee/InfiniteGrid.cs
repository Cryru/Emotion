#region Using

using Emotion.IO;

#endregion

#nullable enable

namespace Emotion.Graphics.ThreeDee;

public sealed class InfiniteGrid : Quad3D
{
	public float TileSize = 100;
	public Vector2 Offset;

	private ShaderAsset? _shader;
	private Vector2 _infiniteGridSize = new Vector2(10_000, 10_000);

	public InfiniteGrid()
	{
		_shader = Engine.AssetLoader.Get<ShaderAsset>("Shaders/3DGrid.xml");
		Size = _infiniteGridSize.ToVec3(1);
	}

	public override void Render(RenderComposer c)
	{
		if (_shader == null) return;

		Vector2 cameraPos = c.Camera.Position.ToVec2();
		Position = cameraPos.ToVec3(Z); // Set position to camera position without the Z
		c.SetShader(_shader.Shader);
		_shader.Shader.SetUniformVector2("squareSize", new Vector2(TileSize));
		_shader.Shader.SetUniformVector2("cameraPos", (cameraPos + Offset) / _infiniteGridSize); // Camera position in UV space.
		_shader.Shader.SetUniformVector2("totalSize", _infiniteGridSize);
		base.Render(c);
		c.SetShader();
	}
}