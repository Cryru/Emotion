#nullable enable

#region Using

using Emotion.Common.Threading;
using Emotion.Game.World2D;
using Emotion.Game.World2D.SceneControl;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

namespace Emotion.Game.World.Editor;

public abstract class WorldBaseEditorGeneric<T> : WorldBaseEditor where T : Map2D
{
	public T? CurrentMape { get; protected set; }

	protected WorldBaseEditorGeneric(IWorld2DAwareScene scene, Type mapType) : base(scene, mapType)
	{
	}
}

public abstract partial class WorldBaseEditor
{
	public Map2D? CurrentMap { get; protected set; }

	public object? CurrentMapObject { get; protected set; }
	public bool EditorOpen { get; protected set; }

	protected IWorld2DAwareScene _scene;
	protected Type _mapType;
	protected CameraBase? _editorCamera;
	protected CameraBase? _cameraOutsideEditor;

	protected UIController? _editUI;
	protected UIController? _editorUIAlways;
	protected List<UIController>? _setControllersToVisible;

	protected WorldBaseEditor(IWorld2DAwareScene scene, Type mapType)
	{
		_scene = scene;
		_mapType = mapType;
		Engine.AssetLoader.GetAsync<FontAsset>(FontAsset.DefaultBuiltInFontName);
	}

	public void InitializeEditor()
	{
		if (!Engine.Configuration.DebugMode) return;
		Engine.Host.OnKey.AddListener(DebugInputHandler, KeyListenerType.Editor);
		_editorUIAlways = new UIController(KeyListenerType.EditorUI)
		{
			Id = "WorldEditor_AlwaysOnTop"
		};
	}

	public void UnloadEditor()
	{
		Engine.Host.OnKey.RemoveListener(DebugInputHandler);
	}

	public void EnterEditor()
	{
		CurrentMap = _scene.GetCurrentMap();

		_cameraOutsideEditor = Engine.Renderer.Camera;
		_editorCamera = GetEditorCamera();
		Engine.Renderer.Camera = _editorCamera;
		Engine.Renderer.Camera.Position = _cameraOutsideEditor.Position;

		EnterEditorInternal();

		EditorOpen = true;
		if (CurrentMap != null)
		{
			CurrentMap.OnMapReset += OnMapReset;
			CurrentMap.EditorMode = true;
		}
	}

	public void ExitEditor()
	{
		// We want to sync editor closing with the rendering to
		// prevent race condition exceptions.
		if (!GLThread.IsGLThread())
		{
			GLThread.ExecuteGLThread(ExitEditor);
			return;
		}

		Engine.Renderer.Camera = _cameraOutsideEditor;
		_editorCamera = null;

		ExitEditorInternal();

		EditorOpen = false;
		if (CurrentMap != null)
		{
			CurrentMap.OnMapReset -= OnMapReset;
			CurrentMap.EditorMode = false;
		}
	}

	protected void CheckMapChange()
	{
		Map2D currentMap = _scene.GetCurrentMap();
		if (currentMap != CurrentMap)
		{
			if (EditorOpen) ExitEditor();
			CurrentMap = currentMap;
			EnterEditor();
		}
	}

	public void ChangeSceneMap(Map2D newMap)
	{
		ExitEditor();
		_scene.ChangeMapAsync(newMap).Wait();
		CheckMapChange();
	}

	public void ChangeSceneMap(string fileName)
	{
		var newMapAsset = Engine.AssetLoader.Get<XMLAsset<Map2D>>(fileName, false);
		Map2D? newMap = newMapAsset?.Content;
		if (newMap == null) return;
		newMap.FileName = fileName;

		ChangeSceneMap(newMap);
	}

	private void OnMapReset()
	{
		// Restart the editor without changing the camera.
		_cameraOutsideEditor = Engine.Renderer.Camera;
		Vector3 cameraPos = _editorCamera!.Position;
		ExitEditor();
		EnterEditor();
		Engine.Renderer.Camera.Position = cameraPos;
	}

	public void Render(RenderComposer c)
	{
		if (!EditorOpen)
		{
			if (_editorUIAlways != null)
			{
				RenderState? stateBefore = c.CurrentState.Clone();
				c.SetUseViewMatrix(false);
				c.SetDepthTest(false);
				_editorUIAlways!.Render(c);
				c.SetState(stateBefore);
			}

			return;
		}

		RenderState? prevState = c.CurrentState.Clone();

		RenderInternal(c);

		c.SetUseViewMatrix(false);
		c.SetDepthTest(false);

		_editUI!.Render(c);
		_editorUIAlways!.Render(c);

		c.SetState(prevState);
	}

	public void Update(float dt)
	{
		_editorUIAlways?.Update();
		if (!EditorOpen) return;

		CheckMapChange();
		// todo: check camera change
		UpdateInternal(dt);
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

		// todo: remove, add priority to camera attach, add camera attach to wasd camera
		if (_editorCamera is WASDMoveCamera2D camera2D)
			camera2D.CameraKeyHandler(key, status);

		if (key == Key.Z && status == KeyStatus.Down && Engine.Host.IsCtrlModifierHeld())
		{
			EditorUndoLastAction();
			return false;
		}

		EditorInputHandlerInternal(key, status);

		return false;
	}

	public void EditorSaveMap(Map2D? map = null)
	{
		map ??= CurrentMap;
		if (map == null) return;

		string? fileName = map.FileName;
		if (fileName == null)
		{
			Engine.Log.Warning("Map is missing file name.", "Map2D");
			return;
		}

		// Unload the preset in the asset loader cache if loaded. This allows for changes to be observed on re-get.
		// This won't break anything as XMLAsset doesn't perform any cleanup.
		if (Engine.AssetLoader.Loaded(fileName)) Engine.AssetLoader.Destroy(fileName);

		XMLAsset<Map2D> asset = XMLAsset<Map2D>.CreateFromContent(map, fileName);
		bool saved = asset.Save();
		EditorMsg(saved ? "Map saved." : "Unable to save map.");
	}

	public void EditorMsg(string txt)
	{
		Engine.Log.Trace(txt, "World2DEditor");

		// todo
	}

	#region Internal API

	protected abstract CameraBase GetEditorCamera();
	protected abstract void EnterEditorInternal();
	protected abstract void ExitEditorInternal();
	protected abstract void EditorInputHandlerInternal(Key key, KeyStatus status);
	protected abstract void UpdateInternal(float dt);
	protected abstract void RenderInternal(RenderComposer c);

	#endregion
}