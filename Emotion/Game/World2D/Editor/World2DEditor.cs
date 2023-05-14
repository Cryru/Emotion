#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Game.World2D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D;

public partial class World2DEditor
{
	public Map2D CurrentMap { get; protected set; }
	public bool EditorOpen { get; protected set; }

	protected IWorld2DAwareScene _scene;
	protected WASDMoveCamera2D? _editorCamera;
	protected CameraBase? _cameraOutsideEditor;
	protected UIController? _editUI;

	public World2DEditor(IWorld2DAwareScene scene)
	{
		_scene = scene;
	}

	public void InitializeEditor()
	{
		if (!Engine.Configuration.DebugMode) return;
		Engine.Host.OnKey.AddListener(DebugInputHandler, KeyListenerType.Editor);
	}

	public void UnloadEditor()
	{
		Engine.Host.OnKey.RemoveListener(DebugInputHandler);
	}

	public void EnterEditor()
	{
		CheckMapChange();
		if (CurrentMap == null!) return;

		_cameraOutsideEditor = Engine.Renderer.Camera;
		_editorCamera = new WASDMoveCamera2D(Vector3.Zero);
		Engine.Renderer.Camera = _editorCamera;
		Engine.Renderer.Camera.Position = _cameraOutsideEditor.Position;

		InitializeEditorInterface();
		InitializeObjectEditor();

		EditorOpen = true;
		CurrentMap.EditorMode = true;
	}

	public void ExitEditor()
	{
		Engine.Renderer.Camera = _cameraOutsideEditor;
		_editorCamera = null;

		DisposeEditorInterface();
		DisposeObjectEditor();

		EditorOpen = false;
		CurrentMap.EditorMode = false;
	}

	protected void CheckMapChange()
	{
		Map2D currentMap = _scene.GetCurrentMap();
		if (currentMap != CurrentMap)
		{
			CurrentMap = currentMap;
			currentMap.EditorMode = EditorOpen;
			MapChanged(CurrentMap, currentMap);
		}
	}

	protected void MapChanged(Map2D? oldMap, Map2D newMap)
	{
		ObjectEditorMapChanged(oldMap, newMap);
	}

	public void Render(RenderComposer c)
	{
		if (!EditorOpen) return;

		RenderState? prevState = c.CurrentState.Clone();
		c.SetUseViewMatrix(true);

		Map2D map = CurrentMap;

		// Render invisible tile layers
		// todo: move to tile editor
		if (map.TileData != null)
		{
			Rectangle clipRect = c.Camera.GetCameraFrustum();
			for (var i = 0; i < map.TileData.Layers.Count; i++)
			{
				Map2DTileMapLayer layer = map.TileData.Layers[i];
				if (!layer.Visible) map.TileData.RenderLayer(c, i, clipRect);
			}

			c.ClearDepth();
		}

		RenderObjectSelection(c);

		c.SetUseViewMatrix(false);
		c.SetDepthTest(false);
		_editUI!.Render(c);

		c.SetUseViewMatrix(true);
		c.SetDepthTest(true);

		c.SetState(prevState);
	}

	public void Update(float dt)
	{
		if (!EditorOpen) return;

		CheckMapChange();
		// todo: check camera change
		UpdateObjectEditor();
		_editUI?.Update();
	}

	private bool DebugInputHandler(Key key, KeyStatus status)
	{
		if (key == Key.F3 && status == KeyStatus.Down)
		{
			if (EditorOpen)
				ExitEditor();
			else
				EnterEditor();
		}

		if (!EditorOpen) return true;
		_editorCamera?.CameraKeyHandler(key, status);

		if (key == Key.Z && status == KeyStatus.Down && Engine.Host.IsCtrlModifierHeld())
		{
			EditorUndoLastAction();
			return false;
		}

		ObjectEditorInputHandler(key, status);

		return false;
	}

	// this handles unspawned objects which might not have sizes.
	private Rectangle GetObjectBoundForEditor(GameObject2D obj)
	{
		return obj.Bounds;
	}

	private int ObjectSort(GameObject2D x, GameObject2D y)
	{
		return MathF.Sign(x.Position.Z - y.Position.Z);
	}

	public async Task EditorSaveMap(Map2D? map = null)
	{
		map ??= CurrentMap;

		string? fileName = map.FileName;
		if (fileName == null)
		{
			Engine.Log.Warning("Map is missing file name.", "Map2D");
			return;
		}

		for (var i = 0; i < map.PersistentObjects.Count; i++)
		{
			GameObject2D obj = map.PersistentObjects[i];
			obj.TrimPropertiesForSerialize();
		}

		// Unload the preset in the asset loader cache if loaded. This allows for changes to be observed on re-get.
		// This won't break anything as XMLAsset doesn't perform any cleanup.
		if (Engine.AssetLoader.Loaded(fileName)) Engine.AssetLoader.Destroy(fileName);

		XMLAsset<Map2D>? asset = XMLAsset<Map2D>.CreateFromContent(map, fileName);
		asset.Save();

		await map.Reset(); // Regenerate trimmed properties
	}
}