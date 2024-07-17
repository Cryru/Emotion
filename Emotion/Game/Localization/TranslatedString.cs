#nullable enable

using System.Diagnostics.CodeAnalysis;

namespace Emotion.Game.Localization;

public struct TranslatedString
{
    public string? StringData;

    public TranslatedString(string str)
    {
        StringData = str;
    }

    public static implicit operator TranslatedString(string str)
    {
        return new TranslatedString(str);
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj is TranslatedString otherStr)
            return otherStr.StringData == StringData;

        return base.Equals(obj);
    }

    public override string ToString()
    {
        return LocalizationEngine.LocalizeString(StringData);
    }
}
