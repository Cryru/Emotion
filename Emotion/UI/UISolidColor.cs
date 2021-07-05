#region Using

using Emotion.Graphics;
using Emotion.Primitives;

#endregion

namespace Emotion.UI
{
    public class UISolidColor : UIBaseWindow
    {
        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Position, Size, _calculatedColor);
            return true;
        }
    }
}