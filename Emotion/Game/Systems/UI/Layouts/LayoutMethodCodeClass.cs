#nullable enable

using static Emotion.Game.Systems.UI2.UILayoutMethod;

namespace Emotion.Game.Systems.UI;

public partial class UIBaseWindow
{
    public abstract class LayoutMethodCodeClass
    {
        public virtual void PreLayout(UIBaseWindow self)
        {

        }
        public abstract int GetMainAxis(UIBaseWindow self);
        public abstract int GetChildrenSize(UIBaseWindow self, int axis);
        public abstract void GrowShrinkAxis(UIBaseWindow self, int axis);
        public abstract void PositionChildren(UIBaseWindow self);

        #region Layout Helpers

        public static UISizing GetSizingInDirection(UIBaseWindow window, int axis)
        {
            return axis == 0 ? window.Layout.SizingX : window.Layout.SizingY;
        }

        public static bool SkipWindowLayout(UIBaseWindow window)
        {
            if (window.IsLoading()) return true;
            if (!window.Visuals.Visible && window.Visuals.DontTakeSpaceWhenHidden) return true;
            if (window._useCustomLayout) return true;
            return false;
        }

        public static ListLayoutItemsAlign GetItemsAlignAcrossFromList(UIMethodName listType, UIAnchor anchor)
        {
            if (anchor == UIAnchor.TopLeft)
                return ListLayoutItemsAlign.Beginning;

            if (listType == UIMethodName.HorizontalList)
            {
                if (anchor == UIAnchor.CenterLeft || anchor == UIAnchor.CenterRight || anchor == UIAnchor.CenterCenter)
                    return ListLayoutItemsAlign.Center;
                if (anchor == UIAnchor.BottomLeft || anchor == UIAnchor.BottomRight || anchor == UIAnchor.BottomCenter)
                    return ListLayoutItemsAlign.End;
            }
            else if (listType == UIMethodName.VerticalList)
            {
                if (anchor == UIAnchor.TopCenter || anchor == UIAnchor.BottomCenter || anchor == UIAnchor.CenterCenter)
                    return ListLayoutItemsAlign.Center;
                if (anchor == UIAnchor.TopRight || anchor == UIAnchor.BottomRight || anchor == UIAnchor.CenterRight)
                    return ListLayoutItemsAlign.End;
            }

            return ListLayoutItemsAlign.Beginning;
        }

        #endregion
    }
}
