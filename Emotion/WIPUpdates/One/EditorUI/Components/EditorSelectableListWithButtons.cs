//using Emotion.UI;
//using Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

//#nullable enable

//namespace Emotion.WIPUpdates.One.EditorUI.Components;

//public class EditorSelectableListWithButtons<T> : EditorSelectableList<T> where T : new()
//{
//    public string? LabelText
//    {
//        get => _label.Text;
//        set => _label.Text = value;
//    }

//    public bool CanAdd = true;
//    public bool CanRearrange = true;
//    public bool CanDelete = true;

//    private EditorLabel _label;
//    private IList<T>? _itemsAsList;

//    private EditorButton _addButton;

//    public EditorSelectableListWithButtons() : base()
//    {
//        LayoutMode = LayoutMode.VerticalList;

//        UIBaseWindow editorsContainer = new UIBaseWindow()
//        {
//            LayoutMode = LayoutMode.HorizontalList,
//            ListSpacing = new Vector2(5, 0),
//            FillY = false,
//            OrderInParent = -1,
//            Margins = new Primitives.Rectangle(5, 0, 0, 5)
//        };
//        AddChild(editorsContainer);

//        _label = new EditorLabel()
//        {
//            TextHeightMode = Game.Text.GlyphHeightMeasurement.NoMinY
//        };
//        editorsContainer.AddChild(_label);

//        var addNew = new SquareEditorButton("+")
//        {
//            OnClickedProxy = (_) =>
//            {
//                AssertNotNull(_itemsAsList);
//                _itemsAsList.Add(new T());
//                OnItemsChanged(_itemsAsList);
//                OnSelectionChanged(_current);
//            }
//        };
//        editorsContainer.AddChild(addNew);
//        _addButton = addNew;

//        UpdateButtonStates();
//    }

//    private void UpdateButtonStates()
//    {
//        _addButton.Enabled = CanAdd && _itemsAsList != null;
//    }

//    public void SetEditorExtended(IList<T>? list, int currentValueIdx, Action<T?>? onSelect)
//    {
//        _itemsAsList = list;
//        base.SetEditor(list, currentValueIdx, onSelect);

//        UpdateButtonStates();
//    }

//    public void SetEditorExtended(IList<T>? list, T? currentValue, Action<T?>? onSelect)
//    {
//        _itemsAsList = list;
//        base.SetEditor(list, currentValue, onSelect);

//        UpdateButtonStates();
//    }
//}
