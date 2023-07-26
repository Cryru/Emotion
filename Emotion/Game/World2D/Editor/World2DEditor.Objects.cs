#region Using

using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Standard.XML;
using Emotion.Standard.XML.TypeHandlers;
using Emotion.UI;
using Emotion.Utility;
using static Emotion.Game.World2D.EditorHelpers.EditorUtility;

#endregion

#nullable enable

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
	// Interface
	protected Dictionary<GameObject2D, MapEditorObjectNameplate>? _namePlates;

	// Selection and MouseOver
	protected bool _canObjectSelect;

	protected GameObject2D? _rolloverObject
	{
		get => _rolloverIndex == -1 ? null : _allObjectsRollover![_rolloverIndex];
	}

	protected List<GameObject2D>? _allObjectsRollover; // List of objects under the mouse cursor (could be more than one)
	protected int _rolloverIndex = -1; // Index in ^
	protected GameObject2D? _selectedObject; // An object that is also presented as selected, used to improve ui

	// Dragging
	protected GameObject2D? _objectDragging;
	protected Vector2 _objectDragOffset;
	protected Vector2 _objectDragStartPos;

	// Temp
	protected static string? _objectCopyClipboard; // todo: platform based clipboard?

	protected void InitializeObjectEditor()
	{
		if (CurrentMap != null)
		{
			CurrentMap.OnObjectAdded += EditorObjectAdded;
			CurrentMap.OnObjectRemoved += EditorObjectRemoved;
		}

		SetObjectSelectionEnabled(true);
		LoadPrefabs();
    }

	protected void DisposeObjectEditor()
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

	protected void UpdateObjectEditor()
	{
		Map2D? map = CurrentMap;
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
			var results = new List<GameObject2D>();
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
				UIBaseWindow? objectListPanel = _editUI.GetWindowById("ObjectListPanel");
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
			_objectDragging.Position = newPos.ToVec3(_objectDragging.Z);
			EditorRegisterMoveAction(_objectDragging, _objectDragStartPos, _objectDragging.Position2);
		}
	}

	protected void RenderObjectSelection(RenderComposer c)
	{
		Map2D? map = CurrentMap;
		if (map == null) return;

		if (!_canObjectSelect) return;

		// Show selection of object, if any.
		if (_editUI?.DropDown?.OwningObject is GameObject2D objectWithContextMenu)
		{
			Rectangle bound = objectWithContextMenu.Bounds;
			c.RenderSprite(bound, Color.White * 0.3f);
		}

		if (_selectedObject != null && _selectedObject.ObjectState != ObjectState.Destroyed)
		{
			Rectangle bound = _selectedObject.Bounds;
			c.RenderSprite(bound, Color.White * 0.3f);
		}

		if (_rolloverObject != null)
		{
			Rectangle bound = _rolloverObject.Bounds;
			c.RenderSprite(bound, Color.White * 0.3f);
		}

		foreach (GameObject2D obj in map.GetObjects(true))
		{
			Rectangle bounds = obj.Bounds;

			if (!obj.ObjectFlags.HasFlag(ObjectFlags.Persistent))
			{
				c.RenderLine(bounds.TopLeft, bounds.BottomRight, Color.Black * 0.5f);
				c.RenderLine(bounds.TopRight, bounds.BottomLeft, Color.Black * 0.5f);
			}
			else if (obj.ObjectState == ObjectState.ConditionallyNonSpawned)
			{
				c.RenderSprite(bounds, Color.Magenta * 0.2f);
			}

			c.RenderOutline(bounds, Color.White * 0.4f);
		}
	}

	private bool ObjectEditorInputHandler(Key key, KeyStatus status)
	{
		if (key == Key.LeftAlt && status == KeyStatus.Down) RolloverObjectIncrement();

		bool leftClick = key == Key.MouseKeyLeft;
		bool rightClick = key == Key.MouseKeyRight;
		bool noMouseFocus = UIController.MouseFocus == _editUI || UIController.MouseFocus == null || UIController.MouseFocus is MapEditorObjectNameplate;

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
			_objectDragging = null;
		}

		return true;
	}

	protected void EnsureObjectNameplate(GameObject2D obj)
	{
		if (_namePlates == null || _namePlates.ContainsKey(obj)) return;

		var namePlate = new MapEditorObjectNameplate();
		namePlate.AttachToObject(obj);
		_namePlates.Add(obj, namePlate);
		_editUI!.AddChild(namePlate);
	}

	private void SetObjectSelectionEnabled(bool val)
	{
		_canObjectSelect = val;
		if (!_canObjectSelect)
		{
			if (_namePlates == null) return;
			foreach (KeyValuePair<GameObject2D, MapEditorObjectNameplate> namePlate in _namePlates)
			{
				_editUI?.RemoveChild(namePlate.Value);
			}

			_namePlates.Clear();
		}
		else
		{
			_namePlates ??= new Dictionary<GameObject2D, MapEditorObjectNameplate>();

			if (CurrentMap != null)
				foreach (GameObject2D obj in CurrentMap.GetObjects(true))
				{
					EnsureObjectNameplate(obj);
				}
		}
	}

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

	private void RolloverObjects(List<GameObject2D>? objs, bool incrementSel = false)
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
		if (prevRolloverObj != null && objs != null && _allObjectsRollover != null && objs[0] == _allObjectsRollover[0])
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
			txt.AppendLine($"Name: [{obj.UniqueId}] {obj.ObjectName ?? "null"}");
			txt.AppendLine($"Type: {obj.GetType().Name}");
			txt.AppendLine($"Pos: {obj.Position}");
			if (_allObjectsRollover != null && _allObjectsRollover.Count > 0) txt.AppendLine($"Objects Here: {_rolloverIndex + 1}/{_allObjectsRollover.Count} [ALT] to switch");

			text.Text = txt.ToString();

			Rectangle bounds = GetObjectBoundForEditor(obj);
			Vector2 attachPoint = bounds.TopRight;
			worldAttachUI.AttachToPosition(attachPoint.ToVec3() + new Vector3(5, 0, 0));
		}
	}

	protected void RolloverObjectIncrement()
	{
		if (_allObjectsRollover == null) return;
		RolloverObjects(_allObjectsRollover, true);
	}

	private void EditorAddObject(Type type)
	{
		Map2D? map = CurrentMap;
		if (map == null) return;

		Vector2 pos = Engine.Host.MousePosition;
		Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(pos).ToVec2();

		ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes)!;
		var newObj = (GameObject2D) constructor.Invoke(null);
		newObj.ObjectFlags |= ObjectFlags.Persistent;
		newObj.Position = worldPos.ToVec3();

		map.AddObject(newObj);
		ObjectSelect(newObj);

		// Stick to mouse to be placed.
		_objectDragging = newObj;
		_objectDragOffset = newObj.Size / 2f;
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
		propPanelOpen?.Close();
	}

	protected string GetObjectSerialized(GameObject2D obj)
	{
		return XMLFormat.To(obj);
	}

	/// <summary>
	/// Changes the object property and reinitializes the object without changing its reference.
	/// </summary>
	public void ChangeObjectProperty(GameObject2D obj, XMLFieldHandler field, object? value, bool recordUndo = true)
	{
		Map2D? objectMap = obj.Map;
		Debug.Assert(objectMap != null);
		Debug.Assert(objectMap == CurrentMap);
		objectMap = CurrentMap;
		if (objectMap == null) return;

        // Register action for undo.
        if (recordUndo)
		{
			object? oldValue = field.ReflectionInfo!.GetValue(obj);
			EditorRegisterObjectPropertyChange(obj, field, oldValue);
		}

		// Clean the object of unserialized properties.
		// This basically brings it in line to how it will look when loaded the first time.
		bool isPersist = obj.ObjectFlags.HasFlag(ObjectFlags.Persistent);
		int id = obj.UniqueId;
		obj.Destroy(); // Make sure object is correctly cleaned up as it will be reinited.
		EditorUtility.SetObjectToSerializationDefault<GameObject2D>(obj);
		obj.UniqueId = id;
		if (isPersist) obj.ObjectFlags |= ObjectFlags.Persistent;

		// Set the new value.
		field.ReflectionInfo.SetValue(obj, value);

		// Reinit the object, calling Init etc. as it is basically new.
		objectMap.Editor_ReinitializeObject(obj);
	}

