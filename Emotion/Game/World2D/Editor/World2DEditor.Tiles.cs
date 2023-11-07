#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World3D.Objects;
using Emotion.Graphics;
using Emotion.Graphics.ThreeDee;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
	// Tile selection
	private bool _tileSelect = false;
	private Vector2 _tileBrush = Vector2.Zero;

	protected InfiniteGrid? _grid;

	protected UIBaseWindow? _tileEditor;

	protected void InitializeTileEditor()
	{
		InitializeTileGrid();
	}

	private void InitializeTileGrid()
	{
		if (_grid == null)
		{
			Task.Run(() =>
			{
				_grid = new InfiniteGrid();
				_grid.LoadAssetsAsync().Wait();
				InitializeTileGrid();
			});
			return;
		}

		if (CurrentMap?.TileData == null) return;

		_grid.TileSize = CurrentMap.TileData.TileSize.X;
		_grid.Offset = new Vector2(_grid.TileSize / 2f + 0.5f);
		_grid.Tint = Color.Black.Clone().SetAlpha(125);
	}

	protected void UpdateTileEditor()
	{

	}

	protected void RenderTileEditor(RenderComposer c)
	{
		if (!IsTileEditorOpen()) return;
		_grid?.Render(c);
	}

	protected bool IsTileEditorOpen()
	{
		return _tileEditor != null && _tileEditor.Controller != null;
	}
}