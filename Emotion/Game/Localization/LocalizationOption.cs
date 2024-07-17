using Emotion.Editor;
using Emotion.Game.Data;
using Emotion.IO;

#nullable enable

namespace Emotion.Game.Localization;

public class LocalizationOption : GameDataObject
{
    [AssetFileName<TextAsset>]
    public string LocalizationCSVFile = string.Empty;
}

public class DefaultLanguage : LocalizationOption
{

}