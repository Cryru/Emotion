#region Using

using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorPanelTopBar : UIBaseWindow
	{
		private bool _mouseDown;
		private Vector2 _mouseDownPos;

		public MapEditorPanelTopBar()
		{
			var closeButton = new MapEditorTopBarButton();
			closeButton.Text = "X";
			closeButton.Id = "CloseButton";
			closeButton.Anchor = UIAnchor.TopRight;
			closeButton.ParentAnchor = UIAnchor.TopRight;
			AddChild(closeButton);

			var txt = new UIText();
			txt.ScaleMode = UIScaleMode.FloatScale;
			txt.WindowColor = MapEditorColorPalette.TextColor;
			txt.Id = "PanelLabel";
			txt.FontFile = "Editor/UbuntuMono-Regular.ttf";
			txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
			txt.IgnoreParentColor = true;
			txt.Anchor = UIAnchor.CenterLeft;
			txt.ParentAnchor = UIAnchor.CenterLeft;
			txt.Margins = new Rectangle(5, 0, 0, 0);
			AddChild(txt);

			InputTransparent = false;
			MinSize = new Vector2(0, 10);
			StretchX = true;
			StretchY = true;
			MaxSize = new Vector2(DefaultMaxSize.X, 10);
		}

		protected override Vector2 InternalMeasure(Vector2 space)
		{
			return MaxSize;
		}

		protected override Vector2 BeforeLayout(Vector2 position)
		{
			_measuredSize.X = Parent!.Width;
			Width = Parent.Width;

			return base.BeforeLayout(position);
		}

		public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
		{
			if (key == Key.MouseKeyLeft)
			{
				_mouseDown = status == KeyStatus.Down;
				_mouseDownPos = Engine.Host.MousePosition;

				UIBaseWindow? panel = Parent?.Parent;
				if (panel != null)
				{
					if (status == KeyStatus.Down)
						panel.ZOffset++;
					else if (status == KeyStatus.Up)
						panel.ZOffset--;
				}
			}

			return base.OnKey(key, status, mousePos);
		}

		protected override bool UpdateInternal()
		{
			if (_mouseDown)
			{
				Vector2 mousePosNow = Engine.Host.MousePosition;
				Vector2 posDiff = mousePosNow - _mouseDownPos;
				_mouseDownPos = mousePosNow;

				UIBaseWindow panelParent = Parent!.Parent!;
				Vector2 winOffset = panelParent.Offset + posDiff / panelParent.GetScale();

				Rectangle snapArea = Controller!.Bounds;
				snapArea.Width /= panelParent.GetScale();
				snapArea.Height /= panelParent.GetScale();

				snapArea.Width -= panelParent.Width / panelParent.GetScale();
				snapArea.Height -= panelParent.Height / panelParent.GetScale();

				UIBaseWindow? topBar = Controller.GetWindowById("TopBar");
				if (topBar != null) snapArea.Y = topBar.Bounds.Bottom / topBar.GetScale();

				if (winOffset.X < snapArea.X) winOffset.X = snapArea.X;
				if (winOffset.Y < snapArea.Y) winOffset.Y = snapArea.Y;
				if (winOffset.X > snapArea.Width) winOffset.X = snapArea.Width;
				if (winOffset.Y > snapArea.Height) winOffset.Y = snapArea.Height;

				panelParent.Offset = winOffset;
				panelParent.InvalidateLayout();
			}

			return base.UpdateInternal();
		}

		protected override bool RenderInternal(RenderComposer c)
		{
			UIBaseWindow? focus = Controller.InputFocus;
			var panelParent = Parent?.Parent;
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