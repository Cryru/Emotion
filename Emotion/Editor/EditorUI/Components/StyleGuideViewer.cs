#nullable enable

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

        var gridContainer = new UIBaseWindow()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.VerticalList(30)
            }
        };
        panelContent.AddChild(gridContainer);

        ButtonState[] buttonStates = Enum.GetValues<ButtonState>();
        ButtonType[] buttonTypes = Enum.GetValues<ButtonType>();
        Func<ButtonState, ButtonType, UIBaseWindow>[] buttonFactories =
        {
            (state, type) =>
            {
                var button = new OneButton("Button", null, type)
                {
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.CenterCenter,
                    }
                };
                button.SetState(state, true);
                return button;
            },
            (state, type) =>
            {
                var button = new OneIconButton("Editor/Pause.png", null, type)
                {
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.CenterCenter,
                    }
                };
                button.SetState(state, true);
                return button;
            }
        };

        foreach (Func<ButtonState, ButtonType, UIBaseWindow> buttonCreator in buttonFactories)
        {
            var grid = new UIBaseWindow()
            {
                Visuals =
                {
                    BackgroundColor = new Color("#160F28"),
                    GridVisual =
                    {
                        LineAfterFirstRow = true,
                        LineColor = new Color("#c3bfcb")
                    }
                },
                Layout =
                {
                    Padding = new UISpacing(20, 20, 20, 20),
                    LayoutMethod = UILayoutMethod.Grid_FixedColumns(buttonStates.Length + 1, 20, 20, false, false),
                },
            };
            gridContainer.AddChild(grid);

            // Corner
            var ph = new UIBaseWindow()
            {
            };
            grid.AddChild(ph);

            // Columns labels
            foreach (ButtonState buttonState in buttonStates)
            {
                grid.AddChild(new UIText()
                {
                    Text = Enum.GetName<ButtonState>(buttonState) ?? string.Empty,
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.CenterCenter,
                        Margins = new UISpacing(0, 0, 0, 15)
                    },
                    TextColor = new Color("#c3bfcb")
                });
            }

            foreach (ButtonType buttonType in buttonTypes)
            {
                var rowLabel = new UIText()
                {
                    Text = Enum.GetName<ButtonType>(buttonType) ?? string.Empty,
                    Layout =
                    {
                        AnchorAndParentAnchor = UIAnchor.CenterLeft
                    },
                    TextColor = new Color("#929194")
                };
                grid.AddChild(rowLabel);

                foreach (ButtonState buttonState in buttonStates)
                {
                    UIBaseWindow button = buttonCreator(buttonState, buttonType);
                    grid.AddChild(button);
                }
            }
        }



        //var textInputSection = new UIContainer()
        //{

        //    SetChildren =
        //    [
        //        new UIText()
        //        {
        //            Text = "Text Input"
        //        }
        //    ]
        //};
        //grid.AddChild(textInputSection);

        //textInputSection.AddChild(new OneTextInput());
    }
}
