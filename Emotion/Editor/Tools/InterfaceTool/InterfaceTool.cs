#nullable enable

using Emotion.Game.Systems.UI;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Editor.EditorUI;
using Emotion.Game.Systems.UI2;

namespace Emotion.Editor.Tools.InterfaceTool;

public class InterfaceTool : TwoSplitEditorWindowFileSupport<UIViewport, ObjectPropertyWindow, UIBaseWindow>
{
    public InterfaceTool() : base("Interface Tool")
    {
    }

    protected override void OnOpen()
    {
        base.OnOpen();
        NewFile();
        SetObjectBeingEdited(new UIBaseWindow()
        {
            Visuals =
            {
                BackgroundColor = Color.Blue
            },

            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(5),

                MinSize = new IntVector2(200, 100),
                Padding = new UISpacing(5, 5, 5, 5)
            },

            Children = [
                new UIBaseWindow() {
                    Layout =
                    {
                        MinSize = new IntVector2(20, 20)
                    },

                    Visuals = {
                        BackgroundColor = Color.PrettyPink
                    }
                },
                new UIBaseWindow() {
                    Layout =
                    {
                        MinSize = new IntVector2(50, 20),
                        SizingX = UISizing.Grow(),
                        SizingY = UISizing.Grow()
                    },

                    Visuals = {
                        BackgroundColor = Color.PrettyYellow
                    }
                },
                new UIBaseWindow() {
                    Layout =
                    {
                        MinSize = new IntVector2(50, 20),
                        SizingX = UISizing.Grow(),
                    },

                    Visuals = {
                        BackgroundColor = Color.PrettyBlue
                    }
                }
            ]
        });
    }

    protected override UIViewport GetLeftSideContent()
    {
        var viewPort = new UIViewport()
        {
            OnRender = RenderViewport,
            WindowColor = Color.Black
        };
        return viewPort;
    }

    protected override ObjectPropertyWindow GetRightSideContent()
    {
        var objProps = new ObjectPropertyWindow()
        {
            Name = "EntityData",
            IgnoreParentColor = true,
            MinSize = new Vector2(50)
        };
        return objProps;
    }

    protected void RenderViewport(UIBaseWindow win, Renderer c)
    {
        c.RenderSprite(Vector3.Zero, new Vector2(1920, 1080), Color.White * 0.25f);

        ObjectBeingEdited?.Update();
        ObjectBeingEdited?.Render(c);
    }

    protected override void OnObjectBeingEditedChange(UIBaseWindow? newObj)
    {
        EngineEditor.UnregisterForObjectChanges(this);
        if (newObj != null)
            EngineEditor.RegisterForObjectChanges(newObj, (_) => MarkUnsavedChanges(), this);

        ObjectPropertyWindow? entityData = _rightContent;
        AssertNotNull(entityData);
        entityData.SetEditor(newObj);
    }
}
