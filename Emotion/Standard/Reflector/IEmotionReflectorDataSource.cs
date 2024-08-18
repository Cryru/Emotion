namespace Emotion.Standard.Reflector;

public interface IEmotionReflectorDataSource
{
    public IReflectorTypeData? GetTypeData(string typeName);
}
