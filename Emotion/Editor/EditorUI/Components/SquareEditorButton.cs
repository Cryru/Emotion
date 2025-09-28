#nullable enable

using Emotion.Game.Systems.UI;
using Emotion.Game.Systems.UI2;

namespace Emotion.Editor.EditorUI.Components;

public class SquareEditorButton : EditorButton
{
    public SquareEditorButton() : base()
    {
        Visuals.BorderColor = Color.White * 0.5f;
        Visuals.Border = 1;

        Layout.Padding = new UISpacing(3, 3, 3, 3);
        Layout.SizingX = UISizing.Fit(); // UISizing.Fixed(30)
        Layout.SizingY = UISizing.Fit(); // UISizing.Fixed(30)
        _label.Layout.AnchorAndParentAnchor = UIAnchor.CenterCenter;
    }

    public SquareEditorButton(string name) : base(name)
    {
        Visuals.BorderColor = Color.White * 0.5f;
        Visuals.Border = 1;

        Layout.Padding = new UISpacing(3, 3, 3, 3);
        Layout.SizingX = UISizing.Fit(); // UISizing.Fixed(30)
        Layout.SizingY = UISizing.Fit(); // UISizing.Fixed(30)
        _label.Layout.AnchorAndParentAnchor = UIAnchor.CenterCenter;
    }
}