#region Prefab System

	protected static Dictionary<string, GameObjectPrefab> _prefabDatabase = new();
	protected static string ObjectPrefabFolderAssets = $"EditorGame/Prefabs";
	private static void LoadPrefabs()
	{
		if (_prefabDatabase.Count != 0) return; // Already loaded.

		string[] prefabs = Engine.AssetLoader.GetAssetsInFolder(ObjectPrefabFolderAssets);
		for (int i = 0; i < prefabs.Length; i++)
		{
			var prefabPath = prefabs[i];
			Task.Run(() =>
			{
				var prefabAsset = Engine.AssetLoader.Get<XMLAsset<GameObjectPrefab>>(prefabPath, false);
				if (prefabAsset != null && prefabAsset.Content != null)
				{
					var prefabName = prefabAsset.Content.PrefabName;

					lock(_prefabDatabase)
					{
                        string originalName = prefabName;
                        int num = 1;
                        while (_prefabDatabase.ContainsKey(prefabName))
                        {
                            prefabName = $"{originalName}_{num}";
                            num++;
                        }

                        _prefabDatabase.Add(prefabName, prefabAsset.Content);
                    }
                }
			});
		}
	}
    protected void CreateObjectPrefab(string prefabName, GameObject2D obj)
	{
		// Check if overwritting existing prefab.
		GameObjectPrefab prefabData;
        if (_prefabDatabase.TryGetValue(prefabName, out GameObjectPrefab? existingPrefab))
		{
			prefabData = existingPrefab;
            prefabData.PrefabVersion++;
        }
		else
		{
            prefabData = new GameObjectPrefab()
            {
                PrefabName = prefabName,
                PrefabVersion = 1
            };
        }

		string objData = GetObjectSerialized(obj);
        prefabData.ObjectData = objData;

		// Fill properties
		prefabData.DefaultProperties ??= new List<Dictionary<string, object?>>();
		prefabData.DefaultProperties.Add(new Dictionary<string, object?>());
		Debug.Assert(prefabData.DefaultProperties.Count == prefabData.PrefabVersion);
		while(prefabData.DefaultProperties.Count < prefabData.PrefabVersion) // Fill until we reach it?
        {
            prefabData.DefaultProperties.Add(new Dictionary<string, object?>());
        }
		var thisVersionPropertyList = prefabData.DefaultProperties[prefabData.PrefabVersion - 1];

        var typeHandler = (XMLComplexBaseTypeHandler)XMLHelpers.GetTypeHandler(obj.GetType())!;
        IEnumerator<XMLFieldHandler> fields = typeHandler.EnumFields();
        while (fields.MoveNext())
		{
            XMLFieldHandler field = fields.Current;
            if (field == null) continue;
			if (ShouldIgnorePrefabProperty(field.Name)) continue; // We expect these to be always different.

			var valueInProp = field.ReflectionInfo.GetValue(obj);

            // ObjectFlags doesn't persist some values.
            if (field.TypeHandler is XMLEnumTypeHandler enumHandler)
            {
                valueInProp = enumHandler.StripDontSerializeValues(valueInProp);
            }

            if (!field.Skip && valueInProp != field.DefaultValue)
			{
				thisVersionPropertyList.Add(field.Name, valueInProp);
			}
        }

        string filePath = AssetLoader.MakeStringPathSafe(prefabName);
		filePath = $"{ObjectPrefabFolderAssets}/{filePath}.xml";

		XMLAsset<GameObjectPrefab>.CreateFromContent(prefabData).SaveAs(filePath, true);
		_prefabDatabase[prefabName] = prefabData;
	}
	protected void PlaceObjectFromPrefab(GameObjectPrefab prefab)
	{
        Map2D? map = CurrentMap;
        if (map == null) return;

        Vector2 pos = Engine.Host.MousePosition;
        Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(pos).ToVec2();

		GameObject2D newObj = XMLFormat.From<GameObject2D>(prefab.ObjectData);
        newObj.ObjectFlags |= ObjectFlags.Persistent;
        newObj.Position = worldPos.ToVec3();
		newObj.PrefabOrigin = new GameObjectPrefabOriginData(prefab);

        map.AddObject(newObj);
        ObjectSelect(newObj);

        // Stick to mouse to be placed.
        _objectDragging = newObj;
        _objectDragOffset = newObj.Size / 2f;

        return;
	}
	public bool IsPropertyDifferentFromPrefab(GameObject2D obj, XMLFieldHandler field)
	{
		if (obj.PrefabOrigin == null) return false;
		var prefabOrigin = obj.PrefabOrigin;
		if (!_prefabDatabase.TryGetValue(prefabOrigin.PrefabName, out GameObjectPrefab? prefab)) return false;
		if (prefab.DefaultProperties == null || prefab.DefaultProperties.Count < prefabOrigin.PrefabVersion) return false;
		if (ShouldIgnorePrefabProperty(field.Name, true)) return false;

		var versionProperties = prefab.DefaultProperties[prefabOrigin.PrefabVersion - 1];
		var objectVal = field.ReflectionInfo.GetValue(obj);
		bool isDefault = field.DefaultValue == objectVal;

		// ObjectFlags doesn't persist some values.
		if (field.TypeHandler is XMLEnumTypeHandler enumHandler)
		{
            objectVal = enumHandler.StripDontSerializeValues(objectVal);
		}

        // If it's missing that means the prefab has it set to the default value for the class.
        if (versionProperties.TryGetValue(field.Name, out object? val))
		{
			return !Helpers.AreObjectsEqual(val, objectVal);
		}
		else if(!isDefault)
		{
			return true;
		}

		return false;
	}

	protected void UpdateObjectToLatestPrefabVersion(GameObject2D obj)
	{

	}

	private bool ShouldIgnorePrefabProperty(string name, bool checkingForDiff = false)
	{
		// When checking for diffs dont show diffs in these properties
		// as they are expected to be customized.
		if (checkingForDiff && name is "ObjectName")
		{
			return true;
		}

		// We expect these to be different
		return name is "Position" or "Center" or "PrefabOrigin";
    }

    #endregion
}