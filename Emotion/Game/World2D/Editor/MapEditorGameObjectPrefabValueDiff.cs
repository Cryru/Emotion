﻿#region Using

using Emotion.Game.World2D.EditorHelpers;
using Emotion.Graphics;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Game.World2D.Editor;

public class MapEditorGameObjectPrefabValueDiff : UIBaseWindow
{
	public MapEditorGameObjectPrefabValueDiff()
	{
		MinSize = new Vector2(10);
		MaxSize = new Vector2(10);
		WindowColor = new Color("df9821");

		MapEditorLabel text = new MapEditorLabel("P");
		text.WindowColor = Color.Black;
		text.Anchor = UIAnchor.CenterCenter;
		text.ParentAnchor = UIAnchor.CenterCenter;
		AddChild(text);
	}

	protected override bool RenderInternal(RenderComposer c)
	{
		c.RenderCircle(Position, Width / 2f, _calculatedColor);

		return base.RenderInternal(c);
	}
}