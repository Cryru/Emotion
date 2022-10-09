#region Using

using System;
using System.Collections.Generic;
using System.Numerics;
using Emotion.Game.Text;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.Primitives;
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
            topBar.ZOffset = 100; // Above dropdown click eater, but below dropdown.
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

        private class DropDownButtonDescription
        {
            public string Name = null!;
            public Action<MapEditorTopBarButton>? Click;
            public Func<bool>? Enabled;
        }

        private void SpawnDropDown(MapEditorTopBarButton button, DropDownButtonDescription[] menuButtons)
        {
            var dropDownWin = new UIDropDown();
            dropDownWin.InputTransparent = false;
            dropDownWin.WindowColor = MapEditorColorPalette.ActiveButtonColor;
            dropDownWin.Anchor = UIAnchor.TopLeft;
            dropDownWin.ParentAnchor = UIAnchor.BottomLeft;
            dropDownWin.StretchX = true;
            dropDownWin.StretchY = true;
            dropDownWin.Offset = new Vector2(-5, 1);

            var innerBg = new UISolidColor();
            innerBg.IgnoreParentColor = true;
            innerBg.InputTransparent = false;
            innerBg.WindowColor = MapEditorColorPalette.BarColor.SetAlpha(255);
            innerBg.StretchX = true;
            innerBg.StretchY = true;
            innerBg.Paddings = new Rectangle(3, 3, 3, 3);

            dropDownWin.AddChild(innerBg);

            var list = new UICallbackListNavigator();
            list.IgnoreParentColor = true;
            list.LayoutMode = LayoutMode.VerticalList;
            list.InputTransparent = false;
            list.StretchX = true;
            list.StretchY = true;
            list.ChildrenAllSameWidth = true;
            list.ListSpacing = new Vector2(0, 2);

            for (var i = 0; i < menuButtons.Length; i++)
            {
                DropDownButtonDescription buttonMeta = menuButtons[i];

                var ddButton = new MapEditorTopBarButton();
                ddButton.StretchX = true;
                ddButton.StretchY = true;
                ddButton.InputTransparent = false;
                ddButton.Text = buttonMeta.Name;
                ddButton.MinSize = new Vector2(50, 0);
                ddButton.OnClickedProxy = _ =>
                {
                    if (buttonMeta.Click == null) return;
                    if (buttonMeta.Enabled != null)
                    {
                        bool enabled = buttonMeta.Enabled();
                        if (!enabled) return;
                    }

                    buttonMeta.Click(ddButton);
                };
                ddButton.Enabled = buttonMeta.Enabled?.Invoke() ?? true;

                list.AddChild(ddButton);
            }

            innerBg.AddChild(list);

            dropDownWin.RelativeTo = $"button{button.Text}";
            _topBarDropDown = dropDownWin;
            _dropDownOpen = button;

            List<UIBaseWindow> siblings = button.Parent!.Children!;
            for (var i = 0; i < siblings.Count; i++)
            {
                UIBaseWindow child = siblings[i];
                if (child is MapEditorTopBarButton but) but.SetDropDownMode(child == button, dropDownWin);
            }

            button.Controller!.AddChild(dropDownWin);
        }

        private MapEditorTopBarButton DropDownButton(string label, DropDownButtonDescription[] menuButtons)
        {
            var button = new MapEditorTopBarButton();
            button.Text = label;
            button.OnMouseEnterProxy = _ =>
            {
                if (_topBarDropDown != null && _topBarDropDown.Controller != null)
                {
                    if (_dropDownOpen != button)
                    {
                        _topBarDropDown.Controller.RemoveChild(_topBarDropDown);
                        SpawnDropDown(button, menuButtons);
                    }
                }
            };
            button.OnClickedProxy = _ =>
            {
                if (_topBarDropDown != null && _topBarDropDown.Controller != null)
                {
                    _topBarDropDown.Controller.RemoveChild(_topBarDropDown);
                    if (_dropDownOpen == button)
                    {
                        _dropDownOpen = null;
                        return;
                    }
                }

                SpawnDropDown(button, menuButtons);
            };
            button.Id = $"button{label}";

            return button;
        }

        private void AttachTopBarButtons(UIBaseWindow parentList)
        {
            string GetObjectSelectionLabel()
            {
                return $"Selection: {(_objectSelect ? "Enabled" : "Disabled")}";
            }

            MapEditorTopBarButton fileMenu = DropDownButton("File", new[]
            {
                new DropDownButtonDescription
                {
                    Name = "Save",
                    Click = _ => EditorSaveMap(),
                    Enabled = () => FileName != null
                },
                new DropDownButtonDescription
                {
                    Name = "Save As"
                }
            });

            MapEditorTopBarButton objectsMenu = DropDownButton("Objects", new[]
            {
                // true by default, mouseover shows props
                // click selects the obj and shows prop editor window
                // alt switch between overlapping objects
                new DropDownButtonDescription
                {
                    Name = GetObjectSelectionLabel(),
                    Click = (t) =>
                    {
                        _objectSelect = !_objectSelect;
                        t.Text = GetObjectSelectionLabel();
                    }
                },
                // Object creation dialog
                new DropDownButtonDescription
                {
                    Name = "Add Object (Ctrl+N)",
                    Click = (t) =>
                    {
                        var test = new MapEditorAddObjectPanel(EditorAddObject);
                        _editUI!.AddChild(test);
                        _topBarDropDown!.Parent!.RemoveChild(_topBarDropDown);
                    }
                },
                new DropDownButtonDescription
                {
                    Name = "Copy Selected (Ctrl+C)"
                },
                new DropDownButtonDescription
                {
                    Name = "Paste Selected (Ctrl+V)"
                },
                new DropDownButtonDescription
                {
                    Name = "Delete Selected (Delete)"
                }
            });

            MapEditorTopBarButton tilesMenu = DropDownButton("Tiles", new[]
            {
                // false by default, mouseover shows props, alt switch layers
                new DropDownButtonDescription
                {
                    Name = $"Selection: {(_tileSelect ? "Enabled" : "Disabled")}"
                },
                // Shows layers, tilesets and other special editors for this mode, disables object selection while open
                new DropDownButtonDescription
                {
                    Name = "Open Tile Editor"
                },
            });

            parentList.AddChild(fileMenu);
            parentList.AddChild(objectsMenu);
            parentList.AddChild(tilesMenu);
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
    }
}