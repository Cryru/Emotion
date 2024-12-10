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

public class EditorWindowFileSupport : EditorWindow
{
    private UIBaseWindow _mainContent;

    public EditorWindowFileSupport(string title) : base(title ?? "Generic Tool")
    {
    }


    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();

        UIBaseWindow mainContent = new()
        {
            LayoutMode = LayoutMode.VerticalList
        };
        contentParent.AddChild(mainContent);
        _mainContent = mainContent;

        UIBaseWindow buttonList = new()
        {
            LayoutMode = LayoutMode.HorizontalList,
            Paddings = new Primitives.Rectangle(5, 5, 5, 5),
            ListSpacing = new Vector2(5, 0)
        };
        mainContent.AddChild(buttonList);

        CreateTopBarButtons(buttonList);
    }

    protected override UIBaseWindow GetContentParent()
    {
        return _mainContent ?? base.GetContentParent();
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
                button.OnClickedProxy = (_) => NewFile();
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Open");
                button.FillX = true;
                button.OnClickedProxy = (_) => OpenFile();
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Save");
                button.FillX = true;
                button.OnClickedProxy = (_) => SaveFile();
                dropDown.AddChild(button);
            }

            //{
            //    EditorButton button = new EditorButton("Save As");
            //    button.FillX = true;
            //    dropDown.AddChild(button);
            //}

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

    #region API

    protected virtual void NewFile()
    {

    }

    protected virtual void SaveFile()
    {

    }

    protected virtual void OpenFile()
    {

    }

    #endregion
}
