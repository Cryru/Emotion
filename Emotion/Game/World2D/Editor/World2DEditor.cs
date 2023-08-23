#nullable enable

#region Using

using Emotion.Game.World.Editor;
using Emotion.Game.World2D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.Platform.Input;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor : WorldBaseEditorGeneric<Map2D>
{
	public World2DEditor(IWorld2DAwareScene scene, Type mapType) : base(scene, mapType)
	{
	}

	protected override CameraBase GetEditorCamera()
	{
		return new WASDMoveCamera2D(Vector3.Zero);
	}

	protected override void EnterEditorInternal()
	{
		InitializeObjectEditor();
		InitializeTileEditor();
	}

	protected override void ExitEditorInternal()
	{
		DisposeObjectEditor();
	}

	protected override void EditorInputHandlerInternal(Key key, KeyStatus status)
	{
		ObjectEditorInputHandler(key, status);
	}

	protected override void UpdateInternal(float dt)
	{
		UpdateObjectEditor();
	}

	protected override void RenderInternal(RenderComposer c)
	{
		c.SetUseViewMatrix(true);
		c.SetDepthTest(true);
		c.SetAlphaBlend(true);

		Map2D? map = CurrentMap;

		// Render invisible tile layers
		// todo: move to tile editor
		if (map?.TileData != null)
		{
			Rectangle clipRect = c.Camera.GetCameraFrustum();
			for (var i = 0; i < map.TileData.Layers.Count; i++)
			{
				Map2DTileMapLayer layer = map.TileData.Layers[i];
				if (!layer.Visible) map.TileData.RenderLayer(c, i, clipRect);
			}

			c.ClearDepth();
		}

		_grid?.Render(c);
		RenderObjectSelection(c);
	}

	// this handles unspawned objects which might not have sizes.
	private Rectangle GetObjectBoundForEditor(GameObject2D obj)
	{
		return obj.Bounds;
	}
}