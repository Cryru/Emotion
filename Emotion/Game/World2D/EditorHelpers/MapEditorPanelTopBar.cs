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

			var closeButton = new MapEditorTopBarButton();
			closeButton.Text = "X";
			closeButton.Id = "CloseButton";
			closeButton.Anchor = UIAnchor.TopRight;
			closeButton.ParentAnchor = UIAnchor.TopRight;
			closeButton.RenderInactiveBG = false;
			AddChild(closeButton);

			HandleInput = true;
			MinSize = new Vector2(0, 10);
			StretchX = true;
			StretchY = true;
			MaxSize = new Vector2(DefaultMaxSize.X, 10);
			LayoutMode = LayoutMode.HorizontalList;
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
				return false;
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

				UIBaseWindow panelParent = Parent!;
				float parentScale = panelParent.GetScale();

				var panelBounds = new Rectangle(panelParent.Offset * parentScale + posDiff, panelParent.Size);

				Rectangle snapArea = Controller!.Bounds;
				snapArea.Width += panelBounds.Width / 2f;
				snapArea.Height += panelBounds.Height / 2f;

				UIBaseWindow? topBar = Controller.GetWindowById("TopBar");
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