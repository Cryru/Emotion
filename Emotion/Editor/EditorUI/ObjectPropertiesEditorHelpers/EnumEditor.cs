#nullable enable

using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class EnumEditor<T, TNum> : TypeEditor
    where T : Enum
    where TNum : INumber<TNum>
{
    private EditorButton _button;
    private EnumTypeHandler<T, TNum>? _typeHandler;

    private T[]? _items;
    private int _currentIndex;

    private Emotion.WIPUpdates.One.Tools.EditorDropDown? _dropDown;

    public EnumEditor()
    {
        _typeHandler = (EnumTypeHandler<T, TNum>?)ReflectorEngine.GetTypeHandler<T>();

        var button = new EditorButton()
        {
            GrowX = true,
            OnClickedProxy = (_) => OpenItemDropdown()
        };
        AddChild(button);
        _button = button;

        var arrowSquare = new UIBaseWindow()
        {
            GrowX = false,
            GrowY = false,
            MinSizeY = 23,
            MinSizeX = 29,
            AnchorAndParentAnchor = UIAnchor.TopRight,
        };
        AddChild(arrowSquare);

        var arrowIcon = new UITexture()
        {
            Smooth = true,
            TextureFile = "Editor/LittleArrow.png",
            ImageScale = new Vector2(0.6f),
            Offset = new Vector2(0, 4),
            AnchorAndParentAnchor = UIAnchor.CenterCenter
        };
        arrowSquare.AddChild(arrowIcon);
    }

    public override void SetValue(object? value)
    {
        if (_typeHandler == null) return;

        string name = _typeHandler.GetValueName(value);
        _button.Text = name;

        T[] items = _typeHandler.GetValues();
        _items = items;

        T? currentAsT = (T?)value;
        _currentIndex = items.IndexOf(currentAsT);
        if (_currentIndex == -1) _currentIndex = 0;
    }

    private void ItemsUIOnClickSelect(int index, T? item)
    {
        _dropDown?.Close();

        if (_typeHandler == null || item == null) return;

        _currentIndex = index;
        OnValueChanged(item);

        string name = _typeHandler.GetValueName(item);
        _button.Text = name;
    }

    protected void OpenItemDropdown()
    {
        if (_items == null) return;

        var dropDown = Emotion.WIPUpdates.One.Tools.EditorDropDown.OpenListDropdown(this);
        dropDown.MaxSizeY = 300;
        dropDown.ParentAnchor = UIAnchor.BottomRight;
        dropDown.Anchor = UIAnchor.TopRight;

        int idx = 0;
        foreach (T item in _items)
        {
            var button = new EditorListItem<T>(idx, item, ItemsUIOnClickSelect);
            button.Selected = _currentIndex == idx;
            dropDown.AddChild(button);

            idx++;
        }

        _dropDown?.Close();
        _dropDown = dropDown;
    }
}
