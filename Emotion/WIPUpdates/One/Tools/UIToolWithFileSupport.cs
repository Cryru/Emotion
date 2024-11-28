using Emotion.Editor.EditorHelpers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools;

public class UIToolWithFileSupport : EditorWindow
{

    public UIToolWithFileSupport() : base("UI Tool With File Support")
    {
        
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var container = new UIBaseWindow
        {
            LayoutMode = LayoutMode.VerticalList
        };
        _contentParent.AddChild(container);

        var buttonContainer = new UIBaseWindow
        {
            LayoutMode = LayoutMode.HorizontalList,
            ListSpacing = new Vector2(5, 0)
        };
        container.AddChild(buttonContainer);

        CreateTopBarButtons(buttonContainer);
    }

    protected virtual void CreateTopBarButtons(UIBaseWindow topBar)
    {
        EditorButton fileButton = new EditorButton("File");
        fileButton.OnClickedProxy = (me) =>
        {
            UIDropDown dropDown = OpenDropdown(me);

            {
                EditorButton button = new EditorButton("New");
                button.FillX = true;
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Open");
                button.FillX = true;
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Save");
                button.FillX = true;
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Save As");
                button.FillX = true;
                dropDown.AddChild(button);
            }

            Controller?.AddChild(dropDown);
        };
        topBar.AddChild(fileButton);
    }

    private UIDropDown OpenDropdown(UIBaseWindow spawningWindow)
    {
        UIDropDown dropDown = new UIDropDown(spawningWindow)
        {
            Paddings = new Primitives.Rectangle(0, 5, 0, 0),
            Anchor = UIAnchor.TopLeft,
            ParentAnchor = UIAnchor.BottomLeft,
            LayoutMode = LayoutMode.VerticalList,
            ListSpacing = new Vector2(0, 5)
        };

        UISolidColor col = new UISolidColor
        {
            FillX = false,
            FillY = false,
            WindowColor = Color.Black * 0.5f,
            BackgroundWindow = true
        };
        dropDown.AddChild(col);

        return dropDown;
    }
}
