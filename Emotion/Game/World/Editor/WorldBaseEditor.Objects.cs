#nullable enable

#region Using

using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.World2D;
using Emotion.Game.World2D.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Implementation.Win32;
using Emotion.Platform.Input;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.UI;
using Emotion.Utility;

#endregion

namespace Emotion.Game.World.Editor;

public abstract partial class WorldBaseEditor
{
	protected BaseGameObject? _rolloverObject
	{
		get => _rolloverIndex == -1 ? null : _allObjectsRollover![_rolloverIndex];
	}

	protected List<BaseGameObject>? _allObjectsRollover; // List of objects under the mouse cursor (could be more than one)
	protected int _rolloverIndex = -1; // Index in ^
	protected BaseGameObject? _selectedObject; // An object that is also presented as selected, used to improve ui

	// Dragging
	protected BaseGameObject? _objectDragging;
	protected Vector2 _objectDragOffset;
	protected Vector2 _objectDragStartPos;

	protected virtual void InitializeObjectEditor()
	{
		if (CurrentMap != null)
		{
			CurrentMap.OnObjectAdded += EditorObjectAdded;
			CurrentMap.OnObjectRemoved += EditorObjectRemoved;
		}

		SetObjectSelectionEnabled(true);
		LoadPrefabs();
	}

	protected virtual void DisposeObjectEditor()
	{
		if (CurrentMap != null)
		{
			CurrentMap.OnObjectAdded -= EditorObjectAdded;
			CurrentMap.OnObjectRemoved -= EditorObjectRemoved;
		}

		_namePlates?.Clear();

		_selectedObject = null;
		_rolloverIndex = -1;
		_allObjectsRollover = null;
	}

	protected virtual void UpdateObjectEditor()
	{
		BaseMap? map = CurrentMap;
		if (map == null) return;

		bool mouseInUI = UIController.MouseFocus != null && UIController.MouseFocus != _editUI; // || _editUI?.InputFocus is UITextInput;
		var mouseFocusNameplate = UIController.MouseFocus as MapEditorObjectNameplate;
		bool mouseNotInUIOrInNameplate = !mouseInUI || mouseFocusNameplate != null;

		// Update objects that are rollovered.
		// ----

		if (_objectDragging != null)
		{
			RolloverObjects(null);
		}
		// If not currently selecting objects, or mouse is in UI, or dragging then dont.
		else if (_canObjectSelect && mouseNotInUIOrInNameplate)
		{
			// Add objects (and their UI) under the mouse to the rollover list.
			Vector2 mouseScreen = Engine.Host.MousePosition;
			Vector2 mouseWorld = Engine.Renderer.Camera.ScreenToWorld(mouseScreen).ToVec2();
			var circle = new Circle(mouseWorld, 1);
			var results = new List<BaseGameObject>();
			foreach (int treeLayerId in map.GetWorldTree()!.ForEachLayer())
			{
				map.GetObjects(results, treeLayerId, circle, QueryFlags.Unique);
			}

			if (mouseFocusNameplate != null) results.Add(mouseFocusNameplate.Object);

			RolloverObjects(results);
		}
		else
		{
			var removeRollover = true;
			if (UIController.MouseFocus != null)
			{
				UIBaseWindow? objectListPanel = _editUI!.GetWindowById("ObjectListPanel");
				if (objectListPanel != null && UIController.MouseFocus.IsWithin(objectListPanel)) removeRollover = false;
			}

			if (removeRollover) RolloverObjects(null);
		}

		// If dragging an object - move it with the mouse.
		// If dragging into ui then dont move.
		if (_objectDragging != null && mouseNotInUIOrInNameplate)
		{
			Vector2 mouseScreen = Engine.Host.MousePosition;
			Vector2 mouseWorld = Engine.Renderer.Camera.ScreenToWorld(mouseScreen).ToVec2();

			Vector2 newPos = mouseWorld + _objectDragOffset;
			newPos *= 100f;
			newPos = newPos.Floor();
			newPos /= 100f;

			// todo: move to editor generic grid
			if (map is Map2D map2d)
				if (Engine.Host.IsCtrlModifierHeld() && map2d.TileData != null)
				{
					Vector2 tileSize = map2d.TileData.TileSize;
					newPos /= tileSize;
					newPos = newPos.Floor();
					newPos *= tileSize;
				}

			_objectDragging.Position = newPos.ToVec3(_objectDragging.Z);
			EditorRegisterMoveAction(_objectDragging, _objectDragStartPos, _objectDragging.Position2);
		}
	}

