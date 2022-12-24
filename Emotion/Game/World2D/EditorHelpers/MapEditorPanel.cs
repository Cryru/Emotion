#region Using

using System.Numerics;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D.EditorHelpers
{
    public class MapEditorPanel : UIBaseWindow
    {
	    public string Header;

        protected UIBaseWindow _contentParent;
        private UIBaseWindow _container;
        private bool _center;

        public MapEditorPanel(string header)
        {
	        Header = header;
	        InputTransparent = false;
            StretchX = true;
            StretchY = true;

            ParentAnchor = UIAnchor.CenterCenter;
            Anchor = UIAnchor.CenterCenter;
            _center = true;
        }

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);

			var container = new UIBaseWindow();
			container.StretchX = true;
			container.StretchY = true;
			container.MinSize = new Vector2(100, 100);
			container.MaxSize = new Vector2(500, 200);
			container.InputTransparent = false;
			_container = container;

			var topBar = new MapEditorPanelTopBar();
			container.AddChild(topBar);

			var closeButton = (MapEditorTopBarButton) topBar.GetWindowById("CloseButton")!;
			closeButton.OnClickedProxy = _ => { controller.RemoveChild(this); };

			var panelLabel = (UIText) topBar.GetWindowById("PanelLabel")!;
			panelLabel.Text = Header;

			var contentParent = new UIBaseWindow();
			contentParent.StretchX = true;
			contentParent.StretchY = true;
			contentParent.Id = "Content";
			contentParent.Margins = new Rectangle(5, 15, 5, 5);
			_contentParent = contentParent;
			container.AddChild(contentParent);

			AddChild(container);
		}

		protected override void AfterLayout()
        {
            base.AfterLayout();

            if (_center)
            {
                Offset = Position2 / GetScale();
                ParentAnchor = UIAnchor.TopLeft;
                Anchor = UIAnchor.TopLeft;
                _center = false;
            }
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(_container.Bounds, MapEditorColorPalette.BarColor * 0.7f);
            c.RenderOutline(_container.Bounds, MapEditorColorPalette.ActiveButtonColor * 0.9f, 2);
            return base.RenderInternal(c);
        }
    }
}