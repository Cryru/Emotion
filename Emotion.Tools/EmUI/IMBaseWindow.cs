#region Using

using System.Numerics;
using Emotion.Common;
using Emotion.Graphics;
using Emotion.IO;
using Emotion.Primitives;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Tools.EmUI
{
    public class IMBaseWindow : UIBaseWindow
    {
        public string Title
        {
            get
            {
                var winTitle = (UIText?) GetWindowById("WindowTitle");
                if (winTitle == null) return "";

                return winTitle.Text;
            }
            set
            {
                var winTitle = (UIText?) GetWindowById("WindowTitle");
                if (winTitle == null) return;

                winTitle.Text = value;
            }
        }

        public static FontAsset? ToolsFont;

        public static Color HeaderColor = new Color(50, 35, 50);
        public static Color MainColor = new Color(70, 55, 70);
        public static Color MainColorInner = new Color(70, 70, 80);
        public static Color MainColorLight = new Color(130, 80, 130);
        public static Color MainColorLightMouseIn = new Color(170, 120, 210);
        public static Color TextColor = new Color(240, 245, 250);
        public static int FontSize = 7;

        public IMBaseWindow(string title, bool hasClose = true)
        {
            ZOffset = 90;
            InputTransparent = false;
            ToolsFont ??= Engine.AssetLoader.Get<FontAsset>("SourceCodePro-Regular.ttf");

            LayoutMode = LayoutMode.VerticalList;
            StretchY = true;
            ListSpacing = new Vector2(0, 4);

            var header = new UISolidColor();
            header.WindowColor = HeaderColor;
            header.StretchY = true;
            header.Paddings = new Rectangle(3, 3, 3, 3);
            header.InputTransparent = false;
            header.LayoutMode = LayoutMode.VerticalList;
            header.ListSpacing = new Vector2(0, 2);
            header.ZOffset = -10;
            header.Id = "Header";
            AddChild(header);

            var lineOnBottom = new UISolidColor();
            lineOnBottom.WindowColor = MainColorLightMouseIn;
            lineOnBottom.MaxSize = new Vector2(DefaultMaxSize.X, 1);
            lineOnBottom.Anchor = UIAnchor.BottomLeft;
            lineOnBottom.ParentAnchor = UIAnchor.BottomLeft;
            lineOnBottom.Offset = new Vector2(-header.Paddings.X, header.Paddings.Y);
            lineOnBottom.RelativeTo = "Header";
            AddChild(lineOnBottom);

            var headerFirstRow = new UIBaseWindow();
            headerFirstRow.InputTransparent = false;
            headerFirstRow.StretchY = true;
            header.AddChild(headerFirstRow);

            var label = new EditorTextWindow();
            label.Id = "WindowTitle";
            label.Anchor = UIAnchor.CenterLeft;
            label.ParentAnchor = UIAnchor.CenterLeft;
            headerFirstRow.AddChild(label);

            if (hasClose)
            {
                var closeButton = new TextCallbackButton("X");
                closeButton.Anchor = UIAnchor.CenterRight;
                closeButton.ParentAnchor = UIAnchor.CenterRight;
                closeButton.OnClickedProxy = _ => { CloseWindow(); };
                headerFirstRow.AddChild(closeButton);
            }

            var headerSecondRow = new UIBaseWindow();
            headerSecondRow.InputTransparent = false;
            headerSecondRow.StretchY = true;
            headerSecondRow.StretchX = true;
            headerSecondRow.LayoutMode = LayoutMode.HorizontalList;
            headerSecondRow.ListSpacing = new Vector2(3, 0);
            header.AddChild(headerSecondRow);
            MenuBarButtons(headerSecondRow);

            Title = title;
        }

        protected virtual void MenuBarButtons(UIBaseWindow headerSecondRow)
        {
        }

        public void CloseWindow()
        {
            UIBaseWindow? controllerChild = this;
            while (controllerChild.Parent != Controller && controllerChild.Parent != null) controllerChild = controllerChild.Parent;
            if (controllerChild.Parent == null) return;
            Controller?.RemoveChild(controllerChild);
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Position, Size, MainColor);
            return base.RenderInternal(c);
        }
    }
}