#region Using

using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.Graphics.Camera;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D
{
	public partial class Map2D
	{
		private UIController? _editUI;
		protected CameraBase? _editorLastGameCamera;
		protected WASDMoveCamera2D? _editorCamera;

		// Selection and MouseOver
		private bool _objectSelect = true;

		private GameObject2D? _rolloverObject
		{
			get => _rolloverIndex == -1 ? null : _allObjectsRollover![_rolloverIndex];
		}

		private List<GameObject2D>? _allObjectsRollover; // List of objects under the mouse cursor (could be more than one)
		private int _rolloverIndex = -1; // Index in ^

		private GameObject2D? _selectedObject; // An object that is also presented as selected, used to improve ui

		private GameObject2D? _objectDragging;
		private Vector2 _objectDragOffset;
		private bool _ignoreNextObjectDragLetGo;
		private Vector2 _objectDragStartPos;

		private static string _objectCopyClipboard; // todo: platform based clipboard?

		// Tile selection
		private bool _tileSelect = false;
		private Vector2 _tileBrush = Vector2.Zero;

		// UI stuff

		private void SetupDebug()
		{
			if (!Engine.Configuration.DebugMode) return;
			Engine.Host.OnKey.AddListener(DebugInputHandler, KeyListenerType.Editor);
		}

		private void DisposeDebug()
		{
			Engine.Host.OnKey.RemoveListener(DebugInputHandler);
		}

		private bool DebugInputHandler(Key key, KeyStatus status)
		{
			if (!Initialized) return true;

			if (key == Key.F3 && status == KeyStatus.Down)
			{
				if (EditorMode)
					ExitEditor();
				else
					EnterEditor();
			}

			if (!EditorMode) return true;

			if (key == Key.Z && status == KeyStatus.Down && Engine.Host.IsCtrlModifierHeld())
			{
				EditorUndoLastAction();
				return false;
			}

			if (key == Key.LeftAlt && status == KeyStatus.Down) RolloverObjectIncrement();

			bool leftClick = key == Key.MouseKeyLeft;
			bool rightClick = key == Key.MouseKeyRight;
			bool noMouseFocus = _editUI!.MouseFocus == _editUI || _editUI.MouseFocus == null || _editUI.MouseFocus is MapEditorObjectNameplate;

			if ((leftClick || rightClick) && status == KeyStatus.Down)
			{
				if (_rolloverObject != null)
				{
					ObjectSelect(_rolloverObject);
					if (rightClick)
					{
						if (noMouseFocus)
							EditorOpenContextMenuForObject(_rolloverObject);
					}
					else
					{
						_objectDragging = _rolloverObject;
						Vector2 mouseScreen = Engine.Host.MousePosition;
						Vector2 mouseWorld = Engine.Renderer.Camera.ScreenToWorld(mouseScreen).ToVec2();
						_objectDragOffset = _objectDragging.Position2 - mouseWorld;
						_objectDragStartPos = _objectDragging.Position2;
					}
				}
				else if (rightClick)
				{
					if (noMouseFocus)
						EditorOpenContextMenuObjectModeNoSelection();
				}
			}
			else if (leftClick && status == KeyStatus.Up)
			{
				if (_objectDragging != null)
				{
					if (!_ignoreNextObjectDragLetGo) _objectDragging = null;
					_ignoreNextObjectDragLetGo = false;
				}
			}

			_editorCamera?.CameraKeyHandler(key, status);

			return false;
		}

		private void EnterEditor()
		{
			_editUI = new UIController();
			_editUI.KeyPriority = KeyListenerType.EditorUI;

			UIBaseWindow topBar = GetEditorTopBar();
			_editUI.AddChild(topBar);

			UIBaseWindow worldInspect = GetWorldAttachInspectWindow();
			_editUI.AddChild(worldInspect);

			_editorLastGameCamera = Engine.Renderer.Camera;
			_editorCamera = new WASDMoveCamera2D(Vector3.Zero);
			Engine.Renderer.Camera = _editorCamera;
			Engine.Renderer.Camera.Position = _editorLastGameCamera.Position;

			// Reset setting
			EditorSetObjectSelect(true);

			EditorMode = true;
		}

		private void ExitEditor()
		{
			EditorMode = false;

			_editUI!.Dispose();
			_editUI = null;

			_namePlates?.Clear();

			_selectedObject = null;
			_rolloverIndex = -1;
			_allObjectsRollover = null;

			Engine.Renderer.Camera = _editorLastGameCamera;
		}

		protected void UpdateDebug()
		{
			if (!EditorMode) return;

			bool mouseInUI = _editUI?.MouseFocus != null && _editUI.MouseFocus != _editUI; // || _editUI?.InputFocus is UITextInput;
			var mouseFocusNameplate = _editUI?.MouseFocus as MapEditorObjectNameplate;
			bool mouseNotInUIOrInNameplate = !mouseInUI || mouseFocusNameplate != null;

			// Update objects that are rollovered.
			// If not currently selecting objects, or mouse is in UI, or dragging then dont.
			if (_objectDragging != null) RolloverObjects(new List<GameObject2D>(1) {_objectDragging});
			if (_objectSelect && mouseNotInUIOrInNameplate)
			{
				// Add objects (and their UI) under the mouse to the rollover list.
				Vector2 mouseScreen = Engine.Host.MousePosition;
				Vector2 mouseWorld = Engine.Renderer.Camera.ScreenToWorld(mouseScreen).ToVec2();
				var circle = new Circle(mouseWorld, 1);
				var results = new List<GameObject2D>();
				foreach (int treeLayerId in _worldTree!.ForEachLayer())
				{
					GetObjects(results, treeLayerId, circle, QueryFlags.Unique);
				}

				if (mouseFocusNameplate != null) results.Add(mouseFocusNameplate.Object);

				RolloverObjects(results);
			}
			else
			{
				RolloverObjects(null);
			}

			// If dragging an object - move it with the mouse.
			// If dragging into ui then dont move.
			if (_objectDragging != null && mouseNotInUIOrInNameplate)
			{
				Vector2 mouseScreen = Engine.Host.MousePosition;
				Vector2 mouseWorld = Engine.Renderer.Camera.ScreenToWorld(mouseScreen).ToVec2();
				_objectDragging.Position = (mouseWorld + _objectDragOffset).ToVec3(_objectDragging.Z);
				EditorRegisterMoveAction(_objectDragging, _objectDragStartPos, _objectDragging.Position2);
			}

			_editUI!.Update();
		}

		protected void RenderDebug(RenderComposer c)
		{
			if (!EditorMode) return;
			RenderState? prevState = c.CurrentState.Clone();

			c.SetUseViewMatrix(true);

			if (TileData != null)
			{
				Rectangle clipRect = c.Camera.GetCameraFrustum();
				for (var i = 0; i < TileData.Layers.Count; i++)
				{
					Map2DTileMapLayer layer = TileData.Layers[i];
					if (!layer.Visible) TileData.RenderLayer(c, i, clipRect);
				}

				c.ClearDepth();
			}

			if (_objectSelect)
			{
				// Show selection of object, if any.
				if (_editUI?.DropDown?.OwningObject is GameObject2D objectWithContextMenu)
				{
					Rectangle bound = objectWithContextMenu.Bounds;
					c.RenderSprite(bound, Color.White * 0.3f);
				}

				if (_selectedObject != null)
				{
					Rectangle bound = _selectedObject.Bounds;
					c.RenderSprite(bound, Color.White * 0.3f);
				}

				if (_rolloverObject != null)
				{
					Rectangle bound = _rolloverObject.Bounds;
					c.RenderSprite(bound, Color.White * 0.3f);
				}

				int count = GetObjectCount();
				for (var i = 0; i < count; i++)
				{
					GameObject2D obj = GetObjectByIndex(i);
					Rectangle bounds = obj.Bounds;

					if (!obj.ObjectFlags.HasFlag(ObjectFlags.Persistent))
					{
						c.RenderLine(bounds.TopLeft, bounds.BottomRight, Color.Black);
						c.RenderLine(bounds.TopRight, bounds.BottomLeft, Color.Black);
					}
					else if (obj.ObjectState == ObjectState.ConditionallyNonSpawned)
					{
						c.RenderSprite(bounds, Color.Magenta * 0.2f);
					}

					c.RenderOutline(bounds, Color.White * 0.4f);
				}
			}

			c.SetUseViewMatrix(false);
			c.SetDepthTest(false);
			_editUI!.Render(c);

			c.SetUseViewMatrix(true);
			c.SetDepthTest(true);

			c.SetState(prevState);
		}

		#region Object Selection

		public void ObjectSelect(GameObject2D? obj)
		{
			if (_selectedObject == obj) return;

			if (_namePlates != null && _selectedObject != null)
			{
				_namePlates.TryGetValue(_selectedObject, out MapEditorObjectNameplate? oldNamePlate);
				oldNamePlate?.SetSelected(false, "ObjectSelection");
			}

			if (obj == null)
			{
				_selectedObject = null;
				return;
			}

			_selectedObject = obj;

			if (_namePlates != null)
			{
				_namePlates.TryGetValue(obj, out MapEditorObjectNameplate? namePlate);
				namePlate?.SetSelected(true, "ObjectSelection");
			}
		}

		#endregion

		#region Helpers

		// this handles unspawned objects which might not have sizes.
		private Rectangle GetObjectBoundForEditor(GameObject2D obj)
		{
			return obj.Bounds;
		}

		private int ObjectSort(GameObject2D x, GameObject2D y)
		{
			return MathF.Sign(x.Position.Z - y.Position.Z);
		}

		private void RolloverObjectIncrement()
		{
			if (_allObjectsRollover == null) return;
			RolloverObjects(_allObjectsRollover, true);
		}

		private void RolloverObjects(List<GameObject2D>? objs, bool incrementSel = false)
		{
			if (objs?.Count == 0) objs = null;
			objs?.Sort(ObjectSort);

			var rolloverChanged = false;

			// Currently rollovered something, check if its the same.
			if (_allObjectsRollover != null)
			{
				if (objs == null || objs.Count != _allObjectsRollover.Count) // none now or different count
				{
					rolloverChanged = true;
				}
				else
				{
					// Check if all the objects are the same (note: they should be in the same order due to same sort)
					var allSame = true;
					for (var i = 0; i < objs.Count; i++)
					{
						GameObject2D obj = objs[i];
						GameObject2D other = _allObjectsRollover[i];
						if (obj != other)
						{
							allSame = false;
							break;
						}
					}

					rolloverChanged = !allSame;
				}
			}
			else if (objs != null)
			{
				rolloverChanged = true;
			}

			if (!rolloverChanged && !incrementSel) return;

			// Update index of current if still here.
			GameObject2D? prevRolloverObj = _allObjectsRollover?[_rolloverIndex];
			var currentObjectChanged = true;
			if (prevRolloverObj != null && objs != null)
			{
				Debug.Assert(_allObjectsRollover != null);
				Debug.Assert(prevRolloverObj == _allObjectsRollover[_rolloverIndex]);

				int currentIdxInNew = objs.IndexOf(prevRolloverObj);
				if (currentIdxInNew != -1)
				{
					_rolloverIndex = currentIdxInNew;
					currentObjectChanged = false;
				}
			}

			// Update current object highlights
			if (currentObjectChanged || incrementSel)
			{
				// Check if none now
				GameObject2D? newRolloverObj;
				if (objs == null)
				{
					_rolloverIndex = -1;
					newRolloverObj = null;
				}
				else if (incrementSel)
				{
					_rolloverIndex++;
					if (_rolloverIndex > objs.Count - 1) _rolloverIndex = 0;
					newRolloverObj = objs[_rolloverIndex];
				}
				else // Completely different, so change current.
				{
					_rolloverIndex = 0;
					newRolloverObj = objs[0];
				}

				if (prevRolloverObj != null)
					if (_namePlates != null)
					{
						_namePlates.TryGetValue(prevRolloverObj, out MapEditorObjectNameplate? namePlate);
						namePlate?.SetSelected(false, "Rollover");
					}

				if (newRolloverObj != null)
					if (_namePlates != null)
					{
						_namePlates.TryGetValue(newRolloverObj, out MapEditorObjectNameplate? namePlate);
						namePlate?.SetSelected(true, "Rollover");
					}
			}

			_allObjectsRollover = objs;

			// Update rollover
			var worldAttachUI = (UIWorldAttachedWindow?) _editUI?.GetWindowById("WorldAttach");
			if (_rolloverObject == null)
			{
				if (worldAttachUI != null) worldAttachUI.Visible = false;
				return;
			}

			if (worldAttachUI != null)
			{
				worldAttachUI.Visible = true;

				GameObject2D? obj = _rolloverObject;
				var text = (UIText?) worldAttachUI.GetWindowById("text")!;
				var txt = new StringBuilder();
				txt.AppendLine($"Name: {obj.ObjectName ?? "null"}");
				txt.AppendLine($"   Type: {obj.GetType().Name}");

				if (_allObjectsRollover != null && _allObjectsRollover.Count > 0) txt.AppendLine($"Objects Here: {_rolloverIndex + 1}/{_allObjectsRollover.Count} [ALT] to switch");

				text.Text = txt.ToString();

				Rectangle bounds = GetObjectBoundForEditor(obj);
				Vector2 attachPoint = bounds.TopRight;
				worldAttachUI.AttachToPosition(attachPoint.ToVec3());
			}
		}

		public async Task EditorSaveMap()
		{
			string? fileName = FileName;
			if (fileName == null)
			{
				Engine.Log.Warning("Map is missing file name.", "Map2D");
				return;
			}

			for (var i = 0; i < PersistentObjects.Count; i++)
			{
				GameObject2D obj = PersistentObjects[i];
				obj.TrimPropertiesForSerialize();
			}

			// Unload the preset in the asset loader cache if loaded. This allows for changes to be observed on re-get.
			// This won't break anything as XMLAsset doesn't perform any cleanup.
			if (Engine.AssetLoader.Loaded(fileName)) Engine.AssetLoader.Destroy(fileName);

			XMLAsset<Map2D>? asset = XMLAsset<Map2D>.CreateFromContent(this, fileName);
			asset.Save();

			await Reset(); // Regenerate trimmed properties
		}

		#endregion

		#region Editor World UI

		private Dictionary<GameObject2D, MapEditorObjectNameplate>? _namePlates;

		private void EnsureObjectNameplate(GameObject2D obj)
		{
			_namePlates ??= new Dictionary<GameObject2D, MapEditorObjectNameplate>();
			if (_namePlates.ContainsKey(obj)) return;

			var namePlate = new MapEditorObjectNameplate();
			namePlate.AttachToObject(obj);
			_namePlates.Add(obj, namePlate);
			_editUI!.AddChild(namePlate);
		}

		private void EditorSetObjectSelect(bool val)
		{
			_objectSelect = val;
			if (!_objectSelect)
			{
				foreach (KeyValuePair<GameObject2D, MapEditorObjectNameplate> namePlate in _namePlates)
				{
					_editUI?.RemoveChild(namePlate.Value);
				}

				_namePlates.Clear();
			}
			else
			{
				foreach (GameObject2D obj in GetObjects())
				{
					EnsureObjectNameplate(obj);
				}
			}
		}

		#endregion

		#region Object Control API

		private void EditorAddObject(Type type)
		{
			Vector2 pos = Engine.Host.MousePosition;
			Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(pos).ToVec2();

			ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes)!;
			var newObj = (GameObject2D) constructor.Invoke(null);
			newObj.ObjectFlags |= ObjectFlags.Persistent;
			newObj.Position = worldPos.ToVec3();
			AddObject(newObj);

			// Stick to mouse to be placed.
			_objectDragging = newObj;
			_objectDragOffset = newObj.Size / 2f;
			_ignoreNextObjectDragLetGo = true;
		}

		private void EditorObjectAdded(GameObject2D newObj)
		{
			EnsureObjectNameplate(newObj);
		}

		private void EditorObjectRemoved(GameObject2D oldObj)
		{
			if (_namePlates != null && _namePlates.TryGetValue(oldObj, out MapEditorObjectNameplate? nameplate))
			{
				_namePlates.Remove(oldObj);
				nameplate.Parent!.RemoveChild(nameplate);
			}

			MapEditorObjectPropertiesPanel? propPanelOpen = EditorGetAlreadyOpenPropertiesPanelForObject(oldObj.UniqueId);
			propPanelOpen?.InvalidateObjectReference();
		}

		#endregion
	}
}