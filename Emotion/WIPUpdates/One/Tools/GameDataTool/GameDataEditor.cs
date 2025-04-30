using Emotion.Game.Data;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using static Emotion.Game.Data.GameDatabase;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools.GameDataTool;

public class GameDataEditor : EditorWindowFileSupport // Heads up. This editor doesn't actually use the file support, just the top bar.
{
    public Type GameDataType { get; private set; }
    public IGenericReflectorComplexTypeHandler? TypeHandler { get; private set; }

    private ObjectPropertyWindow _propertyEditor = null!;
    private object _propertyEditorListener = new object(); // Listener for changes by the property editor.

    private ListEditor<GameDataObject> _listEditor = null!;
    public List<GameDataObject> EmulatedEditList = new List<GameDataObject>(); // List being edited by the list editor
    private List<GameDataObject> _modifiedList = new List<GameDataObject>();

    public GameDataEditor(Type typ) : base($"{typ.Name} Editor")
    {
        GameDataType = typ;
        TypeHandler = ReflectorEngine.GetComplexTypeHandler(GameDataType);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();

        CreateHotReloadSection(contentParent);

        // Standard two-split editor
        UISolidColor content = new UISolidColor
        {
            IgnoreParentColor = true,
            MinSize = new Vector2(300),
            WindowColor = new Color(0, 0, 0, 50),
            Paddings = new Rectangle(5, 5, 5, 5),
            LayoutMode = LayoutMode.HorizontalEditorPanel
        };
        contentParent.AddChild(content);

        var list = new GameDataListEditor(this, GameDataType)
        {
            IgnoreParentColor = true,
            OnItemSelected = SelectItem
        };
        content.AddChild(list);
        _listEditor = list;

        content.AddChild(new HorizontalPanelSeparator());

        UIBaseWindow contentRight = new()
        {
            IgnoreParentColor = true,
            Id = "SelectedInfo",
            LayoutMode = LayoutMode.VerticalList,
            GrowX = false,
            //MinSize = new Vector2(50)
        };
        content.AddChild(contentRight);

        var properties = new ObjectPropertyWindow();
        contentRight.AddChild(properties);
        _propertyEditor = properties;

        // Now that the UI is setup, initialize editting functionality

        // Create a list of game data instances to be edited instead of the real deal.
        GameDataObject[] gameDataOfType = GetObjectsOfType(GameDataType);
        for (int i = 0; i < gameDataOfType.Length; i++)
        {
            GameDataObject dataObject = gameDataOfType[i];
            EmulatedEditList.Add(dataObject.CreateCopy());
        }
        list.SetValue(EmulatedEditList);

        // Attach to events concerning the list (fired by the ListEditor)
        // and hot reload "need" events in order to hide/show the hot reload section.
        EngineEditor.RegisterForObjectChanges(EmulatedEditList, ListChangedEvent, this);
        EditorAdapter.OnHotReloadNeededChange += HotReloadNeededChange;
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

            EngineEditor.ObjectChanged(EmulatedEditList, ObjectChangeType.ValueChanged, this);
        }
    }

    #endregion

    #region Saving

    private EditorButton _saveButton = null!;

    protected override void CreateTopBarButtons(UIBaseWindow topBar)
    {
        EditorButton button = new EditorButton("Save Changes")
        {
            OnClickedProxy = (_) => SaveFile(),
            Enabled = false
        };
        topBar.AddChild(button);
        _saveButton = button;

        // Register hotkey ctrl + S -> save
    }

    protected override void SaveFile()
    {
        EditorAdapter.SaveChanges(GameDataType, _modifiedList);
        base.SaveFile();
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
}
