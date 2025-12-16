#nullable enable

using Emotion.Editor.EditorUI.Components;
using Emotion.Game.Systems.UI;
using System.Globalization;

namespace Emotion.Editor.EditorUI.ObjectPropertiesEditorHelpers;

public class VectorEditor : TypeEditor
{
    private int _componentCount;

    private static string[] _componentNamesDefault = { "X", "Y", "Z", "W" };
    private string[] _componentNames;
    private UITextInput[] _componentEditors;

    private object? _value;
    private float[] _componentValues;

    public VectorEditor(int componentCount, string[]? componentNameOverride = null)
    {
        _componentCount = componentCount;
        _componentEditors = new UITextInput[componentCount];
        _componentValues = new float[componentCount];

        _componentNames = componentNameOverride ?? _componentNamesDefault;

        UIBaseWindow editorContainer = new()
        {
            Layout =
            {
                LayoutMethod = UILayoutMethod.HorizontalList(0)
            }
        };
        AddChild(editorContainer);

        for (int i = 0; i < _componentCount; i++)
        {
            var label = new EditorLabel(_componentNames[i])
            {
                Margins = new Rectangle(0, 0, 5, 0)
            };
            editorContainer.AddChild(label);

            var inputBackground = new UISolidColor
            {
                WindowColor = Color.Black * 0.8f,
                Paddings = new Rectangle(5, 3, 5, 3),

                Margins = new Rectangle(0, 0, i != _componentCount - 1 ? 10 : 0, 0),
            };
            editorContainer.AddChild(inputBackground);

            int inputIndex = i;
            UITextInput input = new UITextInput
            {
                Name = "TextInput",

                FontSize = EditorColorPalette.EditorButtonTextSize,
                Layout =
                {
                    MinSizeX = 100,
                    AnchorAndParentAnchor = UIAnchor.CenterLeft
                },
                IgnoreParentColor = true,

                SubmitOnEnter = true,
                SubmitOnFocusLoss = true,
                OnSubmit = (newText) =>
                {
                    if (float.TryParse(newText, CultureInfo.InvariantCulture, out float parsed))
                    {
                        _componentValues[inputIndex] = parsed;
                        WriteVectorComponents();
                    }
                },
            };
            inputBackground.AddChild(input);

            _componentEditors[i] = input;
        }
    }

    public override void SetValue(object? value)
    {
        _value = value ?? default;
        ReadVectorComponents();
    }

    private void WriteVectorComponents()
    {
        if (_value == null) return;

        if (_componentCount == 2)
        {
            Vector2 valVector = (Vector2)_value;
            valVector.X = _componentValues[0];
            valVector.Y = _componentValues[1];
            _value = valVector;
        }

        if (_componentCount == 3)
        {
            Vector3 valVector = (Vector3)_value;
            valVector.X = _componentValues[0];
            valVector.Y = _componentValues[1];
            valVector.Z = _componentValues[2];
            _value = valVector;
        }

        if (_componentCount == 4)
        {
            if (_value is Vector4 valVector)
            {
                valVector.X = _componentValues[0];
                valVector.Y = _componentValues[1];
                valVector.Z = _componentValues[2];
                valVector.W = _componentValues[3];
                _value = valVector;
            }
            else if (_value is Rectangle valRect)
            {
                valRect.X = _componentValues[0];
                valRect.Y = _componentValues[1];
                valRect.Width = _componentValues[2];
                valRect.Height = _componentValues[3];
                _value = valRect;
            }
        }

        OnValueChanged(_value);
    }

    private void ReadVectorComponents()
    {
        if (_value == null)
        {
            for (int i = 0; i < _componentCount; i++)
            {
                _componentValues[i] = 0;
            }
            return;
        }

        if (_componentCount == 2)
        {
            Vector2 valVector = (Vector2)_value;
            _componentValues[0] = valVector.X;
            _componentValues[1] = valVector.Y;
            _value = valVector;
        }

        if (_componentCount == 3)
        {
            Vector3 valVector = (Vector3)_value;
            _componentValues[0] = valVector.X;
            _componentValues[1] = valVector.Y;
            _componentValues[2] = valVector.Z;
            _value = valVector;
        }

        if (_componentCount == 4)
        {
            if (_value is Vector4 valVector)
            {
                _componentValues[0] = valVector.X;
                _componentValues[1] = valVector.Y;
                _componentValues[2] = valVector.Z;
                _componentValues[3] = valVector.W;
                _value = valVector;
            }
            else if (_value is Rectangle valRect)
            {
                _componentValues[0] = valRect.X;
                _componentValues[1] = valRect.Y;
                _componentValues[2] = valRect.Width;
                _componentValues[3] = valRect.Height;
                _value = valRect;
            }
        }

        for (int i = 0; i < _componentCount; i++)
        {
            var editor = _componentEditors[i];
            editor.Text = _componentValues[i].ToString(CultureInfo.InvariantCulture);
        }
    }
}
