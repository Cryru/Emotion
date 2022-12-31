#region Using

using Emotion.Graphics;
using Emotion.Platform.Input;

#endregion

#nullable enable

namespace Emotion.UI
{
	public class UIDropDown : UIBaseWindow
	{
		public object? OwningObject = null;

		public UIDropDown()
		{
			CodeGenerated = true;
		}
	}

	// Detects when the mouse clicks outside the dropdown
	public class UIDropDownMouseDetect : UIBaseWindow
	{
		private UIDropDown DropDown;

		public UIDropDownMouseDetect(UIDropDown dropdown)
		{
			DropDown = dropdown;
			CodeGenerated = true;
			InputTransparent = false;
			ZOffset = 99;
		}

		public override bool OnKey(Key key, KeyStatus status, Vector2 mousePos)
		{
			if (key > Key.MouseKeyStart && key < Key.MouseKeyEnd)
				if (status == KeyStatus.Down && !DropDown.IsPointInside(mousePos))
					DropDown.Parent!.RemoveChild(DropDown);

			return base.OnKey(key, status, mousePos);
		}

		protected override void RenderChildren(RenderComposer c)
		{
			c.RenderSprite(Bounds, Color.Red * 0.3f);
			base.RenderChildren(c);
		}

		public override UIBaseWindow? FindMouseInput(Vector2 pos)
		{
			// Blocker should never block the dropdown itself, even if it is nested somewhere.
			UIBaseWindow? dropDownFocus = DropDown.FindMouseInput(pos);
			if (dropDownFocus != null) return dropDownFocus;

			return base.FindMouseInput(pos);
		}
	}
}