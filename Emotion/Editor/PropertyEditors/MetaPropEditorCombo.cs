#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Standard.XML;
using Emotion.UI;

#endregion

#nullable enable

namespace Emotion.Editor.PropertyEditors;

public class MetaPropEditorCombo<T> : EditorButtonDropDown, IPropEditorGeneric
{
    public XMLFieldHandler? Field { get; set; }

    private object? _value;

    private T[] _options;
    private string[] _optionNames;

    private Action<object?>? _callback;

    public MetaPropEditorCombo(T[] options)
    {
        _options = options;
        _optionNames = new string[options.Length];
        for (int i = 0; i < options.Length; i++)
        {
            _optionNames[i] = options[i]?.ToString() ?? "<null>";
        }
    }

    protected override void UpdateCurrentOptionText()
    {
        var button = (EditorButton?) GetWindowById("Button");
        if (button == null) return;

        string currentPicked = _value?.ToString() ?? "<null>";
        button.Text = currentPicked;
        button.Enabled = true;
    }

    public virtual void SetValue(object? value)
    {
        _value = value;
        UpdateCurrentOptionText();
    }

    public object GetValue()
    {
        return _value!;
    }

    public void SetCallbackValueChanged(Action<object?> callback)
    {
        _callback = callback;
    }

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);

        var currentIdx = 0;
        var dropDownItems = new EditorDropDownItem[_optionNames.Length];
        for (var i = 0; i < _optionNames.Length; i++)
        {
            string thisOption = _optionNames[i];
            object? val = _options[i];

            dropDownItems[i] = new EditorDropDownItem
            {
                Name = thisOption,
                Click = (_, __) =>
                {
                    SetValue(val);
                    _callback?.Invoke(_value);
                },
                Enabled = () => !ReferenceEquals(val, _value)
            };

            if (ReferenceEquals(val, _value)) currentIdx = i;
        }

        SetItems(dropDownItems, currentIdx);

        Text = "";
        UpdateCurrentOptionText();
    }
}