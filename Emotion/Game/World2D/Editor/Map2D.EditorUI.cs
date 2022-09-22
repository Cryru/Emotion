#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Game.Text;
using Emotion.Game.World2D.EditorHelpers;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.UI;

#endregion

#nullable enable

// Provides pseudo-ui templates for the editor

namespace Emotion.Game.World2D
{
    public partial class Map2D
    {
        protected UIBaseWindow GetEditorTopBar()
        {
            var topBar = new UISolidColor();
            topBar.MinSize = new Vector2(0, 15);
            topBar.MaxSize = new Vector2(UIBaseWindow.DefaultMaxSize.X, 15);
            topBar.ScaleMode = UIScaleMode.FloatScale;
            topBar.WindowColor = MapEditorColorPalette.BarColor;
            topBar.InputTransparent = false;

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

        protected void AttachTopBarButtons(UIBaseWindow parentList)
        {
            string GetObjectSelectionLabel()
            {
                return $"Object Select: {_objectSelect}";
            }

            var objectSelectionToggle = new MapEditorTopBarButton();
            objectSelectionToggle.Text = GetObjectSelectionLabel();
            objectSelectionToggle.OnClickedProxy = _ =>
            {
                _objectSelect = !_objectSelect;
                objectSelectionToggle.Text = GetObjectSelectionLabel();
                SelectObject(null);
            };
            objectSelectionToggle.MinSize = new Vector2(65, 0);
            parentList.AddChild(objectSelectionToggle);

            var saveButton = new MapEditorTopBarButton();
            saveButton.Text = "Save";
            saveButton.OnClickedProxy = _ =>
            {
                EditorSaveMap();
            };
            parentList.AddChild(saveButton);

            var buttonTest = new MapEditorTopBarButton();
            buttonTest.Text = "Layers";
            buttonTest.OnClickedProxy = _ => { Engine.Log.Warning("Hi", "bruh"); };
            parentList.AddChild(buttonTest);
        }

        protected UIBaseWindow GetWorldAttachInspectWindow()
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
            txt.FontSize = 6;
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