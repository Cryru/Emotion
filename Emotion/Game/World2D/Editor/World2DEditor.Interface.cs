#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Editor.EditorComponents;
using Emotion.Editor.EditorHelpers;
using Emotion.Editor.EditorWindows;
using Emotion.Editor.EditorWindows.DataEditorUtil;
using Emotion.Game.World.Editor.Actions;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.IO;
using Emotion.Platform.Implementation.Win32;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
	protected override void EditorAttachTopBarButtons(UIBaseWindow parentList)
	{
		Map2D? map = CurrentMap;

		base.EditorAttachTopBarButtons(parentList);

		string GetObjectSelectionLabel()
		{
			return $"Selection: {(_canObjectSelect ? "Enabled" : "Disabled")}";
		}

		EditorButton objectsMenu = EditorDropDownButton("Objects", new[]
		{
			// true by default, mouseover shows props
			// click selects the obj and shows prop editor window
			// alt switch between overlapping objects
			new EditorDropDownButtonDescription
			{
				Name = GetObjectSelectionLabel(),
				Click = (_, button) =>
				{
					SetObjectSelectionEnabled(!_canObjectSelect);
					button.Text = GetObjectSelectionLabel();
				}
			},
			new EditorDropDownButtonDescription
			{
				Name = "Object Filters",
				Click = (_, __) => { }
			},
			new EditorDropDownButtonDescription
			{
				Name = "View Object List",
				Click = (_, __) =>
				{
					AssertNotNull(map);

					var panel = new EditorListOfItemsPanel<GameObject2D>(
						"All Objects",
						map.GetObjects(),
						obj => { Engine.Renderer.Camera.Position = obj.Bounds.Center.ToVec3(); },
						obj => { RolloverObjects(new List<GameObject2D> {obj}); }
					)
					{
						Id = "ObjectListPanel"
					};
					_editUI!.AddChild(panel);
				},
				Enabled = () => map != null
			},
			// Object creation dialog
			new EditorDropDownButtonDescription
			{
				Name = "Add Object",
				Click = (_, __) =>
				{
					List<Type> objectTypes = EditorUtility.GetTypesWhichInherit<GameObject2D>();

					var panel = new EditorListOfItemsPanel<Type>("Add Object", objectTypes, EditorAddObject);
					panel.Text = "These are all classes with parameterless constructors\nthat inherit GameObject2D.\nChoose class of object to add:";
					panel.CloseOnClick = true;

					_editUI!.AddChild(panel);
				},
				Enabled = () => map != null
			},
			new EditorDropDownButtonDescription
			{
				Name = "Add From Prefab",
				Click = (_, __) =>
				{
					var panel = new EditorListOfItemsPanel<GameObjectPrefab>("Prefab Library", _prefabDatabase.Values, PlaceObjectFromPrefab);
					panel.Text = "Choose prefab to add as a new object:";
					panel.CloseOnClick = true;

					_editUI!.AddChild(panel);
				},
				Enabled = () => map != null && _prefabDatabase.Count > 0
			}
		});

		EditorButton tilesMenu = EditorDropDownButton("Tiles", new[]
		{
			// false by default, mouseover shows props, alt switch layers
			//new EditorDropDownButtonDescription
			//{
			//	Name = $"Selection: {(_tileSelect ? "Enabled" : "Disabled")}"
			//},
			// Shows layers, tilesets and other special editors for this mode, disables object selection while open
			new EditorDropDownButtonDescription
			{
				Name = "Open Tile Editor",
				Click = (_, __) =>
				{
					AssertNotNull(map);
					_editUI!.AddChild(new MapEditorTilePanel(map));
				},
				Enabled = () => map != null
			},
		});

		EditorButton editorMenu = EditorDropDownButton("Editor", new[]
		{
			// Shows actions done in the editor, can be undone
			new EditorDropDownButtonDescription
			{
				Name = "Undo History",
				Click = (_, __) =>
				{
					var panel = new EditorListOfItemsPanel<IWorldEditorAction>("Actions", _actions, obj => { });
					_editUI!.AddChild(panel);
				}
			},
			new EditorDropDownButtonDescription
			{
				Name = "Model Viewer (WIP)",
				Click = (_, __) =>
				{
					var panel = new ModelViewer();
					_editUI!.AddChild(panel);
				},
			},
		});

		EditorButton mapMenu = EditorDropDownButton("Map", new[]
		{
			new EditorDropDownButtonDescription
			{
				Name = "Reload",
				Click = (_, __) => Task.Run(map!.Reset),
				Enabled = () => map != null
			},
			new EditorDropDownButtonDescription
			{
				Name = "Reset From File",
				Click = (_, __) => Task.Run(() => ChangeSceneMap(map!.FileName!)), // todo: pending changes
				Enabled = () => map?.FileName != null
			},
			new EditorDropDownButtonDescription
			{
				Name = "Properties",
				Click = (_, __) =>
				{
					AssertNotNull(map);
					var panel = new GenericPropertiesEditorPanel(map);
					_editUI!.AddChild(panel);
				},
				Enabled = () => map != null
			},
		});

		EditorButton otherTools = EditorDropDownButton("Other", new[]
		{
			new EditorDropDownButtonDescription
			{
				Name = "Open Folder",
				Click = (_, __) => { Process.Start("explorer.exe", "."); },
				Enabled = () => Engine.Host is Win32Platform
			},

			new EditorDropDownButtonDescription
			{
				Name = "Performance Monitor",
				Click = (_, __) =>
				{
					var panel = new PerformanceMonitor();
					_editorUIAlways!.AddChild(panel);
				},
			},
		});

		// todo: GPU texture viewer
		// todo: animation tool (convert from imgui)
		// todo: asset preview tool
		// todo: ema integration

		parentList.AddChild(objectsMenu);
		parentList.AddChild(tilesMenu);
		parentList.AddChild(editorMenu);
		parentList.AddChild(mapMenu);
		parentList.AddChild(otherTools);
	}

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

	private void EditorOpenPropertiesPanelForObject(GameObject2D obj)
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

	private void EditorOpenContextMenuObjectModeNoSelection()
	{
		Map2D? map = CurrentMap;
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
					AssertNotNull(_objectCopyClipboard);

					var newObj = XMLFormat.From<GameObject2D>(_objectCopyClipboard);
					if (newObj == null) return;

					newObj.ObjectFlags |= ObjectFlags.Persistent;
					map.AddObject(newObj);

					Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(mousePos).ToVec2();
					newObj.Position2 = worldPos;
				},
				Enabled = () => !string.IsNullOrEmpty(_objectCopyClipboard)
			}
		};

		contextMenu.SetItems(dropDownMenu);
		_editUI.AddChild(contextMenu);
	}

	private void EditorOpenContextMenuForObject(GameObject2D obj)
	{
		Map2D? map = CurrentMap;
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
				Click = (_, __) => { _objectCopyClipboard = GetObjectSerialized(obj); }
			},
			new EditorDropDownButtonDescription
			{
				Name = "Cut",
				Click = (_, __) =>
				{
					_objectCopyClipboard = GetObjectSerialized(obj);
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
}