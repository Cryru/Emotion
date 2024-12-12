using Emotion.Game.World.Editor;
using Emotion.UI;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools;

public partial class EditorWindowFileSupport
{
    public class EditorDropDown : UIDropDown
    {
        public EditorDropDown(UIBaseWindow spawningWindow) : base(spawningWindow)
        {
        }

        protected override bool RenderInternal(RenderComposer c)
        {
            c.RenderSprite(Position, Size, MapEditorColorPalette.BarColor * 0.8f);
            c.RenderOutline(Position, Size, MapEditorColorPalette.ActiveButtonColor, 2 * GetScale(), false);

            return base.RenderInternal(c);
        }
    }
}