	/// <summary>
	/// Changes the object property and reinitializes the object without changing its reference.
	/// </summary>
	public void ChangeObjectProperty(BaseGameObject obj, XMLFieldHandler field, object? value, bool recordUndo = true)
	{
		BaseMap? objectMap = obj.Map;
		AssertNotNull(objectMap);
		Assert(objectMap == CurrentMap);
		objectMap = CurrentMap;
		if (objectMap == null) return;

		// Register action for undo.
		if (recordUndo)
		{
			object? oldValue = field.ReflectionInfo!.GetValue(obj);
			EditorRegisterObjectMutateAction(obj, field, oldValue);
		}

		// Clean the object of unserialized properties.
		// This basically brings it in line to how it will look when loaded the first time.
		bool isPersist = obj.ObjectFlags.HasFlag(ObjectFlags.Persistent);
		int id = obj.UniqueId;
		obj.Destroy(); // Make sure object is correctly cleaned up as it will be reinitialized.
		EditorUtility.SetObjectToSerializationDefault<BaseGameObject>(obj);
		obj.UniqueId = id;
		if (isPersist) obj.ObjectFlags |= ObjectFlags.Persistent;

		// Set the new value.
		field.ReflectionInfo.SetValue(obj, value);

		// Reinitialize the object, calling Init etc. as it is basically new.
		objectMap.Editor_ReinitializeObject(obj);
	}

	private void EditorObjectAdded(BaseGameObject newObj)
	{
		EnsureObjectNameplate(newObj);
	}

	private void EditorObjectRemoved(BaseGameObject oldObj)
	{
		if (_namePlates != null && _namePlates.TryGetValue(oldObj, out MapEditorObjectNameplate? nameplate))
		{
			_namePlates.Remove(oldObj);
			nameplate.Parent!.RemoveChild(nameplate);
		}

		MapEditorObjectPropertiesPanel? propPanelOpen = EditorGetAlreadyOpenPropertiesPanelForObject(oldObj.UniqueId);
		propPanelOpen?.Close();
	}

	protected string GetObjectSerialized(BaseGameObject obj)
	{
		return XMLFormat.To(obj) ?? "";
	}

	private void SetObjectSelectionEnabled(bool val)
	{
		_canObjectSelect = val;
		if (!_canObjectSelect)
		{
			if (_namePlates == null) return;
			foreach (KeyValuePair<BaseGameObject, MapEditorObjectNameplate> namePlate in _namePlates)
			{
				_editUI?.RemoveChild(namePlate.Value);
			}

			_namePlates.Clear();
		}
		else
		{
			_namePlates ??= new Dictionary<BaseGameObject, MapEditorObjectNameplate>();

			if (CurrentMap != null)
				foreach (BaseGameObject obj in CurrentMap.GetObjects(true))
				{
					EnsureObjectNameplate(obj);
				}
		}
	}

	protected void EnsureObjectNameplate(BaseGameObject obj)
	{
		if (_namePlates == null || _namePlates.ContainsKey(obj)) return;

		var namePlate = new MapEditorObjectNameplate();
		namePlate.AttachToObject(obj);
		_namePlates.Add(obj, namePlate);
		_editUI!.AddChild(namePlate);
	}

	private void EditorAddObject(Type type)
	{
		BaseMap? map = CurrentMap;
		if (map == null) return;

		Vector2 pos = Engine.Host.MousePosition;
		Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(pos).ToVec2();

		ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes)!;
		var newObj = (BaseGameObject) constructor.Invoke(null);
		newObj.ObjectFlags |= ObjectFlags.Persistent;
		newObj.Position = worldPos.ToVec3();

		map.AddObject(newObj);
		SelectObject(newObj);

