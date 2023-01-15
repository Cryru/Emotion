#region Using

using Emotion.Graphics;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.EditorHelpers
{
	public class MapEditorModal : UIBaseWindow
	{
		private MapEditorPanel _panel;

		public MapEditorModal(MapEditorPanel panel)
		{
			_panel = panel;
			InputTransparent = false;
		}

		public override void AttachedToController(UIController controller)
		{
			base.AttachedToController(controller);
			controller.AddChild(_panel);
		}

		protected override bool UpdateInternal()
		{
			if (_panel.Controller == null) Parent!.RemoveChild(this);

			return base.UpdateInternal();
		}

		protected override bool RenderInternal(RenderComposer c)
		{
			c.RenderSprite(Bounds, Color.Black * 0.7f);
			return true;
		}
	}
}