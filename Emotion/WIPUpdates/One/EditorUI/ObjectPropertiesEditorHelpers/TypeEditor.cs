#nullable enable

using Emotion.Common.Serialization;
using Emotion.Standard.Reflector;
using Emotion.Standard.Reflector.Handlers.Base;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public enum LabelStyle
{
    NormalEditor,
    MapEditor
}

public abstract class TypeEditor : UIBaseWindow
{
    [DontSerialize]
    private Action<object?>? _onValueChanged;

    public abstract void SetValue(object? value);

    public void SetCallbackOnValueChange(Action<object?> onValueChanged)
    {
        _onValueChanged = onValueChanged;
    }

    protected void OnValueChanged(object? newValue)
    {
        _onValueChanged?.Invoke(newValue);
    }

    public static UIBaseWindow CreateCustomWithLabel<T>(string labelText, T initialValue, Action<T> valueChanged, LabelStyle style = LabelStyle.NormalEditor)
    {
        UIBaseWindow container = new UIBaseWindow()
        {
            GrowY = false,
            LayoutMode = LayoutMode.HorizontalList
        };

        EditorLabel label = new EditorLabel
        {
            Id = "Label",
            Margins = new Primitives.Rectangle(0, 0, 10, 0),
            Text = labelText,
        };
        container.AddChild(label);

        ReflectorTypeHandlerBase<T>? handler = ReflectorEngine.GetTypeHandler<T>();
        TypeEditor? editor = handler?.GetEditor();
        if (editor != null)
        {
            editor.SetValue(initialValue);
            editor.SetCallbackOnValueChange((newVal) =>
            {
                if (newVal is T valAsT)
                    valueChanged(valAsT);
            });
            container.AddChild(editor);
        }

        switch (style)
        {
            case LabelStyle.MapEditor:
                label.OutlineColor = Color.Black;
                label.OutlineSize = 2;
                label.FontSize = 23;
                break;
        }

        return container;
    }

    public static UIBaseWindow WrapWithLabel(string labelText, TypeEditor editor, bool vertical = false)
    {
        UIBaseWindow container = new UIBaseWindow()
        {
            GrowY = false,
            LayoutMode = LayoutMode.HorizontalList
        };

        EditorLabel label = new EditorLabel
        {
            Id = "Label",
            Margins = new Primitives.Rectangle(0, 0, 10, 0),
            Text = labelText,
        };
        container.AddChild(label);
        container.AddChild(editor);

        if (vertical)
        {
            container.LayoutMode = LayoutMode.VerticalList;
            label.Margins = new Primitives.Rectangle(0, 0, 0, 5);
        }

        return container;
    }
}
