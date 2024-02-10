#nullable enable

#region Using

using Emotion.Game.World;
using Emotion.Graphics;

#endregion

namespace Emotion.Game.World2D.Editor;

public partial class World2DEditor
{
    protected override bool CanSelectObjects()
    {
        return base.CanSelectObjects() && !IsTileEditorOpen();
    }

    protected override void RenderObjectSelection(RenderComposer c)
    {
        BaseMap? map = CurrentMap;
        if (map == null) return;

        if (!CanSelectObjects()) return;

        // Show selection of object, if any.
        if (_editUI?.DropDown?.OwningObject is BaseGameObject objectWithContextMenu)
        {
            Rectangle bound = objectWithContextMenu.GetBoundsForQuadTree();
            c.RenderSprite(bound, Color.White * 0.3f);
        }

        if (_selectedObject != null && _selectedObject.ObjectState != ObjectState.Destroyed)
        {
            Rectangle bound = _selectedObject.GetBoundsForQuadTree();
            c.RenderSprite(bound, Color.White * 0.3f);
        }

        if (_rolloverObject != null)
        {
            Rectangle bound = _rolloverObject.GetBoundsForQuadTree();
            c.RenderSprite(bound, Color.White * 0.3f);
        }

        foreach (BaseGameObject obj in map.ObjectsEnum(null))
        {
            Rectangle bound = obj.GetBoundsForQuadTree();

            if (!obj.ObjectFlags.HasFlag(ObjectFlags.Persistent))
            {
                c.RenderLine(bound.TopLeft, bound.BottomRight, Color.Black * 0.5f);
                c.RenderLine(bound.TopRight, bound.BottomLeft, Color.Black * 0.5f);
            }
            else if (obj.ObjectState == ObjectState.ConditionallyNonSpawned)
            {
                c.RenderSprite(bound, Color.Magenta * 0.2f);
            }

            c.RenderOutline(bound, Color.White * 0.4f);
        }
    }
}