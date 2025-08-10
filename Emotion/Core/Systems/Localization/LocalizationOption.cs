#nullable enable

using Emotion.Core.Systems.IO;

namespace Emotion.Core.Systems.Localization;

public abstract class LocalizationOption : GameDataObject
{
    public SerializableAsset<TextAsset> LocalizationCSVFile = string.Empty;
}

public class DefaultLanguage : LocalizationOption
{

}