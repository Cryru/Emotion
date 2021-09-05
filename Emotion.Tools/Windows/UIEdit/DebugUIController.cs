#region Using

using System.Numerics;
using Emotion.Graphics;
using Emotion.Primitives;
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
            for (var i = 0; i < _activeControllers.Count; i++)
            {
                UIController controller = _activeControllers[i];
                if (controller == this) continue;
                controller.InputTransparent = true;
                controller.InvalidateInputFocus();
            }
        }

        public override void Dispose()
        {
            for (var i = 0; i < _activeControllers.Count; i++)
            {
                UIController controller = _activeControllers[i];
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
            c.RenderSprite(Position, Size, _calculatedColor);
            c.SetClipRect(new Rectangle(Position, Size));

            var scaledGridSize = (int) Maths.RoundClosest(DebugGridSize * GetScale());
            for (var y = 0; y < Size.Y; y += scaledGridSize)
            {
                for (var x = 0; x < Size.X; x += scaledGridSize)
                {
                    c.RenderOutline(new Rectangle(X + x, Y + y, scaledGridSize, scaledGridSize), Color.White * 0.2f);
                }
            }

            c.SetClipRect(null);

            Vector2 posVec2 = Position.ToVec2();
            c.RenderLine(posVec2 + new Vector2(Size.X / 2, 0), posVec2 + new Vector2(Size.X / 2, Size.Y), Color.White * 0.7f);
            c.RenderLine(posVec2 + new Vector2(0, Size.Y / 2), posVec2 + new Vector2(Size.X, Size.Y / 2), Color.White * 0.7f);
            return true;
        }
    }
}