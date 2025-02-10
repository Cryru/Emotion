using Emotion.Game.World.Editor;
using Emotion.UI;
using Emotion.WIPUpdates.One.EditorUI.Components;
using System.Globalization;

#nullable enable

namespace Emotion.WIPUpdates.One.EditorUI.ObjectPropertiesEditorHelpers;

public class VectorEditor : TypeEditor
{
    private int _componentCount;

    private string[] _componentNames = { "X", "Y", "Z", "W" };
    private UITextInput2[] _componentEditors;

    private object? _value;
    private float[] _componentValues;

    public VectorEditor(int componentCount)
    {
        _componentCount = componentCount;
        _componentEditors = new UITextInput2[componentCount];
        _componentValues = new float[componentCount];

        UIBaseWindow editorContainer = new()
        {
            LayoutMode = LayoutMode.HorizontalList
        };
        AddChild(editorContainer);

        for (int i = 0; i < _componentCount; i++)
        {
            var label = new EditorLabel(_componentNames[i])
            {
                Margins = new Primitives.Rectangle(0, 0, 5, 0)
            };
            editorContainer.AddChild(label);

            var inputBackground = new UISolidColor
            {
                WindowColor = Color.Black * 0.5f,
                Paddings = new Primitives.Rectangle(5, 3, 5, 3)
            };
            editorContainer.AddChild(inputBackground);

            int inputIndex = i;
            UITextInput2 input = new UITextInput2
            {
                Id = "TextInput",

                FontSize = MapEditorColorPalette.EditorButtonTextSize,
                MinSizeX = 100,
                AnchorAndParentAnchor = UIAnchor.CenterLeft,
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
                }
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
            Vector4 valVector = (Vector4)_value;
            valVector.X = _componentValues[0];
            valVector.Y = _componentValues[1];
            valVector.Z = _componentValues[2];
            valVector.W = _componentValues[3];
            _value = valVector;
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
            Vector4 valVector = (Vector4)_value;
            _componentValues[0] = valVector.X;
            _componentValues[1] = valVector.Y;
            _componentValues[2] = valVector.Z;
            _componentValues[3] = valVector.W;
            _value = valVector;
        }

        for (int i = 0; i < _componentCount; i++)
        {
            var editor = _componentEditors[i];
            editor.Text = _componentValues[i].ToString(CultureInfo.InvariantCulture);
        }
    }
}
