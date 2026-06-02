#nullable enable

namespace Emotion.Core.Platform.Implementation.Web;

[DontSerialize]
public class EmotionWebService
{
    public Configurator Config;
    public Action InitCode;

    public EmotionWebService(Configurator config, Action initCode)
    {
        Config = config;
        InitCode = initCode;
    }
}