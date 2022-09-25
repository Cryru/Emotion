#region Using

using System.Numerics;
using Emotion.Graphics;
using Emotion.Platform.Input;
using Emotion.Primitives;

#endregion

#nullable enable

namespace Emotion.UI
{
    public class UIDropDown : UIBaseWindow
    {
        private UIDropDownMouseDetect? _mouseDetectOutside;

        public UIDropDown()
        {
            ZOffset = 110; // Needs to be one above the mouse detector.
        }

        public override void AttachedToController(UIController controller)
        {
            base.AttachedToController(controller);
            controller.SetInputFocus(this);

            _mouseDetectOutside = new UIDropDownMouseDetect(this);
            controller.AddChild(_mouseDetectOutside);
        }

        public override void DetachedFromController(UIController controller)
        {
            base.DetachedFromController(controller);
            controller.RemoveChild(_mouseDetectOutside);
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
    }
}