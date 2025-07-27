﻿using Emotion.IO;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.Standard.XML;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI;
using Emotion.WIPUpdates.One.EditorUI.Components;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools;

public partial class EditorWindowFileSupport<T> : EditorWindow
{
    public const string DEFAULT_FILE_NAME = "Untitled";

    private UIBaseWindow _mainContent = null!;

    private string _headerBaseText;
    protected string _currentFileName = DEFAULT_FILE_NAME;

    protected ComplexTypeHandler<T>? _typeHandler;
    public T? ObjectBeingEdited { get; protected set; }

    public EditorWindowFileSupport(string title) : base(title ?? "Generic Tool")
    {
        _headerBaseText = Header;
        if (typeof(T) != typeof(object))
            _typeHandler = ReflectorEngine.GetComplexTypeHandler<T>();
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
                OrderInParent = -10
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
            ListSpacing = new Vector2(5, 0),
            OrderInParent = -5
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
                button.OnClickedProxy = (_) =>
                {
                    OpenFile();
                    Controller?.DropDown?.Close();
                };
                dropDown.AddChild(button);
            }

            {
                EditorButton button = new EditorButton("Save");
                button.GrowX = true;
                button.OnClickedProxy = (_) =>
                {
                    SaveFileClicked();
                    Controller?.DropDown?.Close();
                };
                //button.Enabled = _hasUnsavedChanges;
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

    protected void NewFile()
    {
        // todo: check if unsaved changes, prompt etc.

        if (_typeHandler != null)
            ObjectBeingEdited = (T?)_typeHandler.CreateNew();
        SetObjectBeingEdited(ObjectBeingEdited);
    }

    protected void OpenFile()
    {
        if (_typeHandler != null)
        {
            Action<UIBaseWindow, Action<XMLAsset<T>?>> fileOpenFunc = GetFileOpenFunction();
            fileOpenFunc(this, (file) =>
            {
                if (file == null) return;
                if (file.Content == null) return;

                SetObjectBeingEdited(file.Content);

                _currentFileName = file.Name;
                UpdateHeader();
            });
            //string xml = XMLFormat.To(ObjectBeingEdited);
            //Engine.AssetLoader.Save()
        }
    }

    protected void SaveFileClicked()
    {
        SaveFile();
        _hasUnsavedChanges = false;
        UnsavedChangesChanged();
    }

    #region API

    // You can overwrite this to provide custom XMLAsset specializations for file open
    protected virtual Action<UIBaseWindow, Action<XMLAsset<T>?>> GetFileOpenFunction()
    {
        return FilePicker<XMLAsset<T>>.SelectFile;
    }

    // You can overwrite this if you do not intend to use the built in file management, but
    // want to use the save/unsaved changes functionality.
    protected virtual void SaveFile()
    {
        // todo: promot for name aka save as
        if (_typeHandler != null && ObjectBeingEdited != null)
        {
            string xml = XMLFormat.To(ObjectBeingEdited);
            Engine.AssetLoader.SaveDevMode(xml, _currentFileName);
        }
    }

    /// <summary>
    /// This is called every time the "current object" changes.
    /// Only useful if you intend to use the built in object management.
    /// </summary>
    protected virtual void OnObjectBeingEditedChange(T newObj)
    {

    }

    protected virtual void UpdateHeader()
    {
        Header = $"{(_hasUnsavedChanges ? "*" : "")}{_currentFileName} - {_headerBaseText}";
    }

    protected void SetObjectBeingEdited(T? objToEdit)
    {
        _hasUnsavedChanges = false;
        UnsavedChangesChanged();

        _currentFileName = DEFAULT_FILE_NAME + ".xml"; // Open should set the file name after
        ObjectBeingEdited = objToEdit;
        UpdateHeader();
        if (objToEdit != null)
            OnObjectBeingEditedChange(objToEdit);
    }

    #endregion

    #region Changes

    protected bool _hasUnsavedChanges;
    protected UIBaseWindow _unsavedChangesNotification = null!;

    protected void MarkUnsavedChanges()
    {
        _hasUnsavedChanges = true;
        UnsavedChangesChanged();
    }

    protected virtual void UnsavedChangesChanged()
    {
        _unsavedChangesNotification.Visible = _hasUnsavedChanges;
        UpdateHeader();
    }

    #endregion
}
