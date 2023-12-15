#region Using

using System.Numerics;
using Emotion.Graphics;
using Emotion.Primitives;
using Emotion.Tools.Editors;
using Emotion.UI;
using Emotion.Utility;

#endregion

namespace Emotion.Tools.Windows.UIEdit
{
    public class DebugUIController : UIController
    {
        public int DebugGridSize = 20;

        public DebugUIController()
        {
            WindowColor = new Color(32, 32, 32);
            for (var i = 0; i < _allControllers.Count; i++)
            {
                UIController controller = _allControllers[i];
                if (controller == this) continue;
                controller.InputTransparent = true;
                controller.InvalidateInputFocus();
            }
        }

        public override void Dispose()
        {
            for (var i = 0; i < _allControllers.Count; i++)
            {
                UIController controller = _allControllers[i];
                if (controller == this) continue;
                controller.InputTransparent = false;
                controller.InvalidateInputFocus();
            }

            base.Dispose();
        }

        protected override bool UpdateInternal()
        {
            _updateLayout = true;
            _updateColor = true;
            return base.UpdateInternal();
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            if (!base.RenderInternal(c)) return false;

            c.SetClipRect(new Rectangle(Position, Size));
            EditorHelpers.RenderToolGrid(c, Position, Size, _calculatedColor, (int) Maths.RoundClosest(DebugGridSize * GetScale()));
            c.SetClipRect(null);

            Vector2 posVec2 = Position.ToVec2();
            c.RenderLine(posVec2 + new Vector2(Size.X / 2, 0), posVec2 + new Vector2(Size.X / 2, Size.Y), Color.White * 0.7f);
            c.RenderLine(posVec2 + new Vector2(0, Size.Y / 2), posVec2 + new Vector2(Size.X, Size.Y / 2), Color.White * 0.7f);
            return true;
        }
    }
}