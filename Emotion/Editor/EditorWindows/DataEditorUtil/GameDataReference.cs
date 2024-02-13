#nullable enable

using Emotion.Editor.PropertyEditors;

namespace Emotion.Editor.EditorWindows.DataEditorUtil;

public class GameDataReference
{
    public string? Id;
}

public class GameDataReference<T> : GameDataReference where T : GameDataObject
{
    public bool IsValid()
    {
        return GameDataDatabase.GetDataObject<T>(Id) != null;
    }

    public T? GetDataObjectReferenced()
    {
        return GameDataDatabase.GetDataObject<T>(Id);
    }

    public override string ToString()
    {
        if (string.IsNullOrEmpty(Id)) return "Empty Reference";
        return IsValid() ? $"Ref: {Id}" : $"Invalid Ref: {Id}";
    }
}

public class GameDataReferenceChoiceCombo : MetaPropEditorCombo<string>
{
    private Type _fieldType; // GameDataReference<>

    public GameDataReferenceChoiceCombo(Type fieldType, string[] options) : base(options)
    {
        _fieldType = fieldType;
    }

    public override void SetValue(object? value)
    {
        if (value is string str)
        {
            value = Activator.CreateInstance(_fieldType);
            var valAsGameDataRef = value as GameDataReference;
            AssertNotNull(valAsGameDataRef);
            valAsGameDataRef.Id = str;
        }

        base.SetValue(value);
    }
}