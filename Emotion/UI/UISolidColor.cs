#region Using

using Emotion.Graphics;

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