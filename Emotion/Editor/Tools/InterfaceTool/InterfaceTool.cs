#nullable enable

using Emotion.Game.Systems.UI;
using Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;
using Emotion.Editor.EditorUI;
using Emotion.Game.Systems.UI2;

namespace Emotion.Editor.Tools.InterfaceTool;

public class InterfaceTool : TwoSplitEditorWindowFileSupport<UIViewport, ObjectPropertyWindow, O_UIBaseWindow>
{
    public InterfaceTool() : base("Interface Tool")
    {
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);
        NewFile();
        SetObjectBeingEdited(new O_UIBaseWindow()
        {
            Properties =
            {
                Color = Color.Blue
            },

            Layout =
            {
                LayoutMode = LayoutMode.HorizontalList,
                ListSpacing = new Vector2(5, 0),

                MinSize = new Vector2(200, 100),
                Padding = new UIRectangleSpacingMetric(5, 5, 5, 5)
            },

            Children = [
                new O_UIBaseWindow() {
                    Layout =
                    {
                        MinSize = new Vector2(20, 20)
                    },

                    Properties = {
                        Color = Color.PrettyPink
                    }
                },
                new O_UIBaseWindow() {
                    Layout =
                    {
                        MinSize = new Vector2(50, 20),
                        GrowX = true,
                        GrowY = true
                    },

                    Properties = {
                        Color = Color.PrettyYellow
                    }
                },
                new O_UIBaseWindow() {
                    Layout =
                    {
                        MinSize = new Vector2(50, 20)
                    },

                    Properties = {
                        Color = Color.PrettyBlue
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
            Id = "EntityData",
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

    protected override void OnObjectBeingEditedChange(O_UIBaseWindow? newObj)
    {
        EngineEditor.UnregisterForObjectChanges(this);
        if (newObj != null)
            EngineEditor.RegisterForObjectChanges(newObj, (_) => MarkUnsavedChanges(), this);

        ObjectPropertyWindow? entityData = _rightContent;
        AssertNotNull(entityData);
        entityData.SetEditor(newObj);
    }
}
