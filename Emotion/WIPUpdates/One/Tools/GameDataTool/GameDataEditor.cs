using Emotion.Game.Data;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System;
using static Emotion.Game.Data.GameDatabase;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools.GameDataTool;

public class GameDataEditor : TwoSplitEditorWindowFileSupport<GameDataListEditor, ObjectPropertyWindow, object> // Passing object as edit type as we dont care about the built in file editing
{
    public Type GameDataType { get; private set; }
    public IGenericReflectorComplexTypeHandler? TypeHandler { get; private set; }

    private ObjectPropertyWindow _propertyEditor = null!;
    private object _propertyEditorListener = new object(); // Listener for changes by the property editor.

    private ListEditor<GameDataObject> _listEditor = null!;
    public List<GameDataObject> EmulatedEditList = new List<GameDataObject>(); // List being edited by the list editor
    private List<GameDataObject> _modifiedList = new List<GameDataObject>();

    public GameDataEditor(Type typ) : base($"{ReflectorEngine.GetTypeName(typ)} Editor")
    {
        GameDataType = typ;
        TypeHandler = ReflectorEngine.GetComplexTypeHandler(GameDataType);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();
        CreateHotReloadSection(contentParent);

        // Now that the UI is setup, initialize editting functionality

        // Create a list of game data instances to be edited instead of the real deal.
        GameDataObject[] gameDataOfType = GetObjectsOfType(GameDataType);
        for (int i = 0; i < gameDataOfType.Length; i++)
        {
            GameDataObject dataObject = gameDataOfType[i];
            EmulatedEditList.Add(dataObject.CreateCopy());
        }
        _listEditor.SetValue(EmulatedEditList);

        // Attach to events concerning the list (fired by the ListEditor)
        // and hot reload "need" events in order to hide/show the hot reload section.
        EngineEditor.RegisterForObjectChanges(EmulatedEditList, ListChangedEvent, this);
        EditorAdapter.OnHotReloadNeededChange += HotReloadNeededChange;
    }

    protected override GameDataListEditor GetLeftSideContent()
    {
        var list = new GameDataListEditor(this, GameDataType)
        {
            IgnoreParentColor = true,
            OnItemSelected = SelectItem
        };
        _listEditor = list;
        return list;
    }

    protected override ObjectPropertyWindow GetRightSideContent()
    {
        var properties = new ObjectPropertyWindow();
        _propertyEditor = properties;

        return properties;
    }

    public override void DetachedFromController(UIController controller)
    {
        base.DetachedFromController(controller);
        EngineEditor.UnregisterForObjectChanges(this);
        EditorAdapter.OnHotReloadNeededChange -= HotReloadNeededChange;
        EngineEditor.UnregisterForObjectChanges(_propertyEditorListener);
    }

    private void SelectItem(GameDataObject? selectedGameData)
    {
        EngineEditor.UnregisterForObjectChanges(_propertyEditorListener);
        if (selectedGameData != null)
            EngineEditor.RegisterForObjectChanges(selectedGameData, ObjectChangedEvent, _propertyEditorListener);
        _propertyEditor.SetEditor(selectedGameData);
    }

    private void ListChangedEvent(ObjectChangeType changeType)
    {
        MarkUnsavedChanges();

        EngineEditor.ObjectChanged(EmulatedEditList, ObjectChangeType.ComplexObject_PropertyChanged, this);
        if (changeType == ObjectChangeType.List_NewObj)
        {
            _modifiedList.Add(EmulatedEditList[^1]);
        }
    }

    #region Hot Reload

    private static bool _hotReloadNeeded;
    private UIBaseWindow _hotReloadBox = null!;

    private void CreateHotReloadSection(UIBaseWindow contentParent)
    {
        UISolidColor hotReloadBox = new UISolidColor()
        {
            Paddings = new Primitives.Rectangle(5, 5, 5, 5),
            WindowColor = Color.PrettyOrange * 0.5f,
            Visible = _hotReloadNeeded,
            DontTakeSpaceWhenHidden = true,
            GrowY = false,
            OrderInParent = -1
        };
        contentParent.AddChild(hotReloadBox);
        _hotReloadBox = hotReloadBox;

        EditorLabel changesLabel = new EditorLabel
        {
            Text = $"Some game data objects might have an incorrect class,\nuntil you hot-reload or restart!",
            WindowColor = Color.White,
            FontSize = 20,
            IgnoreParentColor = true
        };
        hotReloadBox.AddChild(changesLabel);
    }

    private void HotReloadNeededChange(List<string> typesNeedReload)
    {
        bool wasNeeded = _hotReloadNeeded;

        _hotReloadNeeded = typesNeedReload.IndexOf(EditorAdapter.GetGameDataTypeDefClassName(GameDataType)) != -1;
        _hotReloadBox.Visible = _hotReloadNeeded;

        // Replace data being edited.
        if (wasNeeded && !_hotReloadNeeded)
        {
            GameDataObject[] gameDataOfType = GetObjectsOfType(GameDataType);
            for (int i = 0; i < gameDataOfType.Length; i++)
            {
                GameDataObject dataObject = gameDataOfType[i];
                GameDataObject newInstance = dataObject.CreateCopy();

                GameDataObject currentItem = EmulatedEditList[i];
                ReflectorEngine.CopyProperties(currentItem, newInstance);
                EmulatedEditList[i] = newInstance;

                if (_propertyEditor.ObjectBeingEdited == currentItem)
                    _propertyEditor.SetEditor(newInstance);
            }

            EngineEditor.ObjectChanged(EmulatedEditList, ObjectChangeType.List_ObjectValueChanged, this);
        }
    }

    #endregion

    #region Saving

    private EditorButton _saveButton = null!;

    protected override void CreateTopBarButtons(UIBaseWindow topBar)
    {
        EditorButton button = new EditorButton("Save Changes")
        {
            OnClickedProxy = (_) => SaveFileClicked(),
            Enabled = false
        };
        topBar.AddChild(button);
        _saveButton = button;

        // Register hotkey ctrl + S -> save
    }

    protected override void SaveFile()
    {
        EditorAdapter.SaveChanges(GameDataType, _modifiedList);
    }

    protected override void UnsavedChangesChanged()
    {
        base.UnsavedChangesChanged();
        _saveButton.Enabled = _hasUnsavedChanges;
    }

    private void ObjectChangedEvent(ObjectChangeType changeType)
    {
        MarkUnsavedChanges();

        var obj = (GameDataObject?)_propertyEditor.ObjectBeingEdited;
        AssertNotNull(obj);
        if (obj != null && _modifiedList.IndexOf(obj) == -1)
            _modifiedList.Add(obj);
    }

    #endregion

    #region Misc

    protected override void UpdateHeader()
    {
        Header = $"{(_hasUnsavedChanges ? "*" : "")}{$"{ReflectorEngine.GetTypeName(GameDataType)} Editor"}";
    }

    #endregion
}
