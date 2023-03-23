#region Using

using System.Diagnostics;
using Emotion.Game.Text;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

#nullable enable

// Provides pseudo-ui templates for the editor

namespace Emotion.Game.World2D
{
	public partial class Map2D
	{
		private UIBaseWindow GetEditorTopBar()
		{
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
			mapName.Text = $"{MapName} @ {FileName ?? "Unsaved"}";
			mapName.FontFile = "Editor/UbuntuMono-Regular.ttf";
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

			AttachTopBarButtons(topBarList);

			return topBar;
		}

		private MapEditorTopBarButton EditorDropDownButton(string label, EditorDropDownButtonDescription[] menuButtons)
		{
			// todo: maybe the drop down exclusivity logic should be handled by the top bar or some kind of parent ui.
			// though either way a SetDropDownMode function will need to exist on buttons to change their style.

			var button = new MapEditorTopBarButton();

			void SpawnDropDown()
			{
				bool openOnMe = _editUI?.DropDown?.OwningObject == button;
				_editUI?.RemoveChild(_editUI.DropDown);
				if (openOnMe) return;

				var dropDownWin = new MapEditorDropdown();
				dropDownWin.ParentAnchor = UIAnchor.BottomLeft;
				dropDownWin.OwningObject = button;
				dropDownWin.SetItems(menuButtons);

				dropDownWin.RelativeTo = $"button{button.Text}";

				List<UIBaseWindow> siblings = button.Parent!.Children!;
				for (var i = 0; i < siblings.Count; i++)
				{
					UIBaseWindow child = siblings[i];
					if (child is MapEditorTopBarButton but) but.SetDropDownMode(child == button, dropDownWin);
				}

				_editUI!.AddChild(dropDownWin);
			}

			button.Text = label;
			button.OnMouseEnterProxy = _ =>
			{
				if (_editUI?.DropDown != null && _editUI.DropDown?.OwningObject != button && _editUI.DropDown?.OwningObject is MapEditorTopBarButton)
					SpawnDropDown();
			};
			button.OnClickedProxy = _ => { SpawnDropDown(); };
			button.Id = $"button{label}";

			return button;
		}

