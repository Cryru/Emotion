#region Using

using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.UI
{
    public class UISolidColor : UIBaseWindow
    {
        protected override bool RenderInternal(RenderComposer c, ref Color windowColor)
        {
            c.RenderSprite(Position, Size, windowColor);
            return true;
        }
    }
}