using Emotion.Core.Systems.IO;
using Emotion.Game.Systems.GameData;

#nullable enable

namespace Emotion.Core.Systems.Localization;

public abstract class LocalizationOption : GameDataObject
{
    public SerializableAsset<TextAsset> LocalizationCSVFile = string.Empty;
}

public class DefaultLanguage : LocalizationOption
{

}