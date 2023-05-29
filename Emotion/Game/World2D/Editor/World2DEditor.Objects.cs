﻿#nullable enable

#region Using

using System.Diagnostics;
using System.Reflection;
using System.Text;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D;

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
	protected bool _ignoreNextObjectDragLetGo;
	protected Vector2 _objectDragStartPos;

	// Temp
	protected static string? _objectCopyClipboard; // todo: platform based clipboard?

	protected void InitializeObjectEditor()
	{
		SetObjectSelectionEnabled(true);
	}

	protected void DisposeObjectEditor()
	{
		CurrentMap.OnObjectAdded -= EditorObjectAdded;
		CurrentMap.OnObjectRemoved -= EditorObjectRemoved;

		_namePlates?.Clear();

		_selectedObject = null;
		_rolloverIndex = -1;
		_allObjectsRollover = null;
	}

	protected void ObjectEditorMapChanged(Map2D? oldMap, Map2D newMap)
	{
		if (oldMap != null)
		{
			oldMap.OnObjectAdded -= EditorObjectAdded;
			oldMap.OnObjectRemoved -= EditorObjectRemoved;
		}

		newMap.OnObjectAdded += EditorObjectAdded;
		newMap.OnObjectRemoved += EditorObjectRemoved;
	}

	protected void UpdateObjectEditor()
	{
		bool mouseInUI = _editUI?.MouseFocus != null && _editUI.MouseFocus != _editUI; // || _editUI?.InputFocus is UITextInput;
		var mouseFocusNameplate = _editUI?.MouseFocus as MapEditorObjectNameplate;
		bool mouseNotInUIOrInNameplate = !mouseInUI || mouseFocusNameplate != null;

		// Update objects that are rollovered.
		// If not currently selecting objects, or mouse is in UI, or dragging then dont.
		if (_objectDragging != null) RolloverObjects(new List<GameObject2D>(1) {_objectDragging});
		if (_canObjectSelect && mouseNotInUIOrInNameplate)
		{
			// Add objects (and their UI) under the mouse to the rollover list.
			Map2D map = CurrentMap;
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
			if (_editUI!.MouseFocus != null)
			{
				UIBaseWindow? objectListPanel = _editUI.GetWindowById("ObjectListPanel");
				if (objectListPanel != null && _editUI.MouseFocus.IsWithin(objectListPanel)) removeRollover = false;
			}

			if (removeRollover) RolloverObjects(null);
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
	}

	protected void RenderObjectSelection(RenderComposer c)
	{
		if (!_canObjectSelect) return;

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

		foreach (GameObject2D obj in CurrentMap.GetObjects(true))
		{
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

	private bool ObjectEditorInputHandler(Key key, KeyStatus status)
	{
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

	protected void RolloverObjectIncrement()
	{
		if (_allObjectsRollover == null) return;
		RolloverObjects(_allObjectsRollover, true);
	}

	private void EditorAddObject(Type type)
	{
		Vector2 pos = Engine.Host.MousePosition;
		Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(pos).ToVec2();

		ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes)!;
		var newObj = (GameObject2D) constructor.Invoke(null);
		newObj.ObjectFlags |= ObjectFlags.Persistent;
		newObj.Position = worldPos.ToVec3();

		CurrentMap.AddObject(newObj);

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

	protected string GetObjectSerialized(GameObject2D obj)
	{
		bool serialized = obj.ObjectFlags.HasFlag(ObjectFlags.Persistent);
		obj.TrimPropertiesForSerialize(); // Ensure that the object looks like it would when loading a file.
		if (serialized) obj.ObjectFlags |= ObjectFlags.Persistent;

		string data = XMLFormat.To(obj);
		return data;
	}
}