		private void AttachTopBarButtons(UIBaseWindow parentList)
		{
			string GetObjectSelectionLabel()
			{
				return $"Selection: {(_objectSelect ? "Enabled" : "Disabled")}";
			}

			MapEditorTopBarButton fileMenu = EditorDropDownButton("File", new[]
			{
				new EditorDropDownButtonDescription
				{
					Name = "New",
					Click = _ =>
					{
						_editUI!.AddChild(new MapEditorModal(new MapEditorCreateMapPanel(this)));
						_editUI!.RemoveChild(_editUI.DropDown);
					}
				},
				new EditorDropDownButtonDescription
				{
					Name = "Save",
					Click = _ => EditorSaveMap(),
					Enabled = () => FileName != null
				},
				new EditorDropDownButtonDescription
				{
					Name = "Save As"
				}
			});

			MapEditorTopBarButton objectsMenu = EditorDropDownButton("Objects", new[]
			{
				// true by default, mouseover shows props
				// click selects the obj and shows prop editor window
				// alt switch between overlapping objects
				new EditorDropDownButtonDescription
				{
					Name = GetObjectSelectionLabel(),
					Click = t =>
					{
						EditorSetObjectSelect(!_objectSelect);
						t.Text = GetObjectSelectionLabel();
					}
				},
				new EditorDropDownButtonDescription
				{
					Name = "Object Filters",
					Click = t => { }
				},
				new EditorDropDownButtonDescription
				{
					Name = "View Object List",
					Click = t =>
					{
						var panel = new EditorListOfItemsPanel<GameObject2D>(this, "All Objects", _objects,
							obj => { Engine.Renderer.Camera.Position = obj.Bounds.Center.ToVec3(); },
							obj => { RolloverObjects(new() {obj}); }
						);

						_editUI!.AddChild(panel);
						_editUI.RemoveChild(_editUI.DropDown);
					}
				},
				// Object creation dialog
				new EditorDropDownButtonDescription
				{
					Name = "Add Object",
					Click = t =>
					{
						List<Type> objectTypes = EditorUtility.GetTypesWhichInherit<GameObject2D>();

						var panel = new EditorListOfItemsPanel<Type>(this, "Add Object", objectTypes, EditorAddObject);
						panel.Text = "These are all classes with parameterless constructors\nthat inherit GameObject2D.\nChoose class of object to add:";
						panel.CloseOnClick = true;

						_editUI!.AddChild(panel);
						_editUI.RemoveChild(_editUI.DropDown);
					}
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

			MapEditorTopBarButton otherTools = EditorDropDownButton("Other", new[]
			{
				// Shows actions done in the editor, can be undone
				new EditorDropDownButtonDescription
				{
					Name = "Action List",
					Click = t =>
					{
						var panel = new EditorListOfItemsPanel<EditorAction>(this, "Actions", _actions ?? new List<EditorAction>(),
							obj => { }
						);

						_editUI!.AddChild(panel);
						_editUI.RemoveChild(_editUI.DropDown);
					}
				},
			});

			parentList.AddChild(fileMenu);
			parentList.AddChild(objectsMenu);
			parentList.AddChild(tilesMenu);
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
			txt.FontFile = "Editor/UbuntuMono-Regular.ttf";
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

			var propsPanel = new MapEditorObjectPropertiesPanel(obj, EditorRegisterObjectPropertyChange);
			_editUI.AddChild(propsPanel);
			_editUI.SetInputFocus(propsPanel);
		}

		private void EditorOpenContextMenuObjectModeNoSelection()
		{
			var contextMenu = new MapEditorDropdown();
			contextMenu.Offset = Engine.Host.MousePosition / _editUI!.GetScale();

			Vector2 mousePos = Engine.Host.MousePosition;
			var dropDownMenu = new[]
			{
				new EditorDropDownButtonDescription
				{
					Name = "Paste",
					Click = _ =>
					{
						var newObj = XMLFormat.From<GameObject2D>(_objectCopyClipboard);
						if (newObj != null)
						{
							bool serialized = newObj.ObjectFlags.HasFlag(ObjectFlags.Persistent);
							newObj.TrimPropertiesForSerialize(); // Ensure that the object looks like it would when loading a file.
							if (serialized) newObj.ObjectFlags |= ObjectFlags.Persistent;
							AddObject(newObj);

							Vector2 worldPos = Engine.Renderer.Camera.ScreenToWorld(mousePos).ToVec2();
							newObj.Position2 = worldPos;
						}

						_editUI.RemoveChild(_editUI.DropDown);
					},
					Enabled = () => !string.IsNullOrEmpty(_objectCopyClipboard)
				}
			};

			contextMenu.SetItems(dropDownMenu);
			_editUI.AddChild(contextMenu);
		}

		private void EditorOpenContextMenuForObject(GameObject2D obj)
		{
			var contextMenu = new MapEditorDropdown();
			contextMenu.Offset = Engine.Host.MousePosition / _editUI!.GetScale();
			contextMenu.OwningObject = obj;

			var dropDownMenu = new[]
			{
				new EditorDropDownButtonDescription
				{
					Name = "Copy",
					Click = _ =>
					{
						_objectCopyClipboard = XMLFormat.To(obj);
						_editUI.RemoveChild(_editUI.DropDown);
					}
				},
				new EditorDropDownButtonDescription
				{
					Name = "Cut",
					Click = _ =>
					{
						_objectCopyClipboard = XMLFormat.To(obj);
						RemoveObject(obj, true); // todo: register undo as delete
						_editUI.RemoveChild(_editUI.DropDown);
					}
				},
				new EditorDropDownButtonDescription
				{
					Name = "Delete",
					Click = _ =>
					{
						RemoveObject(obj, true); // todo: register undo
						_editUI.RemoveChild(_editUI.DropDown);
					}
				},
				new EditorDropDownButtonDescription
				{
					Name = "Properties",
					Click = _ =>
					{
						EditorOpenPropertiesPanelForObject(obj);
						_editUI.RemoveChild(_editUI.DropDown);
					}
				}
			};

			contextMenu.SetItems(dropDownMenu);
			_editUI.AddChild(contextMenu);
		}
	}
}