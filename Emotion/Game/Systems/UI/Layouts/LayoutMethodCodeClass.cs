#nullable enable

using static Emotion.Game.Systems.UI2.UILayoutMethod;

namespace Emotion.Game.Systems.UI;

public partial class UIBaseWindow
{
    public abstract class LayoutMethodCodeClass
    {
        /// <summary>
        /// Calls the Layout_Step1_Measure on all children.
        /// and determine the measured size of the self based on the measured sizes of their children (child.CalculatedMetrics.Size)
        /// The measured size is the minimum size the window could take.
        /// </summary>
        public abstract void Step1_Measure(UIBaseWindow self, out IntVector2 childrenSize);

        /// <summary>
        /// Calls the Layout_Step2_Grow on all children.
        /// and increase the calculated size of children to fill the available size (if their sizing mode is grow)
        /// </summary>
        public abstract void Step2_Grow(UIBaseWindow self);

        /// <summary>
        /// Calls Layout_Step3_Position on all children and determines the position of each child.
        /// </summary>
        public abstract void Step3_Position(UIBaseWindow self, IntRectangle rect);

        #region Layout Helpers

        public static bool SkipWindowLayout(UIBaseWindow window)
        {
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
