#nullable enable

#region Using

using Emotion.Game.World;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
	protected override void RenderObjectSelection(RenderComposer c)
	{
		BaseMap? map = CurrentMap;
		if (map == null) return;

		if (!_canObjectSelect) return;

		// Show selection of object, if any.
		if (_editUI?.DropDown?.OwningObject is BaseGameObject objectWithContextMenu)
		{
			Rectangle bound = objectWithContextMenu.Bounds;
			c.RenderSprite(bound, Color.White * 0.3f);
		}

		if (_selectedObject != null && _selectedObject.ObjectState != ObjectState.Destroyed)
		{
			Rectangle bound = _selectedObject.Bounds;
			c.RenderSprite(bound, Color.White * 0.3f);
		}

		if (_rolloverObject != null)
		{
			Rectangle bound = _rolloverObject.Bounds;
			c.RenderSprite(bound, Color.White * 0.3f);
		}

		foreach (BaseGameObject obj in map.GetObjects(true))
		{
			Rectangle bounds = obj.Bounds;

			if (!obj.ObjectFlags.HasFlag(ObjectFlags.Persistent))
			{
				c.RenderLine(bounds.TopLeft, bounds.BottomRight, Color.Black * 0.5f);
				c.RenderLine(bounds.TopRight, bounds.BottomLeft, Color.Black * 0.5f);
			}
			else if (obj.ObjectState == ObjectState.ConditionallyNonSpawned)
			{
				c.RenderSprite(bounds, Color.Magenta * 0.2f);
			}

			c.RenderOutline(bounds, Color.White * 0.4f);
		}
	}
}