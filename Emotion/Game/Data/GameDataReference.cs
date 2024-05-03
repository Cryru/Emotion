#nullable enable

using Emotion;
using Emotion.Editor.PropertyEditors;

namespace Emotion.Game.Data;

public class GameDataReference
{
    public string? Id;
}

public class GameDataReference<T> : GameDataReference where T : GameDataObject
{
    public static implicit operator GameDataReference<T>(string id)
    {
        return new GameDataReference<T>() { Id = id };
    }

    public static implicit operator GameDataReference<T>?(T? gameData)
    {
        if (gameData == null) return null;
        return new GameDataReference<T>() { Id = gameData.Id };
    }

    public static bool operator ==(GameDataReference<T>? reference, T? gameData)
    {
        string? myId = reference?.Id ?? null;
        string? dataId = gameData?.Id ?? null;

        return myId == dataId;
    }

    public static bool operator !=(GameDataReference<T>? reference, T? gameData)
    {
        return !(reference == gameData);
    }

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

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(this, obj))
            return true;

        if (ReferenceEquals(obj, null))
            return false;

        if (obj is T objData)
            return this == objData;

        return false;
    }

    public override int GetHashCode()
    {
        if (Id == null) return 0;
        return Id.GetHashCode();
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