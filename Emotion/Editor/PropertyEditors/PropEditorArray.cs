#nullable enable

#region Using

using Emotion.Editor.EditorHelpers;
using Emotion.Game.Animation3D;
using Emotion.Game.World3D;
using Emotion.Standard.XML;
using Emotion.UI;
using Emotion.Utility;
using Silk.NET.Assimp;
using System.Runtime.InteropServices;

#endregion

namespace Emotion.Editor.PropertyEditors;

public class PropEditorArray : EditorButtonDropDown, IPropEditorGeneric
{
    public XMLFieldHandler Field { get; set; }

    private EditorDropDownButtonDescription[] _noItems =
    {
            new()
            {
                Name = "No Items",
            }
        };

    private object _value;
    private Action<object> _changeCallback;

    public override void AttachedToController(UIController controller)
    {
        base.AttachedToController(controller);
        FillDropdownItems();
    }

    public void SetValue(object value)
    {
        _value = value;
    }

    public object GetValue()
    {
        return _value;
    }

    public void SetCallbackValueChanged(Action<object> callback)
    {
        _changeCallback = callback;
    }

    private void FillDropdownItems()
    {
        var valueAsArray = (Array?)_value;
        if (valueAsArray != null)
        {
            var arrayItems = new EditorDropDownButtonDescription[valueAsArray.Length];
            for (int i = 0; i < valueAsArray.Length; i++)
            {
                var value = valueAsArray.GetValue(i);
                arrayItems[i] = new EditorDropDownButtonDescription
                {
                    Name = value?.ToString() ?? "<null>",
                    Click = (_, __) =>
                    {
                        if (value == null) return;
                        var panel = new GenericPropertiesEditorPanel(value);
                        Controller?.AddChild(panel);
                    }
                };
            }
            SetItems(arrayItems, 0);
        }
        else
        {
            SetItems(_noItems, 0);
        }
    }

    protected override void UpdateCurrentOptionText()
    {
        var button = (EditorButton?)GetWindowById("Button");
        if (button == null) return;
        button.Text = _value?.ToString() ?? $"Null Array";
        button.Enabled = true;
    }
}