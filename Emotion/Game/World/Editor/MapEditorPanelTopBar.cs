﻿#region Using

using Emotion.Common.Input;
using Emotion.Editor.EditorHelpers;
using Emotion.Graphics;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#endregion

#nullable enable

namespace Emotion.Game.World.Editor
{
    public class MapEditorPanelTopBar : UIBaseWindow
    {
        public bool CanMove = true;

        private bool _mouseDown;
        private Vector2 _mouseDownPos;

        public MapEditorPanelTopBar()
        {
            var txt = new UIText();
            txt.ScaleMode = UIScaleMode.FloatScale;
            txt.WindowColor = MapEditorColorPalette.TextColor;
            txt.Id = "PanelLabel";
            txt.FontFile = "Editor/UbuntuMono-Regular.ttf";
            txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
            txt.IgnoreParentColor = true;
            txt.Anchor = UIAnchor.CenterLeft;
            txt.ParentAnchor = UIAnchor.CenterLeft;
            txt.Margins = new Rectangle(5, 0, 5, 0);
            AddChild(txt);

            var closeButton = new EditorButton();
            closeButton.Text = "X";
            closeButton.Id = "CloseButton";
            closeButton.Anchor = UIAnchor.TopRight;
            closeButton.ParentAnchor = UIAnchor.TopRight;

            AddChild(closeButton);

            HandleInput = true;
            StretchX = true;
            StretchY = true;
            GrowY = false;
            MaxSizeY = 10;
            LayoutMode = LayoutMode.HorizontalList;
            Id = "TopBar";

            UseNewLayoutSystem = true;
        }

        public override bool OnKey(Key key, KeyState status, Vector2 mousePos)
        {
            if (key == Key.MouseKeyLeft)
            {
                _mouseDown = status == KeyState.Down;
                _mouseDownPos = Engine.Host.MousePosition;
                return false;
            }

            return base.OnKey(key, status, mousePos);
        }

        protected override bool UpdateInternal()
        {
            if (_mouseDown && CanMove)
            {
                Vector2 mousePosNow = Engine.Host.MousePosition;
                Vector2 posDiff = mousePosNow - _mouseDownPos;
                _mouseDownPos = mousePosNow;

                UIBaseWindow panelParent = Parent!;
                float parentScale = panelParent.GetScale();

                var panelBounds = new Rectangle(panelParent.Offset * parentScale + posDiff, panelParent.Size);

                Rectangle snapArea = Controller!.Bounds;
                snapArea.Width += panelBounds.Width / 2f;
                snapArea.Height += panelBounds.Height / 2f;

                UIBaseWindow? topBar = Controller.GetWindowById("EditorTopBar");
                if (topBar != null)
                {
                    float topBarPos = topBar.Bounds.Bottom;
                    snapArea.Y = topBarPos;
                    snapArea.Height -= topBarPos;
                }

                panelParent.Offset = snapArea.SnapRectangleInside(panelBounds) / parentScale;
                panelParent.InvalidateLayout();
            }

            return base.UpdateInternal();
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            UIBaseWindow? focus = Controller!.InputFocus;
            UIBaseWindow? panelParent = Parent!.Parent;
            if (focus != null && panelParent != null && focus.IsWithin(panelParent))
                c.RenderSprite(Bounds, _mouseDown || MouseInside ? MapEditorColorPalette.ActiveButtonColor : MapEditorColorPalette.ButtonColor);
            else
                c.RenderSprite(Bounds, Color.Black * 0.5f);

            c.RenderLine(Bounds.TopLeft, Bounds.TopRight, Color.White * 0.5f);
            c.RenderLine(Bounds.TopLeft, Bounds.BottomLeft, Color.White * 0.5f);
            c.RenderLine(Bounds.TopRight, Bounds.BottomRight, Color.White * 0.5f);
            return base.RenderInternal(c);
        }
    }
}