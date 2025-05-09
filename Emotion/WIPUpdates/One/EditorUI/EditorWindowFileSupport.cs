﻿using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools;

public partial class EditorWindowFileSupport : EditorWindow
{
    public const string DEFAULT_FILE_NAME = "Untitled";

    private UIBaseWindow _mainContent = null!;

    private string _headerBaseText;
    protected string _currentFileName = DEFAULT_FILE_NAME;

    private UIBaseWindow _unsavedChangesNotification = null!;

    public EditorWindowFileSupport(string title) : base(title ?? "Generic Tool")
    {
        _headerBaseText = Header;
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

        {
            UISolidColor changesBox = new UISolidColor()
            {
                Paddings = new Primitives.Rectangle(5, 5, 5, 5),
                WindowColor = Color.PrettyRed * 0.5f,
                Visible = false,
                DontTakeSpaceWhenHidden = true,
            };
            mainContent.AddChild(changesBox);
            _unsavedChangesNotification = changesBox;

            EditorLabel changesLabel = new EditorLabel
            {
                Text = $"You have unsaved changes!",
                WindowColor = Color.White,
                FontSize = 20,
                IgnoreParentColor = true
            };
            changesBox.AddChild(changesLabel);
        }

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
            UIDropDown dropDown = EditorDropDown.OpenListDropdown(me);

            {
                EditorButton button = new EditorButton("New");
                button.GrowX = true;
                button.OnClickedProxy = (_) =>
                {
                    NewFile();
                    Controller?.DropDown?.Close();
                };
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Open...");
                button.GrowX = true;
                button.OnClickedProxy = (_) => OpenFile();
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Save");
                button.GrowX = true;
                button.OnClickedProxy = (_) => SaveFile();
                dropDown.AddChild(button);
            }

            //{
            //    EditorButton button = new EditorButton("Save As");
            //    button.FillX = true;
            //    dropDown.AddChild(button);
            //}
        };
        topBar.AddChild(fileButton);
    }

    #region API

    protected virtual void NewFile()
    {
        _currentFileName = DEFAULT_FILE_NAME;
        Header = $"*{_currentFileName} - {_headerBaseText}";
    }

    protected virtual void SaveFile()
    {
        _hasUnsavedChanges = false;
        UnsavedChangesChanged();
    }

    protected virtual void OpenFile()
    {

    }

    #endregion

    #region Changes

    protected bool _hasUnsavedChanges;

    protected void MarkUnsavedChanges()
    {
        _hasUnsavedChanges = true;
        UnsavedChangesChanged();
    }

    protected virtual void UnsavedChangesChanged()
    {
        _unsavedChangesNotification.Visible = _hasUnsavedChanges;
    }

    #endregion
}
