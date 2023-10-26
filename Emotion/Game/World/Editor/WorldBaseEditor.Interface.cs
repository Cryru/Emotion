#nullable enable

#region Using

using System.Threading.Tasks;
using Emotion.Editor.EditorComponents;
using Emotion.Editor.EditorHelpers;
using Emotion.Editor.EditorWindows;
using Emotion.Editor.EditorWindows.DataEditorUtil;
using Emotion.Game.Text;
using Emotion.Game.World.Editor.Actions;
using Emotion.Game.World2D;
using Emotion.Game.World2D.Editor;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Game.World3D;
using Emotion.IO;
using Emotion.Platform.Implementation.Win32;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

namespace Emotion.Game.World.Editor;

public abstract partial class WorldBaseEditor
{
	// Selection and MouseOver
	protected bool _canObjectSelect;

	// Interface
	protected Dictionary<BaseGameObject, MapEditorObjectNameplate>? _namePlates;

	private UIBaseWindow _bottomBar;
	private MapEditorLabel _bottomBarText;

	protected void InitializeEditorInterface()
	{
		_editUI = new UIController(KeyListenerType.EditorUI)
		{
			Id = "WorldEditor"
		};

		UIBaseWindow topBar = GetEditorTopBar();
		_editUI.AddChild(topBar);

		UIBaseWindow bottomBar = GetEditorBottomBar();
		_bottomBar = bottomBar;
		_bottomBarText = (MapEditorLabel) bottomBar.GetWindowById("Label")!;
		_editUI.AddChild(bottomBar);

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
		BaseMap? map = CurrentMap;

		var topBar = new UISolidColor();
		topBar.MaxSizeY = 15;
		topBar.ScaleMode = UIScaleMode.FloatScale;
		topBar.WindowColor = MapEditorColorPalette.BarColor;
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
#if NEW_UI
		topBarList.Margins = new Rectangle(3, 0, 3, 0);
		topBarList.Paddings = new Rectangle(0, 3, 0, 3);
		topBarList.AlignAnchor = UIAnchor.CenterLeft;
#else
		topBarList.Margins = new Rectangle(3, 3, 3, 3);
#endif
		topBarList.Id = "List";
		topBar.AddChild(topBarList);

		var accent = new UISolidColor();
		accent.WindowColor = MapEditorColorPalette.ActiveButtonColor;
		accent.MaxSizeY = 1;
		accent.Anchor = UIAnchor.BottomLeft;
		accent.ParentAnchor = UIAnchor.BottomLeft;
		topBar.AddChild(accent);

		EditorAttachTopBarButtons(topBarList);

		return topBar;
	}

	private UIBaseWindow GetEditorBottomBar()
	{
		var bottomBar = new UISolidColor();
		bottomBar.MaxSizeY = 12;
		bottomBar.ScaleMode = UIScaleMode.FloatScale;
		bottomBar.WindowColor = MapEditorColorPalette.BarColor;
		bottomBar.Id = "BottomBar";
		bottomBar.Anchor = UIAnchor.BottomLeft;
		bottomBar.ParentAnchor = UIAnchor.BottomLeft;

		var label = new MapEditorLabel("No object selected");
		label.Margins = new Rectangle(3, 3, 3, 3);
		label.FontSize = MapEditorColorPalette.EditorButtonTextSize - 2;
		label.ParentAnchor = UIAnchor.CenterLeft;
		label.Anchor = UIAnchor.CenterLeft;
		label.Id = "Label";
		bottomBar.AddChild(label);

		var bottomBarLogTextContainer = new UIBaseWindow();
		bottomBarLogTextContainer.Anchor = UIAnchor.BottomLeft;
		bottomBarLogTextContainer.ParentAnchor = UIAnchor.TopLeft;
		bottomBarLogTextContainer.LayoutMode = LayoutMode.VerticalList;
		bottomBarLogTextContainer.Id = "LogContainer";
		bottomBarLogTextContainer.StretchX = true;
		bottomBarLogTextContainer.StretchY = true;
		bottomBar.AddChild(bottomBarLogTextContainer);

		return bottomBar;
	}

	private class CreateNewMapEnvelope
	{
		public string? Name;
		public string? FilePath;
	}

