#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Graphics.ThreeDee;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
	// Tile selection
	private bool _tileSelect = false;
	private Vector2 _tileBrush = Vector2.Zero;

	protected InfiniteGrid? _grid;

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
				InitializeTileGrid();
			});
			return;
		}

		if (CurrentMap?.TileData == null) return;

		_grid.TileSize = CurrentMap.TileData.TileSize.X;
		_grid.Offset = new Vector2(_grid.TileSize / 2f + 0.5f);
		_grid.Tint = Color.Black.Clone().SetAlpha(125);
	}
}