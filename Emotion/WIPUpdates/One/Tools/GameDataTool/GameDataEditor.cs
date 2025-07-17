using Emotion.Game.Data;
using Emotion.Game.World.Editor;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.Standard.Reflector.Handlers.Interfaces;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;
using System;
using System.Text;
using static Emotion.Game.Data.GameDatabase;

#nullable enable

namespace Emotion.WIPUpdates.One.Tools.GameDataTool;

public class GameDataEditor : TwoSplitEditorWindowFileSupport<GameDataListEditor, ObjectPropertyWindow, object> // Passing object as edit type as we dont care about the built in file editing
{
    public Type GameDataType { get; private set; }
    public IGenericReflectorComplexTypeHandler? TypeHandler { get; private set; }

    private ObjectPropertyWindow _propertyEditor = null!;
    private object _propertyEditorListener = new object(); // Listener for changes by the property editor.

    private GameDataListEditor _listEditor = null!;
    public List<GameDataObject> EmulatedEditList = new List<GameDataObject>(); // List being edited by the list editor
    private List<GameDataObject> _modifiedList = new List<GameDataObject>();
    private List<GameDataObject> _deletedList = new List<GameDataObject>();

    public GameDataEditor(Type typ) : base($"{ReflectorEngine.GetTypeName(typ)} Editor")
    {
        GameDataType = typ;
        TypeHandler = ReflectorEngine.GetComplexTypeHandler(GameDataType);
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        UIBaseWindow contentParent = GetContentParent();
        _unsavedChangesNotification.DontTakeSpaceWhenHidden = false;

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
        EngineEditor.RegisterForObjectChanges(EmulatedEditList, OnListChanged, this);
        EditorAdapter.OnHotReloadNeededChange += HotReloadNeededChange;
    }

