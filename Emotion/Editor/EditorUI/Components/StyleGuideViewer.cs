#nullable enable

using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;

namespace Emotion.Editor.EditorUI.Components;

public class StyleGuideViewer : EditorWindow
{
    public StyleGuideViewer() : base("Editor UI Style Guide")
    {

    }

    protected override void OnOpen()
    {
        base.OnOpen();

        UIBaseWindow panelContent = GetContentParent();

        ButtonState[] buttonStates = Enum.GetValues<ButtonState>();

        var grid = new UIBaseWindow()
        {
            Visuals =
            {
                BackgroundColor = new Color("#160F28")
            },
            Layout =
            {
                Padding = new UISpacing(20, 20, 20, 20),
                LayoutMethod = UILayoutMethod.VerticalList(40)
            }
        };
        panelContent.AddChild(grid);

        var buttonStateSection = new UIContainer()
        {
            Layout =
            {
                Padding = new UISpacing(40, 0, 0, 0),
                LayoutMethod = UILayoutMethod.HorizontalList(40)
            },
        };
        grid.AddChild(buttonStateSection);

        foreach (var buttonState in buttonStates)
        {
            buttonStateSection.AddChild(new NewUIText()
            {
                Text = Enum.GetName<ButtonState>(buttonState) ?? string.Empty
            });
        }

        var defaultButtonSection = new UIContainer()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(40)
            },
            SetChildren =
            [
                new NewUIText()
                {
                    Text = "Default"
                }
            ]
        };
        grid.AddChild(defaultButtonSection);

        foreach (ButtonState but in buttonStates)
        {
            var button = new OneButton("Button");
            button.SetState(but);
            defaultButtonSection.AddChild(button);
        }

        var outlineButtonSection = new UIContainer()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(40)
            },
            SetChildren =
            [
                new NewUIText()
                {
                    Text = "Outlined"
                }
            ]
        };
        grid.AddChild(outlineButtonSection);

        foreach (ButtonState but in buttonStates)
        {
            var button = new OneButton("Button", ButtonType.Outlined);
            button.SetState(but);
            outlineButtonSection.AddChild(button);
        }
    }
}
