#nullable enable

#region Using

using System.Reflection;
using Emotion.Editor.EditorHelpers;
using Emotion.Game.Data;
using Emotion.IO;
using Emotion.UI;

#endregion

namespace Emotion.Editor.EditorWindows;

public class DataEditorGeneric : EditorPanel
{
    protected Type _type;
    protected UICallbackListNavigator _list = null!;
    protected UIBaseWindow _rightSide = null!;
    protected GameDataObject? _selectedObject;
    protected HashSet<GameDataObject> _unsaved = new();

    public DataEditorGeneric(Type type) : base($"{type.Name} Editor")
    {
        _type = type;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var leftPart = new UIBaseWindow
        {
            StretchX = true,
            StretchY = true,
            LayoutMode = LayoutMode.VerticalList
        };
        _contentParent.AddChild(leftPart);

        {
            var buttonList = new UIBaseWindow
            {
                StretchX = true,
                StretchY = true,
                LayoutMode = LayoutMode.HorizontalList,
                Margins = new Rectangle(0, 0, 0, 5),
                ListSpacing = new Vector2(5, 0)
            };
            leftPart.AddChild(buttonList);

            var newButton = new EditorButton
            {
                StretchY = true,
                Text = "New",
                OnClickedProxy = _ => { CreateNew(); }
            };
            buttonList.AddChild(newButton);

            var saveButton = new EditorButton
            {
                StretchY = true,
                Text = "Save",
                OnClickedProxy = _ => { SaveToFile(); }
            };
            buttonList.AddChild(saveButton);

            var deleteCurrent = new EditorButton
            {
                Id = "DeleteButton",
                StretchY = true,
                Text = "Delete",
                OnClickedProxy = _ =>
                {
                    AssertNotNull(_selectedObject);
                    GameDataDatabase.EditorAdapter.EditorDeleteObject(_type, _selectedObject);
                    _selectedObject = null;
                    RegenerateList();
                    RegenerateSelection();
                },
                Enabled = false
            };
            buttonList.AddChild(deleteCurrent);

            var listContainer = new UIBaseWindow
            {
                StretchX = true,
                StretchY = true,
                LayoutMode = LayoutMode.HorizontalList,
                ZOffset = 10
            };
            leftPart.AddChild(listContainer);

            var listNav = new UICallbackListNavigator
            {
                LayoutMode = LayoutMode.VerticalList,
                StretchX = true,
                ListSpacing = new Vector2(0, 1),
                Margins = new Rectangle(0, 0, 5, 0),
                ChildrenAllSameWidth = true,
                MinSizeX = 100
            };
            listNav.OnChoiceConfirmed += (wnd, idx) =>
            {
                if (wnd is not EditorButton nuSelButton) return;

                object? userData = nuSelButton.UserData;
                if (userData == null || !userData.GetType().IsAssignableTo(_type)) return;

                _selectedObject = (GameDataObject) userData;
                RegenerateSelection();
            };
            listContainer.AddChild(listNav);

            var scrollBar = new EditorScrollBar();
            listNav.SetScrollbar(scrollBar);
            listContainer.AddChild(scrollBar);

            _list = listNav;
        }

        var rightPart = new UIBaseWindow
        {
            StretchX = true,
            StretchY = true,
            LayoutMode = LayoutMode.VerticalList,
            MaxSizeX = 200,
            MinSizeX = 200
        };
        _rightSide = rightPart;
        _contentParent.AddChild(rightPart);

        _contentParent.LayoutMode = LayoutMode.HorizontalList;
        RegenerateList();
    }

    private void CreateNew()
    {
        ConstructorInfo constructor = _type.GetConstructor(Type.EmptyTypes)!;
        var newObj = (GameDataObject) constructor.Invoke(null);
        newObj.Id = "Untitled";

        GameDataDatabase.EditorAdapter.EditorAddObject(_type, newObj);
        RegenerateList();
    }

    private void SaveToFile(bool force = false)
    {
        GameDataArray<GameDataObject> data = GameDataDatabase.GetObjectsOfType(_type);
        foreach (var item in data)
        {
            if (!force && !_unsaved.Contains(item)) continue;

            string loadedFromFile = item.LoadedFromFile;
            string newAssetPath = GameDataDatabase.EditorAdapter.GetAssetPath(item);

            // Remove the old file. (the object is resaved with the new id)
            if (!string.IsNullOrEmpty(loadedFromFile) && loadedFromFile != newAssetPath)
                DebugAssetStore.DeleteFile(loadedFromFile);

            item.LoadedFromFile = newAssetPath;
            XMLAsset<GameDataObject> asset = XMLAsset<GameDataObject>.CreateFromContent(item);
            asset.SaveAs(newAssetPath);
        }

        _unsaved.Clear();
        RegenerateList();
    }

    private void RegenerateList()
    {
        _list.ClearChildren();

        var data = GameDataDatabase.GetObjectsOfType(_type);
        if (data == null) return;

        // Create buttons for each of them.
        for (int i = 0; i < data.Length; i++)
        {
            var item = data[i];
            var key = item.Id;

            string label = item.ToString() ?? key;
            if (_unsaved.Contains(item)) label += " (*)";

            var uiForItem = new EditorButton
            {
                StretchY = true,
                Text = label,
                UserData = item,
            };
            _list.AddChild(uiForItem);
        }

        _list.SetupMouseSelection();
    }

    private void RegenerateSelection()
    {
        var deleteButton = GetWindowById("DeleteButton");
        if (deleteButton != null)
        {
            var deleteButtAsObject = (EditorButton) deleteButton;
            deleteButtAsObject.Enabled = _selectedObject != null;
        }

        _rightSide.ClearChildren();
        if (_selectedObject == null) return;
        var properties = new GenericPropertiesEditorPanel(_selectedObject)
        {
            PanelMode = PanelMode.Embedded,
            OnPropertyEdited = (propertyName, oldValue) =>
            {
                if (_unsaved.Add(_selectedObject)) RegenerateList();

                if (propertyName == "Id")
                {
                    string newId = GameDataDatabase.EditorAdapter.EnsureNonDuplicatedId(_selectedObject.Id, _type);
                    _selectedObject.Id = newId;
                    GameDataDatabase.EditorAdapter.EditorReIndex(_type);
                }
            },
        };

        _rightSide.AddChild(properties);
    }

    #region Custom Editor Logic

    private static Dictionary<Type, Type> _gameDataTypeToEditorType = new();

    public static void RegisterCustomGameDataEditor<TEditor, TGameData>()
        where TEditor : DataEditorGeneric
        where TGameData : GameDataObject
    {
        _gameDataTypeToEditorType.Add(typeof(TGameData), typeof(TEditor));
    }

    public static DataEditorGeneric CreateEditorInstanceForType(Type type)
    {
        if (_gameDataTypeToEditorType.TryGetValue(type, out Type? editorType))
        {
            DataEditorGeneric? customEditor = Activator.CreateInstance(editorType) as DataEditorGeneric;
            if (customEditor != null) return customEditor;
        }

        return new DataEditorGeneric(type);
    }

    #endregion
}