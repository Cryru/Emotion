#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.UI;

#endregion

namespace Emotion.Game.World2D.Editor;

public class MapEditorObjectNameplate : UIWorldAttachedWindow
{
	public GameObject2D Object;

	private UIText _label;
	private UISolidColor _bg;

	private ReasonStack _selectionStack = new();

	public MapEditorObjectNameplate()
	{
		StretchX = true;
		StretchY = true;
		Anchor = UIAnchor.BottomCenter;
		ParentAnchor = UIAnchor.TopLeft;
		ZOffset = -10;
		HandleInput = true;

		var bg = new UISolidColor();
		bg.StretchX = true;
		bg.StretchY = true;
		bg.WindowColor = Color.Black * 0.4f;
		bg.Paddings = new Rectangle(3, 3, 3, 3);
		_bg = bg;
		AddChild(bg);

		var txt = new UIText();
		txt.ParentAnchor = UIAnchor.CenterCenter;
		txt.Anchor = UIAnchor.CenterCenter;
		txt.ScaleMode = UIScaleMode.FloatScale;
		txt.WindowColor = MapEditorColorPalette.TextColor * 0.9f;
		txt.FontFile = "Editor/UbuntuMono-Regular.ttf";
		txt.FontSize = MapEditorColorPalette.EditorButtonTextSize;
		txt.IgnoreParentColor = true;
		_label = txt;
		bg.AddChild(txt);
	}

	public override Vector2 CalculateContentPos(Vector2 parentPos, Vector2 parentSize, Rectangle parentScaledPadding)
	{
		parentSize = _measuredSize;
		return base.CalculateContentPos(parentPos, parentSize, parentScaledPadding);
	}

	public void AttachToObject(GameObject2D obj)
	{
		Object = obj;
	}

	public void SetSelected(bool selected, string reason)
	{
		if (selected)
			_selectionStack.AddReason(reason);
		else
			_selectionStack.RemoveReason(reason);

		selected = _selectionStack.AnyReason();
		bool rollover = _selectionStack.HasReason("Rollover");

		_bg.WindowColor = selected ? Color.White * (rollover ? 0.6f : 0.4f) : Color.Black * 0.4f;
		_label.WindowColor = selected ? Color.Black * 0.9f : MapEditorColorPalette.TextColor * 0.9f;
	}

	protected override bool UpdateInternal()
	{
		AttachToPosition(new Vector3(Object.Bounds.X + Object.Bounds.Width / 2f, Object.Bounds.Y, 0));

		_label.Text = $"{Object.ObjectName ?? "null"}{(Object.ObjectState != ObjectState.Alive ? " (NotSpawned)" : "")}";
		return base.UpdateInternal();
	}
}