	protected virtual void EditorAttachTopBarButtons(UIBaseWindow parentList)
	{
		BaseMap? map = CurrentMap;
		Type mapType = _mapType;

		EditorButton fileMenu = EditorDropDownButton("Map", new[]
		{
			new EditorDropDownButtonDescription
			{
				Name = "New",
				Click = (_, __) =>
				{
					var createMapModal = new PropertyInputModal<CreateNewMapEnvelope>(data =>
					{
						if (string.IsNullOrEmpty(data.FilePath) || string.IsNullOrEmpty(data.Name)) return false;

						string fileName = data.FilePath;
						if (!fileName.EndsWith(".xml")) fileName += ".xml";

						var newMap = (BaseMap) Activator.CreateInstance(_mapType, true)!;
						newMap.MapName = data.Name;
						newMap.FileName = fileName;

						newMap.EditorCreateInitialize();

						EditorSaveMap(newMap);
						ChangeSceneMap(newMap);

						return true;
					}, "", "New Map", "Create");

					_editUI!.AddChild(createMapModal);
				}
			},
			new EditorDropDownButtonDescription
			{
				Name = "Open",
				Click = (_, __) =>
				{
					var filePicker = new EditorFileExplorer<XMLAsset<BaseMap>>(
						asset => { ChangeSceneMap(asset.Content!); },
						assetName =>
						{
							// Verify if the xml file is of the editor map type.
							if (!assetName.Contains(".xml")) return false;

							string xmlTag;
							if (_mapType == typeof(Map2D))
							{
								xmlTag = "<Map2D";
							}
							else if (_mapType == typeof(Map3D))
							{
								xmlTag = "<Map3D";
							}
							else
							{
								string mapTypeName = _mapType.FullName ?? "";
								xmlTag = $"<BaseMap type=\"{mapTypeName}\"";
							}

							var assetLoaded = Engine.AssetLoader.Get<TextAsset>(assetName, false);
							return assetLoaded?.Content != null && assetLoaded.Content.Contains(xmlTag);
						}
					);

					_editUI!.AddChild(filePicker);
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

		Type[]? dataTypes = GameDataDatabase.GetGameDataTypes();
		var dataEditors = new EditorDropDownButtonDescription[dataTypes?.Length ?? 0];
		for (var i = 0; i < dataEditors.Length; i++)
		{
			AssertNotNull(dataTypes);
			Type type = dataTypes[i];

			var editor = new EditorDropDownButtonDescription
			{
				Name = $"{type.Name} Editor",
				Click = (_, __) =>
				{
					var editor = new DataEditorGeneric(type);
					_editorUIAlways!.AddChild(editor);
				}
			};
			dataEditors[i] = editor;
		}

		EditorButton dataEditorsButton = EditorDropDownButton("GameData", dataEditors);

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

					var panel = new EditorListOfItemsPanel<BaseGameObject>(
						"All Objects",
						map.GetObjects(),
						EditorOpenPropertiesPanelForObject,
						obj => { RolloverObjects(new List<BaseGameObject> {obj}); }
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
					AssertNotNull(map);

					List<Type> objectTypes = map.GetValidObjectTypes();

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
				Name = "3D Mesh Viewer",
				Click = (_, __) =>
				{
					var panel = new ModelViewer();
					_editUI!.AddChild(panel);
				},
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

		parentList.AddChild(fileMenu);
		parentList.AddChild(dataEditorsButton);
		parentList.AddChild(objectsMenu);
		parentList.AddChild(editorMenu);
		parentList.AddChild(otherTools);
	}

	protected EditorButton EditorDropDownButton(string label, EditorDropDownButtonDescription[] menuButtons)
	{
		// todo: maybe the drop down exclusivity logic should be handled by the top bar or some kind of parent ui.
		// though either way a SetDropDownMode function will need to exist on buttons to change their style.

		var button = new EditorButton();

		void SpawnDropDown()
		{
			bool openOnMe = _editUI?.DropDown?.OwningObject == button;
			_editUI?.RemoveChild(_editUI.DropDown);
			if (openOnMe) return;

			// Make sure we don't display an empty dropdown.
			if (menuButtons.Length == 0)
				menuButtons = new[]
				{
					new EditorDropDownButtonDescription
					{
						Name = "None",
						Enabled = () => false
					}
				};

			var dropDownWin = new EditorDropdown(true)
			{
				ParentAnchor = UIAnchor.BottomLeft,
				OwningObject = button,
				RelativeTo = $"button{button.Text}"
			};
			dropDownWin.SetItems(menuButtons);

			List<UIBaseWindow> siblings = button.Parent!.Children!;
			for (var i = 0; i < siblings.Count; i++)
			{
				UIBaseWindow child = siblings[i];
				if (child is EditorButton but) but.SetActiveMode(child == button);
			}

			dropDownWin.OnCloseProxy = () => button.SetActiveMode(false);

			_editUI!.AddChild(dropDownWin);
		}

		button.Text = label;
		button.OnMouseEnterProxy = _ =>
		{
			if (_editUI?.DropDown != null && _editUI.DropDown?.OwningObject != button && _editUI.DropDown?.OwningObject is EditorButton)
				SpawnDropDown();
		};
		button.OnClickedProxy = _ => SpawnDropDown();
		button.Id = $"button{label}";

		return button;
	}

	private UIBaseWindow GetWorldAttachInspectWindow()
	{
		// todo: Move this window spawning to the class V
		var worldAttachUI = new MapEditorInfoWorldAttach
		{
			ScaleMode = UIScaleMode.FloatScale,
			Id = "WorldAttach",
			Visible = false
		};

		var worldAttachBg = new UISolidColor
		{
			WindowColor = Color.Black * 0.7f,
			StretchX = true,
			StretchY = true,
			Paddings = new Rectangle(3, 3, 3, 3),
			ScaleMode = UIScaleMode.FloatScale
		};
		worldAttachUI.AddChild(worldAttachBg);

		var txt = new UIText
		{
			ScaleMode = UIScaleMode.FloatScale,
			WindowColor = MapEditorColorPalette.TextColor,
			Id = "text",
			FontFile = FontAsset.DefaultBuiltInFontName,
			FontSize = MapEditorColorPalette.EditorButtonTextSize,
			TextShadow = Color.Black,
			TextHeightMode = GlyphHeightMeasurement.NoMinY,
			IgnoreParentColor = true
		};
		worldAttachBg.AddChild(txt);

		return worldAttachUI;
	}
}