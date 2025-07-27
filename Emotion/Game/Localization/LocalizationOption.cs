using Emotion.Editor;
using Emotion.Game.Data;
using Emotion.IO;

#nullable enable

namespace Emotion.Game.Localization;

public abstract class LocalizationOption : GameDataObject
{
    public SerializableAsset<TextAsset> LocalizationCSVFile = string.Empty;
}

public class DefaultLanguage : LocalizationOption
{

}