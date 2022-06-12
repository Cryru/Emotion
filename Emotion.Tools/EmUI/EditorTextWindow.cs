#region Using

using Emotion.UI;

#endregion

namespace Emotion.Tools.EmUI
{
    public class EditorTextWindow : UIText
    {
        public EditorTextWindow()
        {
            FontSize = IMBaseWindow.FontSize;
            FontFile = IMBaseWindow.ToolsFont?.Name;
            WindowColor = IMBaseWindow.TextColor;
            Anchor = UIAnchor.CenterLeft;
            ParentAnchor = UIAnchor.CenterLeft;
        }
    }
}