#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Editor;
using Emotion.Editor.EditorHelpers;
using Emotion.Editor.EditorWindows;
using Emotion.Game.Text;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.IO;
using Emotion.Platform.Input;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
	protected void InitializeEditorInterface()
	{
		_editUI = new UIController(KeyListenerType.EditorUI);

		UIBaseWindow topBar = GetEditorTopBar();
		_editUI.AddChild(topBar);

		UIBaseWindow worldInspect = GetWorldAttachInspectWindow();
		_editUI.AddChild(worldInspect);

		_setControllersToVisible = UIController.GetControllersLesserPriorityThan(KeyListenerType.Editor);
		for (var i = 0; i < _setControllersToVisible.Count; i++)
		{
			_setControllersToVisible[i].SetVisible(false);
		}
	}

	protected void DisposeEditorInterface()
	{
		if (_setControllersToVisible != null)
		{
			for (var i = 0; i < _setControllersToVisible.Count; i++)
			{
				_setControllersToVisible[i].SetVisible(true);
			}

			_setControllersToVisible = null;
		}

		_editUI!.Dispose();
		_editUI = null;
	}

	private UIBaseWindow GetEditorTopBar()
	{
		Map2D? map = CurrentMap;

		var topBar = new UISolidColor();
		topBar.MinSize = new Vector2(0, 15);
		topBar.MaxSize = new Vector2(UIBaseWindow.DefaultMaxSize.X, 15);
		topBar.ScaleMode = UIScaleMode.FloatScale;
		topBar.WindowColor = MapEditorColorPalette.BarColor;
		topBar.InputTransparent = false;
		topBar.Id = "TopBar";

		var mapName = new UIText();
		mapName.ParentAnchor = UIAnchor.CenterRight;
		mapName.Anchor = UIAnchor.CenterRight;
		mapName.ScaleMode = UIScaleMode.FloatScale;
		mapName.WindowColor = MapEditorColorPalette.TextColor;
		mapName.Text = map == null ? "No map loaded." : $"{map.MapName} @ {map.FileName ?? "Unsaved"}";
		mapName.FontFile = FontAsset.DefaultBuiltInFontName;
		mapName.FontSize = 6;
		mapName.Margins = new Rectangle(0, 0, 5, 0);
		topBar.AddChild(mapName);

		var topBarList = new UIBaseWindow();
		topBarList.ScaleMode = UIScaleMode.FloatScale;
		topBarList.LayoutMode = LayoutMode.HorizontalList;
		topBarList.ListSpacing = new Vector2(3, 0);
		topBarList.Margins = new Rectangle(3, 3, 3, 3);
		topBarList.InputTransparent = false;
		topBarList.Id = "List";
		topBar.AddChild(topBarList);

		var accent = new UISolidColor();
		accent.WindowColor = MapEditorColorPalette.ActiveButtonColor;
		accent.MaxSize = new Vector2(UIBaseWindow.DefaultMaxSize.X, 1);
		accent.Anchor = UIAnchor.BottomLeft;
		accent.ParentAnchor = UIAnchor.BottomLeft;
		topBar.AddChild(accent);

		EditorAttachTopBarButtons(topBarList);

		return topBar;
	}

	protected MapEditorTopBarButton EditorDropDownButton(string label, EditorDropDownButtonDescription[] menuButtons)
	{
		// todo: maybe the drop down exclusivity logic should be handled by the top bar or some kind of parent ui.
		// though either way a SetDropDownMode function will need to exist on buttons to change their style.

		var button = new MapEditorTopBarButton();

		void SpawnDropDown()
		{
			bool openOnMe = _editUI?.DropDown?.OwningObject == button;
			_editUI?.RemoveChild(_editUI.DropDown);
			if (openOnMe) return;

			var dropDownWin = new MapEditorDropdown(true);
			dropDownWin.ParentAnchor = UIAnchor.BottomLeft;
			dropDownWin.OwningObject = button;
			dropDownWin.SetItems(menuButtons);

			dropDownWin.RelativeTo = $"button{button.Text}";

			List<UIBaseWindow> siblings = button.Parent!.Children!;
			for (var i = 0; i < siblings.Count; i++)
			{
				UIBaseWindow child = siblings[i];
				if (child is MapEditorTopBarButton but) but.SetActiveMode(child == button);
			}

			dropDownWin.OnCloseProxy = () => button.SetActiveMode(false);

			_editUI!.AddChild(dropDownWin);
		}

		button.Text = label;
		button.OnMouseEnterProxy = _ =>
		{
			if (_editUI?.DropDown != null && _editUI.DropDown?.OwningObject != button && _editUI.DropDown?.OwningObject is MapEditorTopBarButton)
				SpawnDropDown();
		};
		button.OnClickedProxy = _ => SpawnDropDown();
		button.Id = $"button{label}";

		return button;
	}

	protected virtual void EditorAttachTopBarButtons(UIBaseWindow parentList)
	{
		string GetObjectSelectionLabel()
		{
			return $"Selection: {(_canObjectSelect ? "Enabled" : "Disabled")}";
		}

		Map2D? map = CurrentMap;
		Type mapType = _mapType;

		MapEditorTopBarButton fileMenu = EditorDropDownButton("File", new[]
		{
			new EditorDropDownButtonDescription
			{
				Name = "New",
				Click = (_, __) => { _editUI!.AddChild(new MapEditorModal(new MapEditorCreateMapPanel(this, mapType))); }
			},
			new EditorDropDownButtonDescription
			{
				Name = "Open",
				Click = (_, __) =>
				{
					var filePicker = new EditorFileExplorer<XMLAsset<Map2D>>(
						asset => { ChangeSceneMap(asset.Content!); },
						assetName =>
						{
							if (!assetName.Contains(".xml")) return false;

							string xmlTag;
							if (_mapType == typeof(Map2D))
							{
								xmlTag = "<Map2D";
							}
							else
							{
								string mapTypeName = _mapType.FullName ?? "";
								xmlTag = $"<Map2D type=\"{mapTypeName}\"";
							}

							var assetLoaded = Engine.AssetLoader.Get<TextAsset>(assetName, false);
							return assetLoaded?.Content != null && assetLoaded.Content.Contains(xmlTag);
						}
					);

					_editUI!.AddChild(new MapEditorModal(filePicker));
				}
			},
			new EditorDropDownButtonDescription
			{
				Name = "Save",
				Click = (_, __) => { Task.Run(() => EditorSaveMap()); },
				Enabled = () => map?.FileName != null
			},
			// todo: generic text input or explorer ui
			//new EditorDropDownButtonDescription
			//{
			//	Name = "Save As"
			//}
		});

		MapEditorTopBarButton objectsMenu = EditorDropDownButton("Objects", new[]
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
					Debug.Assert(map != null);

					var panel = new EditorListOfItemsPanel<GameObject2D>("All Objects", map.GetObjects(),
						obj => { Engine.Renderer.Camera.Position = obj.Bounds.Center.ToVec3(); },
						obj => { RolloverObjects(new() {obj}); }
					);
					panel.Id = "ObjectListPanel";
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
			}
		});

		MapEditorTopBarButton tilesMenu = EditorDropDownButton("Tiles", new[]
		{
			// false by default, mouseover shows props, alt switch layers
			new EditorDropDownButtonDescription
			{
				Name = $"Selection: {(_tileSelect ? "Enabled" : "Disabled")}"
			},
			// Shows layers, tilesets and other special editors for this mode, disables object selection while open
			new EditorDropDownButtonDescription
			{
				Name = "Open Tile Editor"
			},
		});

		MapEditorTopBarButton mapMenu = EditorDropDownButton("Map", new[]
		{
			// Shows actions done in the editor, can be undone
			new EditorDropDownButtonDescription
			{
				Name = "Undo History",
				Click = (_, __) =>
				{
					var panel = new EditorListOfItemsPanel<EditorAction>("Actions", _actions ?? new List<EditorAction>(),
						obj => { }
					);

					_editUI!.AddChild(panel);
				}
			},
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
					var panel = new GenericPropertiesEditorPanel(map);
					_editUI!.AddChild(panel);
				},
				Enabled = () => map != null
			},
		});

		MapEditorTopBarButton otherTools = EditorDropDownButton("Other", new[]
		{
			new EditorDropDownButtonDescription
			{
				Name = "Model Viewer (WIP)",
				Click = (_, __) =>
				{
					var panel = new ModelViewer();
					_editUI!.AddChild(panel);
				},
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

		// todo: performance tool
		// todo: GPU texture viewer
		// todo: animation tool (convert from imgui)
		// todo: asset preview tool
		// todo: ema integration

		parentList.AddChild(fileMenu);
		parentList.AddChild(objectsMenu);
		parentList.AddChild(tilesMenu);
		parentList.AddChild(mapMenu);
		parentList.AddChild(otherTools);
	}

	private UIBaseWindow GetWorldAttachInspectWindow()
	{
		var worldAttachUI = new MapEditorInfoWorldAttach();
		worldAttachUI.ScaleMode = UIScaleMode.FloatScale;

		var worldAttachBg = new UISolidColor();
		worldAttachBg.WindowColor = Color.Black * 0.7f;
		worldAttachBg.StretchX = true;
		worldAttachBg.StretchY = true;
		worldAttachBg.Paddings = new Rectangle(3, 3, 3, 3);
		worldAttachBg.ScaleMode = UIScaleMode.FloatScale;
		worldAttachUI.AddChild(worldAttachBg);

		var txt = new UIText();
		txt.ScaleMode = UIScaleMode.FloatScale;
		txt.WindowColor = MapEditorColorPalette.TextColor;
		txt.Id = "text";
		txt.FontFile = FontAsset.DefaultBuiltInFontName;
		txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
		txt.TextShadow = Color.Black;
		txt.TextHeightMode = GlyphHeightMeasurement.NoMinY;
		txt.IgnoreParentColor = true;
		worldAttachBg.AddChild(txt);

		worldAttachUI.Id = "WorldAttach";
		worldAttachUI.Visible = false;

		return worldAttachUI;
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
		Debug.Assert(obj != null);
		Debug.Assert(_editUI != null);

		MapEditorObjectPropertiesPanel? existingPanel = EditorGetAlreadyOpenPropertiesPanelForObject(obj.UniqueId);
		if (existingPanel != null)
		{
			_editUI.SetInputFocus(existingPanel);
			return;
		}

		var propsPanel = new MapEditorObjectPropertiesPanel(this, obj);
		_editUI.AddChild(propsPanel);
		_editUI.SetInputFocus(propsPanel);
	}

	private void EditorOpenContextMenuObjectModeNoSelection()
	{
		Map2D? map = CurrentMap;
		Debug.Assert(map != null);

		Vector2 mousePos = Engine.Host.MousePosition;

		var contextMenu = new MapEditorDropdown(true);
		contextMenu.Offset = mousePos / _editUI!.GetScale();

		var dropDownMenu = new[]
		{
			new EditorDropDownButtonDescription
			{
				Name = "Paste",
				Click = (_, __) =>
				{
					var newObj = XMLFormat.From<GameObject2D>(_objectCopyClipboard);
					if (newObj != null)
					{
						newObj.ObjectFlags |= ObjectFlags.Persistent;
						map.AddObject(newObj);

						Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(mousePos).ToVec2();
						newObj.Position2 = worldPos;
					}
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
		Debug.Assert(map != null);

		var contextMenu = new MapEditorDropdown(true);
		contextMenu.Offset = Engine.Host.MousePosition / _editUI!.GetScale();
		contextMenu.OwningObject = obj;

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
			}
		};

		contextMenu.SetItems(dropDownMenu);
		_editUI.AddChild(contextMenu);
	}
}