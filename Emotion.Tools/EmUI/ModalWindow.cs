#region Using

using Emotion.Primitives;
using Emotion.UI;

#endregion

namespace Emotion.Tools.EmUI
{
    // Should be full screen.
    public class ModalWindow : UISolidColor
    {
        public UIBaseWindow ModalContent;

        public ModalWindow(string title, bool hasClose = false)
        {
            ZOffset = 100;
            InputTransparent = false;
            WindowColor = Color.Black * 0.7f;

            var modalContent = new IMBaseWindow(title, hasClose);
            modalContent.IgnoreParentColor = true;
            modalContent.Anchor = UI.UIAnchor.CenterCenter;
            modalContent.ParentAnchor = UI.UIAnchor.CenterCenter;
            modalContent.MaxSize = new System.Numerics.Vector2(200, UIBaseWindow.DefaultMaxSize.Y);
            AddChild(modalContent);
            ModalContent = modalContent;
        }
    }
}