    protected override GameDataListEditor GetLeftSideContent()
    {
        var list = new GameDataListEditor(this, GameDataType)
        {
            AllowObjectEditting = false,
            IgnoreParentColor = true,
            OnItemSelected = SetEditItem
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

    private void SetEditItem(GameDataObject? selectedGameData)
    {
        EngineEditor.UnregisterForObjectChanges(_propertyEditorListener);
        if (selectedGameData != null)
            EngineEditor.RegisterForObjectChanges(selectedGameData, ObjectChangedEvent, _propertyEditorListener);
        _propertyEditor.SetEditor(selectedGameData);
    }

    private void OnListChanged(ObjectChangeEvent changeType)
    {
        if (changeType is not ListChangedEvent listEvent) return;
        if (listEvent.Item is not GameDataObject itemAsGameData) return;

        MarkUnsavedChanges();

        //EngineEditor.ObjectPropertyChangedNoInfo(EmulatedEditList, this);
        switch (listEvent.Type)
        {
            case ListChangedEvent.ChangeType.Delete:
                _deletedList.Add(itemAsGameData);
                SetEditItem(_listEditor.GetSelected());
                break;
            case ListChangedEvent.ChangeType.Add:
                _modifiedList.Add(itemAsGameData);
                _listEditor.UpdatePendingChangesForItems();
                break;
        }
    }

    public bool IsObjectModified(GameDataObject obj)
    {
        return _modifiedList.IndexOf(obj) != -1;
    }

    #region UI

    protected override void CreateTopBarButtons(UIBaseWindow topBar)
    {
        // Register hotkey ctrl + S -> save
        EditorButton button = new EditorButton("Save Changes")
        {
            OnClickedProxy = (_) => SaveFileClicked(),
            Enabled = false
        };
        topBar.AddChild(button);
        _saveButton = button;

        EditorButton statistics = new EditorButton("Statistics")
        {
            OnClickedProxy = (_) => OpenStatistics(),
            Enabled = true
        };
        topBar.AddChild(statistics);

        {
            UISolidOutline hotReloadContainer = new()
            {
                AnchorAndParentAnchor = UIAnchor.TopRight,
                GrowX = false,
                GrowY = false,
                WindowColor = Color.PrettyOrange,
                Paddings = new Primitives.Rectangle(1, 1, 1, 1),
                Visible = _hotReloadNeeded
            };
            topBar.AddChild(hotReloadContainer);
            _hotReloadBox = hotReloadContainer;

            UISolidColor hotReloadButtonHighlight = new()
            {
                WindowColor = Color.White * 0.3f,
                Visible = false,
                OrderInParent = 2
            };
            hotReloadContainer.AddChild(hotReloadButtonHighlight);

            SquareEditorButtonWithTexture hotReloadButton = new("Editor/HotReload.png")
            {
                Scale = new Vector2(0.75f),
                NormalColor = new Primitives.Color(31, 31, 31),
                RolloverColor = new Primitives.Color(31, 31, 31),
                Paddings = new Primitives.Rectangle(4, 4, 4, 4),

                OnMouseEnterProxy = (_) => hotReloadButtonHighlight.Visible = true,
                OnMouseLeaveProxy = (_) => hotReloadButtonHighlight.Visible = false,
                OnRolloverSpawn = () =>
                {
                    // todo: editor rollover
                    var rollover = new UIRollover()
                    {
                        Anchor = UIAnchor.TopRight,
                        ParentAnchor = UIAnchor.BottomRight,
                        Margins = new Primitives.Rectangle(5, 5, 0, 5)
                    };

                    var color = new UISolidColor()
                    {
                        WindowColor = MapEditorColorPalette.BarColor * 0.75f,
                        Paddings = new Primitives.Rectangle(8, 5, 8, 5),
                        MaxSizeX = 400
                    };
                    rollover.AddChild(color);

                    var text = new EditorLabel
                    {
                        Text = "Some game data objects might have an incorrect class, until you hot-reload or restart!"
                    };
                    color.AddChild(text);

                    return rollover;
                }
            };
            hotReloadContainer.AddChild(hotReloadButton);
        }
    }

    #endregion

    #region Hot Reload

    private static bool _hotReloadNeeded;
    private UIBaseWindow _hotReloadBox = null!;

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
                    SetEditItem(newInstance);
            }

            EngineEditor.ReportChange_NoInfo(EmulatedEditList, this);
        }
    }

    #endregion

    #region Saving

    private EditorButton _saveButton = null!;

    protected override void SaveFile()
    {
        for (int i = 0; i < _deletedList.Count; i++)
        {
            EditorAdapter.EditorDeleteObject(GameDataType, _deletedList[i]);
        }
        _deletedList.Clear();

        EditorAdapter.SaveChanges(GameDataType, _modifiedList);
        _modifiedList.Clear();
        _listEditor.UpdatePendingChangesForItems();
    }

    protected override void UnsavedChangesChanged()
    {
        base.UnsavedChangesChanged();
        _saveButton.Enabled = _hasUnsavedChanges;
    }

    private void ObjectChangedEvent(ObjectChangeEvent _)
    {
        MarkUnsavedChanges();

        var obj = (GameDataObject?)_propertyEditor.ObjectBeingEdited;
        AssertNotNull(obj);
        if (obj != null && _modifiedList.IndexOf(obj) == -1)
        {
            _modifiedList.Add(obj);
            _listEditor.UpdatePendingChangesForItems();
        }
    }

    #endregion

    #region Statistics

    public void OpenStatistics()
    {
        if (TypeHandler == null) return;

        StringBuilder s = new StringBuilder();
        foreach (ComplexTypeHandlerMemberBase member in TypeHandler.GetMembersDeep())
        {
            // Skip base type members
            if (member.ParentType == typeof(GameDataObject)) continue;

            s.AppendLine(member.Name);

            Dictionary<object, int> values = new Dictionary<object, int>();

            foreach (GameDataObject dataInstance in EmulatedEditList)
            {
                if (!member.GetValueFromComplexObject(dataInstance, out object? val)) continue;

                if (val is IEnumerable valAsEnum)
                {
                    // Include the combinations also
                    StringBuilder combo = new StringBuilder();
                    bool first = true;
                    bool hasAtleastTwo = false;

                    foreach (object? item in valAsEnum)
                    {
                        object? itemVal = item ?? "<null>";

                        if (!first)
                        {
                            combo.Append(" + ");
                            hasAtleastTwo = true;
                        }
                        combo.Append($"{itemVal}");
                        first = false;

                        if (values.TryGetValue(itemVal, out int itemValueCount))
                            values[itemVal] = itemValueCount + 1;
                        else
                            values[itemVal] = 1;
                    }

                    if (!hasAtleastTwo) continue;

                    val = combo.ToString();
                }

                val ??= "<null>";
                if (values.TryGetValue(val, out int value))
                    values[val] = value + 1;
                else
                    values[val] = 1;
            }

            foreach (KeyValuePair<object, int> valPair in values)
            {
                s.AppendLine($"   [{valPair.Value}] {valPair.Key}");
            }
        }

        string statisticText = s.ToString();
        EngineEditor.EditorRoot.AddChild(new ObjectPropertyEditorWindow(statisticText));
    }

    #endregion

    #region Misc

    protected override void UpdateHeader()
    {
        Header = $"{(_hasUnsavedChanges ? "*" : "")}{$"{ReflectorEngine.GetTypeName(GameDataType)} Editor"}";
    }

    #endregion
}

// ListEditor inline editor should move delete and arrow key buttons to the item container since there is no selection then
// Having the dropdown open on enum editor should keep the item highlighted
// Statistics - square
// Changing data ids does not update registry (couldnt reproduce)