		// Stick to mouse to be placed.
		_objectDragging = newObj;
		_objectDragOffset = newObj.Size / 2f;
	}

	protected virtual void RenderObjectSelection(RenderComposer c)
	{
		// nop
	}

	private void ObjectEditorInputHandler(Key key, KeyStatus status)
	{
		if (key == Key.LeftAlt && status == KeyStatus.Down) RolloverObjectIncrement();

		bool leftClick = key == Key.MouseKeyLeft;
		bool rightClick = key == Key.MouseKeyRight;
		bool noMouseFocus = UIController.MouseFocus == _editUI || UIController.MouseFocus == null || UIController.MouseFocus is MapEditorObjectNameplate;

		if ((leftClick || rightClick) && status == KeyStatus.Down)
		{
			if (_rolloverObject != null)
			{
				SelectObject(_rolloverObject);
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
			_objectDragging = null;
		}
	}

	#region Object Selection and Rollover

	public virtual void SelectObject(BaseGameObject? obj)
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

	private void RolloverObjects(List<BaseGameObject>? objs, bool incrementSel = false)
	{
		if (objs?.Count == 0) objs = null;
		objs?.Sort(Map2D.ObjectComparison);
		objs?.Reverse();

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
					BaseGameObject obj = objs[i];
					BaseGameObject other = _allObjectsRollover[i];
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
		BaseGameObject? prevRolloverObj = _allObjectsRollover?[_rolloverIndex];
		var currentObjectChanged = true;
		if (prevRolloverObj != null && objs != null && _allObjectsRollover != null && objs[0] == _allObjectsRollover[0])
		{
			AssertNotNull(_allObjectsRollover);
			Assert(prevRolloverObj == _allObjectsRollover[_rolloverIndex]);

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
			BaseGameObject? newRolloverObj;
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

			BaseGameObject? obj = _rolloverObject;
			var text = (UIText?) worldAttachUI.GetWindowById("text")!;
			var txt = new StringBuilder();
			txt.AppendLine($"Name: [{obj.UniqueId}] {obj.ObjectName ?? "null"}");
			txt.AppendLine($"Type: {obj.GetType().Name}");
			txt.AppendLine($"Pos: {obj.Position}");
			if (_allObjectsRollover != null && _allObjectsRollover.Count > 0) txt.AppendLine($"Objects Here: {_rolloverIndex + 1}/{_allObjectsRollover.Count} [ALT] to switch");

			text.Text = txt.ToString();

			Rectangle bounds = obj.Bounds;
			Vector2 attachPoint = bounds.TopRight;
			worldAttachUI.AttachToPosition(attachPoint.ToVec3() + new Vector3(5, 0, 0));
		}
	}

	protected void RolloverObjectIncrement()
	{
		if (_allObjectsRollover == null) return;
		RolloverObjects(_allObjectsRollover, true);
	}

	#endregion

	#region Prefab System

	protected static Dictionary<string, GameObjectPrefab> _prefabDatabase = new();
	protected const string OBJECT_PREFAB_FOLDER_ASSETS = "EditorGame/Prefabs";

	private static void LoadPrefabs()
	{
		if (_prefabDatabase.Count != 0) return; // Already loaded.

		string[] prefabs = Engine.AssetLoader.GetAssetsInFolder(OBJECT_PREFAB_FOLDER_ASSETS);
		for (var i = 0; i < prefabs.Length; i++)
		{
			string prefabPath = prefabs[i];
			Task.Run(() =>
			{
				var prefabAsset = Engine.AssetLoader.Get<XMLAsset<GameObjectPrefab>>(prefabPath, false);
				if (prefabAsset?.Content == null) return;

				string prefabName = prefabAsset.Content.PrefabName;
				lock (_prefabDatabase)
				{
					string originalName = prefabName;
					var num = 1;
					while (_prefabDatabase.ContainsKey(prefabName))
					{
						prefabName = $"{originalName}_{num}";
						num++;
					}

					_prefabDatabase.Add(prefabName, prefabAsset.Content);
				}
			});
		}
	}

	protected void CreateObjectPrefab(string prefabName, BaseGameObject obj)
	{
		// Check if overwriting existing prefab.
		GameObjectPrefab prefabData;
		if (_prefabDatabase.TryGetValue(prefabName, out GameObjectPrefab? existingPrefab))
		{
			prefabData = existingPrefab;
			prefabData.PrefabVersion++;
		}
		else
		{
			prefabData = new GameObjectPrefab
			{
				PrefabName = prefabName,
				PrefabVersion = 1
			};
		}

		// Strip old prefab origin from serialization.
		GameObjectPrefabOriginData? prefabOrigin = obj.PrefabOrigin;
		obj.PrefabOrigin = null;

		string objData = GetObjectSerialized(obj);
		prefabData.ObjectData = objData;

		obj.PrefabOrigin = prefabOrigin;

		// Fill properties
		prefabData.DefaultProperties ??= new List<Dictionary<string, object?>>();
		prefabData.DefaultProperties.Add(new Dictionary<string, object?>());
		Assert(prefabData.DefaultProperties.Count == prefabData.PrefabVersion);
		while (prefabData.DefaultProperties.Count < prefabData.PrefabVersion) // Fill until we reach it?
			prefabData.DefaultProperties.Add(new Dictionary<string, object?>());
		Dictionary<string, object?> thisVersionPropertyList = prefabData.DefaultProperties[prefabData.PrefabVersion - 1];

		var typeHandler = (XMLComplexBaseTypeHandler) XMLHelpers.GetTypeHandler(obj.GetType())!;
		IEnumerator<XMLFieldHandler> fields = typeHandler.EnumFields();
		while (fields.MoveNext())
		{
			XMLFieldHandler field = fields.Current;
			if (ShouldIgnorePrefabProperty(field.Name)) continue; // We expect these to be always different.

			object? valueInProp = field.ReflectionInfo.GetValue(obj);

			// ObjectFlags doesn't persist some values.
			if (field.TypeHandler is XMLEnumTypeHandler enumHandler) valueInProp = enumHandler.StripDontSerializeValues(valueInProp);

			if (!field.Skip && !Helpers.AreObjectsEqual(valueInProp, field.DefaultValue)) thisVersionPropertyList.Add(field.Name, valueInProp);
		}

		string filePath = AssetLoader.MakeStringPathSafe(prefabName);
		filePath = $"{OBJECT_PREFAB_FOLDER_ASSETS}/{filePath}.xml";

		XMLAsset<GameObjectPrefab>.CreateFromContent(prefabData).SaveAs(filePath);
		_prefabDatabase[prefabName] = prefabData;
	}

	protected void PlaceObjectFromPrefab(GameObjectPrefab prefab)
	{
		BaseMap? map = CurrentMap;
		if (map == null) return;

		Vector2 pos = Engine.Host.MousePosition;
		Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(pos).ToVec2();

		var newObj = XMLFormat.From<BaseGameObject>(prefab.ObjectData);
		if (newObj == null) return;

		newObj.ObjectFlags |= ObjectFlags.Persistent;
		newObj.Position = worldPos.ToVec3();
		newObj.PrefabOrigin = new GameObjectPrefabOriginData(prefab);

		map.AddObject(newObj);
		SelectObject(newObj);

		// Stick to mouse to be placed.
		_objectDragging = newObj;
		_objectDragOffset = newObj.Size / 2f;
	}

	public bool IsPropertyDifferentFromPrefab(BaseGameObject obj, XMLFieldHandler field)
	{
		if (obj.PrefabOrigin == null) return false;
		GameObjectPrefabOriginData? prefabOrigin = obj.PrefabOrigin;
		if (!_prefabDatabase.TryGetValue(prefabOrigin.PrefabName, out GameObjectPrefab? prefab)) return false;
		if (prefab.DefaultProperties == null || prefab.DefaultProperties.Count < prefabOrigin.PrefabVersion) return false;
		if (ShouldIgnorePrefabProperty(field.Name, true)) return false;

		Dictionary<string, object?> versionProperties = prefab.DefaultProperties[prefabOrigin.PrefabVersion - 1];
		object? objectVal = field.ReflectionInfo.GetValue(obj);

		// ObjectFlags doesn't persist some values.
		if (field.TypeHandler is XMLEnumTypeHandler enumHandler) objectVal = enumHandler.StripDontSerializeValues(objectVal);

		bool isDefault = Helpers.AreObjectsEqual(field.DefaultValue, objectVal);

		// If it's missing that means the prefab has it set to the default value for the class.
		if (versionProperties.TryGetValue(field.Name, out object? val))
			return !Helpers.AreObjectsEqual(val, objectVal);

		return !isDefault;
	}

	protected void UpdateObjectToLatestPrefabVersion(BaseGameObject obj)
	{
	}

	private bool ShouldIgnorePrefabProperty(string name, bool checkingForDiff = false)
	{
		// When checking for diffs dont show diffs in these properties
		// as they are expected to be customized.
		if (checkingForDiff && name is "ObjectName") return true;

		// We expect these to be different
		return name is "Position" or "Center" or "PrefabOrigin";
	}

	#endregion

	#region Interface

	private MapEditorObjectPropertiesPanel? EditorGetAlreadyOpenPropertiesPanelForObject(int objectUid)
	{
		List<UIBaseWindow>? children = _editUI!.Children!;
		for (var i = 0; i < children.Count; i++)
		{
			UIBaseWindow child = children[i];
			if (child is MapEditorObjectPropertiesPanel propPane && propPane.ObjectUId == objectUid)
				return propPane;
		}

		return null;
	}

	private void EditorOpenPropertiesPanelForObject(BaseGameObject obj)
	{
		AssertNotNull(obj);
		AssertNotNull(_editUI);

		MapEditorObjectPropertiesPanel? existingPanel = EditorGetAlreadyOpenPropertiesPanelForObject(obj.UniqueId);
		if (existingPanel != null)
		{
			_editUI.SetInputFocus(existingPanel);
			return;
		}

		_editUI.AddChild(new MapEditorObjectPropertiesPanel(this, obj));
	}

	private string GetClipboard()
	{
		var clipBoard = "";
		if (Engine.Host is Win32Platform winPlat) clipBoard = winPlat.GetClipboard();
		return clipBoard;
	}

	private void EditorOpenContextMenuObjectModeNoSelection()
	{
		BaseMap? map = CurrentMap;
		if (map == null) return;

		Vector2 mousePos = Engine.Host.MousePosition;

		var contextMenu = new EditorDropdown(true)
		{
			Offset = mousePos / _editUI!.GetScale()
		};

		var dropDownMenu = new[]
		{
			new EditorDropDownButtonDescription
			{
				Name = "Paste",
				Click = (_, __) =>
				{
					string clipBoard = GetClipboard();
					AssertNotNull(clipBoard);

					var newObj = XMLFormat.From<GameObject2D>(clipBoard);
					if (newObj == null)
					{
						EditorMsg("Couldn't paste object.");
						return;
					}

					newObj.ObjectFlags |= ObjectFlags.Persistent;
					map.AddObject(newObj);

					Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(mousePos).ToVec2();
					newObj.Position2 = worldPos;
				},
				Enabled = () => !string.IsNullOrEmpty(GetClipboard())
			}
		};

		contextMenu.SetItems(dropDownMenu);
		_editUI.AddChild(contextMenu);
	}

	private void EditorOpenContextMenuForObject(BaseGameObject obj)
	{
		BaseMap? map = CurrentMap;
		AssertNotNull(map);

		var contextMenu = new EditorDropdown(true)
		{
			Offset = Engine.Host.MousePosition / _editUI!.GetScale(),
			OwningObject = obj
		};

		var dropDownMenu = new[]
		{
			new EditorDropDownButtonDescription
			{
				Name = "Copy",
				Click = (_, __) =>
				{
					string objectData = GetObjectSerialized(obj);
					if (Engine.Host is Win32Platform winPlat) winPlat.SetClipboard(objectData);
				}
			},
			new EditorDropDownButtonDescription
			{
				Name = "Cut",
				Click = (_, __) =>
				{
					string objectData = GetObjectSerialized(obj);
					if (Engine.Host is Win32Platform winPlat) winPlat.SetClipboard(objectData);

					map.RemoveObject(obj, true); // todo: register undo as delete
				}
			},
			new EditorDropDownButtonDescription
			{
				Name = "Delete",
				Click = (_, __) =>
				{
					map.RemoveObject(obj, true); // todo: register undo
				}
			},
			new EditorDropDownButtonDescription
			{
				Name = "Properties",
				Click = (_, __) => { EditorOpenPropertiesPanelForObject(obj); }
			},
			new EditorDropDownButtonDescription
			{
				Name = "Create Prefab",
				Click = (_, __) =>
				{
					var nameInput = new PropertyInputModal<StringInputModalEnvelope>(input =>
					{
						string text = input.Name;
						if (text.Length < 1) return false;

						CreateObjectPrefab(text, obj);
						return true;
					}, "Input name for the prefab:", "New Prefab", "Create");
					_editUI!.AddChild(nameInput);
				}
			},
			new EditorDropDownButtonDescription
			{
				Name = "Overwrite Prefab",
				Click = (_, __) => { CreateObjectPrefab(obj.PrefabOrigin!.PrefabName, obj); },
				Enabled = () => obj.PrefabOrigin != null
			},
		};

		contextMenu.SetItems(dropDownMenu);
		_editUI.AddChild(contextMenu);
	}

	#endregion
}